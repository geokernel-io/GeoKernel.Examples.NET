using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ShortestRoute.Wpf;

public sealed class MainWindow : Window
{
    private readonly GeoKernelViewerControl _viewer = new(); private readonly ListBox _directions = new();
    private readonly TextBlock _summary = new() { Text = "Select a start and finish point.", TextWrapping = TextWrapping.Wrap }; private readonly TextBlock _status = new();
    private readonly Button _selectButton = new() { Content = "Select route points", IsEnabled = false }; private readonly RouteOverlayWindow _overlay;
    private ShortestRoutingEngine? _engine; private RoutePoint? _startPoint, _finishPoint; private int _startNode = -1; private ShortestRouteResult? _route;

    public MainWindow()
    {
        Title = "ShortestRoute"; Width = 1200; Height = 760; WindowStartupLocation = WindowStartupLocation.CenterScreen; Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Images/GeoKernelAppIcon.ico"));
        var root = new DockPanel(); Content = root; var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Height = 34 }; toolbar.Children.Add(_selectButton);
        var legend = new TextBlock { VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(8, 0, 0, 0) };
        legend.Inlines.Add(new Run("●") { Foreground = new SolidColorBrush(Color.FromRgb(34, 197, 94)) }); legend.Inlines.Add(new Run(" Start    ")); legend.Inlines.Add(new Run("●") { Foreground = new SolidColorBrush(Color.FromRgb(239, 68, 68)) }); legend.Inlines.Add(new Run(" Finish")); toolbar.Children.Add(legend);
        DockPanel.SetDock(toolbar, Dock.Top); root.Children.Add(toolbar); DockPanel.SetDock(_status, Dock.Bottom); root.Children.Add(_status);
        var side = new Grid { Width = 300, Margin = new Thickness(10) }; side.RowDefinitions.Add(new RowDefinition { Height = new GridLength(22) }); side.RowDefinitions.Add(new RowDefinition { Height = new GridLength(65) }); side.RowDefinitions.Add(new RowDefinition { Height = new GridLength(26) }); side.RowDefinitions.Add(new RowDefinition());
        Add(side, new TextBlock { Text = "Shortest route", FontWeight = FontWeights.Bold }, 0); Add(side, _summary, 1); Add(side, new TextBlock { Text = "Road directions", FontWeight = FontWeights.Bold, VerticalAlignment = VerticalAlignment.Bottom }, 2); Add(side, _directions, 3);
        DockPanel.SetDock(side, Dock.Right); root.Children.Add(side); root.Children.Add(_viewer); _overlay = new RouteOverlayWindow(_viewer);
        Loaded += LoadSample; LocationChanged += (_, _) => _overlay.SyncBounds(); SizeChanged += (_, _) => _overlay.SyncBounds(); StateChanged += (_, _) => _overlay.SyncBounds(); Closed += (_, _) => _overlay.Close();
        _viewer.VisibleExtentChanged += (_, _) => _overlay.Redraw(); _viewer.MapMouseUp += MapClicked; _selectButton.Click += (_, _) => BeginSelection();
    }
    private static void Add(Grid grid, UIElement element, int row) { Grid.SetRow(element, row); grid.Children.Add(element); }

    private async void LoadSample(object sender, RoutedEventArgs e)
    {
        Mouse.OverrideCursor = Cursors.Wait; _status.Text = "Loading Stockholm road network..."; await Task.Yield();
        try
        {
            _viewer.ActiveTool = GeoKernelViewerTool.Pan; var path = SampleData.EnsureWpfSampleFile(new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/stockholm.zip"), "stockholm.zip", "stockholm", "stockholm.shp", this);
            if (string.IsNullOrEmpty(path) || !_viewer.AddLayerFile(path)) return; _viewer.SetLayerCoordinateSystemPreset(0, GeoKernelCoordinateSystemPreset.Wgs84); _viewer.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.WebMercator); _viewer.SetLayerStyle(0, new GeoKernelLayerStyle { LineColor = "#718684", LineWidth = 1 });
            if (!_viewer.BuildRoutingGraphForLayer(0, 1e-6, true, "maxspeed", "name", "oneway", 50)) throw new InvalidOperationException("Routing graph could not be built.");
            _status.Text = "Preparing routing graph..."; var snapshot = await Task.Run(() => _viewer.GetRoutingGraphSnapshot()) ?? throw new InvalidOperationException("Routing graph is unavailable.");
            _engine = await Task.Run(() => new ShortestRoutingEngine(
                snapshot.Nodes.Select(node => new RouteNode(node.Id, new RoutePoint(node.Position.X, node.Position.Y))),
                snapshot.Edges.Select(edge => new RouteEdge(edge.Id, edge.FromId, edge.ToId, edge.Distance, edge.SpeedKmh,
                    edge.Geometry.Select(point => new RoutePoint(point.X, point.Y)).ToArray(), edge.Attributes.TryGetValue("name", out var name) ? name?.ToString() ?? "" : ""))));
            var extent = _viewer.GetLayerInfo(0)?.ProjectedExtent; if (extent is not null) _viewer.ViewExtent = extent.Value; _selectButton.IsEnabled = true; _overlay.Owner = this; _overlay.Show(); BeginSelection();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { Mouse.OverrideCursor = null; }
    }
    private void BeginSelection() { _startPoint = _finishPoint = null; _startNode = -1; _route = null; _directions.Items.Clear(); _summary.Text = "Select a start and finish point."; _overlay.SetState(null, null, null); _viewer.ActiveTool = GeoKernelViewerTool.Route; _status.Text = "Click the map to choose the start point."; }
    private void MapClicked(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (_engine is null || e.Tool != GeoKernelViewerTool.Route || (e.ButtonOrButtons & 1) == 0) return; var source = ShortestRoutingEngine.ToWgs84(new RoutePoint(e.WorldPoint.X, e.WorldPoint.Y)); var snapped = _engine.NearestNode(source, 2000);
        if (snapped is null) { MessageBox.Show(this, "No road node was found near the selected point.", Title, MessageBoxButton.OK, MessageBoxImage.Warning); return; } var world = ShortestRoutingEngine.ToWebMercator(snapped.Position);
        if (!_startPoint.HasValue || _finishPoint.HasValue) { _startPoint = world; _finishPoint = null; _startNode = snapped.Id; _route = null; _directions.Items.Clear(); _summary.Text = "Select the finish point."; _overlay.SetState(_startPoint, null, null); _status.Text = "Start selected. Click the map to choose the finish point."; return; }
        _finishPoint = world; _route = _engine.FindRoute(_startNode, snapped.Id); if (_route is null) { _overlay.SetState(_startPoint, _finishPoint, null); MessageBox.Show(this, "No connected route was found.", Title, MessageBoxButton.OK, MessageBoxImage.Warning); _status.Text = "No connected route found. Click once to choose a new start."; return; }
        _summary.Text = $"{_route.Distance / 1000:0.00} km  •  {_route.Time / 60:0.0} min"; var steps = _engine.RoadSteps(_route); for (var i = 0; i < steps.Count; i++) _directions.Items.Add($"{i + 1}. {steps[i].Name}\n    {(steps[i].Distance >= 1000 ? $"{steps[i].Distance / 1000:0.0} km" : $"{steps[i].Distance:0} m")}"); _overlay.SetState(_startPoint, _finishPoint, _route); _status.Text = $"Route: {_route.Distance / 1000:0.00} km, {_route.Time / 60:0.0} min";
    }

    private sealed class RouteOverlayWindow : Window
    {
        private readonly GeoKernelViewerControl _viewer; private readonly OverlayDrawing _drawing;
        public RouteOverlayWindow(GeoKernelViewerControl viewer) { _viewer = viewer; _drawing = new OverlayDrawing(viewer); Content = _drawing; WindowStyle = WindowStyle.None; AllowsTransparency = true; Background = Brushes.Transparent; ShowInTaskbar = false; ResizeMode = ResizeMode.NoResize; SourceInitialized += (_, _) => { var h = new WindowInteropHelper(this).Handle; SetWindowLong(h, -20, GetWindowLong(h, -20) | 0x20 | 0x08000000 | 0x80); }; }
        protected override void OnActivated(EventArgs e) { base.OnActivated(e); Owner?.Activate(); }
        public void SyncBounds() { if (!IsVisible || !_viewer.IsVisible) return; var p = _viewer.PointToScreen(new Point()); Left = p.X; Top = p.Y; Width = _viewer.ActualWidth; Height = _viewer.ActualHeight; Redraw(); }
        public void SetState(RoutePoint? start, RoutePoint? finish, ShortestRouteResult? route) { _drawing.SetState(start, finish, route); SyncBounds(); } public void Redraw() => _drawing.InvalidateVisual();
        [DllImport("user32.dll")] private static extern int GetWindowLong(IntPtr window, int index); [DllImport("user32.dll")] private static extern int SetWindowLong(IntPtr window, int index, int value);
    }
    private sealed class OverlayDrawing : FrameworkElement
    {
        private readonly GeoKernelViewerControl _viewer; private RoutePoint? _start, _finish; private ShortestRouteResult? _route;
        public OverlayDrawing(GeoKernelViewerControl viewer) { _viewer = viewer; IsHitTestVisible = false; } public void SetState(RoutePoint? start, RoutePoint? finish, ShortestRouteResult? route) { _start = start; _finish = finish; _route = route; InvalidateVisual(); }
        protected override void OnRender(DrawingContext dc) { if (_route is not null && _route.WorldGeometry.Count > 1) { var geometry = new StreamGeometry(); using (var context = geometry.Open()) { context.BeginFigure(Screen(_route.WorldGeometry[0]), false, false); context.PolyLineTo(_route.WorldGeometry.Skip(1).Select(Screen).ToArray(), true, true); } dc.DrawGeometry(null, new Pen(new SolidColorBrush(Color.FromRgb(239, 68, 68)), 4) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round, LineJoin = PenLineJoin.Round }, geometry); } DrawMarker(dc, _start, Color.FromRgb(34, 197, 94), Color.FromRgb(20, 83, 45)); DrawMarker(dc, _finish, Color.FromRgb(239, 68, 68), Color.FromRgb(127, 29, 29)); }
        private void DrawMarker(DrawingContext dc, RoutePoint? point, Color fill, Color outline) { if (point.HasValue) dc.DrawEllipse(new SolidColorBrush(fill), new Pen(new SolidColorBrush(outline), 2), Screen(point.Value), 8, 8); } private Point Screen(RoutePoint point) { var p = _viewer.WorldToScreen(point.X, point.Y); return new Point(p.X, p.Y); }
    }
}
