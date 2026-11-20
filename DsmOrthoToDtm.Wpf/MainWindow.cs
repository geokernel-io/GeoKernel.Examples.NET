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

namespace GeoKernel.DsmOrthoToDtm.Wpf;

/// <summary>
/// DSM + orthophoto to DTM: three independent terrain estimates (classical, DSM-only pix_grad and DSM+RGB
/// rgb_grad_fm) from the same full DSM on four linked native 2D maps, with the input context on the left.
/// The reference is evaluation-only; the methods do not feed each other.
/// </summary>
public sealed class MainWindow : Window
{
    const string WindowTitle = "DSM + Orthophoto to DTM | Three independent methods — GeoKernel";
    static Brush Hex(string color) => (Brush)new BrushConverter().ConvertFromString(color)!;

    readonly OrthoOptions options;
    readonly SampleAssets sampleAssets = Ortho.SampleAssets();
    readonly string[] archiveSha256 = Ortho.ArchiveSha256();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-ortho-").FullName;
    readonly GeoKernelViewerControl[] maps = [new(), new(), new(), new()];
    readonly TextBlock[] headings = new TextBlock[4];
    readonly TextBlock[,] cells = new TextBlock[3, 5];
    readonly Spin overlap = new("AI overlap (px)", 0, 128, 64, 32), threads = new("CPU threads", 1, 8, 4, 1);
    readonly ComboBox preset = new() { ItemsSource = Terrain.PresetNames, SelectedIndex = 0, Margin = new Thickness(0, 0, 0, 8) },
        input = new() { ItemsSource = OrthoText.Inputs, SelectedIndex = 1, Margin = new Thickness(0, 0, 0, 6) },
        mode = new() { ItemsSource = OrthoText.Modes, SelectedIndex = 0, Margin = new Thickness(0, 0, 0, 6) },
        scope = new() { ItemsSource = OrthoText.Scopes, SelectedIndex = 1, Margin = new Thickness(0, 0, 0, 6) };
    readonly TextBlock source = Card("Preparing sample..."), modelInfo = Card("Models not prepared."), state = Wrap("Load data and models, then Run."),
        probe = Card("Click a source pixel to inspect RGB and all terrain estimates."), status = new(),
        legend = new() { Text = "Inputs ready first; Run generates all three terrain estimates.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 4) },
        summary = new() { Text = "No prepared prediction is loaded.", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 2) };
    readonly Button sampleButton = MakeButton("Load Zurich DSM + orthophoto"), openButton = MakeButton("Open aligned DSM + RGB..."),
        referenceButton = MakeButton("Open reference (optional)..."), modelsButton = MakeButton("Download / verify both models"),
        pixButton = MakeButton("Use pix_grad model ZIP..."), rgbButton = MakeButton("Use RGB model ZIP..."),
        runButton = MakeButton("Run all three methods"), cancelButton = MakeButton("Cancel"), saveButton = MakeButton("Save comparison folder...");
    readonly List<UIElement> navigation = [];
    readonly ProgressBar progress = new() { Width = 230, Height = 14, Maximum = 100, Visibility = Visibility.Collapsed };
    readonly (double X, double Y)?[] presses = new (double X, double Y)?[4];
    readonly GeoKernelExtent?[] applied = new GeoKernelExtent?[4];
    CancellationTokenSource cts = new();
    OrthoScene? scene;
    OrthoComparison? comparison;
    string[]? models;
    (double X, double Y)? probePoint;
    GeoKernelViewerTool tool = GeoKernelViewerTool.Pan;
    bool busy, closeQueued, closed, syncing, setupModels;
    int generation;

