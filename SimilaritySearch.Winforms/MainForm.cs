using GeoKernel.Examples.GeoAi;
using GeoKernel.Examples.Similarity;
using GeoKernel.NET.WinForms;

namespace GeoKernel.FindSimilar.Winforms;

/// <summary>
/// Find similar: pick a solar park, a lake, a town or a field (or click / drag on the map) and every
/// 80 m cell of Flevoland is ranked by AlphaEarth embedding similarity, with a heat map and ranked matches.
/// </summary>
internal sealed class MainForm : Form
{
    readonly SimilaritySession session = new();
    readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    readonly CheckBox interactive = new() { Text = "Find similar on the map (click or drag a box)", Appearance = Appearance.Button, Dock = DockStyle.Fill, Height = 30, Enabled = false, TextAlign = ContentAlignment.MiddleCenter };
    readonly TrackBar threshold = new() { Minimum = 50, Maximum = 99, Value = 90, Dock = DockStyle.Fill, TickStyle = TickStyle.None };
    readonly Label thresholdLabel = new() { Text = "Heat map ≥ 0.90", AutoSize = true, Anchor = AnchorStyles.Left },
        status = new() { Dock = DockStyle.Fill, AutoSize = false, Height = 52, Text = "Preparing the Flevoland AlphaEarth embeddings (31 MB on the first run)…" };
    readonly ListBox matches = new() { Dock = DockStyle.Fill, Height = 330, IntegralHeight = false };
    readonly ProgressBar progress = new() { Dock = DockStyle.Fill };
    readonly List<Button> presets = [];
    readonly System.Windows.Forms.Timer poll = new() { Interval = 200 };
    readonly bool smoke;  // --smoke: search the solar park, print the result, close
    CancellationTokenSource? preparing;

    public MainForm(bool smoke = false)
    {
        this.smoke = smoke;
        Text = "Find similar — Windows Forms";
        ExampleIcon.Apply(this);
        Width = 1400;
        Height = 900;
        StartPosition = FormStartPosition.CenterScreen;

        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true, Padding = new Padding(8) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        void Wide(Control control) { panel.Controls.Add(control); panel.SetColumnSpan(control, 2); }
        Wide(new Label { Text = "Look for places like…", Font = new Font(Font, FontStyle.Bold), AutoSize = true });
        for (int i = 0; i < SimilaritySession.Presets.Length; ++i)
        {
            int index = i;
            var button = new Button { Text = SimilaritySession.Presets[i].Name, Dock = DockStyle.Fill, Height = 30, Enabled = false };
            new ToolTip().SetToolTip(button, SimilaritySession.Presets[i].Note);
            button.Click += (_, _) => SearchPreset(index);
            presets.Add(button);
            panel.Controls.Add(button);
        }
        Wide(interactive);
        panel.Controls.Add(thresholdLabel);
        panel.Controls.Add(threshold);
        Wide(matches);
        Wide(progress);
        Wide(status);
        Wide(new Label { Text = SimilaritySession.Help + "\n\n" + SimilaritySession.Attribution, AutoSize = true, MaximumSize = new Size(380, 0) });
        var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 420 };
        split.Panel1.Controls.Add(panel);
        split.Panel2.Controls.Add(viewer);
        Controls.Add(split);
        Load += (_, _) => { split.Panel1MinSize = 400; split.SplitterDistance = 420; };

        interactive.CheckedChanged += (_, _) => session.Search?.SetInteractive(interactive.Checked);
        threshold.ValueChanged += (_, _) =>
        {
            thresholdLabel.Text = $"Heat map ≥ {threshold.Value / 100.0:0.00}";
            session.Search?.SetHeatmapThreshold(threshold.Value / 100.0);
        };
        matches.SelectedIndexChanged += (_, _) => { if (matches.SelectedIndex >= 0) session.Search?.ZoomToMatch(matches.SelectedIndex); };
        poll.Tick += (_, _) => { if (session.NewInteractiveResult() is { } result) Show(result); };
        Shown += async (_, _) => await PrepareAsync();
        FormClosing += (_, _) => { preparing?.Cancel(); poll.Stop(); session.Dispose(); };
    }

    async Task PrepareAsync()
    {
        preparing = new CancellationTokenSource();
        try
        {
            await session.PrepareAsync(new Progress<DownloadProgress>(p => { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; }), preparing.Token);
            status.Text = session.Attach(viewer);
            progress.Value = 100;
            foreach (var button in presets)
                button.Enabled = true;
            interactive.Enabled = true;
            poll.Start();
            if (smoke)
                BeginInvoke(() => SearchPreset(0));
        }
        catch (OperationCanceledException)
        {
            status.Text = "Cancelled.";
        }
        catch (Exception e)
        {
            status.Text = e.Message;
        }
    }

    void SearchPreset(int index)
    {
        try
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var (result, view) = session.SearchPreset(index);
            Show(result);
            viewer.ViewExtent = view;
            if (smoke)
            {
                Console.WriteLine($"smoke: {SimilaritySession.Presets[index].Name}: {SimilaritySession.Summary(result)} ({watch.ElapsedMilliseconds} ms with the view)");
                BeginInvoke(Close);
            }
        }
        catch (Exception e)
        {
            status.Text = e.Message;
        }
    }

    void Show(System.Text.Json.Nodes.JsonObject result)
    {
        matches.BeginUpdate();
        matches.Items.Clear();
        matches.Items.AddRange(SimilaritySession.MatchLines(result).Cast<object>().ToArray());
        matches.EndUpdate();
        status.Text = SimilaritySession.Summary(result);
    }
}
