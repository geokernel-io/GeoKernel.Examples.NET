using GeoKernel.Examples.GeoAi;
using GeoKernel.Examples.TextPromptChange;
using GeoKernel.NET.WinForms;

namespace GeoKernel.TextPromptChangeDetection.Winforms;

/// <summary>
/// What changed between dates, for any concept SAM 3 can segment from text: every date is segmented
/// once, objects are tracked from date to date, the timeline plays the changes and each interval's
/// changes go through a QA review queue before they are saved.
/// </summary>
internal sealed class MainForm : Form
{
    readonly ChangeSession session = new();
    readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    readonly TextBox prompt = new() { Text = "building", Dock = DockStyle.Fill }, reviewer = new() { Dock = DockStyle.Fill, PlaceholderText = "Your name (recorded in the audit trail)" };
    readonly NumericUpDown score = new() { DecimalPlaces = 2, Increment = 0.05M, Minimum = 0.05M, Maximum = 0.95M, Value = 0.5M, Dock = DockStyle.Fill };
    readonly ComboBox dates = Choice("Two dates: 2016 → 2025", "Series: 9 dates 2016 – 2025"), area = Choice("Demo area (320 m)", "Full dataset (1 km)"), provider = Choice("CPU", "CUDA", "Auto");
    readonly Button run = Make("Find changes"), cancel = Make("Cancel"), play = Make("▶ Play"), approve = Make("Approve"), reject = Make("Reject"),
        approvePending = Make("Approve all pending"), saveAccepted = Make("Save accepted changes…"), saveSeries = Make("Save series GeoPackage…");
    readonly TrackBar timeline = new() { Dock = DockStyle.Fill, TickStyle = TickStyle.BottomRight, LargeChange = 1 };
    readonly Label dateLabel = new() { Dock = DockStyle.Fill, AutoSize = false, Height = 36 }, status = new() { Dock = DockStyle.Fill, AutoSize = false, Height = 52 };
    readonly ProgressBar progress = new() { Dock = DockStyle.Fill };
    readonly DataGridView changes = new()
    {
        Dock = DockStyle.Fill, Height = 230, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
    };
    readonly System.Windows.Forms.Timer poll = new() { Interval = 250 }, player = new() { Interval = 1100 };
    readonly bool smoke;  // --smoke: run the demo area, print the result, close
    CancellationTokenSource? preparing;
    int shown = -1;
    bool busy, applied;

    static Button Make(string text) => new() { Text = text, Dock = DockStyle.Fill, Height = 30 };
    static ComboBox Choice(params string[] items)
    {
        var box = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
        box.Items.AddRange(items);
        box.SelectedIndex = 0;
        return box;
    }

