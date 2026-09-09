using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AddPointProgrammatic.Wpf;

public partial class MainWindow
{
    private const string PointLayerName = "Programmatic Points";

    private int _pointLayerIndex = -1;
    private int _pointCursor;

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

        CreatePointLayer();
        BeginPointEditing();
        SetSampleExtent();
        UpdateStatus("Click Add Point to call addPointToEditLayer(index, worldPoint).");
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
                "AddPointProgrammatic",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            viewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreatePointLayer()
    {
        _pointLayerIndex = viewerControl.AddEmptyVectorLayer(
            PointLayerName,
            GeoKernelShapeType.Point,
            PointStyle());

        _pointLayerIndex = viewerControl.GetLayerInfoByName(PointLayerName)?.Index ?? _pointLayerIndex;

        if (_pointLayerIndex < 0)
            MessageBox.Show(this, "Programmatic point layer could not be created.", "AddPointProgrammatic", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void BeginPointEditing()
    {
        if (_pointLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_pointLayerIndex))
            viewerControl.BeginEditLayer(_pointLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_pointLayerIndex);
        UpdatePointCount();
    }

    private void AddPoint_Click(object sender, RoutedEventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        BeginPointEditing();

        var point = SamplePointAt(_pointCursor);
        if (!viewerControl.AddPointToEditLayer(_pointLayerIndex, point.X, point.Y))
        {
            UpdateStatus("Point could not be added.");
            return;
        }

        _pointCursor++;
        RefreshMap();
        UpdatePointCount();
        UpdateStatus($"addPointToEditLayer({_pointLayerIndex}, {point.X:F4}, {point.Y:F4})");
    }

    private void ClearPoints_Click(object sender, RoutedEventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        viewerControl.RollbackEditLayer(_pointLayerIndex);
        _pointCursor = 0;
        BeginPointEditing();
        RefreshMap();
        UpdateStatus("Programmatic points cleared.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex == _pointLayerIndex)
            UpdatePointCount();
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

    private void UpdatePointCount()
    {
        var count = _pointLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_pointLayerIndex) : 0;
        pointCountText.Text = $"Point count: {count}";
    }

    private void UpdateStatus(string message)
    {
        statusText.Text = message;
    }

    private static GeoKernelPoint SamplePointAt(int index)
    {
        const double xMin = -124.0;
        const double yMin = 26.0;
        const double xStep = 1.9;
        const double yStep = 1.8;
        const int columns = 29;
        const int rows = 13;

        var cell = index % (columns * rows);
        var column = (cell * 7) % columns;
        var row = ((cell / columns) + (cell * 11)) % rows;

        return new GeoKernelPoint(xMin + column * xStep, yMin + row * yStep);
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

    private static GeoKernelLayerStyle PointStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            LineColor = "#8C321D",
            PointSize = 9.5,
            LineWidth = 1.2
        };
    }

}
