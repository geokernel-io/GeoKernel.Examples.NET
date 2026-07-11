using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AddPolygonInteractive.Wpf;

public partial class MainWindow
{
    private const string PolygonLayerName = "Drawn Polygons";

    private int _polygonLayerIndex = -1;
    private bool _addPolygonMode = true;
    private int _displayPolygonCount;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPolygon;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePolygonLayer();
        BeginPolygonEditing();
        SetSampleExtent();
        UpdateStatus("Add Polygon active. Click vertices, then double-click or press Enter to finish.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "AddPolygonInteractive",
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
                "AddPolygonInteractive",
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
        _displayPolygonCount = FeatureCount();
    }

    private void BeginPolygonEditing()
    {
        if (_polygonLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_polygonLayerIndex))
            viewerControl.BeginEditLayer(_polygonLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_polygonLayerIndex);
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _polygonLayerIndex)
            return;

        _displayPolygonCount = FeatureCount();
        RefreshMap();
        UpdateStatus(_addPolygonMode
            ? "Polygon layer updated. Click vertices, then double-click or press Enter to finish."
            : "Polygon layer updated.");
    }

    private void AddPolygon_Click(object sender, RoutedEventArgs e)
    {
        _addPolygonMode = true;
        addPolygonButton.IsChecked = true;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPolygon;
        BeginPolygonEditing();
        UpdateStatus("Add Polygon active. Click vertices, then double-click or press Enter to finish.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        _addPolygonMode = false;
        addPolygonButton.IsChecked = false;
        panButton.IsChecked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan active.");
    }

    private void ClearPolygons_Click(object sender, RoutedEventArgs e)
    {
        if (_polygonLayerIndex < 0)
            return;

        viewerControl.RollbackEditLayer(_polygonLayerIndex);
        BeginPolygonEditing();
        _displayPolygonCount = FeatureCount();
        RefreshMap();
        UpdateStatus("Drawn polygons cleared.");
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
        polygonCountText.Text = $"Polygon count: {_displayPolygonCount}";
        statusText.Text = message;
    }

    private int FeatureCount()
    {
        return _polygonLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_polygonLayerIndex) : 0;
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
