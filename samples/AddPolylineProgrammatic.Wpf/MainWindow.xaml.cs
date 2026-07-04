using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AddPolylineProgrammatic.Wpf;

public partial class MainWindow
{
    private const string PolylineLayerName = "Programmatic Polylines";

    private int _polylineLayerIndex = -1;
    private int _polylineCursor;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePolylineLayer();
        BeginPolylineEditing();
        SetSampleExtent();
        UpdateStatus("Click Add Polyline to call addPolylineToEditLayer(index, worldPoints).");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "AddPolylineProgrammatic",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

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
                "AddPolylineProgrammatic",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            viewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreatePolylineLayer()
    {
        _polylineLayerIndex = viewerControl.AddEmptyVectorLayer(
            PolylineLayerName,
            GeoKernelShapeType.Polyline,
            PolylineStyle());

        _polylineLayerIndex = viewerControl.GetLayerInfoByName(PolylineLayerName)?.Index ?? _polylineLayerIndex;

        if (_polylineLayerIndex < 0)
            MessageBox.Show(this, "Programmatic polyline layer could not be created.", "AddPolylineProgrammatic", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void BeginPolylineEditing()
    {
        if (_polylineLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_polylineLayerIndex))
            viewerControl.BeginEditLayer(_polylineLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_polylineLayerIndex);
        UpdatePolylineCount();
    }

    private void AddPolyline_Click(object sender, RoutedEventArgs e)
    {
        if (_polylineLayerIndex < 0)
            return;

        BeginPolylineEditing();

        var points = SamplePolylineAt(_polylineCursor);
        if (!viewerControl.AddPolylineToEditLayer(_polylineLayerIndex, points))
        {
            UpdateStatus("Polyline could not be added.");
            return;
        }

        _polylineCursor++;
        RefreshMap();
        UpdatePolylineCount();
        UpdateStatus($"addPolylineToEditLayer({_polylineLayerIndex}, {points.Count} vertices)");
    }

    private void ClearLines_Click(object sender, RoutedEventArgs e)
    {
        if (_polylineLayerIndex < 0)
            return;

        viewerControl.RollbackEditLayer(_polylineLayerIndex);
        _polylineCursor = 0;
        BeginPolylineEditing();
        RefreshMap();
        UpdateStatus("Programmatic polylines cleared.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex == _polylineLayerIndex)
            UpdatePolylineCount();
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

    private void UpdatePolylineCount()
    {
        var count = _polylineLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_polylineLayerIndex) : 0;
        polylineCountText.Text = $"Polyline count: {count}";
    }

    private void UpdateStatus(string message)
    {
        statusText.Text = message;
    }

    private static IReadOnlyList<GeoKernelPoint> SamplePolylineAt(int index)
    {
        const double startX = -124.0;
        const double startY = 29.0;
        const double xStep = 7.0;
        const double yStep = 3.0;
        const int columns = 7;

        var column = index % columns;
        var row = index / columns;
        var x = startX + column * xStep;
        var y = startY + row * yStep;

        return
        [
            new GeoKernelPoint(x, y),
            new GeoKernelPoint(x + 2.2, y + 1.4),
            new GeoKernelPoint(x + 4.8, y + 0.4),
            new GeoKernelPoint(x + 6.4, y + 2.2)
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

    private static GeoKernelLayerStyle PolylineStyle()
    {
        return new GeoKernelLayerStyle
        {
            LineColor = "#D95D39",
            LineWidth = 2.6
        };
    }

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
}
