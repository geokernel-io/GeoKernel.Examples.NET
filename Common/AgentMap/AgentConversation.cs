using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
#if AGENT_WPF
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.AgentMap;

/// <summary>A prepared sample request and the workflow DSL v2 proposal an LLM would return for it.</summary>
public sealed record DemoRequest(string Request, string Proposal);

/// <summary>The sample data folder: grounding notes, prepared plans and the neighbourhood layers.</summary>
public sealed class AgentDemo
{
    public string DataDirectory { get; }
    public string GroundingNotes { get; }
    public IReadOnlyList<DemoRequest> Requests { get; }

    AgentDemo(string directory, string notes, IReadOnlyList<DemoRequest> requests) =>
        (DataDirectory, GroundingNotes, Requests) = (directory, notes, requests);

    public static AgentDemo Load()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "data");
        var root = JsonNode.Parse(File.ReadAllText(Path.Combine(directory, "agent-demo.json")))!.AsObject();
        var requests = root["requests"]!.AsArray()
            .Select(item => new DemoRequest(item!["request"]!.GetValue<string>(), item["proposal"]!.ToJsonString()))
            .ToList();
        return new AgentDemo(directory, root["groundingNotes"]!.GetValue<string>(), requests);
    }

    // Bottom to top; each new layer becomes index 0.
    static readonly (string File, string Style)[] Layers =
    [
        ("parks.geojson", """{"fillColor":"#A1D99B","fillOpacity":255,"lineColor":"#41AB5D","lineWidth":1.0}"""),
        ("streets.geojson", """{"lineColor":"#9E9E9E","lineWidth":3.0}"""),
        ("buildings.geojson", """{"fillColor":"#E7D8C3","fillOpacity":255,"lineColor":"#A08B70","lineWidth":0.8}"""),
        ("schools.geojson", """{"pointColor":"#2171B5","fillColor":"#2171B5","lineColor":"#08306B","pointSize":12.0,"lineWidth":1.2}"""),
    ];

    public void LoadNeighbourhood(GeoKernelViewerControl viewer)
    {
        foreach (var (file, style) in Layers)
        {
            if (!viewer.AddLayerFile(Path.Combine(DataDirectory, file)))
                throw new InvalidOperationException($"{file} could not be loaded.");
            viewer.SetLayerStyleJson(0, style);
        }
        viewer.RefreshLayers();
        viewer.FullExtent();
    }
}

/// <summary>One log line: who said it, the text, and an optional colour (#RRGGBB).</summary>
public sealed record LogEntry(string Who, string Text, string? Color = null);

/// <summary>
/// The assistant's conversation, independent of the UI toolkit:
/// request → proposal (offline demo or LLM) → validation → preview → run (with the user's
/// confirmation of that exact planId when it changes data) → results → undo, provenance.
/// Call everything on the UI thread that owns the viewer; only the LLM call runs elsewhere.
/// </summary>
public sealed class AgentConversation : IDisposable
{
    public const string Ok = "#1b5e20", Warning = "#8a4b00", Error = "#b00020";

    readonly AgentDemo demo;
    readonly GisAgent agent;
    readonly Dictionary<string, string> prepared;
    readonly Stack<int> workflows = new();
    string? pendingProposal;
    JsonObject? pending;

    public AgentConversation(GeoKernelViewerControl viewer, AgentDemo demo)
    {
        this.demo = demo;
        agent = new GisAgent(viewer);
        prepared = demo.Requests.ToDictionary(item => item.Request, item => item.Proposal, StringComparer.OrdinalIgnoreCase);
        var provider = Environment.GetEnvironmentVariable("GEOKERNEL_LLM_PROVIDER")?.Trim().ToLowerInvariant();
        UsesLlm = !string.IsNullOrEmpty(provider);
        var model = Environment.GetEnvironmentVariable("GEOKERNEL_LLM_MODEL")?.Trim();
        PlannerDescription = UsesLlm
            ? $"Planner: {provider} ({(string.IsNullOrEmpty(model) ? (provider == "anthropic" ? "claude-opus-5-5" : "model not set") : model)})"
            : "Planner: offline demo (prepared plans)";
    }

    public event Action<LogEntry>? Logged;
    public event Action? Changed;
    public event Action<string>? Status;

    public bool UsesLlm { get; }
    public string PlannerDescription { get; }
    public bool Busy { get; private set; }
    public bool HasPlan => pending is not null;
    public bool NeedsConfirmation => pending?["needsConfirmation"]?.AsArray().Count > 0;
    public bool CanUndo => workflows.Count > 0 && !Busy;

    /// <summary>The previewed plan, one line per step, with a shortened planId.</summary>
    public string PlanText
    {
        get
        {
            if (pending is null)
                return "";
            var id = pending["planId"]!.GetValue<string>();
            var summary = pending["summary"]!.GetValue<string>();
            var index = summary.IndexOf(id, StringComparison.Ordinal);
            return index < 0 ? summary : summary[..index] + id[..Math.Min(16, id.Length)] + "…" + summary[(index + id.Length)..];
        }
    }

