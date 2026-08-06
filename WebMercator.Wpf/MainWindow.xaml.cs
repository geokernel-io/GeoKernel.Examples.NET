using GeoKernel.Examples.Common;
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
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        detailsTextBox.Text = WebMercatorDetails();

        if (!LoadLayer())
            return;

        viewerControl.SetLayerCoordinateSystemPreset(0, GeoKernelCoordinateSystemPreset.Wgs84);
        viewerControl.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.WebMercator);
        SetWorldExtent();
        statusTextBlock.Text = "Move the mouse over the map to inspect EPSG:3857 meter coordinates.";
    }

    private bool LoadLayer()
    {
        var shapefilePath = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
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
            viewerControl.SetLayerName(worldLayer.Index, "World countries - source EPSG:4326");

        return true;
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
            "viewerControl.AddLayerFile(\"world_4326.shp\", options)",
            "viewerControl.SetLayerCoordinateSystemPreset(0, Wgs84)",
            "viewerControl.SetCoordinateSystemPreset(WebMercator)",
            "",
            "The EPSG:4326 source layer is reprojected on the fly to EPSG:3857.");
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
