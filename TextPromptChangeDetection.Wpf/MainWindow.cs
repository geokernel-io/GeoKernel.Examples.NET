using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GeoKernel.Examples.GeoAi;
using GeoKernel.Examples.TextPromptChange;
using GeoKernel.NET.Wpf.Controls;
using Microsoft.Win32;

namespace GeoKernel.TextPromptChangeDetection.Wpf;

/// <summary>
/// What changed between dates, for any concept SAM 3 can segment from text: every date is segmented
/// once, objects are tracked from date to date, the timeline plays the changes and each interval's
/// changes go through a QA review queue before they are saved.
/// </summary>
internal sealed class MainWindow : Window
{
    readonly ChangeSession session = new();
    readonly GeoKernelViewerControl viewer = new();
    readonly TextBox prompt = new() { Text = "building" }, reviewer = new() { ToolTip = "Your name (recorded in the audit trail)" };
    readonly ComboBox score = Choice("0.50", "0.30", "0.20", "0.60", "0.75"),
        dates = Choice("Two dates: 2016 → 2025", "Series: 9 dates 2016 – 2025"), area = Choice("Demo area (320 m)", "Full dataset (1 km)"),
        provider = Choice("CPU", "CUDA", "Auto");
    readonly Button run = Make("Find changes"), cancel = Make("Cancel"), play = Make("▶ Play"), approve = Make("Approve"), reject = Make("Reject"),
        approvePending = Make("Approve all pending"), saveAccepted = Make("Save accepted changes…"), saveSeries = Make("Save series GeoPackage…");
    readonly Slider timeline = new() { IsSnapToTickEnabled = true, TickFrequency = 1, TickPlacement = System.Windows.Controls.Primitives.TickPlacement.BottomRight, Minimum = 0, Maximum = 0 };
    readonly TextBlock dateLabel = new() { TextWrapping = TextWrapping.Wrap, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) },
        status = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4) };
    readonly ProgressBar progress = new() { Height = 18, Maximum = 100 };
    readonly ListView changes = new() { Height = 230, SelectionMode = SelectionMode.Single };
    readonly DispatcherTimer poll = new() { Interval = TimeSpan.FromMilliseconds(250) }, player = new() { Interval = TimeSpan.FromMilliseconds(1100) };
    readonly bool smoke;  // --smoke: run the demo area, print the result, close
    CancellationTokenSource? preparing;
    int shown = -1;
    bool busy, applied;

    static Button Make(string text) => new() { Content = text, Padding = new Thickness(8, 3, 8, 3), Margin = new Thickness(0, 2, 4, 2) };
    static ComboBox Choice(params string[] items)
    {
        var box = new ComboBox { ItemsSource = items, SelectedIndex = 0 };
        return box;
    }
    static TextBlock Heading(string text) => new() { Text = text, FontWeight = FontWeights.Bold, Margin = new Thickness(0, 10, 0, 4) };

    public MainWindow(bool smoke = false)
    {
        this.smoke = smoke;
        Title = "Text-prompt change detection — WPF";
        ExampleIcon.Apply(this);
        Width = 1440;
        Height = 920;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        dates.SelectedIndex = 1;

        var grid = new GridView();
        foreach (var (header, path, width) in new[] { ("Change", "Change", 100.0), ("Score", "ScoreText", 70.0), ("Area m²", "AreaText", 70.0), ("QA state", "State", 150.0) })
            grid.Columns.Add(new GridViewColumn { Header = header, DisplayMemberBinding = new System.Windows.Data.Binding(path), Width = width });
        changes.View = grid;

        var form = new Grid();
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(130) });
        form.ColumnDefinitions.Add(new ColumnDefinition());
        void Row(string label, UIElement control)
        {
            form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var text = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center };
            Grid.SetRow(text, form.RowDefinitions.Count - 1);
            Grid.SetRow(control, form.RowDefinitions.Count - 1);
            Grid.SetColumn(control, 1);
            ((FrameworkElement)control).Margin = new Thickness(0, 2, 0, 2);
            form.Children.Add(text);
            form.Children.Add(control);
        }
        Row("What to look for", prompt);
        Row("Score threshold", score);
        Row("Dates", dates);
        Row("Area", area);
        Row("Provider", provider);
        Row("Reviewer", reviewer);

        StackPanel Horizontal(params UIElement[] children)
        {
            var panel = new StackPanel { Orientation = Orientation.Horizontal };
            foreach (var child in children) panel.Children.Add(child);
            return panel;
        }
        var side = new StackPanel { Margin = new Thickness(10) };
        side.Children.Add(form);
        side.Children.Add(Horizontal(run, cancel));
        side.Children.Add(progress);
        side.Children.Add(status);
        side.Children.Add(Heading("Timeline"));
        side.Children.Add(timeline);
        side.Children.Add(Horizontal(play, dateLabel));
        side.Children.Add(new TextBlock { Text = ChangeSession.Legend, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0) });
        side.Children.Add(Heading("Review the changes of this interval"));
        side.Children.Add(changes);
        side.Children.Add(Horizontal(approve, reject, approvePending));
        side.Children.Add(Horizontal(saveAccepted, saveSeries));
        side.Children.Add(new TextBlock { Text = ChangeSession.Help, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 10, 0, 0), Foreground = Brushes.DimGray });
        var root = new Grid();
        root.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(460) });
        root.ColumnDefinitions.Add(new ColumnDefinition());
        var scroll = new ScrollViewer { Content = side, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Grid.SetColumn(viewer, 1);
        root.Children.Add(scroll);
        root.Children.Add(viewer);
        Content = root;

        run.Click += async (_, _) => await FindChangesAsync();
        cancel.Click += (_, _) => Cancel();
        timeline.ValueChanged += (_, e) => ShowDate((int)e.NewValue);
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
        ContentRendered += async (_, _) => await PrepareAsync();
        UpdateControls();
    }

    sealed record RowView(ChangeRow Row)
    {
        public string Change => Row.Change;
        public string ScoreText => ChangeSession.Format(Row.Score, "F3");
        public string AreaText => ChangeSession.Format(Row.Area, "F0");
        public string State => Row.State;
    }

    IProgress<DownloadProgress> Reporter() => new Progress<DownloadProgress>(p => { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; });

    void ShowDemoArea() => viewer.ViewExtent = new GeoKernelExtent(ChangeSession.DemoView[0], ChangeSession.DemoView[1], ChangeSession.DemoView[2], ChangeSession.DemoView[3]);

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
            ShowDemoArea();
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
            await session.StartAsync(prompt.Text.Trim(), double.Parse((string)score.SelectedItem!, System.Globalization.CultureInfo.InvariantCulture),
                dates.SelectedIndex == 1, area.SelectedIndex == 0, (string)provider.SelectedItem!, Reporter(), preparing.Token);
            poll.Start();
        }
        catch (Exception error) { status.Text = error.Message; busy = false; UpdateControls(); }
    }

    void Cancel()
    {
        preparing?.Cancel();
        try { if (session.Job is not null && poll.IsEnabled) session.Job.Cancel(); } catch (ChangeTaskException) { }
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
        if (timeline.Value == timeline.Maximum) ShowDate((int)timeline.Maximum); else timeline.Value = timeline.Maximum;
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
        if (player.IsEnabled) { StopPlay(); return; }
        timeline.Value = 0;
        play.Content = "■ Stop";
        player.Start();
    }

    void StopPlay()
    {
        player.Stop();
        play.Content = "▶ Play";
    }

    void FillChanges()
    {
        changes.ItemsSource = session.Rows(shown).Select(row => new RowView(row)).ToList();
        UpdateControls();
    }

    void LocateSelected()
    {
        if (changes.SelectedItem is not RowView view) return;
        var e = ChangeSession.Around(view.Row.Geometry);
        viewer.ViewExtent = new GeoKernelExtent(e[0], e[1], e[2], e[3]);
    }

    void Decide(string action)
    {
        var row = (changes.SelectedItem as RowView)?.Row;
        if (action != "approvePending" && row is null) return;
        int selected = changes.SelectedIndex;
        try { session.Review(shown, action, reviewer.Text.Trim(), row?.QaId); }
        catch (Exception error) { status.Text = error.Message; return; }
        FillChanges();
        if (selected >= 0 && selected + 1 < changes.Items.Count) changes.SelectedIndex = selected + 1;
    }

    void Save(bool accepted)
    {
        var dialog = new SaveFileDialog
        {
            FileName = accepted ? $"accepted-changes-{session.Dates[shown][..4]}.gpkg" : "change-series.gpkg",
            Filter = accepted ? "GeoPackage (*.gpkg)|*.gpkg|GeoJSON (*.geojson)|*.geojson" : "GeoPackage (*.gpkg)|*.gpkg",
        };
        if (dialog.ShowDialog(this) != true) return;
        try { status.Text = accepted ? session.SaveAccepted(shown, dialog.FileName) : session.SaveSeries(dialog.FileName); }
        catch (Exception error) { status.Text = error.Message; }
    }

    void UpdateControls()
    {
        run.IsEnabled = !busy && session.SeriesDirectory is not null;
        cancel.IsEnabled = busy;
        foreach (var control in new Control[] { prompt, score, dates, area, provider }) control.IsEnabled = !busy;
        bool ready = !busy && applied;
        timeline.IsEnabled = play.IsEnabled = saveSeries.IsEnabled = ready;
        bool reviewing = ready && session.CanReview(shown);
        approve.IsEnabled = reject.IsEnabled = approvePending.IsEnabled = reviewing && reviewer.Text.Trim().Length > 0;
        saveAccepted.IsEnabled = reviewing;
    }

    protected override void OnClosed(EventArgs e)
    {
        poll.Stop();
        player.Stop();
        preparing?.Cancel();
        session.Dispose();  // joins the current date if a task is still running
        base.OnClosed(e);
    }
}
