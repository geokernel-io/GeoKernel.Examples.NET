using System.IO;
using System.Drawing;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AddLayers.Wpf;

public partial class MainWindow
{
    private const string SampleDataBaseUrl = "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/";
    private const string RasterLayerName = "World Raster";
    private const string PolygonLayerName = "Countries";
    private const string CityLayerName = "Cities";

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        viewerControl.ClearLayers();
        viewerControl.AddOpenStreetMapLayer();

        if (!AddSampleLayer(
            "world_8km_png.zip",
            "world_8km_png",
            "world_8km.png",
            RasterLayerName))
            return;

        if (!AddSampleLayer(
            "world_4326.zip",
            "world_4326",
            "world_4326.shp",
            PolygonLayerName,
            new GeoKernelLayerStyle
            {
                FillColor = "#35475B",
                FillOpacity = 172,
                LineColor = "#B7E8FF",
                LineWidth = 0.85,
                LabelColor = "#FFFFFF",
                LabelHaloColor = "#10263A"
            }))
            return;

        if (!AddSampleLayer(
            "world_cities_4326.zip",
            "world_cities_4326",
            "world_cities_4326.shp",
            CityLayerName,
            new GeoKernelLayerStyle
            {
                PointColor = "#1D8FC7",
                PointSize = 4.2,
                LineColor = "#B9E6F5",
                LineWidth = 0.9
            }))
            return;

        ZoomToLayer(RasterLayerName);
    }

    private bool AddSampleLayer(
        string archiveName,
        string extractFolderName,
        string requiredFileName,
        string displayName,
        GeoKernelLayerStyle? style = null)
    {
        var path = SampleData.EnsureSampleFile(
            SampleDataUri(archiveName),
            archiveName,
            extractFolderName,
            requiredFileName,
            this);

        if (string.IsNullOrWhiteSpace(path))
            return false;

        var loaded = style is null
            ? viewerControl.AddLayerFile(path)
            : viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                });

        if (!loaded)
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{path}",
                "AddLayers",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(0);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void ZoomToLayer(string layerName)
    {
        var layer = viewerControl.GetLayerInfoByName(layerName);
        if (layer is null || !viewerControl.ZoomToLayer(layer.Index))
            viewerControl.FullExtent();
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ZoomIn();
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ZoomOut();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.FullExtent();
    }

    private void ZoomRect_Click(object sender, RoutedEventArgs e)
    {
        SetTool(GeoKernelViewerTool.ZoomBox);
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        viewerControl.ActiveTool = tool;
        zoomRectButton.IsChecked = tool == GeoKernelViewerTool.ZoomBox;
        panButton.IsChecked = tool == GeoKernelViewerTool.Pan;
    }

    private static Uri SampleDataUri(string archiveName) => new($"{SampleDataBaseUrl}{archiveName}");

}
