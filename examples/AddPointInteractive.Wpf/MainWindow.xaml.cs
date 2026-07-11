using System.IO;
using System.Drawing;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AddPointInteractive.Wpf;

public partial class MainWindow
{
    private const string PointLayerName = "Clicked Points";

    private int _pointLayerIndex = -1;
    private bool _addPointMode = true;
    private int _displayPointCount;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePointLayer();
        BeginPointEditing();
        SetSampleExtent();
        UpdateStatus("Add Point active. Click the map to add points.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "AddPointInteractive",
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
                "AddPointInteractive",
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
        _pointLayerIndex = viewerControl.AddPointLayer(
            PointLayerName,
            [new GeoKernelPoint(-122.4194, 37.7749)],
            PointStyle());

        _pointLayerIndex = viewerControl.GetLayerInfoByName(PointLayerName)?.Index ?? _pointLayerIndex;
        _displayPointCount = FeatureCount();
    }

    private void BeginPointEditing()
    {
        if (_pointLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_pointLayerIndex))
            viewerControl.BeginEditLayer(_pointLayerIndex);
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _pointLayerIndex)
            return;

        _displayPointCount = FeatureCount();
        RefreshMap();
        UpdateStatus(_addPointMode ? "Point layer updated. Click the map to add points." : "Point layer updated.");
    }

    private void AddPoint_Click(object sender, RoutedEventArgs e)
    {
        _addPointMode = true;
        addPointButton.IsChecked = true;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        BeginPointEditing();
        UpdateStatus("Add Point active. Click the map to add points.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        _addPointMode = false;
        addPointButton.IsChecked = false;
        panButton.IsChecked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan active.");
    }

    private void ClearPoints_Click(object sender, RoutedEventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        viewerControl.RollbackEditLayer(_pointLayerIndex);
        BeginPointEditing();
        _displayPointCount = FeatureCount();
        RefreshMap();
        UpdateStatus("Clicked points cleared.");
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
        pointCountText.Text = $"Point count: {_displayPointCount}";
        statusText.Text = message;
    }

    private int FeatureCount()
    {
        return _pointLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_pointLayerIndex) : 0;
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
            PointSize = 9.0,
            LineWidth = 1.2
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
