using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GeoKernel.Examples.DsmToDtm;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf.Controls;
using Microsoft.Win32;

namespace GeoKernel.DsmToDtmAccuracy.Wpf;

/// <summary>
/// Reference accuracy by area: three linked native 2D maps keep the original DSM beside
/// classical and AI error maps on one shared colour scale. Select an area to compare numeric
/// errors, their distribution and individual source pixels. No inference runs here.
/// </summary>
public sealed class MainWindow : Window
{
    static Brush Hex(string color) => (Brush)new BrushConverter().ConvertFromString(color)!;
    static readonly (double XMin, double YMin, double XMax, double YMax) FullView = (2677090, 1251340, 2677360, 1251610);
    static readonly double[] Thresholds = Enumerable.Range(1, 40).Select(i => i * .5).Prepend(.1).Prepend(.25).ToArray();

    readonly DsmOptions options;
    readonly SampleAssets assets = SampleAssets.ZurichV2();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-accuracy-").FullName;
    readonly GeoKernelViewerControl[] maps = [new(), new(), new()];
    readonly TextBlock[] headings = new TextBlock[3];
    readonly ComboBox mode = new() { ItemsSource = new[] { "Signed reference errors", "Absolute reference errors", "Terrain estimates" }, SelectedIndex = 0 };
    readonly ComboBox range = new() { ItemsSource = new[] { "1 m", "2 m", "5 m", "10 m" }, SelectedIndex = 1, Width = 90 };
    readonly ComboBox threshold = new() { Width = 90 };
    readonly ComboBox presets = new() { ItemsSource = Accuracy.Presets, SelectedIndex = 0 };
    readonly TextBlock instructions = Card("Preparing sample data..."), winners = Card("Compare lower errors for each metric."),
        probe = Card("Click a map pixel to inspect its heights and signed errors."), status = new(),
        scope = new() { Text = "No evaluation area loaded.", Margin = new Thickness(0, 4, 0, 4), TextWrapping = TextWrapping.Wrap };
    readonly Button export = new() { Content = "Export area report" }, reload = new() { Content = "Retry / reload sample" }, cancel = new() { Content = "Cancel loading" };
    readonly ToggleButton areaButton = new() { Content = "Select evaluation area", Margin = new Thickness(2, 0, 2, 0) };
    readonly List<UIElement> navigation = [];
    readonly Grid table = new() { Background = Brushes.White };
    readonly TextBlock[,] cells = new TextBlock[3, 6];
    readonly Legend legend = new() { Height = 62 };
    readonly Histogram histogram = new() { Height = 155 };
    readonly ProgressBar progress = new() { Width = 210, Height = 14, Maximum = 100, Visibility = Visibility.Collapsed };
    readonly (double X, double Y)?[] presses = new (double X, double Y)?[3];
    readonly GeoKernelExtent?[] applied = new GeoKernelExtent?[3];
    CancellationTokenSource cts = new();
    Grids? grids;
    PixelWindow? area;
    string areaName = "";
    AccuracyMetric[]? metrics;
    (double X, double Y)? probePoint;
    bool busy, closeQueued, closed, syncing, updatingPreset;
    int generation;

