using System.IO;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.InMemoryLayers.Wpf;

public partial class MainWindow
{
    private const string RegionLayerName = "Memory Regions";
    private const string RouteLayerName = "Memory Routes";
    private const string CityLayerName = "Memory Cities";

    private int _regionLayerIndex = -1;
    private int _routeLayerIndex = -1;
    private int _cityLayerIndex = -1;
    private int _pointCursor;
    private int _lineCursor = 1;
    private int _polygonCursor = 1;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadLayer())
            return;

        CreateMemoryLayers();
        RefreshMap();
        SetSampleExtent();
        UpdateStatus("Memory layers created.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "InMemoryLayers",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = new GeoKernelLayerStyle
                    {
                        FillColor = "#D8E5E1",
                        FillOpacity = 210,
                        LineColor = "#6F8883",
                        LineWidth = 0.7
                    }
                }))
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{path}",
                "InMemoryLayers",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(0);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, "World");

        return true;
    }

    private void CreateMemoryLayers()
    {
        viewerControl.AddPolygonLayer(
            RegionLayerName,
            RegionShape(0.0),
            new GeoKernelLayerStyle
            {
                FillColor = "#F1D58A",
                FillOpacity = 150,
                LineColor = "#9A7A1F",
                LineWidth = 1.5
            });

        viewerControl.AddPolylineLayer(
            RouteLayerName,
            RouteShape(0.0),
            new GeoKernelLayerStyle
            {
                LineColor = "#266D8F",
                LineWidth = 2.2
            });

        viewerControl.AddPointLayer(
            CityLayerName,
            [
                new GeoKernelPoint(-122.4194, 37.7749),
                new GeoKernelPoint(-118.2437, 34.0522)
            ],
            new GeoKernelLayerStyle
            {
                PointColor = "#D95F35",
                PointSize = 7.0
            });

        ResolveMemoryLayerIndexes();
        _pointCursor = 0;
        _lineCursor = 1;
        _polygonCursor = 1;
    }

    private void AddPoint_Click(object sender, RoutedEventArgs e)
    {
        var point = GeneratedPoint(_pointCursor);
        if (AddPointToMemoryLayer(_cityLayerIndex, point))
        {
            ++_pointCursor;
            RefreshMap();
            UpdateStatus("Point added.");
        }
        else
        {
            UpdateStatus("Point could not be added.");
        }
    }

    private void AddLine_Click(object sender, RoutedEventArgs e)
    {
        if (AddPolylineToMemoryLayer(_routeLayerIndex, RouteShape(_lineCursor * 2.0)))
        {
            ++_lineCursor;
            RefreshMap();
            UpdateStatus("Line added.");
        }
        else
        {
            UpdateStatus("Line could not be added.");
        }
    }

    private void AddPolygon_Click(object sender, RoutedEventArgs e)
    {
        if (AddPolygonToMemoryLayer(_regionLayerIndex, RegionShape(_polygonCursor * 5.0)))
        {
            ++_polygonCursor;
            RefreshMap();
            UpdateStatus("Polygon added.");
        }
        else
        {
            UpdateStatus("Polygon could not be added.");
        }
    }

    private void ClearMemoryLayers_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.RemoveLayerByName(CityLayerName);
        viewerControl.RemoveLayerByName(RouteLayerName);
        viewerControl.RemoveLayerByName(RegionLayerName);
        CreateMemoryLayers();
        RefreshMap();
        UpdateStatus("Memory layers reset.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private bool AddPointToMemoryLayer(int layerIndex, GeoKernelPoint point)
    {
        if (layerIndex < 0 || !viewerControl.BeginEditLayer(layerIndex))
            return false;

        if (!viewerControl.AddPointToEditLayer(layerIndex, point.X, point.Y))
        {
            viewerControl.RollbackEditLayer(layerIndex);
            return false;
        }

        return viewerControl.CommitEditLayer(layerIndex);
    }

    private bool AddPolylineToMemoryLayer(int layerIndex, IReadOnlyList<GeoKernelPoint> points)
    {
        if (layerIndex < 0 || !viewerControl.BeginEditLayer(layerIndex))
            return false;

        if (!viewerControl.AddPolylineToEditLayer(layerIndex, points))
        {
            viewerControl.RollbackEditLayer(layerIndex);
            return false;
        }

        return viewerControl.CommitEditLayer(layerIndex);
    }

    private bool AddPolygonToMemoryLayer(int layerIndex, IReadOnlyList<GeoKernelPoint> points)
    {
        if (layerIndex < 0 || !viewerControl.BeginEditLayer(layerIndex))
            return false;

        if (!viewerControl.AddPolygonToEditLayer(layerIndex, points))
        {
            viewerControl.RollbackEditLayer(layerIndex);
            return false;
        }

        return viewerControl.CommitEditLayer(layerIndex);
    }

    private void ResolveMemoryLayerIndexes()
    {
        _regionLayerIndex = viewerControl.GetLayerInfoByName(RegionLayerName)?.Index ?? -1;
        _routeLayerIndex = viewerControl.GetLayerInfoByName(RouteLayerName)?.Index ?? -1;
        _cityLayerIndex = viewerControl.GetLayerInfoByName(CityLayerName)?.Index ?? -1;
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
        ResolveMemoryLayerIndexes();
        statusText.Text =
            $"{message} Memory features - points: {FeatureCount(_cityLayerIndex)} | lines: {FeatureCount(_routeLayerIndex)} | polygons: {FeatureCount(_regionLayerIndex)}";
    }

    private int FeatureCount(int layerIndex)
    {
        return layerIndex >= 0 ? viewerControl.GetLayerInfo(layerIndex)?.FeatureCount ?? 0 : 0;
    }

    private static GeoKernelPoint GeneratedPoint(int index)
    {
        const int columns = 12;
        const double startX = -124.0;
        const double startY = 25.0;
        const double stepX = 4.8;
        const double stepY = 3.2;

        var column = index % columns;
        var row = index / columns;
        var jitterX = row % 3 * 0.35;
        var jitterY = column % 4 * 0.25;
        return new GeoKernelPoint(startX + column * stepX + jitterX, startY + row * stepY + jitterY);
    }

    private static GeoKernelPoint[] RouteShape(double offset)
    {
        return
        [
            new GeoKernelPoint(-122.4194 + offset, 37.7749),
            new GeoKernelPoint(-118.2437 + offset, 34.0522),
            new GeoKernelPoint(-112.0740 + offset, 33.4484),
            new GeoKernelPoint(-104.9903 + offset, 39.7392)
        ];
    }

    private static GeoKernelPoint[] RegionShape(double offset)
    {
        return
        [
            new GeoKernelPoint(-101.0 + offset, 30.0),
            new GeoKernelPoint(-91.0 + offset, 30.0),
            new GeoKernelPoint(-89.0 + offset, 37.0),
            new GeoKernelPoint(-96.0 + offset, 42.0),
            new GeoKernelPoint(-103.0 + offset, 38.0),
            new GeoKernelPoint(-101.0 + offset, 30.0)
        ];
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
