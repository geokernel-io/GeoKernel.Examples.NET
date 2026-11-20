using System.ComponentModel;
using System.Globalization;
using System.IO;
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

namespace GeoKernel.DsmToDtmProfile.Wpf;

/// <summary>
/// Interactive DSM / DTM elevation profile: draw an A → B transect on two linked native 2D
/// maps and compare the original DSM, classical DTM, AI DTM and provider reference heights.
/// Profiles sample the numeric rasters; the maps show matching display reliefs. No inference.
/// </summary>
public sealed class MainWindow : Window
{
    static Brush Hex(string color) => (Brush)new BrushConverter().ConvertFromString(color)!;
    static readonly string[] Colors = DsmText.Colors;
    static readonly double[] Spacings = Enumerable.Range(1, 20).Select(i => i * .5).ToArray();

    readonly DsmOptions options;
    readonly SampleAssets assets = SampleAssets.ZurichV2();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-profile-").FullName;
    readonly GeoKernelViewerControl[] maps = [new(), new()];
    readonly TextBlock[] headings = new TextBlock[2];
    readonly ComboBox surface = new() { ItemsSource = new[] { "Classical DTM", "AI DTM · pix_grad", "Reference DTM · evaluation only" }, SelectedIndex = 0 };
    readonly ComboBox mode = new() { ItemsSource = new[] { "Compare elevations", "Compare errors against reference" }, SelectedIndex = 0 };
    readonly ComboBox spacing = new() { ItemsSource = Spacings.Select(v => $"{v:0.0} m").ToArray(), SelectedIndex = 0, Width = 90 };
    readonly TextBlock instructions = Card("Preparing sample data..."), stats = Card("Select a transect to compare errors."), cursor = Card(ProfileText.NoCursor),
        status = new(), legend = new() { Text = ProfileText.Legends[0], Margin = new Thickness(0, 4, 0, 0) };
    readonly Button export = new() { Content = "Export profile CSV" }, reload = new() { Content = "Retry / reload sample" }, cancel = new() { Content = "Cancel loading" };
    readonly ToggleButton draw = new() { Content = "Draw transect · A → B", Margin = new Thickness(2, 0, 2, 0) };
    readonly List<UIElement> navigation = [];
    readonly ProfileChart chart;
    readonly ProgressBar progress = new() { Width = 200, Height = 14, Maximum = 100, Visibility = Visibility.Collapsed };
    readonly (double X, double Y)?[] presses = new (double X, double Y)?[2];
    readonly GeoKernelExtent?[] applied = new GeoKernelExtent?[2];
    CancellationTokenSource cts = new();
    Grids? grids;
    (double X, double Y)? draft;
    bool busy, closeQueued, closed, syncing;
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
        chart = new ProfileChart(Hover) { MinHeight = 200 };
        Title = ProfileText.Title;
        Width = 1460;
        Height = 940;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Background = Hex("#f2f6f8");
        Foreground = Hex("#233e50");
        ExampleIcon.Apply(this);
        Content = BuildLayout();
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
        toolbar.Items.Add(draw);
        Tool("Example transect", null, ExampleProfile);
        Tool("Clear transect", null, ClearProfile);
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

