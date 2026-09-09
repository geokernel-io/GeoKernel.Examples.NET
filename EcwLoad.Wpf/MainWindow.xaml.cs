using System.IO;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.EcwLoad.Wpf;

public partial class MainWindow
{
    private string _rasterPath = string.Empty;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        _rasterPath = SampleData.EnsureKnownWpfSampleFile("world_8km.ecw", this);
        if (!string.IsNullOrWhiteSpace(_rasterPath))
            LoadSample();
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e) => viewerControl.ZoomIn();
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => viewerControl.ZoomOut();
    private void FullExtent_Click(object sender, RoutedEventArgs e) => viewerControl.FullExtent();
    private void ZoomRectangle_Click(object sender, RoutedEventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.ZoomBox;
    private void Pan_Click(object sender, RoutedEventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

    private void LoadSample()
    {
        viewerControl.ClearLayers();
        var details = new List<string>
        {
            "EcwLoad sample",
            "",
            "API",
            "AddLayerFile(path);",
            "GetLayerInfo(index);",
            "GetLayerAttributeDefinitions(index)",
            ""
        };

        try
        {
            if (!File.Exists(_rasterPath))
                throw new FileNotFoundException("ECW sample data file could not be found.", _rasterPath);
            if (!viewerControl.AddLayerFile(_rasterPath))
                throw new InvalidOperationException($"ECW layer could not be loaded: {_rasterPath}");

            viewerControl.FullExtent();
            details.Add("Loaded: " + _rasterPath);
            details.Add("");
            details.Add("Layers");
            foreach (var layer in viewerControl.GetLayersInfo())
            {
                details.Add($"#{layer.Index}: {layer.Name} | features: {layer.FeatureCount} | type: {layer.ShapeType}");
                details.Add($"EPSG: {(layer.CoordinateSystem.EpsgCode == 0 ? "unknown" : layer.CoordinateSystem.EpsgCode)}");
                details.Add($"Coordinate system: {layer.CoordinateSystem.Name}");
                var extent = layer.ProjectedExtent;
                details.Add($"Projected extent: ({extent.XMin:F2}, {extent.YMin:F2}) - ({extent.XMax:F2}, {extent.YMax:F2})");
            }

            detailsTextBox.Text = string.Join(Environment.NewLine, details);
            statusText.Text = "EcwLoad loaded.";
        }
        catch (Exception ex)
        {
            details.Add(ex.Message);
            detailsTextBox.Text = string.Join(Environment.NewLine, details);
            statusText.Text = "EcwLoad failed.";
            MessageBox.Show(this, ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
