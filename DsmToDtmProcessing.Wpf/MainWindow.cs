using System.Globalization;
using System.IO;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GeoKernel.Examples.DsmToDtm;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf.Controls;
using Microsoft.Win32;

namespace GeoKernel.DsmToDtmProcessing.Wpf;

/// <summary>
/// Live classical DSM-to-DTM processing: run the multiscale morphological filter and TIN on the
/// input DSM, change parameters and inspect the new terrain on three linked native 2D maps
/// (original DSM, generated DTM, diagnostic). The reference is evaluation-only.
/// </summary>
public sealed class MainWindow : Window
{
    const string WindowTitle = "DSM to DTM | Live classical processing — GeoKernel";
    static Brush Hex(string color) => (Brush)new BrushConverter().ConvertFromString(color)!;

    readonly DsmOptions options;
    readonly SampleAssets assets = SampleAssets.ZurichV2();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-processing-").FullName;
    readonly GeoKernelViewerControl[] maps = [new(), new(), new()];
    readonly TextBlock[] headings = new TextBlock[3];
    readonly ComboBox presets = new() { ItemsSource = Terrain.PresetNames, SelectedIndex = 0, Margin = new Thickness(0, 0, 0, 8) };
    readonly ComboBox mode = new() { ItemsSource = TerrainText.DiagnosticNames, SelectedIndex = 0, Margin = new Thickness(0, 0, 0, 10) };
    readonly Spin working = new("Working grid (m)", .25, 20, 2, .5, 2), window = new("Max. window (m)", 4, 200, 64, 4, 1),
        threshold = new("Base threshold (m)", .05, 3, .4, .05, 2), slope = new("Slope allowance", 0, .5, .12, .02, 2),
        relief = new("Relief allowance", 0, 2, .5, .1, 2), retain = new("Keep tolerance (m)", 0, 1, .35, .05, 2);
    readonly Spin[] parameters;
    readonly TextBlock source = Card("No source loaded."), state = Wrap("Load a DSM to begin."), metrics = Card("Reference errors appear after a completed run."),
        probe = Card("Click any map to inspect source heights."), status = new(),
        legend = new() { Text = "Original DSM remains visible. Both relief maps use identical lighting and contrast.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 2) },
        summary = new() { Text = "No processing has run yet.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 2) };
    readonly Button sampleButton = MakeButton("Load Zurich sample"), openButton = MakeButton("Open DSM GeoTIFF..."), referenceButton = MakeButton("Open reference (optional)..."),
        runButton = MakeButton("Run DSM → DTM"), cancelButton = MakeButton("Cancel"), saveButton = MakeButton("Save result GeoTIFFs...");
    readonly List<UIElement> navigation = [];
    readonly ProgressBar progress = new() { Width = 230, Height = 14, Maximum = 100, Visibility = Visibility.Collapsed };
    readonly (double X, double Y)?[] presses = new (double X, double Y)?[3];
    readonly GeoKernelExtent?[] applied = new GeoKernelExtent?[3];
    CancellationTokenSource cts = new();
    TerrainScene? scene;
    TerrainRun? run;
    (double X, double Y)? probePoint;
    GeoKernelViewerTool tool = GeoKernelViewerTool.Pan;
    bool busy, closeQueued, closed, syncing;
    int generation;

    static TextBlock Wrap(string text) => new() { Text = text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) };
    static TextBlock Card(string text)
    {
        var block = Wrap(text);
        block.Background = Brushes.White;
        block.Padding = new Thickness(9);
        return block;
    }
    static Button MakeButton(string text) => new() { Content = text, Padding = new Thickness(7), Margin = new Thickness(0, 0, 0, 8) };

    public MainWindow(DsmOptions options)
    {
        this.options = options;
        parameters = [working, window, threshold, slope, relief, retain];
        Title = WindowTitle;
        Width = 1720;
        Height = 960;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Hex("#f2f6f8");
        Foreground = Hex("#233e50");
        ExampleIcon.Apply(this);
        Content = BuildLayout();
        SetBusy(false);
        ContentRendered += (_, _) => _ = LoadSampleAsync();
    }

    UIElement BuildLayout()
    {
        var root = new DockPanel();
        var toolbar = new ToolBar { Background = Brushes.White, Padding = new Thickness(6) };
        ToolBarTray.SetIsLocked(toolbar, true);
        void Tool(string text, string? image, Action action)
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
        }
        Tool("Pan", null, () => UseTool(GeoKernelViewerTool.Pan));
        Tool("Zoom box", null, () => UseTool(GeoKernelViewerTool.ZoomBox));
        Tool("Zoom in", "ZoomIn.png", () => maps[0].ZoomIn(1.5));
        Tool("Zoom out", "ZoomOut.png", () => maps[0].ZoomOut(1.5));
        Tool("Fit scene", "FullExtent.png", Fit);
        var tray = new ToolBarTray { Background = Brushes.White };
        tray.ToolBars.Add(toolbar);
        DockPanel.SetDock(tray, Dock.Top);
        root.Children.Add(tray);

        var statusBar = new StatusBar();
        statusBar.Items.Add(new StatusBarItem { Content = status });
        statusBar.Items.Add(new StatusBarItem { Content = progress, HorizontalAlignment = HorizontalAlignment.Right });
        DockPanel.SetDock(statusBar, Dock.Bottom);
        root.Children.Add(statusBar);

        var side = new StackPanel { Width = 262, Margin = new Thickness(12) };
        var title = Wrap("DSM → DTM\nRun the conversion");
        title.FontSize = 21;
        title.FontWeight = FontWeights.SemiBold;
        title.Foreground = Hex("#145e72");
        foreach (var element in new UIElement[] {
            title, Wrap("Live multiscale filtering + TIN.\nChange parameters, then Run."), sampleButton, openButton, referenceButton, source, presets,
            working, window, threshold, slope, relief, retain, runButton, cancelButton, state, Wrap("Right map"), mode, metrics, saveButton, probe,
            Wrap("Sample: Federal Office of Topography swisstopo.\nReference is evaluation-only. Heights are metres; no vertical datum conversion.") })
            side.Children.Add(element);
        var scroll = new ScrollViewer { Content = side, Width = 300, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        DockPanel.SetDock(scroll, Dock.Left);
        root.Children.Add(scroll);

        var workspace = new Grid { Margin = new Thickness(0, 12, 12, 6) };
        foreach (var height in new[] { new GridLength(1, GridUnitType.Star), GridLength.Auto, GridLength.Auto, GridLength.Auto })
            workspace.RowDefinitions.Add(new RowDefinition { Height = height });
        void Row(UIElement element, int row) { Grid.SetRow(element, row); workspace.Children.Add(element); }
        var views = new Grid();
        for (int i = 0; i < 3; i++) views.ColumnDefinitions.Add(new ColumnDefinition());
        string[] titles = ["ORIGINAL DSM · input", "PROCESSED DTM · Run to generate", "REMOVED HEIGHT · Run to generate"];
        for (int i = 0; i < 3; i++)
        {
            int pane = i;
            var column = new DockPanel { Margin = new Thickness(i == 0 ? 0 : 4, 0, i == 2 ? 0 : 4, 0), MinWidth = 240, MinHeight = 260 };
            headings[i] = new TextBlock { Text = titles[i], Background = Hex("#e4eef2"), Foreground = Hex("#125a70"), Padding = new Thickness(9), FontWeight = FontWeights.SemiBold };
            DockPanel.SetDock(headings[i], Dock.Top);
            column.Children.Add(headings[i]);
            column.Children.Add(maps[i]);
            maps[i].VisibleExtentChanged += (_, _) => ExtentChanged(pane);
            maps[i].MapMouseDown += (_, e) => { if (e.ButtonOrButtons == 1) presses[pane] = (e.ScreenPoint.X, e.ScreenPoint.Y); };
            maps[i].MapMouseUp += (_, e) => MapClick(pane, e);
            Grid.SetColumn(column, i);
            views.Children.Add(column);
        }
        Row(views, 0);
        Row(legend, 1);
        Row(summary, 2);
        Row(new TextBlock { Text = TerrainText.EdgeNote, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) }, 3);
        root.Children.Add(workspace);

        foreach (var spin in parameters) spin.ValueChanged += () => { if (run != null) state.Text = TerrainText.ControlsChanged; };
        presets.SelectionChanged += (_, _) => ApplyPreset(presets.SelectedIndex);
        mode.SelectionChanged += (_, _) => ShowDiagnostic();
        sampleButton.Click += (_, _) => _ = LoadSampleAsync();
        openButton.Click += (_, _) => OpenDsm();
        referenceButton.Click += (_, _) => OpenReference();
        runButton.Click += (_, _) => Process();
        cancelButton.Click += (_, _) => RequestCancel();
        saveButton.Click += (_, _) => SaveResult();
        return root;
    }

