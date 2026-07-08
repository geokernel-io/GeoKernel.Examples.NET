using System.Drawing;
using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.WebMercator.Wpf;

public partial class MainWindow
{
    private const double OriginShift = 20037508.342789244;

    public MainWindow()
    {
        InitializeComponent();
        viewerControl.MouseCoordinatesChanged += ViewerControl_MouseCoordinatesChanged;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        detailsTextBox.Text = WebMercatorDetails();

        if (!LoadLayer())
            return;

        CreateCityLayer();
        SetWorldExtent();
        statusTextBlock.Text = "Move the mouse over the map to inspect EPSG:3857 meter coordinates.";
    }

    private bool LoadLayer()
    {
        var shapefilePath = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_3857.shp");
        if (!File.Exists(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be found:{Environment.NewLine}{shapefilePath}",
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(
                shapefilePath,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = WorldStyle()
                }))
        {
            MessageBox.Show(
                this,
                $"World layer could not be loaded:{Environment.NewLine}{shapefilePath}",
                Title,
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            viewerControl.SetLayerName(worldLayer.Index, "World source - EPSG:3857");

        return true;
    }

    private void CreateCityLayer()
    {
        var layerIndex = viewerControl.AddEmptyVectorLayer(
            "City points - EPSG:3857",
            GeoKernelShapeType.Point,
            CityStyle());

        if (layerIndex < 0)
            return;

        viewerControl.BeginEditLayer(layerIndex);
        viewerControl.SetActiveEditLayerIndex(layerIndex);

        foreach (var city in Cities())
        {
            var point = ToWebMercator(city.Longitude, city.Latitude);
            viewerControl.AddPointToEditLayer(layerIndex, point.X, point.Y);
        }

        viewerControl.CommitEditLayer(layerIndex);
        viewerControl.RefreshLayers();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetWorldExtent();
    }

    private void SetWorldExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-OriginShift, -OriginShift, OriginShift, OriginShift);
    }

    private void ViewerControl_MouseCoordinatesChanged(object? sender, GeoKernelMouseCoordinatesChangedEventArgs e)
    {
        statusTextBlock.Text =
            $"Screen: {e.ScreenPoint.X:0}, {e.ScreenPoint.Y:0} | WebMercator meters: {e.WorldPoint.X:0.00}, {e.WorldPoint.Y:0.00}";
    }

    private static GeoKernelPoint ToWebMercator(double longitude, double latitude)
    {
        var clampedLatitude = Math.Clamp(latitude, -85.05112878, 85.05112878);
        var x = longitude * OriginShift / 180.0;
        var y = Math.Log(Math.Tan((90.0 + clampedLatitude) * Math.PI / 360.0)) / (Math.PI / 180.0);
        y *= OriginShift / 180.0;
        return new GeoKernelPoint(x, y);
    }

    private static IEnumerable<(string Name, double Longitude, double Latitude)> Cities()
    {
        yield return ("Istanbul", 28.9784, 41.0082);
        yield return ("London", -0.1276, 51.5072);
        yield return ("New York", -74.0060, 40.7128);
        yield return ("Tokyo", 139.6917, 35.6895);
        yield return ("Sydney", 151.2093, -33.8688);
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

    private static GeoKernelLayerStyle CityStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            LineColor = "#8C321D",
            PointSize = 10.0,
            LineWidth = 1.2
        };
    }

    private static string WebMercatorDetails()
    {
        return string.Join(
            Environment.NewLine,
            "KnownCoordinateSystems::webMercator()",
            "",
            "Coordinate system",
            "EPSG: 3857",
            "Name: WGS 84 / Pseudo-Mercator",
            "Type: Projected",
            "",
            "Base geographic coordinate system",
            "EPSG: 4326",
            "Name: WGS 84",
            "",
            "Projection",
            "Name: Popular Visualisation Pseudo Mercator",
            "Method: WebMercator",
            "",
            "Linear unit",
            "Name: Meter",
            "Meters per unit: 1",
            "",
            "Axes",
            "1. Easting (E), direction: east",
            "2. Northing (N), direction: north",
            "",
            "WPF sample setup",
            "viewerControl.AddLayerFile(\"world_3857.shp\", options)",
            "viewerControl.ViewExtent = WebMercator full world extent",
            "",
            "City points are transformed from longitude/latitude degrees to EPSG:3857 meters.");
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