    public async Task<bool> SubmitAsync(string request)
    {
        request = request.Trim();
        if (request.Length == 0 || Busy)
            return false;
        Discard(log: false);
        Log("You", request);
        string? proposal = null, error = null;
        if (!UsesLlm)
        {
            if (!prepared.TryGetValue(request, out proposal))
                error = "offline demo mode only has plans for the sample requests. Set GEOKERNEL_LLM_PROVIDER "
                        + "(anthropic, openai or ollama) and its API key to plan your own requests.";
        }
        else
        {
            // The model call can take a while; it runs on a worker thread and touches no viewer state.
            var grounding = agent.Grounding() + "\n" + demo.GroundingNotes;
            SetBusy(true);
            Status?.Invoke("Planning...");
            try { proposal = await Task.Run(() => GisLlm.Propose(request, grounding)); }
            catch (Exception failure) { error = failure.Message; }
            finally { SetBusy(false); Status?.Invoke(""); }
        }
        if (error is not null)
        {
            Log("Planner", "No plan was proposed: " + error, Error);
            Changed?.Invoke();
            return true;
        }
        Preview(proposal!);
        return true;
    }

    void Preview(string proposal)
    {
        var evaluation = agent.Evaluate(proposal);
        var status = evaluation["status"]!.GetValue<string>();
        if (status is not ("validated" or "confirmation-required"))
        {
            var details = string.Join("; ", evaluation["steps"]!.AsArray()
                .Where(step => !string.IsNullOrEmpty(step?["message"]?.GetValue<string>()))
                .Select(step => $"{step!["stepId"]}: {step["message"]}"));
            Log("Planner", $"The plan was rejected ({status}): {evaluation["message"]}" + (details.Length > 0 ? " — " + details : ""), Error);
            Changed?.Invoke();
            return;
        }
        pendingProposal = proposal;
        pending = evaluation;
        var confirm = evaluation["needsConfirmation"]!.AsArray().Select(step => step!.GetValue<string>()).ToList();
        if (confirm.Count > 0)
        {
            Log("Planner", $"Plan ready. Steps {string.Join(", ", confirm)} change or delete data; review them before running.", Warning);
            Status?.Invoke("The plan changes data: review it and click Confirm and run");
        }
        else
        {
            var steps = PlanText.Split('\n').Length - 1;
            Log("Planner", $"Plan ready ({Steps(steps)}). Review it and press Run.");
            Status?.Invoke("Plan validated: review it and click Run");
        }
        Changed?.Invoke();
    }

    /// <summary>Runs the previewed plan; clicking Run is the user's confirmation of that exact plan.</summary>
    public void RunPending()
    {
        if (pending is null || pendingProposal is null || Busy)
            return;
        var confirmed = NeedsConfirmation ? pending["planId"]!.GetValue<string>() : null;
        var result = agent.Run(pendingProposal, confirmed);
        Discard(log: false);
        var completed = result["completedSteps"]?.GetValue<int>() ?? 0;
        if (completed > 0)
            workflows.Push(completed);
        var report = result["report"]?.GetValue<string>();
        if (!string.IsNullOrEmpty(report))
            Log("Result", report);
        if (result["status"]!.GetValue<string>() == "executed")
        {
            Log("Map", $"Done: {Steps(completed)} ran. Provenance {result["provenanceHash"]!.GetValue<string>()[..12]}.", Ok);
            Status?.Invoke("Executed " + result["planId"]!.GetValue<string>()[..16]);
        }
        else
        {
            var failed = result["failedStepId"]?.GetValue<string>();
            Log("Map", $"Stopped at step {(string.IsNullOrEmpty(failed) ? "-" : failed)}: {result["message"]}. {completed} completed steps can be undone.", Error);
        }
        Changed?.Invoke();
    }

    public void Discard(bool log = true)
    {
        var had = pending is not null;
        pending = null;
        pendingProposal = null;
        if (log && had)
        {
            Log("Planner", "Plan discarded; nothing ran.");
            Status?.Invoke("");
        }
        Changed?.Invoke();
    }

    public void UndoLast()
    {
        if (!CanUndo)
            return;
        var result = agent.Undo(workflows.Pop());
        var text = "Undone: " + string.Join(", ", result["undoneSteps"]!.AsArray().Select(step => step!.GetValue<string>())) + ".";
        var irreversible = result["irreversible"]?.AsArray().Select(item => item!.GetValue<string>()).ToList() ?? [];
        if (irreversible.Count > 0)
            text += " Could not undo: " + string.Join("; ", irreversible) + ".";
        Log("Map", text);
        Changed?.Invoke();
    }

    /// <summary>Summary line and the indented provenance JSON.</summary>
    public (string Summary, string Json) Provenance()
    {
        var records = agent.Provenance();
        var count = records["records"]?.AsArray().Count ?? 0;
        var summary = count == 0
            ? "No workflow has run yet."
            : $"{count} executed workflow(s). Each record holds the normalized plan, its validation decision and the SHA-256 of every step output, chained by hash.";
        return (summary, records.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
    }

    public void Dispose() => agent.Dispose();

    static string Steps(int count) => count == 1 ? "1 step" : $"{count} steps";

    void Log(string who, string text, string? color = null) => Logged?.Invoke(new LogEntry(who, text, color));

    void SetBusy(bool busy)
    {
        Busy = busy;
        Changed?.Invoke();
    }
}