    public MainForm(bool smoke = false)
    {
        this.smoke = smoke;
        Text = "Text-prompt change detection — Windows Forms";
        ExampleIcon.Apply(this);
        Width = 1440;
        Height = 920;
        StartPosition = FormStartPosition.CenterScreen;
        dates.SelectedIndex = 1;
        changes.Columns.Add("change", "Change");
        changes.Columns.Add("score", "Score");
        changes.Columns.Add("area", "Area m²");
        changes.Columns.Add("state", "QA state");

        var panel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoScroll = true, Padding = new Padding(8) };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        void Row(string label, Control control) { panel.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }); panel.Controls.Add(control); }
        void Wide(Control control) { panel.Controls.Add(control); panel.SetColumnSpan(control, 2); }
        Row("What to look for", prompt);
        Row("Score threshold", score);
        Row("Dates", dates);
        Row("Area", area);
        Row("Provider", provider);
        panel.Controls.Add(run);
        panel.Controls.Add(cancel);
        Wide(progress);
        Wide(status);
        Wide(new Label { Text = "Timeline", Font = new Font(Font, FontStyle.Bold), AutoSize = true });
        Wide(timeline);
        panel.Controls.Add(play);
        panel.Controls.Add(dateLabel);
        Wide(new Label { Text = ChangeSession.Legend, AutoSize = true });
        Wide(new Label { Text = "Review the changes of this interval", Font = new Font(Font, FontStyle.Bold), AutoSize = true });
        Wide(changes);
        Row("Reviewer", reviewer);
        var decide = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, Height = 34 };
        for (int i = 0; i < 3; ++i) decide.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.3F));
        decide.Controls.AddRange([approve, reject, approvePending]);
        Wide(decide);
        Wide(saveAccepted);
        Wide(saveSeries);
        Wide(new Label { Text = ChangeSession.Help, AutoSize = true, MaximumSize = new Size(420, 0) });
        var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1, SplitterDistance = 460 };
        split.Panel1.Controls.Add(panel);
        split.Panel2.Controls.Add(viewer);
        Controls.Add(split);
        Load += (_, _) => { split.Panel1MinSize = 440; split.SplitterDistance = 470; };

        run.Click += async (_, _) => await FindChangesAsync();
        cancel.Click += (_, _) => Cancel();
        timeline.ValueChanged += (_, _) => ShowDate(timeline.Value);
        play.Click += (_, _) => TogglePlay();
        player.Tick += (_, _) => { if (timeline.Value >= timeline.Maximum) StopPlay(); else timeline.Value++; };
        poll.Tick += (_, _) => Poll();
        changes.SelectionChanged += (_, _) => LocateSelected();
        approve.Click += (_, _) => Decide("approve");
        reject.Click += (_, _) => Decide("reject");
        approvePending.Click += (_, _) => Decide("approvePending");
        saveAccepted.Click += (_, _) => Save(true);
        saveSeries.Click += (_, _) => Save(false);
        reviewer.TextChanged += (_, _) => UpdateControls();
        Shown += async (_, _) => await PrepareAsync();
        UpdateControls();
    }

    IProgress<DownloadProgress> Reporter() => new Progress<DownloadProgress>(p => { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; });

    async Task PrepareAsync()
    {
        busy = true;
        UpdateControls();
        preparing = new CancellationTokenSource();
        try
        {
            status.Text = "Preparing the 9-date Oosterwold series…";
            await session.PrepareSeriesAsync(Reporter(), preparing.Token);
            ShowRaster(ChangeSession.Years[0]);
            viewer.ViewExtent = new GeoKernelExtent(ChangeSession.DemoView[0], ChangeSession.DemoView[1], ChangeSession.DemoView[2], ChangeSession.DemoView[3]);
            progress.Value = 100;
            status.Text = "Series ready (2016 shown). Enter what to look for and click Find changes.";
        }
        catch (Exception error) { status.Text = error.Message; }
        finally { busy = false; UpdateControls(); }
        if (smoke && session.SeriesDirectory is not null) await FindChangesAsync();
    }

    async Task FindChangesAsync()
    {
        if (busy || session.SeriesDirectory is null || prompt.Text.Trim().Length == 0) return;
        StopPlay();
        busy = true;
        applied = false;
        UpdateControls();
        preparing = new CancellationTokenSource();
        try
        {
            status.Text = "Preparing the SAM 3 text package…";
            await session.StartAsync(prompt.Text.Trim(), (double)score.Value, dates.SelectedIndex == 1, area.SelectedIndex == 0,
                (string)provider.SelectedItem!, Reporter(), preparing.Token);
            poll.Start();
        }
        catch (Exception error) { status.Text = error.Message; busy = false; UpdateControls(); }
    }

    void Cancel()
    {
        preparing?.Cancel();
        try { if (session.Job is not null && poll.Enabled) session.Job.Cancel(); } catch (ChangeTaskException) { }
    }

    void Poll()
    {
        var state = session.Poll();
        progress.Value = Math.Clamp((int)state["percent"]!, 0, 100);
        status.Text = ChangeSession.Describe(state);
        switch ((string?)state["state"])
        {
            case "completed":
                poll.Stop();
                busy = false;
                Apply();
                break;
            case "failed" or "cancelled":
                poll.Stop();
                busy = false;
                status.Text = (string?)state["error"] is { Length: > 0 } error ? error : "Cancelled. The previous result is kept.";
                UpdateControls();
                break;
        }
    }

    void Apply()
    {
        applied = true;
        timeline.Maximum = session.Dates.Count - 1;
        shown = -1;
        if (timeline.Value == timeline.Maximum) ShowDate(timeline.Maximum); else timeline.Value = timeline.Maximum;
        progress.Value = 100;
        status.Text = session.Summary();
        UpdateControls();
        if (!smoke) return;
        timeline.Value = Math.Min(3, timeline.Maximum);
        Console.WriteLine($"smoke | {status.Text} | {dateLabel.Text}");
        Close();
    }

    void ShowRaster(int year)
    {
        viewer.ClearLayers();
        viewer.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.Wgs84);
        if (!viewer.AddLayerFile(session.RasterPath(year))) throw new InvalidOperationException($"Cannot open the {year} orthophoto.");
        viewer.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.Wgs84);
        viewer.SetLayerName(0, $"Orthophoto {year}");
    }

    void ShowDate(int date)
    {
        if (!applied || date < 0 || date >= session.Dates.Count || date == shown) return;
        shown = date;
        ShowRaster(int.Parse(session.Dates[date][..4]));
        var (layers, label) = session.Frame(date);
        foreach (var layer in layers)
        {
            if (!viewer.AddLayerFile(layer.Path)) continue;
            viewer.SetLayerName(0, layer.Name);
            viewer.SetLayerStyleJson(0, layer.StyleJson);
        }
        viewer.RefreshLayers();
        dateLabel.Text = label;
        FillChanges();
    }

    void TogglePlay()
    {
        if (player.Enabled) { StopPlay(); return; }
        timeline.Value = 0;
        play.Text = "■ Stop";
        player.Start();
    }

    void StopPlay()
    {
        player.Stop();
        play.Text = "▶ Play";
    }

    void FillChanges()
    {
        changes.Rows.Clear();
        foreach (var row in session.Rows(shown))
        {
            int index = changes.Rows.Add(row.Change, ChangeSession.Format(row.Score, "F3"), ChangeSession.Format(row.Area, "F0"), row.State);
            changes.Rows[index].Tag = row;
        }
        UpdateControls();
    }

    void LocateSelected()
    {
        if (changes.SelectedRows.Count == 0 || changes.SelectedRows[0].Tag is not ChangeRow row) return;
        var e = ChangeSession.Around(row.Geometry);
        viewer.ViewExtent = new GeoKernelExtent(e[0], e[1], e[2], e[3]);
    }

    void Decide(string action)
    {
        var row = changes.SelectedRows.Count > 0 ? changes.SelectedRows[0].Tag as ChangeRow : null;
        if (action != "approvePending" && row is null) return;
        int selected = changes.SelectedRows.Count > 0 ? changes.SelectedRows[0].Index : -1;
        try { session.Review(shown, action, reviewer.Text.Trim(), row?.QaId); }
        catch (Exception error) { status.Text = error.Message; return; }
        FillChanges();
        if (selected + 1 < changes.Rows.Count && selected >= 0) changes.Rows[selected + 1].Selected = true;
    }

    void Save(bool accepted)
    {
        using var dialog = new SaveFileDialog
        {
            FileName = accepted ? $"accepted-changes-{session.Dates[shown][..4]}.gpkg" : "change-series.gpkg",
            Filter = accepted ? "GeoPackage (*.gpkg)|*.gpkg|GeoJSON (*.geojson)|*.geojson" : "GeoPackage (*.gpkg)|*.gpkg",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { status.Text = accepted ? session.SaveAccepted(shown, dialog.FileName) : session.SaveSeries(dialog.FileName); }
        catch (Exception error) { status.Text = error.Message; }
    }

    void UpdateControls()
    {
        run.Enabled = !busy && session.SeriesDirectory is not null;
        cancel.Enabled = busy;
        foreach (Control control in new Control[] { prompt, score, dates, area, provider }) control.Enabled = !busy;
        bool ready = !busy && applied;
        timeline.Enabled = play.Enabled = saveSeries.Enabled = ready;
        bool reviewing = ready && session.CanReview(shown);
        approve.Enabled = reject.Enabled = approvePending.Enabled = reviewing && reviewer.Text.Trim().Length > 0;
        saveAccepted.Enabled = reviewing;
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        poll.Stop();
        player.Stop();
        preparing?.Cancel();
        session.Dispose();  // joins the current date if a task is still running
        base.OnFormClosed(e);
    }
}
