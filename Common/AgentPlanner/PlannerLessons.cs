using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
#if AGENT_WPF
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.AgentPlanner;

/// <summary>
/// The five lessons of AgentPlannerBasics on one agent session, independent of the UI toolkit.
/// Every request goes through the planner's stages, one at a time:
/// ① request → ② proposal (workflow DSL v2) → ③ validation → ④ deterministic execution → ⑤ provenance.
/// Each stage is written as a line of text to <see cref="Logged"/>. Call everything on the UI thread
/// that owns the viewer; only <see cref="ProposeAsync"/> runs the model call elsewhere.
/// </summary>
public sealed class PlannerLessons : IDisposable
{
    public static readonly string[] Titles =
    [
        "1. A plan that reads data and changes the view",
        "2. Same plan, same result",
        "3. Proposals that never run",
        "4. Changing data needs confirmation",
        "5. A record that shows any change",
    ];

    // Bottom to top; each new layer becomes index 0.
    static readonly (string File, string Style)[] Layers =
    [
        ("parks.geojson", """{"fillColor":"#A1D99B","fillOpacity":255,"lineColor":"#41AB5D","lineWidth":1.0}"""),
        ("streets.geojson", """{"lineColor":"#9E9E9E","lineWidth":3.0}"""),
        ("buildings.geojson", """{"fillColor":"#E7D8C3","fillOpacity":255,"lineColor":"#A08B70","lineWidth":0.8}"""),
        ("schools.geojson", """{"pointColor":"#2171B5","fillColor":"#2171B5","lineColor":"#08306B","pointSize":12.0,"lineWidth":1.2}"""),
    ];

    readonly GisAgent agent;
    readonly JsonObject lessons;
    readonly Stack<int> runs = new();  // completed steps per executed workflow, for undo
    string? pendingProposal;
    JsonObject? pendingEvaluation;

    public PlannerLessons(GeoKernelViewerControl viewer)
    {
        var directory = Path.Combine(AppContext.BaseDirectory, "data");
        lessons = JsonNode.Parse(File.ReadAllText(Path.Combine(directory, "planner-lessons.json")))!.AsObject();
        foreach (var (file, style) in Layers)
        {
            if (!viewer.AddLayerFile(Path.Combine(directory, file)))
                throw new InvalidOperationException($"{file} could not be loaded.");
            viewer.SetLayerStyleJson(0, style);
        }
        viewer.RefreshLayers();
        viewer.FullExtent();
        agent = new GisAgent(viewer);
        var provider = Environment.GetEnvironmentVariable("GEOKERNEL_LLM_PROVIDER")?.Trim().ToLowerInvariant();
        var model = Environment.GetEnvironmentVariable("GEOKERNEL_LLM_MODEL")?.Trim();
        UsesLlm = !string.IsNullOrEmpty(provider);
        PlannerDescription = UsesLlm
            ? $"Your requests: {provider} ({(string.IsNullOrEmpty(model) ? (provider == "anthropic" ? "claude-opus-5-5" : "model not set") : model)})"
            : "Lessons run offline; set GEOKERNEL_LLM_PROVIDER to plan your own requests";
    }

    public event Action<string>? Logged;

    public bool UsesLlm { get; }
    public string PlannerDescription { get; }
    public bool HasPending => pendingProposal is not null;
    public bool PendingNeedsConfirmation => pendingEvaluation?["needsConfirmation"]?.AsArray().Count > 0;

    public bool Run(int lesson) => lesson switch
    {
        0 => Basic(),
        1 => Determinism(),
        2 => Rejected(),
        3 => Confirmation(),
        _ => Tamper(),
    };

    // ------------------------------------------------------------------ lessons

    bool Basic()
    {
        var lesson = lessons["basic"]!;
        Heading(1, Text(lesson["title"]));
        Reset();
        var proposal = lesson["proposal"]!.ToJsonString();
        StageRequest(Text(lesson["request"]));
        StageProposal("prepared, exactly as a model returns it", proposal);
        var evaluation = StageValidation(proposal);
        var run = Execute(proposal);
        StageExecution(run);
        StageProvenance(run);
        return Text(evaluation["status"]) == "validated" && Text(run["status"]) == "executed";
    }