    static TextBlock Wrap(string text) => new() { Text = text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) };
    static TextBlock Card(string text)
    {
        var block = Wrap(text);
        block.Background = Brushes.White;
        block.Padding = new Thickness(10);
        return block;
    }

    public MainWindow(DsmOptions options)
    {
        this.options = options;
        Title = AccuracyText.Title;
        Width = 1720;
        Height = 970;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Hex("#f2f6f8");
        Foreground = Hex("#233e50");
        ExampleIcon.Apply(this);
        threshold.ItemsSource = Thresholds.OrderBy(v => v).Select(v => $"{v:0.##} m").ToArray();
        threshold.SelectedIndex = Array.IndexOf(Thresholds.OrderBy(v => v).ToArray(), 2.0);
        Content = BuildLayout();
        SetBusy(false);
        ContentRendered += (_, _) => _ = StartAsync();
    }

    double ThresholdValue => Thresholds.OrderBy(v => v).ElementAt(Math.Max(0, threshold.SelectedIndex));

    UIElement BuildLayout()
    {
        var root = new DockPanel();
        var toolbar = new ToolBar { Background = Brushes.White, Padding = new Thickness(6) };
        ToolBarTray.SetIsLocked(toolbar, true);
        Button Tool(string text, string? image, Action action)
        {
            object content = text;
            if (image != null)
            {
                using var stream = typeof(MainWindow).Assembly.GetManifestResourceStream("GeoKernel.Examples." + image)!;
                var panel = new StackPanel { Orientation = Orientation.Horizontal };
                panel.Children.Add(new Image { Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad), Width = 20, Height = 20, Margin = new Thickness(0, 0, 4, 0) });
                panel.Children.Add(new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center });
                content = panel;
            }
            var button = new Button { Content = content, Margin = new Thickness(2, 0, 2, 0) };
            System.Windows.Automation.AutomationProperties.SetName(button, text);
            button.Click += (_, _) => action();
            toolbar.Items.Add(button);
            navigation.Add(button);
            return button;
        }
        toolbar.Items.Add(areaButton);
        Tool("Evaluate current view", null, () => { var e = maps[0].ViewExtent; ChooseArea((e.XMin, e.YMin, e.XMax, e.YMax), "Current map view"); });
        Tool("Full sample", null, () => ApplyPreset(0));
        toolbar.Items.Add(new Separator());
        Tool("Pan", null, () => UseTool(GeoKernelViewerTool.Pan));
        Tool("Zoom box", null, () => UseTool(GeoKernelViewerTool.ZoomBox));
        Tool("Zoom in", "ZoomIn.png", () => maps[0].ZoomIn(1.5));
        Tool("Zoom out", "ZoomOut.png", () => maps[0].ZoomOut(1.5));
        Tool("Full extent", "FullExtent.png", FullExtent);
        var tray = new ToolBarTray { Background = Brushes.White };
        tray.ToolBars.Add(toolbar);
        DockPanel.SetDock(tray, Dock.Top);
        root.Children.Add(tray);

        var statusBar = new StatusBar();
        statusBar.Items.Add(new StatusBarItem { Content = status });
        statusBar.Items.Add(new StatusBarItem { Content = progress, HorizontalAlignment = HorizontalAlignment.Right });
        DockPanel.SetDock(statusBar, Dock.Bottom);
        root.Children.Add(statusBar);

        var side = new StackPanel { Width = 248, Margin = new Thickness(12) };
        var title = Wrap("Where does each\nmethod work better?");
        title.FontSize = 21;
        title.FontWeight = FontWeights.SemiBold;
        title.Foreground = Hex("#145e72");
        DockPanel Field(string text, Control control)
        {
            var row = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
            row.Children.Add(new TextBlock { Text = text, VerticalAlignment = VerticalAlignment.Center, Width = 110 });
            row.Children.Add(control);
            return row;
        }
        mode.Margin = presets.Margin = new Thickness(0, 0, 0, 10);
        foreach (var button in new[] { export, reload, cancel }) { button.Padding = new Thickness(7); button.Margin = new Thickness(0, 4, 0, 4); }
        foreach (var element in new UIElement[] {
            title, Wrap("Zürich · same DSM and reference\n250 × 250 m · 0.5 m source cells"), instructions,
            Wrap("Map comparison"), mode, Field("Display limit", range), Field("Large error >", threshold),
            Wrap("Study-area preset"), presets, winners, probe, Wrap(AccuracyText.Note), export, reload, cancel,
            Wrap("Source: Federal Office of Topography swisstopo\nAI: David Perez · experimental pix_grad") })
            side.Children.Add(element);
        var scroll = new ScrollViewer { Content = side, Width = 290, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        DockPanel.SetDock(scroll, Dock.Left);
        root.Children.Add(scroll);

        var workspace = new Grid { Margin = new Thickness(0, 12, 12, 6) };
        foreach (var height in new[] { new GridLength(1, GridUnitType.Star), GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto })
            workspace.RowDefinitions.Add(new RowDefinition { Height = height });
        void Row(UIElement element, int row) { Grid.SetRow(element, row); workspace.Children.Add(element); }
        var views = new Grid();
        for (int i = 0; i < 3; i++) views.ColumnDefinitions.Add(new ColumnDefinition());
        for (int i = 0; i < 3; i++)
        {
            int pane = i;
            var column = new DockPanel { Margin = new Thickness(i == 0 ? 0 : 4, 0, i == 2 ? 0 : 4, 0), MinWidth = 270, MinHeight = 290 };
            headings[i] = new TextBlock { Text = AccuracyText.Titles[i], Background = Hex("#e4eef2"), Foreground = Hex("#125a70"), Padding = new Thickness(9), FontWeight = FontWeights.SemiBold };
            DockPanel.SetDock(headings[i], Dock.Top);
            column.Children.Add(headings[i]);
            column.Children.Add(maps[i]);
            maps[i].VisibleExtentChanged += (_, _) => ExtentChanged(pane);
            maps[i].MapSelectionBoxFinished += (_, e) => SelectionFinished(e.WorldExtent);
            maps[i].MapMouseDown += (_, e) => { if (e.ButtonOrButtons == 1) presses[pane] = (e.ScreenPoint.X, e.ScreenPoint.Y); };
            maps[i].MapMouseUp += (_, e) => MapClick(pane, e);
            Grid.SetColumn(column, i);
            views.Children.Add(column);
        }
        Row(views, 0);
        legend.Margin = new Thickness(0, 8, 0, 0);
        Row(legend, 1);
        Row(scope, 2);
        string[] header = ["Method", "RMSE (m)", "MAE (m)", "Bias (m)", "P95 |error| (m)", "|error| > 2 m (%)"];
        for (int c = 0; c < 6; c++) table.ColumnDefinitions.Add(new ColumnDefinition());
        for (int r = 0; r < 3; r++)
        {
            table.RowDefinitions.Add(new RowDefinition { Height = new GridLength(28) });
            for (int c = 0; c < 6; c++)
            {
                var cell = new TextBlock { Text = r == 0 ? header[c] : "", Padding = new Thickness(6, 5, 6, 5), FontWeight = r == 0 ? FontWeights.SemiBold : FontWeights.Normal };
                var border = new Border { BorderBrush = Hex("#d5e2e9"), BorderThickness = new Thickness(0, 0, 1, 1), Child = cell };
                Grid.SetRow(border, r);
                Grid.SetColumn(border, c);
                table.Children.Add(border);
                cells[r, c] = cell;
            }
        }
        Row(table, 3);
        histogram.Margin = new Thickness(0, 8, 0, 0);
        Row(histogram, 4);
        Row(new TextBlock { Text = AccuracyText.Footnote, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) }, 5);
        root.Children.Add(workspace);

        areaButton.Checked += (_, _) => ToggleArea(true);
        areaButton.Unchecked += (_, _) => ToggleArea(false);
        mode.SelectionChanged += (_, _) => SelectDisplay();
        range.SelectionChanged += (_, _) => SelectDisplay();
        threshold.SelectionChanged += (_, _) => { if (grids != null) UpdateMetrics(); };
        presets.SelectionChanged += (_, _) => { if (!updatingPreset) ApplyPreset(presets.SelectedIndex); };
        export.Click += (_, _) => ExportReport();
        reload.Click += (_, _) => _ = StartAsync();
        cancel.Click += (_, _) => RequestCancel();
        return root;
    }

    // ----- loading -------------------------------------------------------------------------
    void SetBusy(bool value)
    {
        busy = value;
        bool ready = !value && grids != null;
        reload.IsEnabled = !value;
        cancel.IsEnabled = value;
        progress.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        mode.IsEnabled = threshold.IsEnabled = presets.IsEnabled = export.IsEnabled = areaButton.IsEnabled = ready;
        range.IsEnabled = ready && mode.SelectedIndex != 2;
        foreach (var item in navigation) item.IsEnabled = ready;
    }

    void RequestCancel()
    {
        cts.Cancel();
        cancel.IsEnabled = false;
        status.Text = "Cancelling preparation...";
    }

    async Task StartAsync()
    {
        if (busy) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        areaButton.IsChecked = false;
        SetBusy(true);
        var report = new Progress<(int Percent, string Message)>(p => { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; });
        string folder = Path.Combine(temporary, (++generation).ToString(CultureInfo.InvariantCulture));
        try
        {
            string sample = await SampleCache.PrepareAsync(assets, options, report, token);
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(sample);
            Directory.CreateDirectory(folder);
            const string message = "Reading heights and preparing shared-scale Byte RGB maps...";
            var percent = new Progress<int>(p => { progress.Value = Math.Clamp(p, 0, 100); status.Text = message; });
            status.Text = message;
            var (loaded, files) = await Task.Run(() =>
            {
                var g = Grids.Read(sample, token);
                return (g, Accuracy.PrepareDisplays(g, folder, percent, token));
            }, token);
            token.ThrowIfCancellationRequested();
            Publish(loaded, files.Original, files.Methods);
            SetBusy(false);
            if (area == null) ApplyPreset(0);
            else { UpdateMetrics(); DrawOverlays(); }
            instructions.Text = AccuracyText.Idle;
            status.Text = AccuracyText.Ready;
            if (closeQueued) _ = Dispatcher.BeginInvoke(Close);
        }
        catch (Exception e)
        {
            if (closed) return;
            SetBusy(false);
            bool cancelled = e is OperationCanceledException || cts.IsCancellationRequested;
            status.Text = cancelled ? "Preparation cancelled; completed comparison retained." : e.Message;
            if (closeQueued) _ = Dispatcher.BeginInvoke(Close);
            else if (!cancelled) MessageBox.Show(this, e.Message, "Unable to load comparison", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    void Publish(Grids loaded, string original, string[][] methods)
    {
        grids = loaded;
        for (int pane = 0; pane < 3; pane++)
        {
            var map = maps[pane];
            map.ClearShapes();
            map.ClearLayers();
            // The empty map adopts the first layer's CRS (EPSG:2056); new layers go to index 0.
            string[] files = pane == 0 ? [original] : methods[pane - 1];
            for (int k = 0; k < files.Length; k++)
            {
                if (!map.AddLayerFile(files[k])) throw new InvalidOperationException($"The display layer could not be added: {files[k]}");
                map.SetLayerName(0, pane == 0 ? "Original DSM" : $"{(pane == 1 ? "Classical" : "AI")} display {k}");
            }
            map.SetTool(GeoKernelViewerTool.Pan);
        }
        Array.Clear(applied);
        FullExtent();
        SelectDisplay();
    }

    // ----- maps ----------------------------------------------------------------------------
    void FullExtent()
    {
        maps[0].ViewExtent = new GeoKernelExtent(FullView.XMin, FullView.YMin, FullView.XMax, FullView.YMax);
        SyncFrom(0);
    }

    void SyncFrom(int side)
    {
        syncing = true;
        try
        {
            applied[side] = maps[side].ViewExtent;
            for (int other = 0; other < 3; other++)
                if (other != side) { maps[other].ViewExtent = applied[side]!.Value; applied[other] = maps[other].ViewExtent; }
        }
        finally { syncing = false; }
    }

    static bool Same(GeoKernelExtent? a, GeoKernelExtent b) =>
        a is GeoKernelExtent x && Math.Abs(x.XMin - b.XMin) < 1e-9 && Math.Abs(x.YMin - b.YMin) < 1e-9 && Math.Abs(x.XMax - b.XMax) < 1e-9 && Math.Abs(x.YMax - b.YMax) < 1e-9;

    void ExtentChanged(int side)
    {
        // The event can arrive synchronously inside SetViewExtent or while the map renders:
        // ignore our own updates and read the new extent after the native call returns.
        if (syncing || closed) return;
        Dispatcher.BeginInvoke(() => { if (!syncing && !closed && grids != null && !Same(applied[side], maps[side].ViewExtent)) SyncFrom(side); });
    }

    void SelectionFinished(GeoKernelExtent e)
    {
        if (areaButton.IsChecked != true) return;
        Dispatcher.BeginInvoke(() =>
        {
            ChooseArea((e.XMin, e.YMin, e.XMax, e.YMax), "User-selected area");
            areaButton.IsChecked = false;
        });
    }

    void MapClick(int side, GeoKernelMapMouseEventArgs e)
    {
        if (e.ButtonOrButtons != 1) return;
        var press = presses[side];
        presses[side] = null;
        if (press is { } p && areaButton.IsChecked != true && Math.Abs(p.X - e.ScreenPoint.X) + Math.Abs(p.Y - e.ScreenPoint.Y) <= 3)
            Inspect(e.WorldPoint.X, e.WorldPoint.Y);
    }

    void UseTool(GeoKernelViewerTool tool)
    {
        areaButton.IsChecked = false;
        foreach (var map in maps) map.SetTool(tool);
    }

    void ToggleArea(bool active)
    {
        foreach (var map in maps) map.SetTool(active ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan);
        instructions.Text = active ? AccuracyText.Selecting : AccuracyText.Idle;
    }

    void SelectDisplay()
    {
        legend.Mode = Math.Max(0, mode.SelectedIndex);
        legend.Limit = Accuracy.DisplayLimits[Math.Max(0, range.SelectedIndex)];
        legend.InvalidateVisual();
        range.IsEnabled = !busy && grids != null && mode.SelectedIndex != 2;
        if (grids == null) return;
        int selected = mode.SelectedIndex == 2 ? 8 : (mode.SelectedIndex == 1 ? 4 : 0) + range.SelectedIndex;
        for (int m = 1; m <= 2; m++)
        {
            for (int k = 0; k < 9; k++) maps[m].SetLayerVisible(8 - k, k == selected); // files 0..8 were prepended in order
            maps[m].RefreshLayers();
            headings[m].Text = AccuracyText.Headings[mode.SelectedIndex][m - 1];
        }
    }

    /// <summary>Owned shapes on all maps: four translucent bands dim the outside of the evaluated
    /// cells, an outline marks them and a point shows the probed cell. GeoKernel.NET 1.5.40 wraps no
    /// overlay point, so the probe is redrawn with the area shapes.</summary>
    void DrawOverlays()
    {
        if (grids == null || area is not { } window) return;
        var (x0, y0, x1, y1) = Accuracy.Extent(grids, window);
        var t = grids.Transform;
        const double margin = 2000;
        double ox0 = t[0] - margin, oy0 = t[3] + grids.Height * t[5] - margin, ox1 = t[0] + grids.Width * t[1] + margin, oy1 = t[3] + margin;
        (double, double, double, double)[] bands = [(ox0, y1, ox1, oy1), (ox0, oy0, ox1, y0), (ox0, y0, x0, y1), (x1, y0, ox1, y1)];
        var dim = new GeoKernelLayerStyle { FillColor = "#ffffff", FillOpacity = 100, LineColor = "#ffffff", LineWidth = 0 };
        GeoKernelPoint[] outline = [new(x0, y0), new(x1, y0), new(x1, y1), new(x0, y1), new(x0, y0)];
        foreach (var map in maps)
        {
            map.ClearShapes();
            foreach (var (a, b, c, d) in bands)
                if (c > a && d > b) map.AddPolygonShape([new(a, b), new(c, b), new(c, d), new(a, d), new(a, b)], dim);
            map.AddPolylineShape(outline, new GeoKernelLayerStyle { LineColor = "#167d8d", LineWidth = 2 });
            if (probePoint is { } p)
                map.AddPointShape(new GeoKernelPoint(p.X, p.Y), new GeoKernelLayerStyle { PointColor = "#dc3672", PointSize = 11, LineColor = "#ffffff", LineWidth = 2 });
            map.RefreshLayers(); // owned shapes are drawn on the next full map render
        }
    }

    // ----- evaluation ----------------------------------------------------------------------
    void ChooseArea((double XMin, double YMin, double XMax, double YMax) extent, string name, int preset = -1)
    {
        if (grids == null) return;
        try { area = Accuracy.Window(grids, extent); }
        catch (ArgumentException e) { status.Text = e.Message; return; }
        areaName = name;
        updatingPreset = true;
        presets.SelectedIndex = preset < 0 ? 4 : preset;
        updatingPreset = false;
        UpdateMetrics();
        DrawOverlays();
        status.Text = "Updated both methods on exactly the same source cells.";
    }

    void ApplyPreset(int index)
    {
        if (grids == null || index < 0) return;
        if (index == 4) { areaButton.IsChecked = true; return; }
        areaButton.IsChecked = false;
        ChooseArea(Accuracy.PresetArea(grids, index), Accuracy.Presets[index], index);
    }

    void UpdateMetrics()
    {
        if (grids == null || area is not { } window) return;
        double limit = ThresholdValue;
        metrics = Accuracy.Measure(grids, window, limit);
        histogram.Metrics = metrics;
        histogram.InvalidateVisual();
        cells[0, 5].Text = $"|error| > {limit:0.##} m (%)";
        for (int m = 0; m < 2; m++)
        {
            var values = AccuracyText.Row(m, metrics[m]);
            for (int c = 0; c < 6; c++) { cells[m + 1, c].Text = values[c]; cells[m + 1, c].Foreground = Hex(Accuracy.MethodColors[m]); }
        }
        winners.Text = AccuracyText.Winners(metrics);
        scope.Text = AccuracyText.Scope(areaName, metrics);
    }

    void Inspect(double x, double y)
    {
        if (grids == null || busy) return;
        var t = grids.Transform;
        double col = (x - t[0]) / .5, row = (t[3] - y) / .5;
        if (!(col >= 0 && row >= 0 && col < grids.Width && row < grids.Height)) { status.Text = "Click inside the source raster."; return; }
        int cx = (int)col, cy = (int)row;
        probePoint = (t[0] + (cx + .5) * .5, t[3] - (cy + .5) * .5);
        probe.Text = AccuracyText.Probe(grids, cx, cy, area);
        DrawOverlays();
    }

    void ExportReport()
    {
        if (grids == null || area is not { } window || metrics == null) return;
        var dialog = new SaveFileDialog { Title = "Export evaluation report", FileName = "accuracy-report.json", Filter = "JSON (*.json)|*.json" };
        if (dialog.ShowDialog(this) != true) return;
        try { Accuracy.ExportReport(dialog.FileName, grids, window, areaName, ThresholdValue, metrics, assets.Sha256); }
        catch (Exception e) { MessageBox.Show(this, e.Message, "Export report", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        status.Text = "Saved selected-area metrics and source provenance.";
    }

    // ----- shutdown ------------------------------------------------------------------------
    protected override void OnClosing(CancelEventArgs e)
    {
        if (busy) { closeQueued = true; RequestCancel(); e.Cancel = true; return; }
        closed = true;
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        foreach (var map in maps) map.Dispose();
        try { Directory.Delete(temporary, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    static FormattedText Text(Visual visual, string text, double size = 12) =>
        new(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Segoe UI"), size, Hex("#476174"), VisualTreeHelper.GetDpi(visual).PixelsPerDip);

    sealed class Legend : FrameworkElement
    {
        public int Mode;
        public double Limit = 2;

        protected override void OnRender(DrawingContext dc)
        {
            double w = ActualWidth;
            dc.DrawRectangle(Brushes.White, null, new Rect(RenderSize));
            if (Mode == 2)
            {
                var relief = Text(this, AccuracyText.LegendRelief);
                dc.DrawText(relief, new Point((w - relief.Width) / 2, (ActualHeight - relief.Height) / 2));
                return;
            }
            var title = Text(this, AccuracyText.LegendTitle(Mode));
            dc.DrawText(title, new Point((w - title.Width) / 2, 3));
            var stops = new GradientStopCollection();
            for (int i = 0; i <= 20; i++)
            {
                double t = i / 20.0, value = Mode == 0 ? (t * 2 - 1) * Limit : t * Limit;
                var c = Accuracy.ErrorColor(value, Mode == 1, Limit);
                stops.Add(new GradientStop(Color.FromRgb((byte)c.R, (byte)c.G, (byte)c.B), t));
            }
            dc.DrawRectangle(new LinearGradientBrush(stops, 0), null, new Rect(70, 25, Math.Max(1, w - 140), 13));
            dc.DrawText(Text(this, Mode == 0 ? $"≤ −{Limit:0.#} m" : "0 m"), new Point(8, 42));
            var right = Text(this, $"≥ {Limit:0.#} m");
            dc.DrawText(right, new Point(w - 8 - right.Width, 42));
            var note = Text(this, AccuracyText.LegendNote);
            dc.DrawText(note, new Point((w - note.Width) / 2, 42));
        }
    }

    sealed class Histogram : FrameworkElement
    {
        public AccuracyMetric[]? Metrics;

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Brushes.White, null, new Rect(RenderSize));
            if (Metrics is not { } metrics) return;
            var title = Text(this, "Absolute error distribution · evaluated area · teal classical / purple AI");
            dc.DrawText(title, new Point((ActualWidth - title.Width) / 2, 3));
            var plot = new Rect(50, 30, Math.Max(1, ActualWidth - 70), Math.Max(1, ActualHeight - 60));
            double group = plot.Width / 6;
            double maximum = Math.Max(10, metrics.SelectMany(m => m.Bins.Select(b => 100.0 * b / m.Count)).Max());
            maximum = Math.Ceiling(maximum / 10) * 10;
            var gridPen = new Pen(Hex("#e2e9ee"), 1);
            for (int tick = 0; tick <= 2; tick++)
            {
                double y = plot.Bottom - tick * .5 * plot.Height;
                dc.DrawLine(gridPen, new Point(plot.Left, y), new Point(plot.Right, y));
                var label = Text(this, $"{tick * .5 * maximum:F0}%");
                dc.DrawText(label, new Point(43 - label.Width, y - 8));
            }
            for (int b = 0; b < 6; b++)
            {
                for (int m = 0; m < 2; m++)
                {
                    double h = 100.0 * metrics[m].Bins[b] / metrics[m].Count / maximum * plot.Height;
                    dc.DrawRectangle(Hex(Accuracy.MethodColors[m]), null, new Rect(plot.Left + b * group + group * (.18 + m * .32), plot.Bottom - h, group * .28, Math.Max(0, h)));
                }
                var label = Text(this, AccuracyText.BinLabels[b]);
                dc.DrawText(label, new Point(plot.Left + b * group + (group - label.Width) / 2, plot.Bottom + 3));
            }
        }
    }
}
