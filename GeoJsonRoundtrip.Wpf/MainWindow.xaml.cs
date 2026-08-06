using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GeoJsonRoundtrip.Wpf;

public partial class MainWindow
{
    private const string PointGeoJson = "{\"type\":\"Point\",\"coordinates\":[-122.4194,37.7749]}";
    private const string PolylineGeoJson = "{\"type\":\"LineString\",\"coordinates\":[[-123.00,37.10],[-122.55,37.65],[-122.05,37.30],[-121.55,38.10],[-120.90,37.55]]}";
    private const string PolygonGeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[-123.25,37.15],[-122.15,36.95],[-121.55,37.65],[-122.05,38.35],[-123.05,38.15],[-123.25,37.15]]]}";
    private bool _loaded;

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _loaded = true;
        ResetGeoJson();
        RunRoundtrip();
    }

    private void GeometryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_loaded)
            return;

        ResetGeoJson();
        RunRoundtrip();
    }

    private void RunRoundtrip_Click(object sender, RoutedEventArgs e) => RunRoundtrip();
    private void Reset_Click(object sender, RoutedEventArgs e) { ResetGeoJson(); RunRoundtrip(); }

    private void GeoJsonTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;
        e.Handled = true;
        RunRoundtrip();
    }

    private void ResetGeoJson() => geoJsonTextBox.Text = (GeometryMode)geometryComboBox.SelectedIndex switch
    {
        GeometryMode.Point => PointGeoJson,
        GeometryMode.Polyline => PolylineGeoJson,
        _ => PolygonGeoJson
    };

    private void RunRoundtrip()
    {
        var input = geoJsonTextBox.Text.Trim();
        viewerControl.ClearLayers();
        viewerControl.AddOpenStreetMapLayer();

        try
        {
            var mode = (GeometryMode)geometryComboBox.SelectedIndex;
            var api = $"{ReadApiName(mode)} -> {WriteApiName(mode)}";
            string output;
            GeoKernelExtent extent;
            int count;

            switch (mode)
            {
                case GeometryMode.Point:
                    var point = viewerControl.ReadGeoJsonPoint(input);
                    output = viewerControl.WriteGeoJsonPoint(point);
                    var webPoint = ToWebMercator(point);
                    viewerControl.AddPointLayer("Roundtrip Point", [webPoint], PointStyle());
                    extent = PaddedExtent([webPoint]);
                    count = 1;
                    break;
                case GeometryMode.Polyline:
                    var line = viewerControl.ReadGeoJsonLineString(input);
                    output = viewerControl.WriteGeoJsonLineString(line);
                    var webLine = line.Select(ToWebMercator).ToArray();
                    viewerControl.AddPolylineLayer("Roundtrip Polyline", webLine, LineStyle());
                    extent = PaddedExtent(webLine);
                    count = line.Count;
                    break;
                default:
                    var rings = viewerControl.ReadGeoJsonPolygon(input);
                    output = viewerControl.WriteGeoJsonPolygon(rings);
                    var webRings = rings.Select(r => (IReadOnlyList<GeoKernelPoint>)r.Select(ToWebMercator).ToArray()).ToArray();
                    viewerControl.AddPolygonLayer("Roundtrip Polygon", webRings, PolygonStyle());
                    extent = PaddedExtent(webRings.SelectMany(r => r).ToArray());
                    count = rings.Sum(r => r.Count);
                    break;
            }

            viewerControl.ViewExtent = extent;
            detailsTextBox.Text = DetailsText(api, input, output, count);
            statusText.Text = string.Equals(input, output, StringComparison.Ordinal)
                ? "Roundtrip completed. Output is identical."
                : "Roundtrip completed. Output is normalized by GisGeoJsonWriter.";
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            detailsTextBox.Text = $"GeoJSON roundtrip failed:{Environment.NewLine}{ex.Message}";
            statusText.Text = "GeoJSON roundtrip failed.";
        }
    }

    private static string DetailsText(string api, string input, string output, int count) => string.Join(Environment.NewLine,
        "GeoJsonRoundtrip sample", "", "API", api, "", "Input GeoJSON", input, "", "Output GeoJSON", output, "",
        "Comparison", $"Identical: {string.Equals(input, output, StringComparison.Ordinal)}", $"Vertex count: {count}", "",
        "Note", "GisGeoJsonWriter can normalize formatting even when geometry is unchanged.");

    private static string ReadApiName(GeometryMode mode) => mode switch
    {
        GeometryMode.Point => "GisGeoJsonReader::readPoint(geoJson)",
        GeometryMode.Polyline => "GisGeoJsonReader::readLineString(geoJson)",
        _ => "GisGeoJsonReader::readPolygon(geoJson)"
    };

    private static string WriteApiName(GeometryMode mode) => mode switch
    {
        GeometryMode.Point => "GisGeoJsonWriter::writePoint(shape)",
        GeometryMode.Polyline => "GisGeoJsonWriter::writePolyline(shape)",
        _ => "GisGeoJsonWriter::writePolygon(shape)"
    };

    private static GeoKernelLayerStyle PointStyle() => new() { PointColor = "#D95D39", LineColor = "#8C321D", PointSize = 13, LineWidth = 1.4 };
    private static GeoKernelLayerStyle LineStyle() => new() { LineColor = "#E4572E", LineWidth = 3.4, PointColor = "#F3A712", PointSize = 7 };
    private static GeoKernelLayerStyle PolygonStyle() => new() { FillColor = "#88D18A", FillOpacity = 130, LineColor = "#1F7A4D", LineWidth = 2.4 };

    private static GeoKernelPoint ToWebMercator(GeoKernelPoint p)
    {
        const double shift = 20037508.342789244;
        var lon = Math.Clamp(p.X, -180, 180);
        var lat = Math.Clamp(p.Y, -85.05112878, 85.05112878);
        return new GeoKernelPoint(lon * shift / 180, Math.Log(Math.Tan((90 + lat) * Math.PI / 360)) * shift / Math.PI);
    }

    private static GeoKernelExtent PaddedExtent(IReadOnlyList<GeoKernelPoint> points)
    {
        var x0 = points.Min(p => p.X); var y0 = points.Min(p => p.Y);
        var x1 = points.Max(p => p.X); var y1 = points.Max(p => p.Y);
        var px = Math.Max(350000, (x1 - x0) * .45); var py = Math.Max(350000, (y1 - y0) * .45);
        return new GeoKernelExtent(x0 - px, y0 - py, x1 + px, y1 + py);
    }

    private enum GeometryMode { Point, Polyline, Polygon }
}