    bool Determinism()
    {
        var proposal = lessons["basic"]!["proposal"]!.ToJsonString();
        Heading(2, "Same plan, same result");
        Note("The plan from lesson 1 runs, is undone, and runs again on the same data.");
        Reset();
        var first = Execute(proposal);
        var firstRecord = LastRecord();
        Line("④ First run", $"{Status(first)} · {Steps(first)} steps · record #{firstRecord["sequence"]}");
        Reset();
        var second = Execute(proposal);
        var secondRecord = LastRecord();
        Line("④ Second run", $"{Status(second)} · {Steps(second)} steps · record #{secondRecord["sequence"]}");
        var sameInputs = JsonNode.DeepEquals(firstRecord["inputSha256"], secondRecord["inputSha256"]);
        var sameSteps = JsonNode.DeepEquals(firstRecord["stepOutputs"], secondRecord["stepOutputs"]);
        var firstOutput = Text(firstRecord["outputSha256"]);
        var sameOutput = firstOutput.Length > 0 && firstOutput == Text(secondRecord["outputSha256"]);
        Line("⑤ Compare", $"input hashes {(sameInputs ? "identical" : "DIFFERENT")} · output {Short(firstOutput)} = "
                          + $"{Short(Text(secondRecord["outputSha256"]))} {(sameOutput && sameSteps ? "✓ identical, step by step" : "✗ different")}");
        foreach (var step in secondRecord["stepOutputs"]?.AsArray() ?? [])
            Detail($"{Text(step!["stepId"]),-14} {Short(Text(step["outputSha256"]))}");
        Note($"Records #{firstRecord["sequence"]} and #{secondRecord["sequence"]} differ only in their execution id, time and chain "
             + "position; anyone can replay a record and compare the output hash.");
        return sameInputs && sameSteps && sameOutput;
    }

    bool Rejected()
    {
        Heading(3, "Proposals that never run");
        Note("A careless or manipulated model can return anything. Each proposal below goes to the same planner; none reaches the map.");
        Reset();
        var before = RecordCount();
        var allStopped = true;
        var index = 0;
        foreach (var item in lessons["rejected"]!.AsArray())
        {
            var value = item!["proposal"]!;
            var proposal = value.GetValueKind() == JsonValueKind.String ? value.GetValue<string>() : value.ToJsonString();
            Log("");
            Log($"{++index}. {Text(item["title"])}");
            StageProposal("bad", proposal);
            var evaluation = agent.Evaluate(proposal);
            var run = Execute(proposal);
            var stopped = Steps(run) == 0 && Text(run["status"]) != "executed" && Text(evaluation["status"]) != "validated";
            allStopped &= stopped;
            Line("③ Validation", $"{Status(evaluation)} · {Text(evaluation["code"])} — {Text(evaluation["message"])}");
            Line("④ Execution", stopped ? "not started: nothing ran" : "✗ it ran");
            Detail(Text(item["why"]));
        }
        var after = RecordCount();
        Line("⑤ Provenance", $"{before} records before, {after} after: refused proposals leave nothing to record");
        return allStopped && before == after && index > 0;
    }

