using System.Windows;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.WktReadPoint.Wpf;

public partial class MainWindow
{
    private const string DefaultWkt = "POINT(-122.4194 37.7749)";

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        ParseAndRender();
    }

    private void ReadPoint_Click(object sender, RoutedEventArgs e) => ParseAndRender();

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        wktTextBox.Text = DefaultWkt;
        ParseAndRender();
    }

    private void WktTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;
        ParseAndRender();
    }

    private void ParseAndRender()
    {
        var input = wktTextBox.Text.Trim();
        viewerControl.ClearLayers();

        try
        {
            var lonLat = viewerControl.ReadWktPoint(input);
            var webMercator = ToWebMercator(lonLat);

            viewerControl.AddOpenStreetMapLayer();
            viewerControl.AddPointLayer("WKT Point", [webMercator], PointStyle());
            viewerControl.ViewExtent = PointViewExtent(webMercator);

            detailsTextBox.Text = DetailsText(input, lonLat, webMercator);
            statusText.Text = $"GisWktReader::readPoint parsed lon/lat POINT({lonLat.X:F6} {lonLat.Y:F6}) over OSM.";
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            detailsTextBox.Text = $"WKT parse failed:{Environment.NewLine}{ex.Message}";
            statusText.Text = "WKT parse failed.";
        }
    }

    private static string DetailsText(string inputWkt, GeoKernelPoint lonLat, GeoKernelPoint webMercator) =>
        string.Join(
            Environment.NewLine,
            "WktReadPoint sample",
            "",
            "API",
            "GisWktReader::readPoint(wkt)",
            "",
            "Input WKT",
            inputWkt,
            "",
            "Parsed lon/lat point",
            $"X: {lonLat.X:F6}",
            $"Y: {lonLat.Y:F6}",
            "",
            "Displayed over OSM",
            "Input CRS: EPSG:4326",
            "Viewer/OSM CRS: EPSG:3857",
            $"WebMercator X: {webMercator.X:F3}",
            $"WebMercator Y: {webMercator.Y:F3}",
            "",
            "Round-trip WKT",
            $"POINT({lonLat.X:0.######} {lonLat.Y:0.######})");

    private static GeoKernelLayerStyle PointStyle() => new()
    {
        PointColor = "#D95D39",
        LineColor = "#8C321D",
        PointSize = 14.0,
        LineWidth = 1.5
    };

    private static GeoKernelPoint ToWebMercator(GeoKernelPoint lonLat)
    {
        const double originShift = 20037508.342789244;
        var lon = Math.Clamp(lonLat.X, -180.0, 180.0);
        var lat = Math.Clamp(lonLat.Y, -85.05112878, 85.05112878);
        var x = lon * originShift / 180.0;
        var y = Math.Log(Math.Tan((90.0 + lat) * Math.PI / 360.0)) * originShift / Math.PI;
        return new GeoKernelPoint(x, y);
    }

    private static GeoKernelExtent PointViewExtent(GeoKernelPoint point) =>
        new(
            point.X - 2_500_000.0,
            point.Y - 1_800_000.0,
            point.X + 2_500_000.0,
            point.Y + 1_800_000.0);
}
