using System.IO;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LabelCollisionOff.Wpf;

public partial class MainWindow
{
    private static readonly GeoKernelExtent ContinentalUsExtent = new(-127.0, 23.0, -66.0, 50.0);

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            var worldPath = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
            var citiesPath = SampleData.EnsureKnownWpfSampleFile("world_cities_4326.shp", this);
            if (!File.Exists(worldPath) || !File.Exists(citiesPath))
                return;

            if (!LoadComparisonLayers(collisionOnViewer, worldPath, citiesPath, false) ||
                !LoadComparisonLayers(collisionOffViewer, worldPath, citiesPath, true))
            {
                MessageBox.Show(this, "Comparison layers could not be loaded.", Title, MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            collisionOnViewer.ViewExtent = ContinentalUsExtent;
            collisionOffViewer.ViewExtent = ContinentalUsExtent;
            statusText.Text = "Left: collision filtering. Right: label overlap allowed.";
        }
        catch (Exception ex)
        {
            statusText.Text = "Label collision comparison failed.";
            MessageBox.Show(this, ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static bool LoadComparisonLayers(GeoKernelViewerControl viewer, string worldPath, string citiesPath, bool allowOverlap)
    {
        viewer.ActiveTool = GeoKernelViewerTool.Pan;
        if (!viewer.AddLayerFile(worldPath) || !viewer.AddLayerFile(citiesPath))
            return false;

        var cities = viewer.GetLayerInfo(0);
        var world = viewer.GetLayerInfo(1);
        if (cities is null || world is null)
            return false;

        viewer.SetLayerName(world.Index, "World");
        viewer.SetLayerName(cities.Index, allowOverlap ? "Cities - labelAllowOverlap true" : "Cities - labelAllowOverlap false");
        viewer.SetLayerStyle(world.Index, WorldStyle());
        viewer.SetLayerStyle(cities.Index, CityStyle(allowOverlap));
        return true;
    }

    private static GeoKernelLayerStyle WorldStyle() => new()
    {
        FillColor = "#D8E5E1", FillOpacity = 215, LineColor = "#6F8380", LineWidth = 0.8
    };

    private static GeoKernelLayerStyle CityStyle(bool allowOverlap) => new()
    {
        PointColor = "#D56037", LineColor = "#A23D23", PointSize = 5.5, LineWidth = 0.8,
        ShowLabels = true, LabelField = "CITY_NAME", LabelFontSize = 8.0, LabelColor = "#1F2933",
        LabelHaloEnabled = true, LabelHaloColor = "#FFFFFF", LabelHaloWidth = 1.5,
        LabelAllowOverlap = allowOverlap, LabelOffsetX = 7.0, LabelOffsetY = -7.0
    };
}
