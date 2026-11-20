using GeoKernel.Examples.AgentMap;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.AgentMapAssistant.Winforms;

/// <summary>
/// Natural-language map requests as validated, previewed and recorded workflows. A planner (an LLM or
/// the offline demo) proposes a workflow DSL v2 plan; GeoKernel validates it, the plan is shown before
/// anything runs, steps that change data need the user's confirmation of that exact plan, and every
/// executed workflow is undoable and recorded with hash-chained provenance.
/// </summary>
internal sealed class MainForm : Form
{
    const string Title = "Agent Map Assistant";

    readonly AgentDemo demo = AgentDemo.Load();
    readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    readonly ToolStripComboBox requests = new() { DropDownStyle = ComboBoxStyle.DropDownList, AutoSize = false, Width = 520 };
    readonly ToolStripButton ask = new("Ask"), fullExtent = new("Full Extent"), provenance = new("Provenance...");
    readonly RichTextBox log = new() { Dock = DockStyle.Fill, ReadOnly = true, BackColor = Color.White, BorderStyle = BorderStyle.FixedSingle, DetectUrls = false };
    readonly TextBox plan = new()
    {
        Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, BackColor = Color.White,
        Font = new Font("Consolas", 9f), PlaceholderText = "The proposed steps appear here before anything runs.",
    };
    readonly Button discard = new() { Text = "Discard", AutoSize = true }, run = new() { Text = "Run", AutoSize = true, MinimumSize = new Size(110, 0) },
        send = new() { Text = "Send", AutoSize = true }, undo = new() { Text = "Undo last", AutoSize = true };
    readonly TextBox input = new() { Dock = DockStyle.Fill, PlaceholderText = "Describe what to do on the map" };
    readonly ToolStripStatusLabel status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft }, planner = new();
    AgentConversation? conversation;

    public MainForm()
    {
        Text = Title;
        ExampleIcon.Apply(this);
        ClientSize = new Size(1360, 820);
        StartPosition = FormStartPosition.CenterScreen;

        var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Padding = new Padding(6, 2, 6, 2) };
        foreach (var item in demo.Requests)
            requests.Items.Add(item.Request);
        requests.SelectedIndex = 0;
        toolbar.Items.AddRange([new ToolStripLabel("Try:"), requests, ask, new ToolStripSeparator(), fullExtent, provenance]);

        var planBox = new GroupBox { Text = "Plan", Dock = DockStyle.Fill, Padding = new Padding(8) };
        var planButtons = new FlowLayoutPanel { Dock = DockStyle.Bottom, FlowDirection = FlowDirection.RightToLeft, AutoSize = true };
        planButtons.Controls.AddRange([run, discard]);
        planBox.Controls.Add(plan);
        planBox.Controls.Add(planButtons);

        var entry = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
        entry.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        entry.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        entry.Controls.Add(input, 0, 0);
        entry.Controls.Add(send, 1, 0);

        var side = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(8) };
        side.RowStyles.Add(new RowStyle(SizeType.Percent, 58));
        side.RowStyles.Add(new RowStyle(SizeType.Percent, 42));
        side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        side.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        side.Controls.Add(log, 0, 0);
        side.Controls.Add(planBox, 0, 1);
        side.Controls.Add(entry, 0, 2);
        side.Controls.Add(undo, 0, 3);

        var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel2 };
        split.Panel1.Controls.Add(viewer);
        split.Panel2.Controls.Add(side);

        var statusStrip = new StatusStrip();
        statusStrip.Items.AddRange([status, planner]);
        Controls.Add(split);
        Controls.Add(toolbar);
        Controls.Add(statusStrip);
        Load += (_, _) => { split.SplitterDistance = Math.Max(400, split.Width - 430); };

        ask.Click += async (_, _) => { if (conversation is not null) await conversation.SubmitAsync(requests.Text); };
        send.Click += async (_, _) => await SendTyped();
        input.KeyDown += async (_, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; await SendTyped(); } };
        run.Click += (_, _) => conversation?.RunPending();
        discard.Click += (_, _) => conversation?.Discard();
        undo.Click += (_, _) => conversation?.UndoLast();
        fullExtent.Click += (_, _) => viewer.FullExtent();
        provenance.Click += (_, _) => ShowProvenance();
        Shown += (_, _) => Initialize();
        FormClosed += (_, _) => conversation?.Dispose();
        Refresh(null);
    }

    void Initialize()
    {
        try
        {
            demo.LoadNeighbourhood(viewer);
            conversation = new AgentConversation(viewer, demo);
        }
        catch (Exception e)
        {
            MessageBox.Show(this, "The map could not be prepared:\n" + e.Message, Title, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        conversation.Logged += Append;
        conversation.Changed += () => Refresh(conversation);
        conversation.Status += text => status.Text = text;
        planner.Text = conversation.PlannerDescription;
        Refresh(conversation);
    }

    async Task SendTyped()
    {
        if (conversation is not null && await conversation.SubmitAsync(input.Text))
            input.Clear();
    }

    void Refresh(AgentConversation? c)
    {
        var ready = c is not null && !c.Busy;
        ask.Enabled = send.Enabled = ready;
        run.Enabled = discard.Enabled = ready && c!.HasPlan;
        undo.Enabled = ready && c!.CanUndo;
        run.Text = c is not null && c.NeedsConfirmation ? "Confirm and run" : "Run";
        plan.Text = c?.PlanText.Replace("\n", Environment.NewLine) ?? "";
    }

    void Append(LogEntry entry)
    {
        if (log.TextLength > 0)
            log.AppendText(Environment.NewLine);
        log.SelectionStart = log.TextLength;
        log.SelectionFont = new Font(log.Font, FontStyle.Bold);
        log.SelectionColor = Color.Black;
        log.AppendText(entry.Who + ": ");
        log.SelectionFont = log.Font;
        log.SelectionColor = entry.Color is null ? Color.Black : ColorTranslator.FromHtml(entry.Color);
        log.AppendText(entry.Text.Replace("\n", Environment.NewLine));
        log.ScrollToCaret();
    }

    void ShowProvenance()
    {
        if (conversation is null)
            return;
        var (summary, json) = conversation.Provenance();
        using var dialog = new Form { Text = "Provenance", ClientSize = new Size(760, 560), StartPosition = FormStartPosition.CenterParent, ShowInTaskbar = false };
        var text = new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, WordWrap = false, Font = new Font("Consolas", 9f), Text = json.Replace("\n", Environment.NewLine) };
        dialog.Controls.Add(text);
        dialog.Controls.Add(new Label { Dock = DockStyle.Top, Text = summary, AutoSize = false, Height = 40, Padding = new Padding(6) });
        dialog.ShowDialog(this);
    }
}
