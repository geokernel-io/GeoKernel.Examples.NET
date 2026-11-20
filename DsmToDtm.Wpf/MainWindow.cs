using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using GeoKernel.Examples.DsmToDtm;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.DsmToDtm.Wpf;

/// <summary>
/// Ground beneath buildings: two linked native 3D viewers show the original DSM beside a
/// prepared terrain estimate. The slider interpolates numeric heights between both endpoints.
/// No inference runs here; the reference DTM is used only for the profile and scores.
/// </summary>
public sealed class MainWindow : Window
{
    static Brush Hex(string color) => (Brush)new BrushConverter().ConvertFromString(color)!;

    readonly DsmOptions options;
    readonly SampleAssets assets = SampleAssets.ZurichV2();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-to-dtm-").FullName;
    readonly GeoKernelViewer3DControl[] viewers = [new(), new()];
    readonly TextBlock[] headings = new TextBlock[2];
    readonly ComboBox method = new() { ItemsSource = new[] { "Classical · morphological / TIN", "AI · DSM-only pix_grad" }, SelectedIndex = 0 };
    readonly Slider slider = new() { Minimum = 0, Maximum = 100, Value = 100, IsSnapToTickEnabled = true, TickFrequency = 1, LargeChange = 10, SmallChange = 1, ToolTip = "0% = original DSM; 100% = prepared DTM. Release to rebuild the surface." };
    readonly ComboBox exaggeration = new() { ItemsSource = Enumerable.Range(1, 16).Select(i => $"{i * .25:0.00} ×").ToArray(), SelectedIndex = 3, Width = 90 };
    readonly TextBlock percentLabel = Wrap("100% · prepared DTM endpoint"), metrics = Wrap("Preparing the sample..."), status = new();
    readonly Button retry = new() { Content = "Retry / reload sample" }, cancel = new() { Content = "Cancel loading" };
    readonly Button[] endpoints = [new() { Content = "Original · 0%" }, new() { Content = "Terrain · 100%" }];
    readonly ProfileChart chart = new() { Height = 190 };
    readonly ProgressBar progress = new() { Width = 200, Height = 14, Maximum = 100, Visibility = Visibility.Collapsed };
    readonly DispatcherTimer cameraPoll = new() { Interval = TimeSpan.FromMilliseconds(30) }, debounce = new() { Interval = TimeSpan.FromMilliseconds(200) };
    readonly float[]?[] lastCamera = new float[]?[2];
    CancellationTokenSource cts = new();
    Grids? grids;
    bool ready, busy, closeQueued, sliderDown, restoring;
    int committedMethod = 1, committedPercent = 100, serial;
    float heightScale = 1;

