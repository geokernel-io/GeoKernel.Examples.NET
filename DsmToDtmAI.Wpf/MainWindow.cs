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

namespace GeoKernel.DsmToDtmAI.Wpf;

/// <summary>
/// Live AI versus classical: run the experimental pix_grad model through GeoKernel.AI and the native
/// classical filter independently on the same DSM, and compare the generated heights on three linked
/// native 2D maps (original DSM, classical, AI). The reference is evaluation-only.
/// </summary>
public sealed class MainWindow : Window
{
    const string WindowTitle = "DSM to DTM | Live AI versus classical — GeoKernel";
    static Brush Hex(string color) => (Brush)new BrushConverter().ConvertFromString(color)!;

    readonly AiOptions options;
    readonly SampleAssets sampleAssets = SampleAssets.ZurichV2();
    readonly JsonObject modelAssets = SampleAssets.Resource("ai-model-assets.json"), terrainAssets = Ai.TerrainAssets();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-ai-").FullName;
    readonly GeoKernelViewerControl[] maps = [new(), new(), new()];
    readonly TextBlock[] headings = new TextBlock[3];
    readonly TextBlock[,] cells = new TextBlock[2, 5];
    readonly Spin overlap = new("Tile overlap (px)", 0, 128, 64, 32), threads = new("CPU threads", 1, 8, 4, 1);
    readonly ComboBox preset = new() { ItemsSource = Terrain.PresetNames, SelectedIndex = 0, Margin = new Thickness(0, 0, 0, 8) },
        mode = new() { ItemsSource = AiText.Modes, SelectedIndex = 0, Margin = new Thickness(0, 0, 0, 6) },
        scope = new() { ItemsSource = AiText.Scopes, SelectedIndex = 0, Margin = new Thickness(0, 0, 0, 6) };
    readonly TextBlock source = Card("Preparing source data..."), modelInfo = Card("Model not prepared."), state = Wrap("Load source and model, then Run."),
        probe = Card("Click a source pixel to inspect both estimates."), status = new(),
        legend = new() { Text = "Same light and contrast for both methods. No conversion has run yet.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 4) },
        summary = new() { Text = "No prepared terrain result is loaded.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 2) };
    readonly Button sampleButton = MakeButton("Load full Zurich 1 km tile"), openButton = MakeButton("Open DSM GeoTIFF..."),
        referenceButton = MakeButton("Open reference (optional)..."), modelButton = MakeButton("Download / verify model"), zipButton = MakeButton("Use model ZIP..."),
        runButton = MakeButton("Run AI + classical"), cancelButton = MakeButton("Cancel"), saveButton = MakeButton("Save comparison folder...");
    readonly List<UIElement> navigation = [];
    readonly ProgressBar progress = new() { Width = 230, Height = 14, Maximum = 100, Visibility = Visibility.Collapsed };
    readonly (double X, double Y)?[] presses = new (double X, double Y)?[3];
    readonly GeoKernelExtent?[] applied = new GeoKernelExtent?[3];
    CancellationTokenSource cts = new();
    TerrainScene? scene;
    AiComparison? comparison;
    string? model;
    (double X, double Y)? probePoint;
    GeoKernelViewerTool tool = GeoKernelViewerTool.Pan;
    bool busy, closeQueued, closed, syncing, setupModel;
    int generation;

    static TextBlock Wrap(string text) => new() { Text = text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 9) };
    static TextBlock Card(string text)
    {
        var block = Wrap(text);
        block.Background = Brushes.White;
        block.Padding = new Thickness(9);
        return block;
    }
    static Button MakeButton(string text) => new() { Content = text, Padding = new Thickness(7), Margin = new Thickness(0, 0, 0, 7) };

    public MainWindow(AiOptions options)
    {
        this.options = options;
        Title = WindowTitle;
        Width = 1760;
        Height = 980;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Hex("#f2f6f8");
        Foreground = Hex("#233e50");
        ExampleIcon.Apply(this);
        Content = BuildLayout();
        SetBusy(false);
        ContentRendered += (_, _) => LoadSample();
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
        Tool("Fit comparison", "FullExtent.png", Fit);
        var tray = new ToolBarTray { Background = Brushes.White };
        tray.ToolBars.Add(toolbar);
        DockPanel.SetDock(tray, Dock.Top);
        root.Children.Add(tray);

        var statusBar = new StatusBar();
        statusBar.Items.Add(new StatusBarItem { Content = status });
        statusBar.Items.Add(new StatusBarItem { Content = progress, HorizontalAlignment = HorizontalAlignment.Right });
        DockPanel.SetDock(statusBar, Dock.Bottom);
        root.Children.Add(statusBar);

        var side = new StackPanel { Width = 272, Margin = new Thickness(12) };
        var title = Wrap("AI / Classical\nRun both methods");
        title.FontSize = 21;
        title.FontWeight = FontWeights.SemiBold;
        title.Foreground = Hex("#145e72");
        var presetRow = new DockPanel { Margin = new Thickness(0, 0, 0, 2) };
        var presetLabel = new TextBlock { Text = "Classical preset", Width = 120, VerticalAlignment = VerticalAlignment.Center };
        DockPanel.SetDock(presetLabel, Dock.Left);
        presetRow.Children.Add(presetLabel);
        presetRow.Children.Add(preset);
        System.Windows.Automation.AutomationProperties.SetName(preset, "Classical preset");
        System.Windows.Automation.AutomationProperties.SetName(mode, "Compare maps");
        System.Windows.Automation.AutomationProperties.SetName(scope, "Evaluation scope");
        foreach (var element in new UIElement[] {
            title, Wrap("One DSM · two independent estimates.\npix_grad · experimental DSM-only model."), sampleButton, openButton, referenceButton, source,
            modelButton, zipButton, modelInfo, overlap, threads, presetRow, runButton, cancelButton, state, Wrap("Compare maps"), mode, scope, Wrap(AiText.ScopeNote),
            saveButton, probe, Wrap(AiText.Attribution) })
            side.Children.Add(element);
        var scroll = new ScrollViewer { Content = side, Width = 310, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        DockPanel.SetDock(scroll, Dock.Left);
        root.Children.Add(scroll);

        var workspace = new Grid { Margin = new Thickness(0, 12, 12, 6) };
        foreach (var height in new[] { new GridLength(1, GridUnitType.Star), GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto })
            workspace.RowDefinitions.Add(new RowDefinition { Height = height });
        void Row(UIElement element, int row) { Grid.SetRow(element, row); workspace.Children.Add(element); }
        var views = new Grid();
        for (int i = 0; i < 3; i++) views.ColumnDefinitions.Add(new ColumnDefinition());
        string[] titles = ["ORIGINAL DSM · shared input", "CLASSICAL DTM · Run to generate", "AI DTM · Run to generate"];
        for (int i = 0; i < 3; i++)
        {
            int pane = i;
            var column = new DockPanel { Margin = new Thickness(i == 0 ? 0 : 4, 0, i == 2 ? 0 : 4, 0), MinWidth = 250, MinHeight = 270 };
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
        Row(ScoreTable(), 2);
        Row(summary, 3);
        Row(new TextBlock { Text = AiText.Footer, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) }, 4);
        root.Children.Add(workspace);

        void Changed() { if (comparison != null) state.Text = AiText.ControlsChanged; }
        overlap.ValueChanged += Changed;
        threads.ValueChanged += Changed;
        preset.SelectionChanged += (_, _) => Changed();
        mode.SelectionChanged += (_, _) => Display();
        scope.SelectionChanged += (_, _) => { UpdateScores(); Fit(); };
        sampleButton.Click += (_, _) => LoadSample();
        openButton.Click += (_, _) => OpenDsm();
        referenceButton.Click += (_, _) => OpenReference();
        modelButton.Click += (_, _) => PrepareModel(null);
        zipButton.Click += (_, _) => LocalModel();
        runButton.Click += (_, _) => Process();
        cancelButton.Click += (_, _) => RequestCancel();
        saveButton.Click += (_, _) => ExportResults();
        return root;
    }

    /// <summary>Method · valid pixels · RMSE · MAE · bias for both estimates.</summary>
    UIElement ScoreTable()
    {
        var grid = new Grid { Background = Brushes.White };
        for (int c = 0; c < 5; c++) grid.ColumnDefinitions.Add(new ColumnDefinition());
        for (int r = 0; r < 3; r++) grid.RowDefinitions.Add(new RowDefinition());
        for (int c = 0; c < 5; c++)
        {
            var header = new TextBlock { Text = AiText.Columns[c], FontWeight = FontWeights.SemiBold, Padding = new Thickness(8, 5, 8, 5), Background = Hex("#eef3f6") };
            Grid.SetColumn(header, c);
            grid.Children.Add(header);
            for (int r = 0; r < 2; r++)
            {
                var cell = cells[r, c] = new TextBlock { Padding = new Thickness(8, 5, 8, 5), Foreground = Hex(r == 1 ? "#8857bd" : "#087e8b") };
                Grid.SetColumn(cell, c);
                Grid.SetRow(cell, r + 1);
                grid.Children.Add(cell);
            }
        }
        return new Border { Child = grid, BorderBrush = Hex("#c9d8e0"), BorderThickness = new Thickness(1) };
    }

    // ----- state ---------------------------------------------------------------------------
    void SetBusy(bool value)
    {
        busy = value;
        foreach (UIElement e in (UIElement[])[sampleButton, openButton, modelButton, zipButton, overlap, threads, preset, scope]) e.IsEnabled = !value;
        foreach (var item in navigation) item.IsEnabled = !value && scene != null;
        runButton.IsEnabled = !value && scene != null && model != null;
        referenceButton.IsEnabled = !value && scene != null;
        saveButton.IsEnabled = mode.IsEnabled = !value && comparison != null;
        cancelButton.IsEnabled = value;
        progress.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    void RequestCancel()
    {
        cts.Cancel();
        cancelButton.IsEnabled = false;
        status.Text = "Cancelling; completed comparison retained...";
    }

    string NextFolder() => Directory.CreateDirectory(Path.Combine(temporary, (++generation).ToString(CultureInfo.InvariantCulture))).FullName;

    /// <summary>Run a worker job; on success apply its result, keeping the last completed state on failure.</summary>
    async void RunJob<T>(Func<CancellationToken, Action<int, string>, T> job, Action<T> apply)
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
            else if (setupModel && scene != null && model == null) { setupModel = false; _ = Dispatcher.BeginInvoke(() => PrepareModel(null)); }
        }
        catch (Exception e)
        {
            if (closed) return;
            SetBusy(false);
            bool cancelled = e is OperationCanceledException || token.IsCancellationRequested;
            state.Text = status.Text = cancelled ? "Cancelled; completed comparison retained." : e.Message;
            if (closeQueued) _ = Dispatcher.BeginInvoke(Close);
            else if (!cancelled) MessageBox.Show(this, e.Message, "Unable to complete operation", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ----- jobs ----------------------------------------------------------------------------
    void PrepareModel(string? archive)
    {
        if (busy) return;
        RunJob((token, report) => Ai.PrepareModel(options, archive, token, report), folder =>
        {
            model = folder;
            modelInfo.Text = AiText.ModelReady;
            state.Text = status.Text = scene != null ? "Source and model ready. Press Run AI + classical." : "Model ready; load a DSM.";
        });
    }

    void LocalModel()
    {
        if (busy) return;
        var dialog = new OpenFileDialog { Title = "Choose the pinned pix_grad model ZIP", Filter = "ZIP (*.zip)|*.zip" };
        if (dialog.ShowDialog(this) == true) PrepareModel(dialog.FileName);
    }

    void LoadSample()
    {
        if (busy) return;
        string folder = NextFolder();
        setupModel = model == null;
        RunJob((token, report) =>
        {
            string support = SampleCache.PrepareAsync(sampleAssets, options.Sample, new Progress<(int Percent, string Message)>(p => report(p.Percent, "Projection/sample support: " + p.Message)), token)
                .GetAwaiter().GetResult();
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(support);
            string terrain = Ai.PrepareTerrain(terrainAssets, options.Sample.DataRoot, token, report);
            var manifest = (JsonObject)terrainAssets.DeepClone();
            manifest["standard_full_tile"] = true;
            var loaded = TerrainJobs.Load(Path.Combine(terrain, "dsm.tif"), Path.Combine(terrain, "reference.tif"), manifest, folder, token, report);
            var (x0, y0, x1, y1) = loaded.Dsm.Extent;
            if (loaded.Dsm.Width != 2000 || loaded.Dsm.Height != 2000 || loaded.Dsm.Pixel != .5 || (x0, y0, x1, y1) != (2677000, 1251000, 2678000, 1252000))
                throw new InvalidDataException("Unexpected full Zurich grid.");
            return loaded;
        }, loaded => Publish(loaded, null, "Source ready. Prepare model and Run."));
    }

    void OpenDsm()
    {
        if (busy) return;
        var dialog = new OpenFileDialog { Title = "Open metre-height DSM", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff" };
        if (dialog.ShowDialog(this) != true) return;
        string path = dialog.FileName, folder = NextFolder();
        RunJob((token, report) => TerrainJobs.Load(path, null, new JsonObject(), folder, token, report),
            loaded => Publish(loaded, null, "Source ready. Prepare model and Run."));
    }

    void OpenReference()
    {
        if (busy || scene is not { } current) return;
        var dialog = new OpenFileDialog { Title = "Open aligned evaluation-only reference", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff" };
        if (dialog.ShowDialog(this) != true) return;
        string path = dialog.FileName, folder = NextFolder();
        var completed = comparison;
        RunJob((token, report) =>
        {
            report(5, "Reading evaluation-only reference...");
            var reference = Terrain.Read(path, token);
            Terrain.ValidateReference(current.Dsm, reference);
            var next = current with { Reference = reference };
            var updated = completed == null ? null : Ai.AttachReference(next, completed, folder, modelAssets, token, report);
            token.ThrowIfCancellationRequested();
            return (next, updated);
        }, result => Publish(result.next, result.updated, "Reference updated. Both generated height arrays are unchanged."));
    }

    void Process()
    {
        if (busy || scene is not { } current || model is not { } package) return;
        var parameters = Ai.EffectiveParameters(current.Dsm, Terrain.Presets[Math.Max(0, preset.SelectedIndex)]);
        int tileOverlap = (int)overlap.Value, cpu = (int)threads.Value;
        string folder = NextFolder();
        RunJob((token, report) => Ai.Compare(current, package, parameters, tileOverlap, cpu, folder, modelAssets, token, report),
            result => Publish(current, result, "Both methods completed from the same full DSM."));
    }

    void ExportResults()
    {
        if (busy || comparison is not { } completed) return;
        var dialog = new OpenFolderDialog { Title = "Choose parent folder for a new result bundle" };
        if (dialog.ShowDialog(this) != true) return;
        string parent = dialog.FolderName;
        RunJob((token, report) => Ai.Export(completed, parent, token, report), target => { state.Text = status.Text = "Saved complete comparison bundle: " + target; });
    }

    void Publish(TerrainScene loaded, AiComparison? result, string message)
    {
        bool changed = scene == null || !ReferenceEquals(loaded.Dsm, scene.Dsm);
        scene = loaded;
        comparison = result;
        if (changed)
        {
            Replace(0, loaded.Original);
            for (int pane = 1; pane < 3; pane++) { maps[pane].ClearLayers(); maps[pane].RefreshLayers(); }
            probePoint = null;
            probe.Text = "Click a source pixel to inspect both estimates.";
            Array.Clear(applied);
            _ = Dispatcher.BeginInvoke(Fit);
        }
        source.Text = AiText.Source(loaded.Dsm, loaded.Reference != null);
        if (comparison != null)
        {
            Display();
            UpdateScores();
        }
        else
        {
            foreach (var cell in cells) cell.Text = "";
            summary.Text = "No terrain conversion has run for this input.";
            headings[1].Text = "CLASSICAL DTM · Run to generate";
            headings[2].Text = "AI DTM · Run to generate";
        }
        state.Text = status.Text = message;
    }

    // ----- maps ----------------------------------------------------------------------------
    /// <summary>Swap a map's raster but keep its view; a map's first layer joins the linked view.</summary>
    void Replace(int pane, string path)
    {
        var map = maps[pane];
        GeoKernelExtent? extent = map.LayerCount > 0 ? map.ViewExtent : pane > 0 && maps[0].LayerCount > 0 ? maps[0].ViewExtent : null;
        syncing = true;
        try
        {
            map.ClearLayers();
            if (!map.AddLayerFile(path)) throw new InvalidOperationException($"The display layer could not be added: {path}");
            map.SetTool(tool);
            if (extent is { } e) { map.ViewExtent = e; applied[pane] = map.ViewExtent; }
            map.RefreshLayers();
        }
        finally { syncing = false; }
    }

    void Fit()
    {
        if (scene == null) return;
        bool crop = scope.SelectedIndex == 1;
        var (x0, y0, x1, y1) = crop ? Ai.CropExtent(scene) : scene.Dsm.Extent;
        double dx = (x1 - x0) * .025, dy = (y1 - y0) * .025;
        DrawShapes();
        syncing = true;
        try { maps[0].ViewExtent = new GeoKernelExtent(x0 - dx, y0 - dy, x1 + dx, y1 + dy); }
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
                if (other != side && maps[other].LayerCount > 0) { maps[other].ViewExtent = applied[side]!.Value; applied[other] = maps[other].ViewExtent; }
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
        Dispatcher.BeginInvoke(() =>
        {
            if (!syncing && !closed && scene != null && maps[side].LayerCount > 0 && !Same(applied[side], maps[side].ViewExtent)) SyncFrom(side);
        });
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

    void Display()
    {
        if (comparison == null || scene == null) return;
        int selected = Math.Max(0, mode.SelectedIndex);
        if (selected == 2 && scene.Reference == null)
        {
            mode.SelectedIndex = 0; // re-enters through SelectionChanged
            status.Text = "Attach an aligned reference to compare errors.";
            return;
        }
        for (int method = 0; method < 2; method++) Replace(method + 1, comparison.Maps[method][selected]);
        headings[1].Text = "CLASSICAL " + AiText.Suffix[selected];
        headings[2].Text = "AI " + AiText.Suffix[selected];
        legend.Text = AiText.Legends[selected];
        DrawShapes();
    }

    void UpdateScores()
    {
        if (comparison == null || scene == null) return;
        bool crop = scope.SelectedIndex == 1;
        var values = crop ? comparison.Crop : comparison.Full;
        for (int method = 0; method < 2; method++)
        {
            string[] row = AiText.Row(method, values[method]);
            for (int column = 0; column < row.Length; column++) cells[method, column].Text = row[column];
        }
        summary.Text = AiText.Summary(comparison, scene, crop);
    }

    /// <summary>
    /// GeoKernel.NET 1.5.40 wraps no overlay point, so the probe is an owned point shape; owned shapes also dim
    /// everything outside the shared comparison crop (four translucent bands) and outline it.
    /// </summary>
    void DrawShapes()
    {
        if (scene == null) return;
        var (x0, y0, x1, y1) = Ai.CropExtent(scene);
        var (ox0, oy0, ox1, oy1) = scene.Dsm.Extent;
        double margin = Math.Max(ox1 - ox0, oy1 - oy0);
        (ox0, oy0, ox1, oy1) = (ox0 - margin, oy0 - margin, ox1 + margin, oy1 + margin);
        (double, double, double, double)[] bands = [(ox0, y1, ox1, oy1), (ox0, oy0, ox1, y0), (ox0, y0, x0, y1), (x1, y0, ox1, y1)];
        var dim = new GeoKernelLayerStyle { FillColor = "#ffffff", FillOpacity = 155, LineWidth = 0, LineColor = "#ffffff" };
        foreach (var map in maps)
        {
            map.ClearShapes();
            if (scope.SelectedIndex == 1)
            {
                foreach (var (a, b, c, d) in bands)
                    map.AddPolygonShape([new(a, b), new(c, b), new(c, d), new(a, d), new(a, b)], dim);
                map.AddPolylineShape([new(x0, y0), new(x1, y0), new(x1, y1), new(x0, y1), new(x0, y0)], new GeoKernelLayerStyle { LineColor = "#098b97", LineWidth = 2 });
            }
            if (probePoint is { } p)
                map.AddPointShape(new GeoKernelPoint(p.X, p.Y), new GeoKernelLayerStyle { PointColor = "#d73573", PointSize = 11, LineColor = "#ffffff", LineWidth = 2 });
            map.RefreshLayers(); // owned shapes are drawn on the next full map render
        }
    }

    void Inspect(double x, double y)
    {
        if (scene == null || busy) return;
        if (AiText.Probe(scene, comparison, x, y) is not { } hit) return;
        probe.Text = hit.Text;
        probePoint = (hit.X, hit.Y);
        DrawShapes();
    }

    // ----- shutdown ------------------------------------------------------------------------
    protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
    {
        // The SDK inference call has no cancellation hook; a queued close completes after the current AI pass.
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

    /// <summary>A labelled whole-number field with step buttons, clamped to its range.</summary>
    sealed class Spin : DockPanel
    {
        readonly TextBox box = new() { Width = 70, VerticalContentAlignment = VerticalAlignment.Center };
        readonly int minimum, maximum, step;
        int value;
        public event Action? ValueChanged;

        public Spin(string label, int minimum, int maximum, int initial, int step)
        {
            (this.minimum, this.maximum, this.step, value) = (minimum, maximum, step, initial);
            Margin = new Thickness(0, 0, 0, 6);
            System.Windows.Automation.AutomationProperties.SetName(box, label);
            var buttons = new StackPanel { Orientation = Orientation.Horizontal };
            RepeatButton Step(string text, int sign)
            {
                var b = new RepeatButton { Content = text, Width = 22, Margin = new Thickness(2, 0, 0, 0) };
                System.Windows.Automation.AutomationProperties.SetName(b, $"{label} {(sign > 0 ? "up" : "down")}");
                b.Click += (_, _) => Value = value + sign * this.step;
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
            if (int.TryParse(box.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsed)) Value = parsed;
            else Show();
        }

        void Show() => box.Text = value.ToString(CultureInfo.InvariantCulture);

        public double Value
        {
            get => value;
            set
            {
                int clamped = (int)Math.Clamp(Math.Round(value), minimum, maximum);
                bool changed = clamped != this.value;
                this.value = clamped;
                Show();
                if (changed) ValueChanged?.Invoke();
            }
        }
    }
}
