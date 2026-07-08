using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.Wgs84Setup.Wpf;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        viewerControl.MouseCoordinatesChanged += ViewerControl_MouseCoordinatesChanged;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        detailsTextBox.Text = Wgs84Details();

        if (!LoadLayer())
            return;

        SetWorldExtent();
        statusTextBlock.Text = "Move the mouse over the map to inspect screen/world coordinates.";
    }

    private bool LoadLayer()
    {
        var shapefilePath = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
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
            viewerControl.SetLayerName(worldLayer.Index, "World - EPSG:4326");

        return true;
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetWorldExtent();
    }

    private void SetWorldExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-180.0, -85.0, 180.0, 85.0);
    }

    private void ViewerControl_MouseCoordinatesChanged(object? sender, GeoKernelMouseCoordinatesChangedEventArgs e)
    {
        statusTextBlock.Text =
            $"Screen: {e.ScreenPoint.X:0}, {e.ScreenPoint.Y:0} | World: {e.WorldPoint.X:0.000000}, {e.WorldPoint.Y:0.000000}";
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

    private static string Wgs84Details()
    {
        return string.Join(
            Environment.NewLine,
            "KnownCoordinateSystems::wgs84()",
            "",
            "Coordinate system",
            "EPSG: 4326",
            "Name: WGS 84",
            "Type: Geographic",
            "",
            "Datum",
            "EPSG: 6326",
            "Name: World Geodetic System 1984",
            "Ellipsoid: WGS 84",
            "",
            "Angular unit",
            "Name: degree",
            "Axes:",
            "1. Longitude (Lon), direction: east",
            "2. Latitude (Lat), direction: north",
            "",
            "WPF wrapper usage",
            "viewerControl.AddLayerFile(\"world_4326.shp\", options)",
            "viewerControl.ScreenToWorld(screenX, screenY)",
            "",
            "Mouse status shows longitude/latitude degrees.");
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