        var side = new StackPanel { Width = 270, Margin = new Thickness(12) };
        var title = Wrap("Compare along\nyour own transect");
        title.FontSize = 21;
        title.FontWeight = FontWeights.SemiBold;
        title.Foreground = Hex("#145e72");
        var spacingRow = new DockPanel { Margin = new Thickness(0, 0, 0, 10) };
        spacingRow.Children.Add(new TextBlock { Text = "Sample spacing", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
        spacingRow.Children.Add(spacing);
        surface.Margin = new Thickness(0, 0, 0, 10);
        foreach (var button in new[] { export, reload, cancel }) { button.Padding = new Thickness(7); button.Margin = new Thickness(0, 4, 0, 4); }
        foreach (var element in new UIElement[] {
            title, Wrap("Zürich · 250 × 250 m\n0.5 m height pixels · EPSG:2056"), instructions,
            Wrap("Right-hand map"), surface, spacingRow, stats, cursor, Wrap(ProfileText.Numeric), export, reload, cancel,
            Wrap("Source: Federal Office of Topography swisstopo\nAI: David Perez · experimental pix_grad") })
            side.Children.Add(element);
        var scroll = new ScrollViewer { Content = side, Width = 295, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        DockPanel.SetDock(scroll, Dock.Left);
        root.Children.Add(scroll);

        var workspace = new Grid { Margin = new Thickness(0, 12, 12, 6) };
        workspace.RowDefinitions.Add(new RowDefinition { Height = new GridLength(2, GridUnitType.Star), MinHeight = 280 });
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        workspace.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star), MinHeight = 220 });
        workspace.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        void Row(UIElement element, int row) { Grid.SetRow(element, row); workspace.Children.Add(element); }
        var views = new Grid();
        views.ColumnDefinitions.Add(new ColumnDefinition());
        views.ColumnDefinitions.Add(new ColumnDefinition());
        for (int i = 0; i < 2; i++)
        {
            int sideIndex = i;
            var column = new DockPanel { Margin = new Thickness(i == 0 ? 0 : 4, 0, i == 0 ? 4 : 0, 0), MinWidth = 300 };
            headings[i] = new TextBlock
            {
                Text = i == 0 ? "ORIGINAL DSM · roofs + trees + ground" : ProfileText.Headings[0],
                Background = Hex("#e4eef2"), Foreground = Hex("#125a70"), Padding = new Thickness(9), FontWeight = FontWeights.SemiBold,
            };
            DockPanel.SetDock(headings[i], Dock.Top);
            column.Children.Add(headings[i]);
            column.Children.Add(maps[i]);
            maps[i].VisibleExtentChanged += (_, _) => ExtentChanged(sideIndex);
            maps[i].MapMouseDown += (_, e) => { if (e.ButtonOrButtons == 1) presses[sideIndex] = (e.ScreenPoint.X, e.ScreenPoint.Y); };
            maps[i].MapMouseUp += (_, e) => MapClick(sideIndex, e);
            Grid.SetColumn(column, i);
            views.Children.Add(column);
        }
        Row(views, 0);
        Row(new GridSplitter { Height = 6, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Transparent }, 1);
        var profilePane = new DockPanel();
        DockPanel.SetDock(mode, Dock.Top);
        DockPanel.SetDock(legend, Dock.Bottom);
        profilePane.Children.Add(mode);
        profilePane.Children.Add(legend);
        profilePane.Children.Add(chart);
        Row(profilePane, 2);
        Row(new TextBlock { Text = ProfileText.Footnote, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) }, 3);
        root.Children.Add(workspace);

        draw.Checked += (_, _) => ToggleDraw(true);
        draw.Unchecked += (_, _) => ToggleDraw(false);
        surface.SelectionChanged += (_, _) => SelectSurface();
        spacing.SelectionChanged += (_, _) => Resample();
        mode.SelectionChanged += (_, _) => { chart.Errors = mode.SelectedIndex == 1; legend.Text = ProfileText.Legends[mode.SelectedIndex]; chart.InvalidateVisual(); };
        export.Click += (_, _) => ExportCsv();
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
        draw.IsEnabled = surface.IsEnabled = spacing.IsEnabled = ready;
        foreach (var item in navigation) item.IsEnabled = ready;
        export.IsEnabled = !value && chart.Profile != null;
    }

    void Report(int percent, string message)
    {
        progress.Value = Math.Clamp(percent, 0, 100);
        status.Text = message;
    }

    void RequestCancel()
    {
        cts.Cancel();
        cancel.IsEnabled = false;
        status.Text = "Cancelling loading...";
    }

    async Task StartAsync()
    {
        if (busy) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        SetBusy(true);
        var report = new Progress<(int Percent, string Message)>(p => Report(p.Percent, p.Message));
        string folder = Path.Combine(temporary, (++generation).ToString(CultureInfo.InvariantCulture));
        try
        {
            string sample = await SampleCache.PrepareAsync(assets, options, report, token);
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(sample);
            Directory.CreateDirectory(folder);
            var (loaded, reliefs) = await Task.Run(() =>
            {
                const string message = "Reading numeric heights and preparing matching relief maps...";
                ((IProgress<(int, string)>)report).Report((0, message));
                var g = Grids.Read(sample, token);
                var paths = new string[4];
                for (int band = 0; band < 4; band++)
                {
                    paths[band] = Gdal.WriteRgb(Path.Combine(folder, $"relief-{band}.tif"), g.Width, g.Height, g.Transform, g.Projection, Profiles.Relief(g, band, token));
                    ((IProgress<(int, string)>)report).Report(((band + 1) * 25, message));
                }
                return (g, paths);
            }, token);
            token.ThrowIfCancellationRequested();
            Publish(loaded, reliefs);
            SetBusy(false);
            ExampleProfile();
            status.Text = ProfileText.Ready;
            if (closeQueued) _ = Dispatcher.BeginInvoke(Close);
        }
        catch (Exception e)
        {
            if (closed) return;
            SetBusy(false);
            bool cancelled = e is OperationCanceledException || cts.IsCancellationRequested;
            status.Text = cancelled ? "Loading cancelled." : e.Message;
            if (closeQueued) _ = Dispatcher.BeginInvoke(Close);
            else if (!cancelled) MessageBox.Show(this, e.Message, "Unable to load sample", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    /// <summary>DSM relief left; all three estimate reliefs right with one visible.</summary>
    void Publish(Grids loaded, string[] reliefs)
    {
        grids = loaded;
        for (int pane = 0; pane < 2; pane++)
        {
            var map = maps[pane];
            map.ClearShapes();
            map.ClearLayers();
            // The empty map adopts the first layer's CRS (EPSG:2056); new layers go to index 0.
            foreach (int band in pane == 0 ? new[] { 0 } : [1, 2, 3])
            {
                if (!map.AddLayerFile(reliefs[band])) throw new InvalidOperationException($"The relief layer could not be added: {reliefs[band]}");
                map.SetLayerName(0, Profiles.SurfaceNames[band]);
            }
            map.SetTool(GeoKernelViewerTool.Pan);
        }
        applied[0] = applied[1] = null;
        FullExtent();
        SelectSurface();
    }

    // ----- maps ----------------------------------------------------------------------------
    static int RightIndex(int band) => 3 - band; // bands 1, 2, 3 were prepended in that order

    void FullExtent()
    {
        var e = ProfileText.FullExtent;
        maps[0].ViewExtent = new GeoKernelExtent(e.XMin, e.YMin, e.XMax, e.YMax);
        SyncFrom(0);
    }

    void SyncFrom(int side)
    {
        int other = 1 - side;
        syncing = true;
        try
        {
            applied[side] = maps[side].ViewExtent;
            maps[other].ViewExtent = applied[side]!.Value;
            applied[other] = maps[other].ViewExtent;
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
            if (!syncing && !closed && grids != null && !Same(applied[side], maps[side].ViewExtent)) SyncFrom(side);
        });
    }

    void MapClick(int side, GeoKernelMapMouseEventArgs e)
    {
        if (e.ButtonOrButtons != 1) return;
        var press = presses[side];
        presses[side] = null;
        // A click (not a pan drag) picks a point; dragging still pans.
        if (press is { } p && Math.Abs(p.X - e.ScreenPoint.X) + Math.Abs(p.Y - e.ScreenPoint.Y) <= 3)
            Pick(e.WorldPoint.X, e.WorldPoint.Y);
    }

    void UseTool(GeoKernelViewerTool tool)
    {
        draw.IsChecked = false;
        foreach (var map in maps) map.SetTool(tool);
    }

    void SelectSurface()
    {
        if (grids == null) return;
        int choice = surface.SelectedIndex + 1;
        for (int band = 1; band <= 3; band++) maps[1].SetLayerVisible(RightIndex(band), band == choice);
        maps[1].RefreshLayers();
        headings[1].Text = ProfileText.Headings[choice - 1];
    }

    static GeoKernelLayerStyle Marker(string color, double size) => new() { PointColor = color, PointSize = size, LineColor = "#ffffff", LineWidth = 2 };

    /// <summary>Owned shapes on both maps: white halo + teal line, A teal, B orange and the hovered sample.
    /// GeoKernel.NET 1.5.40 wraps no overlay point, so hovering redraws these shapes.</summary>
    void DrawOverlays()
    {
        var profile = chart.Profile;
        foreach (var map in maps)
        {
            map.ClearShapes();
            if (draft is { } a) map.AddPointShape(new GeoKernelPoint(a.X, a.Y), Marker(Colors[1], 16));
            else if (profile != null)
            {
                GeoKernelPoint[] line = [new(profile.Start.X, profile.Start.Y), new(profile.End.X, profile.End.Y)];
                map.AddPolylineShape(line, new GeoKernelLayerStyle { LineColor = "#ffffff", LineWidth = 5 });
                map.AddPolylineShape(line, new GeoKernelLayerStyle { LineColor = Colors[1], LineWidth = 2.5 });
                map.AddPointShape(line[0], Marker(Colors[1], 16));
                map.AddPointShape(line[1], Marker(Colors[3], 16));
                if (chart.Hovered >= 0 && chart.Hovered < profile.Samples.Count)
                {
                    var s = profile.Samples[chart.Hovered];
                    map.AddPointShape(new GeoKernelPoint(s.X, s.Y), Marker("#dd3470", 11));
                }
            }
            map.RefreshLayers(); // owned shapes are drawn on the next full map render
        }
    }

    // ----- transect ------------------------------------------------------------------------
    void ToggleDraw(bool on)
    {
        draft = null;
        if (on)
        {
            foreach (var map in maps) map.SetTool(GeoKernelViewerTool.Pan);
            instructions.Text = "Click A on either map. Dragging still pans; wheel zooms.";
        }
        else instructions.Text = "Draw a new A → B transect, or inspect the current profile.";
        DrawOverlays();
    }

    void Pick(double x, double y)
    {
        if (grids == null || busy || draw.IsChecked != true) return;
        if (Profiles.Height(grids, 0, x, y) == null) { status.Text = "Click inside the valid raster pixel centres."; return; }
        if (draft is not { } a)
        {
            draft = (x, y);
            instructions.Text = "A selected. Click B on either map. Toggle Draw off to cancel.";
            DrawOverlays();
            return;
        }
        try { CommitProfile(a, (x, y)); }
        catch (ArgumentException e) { status.Text = e.Message; return; }
        draft = null;
        draw.IsChecked = false;
        instructions.Text = "Your transect is ready. Hover over the chart to locate matching heights on both maps.";
    }

    void ExampleProfile()
    {
        if (grids == null) return;
        draw.IsChecked = false;
        draft = null;
        var (a, b) = Profiles.Example(grids);
        CommitProfile(a, b);
        instructions.Text = "Example transect loaded. Select Draw transect, then click A and B to compare your own line.";
    }

    void ClearProfile()
    {
        draw.IsChecked = false;
        draft = null;
        chart.Profile = null;
        chart.Hovered = -1;
        chart.InvalidateVisual();
        RefreshStats();
        Hover(-1);
    }

    void Resample()
    {
        if (chart.Profile is not { } p) return;
        try { CommitProfile(p.Start, p.End); }
        catch (ArgumentException e) { status.Text = e.Message; }
    }

    void CommitProfile((double X, double Y) a, (double X, double Y) b)
    {
        chart.Profile = Profiles.Make(grids!, a, b, Spacings[Math.Max(0, spacing.SelectedIndex)]);
        chart.Hovered = -1;
        chart.InvalidateVisual();
        RefreshStats();
        Hover(-1);
    }

    void RefreshStats()
    {
        export.IsEnabled = !busy && chart.Profile != null;
        stats.Text = ProfileText.Stats(chart.Profile);
    }

    void Hover(int index)
    {
        var p = chart.Profile;
        cursor.Text = p != null && index >= 0 && index < p.Samples.Count ? ProfileText.Cursor(p.Samples[index]) : ProfileText.NoCursor;
        DrawOverlays();
    }

    void ExportCsv()
    {
        if (chart.Profile is not { } profile || grids == null) return;
        var dialog = new SaveFileDialog { Title = "Export numeric profile", FileName = "profile.csv", Filter = "CSV (*.csv)|*.csv" };
        if (dialog.ShowDialog(this) != true) return;
        try { Profiles.Export(dialog.FileName, profile, grids, assets.Sha256); }
        catch (Exception e) { MessageBox.Show(this, e.Message, "Export profile", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
        status.Text = "Saved numeric profile CSV and metadata sidecar.";
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

    sealed class ProfileChart(Action<int> hoverChanged) : FrameworkElement
    {
        public TransectProfile? Profile;
        public bool Errors;
        public int Hovered = -1;

        Rect Plot => new(65, 30, Math.Max(1, ActualWidth - 90), Math.Max(1, ActualHeight - 72));

        void SetHovered(int index)
        {
            if (index == Hovered) return;
            Hovered = index;
            hoverChanged(index);
            InvalidateVisual();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var plot = Plot;
            var position = e.GetPosition(this);
            int count = Profile?.Samples.Count ?? 0;
            SetHovered(count == 0 || !plot.Contains(position) ? -1
                : Math.Clamp((int)Math.Round((position.X - plot.Left) / plot.Width * (count - 1)), 0, count - 1));
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            SetHovered(-1);
        }

        protected override void OnRender(DrawingContext dc)
        {
            dc.DrawRectangle(Brushes.White, null, new Rect(RenderSize));
            var ink = Hex("#476174");
            var typeface = new Typeface("Segoe UI");
            double dpi = VisualTreeHelper.GetDpi(this).PixelsPerDip;
            FormattedText Text(string text) => new(text, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, typeface, 12, ink, dpi);
            if (Profile is not { } profile)
            {
                var empty = Text("Draw a transect: choose A, then B on either map.");
                dc.DrawText(empty, new Point((ActualWidth - empty.Width) / 2, (ActualHeight - empty.Height) / 2));
                return;
            }
            var model = ChartModel.From(profile, Errors);
            double low = model.Low, high = model.High;
            var plot = Plot;
            double Y(double v) => plot.Bottom - (v - low) / (high - low) * plot.Height;
            dc.DrawText(Text(Errors ? "Estimate minus reference (m) · zero is closest" : "Source elevation (LN02 metres) · all four rasters on one scale"), new Point(8, 6));
            var gridPen = new Pen(Hex("#e2e9ee"), 1);
            for (int i = 0; i <= 4; i++)
            {
                double t = i / 4.0, y = plot.Bottom - t * plot.Height;
                dc.DrawLine(gridPen, new Point(plot.Left, y), new Point(plot.Right, y));
                var label = Text($"{low + t * (high - low):F1}");
                dc.DrawText(label, new Point(58 - label.Width, y - 8));
                var distance = Text($"{t * profile.Length:F1}");
                dc.DrawText(distance, new Point(plot.Left + t * plot.Width - distance.Width / 2, plot.Bottom + 5));
            }
            var axis = Text("Distance along A → B (projected metres)");
            dc.DrawText(axis, new Point((ActualWidth - axis.Width) / 2, ActualHeight - 21));
            if (Errors)
                dc.DrawLine(new Pen(Hex("#b4bfca"), 1) { DashStyle = DashStyles.Dash }, new Point(plot.Left, Y(0)), new Point(plot.Right, Y(0)));
            foreach (int band in new[] { 0, 3, 1, 2 })
            {
                if (!model.Bands.Contains(band)) continue;
                var geometry = new StreamGeometry();
                using (var context = geometry.Open())
                {
                    for (int i = 0; i < profile.Samples.Count; i++)
                    {
                        var s = profile.Samples[i];
                        var point = new Point(plot.Left + s.Distance / profile.Length * plot.Width, Y(ChartModel.Value(s, band, Errors)));
                        if (i == 0) context.BeginFigure(point, false, false); else context.LineTo(point, true, false);
                    }
                }
                geometry.Freeze();
                var pen = new Pen(Hex(Colors[band]), band == 0 ? 1.5 : 2);
                if (band == 3) pen.DashStyle = DashStyles.Dash;
                dc.DrawGeometry(null, pen, geometry);
            }
            if (Hovered >= 0 && Hovered < profile.Samples.Count)
            {
                double x = plot.Left + profile.Samples[Hovered].Distance / profile.Length * plot.Width;
                dc.DrawLine(new Pen(Hex("#dd3470"), 1) { DashStyle = DashStyles.Dash }, new Point(x, plot.Top), new Point(x, plot.Bottom));
            }
        }
    }
}
