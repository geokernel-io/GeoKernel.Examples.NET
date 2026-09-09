using GeoKernel.Examples.Common;
using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.CoordinateTransform.Wpf;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
        viewerControl.MapMouseMove += ViewerControl_MapMouseMove;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.Wgs84);

        if (!LoadLayer())
            return;

        SetWorldExtent();
        statusTextBlock.Text = "Move the mouse over the map. The status bar shows EPSG:4326 -> EPSG:3857.";
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

    private void ViewerControl_MapMouseMove(object? sender, GeoKernelMapMouseEventArgs e)
    {
        var mercator = ToWebMercator(e.WorldPoint.X, e.WorldPoint.Y);
        statusTextBlock.Text =
            $"Screen: {e.ScreenPoint.X:0}, {e.ScreenPoint.Y:0} | " +
            $"EPSG:4326 lon/lat: {e.WorldPoint.X:0.000000}, {e.WorldPoint.Y:0.000000} | " +
            $"EPSG:3857 meters: {mercator.X:0.00}, {mercator.Y:0.00}";
    }

    private static (double X, double Y) ToWebMercator(double longitude, double latitude)
    {
        const double earthRadius = 6378137.0;
        var clampedLatitude = Math.Clamp(latitude, -85.05112878, 85.05112878);
        var x = earthRadius * longitude * Math.PI / 180.0;
        var y = earthRadius * Math.Log(Math.Tan(Math.PI / 4.0 + clampedLatitude * Math.PI / 360.0));
        return (x, y);
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
