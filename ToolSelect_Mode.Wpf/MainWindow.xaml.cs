using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace GeoKernel.ToolSelect_Mode.Wpf;

public partial class MainWindow
{
    private readonly ObservableCollection<HitRow> _hitRows = [];
    private readonly ObservableCollection<DetailRow> _details = [];
    private readonly List<GeoKernelFeatureHitTestResult> _hits = [];

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        hitsGrid.ItemsSource = _hitRows;
        attributesGrid.ItemsSource = _details;
        viewerControl.ActiveTool = GeoKernelViewerTool.Select;
        viewerControl.MapSelectionBoxFinished += ViewerControl_MapSelectionBoxFinished;
        if (!LoadSampleLayers()) return;
        ShowEmptyHits();
        SetSampleExtent();
        UpdateStatus("Select mode is active. Drag a box to select and inspect intersecting features.");
    }

    private bool LoadSampleLayers() =>
        AddLayer("world_4326.shp", "World", WorldStyle()) &&
        AddLayer("usa_states.shp", "USA States", StateStyle()) &&
        AddLayer("cities_4326.shp", "Cities", CityStyle());

    private bool AddLayer(string fileName, string displayName, GeoKernelLayerStyle style)
    {
        var path = SampleData.EnsureKnownWpfSampleFile(fileName, this);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "ToolSelect_Mode", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "ToolSelect_Mode", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
        var layer = viewerControl.GetLayerInfo(viewerControl.LayerCount - 1);
        if (layer is not null) viewerControl.SetLayerName(layer.Index, displayName);
        return true;
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    { selectButton.IsChecked = true; panButton.IsChecked = false; viewerControl.ActiveTool = GeoKernelViewerTool.Select; UpdateStatus("Drag a box to select features."); }
    private void Pan_Click(object sender, RoutedEventArgs e)
    { panButton.IsChecked = true; selectButton.IsChecked = false; viewerControl.ActiveTool = GeoKernelViewerTool.Pan; UpdateStatus("Pan mode."); }
    private void ClearSelection_Click(object sender, RoutedEventArgs e)
    { viewerControl.ClearSelectedFeatures(); ShowEmptyHits(); UpdateStatus("Selection cleared."); }
    private void FullExtent_Click(object sender, RoutedEventArgs e) => SetSampleExtent();

    private void ViewerControl_MapSelectionBoxFinished(object? sender, GeoKernelSelectionBoxFinishedEventArgs e)
    {
        viewerControl.SelectFeaturesInScreenRectangle(e.ScreenRectangle, GeoKernelFeatureSelectionMode.Replace);
        var hits = viewerControl.HitTestFeaturesInScreenRectangle(e.ScreenRectangle).Where(hit => hit.IsValid).ToList();
        ShowHits(hits);
        if (hits.Count == 0)
        {
            ShowEmptyAttributes("No features intersect the selection box.");
            UpdateStatus($"No features in screen rect {RectText(e.ScreenRectangle)}.");
            return;
        }
        hitsGrid.SelectedIndex = 0;
        ShowAttributes(hits[0]);
        UpdateStatus($"{hits.Count} feature hit(s), screen rect {RectText(e.ScreenRectangle)}.");
    }

    private void HitsGrid_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        var index = hitsGrid.SelectedIndex;
        if (index < 0 || index >= _hits.Count) return;
        ShowAttributes(_hits[index]);
        UpdateStatus($"Selected row {index + 1}/{_hits.Count}: {_hits[index].LayerName} feature {_hits[index].ShapeId}.");
    }

    private void ShowEmptyHits()
    { _hits.Clear(); _hitRows.Clear(); _hitRows.Add(new("-", "No hits", "-", "-", "-")); ShowEmptyAttributes("Drag a selection box to list matching features."); }

    private void ShowHits(IReadOnlyList<GeoKernelFeatureHitTestResult> hits)
    {
        _hits.Clear(); _hits.AddRange(hits); _hitRows.Clear();
        for (var i = 0; i < _hits.Count; i++)
        {
            var hit = _hits[i];
            _hitRows.Add(new((i + 1).ToString(), hit.LayerName, hit.ShapeId.ToString(), hit.FeatureId.ToString(), hit.ShapeType.ToString()));
        }
        if (_hits.Count == 0) _hitRows.Add(new("-", "No hits", "-", "-", "-"));
    }

    private void ShowEmptyAttributes(string text) { _details.Clear(); _details.Add(new("Hit", text)); }
    private void ShowAttributes(GeoKernelFeatureHitTestResult hit)
    {
        _details.Clear();
        _details.Add(new("Layer", hit.LayerName)); _details.Add(new("Layer index", hit.LayerIndex.ToString()));
        _details.Add(new("Shape id", hit.ShapeId.ToString())); _details.Add(new("Feature id", hit.FeatureId.ToString()));
        _details.Add(new("Shape type", hit.ShapeType.ToString())); _details.Add(new("Extent", ExtentText(hit.Extent)));
        foreach (var pair in hit.Attributes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            _details.Add(new(pair.Key, pair.Value?.ToString() ?? "<null>"));
    }

    private void SetSampleExtent() => viewerControl.ViewExtent = new GeoKernelExtent(-130, 22, -65, 55);
    private void UpdateStatus(string text) => statusText.Text = text;
    private static string RectText(GeoKernelScreenRectangle rect) => $"{rect.Left},{rect.Top} - {rect.Right},{rect.Bottom}";
    private static string ExtentText(GeoKernelExtent extent) => $"({extent.XMin:F6}, {extent.YMin:F6}) - ({extent.XMax:F6}, {extent.YMax:F6})";

    private static GeoKernelLayerStyle WorldStyle() => new() { FillColor="#D8E5E1", FillOpacity=210, LineColor="#708984", LineWidth=.6, SelectedLineColor="#F59E0B", SelectedLineWidth=3 };
    private static GeoKernelLayerStyle StateStyle() => new() { FillColor="#C7DEE7", FillOpacity=155, LineColor="#2D6F8E", LineWidth=1, SelectedLineColor="#F59E0B", SelectedLineWidth=4 };
    private static GeoKernelLayerStyle CityStyle() => new() { PointColor="#D95D39", LineColor="#8C321D", PointSize=8, LineWidth=1, SelectedLineColor="#F59E0B", SelectedLineWidth=4, ShowLabels=true, LabelField="NAME", LabelFontSize=9, LabelColor="#263238", LabelHaloEnabled=true, LabelHaloColor="#FFFFFF", LabelHaloWidth=2 };

    private sealed record HitRow(string Number, string Layer, string ShapeId, string FeatureId, string ShapeType);
    private sealed record DetailRow(string Name, string Value);
}
