using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.WorldTolerance.Wpf;

public partial class MainWindow
{
    private readonly ObservableCollection<HitRow> _hitRows = [];
    private readonly ObservableCollection<DetailRow> _details = [];
    private readonly List<GeoKernelFeatureHitTestResult> _hits = [];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        hitsGrid.ItemsSource = _hitRows;
        attributesGrid.ItemsSource = _details;        
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!LoadSampleLayers())
            return;

        ShowEmptyHits();
        SetSampleExtent();
        UpdateStatus("Click the map to call HitTestFeatures(worldX, worldY, worldTolerance).");
    }

    private bool LoadSampleLayers()
    {
        return AddLayer("world_4326.shp", "World", WorldStyle())
            && AddLayer("usa_states_4326.shp", "USA States", StateStyle())
            && AddLayer("cities_4326.shp", "Cities", CityStyle());
    }

    private bool AddLayer(string fileName, string displayName, GeoKernelLayerStyle style)
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", fileName);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "WorldTolerance", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "WorldTolerance", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(0);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void Identify_Click(object sender, RoutedEventArgs e)
    {
        identifyButton.IsChecked = true;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;
        UpdateStatus("Click a world point to search using the selected world tolerance.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        panButton.IsChecked = true;
        identifyButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan mode.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ToleranceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (toleranceText is not null)
            toleranceText.Text = $"{e.NewValue:F2} deg";
    }

    private void ViewerControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (viewerControl.ActiveTool != GeoKernelViewerTool.Info)
            return;

        var position = e.GetPosition(viewerControl);
        var worldPoint = viewerControl.ScreenToWorld(position.X, position.Y);
        var tolerance = toleranceSlider.Value;
        var hits = viewerControl.HitTestFeatures(worldPoint.X, worldPoint.Y, tolerance)
            .Where(hit => hit.IsValid)
            .ToList();

        ShowHits(hits);
        if (hits.Count == 0)
        {
            viewerControl.ClearSelectedFeatures();
            ShowEmptyAttributes("No feature inside world tolerance.");
            UpdateStatus($"No feature hit at {worldPoint.X:F6}, {worldPoint.Y:F6} with tolerance {tolerance:F2} deg.");
            return;
        }

        hitsGrid.SelectedIndex = 0;
        ShowAttributes(hits[0]);
        UpdateStatus($"{hits.Count} feature hit(s) at {worldPoint.X:F6}, {worldPoint.Y:F6} with tolerance {tolerance:F2} deg.");
    }

    private void HitsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var index = hitsGrid.SelectedIndex;
        if (index < 0 || index >= _hits.Count)
            return;

        ShowAttributes(_hits[index]);
        UpdateStatus($"Selected hit {index + 1}/{_hits.Count}: {_hits[index].LayerName} feature {_hits[index].ShapeId}.");
    }

    private void ShowEmptyHits()
    {
        _hits.Clear();
        _hitRows.Clear();
        _hitRows.Add(new HitRow("-", "No hits", "-", "-"));
        ShowEmptyAttributes("Click the map to inspect features inside world tolerance.");
    }

    private void ShowHits(IReadOnlyList<GeoKernelFeatureHitTestResult> hits)
    {
        _hits.Clear();
        _hits.AddRange(hits);
        _hitRows.Clear();

        for (var i = 0; i < _hits.Count; i++)
        {
            var hit = _hits[i];
            _hitRows.Add(new HitRow((i + 1).ToString(), hit.LayerName, hit.ShapeId.ToString(), hit.ShapeType.ToString()));
        }

        if (_hits.Count == 0)
            _hitRows.Add(new HitRow("-", "No hits", "-", "-"));
    }

    private void ShowEmptyAttributes(string text)
    {
        _details.Clear();
        _details.Add(new DetailRow("Hit", text));
    }

    private void ShowAttributes(GeoKernelFeatureHitTestResult hit)
    {
        _details.Clear();
        _details.Add(new DetailRow("Layer", hit.LayerName));
        _details.Add(new DetailRow("Layer index", hit.LayerIndex.ToString()));
        _details.Add(new DetailRow("Shape id", hit.ShapeId.ToString()));
        _details.Add(new DetailRow("Feature id", hit.FeatureId.ToString()));
        _details.Add(new DetailRow("Shape type", hit.ShapeType.ToString()));
        _details.Add(new DetailRow("World point", $"{hit.WorldPoint.X:F6}, {hit.WorldPoint.Y:F6}"));
        _details.Add(new DetailRow("Extent", ExtentText(hit.Extent)));

        foreach (var pair in hit.Attributes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            _details.Add(new DetailRow(pair.Key, pair.Value?.ToString() ?? "<null>"));
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
        statusText.Text = text;
    }

    private static string ExtentText(GeoKernelExtent extent)
    {
        return $"({extent.XMin:F6}, {extent.YMin:F6}) - ({extent.XMax:F6}, {extent.YMax:F6})";
    }

    private static GeoKernelLayerStyle WorldStyle() => new()
    {
        FillColor = "#D8E5E1",
        FillOpacity = 210,
        LineColor = "#708984",
        LineWidth = 0.6,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 3.0
    };

    private static GeoKernelLayerStyle StateStyle() => new()
    {
        FillColor = "#C7DEE7",
        FillOpacity = 155,
        LineColor = "#2D6F8E",
        LineWidth = 1.0,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 4.0
    };

    private static GeoKernelLayerStyle CityStyle() => new()
    {
        PointColor = "#D95D39",
        LineColor = "#8C321D",
        PointSize = 8.0,
        LineWidth = 1.0,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 4.0,
        ShowLabels = true,
        LabelField = "NAME",
        LabelFontSize = 9.0,
        LabelColor = "#263238",
        LabelHaloEnabled = true,
        LabelHaloColor = "#FFFFFF",
        LabelHaloWidth = 2.0
    };

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "assets", "data")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }

    private sealed record HitRow(string Number, string Layer, string ShapeId, string ShapeType);
    private sealed record DetailRow(string Name, string Value);
}
