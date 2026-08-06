using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AddShapeToEditLayer.Wpf;

public partial class MainWindow
{
    private const string PolygonLayerName = "Programmatic Polygons";
    private int _polygonLayerIndex = -1;
    private int _polygonCursor;

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        var path = SampleData.EnsureWpfSampleFile(
            new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/world_4326.zip"),
            "world_4326.zip", "world_4326", "world_4326.shp", this);
        if (string.IsNullOrWhiteSpace(path)) return;

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions
            { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}",
                Title, MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null) viewerControl.SetLayerName(worldLayer.Index, "World");

        _polygonLayerIndex = viewerControl.AddEmptyVectorLayer(
            PolygonLayerName, GeoKernelShapeType.Polygon, PolygonStyle());
        _polygonLayerIndex = viewerControl.GetLayerInfoByName(PolygonLayerName)?.Index ?? _polygonLayerIndex;
        if (_polygonLayerIndex < 0)
        {
            MessageBox.Show(this, "Programmatic polygon layer could not be created.", Title,
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        BeginPolygonEditing();
        SetSampleExtent();
        UpdateStatus("Click Add Shape to add polygon geometry to the active edit layer.");
    }

    private void AddShape_Click(object sender, RoutedEventArgs e)
    {
        if (_polygonLayerIndex < 0) return;
        BeginPolygonEditing();
        var points = SamplePolygonAt(_polygonCursor);
        if (!viewerControl.AddPolygonToEditLayer(_polygonLayerIndex, points))
        {
            UpdateStatus("Polygon could not be added.");
            return;
        }
        _polygonCursor++;
        RefreshMap();
        UpdatePolygonCount();
        UpdateStatus($"Shape added with AddPolygonToEditLayer({_polygonLayerIndex}, {points.Count} vertices).");
    }

    private void ClearPolygons_Click(object sender, RoutedEventArgs e)
    {
        if (_polygonLayerIndex < 0) return;
        viewerControl.RollbackEditLayer(_polygonLayerIndex);
        _polygonCursor = 0;
        BeginPolygonEditing();
        RefreshMap();
        UpdateStatus("Programmatic polygons cleared.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e) => SetSampleExtent();

    private void BeginPolygonEditing()
    {
        if (_polygonLayerIndex < 0) return;
        if (!viewerControl.IsLayerEditing(_polygonLayerIndex)) viewerControl.BeginEditLayer(_polygonLayerIndex);
        viewerControl.SetActiveEditLayerIndex(_polygonLayerIndex);
        UpdatePolygonCount();
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex == _polygonLayerIndex) UpdatePolygonCount();
    }

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(false, true);
        viewerControl.RefreshLayers();
    }

    private void SetSampleExtent() => viewerControl.ViewExtent = new GeoKernelExtent(-130, 20, -65, 52);
    private void UpdatePolygonCount() => polygonCountText.Text =
        $"Polygon count: {(_polygonLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_polygonLayerIndex) : 0)}";
    private void UpdateStatus(string text) => statusText.Text = text;

    private static IReadOnlyList<GeoKernelPoint> SamplePolygonAt(int index)
    {
        const double startX = -124, startY = 27, xStep = 7.5, yStep = 4.2;
        const int columns = 7;
        var x = startX + index % columns * xStep;
        var y = startY + index / columns * yStep;
        return [new(x, y), new(x + 4.4, y + .2), new(x + 5.6, y + 2.4),
            new(x + 2.3, y + 3.4), new(x - .4, y + 2), new(x, y)];
    }

    private static GeoKernelLayerStyle WorldStyle() => new()
        { FillColor = "#D8E5E1", FillOpacity = 210, LineColor = "#6F8883", LineWidth = .7 };
    private static GeoKernelLayerStyle PolygonStyle() => new()
        { FillColor = "#F2D27A", FillOpacity = 160, LineColor = "#D95D39", LineWidth = 2 };
}