    static TextBlock Wrap(string text) => new() { Text = text, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 10) };

    public MainWindow(DsmOptions options)
    {
        this.options = options;
        Title = "DSM to DTM | Ground beneath buildings — GeoKernel";
        Width = 1520;
        Height = 960;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Hex("#f2f6f8");
        Foreground = Hex("#233e50");
        ExampleIcon.Apply(this);
        Content = BuildLayout();
        cameraPoll.Tick += (_, _) => PollCameras();
        debounce.Tick += (_, _) => { debounce.Stop(); if (!sliderDown) _ = UpdateSurfaceAsync(); };
        SetBusy(false);
        ContentRendered += (_, _) => _ = StartAsync();
    }

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
                content = new Image { Source = BitmapFrame.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.OnLoad), Width = 20, Height = 20 };
            }
            var button = new Button { Content = content, ToolTip = text, Margin = new Thickness(2, 0, 2, 0) };
            AutomationProperties(button, text);
            button.Click += (_, _) => action();
            toolbar.Items.Add(button);
            return button;
        }
        Tool("Reset view", "FullExtent.png", ResetView);
        Tool("Top view", null, TopView);
        Tool("Zoom in", "ZoomIn.png", () => Zoom(.8f));
        Tool("Zoom out", "ZoomOut.png", () => Zoom(1.25f));
        var profile = new ToggleButton { Content = "Show profile", IsChecked = true, Margin = new Thickness(2, 0, 2, 0) };
        profile.Checked += (_, _) => chart.Visibility = Visibility.Visible;
        profile.Unchecked += (_, _) => chart.Visibility = Visibility.Collapsed;
        toolbar.Items.Add(profile);
        Tool("Data folder", null, () => { Directory.CreateDirectory(options.DataRoot); Process.Start(new ProcessStartInfo(options.DataRoot) { UseShellExecute = true }); });
        Tool("Controls", null, () => MessageBox.Show(this, DsmText.Controls, "Camera controls"));
        var tray = new ToolBarTray { Background = Brushes.White };
        tray.ToolBars.Add(toolbar);
        DockPanel.SetDock(tray, Dock.Top);
        root.Children.Add(tray);

        var statusBar = new StatusBar();
        statusBar.Items.Add(new StatusBarItem { Content = status });
        statusBar.Items.Add(new StatusBarItem { Content = progress, HorizontalAlignment = HorizontalAlignment.Right });
        DockPanel.SetDock(statusBar, Dock.Bottom);
        root.Children.Add(statusBar);

        var side = new StackPanel { Width = 285, Margin = new Thickness(12) };
        var title = Wrap("Ground beneath buildings");
        title.FontSize = 22;
        title.FontWeight = FontWeights.SemiBold;
        title.Foreground = Hex("#145e72");
        metrics.Background = Brushes.White;
        metrics.Padding = new Thickness(12);
        var notice = Wrap("Prepared results · no inference runs here. Both methods start from the original DSM independently.");
        notice.Background = Hex("#fff5db");
        notice.Foreground = Hex("#705528");
        notice.Padding = new Thickness(9);
        var endpointRow = new UniformGrid { Columns = 2, Margin = new Thickness(0, 0, 0, 10) };
        foreach (var button in endpoints) { button.Padding = new Thickness(6); button.Margin = new Thickness(0, 0, 4, 0); endpointRow.Children.Add(button); }
        var exaggerationRow = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
        exaggerationRow.Children.Add(new TextBlock { Text = "Vertical exaggeration", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
        exaggerationRow.Children.Add(exaggeration);
        foreach (var button in new[] { retry, cancel }) { button.Padding = new Thickness(7); button.Margin = new Thickness(0, 4, 0, 4); }
        method.Margin = slider.Margin = new Thickness(0, 0, 0, 10);
        foreach (var element in new UIElement[] {
            title, Wrap("Zürich · 250 × 250 m\n0.5 m height pixels · EPSG:2056"),
            Wrap("1  Inspect the original roofs and trees.\n2  Choose a prepared terrain estimate.\n3  Move the slider to reveal the ground."),
            Wrap("Terrain estimate"), method, percentLabel, slider, endpointRow,
            Wrap("Intermediate positions interpolate heights for display. They are not new terrain estimates."),
            exaggerationRow, metrics, notice, retry, cancel,
            Wrap("Source: Federal Office of Topography swisstopo\nAI: David Perez · experimental pix_grad") })
            side.Children.Add(element);
        var scroll = new ScrollViewer { Content = side, Width = 310, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        DockPanel.SetDock(scroll, Dock.Left);
        root.Children.Add(scroll);

        var workspace = new Grid { Margin = new Thickness(0, 12, 12, 6) };
        foreach (var height in new[] { GridLength.Auto, new GridLength(1, GridUnitType.Star), GridLength.Auto, GridLength.Auto, GridLength.Auto })
            workspace.RowDefinitions.Add(new RowDefinition { Height = height });
        void Row(UIElement element, int row) { Grid.SetRow(element, row); workspace.Children.Add(element); }
        Row(Wrap("One observed surface. Two independent ways to estimate terrain."), 0);
        var views = new Grid();
        views.ColumnDefinitions.Add(new ColumnDefinition());
        views.ColumnDefinitions.Add(new ColumnDefinition());
        for (int i = 0; i < 2; i++)
        {
            var column = new DockPanel { Margin = new Thickness(i == 0 ? 0 : 4, 0, i == 0 ? 4 : 0, 0), MinWidth = 300, MinHeight = 320 };
            headings[i] = new TextBlock
            {
                Text = i == 0 ? "ORIGINAL DSM · roofs + trees + ground" : "CLASSICAL DTM · prepared terrain estimate",
                Background = Hex("#e4eef2"), Foreground = Hex("#125a70"), Padding = new Thickness(10), FontWeight = FontWeights.SemiBold,
            };
            DockPanel.SetDock(headings[i], Dock.Top);
            column.Children.Add(headings[i]);
            column.Children.Add(viewers[i]);
            Grid.SetColumn(column, i);
            views.Children.Add(column);
        }
        Row(views, 1);
        chart.Margin = new Thickness(0, 8, 0, 0);
        Row(chart, 2);
        Row(Wrap("Profile: gray DSM · teal classical · purple AI · dashed orange reference. The line is fixed across the same buildings in both views."), 3);
        Row(Wrap("Local 3D preview of source LN02 heights; no geoid conversion. DSM/reference source years: 2018/2019. Fixed development crop; scores are not a held-out accuracy benchmark."), 4);
        root.Children.Add(workspace);

        retry.Click += (_, _) => _ = StartAsync();
        cancel.Click += (_, _) => RequestCancel();
        slider.ValueChanged += (_, _) => { if (restoring) return; percentLabel.Text = DsmText.PercentLabel((int)slider.Value); debounce.Stop(); debounce.Start(); };
        slider.AddHandler(Thumb.DragStartedEvent, new DragStartedEventHandler((_, _) => sliderDown = true));
        slider.AddHandler(Thumb.DragCompletedEvent, new DragCompletedEventHandler((_, _) => { sliderDown = false; debounce.Stop(); _ = UpdateSurfaceAsync(); }));
        endpoints[0].Click += (_, _) => slider.Value = 0;
        endpoints[1].Click += (_, _) => slider.Value = 100;
        method.SelectionChanged += (_, _) => { if (restoring) return; debounce.Stop(); _ = UpdateSurfaceAsync(); };
        exaggeration.SelectionChanged += (_, _) => SetExaggeration((exaggeration.SelectedIndex + 1) * .25f);
        return root;
    }

    static void AutomationProperties(FrameworkElement element, string name) => System.Windows.Automation.AutomationProperties.SetName(element, name);

    // ----- state ---------------------------------------------------------------------------
    void SetBusy(bool value)
    {
        busy = value;
        retry.IsEnabled = !value;
        cancel.IsEnabled = value;
        foreach (var control in new Control[] { method, slider, endpoints[0], endpoints[1] })
            control.IsEnabled = !value && ready;
        progress.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
    }

    void Report(int percent, string message)
    {
        progress.IsIndeterminate = false;
        progress.Value = Math.Clamp(percent, 0, 100);
        status.Text = message;
    }

    void Failure(string message)
    {
        SetBusy(false);
        status.Text = message;
        if (closeQueued) { Dispatcher.BeginInvoke(Close); return; }
        if (!cts.IsCancellationRequested) MessageBox.Show(this, message, "Sample unavailable", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    void RequestCancel()
    {
        cts.Cancel();
        cancel.IsEnabled = false;
        status.Text = "Cancelling... Previous complete views are retained.";
    }

    async Task StartAsync()
    {
        if (busy) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        SetBusy(true);
        int m = method.SelectedIndex + 1, percent = (int)slider.Value;
        string surface = Path.Combine(temporary, $"surface-{++serial}.tif");
        try
        {
            string folder = await SampleCache.PrepareAsync(assets, options, new Progress<(int Percent, string Message)>(p => Report(p.Percent, p.Message)), token);
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(folder);
            Report(0, "Reading numeric heights and preparing both native 3D scenes...");
            var loaded = await Task.Run(() => Grids.Read(folder, token), token);
            string path = await Task.Run(() => loaded.SurfaceFile(m, percent, surface), token);
            await LoadViewersAsync(loaded, Path.Combine(folder, "dsm.tif"), path, token);
            Commit(loaded, m, percent);
        }
        catch (Exception e) { Abort(e); }
    }

    async Task UpdateSurfaceAsync()
    {
        if (!ready || busy || grids == null) return;
        int m = method.SelectedIndex + 1, percent = (int)slider.Value;
        if (m == committedMethod && percent == committedPercent) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        SetBusy(true);
        Report(0, "Preparing the interpolated display surface...");
        var current = grids;
        string surface = Path.Combine(temporary, $"surface-{++serial}.tif");
        try
        {
            string path = await Task.Run(() => current.SurfaceFile(m, percent, surface), token);
            await LoadViewersAsync(current, null, path, token);
            Commit(current, m, percent);
        }
        catch (Exception e) { Abort(e); }
    }

    async Task LoadViewersAsync(Grids source, string? original, string result, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Viewer3D prepares geometry on its own worker; the C API reports no percentage.
        progress.IsIndeterminate = true;
        status.Text = "Preparing native 3D terrain...";
        string neutral = Path.Combine(source.Folder, "neutral_surface.tif");
        var loads = new List<Task>();
        if (original != null) loads.Add(viewers[0].LoadTerrainAndImageryAsync(original, neutral, 500, token));
        loads.Add(viewers[1].LoadTerrainAndImageryAsync(result, neutral, 500, token));
        await Task.WhenAll(loads);
    }

    void Abort(Exception e)
    {
        restoring = true;
        method.SelectedIndex = committedMethod - 1;
        slider.Value = committedPercent;
        restoring = false;
        debounce.Stop();
        percentLabel.Text = DsmText.PercentLabel(committedPercent);
        Failure(e is OperationCanceledException || cts.IsCancellationRequested ? "Loading cancelled. Previous complete views retained." : e.Message);
    }

    void Commit(Grids loaded, int m, int percent)
    {
        bool first = !ready;
        grids = loaded;
        ready = true;
        committedMethod = m;
        committedPercent = percent;
        foreach (var viewer in viewers) viewer.HeightScale = heightScale;
        chart.Grids = loaded;
        chart.Selected = m;
        chart.InvalidateVisual();
        if (first) { ResetView(); cameraPoll.Start(); }
        else SyncFrom(0);
        headings[1].Text = DsmText.Heading(m, percent);
        metrics.Text = DsmText.Metrics(loaded);
        percentLabel.Text = DsmText.PercentLabel((int)slider.Value);
        SetBusy(false);
        status.Text = DsmText.Ready;
        if (closeQueued) Dispatcher.BeginInvoke(Close);
    }

    // ----- cameras -------------------------------------------------------------------------
    void ResetView()
    {
        if (!ready || grids == null) return;
        viewers[0].ResetCamera();
        // Both scenes share the grid-centre origin at zero height: the crop centre at the
        // mean DSM height is (0, mean * scale * exaggeration, 0) in render units.
        viewers[0].MoveCamera([20, 48, 3.25f, 0, (float)(grids.MeanDsm * grids.RenderScale * heightScale), 0], 0);
        SyncFrom(0);
    }

    void TopView()
    {
        if (!ready) return;
        var camera = viewers[0].GetCamera();
        camera[0] = 0;
        camera[1] = 85;
        viewers[0].MoveCamera(camera, 0);
        SyncFrom(0);
    }

    void Zoom(float factor)
    {
        if (!ready) return;
        var camera = viewers[0].GetCamera();
        camera[2] = Math.Clamp(camera[2] * factor, .05f, 20f);
        viewers[0].MoveCamera(camera, 0);
        SyncFrom(0);
    }

    void SyncFrom(int source)
    {
        // Same grid, same origin and (to within 1e-5) the same render scale: render-space
        // cameras transfer directly between the two native scenes.
        viewers[1 - source].MoveCamera(viewers[source].GetCamera(), 0);
        for (int i = 0; i < 2; i++) lastCamera[i] = viewers[i].GetCamera();
    }

    void PollCameras()
    {
        // Pause while a scene is being replaced: its interim camera must not propagate.
        if (!ready || busy) return;
        for (int i = 0; i < 2; i++)
            if (!Cameras.Same(lastCamera[i], viewers[i].GetCamera())) { SyncFrom(i); break; }
    }

    void SetExaggeration(float value)
    {
        float old = heightScale;
        heightScale = value;
        foreach (var viewer in viewers)
        {
            float[]? camera = ready ? viewer.GetCamera() : null;
            viewer.HeightScale = value;
            if (camera != null) { camera[4] *= value / old; viewer.MoveCamera(camera, 0); }
        }
        if (ready) SyncFrom(0);
    }

    // ----- shutdown ------------------------------------------------------------------------
    protected override void OnClosing(CancelEventArgs e)
    {
        if (busy) { closeQueued = true; RequestCancel(); e.Cancel = true; return; }
        cameraPoll.Stop();
        debounce.Stop();
        base.OnClosing(e);
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        foreach (var viewer in viewers) viewer.Dispose();
        try { Directory.Delete(temporary, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    sealed class ProfileChart : FrameworkElement
    {
        public Grids? Grids;
        public int Selected = 1;

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Brushes.White, null, new Rect(RenderSize));
            if (Grids == null) return;
            var (rows, low, high) = Grids.Profile();
            var plot = new Rect(64, 30, Math.Max(1, ActualWidth - 88), Math.Max(1, ActualHeight - 64));
            var ink = Hex("#476174");
            var typeface = new Typeface("Segoe UI");
            double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            FormattedText Text(string text) => new(text, System.Globalization.CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 12, ink, dpi);
            dc.DrawText(Text("Fixed building transect · original pixel heights (LN02 metres)"), new Point(8, 6));
            var gridPen = new Pen(Hex("#e2e9ee"), 1);
            for (int i = 0; i <= 4; i++)
            {
                double t = i / 4.0, y = plot.Bottom - t * plot.Height;
                dc.DrawLine(gridPen, new Point(plot.Left, y), new Point(plot.Right, y));
                var label = Text($"{low + t * (high - low):F0}");
                dc.DrawText(label, new Point(56 - label.Width, y - 8));
                var distance = Text($"{t * 249.5:F0} m");
                dc.DrawText(distance, new Point(plot.Left + t * plot.Width - distance.Width / 2, plot.Bottom + 5));
            }
            foreach (int index in new[] { 0, 3, 1, 2 })
            {
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    for (int x = 0; x < rows[index].Length; x++)
                    {
                        var point = new Point(plot.Left + (double)x / (Grids.Width - 1) * plot.Width, plot.Bottom - (rows[index][x] - low) / (high - low) * plot.Height);
                        if (x == 0) context.BeginFigure(point, false, false); else context.LineTo(point, true, false);
                    }
                }
                geometry.Freeze();
                var pen = new Pen(Hex(DsmText.Colors[index]), index == Selected ? 2.5 : 1.4);
                if (index == 3) pen.DashStyle = DashStyles.Dash;
                dc.DrawGeometry(null, pen, geometry);
            }
        }
    }
}
