using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AddPolylineInteractive.Wpf;

public partial class MainWindow
{
    private const string PolylineLayerName = "Drawn Polylines";

    private int _polylineLayerIndex = -1;
    private bool _addPolylineMode = true;
    private int _displayPolylineCount;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePolylineLayer();
        BeginPolylineEditing();
        SetSampleExtent();
        UpdateStatus("Add Polyline active. Click vertices, then double-click or press Enter to finish.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "AddPolylineInteractive",
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
                "AddPolylineInteractive",
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
        _displayPolylineCount = FeatureCount();
    }

    private void BeginPolylineEditing()
    {
        if (_polylineLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_polylineLayerIndex))
            viewerControl.BeginEditLayer(_polylineLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_polylineLayerIndex);
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _polylineLayerIndex)
            return;

        _displayPolylineCount = FeatureCount();
        RefreshMap();
        UpdateStatus(_addPolylineMode
            ? "Polyline layer updated. Click vertices, then double-click or press Enter to finish."
            : "Polyline layer updated.");
    }

    private void AddPolyline_Click(object sender, RoutedEventArgs e)
    {
        _addPolylineMode = true;
        addPolylineButton.IsChecked = true;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        BeginPolylineEditing();
        UpdateStatus("Add Polyline active. Click vertices, then double-click or press Enter to finish.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        _addPolylineMode = false;
        addPolylineButton.IsChecked = false;
        panButton.IsChecked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan active.");
    }

    private void ClearLines_Click(object sender, RoutedEventArgs e)
    {
        if (_polylineLayerIndex < 0)
            return;

        viewerControl.RollbackEditLayer(_polylineLayerIndex);
        BeginPolylineEditing();
        _displayPolylineCount = FeatureCount();
        RefreshMap();
        UpdateStatus("Drawn polylines cleared.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
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

    private void UpdateStatus(string message)
    {
        polylineCountText.Text = $"Polyline count: {_displayPolylineCount}";
        statusText.Text = message;
    }

    private int FeatureCount()
    {
        return _polylineLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_polylineLayerIndex) : 0;
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
