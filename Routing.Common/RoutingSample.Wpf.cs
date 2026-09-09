using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using GeoKernel.NET.Wpf.Controls;
using GeoKernel.Examples.Common;

namespace GeoKernel.Examples.Routing.Wpf;

public class RoutingSampleWindow : Window
{
    private readonly GeoKernelViewerControl _viewer = new();
    private readonly System.Windows.Controls.ListBox _routes = new(), _directions = new();
    private readonly TextBlock _summary = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(6) };
    private readonly TextBlock _status = new();
    private readonly List<GeoKernelPoint> _points = [];
    private readonly Button _routeButton = new() { Content = "New route" };
    private readonly string _sample = System.Reflection.Assembly.GetEntryAssembly()!.GetName().Name!.Replace(".Wpf", "");

    public RoutingSampleWindow()
    {
        Title = _sample; Width = 1200; Height = 800; WindowStartupLocation = WindowStartupLocation.CenterScreen;
        Icon = BitmapFrame.Create(new Uri("pack://application:,,,/Images/GeoKernelAppIcon.ico", UriKind.Absolute));
        var root = new DockPanel(); Content = root;
        var bar = new StackPanel { Orientation = Orientation.Horizontal, Height = 34 };
        var calculate = new Button { Content = "Calculate route" };
        if (_sample.Contains("AlternativeRoutes", StringComparison.OrdinalIgnoreCase))
        {
            _routeButton.Content = "Select route points";
            calculate.Visibility = Visibility.Collapsed;
        }
        bar.Children.Add(_routeButton); bar.Children.Add(calculate); bar.Children.Add(new TextBlock { Text = "  ● Start   ● Stop   ● Finish", VerticalAlignment = VerticalAlignment.Center });
        DockPanel.SetDock(bar, Dock.Top); root.Children.Add(bar); DockPanel.SetDock(_status, Dock.Bottom); root.Children.Add(_status);
        var panel = new Grid { Width = 310 }; panel.RowDefinitions.Add(new RowDefinition { Height = new GridLength(80) });
        panel.RowDefinitions.Add(new RowDefinition()); panel.RowDefinitions.Add(new RowDefinition());
        Grid.SetRow(_summary, 0); Grid.SetRow(_routes, 1); Grid.SetRow(_directions, 2); panel.Children.Add(_summary); panel.Children.Add(_routes); panel.Children.Add(_directions);
        DockPanel.SetDock(panel, Dock.Right); root.Children.Add(panel); root.Children.Add(_viewer);
        Loaded += OnLoaded; _viewer.MapMouseUp += OnMapClicked; _routeButton.Click += (_, _) => BeginRouteSelection(); calculate.Click += (_, _) => Calculate();
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _viewer.ActiveTool = GeoKernelViewerTool.Pan;
            var path = SampleData.EnsureWpfSampleFile(
                new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/stockholm.zip"),
                "stockholm.zip", "stockholm", "stockholm.shp", this);
            if (string.IsNullOrWhiteSpace(path) || !_viewer.AddLayerFile(path)) throw new InvalidOperationException("Stockholm road layer could not be loaded.");
            _viewer.SetLayerCoordinateSystemPreset(0, GeoKernelCoordinateSystemPreset.Wgs84);
            _viewer.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.WebMercator);
            _viewer.SetLayerStyle(0, new GeoKernelLayerStyle { LineColor = "#718684", LineWidth = 1 });
            if (!_viewer.BuildRoutingGraphForLayer(0, .000001, true, "maxspeed", "name", "oneway", 50)) throw new InvalidOperationException("Routing graph could not be built.");
            var stockholmExtent = _viewer.GetLayerInfo(0)?.ProjectedExtent;
            if (stockholmExtent is not null) _viewer.ViewExtent = stockholmExtent.Value;
            else _viewer.FullExtent();
            BeginRouteSelection();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, _sample, MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void Reset() { while (_viewer.LayerCount > 1) _viewer.RemoveLayer(0); _points.Clear(); _routes.Items.Clear(); _directions.Items.Clear(); _summary.Text = "Select route points on the map."; }
    private void BeginRouteSelection()
    {
        Reset();
        _viewer.ActiveTool = GeoKernelViewerTool.Route;
        _status.Text = "Click the map to choose the start point.";
    }
    private void OnMapClicked(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if ((e.ButtonOrButtons & 1) == 0 || e.Tool != GeoKernelViewerTool.Route) return;
        if (!_sample.Contains("MultiStop") && !_sample.Contains("Optimization") && _points.Count == 2) Reset();
        _points.Add(e.WorldPoint); _status.Text = $"Point {_points.Count} selected.";
        if (!_sample.Contains("MultiStop") && !_sample.Contains("Optimization") && _points.Count == 2) Calculate();
    }
    private void Calculate()
    {
        if (_points.Count < 2) return; while (_viewer.LayerCount > 1) _viewer.RemoveLayer(0); _routes.Items.Clear(); _directions.Items.Clear();
        double distance = 0, time = 0;
        var succeeded = 0;
        for (var i = 1; i < _points.Count; i++)
        {
            var r = _viewer.AddShortestRouteLayerBetweenPoints(_points[i - 1], _points[i], GeoKernelRoutingCostMetric.TravelTime, double.PositiveInfinity, 50, $"Route {i}", false);
            if (!r.Succeeded) { _routes.Items.Add($"Leg {i}: no connected route"); continue; }
            succeeded++;
            distance += r.TotalDistance; time += r.TotalTime; _routes.Items.Add($"Leg {i}: {r.TotalDistance / 1000:0.00} km • {r.TotalTime / 60:0.0} min");
            _directions.Items.Add($"{i}. Route segment • {r.TotalDistance:0} m"); _viewer.SetLayerStyle(0, new GeoKernelLayerStyle { LineColor = "#2563EB", LineWidth = 4 });
        }
        if (succeeded == 0)
        {
            _summary.Text = "No connected route was found.";
            _status.Text = "No connected route was found. Select a new start point.";
            return;
        }
        _summary.Text = $"{_sample}\n{distance / 1000:0.00} km • {time / 60:0.0} min"; _status.Text = "Route calculated successfully.";
    }

}
