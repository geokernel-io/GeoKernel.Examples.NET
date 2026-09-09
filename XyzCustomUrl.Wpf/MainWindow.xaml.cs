using System.Windows;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.XyzCustomUrl.Wpf;

public sealed partial class MainWindow
{
    private const string DefaultUrl = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
    private static readonly GeoKernelExtent DefaultExtent = new(-1400000, 4100000, 4200000, 7800000);

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        urlTextBox.Text = DefaultUrl;
        minZoomTextBox.Text = "0";
        maxZoomTextBox.Text = "19";
        localCacheCheckBox.IsChecked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        ApplyCustomUrl();
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e) => viewerControl.ZoomIn();
    private void ZoomOut_Click(object sender, RoutedEventArgs e) => viewerControl.ZoomOut();
    private void FullExtent_Click(object sender, RoutedEventArgs e) => viewerControl.ViewExtent = DefaultExtent;
    private void ZoomRect_Click(object sender, RoutedEventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.ZoomBox;
    private void Pan_Click(object sender, RoutedEventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
    private void ApplyUrl_Click(object sender, RoutedEventArgs e) => ApplyCustomUrl();
    private void UrlTextBox_KeyDown(object sender, KeyEventArgs e) { if (e.Key == Key.Enter) ApplyCustomUrl(); }

    private void ApplyCustomUrl()
    {
        var url = urlTextBox.Text.Trim();
        if (!IsSupportedTileTemplate(url))
        {
            MessageBox.Show(this, "Tile URL template must include {z}, {x}, and {y}, or Bing-style {q}.", "XyzCustomUrl", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (!int.TryParse(minZoomTextBox.Text, out var minZoom) || !int.TryParse(maxZoomTextBox.Text, out var maxZoom) ||
            minZoom is < 0 or > 21 || maxZoom is < 0 or > 21 || minZoom > maxZoom)
        {
            MessageBox.Show(this, "Zoom values must be between 0 and 21, and minimum cannot exceed maximum.", "XyzCustomUrl", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            viewerControl.ClearLayers();
            var cacheEnabled = localCacheCheckBox.IsChecked == true;
            var index = viewerControl.AddXyzLayer("Custom XYZ", url, minZoom: minZoom, maxZoom: maxZoom,
                tileSize: 256, localCacheEnabled: cacheEnabled);
            if (index < 0) throw new InvalidOperationException("Custom XYZ layer could not be loaded.");
            viewerControl.ViewExtent = DefaultExtent;
            detailsTextBox.Text = string.Join(Environment.NewLine,
                "Custom XYZ URL sample", "", "Active URL template:", url, "", $"Min zoom: {minZoom}",
                $"Max zoom: {maxZoom}", "Tile size: 256", $"Local cache: {(cacheEnabled ? "enabled" : "disabled")}", "",
                "SDK flow:", "viewerControl.AddXyzLayer(name, urlTemplate, minZoom, maxZoom, tileSize, localCacheEnabled)", "",
                "Template requirements:", "- XYZ: {z}, {x}, {y}", "- or Bing style: {q}");
            statusText.Text = "Custom XYZ URL applied.";
        }
        catch (Exception ex)
        {
            statusText.Text = "Custom XYZ layer failed.";
            MessageBox.Show(this, $"Custom XYZ layer could not be loaded:\n{ex.Message}", "XyzCustomUrl", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private static bool IsSupportedTileTemplate(string url) =>
        (url.Contains("{z}") && url.Contains("{x}") && url.Contains("{y}")) || url.Contains("{q}");
}
