using GeoKernel.Examples.AgentPlanner;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.AgentPlannerBasics.Winforms;

/// <summary>
/// How a natural-language request becomes a safe, recorded map operation: five lessons send requests
/// through the planner's stages (request → proposal → validation → execution → provenance) and show
/// what each stage guarantees. The lessons run offline; your own requests need GEOKERNEL_LLM_PROVIDER.
/// </summary>
internal sealed class MainForm : Form
{
    const string Title = "Agent Planner Basics";

    readonly bool smoke;  // --smoke: run the five lessons, print them, close
    readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    readonly TextBox log = new()
    {
        Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.White,
        Font = new Font("Consolas", 9f),
    };
    readonly List<Button> lessonButtons = [];
    readonly TextBox request = new() { Dock = DockStyle.Fill };
    readonly Button plan = new() { Text = "Plan", AutoSize = true }, run = new() { Text = "Run", AutoSize = true, MinimumSize = new Size(110, 0) },
        undo = new() { Text = "Undo all", AutoSize = true }, clear = new() { Text = "Clear log", AutoSize = true };
    readonly ToolStripStatusLabel status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft }, planner = new();
    PlannerLessons? lessons;
    bool busy;

    public MainForm(bool smoke = false)
    {
        this.smoke = smoke;
        Text = Title;
        ExampleIcon.Apply(this);
        ClientSize = new Size(1440, 900);
        StartPosition = FormStartPosition.CenterScreen;

        var side = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(8) };
        side.Controls.Add(new Label
        {
            Text = "Each lesson sends a request through the planner's five stages: ① request → ② proposal → ③ validation → "
                   + "④ execution → ⑤ provenance. A lesson starts from the original map; its result stays on the map.",
            AutoSize = true, MaximumSize = new Size(560, 0), Margin = new Padding(0, 0, 0, 6),
        });
        for (var i = 0; i < PlannerLessons.Titles.Length; i++)
        {
            var index = i;
            var button = new Button { Text = PlannerLessons.Titles[i], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Height = 30 };
            button.Click += (_, _) => RunLesson(index);
            lessonButtons.Add(button);
            side.Controls.Add(button);
        }
        side.Controls.Add(new Label { Text = "Your own request:", AutoSize = true, Margin = new Padding(0, 10, 0, 2) });
        var entry = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoSize = true };
        entry.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        entry.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        entry.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        entry.Controls.Add(request, 0, 0);
        entry.Controls.Add(plan, 1, 0);
        entry.Controls.Add(run, 2, 0);
        side.Controls.Add(entry);
        side.Controls.Add(log);
        var bottom = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Fill };
        bottom.Controls.AddRange([undo, clear]);
        side.Controls.Add(bottom);
        for (var i = 0; i < side.Controls.Count; i++)
            side.RowStyles.Add(side.Controls[i] == log ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.AutoSize));

        var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2 };
        split.Panel1.Controls.Add(viewer);
        split.Panel2.Controls.Add(side);
        var statusStrip = new StatusStrip();
        statusStrip.Items.AddRange([status, planner]);
        Controls.Add(split);
        Controls.Add(statusStrip);
        Load += (_, _) => { split.SplitterDistance = Math.Max(400, split.Width - 600); };

        plan.Click += async (_, _) => await PlanAsync();
        request.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await PlanAsync(); } };
        run.Click += (_, _) => { lessons?.RunPending(); viewer.RefreshLayers(); RefreshButtons(); };
        undo.Click += (_, _) => { lessons?.Reset(); viewer.RefreshLayers(); status.Text = "Every lesson result was undone"; };
        clear.Click += (_, _) => log.Clear();
        Shown += (_, _) => Initialize();
        FormClosed += (_, _) => lessons?.Dispose();
        RefreshButtons();
    }

    void Initialize()
    {
        try
        {
            lessons = new PlannerLessons(viewer);
        }
        catch (Exception e)
        {
            if (smoke)
                Console.WriteLine("error: " + e.Message);
            MessageBox.Show(this, "The map could not be prepared:\n" + e.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        lessons.Logged += Append;
        planner.Text = lessons.PlannerDescription;
        if (lessons.UsesLlm)
            request.Text = "Show the parks with more than 50 trees in dark green";
        else
            request.PlaceholderText = "Set GEOKERNEL_LLM_PROVIDER (anthropic, openai or ollama) to plan your own requests";
        RefreshButtons();
        if (smoke)
            BeginInvoke(RunSmoke);
    }

    bool RunLesson(int index)
    {
        if (lessons is null || busy)
            return false;
        var ok = lessons.Run(index);
        viewer.RefreshLayers();
        status.Text = $"Lesson {index + 1} " + (ok ? "done" : "did not behave as described");
        RefreshButtons();
        return ok;
    }

    void RunSmoke()
    {
        var results = Enumerable.Range(0, PlannerLessons.Titles.Length).Select(RunLesson).ToList();
        for (var i = 0; i < results.Count; i++)
            Console.WriteLine($"smoke | lesson {i + 1}: {(results[i] ? "ok" : "FAILED")}");
        Environment.ExitCode = results.All(ok => ok) ? 0 : 2;
        BeginInvoke(Close);
    }

    async Task PlanAsync()
    {
        if (lessons is null || busy || !lessons.UsesLlm || request.Text.Trim().Length == 0)
            return;
        busy = true;
        status.Text = "Asking the model for a plan…";
        RefreshButtons();
        await lessons.ProposeAsync(request.Text.Trim());
        busy = false;
        status.Text = "";
        RefreshButtons();
    }

    void RefreshButtons()
    {
        var ready = lessons is not null && !busy;
        foreach (var button in lessonButtons)
            button.Enabled = ready;
        request.Enabled = plan.Enabled = ready && lessons!.UsesLlm;
        run.Enabled = ready && lessons!.HasPending;
        run.Text = lessons is not null && lessons.PendingNeedsConfirmation ? "Confirm and run" : "Run";
        undo.Enabled = ready;
    }

    void Append(string line)
    {
        log.AppendText((log.TextLength > 0 ? Environment.NewLine : "") + line);
        if (smoke)
            Console.WriteLine(line);
    }
}
