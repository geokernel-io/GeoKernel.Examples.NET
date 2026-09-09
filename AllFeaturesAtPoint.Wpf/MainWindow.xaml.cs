using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AllFeaturesAtPoint.Wpf;

public partial class MainWindow
{
    private readonly ObservableCollection<HitRow> _hitRows = [];
    private readonly ObservableCollection<DetailRow> _details = [];
    private readonly List<GeoKernelFeatureHitTestResult> _hits = [];
    private readonly Dictionary<string, string> _samplePaths = new(StringComparer.OrdinalIgnoreCase);

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        hitsGrid.ItemsSource = _hitRows;
        attributesGrid.ItemsSource = _details;
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!PrepareSampleData() || !LoadSampleLayers())
            return;

        ShowEmptyHits();
        SetSampleExtent();
        UpdateStatus("Click the map to run HitTestFeaturesAt(screenX, screenY, 8).");
    }

    private bool PrepareSampleData()
    {
        var world = EnsureSample("world_4326.zip", "world_4326", "world_4326.shp");
        var states = EnsureSample("usa_states.zip", "usa_states", "usa_states.shp");
        var cities = EnsureSample("cities_4326.zip", "cities_4326", "cities_4326.shp");
        if (string.IsNullOrWhiteSpace(world) || string.IsNullOrWhiteSpace(states) || string.IsNullOrWhiteSpace(cities))
            return false;

        _samplePaths["world_4326.shp"] = world;
        _samplePaths["usa_states.shp"] = states;
        _samplePaths["cities_4326.shp"] = cities;
        return true;
    }

    private string EnsureSample(string archiveName, string folderName, string fileName) =>
        SampleData.EnsureWpfSampleFile(
            new Uri($"https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/{archiveName}"),
            archiveName, folderName, fileName, this);

    private bool LoadSampleLayers()
    {
        return AddLayer("world_4326.shp", "World", WorldStyle())
            && AddLayer("usa_states.shp", "USA States", StateStyle())
            && AddLayer("cities_4326.shp", "Cities", CityStyle());
    }

    private bool AddLayer(string fileName, string displayName, GeoKernelLayerStyle style)
    {
        if (!_samplePaths.TryGetValue(fileName, out var path))
            return false;

        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "AllFeaturesAtPoint", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "AllFeaturesAtPoint", MessageBoxButton.OK, MessageBoxImage.Error);
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
        toolStateText.Text = "Tool: hitTestFeaturesAt";
        UpdateStatus("Click a point to list all feature hits.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        panButton.IsChecked = true;
        identifyButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        toolStateText.Text = "Tool: Pan";
        UpdateStatus("Pan mode.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_MapMouseUp(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (e.Tool != GeoKernelViewerTool.Info)
            return;

        var hits = viewerControl.HitTestFeaturesAt(e.ScreenPoint.X, e.ScreenPoint.Y, 8)
            .Where(hit => hit.IsValid)
            .ToList();

        ShowHits(hits);
        if (hits.Count == 0)
        {
            viewerControl.ClearSelectedFeatures();
            ShowEmptyAttributes("No feature at clicked point.");
            UpdateStatus("No feature hit.");
            return;
        }

        hitsGrid.SelectedIndex = 0;
        ShowAttributes(hits[0]);
        UpdateStatus($"{hits.Count} feature hit(s) at the clicked point.");
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
        ShowEmptyAttributes("Click the map to inspect all feature hits.");
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
        _details.Add(new DetailRow("Provider feature", hit.IsProviderFeature.ToString()));
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

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 210,
            LineColor = "#708984",
            LineWidth = 0.6,
            SelectedLineColor = "#F59E0B",
            SelectedLineWidth = 3.0
        };
    }

    private static GeoKernelLayerStyle StateStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#C7DEE7",
            FillOpacity = 160,
            LineColor = "#2D6F8E",
            LineWidth = 1.0,
            SelectedLineColor = "#F59E0B",
            SelectedLineWidth = 4.0
        };
    }

    private static GeoKernelLayerStyle CityStyle()
    {
        return new GeoKernelLayerStyle
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
    }

    private sealed record HitRow(string Number, string Layer, string ShapeId, string ShapeType);
    private sealed record DetailRow(string Name, string Value);
}
