using System.IO;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GeoTiffOverlay.Wpf;

public sealed partial class MainWindow
{
    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e) => LoadSample();
    private void ZoomIn_Click(object sender, RoutedEventArgs e) => viewerControl.ZoomIn();
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => viewerControl.ZoomOut();
    private void FullExtent_Click(object sender, RoutedEventArgs e) => viewerControl.FullExtent();
    private void ZoomRectangle_Click(object sender, RoutedEventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.ZoomBox;
    private void Pan_Click(object sender, RoutedEventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

    private void LoadSample()
    {
        viewerControl.ClearLayers();
        try
        {
            var path = SampleData.EnsureKnownWpfSampleFile("world_8km.tif", this);
            if (!File.Exists(path))
                throw new FileNotFoundException("GeoTIFF sample data could not be found.", path);
            if (!viewerControl.AddLayerFile(path))
                throw new InvalidOperationException($"GeoTIFF could not be loaded: {path}");

            var layer = viewerControl.GetLayerInfo(0)
                ?? throw new InvalidOperationException("GeoTIFF layer information could not be read.");
            var extent = layer.ProjectedExtent;
            var file = new FileInfo(path);
            detailsTextBox.Text = string.Join(Environment.NewLine,
                "GeoTIFF overlay sample", "", "File", $"Path: {layer.Path}",
                $"Exists: {(file.Exists ? "yes" : "no")}", $"Size: {(file.Exists ? file.Length : 0)} bytes", "",
                "Raster", $"Layer: {layer.Name}", $"Open: {(layer.IsOpen ? "yes" : "no")}",
                $"EPSG: {(layer.CoordinateSystem.EpsgCode == 0 ? "unknown" : layer.CoordinateSystem.EpsgCode)}",
                $"Coordinate system: {layer.CoordinateSystem.Name}",
                $"Projected extent: ({extent.XMin:F2}, {extent.YMin:F2}) - ({extent.XMax:F2}, {extent.YMax:F2})", "",
                "SDK flow", "viewerControl.AddLayerFile(path);", "viewerControl.GetLayerInfo(index);", "viewerControl.FullExtent();");
            viewerControl.FullExtent();
            statusText.Text = "GeoTIFF loaded: world_8km.tif";
        }
        catch (Exception ex)
        {
            detailsTextBox.Text = ex.Message;
            statusText.Text = "GeoTiffOverlay failed.";
            MessageBox.Show(this, ex.Message, "GeoTiffOverlay", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