    bool Confirmation()
    {
        var lesson = lessons["confirm"]!;
        Heading(4, Text(lesson["title"]));
        Reset();
        var proposal = lesson["proposal"]!.ToJsonString();
        StageRequest(Text(lesson["request"]));
        StageProposal("prepared", proposal);
        var evaluation = StageValidation(proposal);
        var planId = Text(evaluation["planId"]);

        var unconfirmed = Execute(proposal);
        Line("④ Run, not confirmed", $"{Status(unconfirmed)} · {Steps(unconfirmed)} steps ran");

        var changed = lesson["proposal"]!.DeepClone().AsObject();
        foreach (var step in changed["steps"]!.AsArray())
            if (Text(step!["id"]) == Text(lesson["changedStep"]))
                step["parameters"]!["values"] = lesson["changedValues"]!.DeepClone();
        var changedText = changed.ToJsonString();
        var changedId = Text(agent.Evaluate(changedText)["planId"]);
        var swapped = Execute(changedText, planId);
        Line("④ Swapped plan", $"{Text(lesson["changedTitle"])}: plan {Short(changedId)} with the confirmation of {Short(planId)} "
                               + $"→ {Status(swapped)} · {Steps(swapped)} steps ran");

        var confirmed = Execute(proposal, planId);
        Line("④ Confirmed run", $"the user confirms plan {Short(planId)}");
        StageExecution(confirmed);
        StageProvenance(confirmed);
        return Text(evaluation["status"]) == "confirmation-required"
               && Text(unconfirmed["status"]) == "confirmation-required" && Steps(unconfirmed) == 0
               && changedId != planId
               && Text(swapped["status"]) == "confirmation-required" && Steps(swapped) == 0
               && Text(confirmed["status"]) == "executed";
    }

    bool Tamper()
    {
        Heading(5, "A record that shows any change");
        if (RecordCount() == 0)
        {
            Note("No workflow has run yet; running lesson 1 first.");
            Basic();
        }
        var exported = agent.Provenance();
        var records = exported["records"]!.AsArray();
        Line("⑤ Export", $"{records.Count} records, {exported.ToJsonString().Length} bytes of JSON");
        foreach (var record in records)
            Detail($"#{record!["sequence"]} {Text(record["executionId"]),-12} {Short(Text(record["hash"]))} ← {Short(Text(record["previousHash"]))}");
        var intact = agent.VerifyProvenance(exported);
        Line("⑤ Verify", intact["verified"]!.GetValue<bool>() ? "✓ every record matches its hash and the chain" : "✗ " + Text(intact["message"]));

        // Somebody edits one output hash in the saved file.
        var edited = exported.DeepClone().AsObject();
        var first = edited["records"]![0]!;
        var hash = Text(first["outputSha256"]);
        first["outputSha256"] = hash[..^1] + (hash.EndsWith('0') ? "1" : "0");
        var broken = agent.VerifyProvenance(edited);
        var detected = !broken["verified"]!.GetValue<bool>() && broken["brokenAtSequence"]!.GetValue<int>() == 1;
        Line("⑤ Edited copy", "record #1 output hash changed in one character → "
                              + (detected ? "✗ record 1 no longer matches its hash: the change is detected" : "not detected"));
        Note("Each record's hash covers the normalized plan, the validation decision, the input and output hashes and the previous record's hash.");
        return intact["verified"]!.GetValue<bool>() && detected;
    }

    // ------------------------------------------------------------------ your own request

    /// <summary>Stages 1–3 of a request: the model call runs on a worker thread.</summary>
    public async Task ProposeAsync(string request)
    {
        pendingProposal = null;
        pendingEvaluation = null;
        Heading(0, "Your request");
        Reset();
        StageRequest(request);
        var grounding = agent.Grounding() + "\n" + Text(lessons["groundingNotes"]);
        string proposal;
        try
        {
            proposal = await Task.Run(() => GisLlm.Propose(request, grounding));
        }
        catch (Exception error)
        {
            Line("② Proposal", "the model returned no plan: " + error.Message);
            return;
        }
        StageProposal("from the model", proposal);
        var evaluation = StageValidation(proposal);
        var status = Text(evaluation["status"]);
        if (status is not ("validated" or "confirmation-required"))
            return;
        pendingProposal = proposal;
        pendingEvaluation = evaluation;
        Note(status == "confirmation-required" ? "The plan changes data. Clicking Confirm and run confirms this exact plan." : "Click Run to execute the plan.");
    }

    /// <summary>Runs the planned request; clicking the button is the confirmation of that exact plan.</summary>
    public void RunPending()
    {
        if (pendingProposal is null || pendingEvaluation is null)
            return;
        var confirmed = PendingNeedsConfirmation ? Text(pendingEvaluation["planId"]) : null;
        var run = Execute(pendingProposal, confirmed);
        pendingProposal = null;
        pendingEvaluation = null;
        StageExecution(run);
        StageProvenance(run);
    }

