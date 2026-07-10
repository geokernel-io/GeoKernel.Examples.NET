using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.HelloMap.Wpf;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var shapefilePath = SampleData.EnsureSampleFile(
            new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/world_4326.zip"),
            "world_4326.zip",
            "world_4326",
            "world_4326.shp",
            this);

        if (string.IsNullOrWhiteSpace(shapefilePath))
            return;

        if (!viewerControl.AddLayerFile(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be loaded:{Environment.NewLine}{shapefilePath}",
                "HelloMap",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        viewerControl.FullExtent();
        SetTool(GeoKernelViewerTool.Pan);
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
}
