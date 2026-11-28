using GeoKernel.Examples.GeoAi;
using GeoKernel.Examples.Vlm;
using GeoKernel.NET.WinForms;

namespace GeoKernel.ImageQA.Winforms;

/// <summary>
/// What is in this area? Pick an area of an aerial photo; Florence-2 describes it and marks objects named in plain
/// language on this computer, and a remote vision model (optional) answers questions about it.
/// </summary>
internal sealed class MainForm : Form
{
    readonly VlmSession session = new();
    readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    readonly CheckBox pick = new() { Text = "Pick an area on the map (drag a box or click)", Appearance = Appearance.Button, Dock = DockStyle.Fill, Height = 30, TextAlign = ContentAlignment.MiddleCenter };
    readonly Button describe = new() { Text = "Describe", Dock = DockStyle.Fill, Height = 30 },
        detail = new() { Text = "In detail", Dock = DockStyle.Fill, Height = 30 },
        ask = new() { Text = "Ask", Dock = DockStyle.Fill, Height = 28 },
        mark = new() { Text = "Mark", Dock = DockStyle.Fill, Height = 28 };
    readonly TextBox question = new() { Text = "How many houses are in this area?", Dock = DockStyle.Fill },
        prompt = new() { Text = "car", Dock = DockStyle.Fill },
        answer = new() { Multiline = true, ReadOnly = true, Dock = DockStyle.Fill, Height = 260, ScrollBars = ScrollBars.Vertical };
    readonly Label status = new() { Dock = DockStyle.Fill, AutoSize = false, Height = 52, Text = "Preparing the aerial photo (56 MB) and the Florence-2 model (630 MB) on the first run…" };
    readonly ProgressBar progress = new() { Dock = DockStyle.Fill };
    readonly List<Button> areas = [];
    readonly System.Windows.Forms.Timer poll = new() { Interval = 200 };
    readonly bool smoke;  // --smoke: describe the houses, mark cars (and ask), print, close
    CancellationTokenSource? preparing;

    public MainForm(bool smoke = false)
    {
        this.smoke = smoke;
        Text = "What is in this area? — Windows Forms";
        ExampleIcon.Apply(this);
        Width = 1400;
        Height = 900;
        StartPosition = FormStartPosition.CenterScreen;

        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, AutoScroll = true, Padding = new Padding(8) };
        for (int c = 0; c < 3; ++c)
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3f));
        void Wide(Control control) { panel.Controls.Add(control); panel.SetColumnSpan(control, 3); }
        void Row(Control field, Control button) { panel.Controls.Add(field); panel.SetColumnSpan(field, 2); panel.Controls.Add(button); }
        Wide(new Label { Text = "Example areas", Font = new Font(Font, FontStyle.Bold), AutoSize = true });
        for (int i = 0; i < VlmSession.Areas.Length; ++i)
        {
            int index = i;
            var button = new Button { Text = VlmSession.Areas[i].Name, Dock = DockStyle.Fill, Height = 30 };
            new ToolTip().SetToolTip(button, VlmSession.Areas[i].Note);
            button.Click += (_, _) => Run(() => session.Pick(index), false);
            areas.Add(button);
            panel.Controls.Add(button);
        }
        Wide(pick);
        panel.Controls.Add(describe);
        panel.Controls.Add(detail);
        panel.Controls.Add(new Label());
        Row(question, ask);
        Row(prompt, mark);
        Wide(answer);
        Wide(progress);
        Wide(status);
        Wide(new Label { Text = VlmSession.Help + "\n\n" + VlmSession.Credit, AutoSize = true, MaximumSize = new Size(400, 0) });
        var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 440 };
        split.Panel1.Controls.Add(panel);
        split.Panel2.Controls.Add(viewer);
        Controls.Add(split);
        Load += (_, _) => { split.Panel1MinSize = 420; split.SplitterDistance = 440; };
        Enable(false);

        pick.CheckedChanged += (_, _) => session.Assistant?.SetInteractive(pick.Checked);
        describe.Click += (_, _) => Run(() => session.Assistant!.Describe("caption"));
        detail.Click += (_, _) => Run(() => session.Assistant!.Describe("paragraph"));
        ask.Click += (_, _) => Run(() => session.Assistant!.Ask(question.Text));
        mark.Click += (_, _) => Run(() => session.Assistant!.Find(prompt.Text));
        poll.Tick += (_, _) => { if (session.NewResult() is { } result) Show(result); };
        Shown += async (_, _) => await PrepareAsync();
        FormClosing += (_, _) => { preparing?.Cancel(); poll.Stop(); session.Dispose(); };
    }

    void Enable(bool enabled)
    {
        foreach (Control control in new Control[] { pick, describe, detail, ask, mark, question, prompt }.Concat(areas))
            control.Enabled = enabled;
        if (enabled && !session.HasRemote)
        {
            ask.Enabled = question.Enabled = false;
            question.Text = "Set GEOKERNEL_LLM_PROVIDER to ask questions";
        }
    }

    async Task PrepareAsync()
    {
        preparing = new CancellationTokenSource();
        try
        {
            await session.PrepareAsync(new Progress<DownloadProgress>(p => { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; }), preparing.Token);
            status.Text = "Loading Florence-2…";
            Refresh();
            status.Text = session.Attach(viewer);
            progress.Value = 100;
            Enable(true);
            poll.Start();
            if (smoke)
                BeginInvoke(() => { session.Pick(0); Run(() => session.Assistant!.Describe("caption")); });
        }
        catch (OperationCanceledException)
        {
            status.Text = "Cancelled.";
        }
        catch (Exception e)
        {
            status.Text = e.Message;
            if (smoke)
            {
                Console.WriteLine("error: " + e.Message);
                BeginInvoke(Close);
            }
        }
    }

    // Starts a run (or another action); the result arrives through the poll timer.
    void Run(Action action, bool starts = true)
    {
        try
        {
            action();
            if (!starts)
                return;
            Enable(false);
            status.Text = "Working… (Florence-2 on this computer, or the remote model for questions)";
        }
        catch (Exception e)
        {
            status.Text = e.Message;
        }
    }

    void Show(System.Text.Json.Nodes.JsonObject result)
    {
        Enable(true);
        status.Text = VlmSession.Status(result);
        answer.Text = VlmSession.Answer(result);
        if (smoke)
            Smoke(result);
    }

    void Smoke(System.Text.Json.Nodes.JsonObject result)
    {
        var kind = (string?)result["kind"];
        Console.WriteLine(kind == "find" ? $"smoke find car: {result["objects"]?.AsArray().Count} objects ({result["elapsedMs"]} ms)"
                                         : $"smoke {kind}: {result["text"] ?? result["error"]} ({result["elapsedMs"]} ms)");
        if (kind == "describe")
            Run(() => session.Assistant!.Find("car"));
        else if (kind == "find" && session.HasRemote)
            Run(() => session.Assistant!.Ask("How many houses are in this area?"));
        else
            BeginInvoke(Close);
    }
}
