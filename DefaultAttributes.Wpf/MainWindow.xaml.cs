using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.DefaultAttributes.Wpf;

public partial class MainWindow
{
    private const string PointLayerName = "Points With Attributes";
    private readonly ObservableCollection<FeatureRow> _rows = [];
    private int _pointLayerIndex = -1;
    private int _pointCursor;
    private bool _infoMode;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        attributesGrid.ItemsSource = _rows;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadWorldLayer())
            return;

        CreatePointLayer();
        BeginPointEditing();
        SetSampleExtent();
        UpdateInfoText("Click Add Point With Attributes, then use Info and click an added point.");
        UpdateStatus("AddPointToEditLayer(index, x, y, attributes) sample.");
    }

    private bool LoadWorldLayer()
    {
        var path = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = WorldStyle()
            }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Title,
                MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            viewerControl.SetLayerName(worldLayer.Index, "World");
        return true;
    }

    private void CreatePointLayer()
    {
        _pointLayerIndex = viewerControl.AddEmptyVectorLayer(PointLayerName, GeoKernelShapeType.Point, PointStyle());
        _pointLayerIndex = viewerControl.GetLayerInfoByName(PointLayerName)?.Index ?? _pointLayerIndex;
        if (_pointLayerIndex < 0)
            MessageBox.Show(this, "Point layer could not be created.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void BeginPointEditing()
    {
        if (_pointLayerIndex < 0)
            return;
        if (!viewerControl.IsLayerEditing(_pointLayerIndex))
            viewerControl.BeginEditLayer(_pointLayerIndex);
        viewerControl.SetActiveEditLayerIndex(_pointLayerIndex);
        UpdateFeatureCount();
    }

    private void AddPoint_Click(object sender, RoutedEventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;
        BeginPointEditing();

        var featureNo = _pointCursor + 1;
        var point = SamplePointAt(_pointCursor);
        var defaults = JsonSerializer.Deserialize<Dictionary<string, object?>>(
            viewerControl.GetDefaultAttributesForLayerJson(_pointLayerIndex)) ?? [];
        var attributes = CreateAttributes(featureNo);
        foreach (var pair in defaults)
            attributes.TryAdd(pair.Key, pair.Value);

        if (!viewerControl.AddPointToEditLayer(_pointLayerIndex, point.X, point.Y, attributes))
        {
            UpdateStatus("Point with attributes could not be added.");
            return;
        }

        _pointCursor++;
        _rows.Add(new FeatureRow(
            featureNo,
            Convert.ToString(attributes["Name"]) ?? "",
            Convert.ToString(attributes["Category"]) ?? "",
            Convert.ToString(attributes["Score"]) ?? "",
            Convert.ToString(attributes["Source"]) ?? ""));
        SelectGridRow(featureNo);
        RefreshMap();
        UpdateFeatureCount();
        UpdateStatus($"GetDefaultAttributesForLayer returned {defaults.Count} fields; point {featureNo} added.");
    }

    private void Info_Click(object sender, RoutedEventArgs e)
    {
        _infoMode = infoButton.IsChecked == true;
        viewerControl.ActiveTool = _infoMode ? GeoKernelViewerTool.Info : GeoKernelViewerTool.Pan;
        UpdateStatus(_infoMode ? "Info mode: click an added point to read attributes." : "Pan mode.");
    }

    private void ClearPoints_Click(object sender, RoutedEventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;
        viewerControl.RollbackEditLayer(_pointLayerIndex);
        _pointCursor = 0;
        _rows.Clear();
        BeginPointEditing();
        RefreshMap();
        UpdateInfoText("Click Add Point With Attributes, then use Info and click an added point.");
        UpdateStatus("Points with attributes cleared.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e) => SetSampleExtent();

    private void ViewerControl_MapMouseUp(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (e.Tool != GeoKernelViewerTool.Info)
            return;
        var result = viewerControl.HitTestTopFeatureAt(e.ScreenPoint.X, e.ScreenPoint.Y, 8);
        if (result is null || !result.IsValid)
        {
            attributesGrid.SelectedItem = null;
            UpdateInfoText("No feature found.");
            UpdateStatus("No feature found under cursor.");
            return;
        }

        UpdateInfoText(FormatFeatureAttributes(result));
        if (result.LayerIndex == _pointLayerIndex)
            SelectGridRow(result.ShapeId);
        UpdateStatus($"Attributes read from layer '{result.LayerName}', feature {result.ShapeId}.");
    }

    private void SelectGridRow(int shapeId)
    {
        var row = _rows.FirstOrDefault(item => item.ShapeId == shapeId);
        attributesGrid.SelectedItem = row;
        if (row is not null)
            attributesGrid.ScrollIntoView(row);
    }

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private void SetSampleExtent() =>
        viewerControl.ViewExtent = new GeoKernelExtent(-130.0, 20.0, -65.0, 55.0);

    private void UpdateFeatureCount()
    {
        var count = _pointLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_pointLayerIndex) : 0;
        pointCountText.Text = $"Feature count: {count}";
    }

    private void UpdateInfoText(string text) => infoTextBox.Text = text;
    private void UpdateStatus(string text) => statusText.Text = text;

    private static GeoKernelPoint SamplePointAt(int index)
    {
        const double xMin = -123.0;
        const double yMin = 29.0;
        const double xStep = 5.0;
        const double yStep = 4.0;
        const int columns = 12;
        return new GeoKernelPoint(xMin + index % columns * xStep, yMin + index / columns * yStep);
    }

    private static Dictionary<string, object?> CreateAttributes(int featureNo) => new()
    {
        ["Name"] = $"Site {featureNo}",
        ["Category"] = featureNo % 2 == 0 ? "Even" : "Odd",
        ["Score"] = featureNo * 10,
        ["Source"] = ".NET Dictionary"
    };

    private static string FormatFeatureAttributes(GeoKernelFeatureHitTestResult result)
    {
        var lines = new List<string>
        {
            $"Layer: {result.LayerName}",
            $"Shape ID: {result.ShapeId}",
            $"Feature ID: {result.FeatureId}",
            ""
        };
        foreach (var pair in result.Attributes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            lines.Add($"{pair.Key} = {pair.Value}");
        return string.Join(Environment.NewLine, lines);
    }

    private static GeoKernelLayerStyle WorldStyle() => new()
    {
        FillColor = "#D8E5E1",
        FillOpacity = 210,
        LineColor = "#6F8883",
        LineWidth = 0.7
    };

    private static GeoKernelLayerStyle PointStyle() => new()
    {
        PointColor = "#D95D39",
        LineColor = "#8C321D",
        PointSize = 9.5,
        LineWidth = 1.2,
        ShowLabels = true,
        LabelField = "Name",
        LabelFontSize = 10.0,
        LabelColor = "#263238",
        LabelHaloEnabled = true,
        LabelHaloColor = "#FFFFFF",
        LabelHaloWidth = 2.0,
        LabelOffsetY = -11.0,
        LabelAllowOverlap = true
    };

    private sealed record FeatureRow(int ShapeId, string Name, string Category, string Score, string Source);
}
