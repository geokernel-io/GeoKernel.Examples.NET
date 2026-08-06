using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AddPolygonProgrammatic.Wpf;

public partial class MainWindow
{
    private const string PolygonLayerName = "Programmatic Polygons";

    private int _polygonLayerIndex = -1;
    private int _polygonCursor;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePolygonLayer();
        BeginPolygonEditing();
        SetSampleExtent();
        UpdateStatus("Click Add Polygon to call addPolygonToEditLayer(index, points).");
    }

    private bool LoadLayer()
    {
        var path = SampleData.EnsureWpfSampleFile(
            new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/world_4326.zip"),
            "world_4326.zip",
            "world_4326",
            "world_4326.shp",
            this);

        if (string.IsNullOrWhiteSpace(path))
            return false;

        if (!viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = WorldStyle()
                }))
        {
            MessageBox.Show(
                this,
                $"World layer could not be loaded:{Environment.NewLine}{path}",
                "AddPolygonProgrammatic",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            viewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreatePolygonLayer()
    {
        _polygonLayerIndex = viewerControl.AddEmptyVectorLayer(
            PolygonLayerName,
            GeoKernelShapeType.Polygon,
            PolygonStyle());

        _polygonLayerIndex = viewerControl.GetLayerInfoByName(PolygonLayerName)?.Index ?? _polygonLayerIndex;

        if (_polygonLayerIndex < 0)
            MessageBox.Show(this, "Programmatic polygon layer could not be created.", "AddPolygonProgrammatic", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void BeginPolygonEditing()
    {
        if (_polygonLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_polygonLayerIndex))
            viewerControl.BeginEditLayer(_polygonLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_polygonLayerIndex);
        UpdatePolygonCount();
    }

    private void AddPolygon_Click(object sender, RoutedEventArgs e)
    {
        if (_polygonLayerIndex < 0)
            return;

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
        UpdateStatus($"addPolygonToEditLayer({_polygonLayerIndex}, {points.Count} vertices)");
    }

    private void ClearPolygons_Click(object sender, RoutedEventArgs e)
    {
        if (_polygonLayerIndex < 0)
            return;

        viewerControl.RollbackEditLayer(_polygonLayerIndex);
        _polygonCursor = 0;
        BeginPolygonEditing();
        RefreshMap();
        UpdateStatus("Programmatic polygons cleared.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex == _polygonLayerIndex)
            UpdatePolygonCount();
    }

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-130.0, 20.0, -65.0, 52.0);
    }

    private void UpdatePolygonCount()
    {
        var count = _polygonLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_polygonLayerIndex) : 0;
        polygonCountText.Text = $"Polygon count: {count}";
    }

    private void UpdateStatus(string message)
    {
        statusText.Text = message;
    }

    private static IReadOnlyList<GeoKernelPoint> SamplePolygonAt(int index)
    {
        const double startX = -124.0;
        const double startY = 27.0;
        const double xStep = 7.5;
        const double yStep = 4.2;
        const int columns = 7;

        var column = index % columns;
        var row = index / columns;
        var x = startX + column * xStep;
        var y = startY + row * yStep;

        return
        [
            new GeoKernelPoint(x, y),
            new GeoKernelPoint(x + 4.4, y + 0.2),
            new GeoKernelPoint(x + 5.6, y + 2.4),
            new GeoKernelPoint(x + 2.3, y + 3.4),
            new GeoKernelPoint(x - 0.4, y + 2.0),
            new GeoKernelPoint(x, y)
        ];
    }

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 210,
            LineColor = "#6F8883",
            LineWidth = 0.7
        };
    }

    private static GeoKernelLayerStyle PolygonStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#F2D27A",
            FillOpacity = 160,
            LineColor = "#D95D39",
            LineWidth = 2.0
        };
    }

}