    static TextBlock Wrap(string text) => new() { Text = text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 9) };
    static TextBlock Card(string text)
    {
        var block = Wrap(text);
        block.Background = Brushes.White;
        block.Padding = new Thickness(9);
        return block;
    }
    /// <summary>A TextBlock keeps underscores literal (a plain string content would make "_g" an access key).</summary>
    static Button MakeButton(string text)
    {
        var button = new Button { Content = new TextBlock { Text = text }, Padding = new Thickness(7), Margin = new Thickness(0, 0, 0, 7) };
        System.Windows.Automation.AutomationProperties.SetName(button, text);
        return button;
    }

    public MainWindow(OrthoOptions options)
    {
        this.options = options;
        Title = WindowTitle;
        Width = 1920;
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
        var title = Wrap("DSM + RGB → DTM\nCompare three methods");
        title.FontSize = 21;
        title.FontWeight = FontWeights.SemiBold;
        title.Foreground = Hex("#145e72");
        var presetRow = new DockPanel { Margin = new Thickness(0, 0, 0, 2) };
        var presetLabel = new TextBlock { Text = "Classical preset", Width = 120, VerticalAlignment = VerticalAlignment.Center };
        DockPanel.SetDock(presetLabel, Dock.Left);
        presetRow.Children.Add(presetLabel);
        presetRow.Children.Add(preset);
        System.Windows.Automation.AutomationProperties.SetName(preset, "Classical preset");
        System.Windows.Automation.AutomationProperties.SetName(input, "Input context");
        System.Windows.Automation.AutomationProperties.SetName(mode, "Compare estimates");
        System.Windows.Automation.AutomationProperties.SetName(scope, "Evaluation scope");
        foreach (var element in new UIElement[] {
            title, Wrap("Classical · DSM-only AI · DSM+RGB AI.\nEach starts from the same full DSM."), sampleButton, openButton, referenceButton, source,
            modelsButton, pixButton, rgbButton, modelInfo, overlap, threads, presetRow, runButton, cancelButton, state, Wrap("Input context"), input,
            Wrap("Compare estimates"), mode, scope, saveButton, probe, Wrap(OrthoText.Attribution) })
            side.Children.Add(element);
        var scroll = new ScrollViewer { Content = side, Width = 310, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        DockPanel.SetDock(scroll, Dock.Left);
        root.Children.Add(scroll);

        var workspace = new Grid { Margin = new Thickness(0, 12, 12, 6) };
        foreach (var height in new[] { new GridLength(1, GridUnitType.Star), GridLength.Auto, GridLength.Auto, GridLength.Auto, GridLength.Auto })
            workspace.RowDefinitions.Add(new RowDefinition { Height = height });
        void Row(UIElement element, int row) { Grid.SetRow(element, row); workspace.Children.Add(element); }
        var views = new Grid();
        for (int i = 0; i < 4; i++) views.ColumnDefinitions.Add(new ColumnDefinition());
        for (int i = 0; i < 4; i++)
        {
            int pane = i;
            var column = new DockPanel { Margin = new Thickness(i == 0 ? 0 : 3.5, 0, i == 3 ? 0 : 3.5, 0), MinWidth = 230, MinHeight = 260 };
            headings[i] = new TextBlock { Text = OrthoText.Titles[i], Background = Hex("#e4eef2"), Foreground = Hex("#125a70"), Padding = new Thickness(9), FontWeight = FontWeights.SemiBold };
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
        Row(new TextBlock { Text = OrthoText.Footer, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 2, 0, 0) }, 4);
        root.Children.Add(workspace);

        void Changed() { if (comparison != null) state.Text = OrthoText.ControlsChanged; }
        overlap.ValueChanged += Changed;
        threads.ValueChanged += Changed;
        preset.SelectionChanged += (_, _) => Changed();
        input.SelectionChanged += (_, _) => ShowInput();
        mode.SelectionChanged += (_, _) => Display();
        scope.SelectionChanged += (_, _) => { UpdateScores(); Fit(); };
        sampleButton.Click += (_, _) => LoadSample();
        openButton.Click += (_, _) => OpenInputs();
        referenceButton.Click += (_, _) => OpenReference();
        modelsButton.Click += (_, _) => PrepareModels(-1, null);
        pixButton.Click += (_, _) => ChooseModel(0);
        rgbButton.Click += (_, _) => ChooseModel(1);
        runButton.Click += (_, _) => Process();
        cancelButton.Click += (_, _) => RequestCancel();
        saveButton.Click += (_, _) => ExportResults();
        return root;
    }

    /// <summary>Method · valid pixels · RMSE · MAE · bias for the three estimates.</summary>
    UIElement ScoreTable()
    {
        var grid = new Grid { Background = Brushes.White };
        for (int c = 0; c < 5; c++) grid.ColumnDefinitions.Add(new ColumnDefinition());
        for (int r = 0; r < 4; r++) grid.RowDefinitions.Add(new RowDefinition());
        for (int c = 0; c < 5; c++)
        {
            var header = new TextBlock { Text = AiText.Columns[c], FontWeight = FontWeights.SemiBold, Padding = new Thickness(8, 5, 8, 5), Background = Hex("#eef3f6") };
            Grid.SetColumn(header, c);
            grid.Children.Add(header);
            for (int r = 0; r < 3; r++)
            {
                var cell = cells[r, c] = new TextBlock { Padding = new Thickness(8, 5, 8, 5), Foreground = Hex(OrthoText.Colors[r]) };
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
        foreach (UIElement e in (UIElement[])[sampleButton, openButton, modelsButton, pixButton, rgbButton, overlap, threads, preset, input, scope]) e.IsEnabled = !value;
        foreach (var item in navigation) item.IsEnabled = !value && scene != null;
        runButton.IsEnabled = !value && scene != null && models != null;
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
            else if (setupModels && scene != null && models == null) { setupModels = false; _ = Dispatcher.BeginInvoke(() => PrepareModels(-1, null)); }
        }
        catch (Exception e)
        {
            if (closed) return;
            SetBusy(false);
            bool cancelled = e is OperationCanceledException || token.IsCancellationRequested;
            state.Text = status.Text = cancelled ? "Cancelled; last completed comparison retained." : e.Message;
            if (closeQueued) _ = Dispatcher.BeginInvoke(Close);
            else if (!cancelled) MessageBox.Show(this, e.Message, "Unable to complete operation", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    // ----- jobs ----------------------------------------------------------------------------
    void ChooseModel(int index)
    {
        if (busy) return;
        var dialog = new OpenFileDialog { Title = $"Choose the pinned {Ortho.Methods[index + 1]} model ZIP", Filter = "ZIP (*.zip)|*.zip" };
        if (dialog.ShowDialog(this) == true) PrepareModels(index, dialog.FileName);
    }

    void PrepareModels(int selected, string? local)
    {
        if (busy) return;
        RunJob((token, report) =>
        {
            try { return new[] { Ortho.PrepareModel(options, 0, selected == 0 ? local : null, token, report), Ortho.PrepareModel(options, 1, selected == 1 ? local : null, token, report) }; }
            catch (Exception e) when (e is not OperationCanceledException) { throw new InvalidOperationException(e.Message + "\nUse the pinned model ZIPs or retry the published URLs.", e); }
        }, folders =>
        {
            models = folders;
            modelInfo.Text = OrthoText.ModelsReady;
            state.Text = status.Text = scene != null ? "Inputs and both models ready. Press Run." : "Models ready; load aligned DSM and RGB.";
        });
    }

    OrthoScene LoadScene(string dsmPath, string rgbPath, string? referencePath, JsonObject manifest, string folder, CancellationToken token, Action<int, string> report)
    {
        report(5, "Reading source DSM...");
        var dsm = Terrain.Read(dsmPath, token);
        report(25, "Checking RGB channels, grid and coverage...");
        var rgb = Ortho.ReadOrtho(dsm, rgbPath, token);
        TerrainRaster? reference = null;
        if (referencePath != null)
        {
            report(50, "Reading evaluation-only reference...");
            reference = Terrain.Read(referencePath, token);
            Terrain.ValidateReference(dsm, reference);
        }
        if ((bool?)manifest["standard_full_tile"] == true)
        {
            var (x0, y0, x1, y1) = dsm.Extent;
            if (!Gdal.SameCrs(dsm.Projection, 2056) || dsm.Width != 2000 || dsm.Height != 2000 || dsm.Pixel != .5 || (x0, y0, x1, y1) != (2677000, 1251000, 2678000, 1252000))
                throw new InvalidDataException("Unexpected Zurich sample grid.");
        }
        report(70, "Preparing original DSM relief...");
        var (r, g, b) = Terrain.Relief(dsm, dsm.Z, token);
        string original = Gdal.WriteRgb(Path.Combine(folder, "original.tif"), dsm.Width, dsm.Height, dsm.Transform, dsm.Projection, r, g, b);
        token.ThrowIfCancellationRequested();
        return new OrthoScene(dsm, rgb, reference, original, manifest);
    }

    void LoadSample()
    {
        if (busy) return;
        string folder = NextFolder();
        setupModels = models == null;
        RunJob((token, report) =>
        {
            string sample = SampleCache.PrepareAsync(sampleAssets, options.Sample, new Progress<(int Percent, string Message)>(p => report(p.Percent, p.Message)), token)
                .GetAwaiter().GetResult();
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(sample);
            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(sample, "source-manifest.json")))!.AsObject();
            return LoadScene(Path.Combine(sample, "dsm.tif"), Path.Combine(sample, "ortho_rgb.tif"), Path.Combine(sample, "reference.tif"), manifest, folder, token, report);
        }, loaded => Publish(loaded, null, "Aligned inputs ready. Prepare both models and Run."));
    }

    void OpenInputs()
    {
        if (busy) return;
        var dsmDialog = new OpenFileDialog { Title = "Open metre-height DSM", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff" };
        if (dsmDialog.ShowDialog(this) != true) return;
        var rgbDialog = new OpenFileDialog { Title = "Open exactly aligned RGB Byte orthophoto", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff", InitialDirectory = Path.GetDirectoryName(dsmDialog.FileName) };
        if (rgbDialog.ShowDialog(this) != true) return;
        string dsm = dsmDialog.FileName, rgb = rgbDialog.FileName, folder = NextFolder();
        RunJob((token, report) => LoadScene(dsm, rgb, null, new JsonObject(), folder, token, report),
            loaded => Publish(loaded, null, "Aligned inputs ready. Prepare both models and Run."));
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
            var updated = completed == null ? null : Ortho.AttachReference(next, completed, folder, archiveSha256, token, report);
            token.ThrowIfCancellationRequested();
            return (next, updated);
        }, result => Publish(result.next, result.updated, "Reference updated; all generated height arrays are unchanged."));
    }

    void Process()
    {
        if (busy || scene is not { } current || models is not { } packages) return;
        var parameters = Ai.EffectiveParameters(current.Dsm, Terrain.Presets[Math.Max(0, preset.SelectedIndex)]);
        int tileOverlap = (int)overlap.Value, cpu = (int)threads.Value;
        string folder = NextFolder();
        RunJob((token, report) => Ortho.Compare(current, packages, parameters, tileOverlap, cpu, folder, archiveSha256, token, report),
            result => Publish(current, result, "All three methods completed from the same full source grid."));
    }

    void ExportResults()
    {
        if (busy || comparison is not { } completed) return;
        var dialog = new OpenFolderDialog { Title = "Choose parent folder for a new complete result bundle" };
        if (dialog.ShowDialog(this) != true) return;
        string parent = dialog.FolderName;
        RunJob((token, report) => Ortho.Export(completed, parent, token, report), target => { state.Text = status.Text = "Saved complete comparison bundle: " + target; });
    }

    void Publish(OrthoScene loaded, OrthoComparison? result, string message)
    {
        bool changed = scene == null || !ReferenceEquals(loaded.Dsm, scene.Dsm);
        scene = loaded;
        comparison = result;
        if (changed)
        {
            ShowInput();
            for (int pane = 1; pane < 4; pane++) { maps[pane].ClearLayers(); maps[pane].RefreshLayers(); }
            probePoint = null;
            probe.Text = "Click a source pixel to inspect RGB and all terrain estimates.";
            Array.Clear(applied);
            _ = Dispatcher.BeginInvoke(Fit);
        }
        source.Text = OrthoText.Source(loaded);
        if (comparison != null)
        {
            Display();
            UpdateScores();
        }
        else
        {
            foreach (var cell in cells) cell.Text = "";
            summary.Text = "Aligned inputs loaded. Run to create all three terrain estimates.";
            for (int pane = 1; pane < 4; pane++) headings[pane].Text = OrthoText.Pending[pane - 1];
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

    void ShowInput()
    {
        if (scene == null) return;
        bool rgb = input.SelectedIndex == 1;
        Replace(0, rgb ? scene.Rgb.Path : scene.Original);
        headings[0].Text = rgb ? "RGB ORTHOPHOTO · source input" : "ORIGINAL DSM · source input";
        DrawShapes();
    }

    void Fit()
    {
        if (scene == null) return;
        bool crop = scope.SelectedIndex == 1;
        var (x0, y0, x1, y1) = crop ? Ai.CropExtent(scene.Terrain) : scene.Dsm.Extent;
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
            for (int other = 0; other < 4; other++)
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
            status.Text = "Attach an aligned reference to show errors.";
            return;
        }
        for (int method = 0; method < 3; method++)
        {
            Replace(method + 1, comparison.Maps[method][selected]);
            headings[method + 1].Text = $"{Ortho.Methods[method].ToUpperInvariant()} {OrthoText.Suffix[selected]}";
        }
        legend.Text = OrthoText.Legends[selected];
        DrawShapes();
    }

    void UpdateScores()
    {
        if (comparison == null || scene == null) return;
        bool crop = scope.SelectedIndex == 1;
        var values = crop ? comparison.Crop : comparison.Full;
        for (int method = 0; method < 3; method++)
        {
            string[] row = OrthoText.Row(method, values[method]);
            for (int column = 0; column < row.Length; column++) cells[method, column].Text = row[column];
        }
        summary.Text = OrthoText.Summary(comparison, scene, crop);
    }

    /// <summary>
    /// GeoKernel.NET 1.5.40 wraps no overlay point, so the probe is an owned point shape; owned shapes also dim
    /// everything outside the shared comparison crop (four translucent bands) and outline it.
    /// </summary>
    void DrawShapes()
    {
        if (scene == null) return;
        var (x0, y0, x1, y1) = Ai.CropExtent(scene.Terrain);
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
        if (OrthoText.Probe(scene, comparison, x, y) is not { } hit) return;
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