    // ----- state ---------------------------------------------------------------------------
    void SetBusy(bool value)
    {
        busy = value;
        foreach (UIElement e in (UIElement[])[sampleButton, openButton, presets, .. parameters]) e.IsEnabled = !value;
        foreach (var item in navigation) item.IsEnabled = !value && scene != null;
        runButton.IsEnabled = referenceButton.IsEnabled = !value && scene != null;
        saveButton.IsEnabled = mode.IsEnabled = !value && run != null;
        cancelButton.IsEnabled = value;
        progress.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    void RequestCancel()
    {
        cts.Cancel();
        cancelButton.IsEnabled = false;
        status.Text = "Cancelling; completed result will be retained...";
    }

    void ApplyPreset(int index)
    {
        var p = Terrain.Presets[Math.Max(0, index)];
        double[] values = [p.Working, p.Window, p.Threshold, p.Slope, p.Terrain, p.Retain];
        for (int i = 0; i < parameters.Length; i++) parameters[i].Value = values[i];
    }

    string NextFolder() => Directory.CreateDirectory(Path.Combine(temporary, (++generation).ToString(CultureInfo.InvariantCulture))).FullName;

    /// <summary>Run a worker job; on success apply its result, keeping the last completed state on failure.</summary>
    async Task RunJob<T>(Func<CancellationToken, Action<int, string>, T> job, Action<T> apply)
    {
        if (busy) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        SetBusy(true);
        IProgress<(int Percent, string Message)> sink = new Progress<(int Percent, string Message)>(p =>
        {
            if (!closed) { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; }
        });
        try
        {
            T result = await Task.Run(() => job(token, (p, m) => sink.Report((p, m))), token);
            token.ThrowIfCancellationRequested();
            if (closed) return;
            apply(result);
            SetBusy(false);
            if (closeQueued) _ = Dispatcher.BeginInvoke(Close);
        }
        catch (Exception e)
        {
            if (closed) return;
            SetBusy(false);
            bool cancelled = e is OperationCanceledException || token.IsCancellationRequested;
            status.Text = cancelled ? "Cancelled. Last completed source/result retained." : e.Message;
            if (closeQueued) _ = Dispatcher.BeginInvoke(Close);
            else if (!cancelled) MessageBox.Show(this, e.Message, "Operation failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ----- jobs ----------------------------------------------------------------------------
    async Task LoadSampleAsync()
    {
        if (busy) return;
        string folder = NextFolder();
        await RunJob((token, report) =>
        {
            report(0, "Checking cached sample files...");
            string sample = SampleCache.PrepareAsync(assets, options, new Progress<(int Percent, string Message)>(p => report(p.Percent, p.Message)), token).GetAwaiter().GetResult();
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(sample);
            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(sample, "source-manifest.json")))!.AsObject();
            return TerrainJobs.Load(Path.Combine(sample, "dsm.tif"), Path.Combine(sample, "dtm_reference.tif"), manifest, folder, token, report);
        }, loaded => Publish(loaded, null, "Source ready. Choose parameters and Run."));
    }

    void OpenDsm()
    {
        if (busy) return;
        var dialog = new OpenFileDialog { Title = "Open DSM: metre heights in projected metre CRS", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff" };
        if (dialog.ShowDialog(this) != true) return;
        string path = dialog.FileName, folder = NextFolder();
        _ = RunJob((token, report) => TerrainJobs.Load(path, null, new JsonObject(), folder, token, report),
            loaded => Publish(loaded, null, "Source ready. Choose parameters and Run."));
    }

    void OpenReference()
    {
        if (busy || scene is not { } current) return;
        var dialog = new OpenFileDialog { Title = "Open aligned reference (evaluation only)", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff" };
        if (dialog.ShowDialog(this) != true) return;
        string path = dialog.FileName, folder = NextFolder();
        var completed = run;
        _ = RunJob((token, report) => TerrainJobs.AttachReference(current, completed, path, folder, token, report),
            result => Publish(result.Scene, result.Run, "Reference attached; processing heights unchanged."));
    }

    void Process()
    {
        if (busy || scene is not { } current) return;
        var p = new TerrainParameters(working.Value, window.Value, threshold.Value, slope.Value, relief.Value, retain.Value);
        string folder = NextFolder();
        _ = RunJob((token, report) => TerrainJobs.Process(current, p, folder, token, report),
            result => Publish(current, result, "Processing complete. Change parameters to run again, or save the result."));
    }

    void SaveResult()
    {
        if (busy || scene is not { } current || run is not { } completed) return;
        var dialog = new SaveFileDialog { Title = "Save DTM, removed heights, QA and JSON", FileName = "terrain-result.tif", Filter = "GeoTIFF (*.tif)|*.tif", OverwritePrompt = true };
        if (dialog.ShowDialog(this) != true) return;
        string[] destinations = TerrainJobs.Destinations(dialog.FileName);
        if (TerrainJobs.UsesSource(current, destinations))
        {
            MessageBox.Show(this, "Choose output names different from the source and reference.", "Cannot export", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (destinations.Skip(1).Any(File.Exists) && MessageBox.Show(this, "Removed-height, QA or JSON companion files already exist. Replace the result set?",
                "Replace existing result companions?", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes) return;
        _ = RunJob((token, report) => { TerrainJobs.Save(completed, destinations, token, report); return true; },
            _ => { state.Text = status.Text = "Saved DTM, removed heights, QA flags and provenance JSON."; });
    }

    void Publish(TerrainScene loaded, TerrainRun? result, string message)
    {
        bool changed = scene == null || !ReferenceEquals(loaded.Dsm, scene.Dsm);
        scene = loaded;
        run = result;
        if (changed)
        {
            Replace(0, loaded.Original);
            for (int pane = 1; pane < 3; pane++) { maps[pane].ClearLayers(); maps[pane].RefreshLayers(); }
            probePoint = null;
            DrawProbe();
            probe.Text = "Click a map to inspect source heights.";
            Array.Clear(applied);
            _ = Dispatcher.BeginInvoke(Fit);
        }
        if (run != null)
        {
            Replace(1, run.Display);
            ShowDiagnostic();
            summary.Text = TerrainText.Summary(run.Terrain);
            metrics.Text = TerrainText.Metrics(run.All, run.Interior, loaded.Reference != null);
        }
        else
        {
            headings[1].Text = "PROCESSED DTM · Run to generate";
            headings[2].Text = "REMOVED HEIGHT · Run to generate";
            metrics.Text = "No processing has run yet.";
            summary.Text = "Source loaded. Run to generate a new terrain estimate.";
        }
        source.Text = TerrainText.Source(loaded.Dsm, loaded.Reference != null);
        state.Text = status.Text = message;
    }

    // ----- maps ----------------------------------------------------------------------------
    /// <summary>Swap a map's raster but keep its view; the empty map adopts the raster's CRS.</summary>
    void Replace(int pane, string path)
    {
        var map = maps[pane];
        GeoKernelExtent? extent = map.LayerCount > 0 ? map.ViewExtent : null;
        syncing = true;
        try
        {
            map.ClearLayers();
            if (!map.AddLayerFile(path)) throw new InvalidOperationException($"The display layer could not be added: {path}");
            map.SetTool(tool);
            if (extent is { } e) map.ViewExtent = e;
            map.RefreshLayers();
        }
        finally { syncing = false; }
    }

    void Fit()
    {
        if (scene == null) return;
        var (x0, y0, x1, y1) = scene.Dsm.Extent;
        double pad = scene.Dsm.Pixel * 10;
        syncing = true;
        try { maps[0].ViewExtent = new GeoKernelExtent(x0 - pad, y0 - pad, x1 + pad, y1 + pad); }
        finally { syncing = false; }
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
        Dispatcher.BeginInvoke(() => { if (!syncing && !closed && scene != null && !Same(applied[side], maps[side].ViewExtent)) SyncFrom(side); });
    }

    void MapClick(int side, GeoKernelMapMouseEventArgs e)
    {
        if (e.ButtonOrButtons != 1) return;
        var press = presses[side];
        presses[side] = null;
        if (press is { } p && tool == GeoKernelViewerTool.Pan && Math.Abs(p.X - e.ScreenPoint.X) + Math.Abs(p.Y - e.ScreenPoint.Y) <= 3)
        {
            double x = e.WorldPoint.X, y = e.WorldPoint.Y;
            Dispatcher.BeginInvoke(() => Inspect(x, y));
        }
    }

    void UseTool(GeoKernelViewerTool value)
    {
        tool = value;
        foreach (var map in maps) map.SetTool(value);
    }

    void ShowDiagnostic()
    {
        if (run == null || scene == null) return;
        int selected = Math.Max(0, mode.SelectedIndex);
        if (selected == 2 && scene.Reference == null)
        {
            mode.SelectedIndex = 0; // re-enters through SelectionChanged
            status.Text = "Attach an aligned reference to show errors.";
            return;
        }
        Replace(2, run.Diagnostics[selected]);
        headings[1].Text = "PROCESSED DTM · live native result";
        headings[2].Text = TerrainText.DiagnosticTitles[selected];
        legend.Text = TerrainText.Legends[selected];
        DrawProbe();
    }

    /// <summary>GeoKernel.NET 1.5.40 wraps no overlay point, so the probe is an owned point shape.</summary>
    void DrawProbe()
    {
        foreach (var map in maps)
        {
            map.ClearShapes();
            if (probePoint is { } p)
                map.AddPointShape(new GeoKernelPoint(p.X, p.Y), new GeoKernelLayerStyle { PointColor = "#d73573", PointSize = 11, LineColor = "#ffffff", LineWidth = 2 });
            map.RefreshLayers(); // owned shapes are drawn on the next full map render
        }
    }

    void Inspect(double x, double y)
    {
        if (scene == null || busy) return;
        if (TerrainText.Probe(scene.Dsm, run?.Terrain, scene.Reference, x, y) is not { } hit) return;
        probe.Text = hit.Text;
        probePoint = (hit.X, hit.Y);
        DrawProbe();
    }

    // ----- shutdown ------------------------------------------------------------------------
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
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

    /// <summary>A labelled numeric field with step buttons, clamped to its range.</summary>
    sealed class Spin : DockPanel
    {
        readonly TextBox box = new() { Width = 70, VerticalContentAlignment = VerticalAlignment.Center };
        readonly double minimum, maximum, step;
        readonly int decimals;
        double value;
        public event Action? ValueChanged;

        public Spin(string label, double minimum, double maximum, double initial, double step, int decimals)
        {
            (this.minimum, this.maximum, this.step, this.decimals, value) = (minimum, maximum, step, decimals, initial);
            Margin = new Thickness(0, 0, 0, 6);
            System.Windows.Automation.AutomationProperties.SetName(box, label);
            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            RepeatButton Step(string text, double sign)
            {
                var b = new RepeatButton { Content = text, Width = 22, Margin = new Thickness(2, 0, 0, 0) };
                System.Windows.Automation.AutomationProperties.SetName(b, $"{label} {(sign > 0 ? "up" : "down")}");
                b.Click += (_, _) => Value = Math.Round(value + sign * this.step, 6);
                return b;
            }
            buttons.Children.Add(Step("−", -1));
            buttons.Children.Add(Step("+", 1));
            SetDock(buttons, Dock.Right);
            Children.Add(buttons);
            var name = new TextBlock { Text = label, Width = 120, VerticalAlignment = VerticalAlignment.Center };
            SetDock(name, Dock.Left);
            Children.Add(name);
            Children.Add(box);
            box.LostFocus += (_, _) => Commit();
            box.KeyDown += (_, e) => { if (e.Key == Key.Enter) Commit(); };
            Show();
        }

        void Commit()
        {
            if (double.TryParse(box.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed)) Value = parsed;
            else Show();
        }

        void Show() => box.Text = value.ToString("F" + decimals, CultureInfo.InvariantCulture);

        public double Value
        {
            get => value;
            set
            {
                double clamped = Math.Clamp(value, minimum, maximum);
                bool changed = clamped != this.value;
                this.value = clamped;
                Show();
                if (changed) ValueChanged?.Invoke();
            }
        }
    }
}