    public void Reset()
    {
        while (runs.Count > 0)
            agent.Undo(runs.Pop());
    }

    public void Dispose() => agent.Dispose();

    // ------------------------------------------------------------------ stages

    JsonObject Execute(string proposal, string? confirmedPlanId = null)
    {
        var result = agent.Run(proposal, confirmedPlanId);
        if (Steps(result) > 0)
            runs.Push(Steps(result));
        return result;
    }

    void StageRequest(string request) => Line("① Request", $"“{request}”");

    void StageProposal(string source, string proposal)
    {
        Line("② Proposal", source);
        JsonObject? plan = null;
        try { plan = JsonNode.Parse(proposal) as JsonObject; }
        catch (JsonException) { }
        if (plan is null)
        {
            Detail(proposal.Length > 160 ? proposal[..160] : proposal);
            return;
        }
        var steps = plan["steps"]?.AsArray() ?? [];
        plan.Remove("steps");
        Detail(plan.ToJsonString()[..^1] + ",\"steps\":[");
        foreach (var step in steps)
            Detail("  " + step!.ToJsonString());
        Detail("]}");
    }

    JsonObject StageValidation(string proposal)
    {
        var evaluation = agent.Evaluate(proposal);
        Line("③ Validation", $"{Status(evaluation)} · {Text(evaluation["code"])} — {Text(evaluation["message"])}");
        var planId = Text(evaluation["planId"]);
        foreach (var line in Text(evaluation["summary"]).Split('\n', StringSplitOptions.RemoveEmptyEntries))
            Detail(planId.Length > 0 ? line.Replace(planId, Short(planId)) : line);
        return evaluation;
    }

    void StageExecution(JsonObject run)
    {
        Line("④ Execution", $"{Status(run)} · {Steps(run)} steps · {Text(run["message"])}");
        foreach (var line in Text(run["report"]).Split('\n', StringSplitOptions.RemoveEmptyEntries))
            Detail(line);
    }

    void StageProvenance(JsonObject run)
    {
        if (Text(run["provenanceHash"]).Length == 0 || RecordCount() == 0)
        {
            Line("⑤ Provenance", "nothing ran, nothing recorded");
            return;
        }
        var record = LastRecord();
        Line("⑤ Provenance", $"record #{record["sequence"]} · hash {Short(Text(record["hash"]))} ← previous {Short(Text(record["previousHash"]))}");
        Detail($"plan    {Short(Text(record["validationAudit"]?["planId"]))}");
        foreach (var (name, value) in record["inputSha256"]!.AsObject())
            Detail($"input   {name,-18} {Short(Text(value))}");
        Detail($"output  {Short(Text(record["outputSha256"]))} ({record["stepOutputs"]?.AsArray().Count ?? 0} step outputs)");
    }

    JsonObject LastRecord() => agent.Provenance()["records"]!.AsArray()[^1]!.AsObject();
    int RecordCount() => agent.Provenance()["records"]?.AsArray().Count ?? 0;

    void Heading(int lesson, string title)
    {
        Log("");
        Log("━━ " + (lesson > 0 ? $"Lesson {lesson} · " : "") + title);
    }

    void Line(string stage, string text) => Log($"{stage}  {text}");
    void Detail(string text) => Log("      " + text);
    void Note(string text) => Log(text);
    void Log(string text) => Logged?.Invoke(text);

    static string Text(JsonNode? node) => node is JsonValue value && value.TryGetValue<string>(out var text) ? text : "";
    static int Steps(JsonObject result) => result["completedSteps"]?.GetValue<int>() ?? 0;
    static string Status(JsonObject result) => Text(result["status"]) == "confirmation-required" ? "confirmation required" : Text(result["status"]);
    static string Short(string hash) => hash.Length == 0 ? "(none)" : hash[..Math.Min(12, hash.Length)] + "…";
}
