using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.XyzTileSize.Wpf;

public sealed partial class MainWindow
{
    private const string UrlTemplate = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
    private static readonly GeoKernelExtent DefaultExtent = new(-1400000.0, 4100000.0, 4200000.0, 7800000.0);

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            AddTileLayer(leftViewerControl, 256);
            AddTileLayer(rightViewerControl, 512);
            SetExtentForBoth();
            SetToolForBoth(GeoKernelViewerTool.Pan);
            statusText.Text = "Compare AddXyzLayer tileSize 256 and 512.";
        }
        catch (Exception ex)
        {
            statusText.Text = "XyzTileSize failed.";
            MessageBox.Show(this, ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static void AddTileLayer(GeoKernelViewerControl viewer, int tileSize)
    {
        viewer.ClearLayers();
        var cacheDirectory = Path.Combine(AppContext.BaseDirectory, "XyzTileSizeCache", tileSize.ToString());
        var index = viewer.AddXyzLayer($"OSM tileSize {tileSize}", UrlTemplate, 0, 19, tileSize,
            "OpenStreetMap contributors", true, cacheDirectory);
        if (index < 0)
            throw new InvalidOperationException($"XYZ layer with tileSize {tileSize} could not be added.");
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e) { leftViewerControl.ZoomIn(); rightViewerControl.ZoomIn(); }
    private void ZoomOut_Click(object sender, RoutedEventArgs e) { leftViewerControl.ZoomOut(); rightViewerControl.ZoomOut(); }
    private void FullExtent_Click(object sender, RoutedEventArgs e) => SetExtentForBoth();
    private void ZoomRect_Click(object sender, RoutedEventArgs e) => SetToolForBoth(GeoKernelViewerTool.ZoomBox);
    private void Pan_Click(object sender, RoutedEventArgs e) => SetToolForBoth(GeoKernelViewerTool.Pan);

    private void SetExtentForBoth() { leftViewerControl.ViewExtent = DefaultExtent; rightViewerControl.ViewExtent = DefaultExtent; }
    private void SetToolForBoth(GeoKernelViewerTool tool) { leftViewerControl.ActiveTool = tool; rightViewerControl.ActiveTool = tool; }

    private static string DetailsText() => string.Join(Environment.NewLine,
        "XYZ tile size sample", "", "Left map:", "AddXyzLayer(..., tileSize: 256)", "",
        "Right map:", "AddXyzLayer(..., tileSize: 512)", "", "URL template:", UrlTemplate, "",
        "Why this matters:", "- tileSize is the expected pixel size of one downloaded tile.",
        "- Standard OSM tiles are usually 256 px.", "- Some services expose 512 px retina/high-DPI tiles.",
        "- The cache key includes tileSize, so 256 and 512 variants stay separate.", "", "SDK flow:",
        "viewer.AddXyzLayer(name, urlTemplate, 0, 19, tileSize, attribution, true, cacheDirectory);");
}
