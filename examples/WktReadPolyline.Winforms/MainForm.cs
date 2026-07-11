using GeoKernel.NET.WinForms;

namespace GeoKernel.WktReadPolyline.Winforms;

public sealed partial class MainForm : Form
{
    private const string DefaultWkt = "LINESTRING(-122.4194 37.7749, -121.8863 37.3382, -121.4944 38.5816, -120.7401 37.6391)";

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {        
        ParseAndRender();
    }

    private void readButton_Click(object? sender, EventArgs e) => ParseAndRender();

    private void resetButton_Click(object? sender, EventArgs e)
    {
        wktTextBox.Text = DefaultWkt;
        ParseAndRender();
    }

    private void wktTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.Handled = true;
        e.SuppressKeyPress = true;
        ParseAndRender();
    }

    private void ParseAndRender()
    {
        var input = wktTextBox.Text.Trim();
        geoKernelViewerControl.ClearLayers();

        try
        {
            var lonLatPoints = geoKernelViewerControl.ReadWktLineString(input);
            var webMercatorPoints = lonLatPoints.Select(ToWebMercator).ToArray();
            var viewExtent = PaddedExtent(webMercatorPoints);

            geoKernelViewerControl.AddOpenStreetMapLayer();
            geoKernelViewerControl.AddPolylineLayer("WKT LineString", webMercatorPoints, LineStyle());
            geoKernelViewerControl.ViewExtent = viewExtent;

            detailsTextBox.Text = DetailsText(input, lonLatPoints, viewExtent);
            statusLabel.Text = $"GisWktReader::readLineString parsed {lonLatPoints.Count} vertices.";
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            detailsTextBox.Text = $"WKT parse failed:{Environment.NewLine}{ex.Message}";
            statusLabel.Text = "WKT parse failed.";
        }
    }

    private static string DetailsText(string inputWkt, IReadOnlyList<GeoKernelPoint> lonLatPoints, GeoKernelExtent webMercatorExtent) =>
        string.Join(
            Environment.NewLine,
            "WktReadPolyline sample",
            "",
            "API",
            "GisWktReader::readLineString(wkt)",
            "",
            "Input WKT",
            inputWkt,
            "",
            "Parsed line",
            "Parts: 1",
            $"Vertices: {lonLatPoints.Count}",
            $"Lon/lat extent: {ExtentText(PaddedExtent(lonLatPoints, paddingRatio: 0.0, minimumPadding: 0.0))}",
            "",
            "Displayed over OSM",
            "Input CRS: EPSG:4326",
            "Viewer/OSM CRS: EPSG:3857",
            $"WebMercator view extent: {ExtentText(webMercatorExtent)}",
            "",
            "Round-trip WKT",
            $"LINESTRING({string.Join(", ", lonLatPoints.Select(p => $"{p.X:0.######} {p.Y:0.######}"))})");

    private static GeoKernelLayerStyle LineStyle() => new()
    {
        LineColor = "#E4572E",
        LineWidth = 4.0,
        PointColor = "#F3A712",
        PointSize = 7.0
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

    private static GeoKernelExtent PaddedExtent(IReadOnlyList<GeoKernelPoint> points, double paddingRatio = 0.25, double minimumPadding = 250_000.0)
    {
        var xMin = points.Min(point => point.X);
        var yMin = points.Min(point => point.Y);
        var xMax = points.Max(point => point.X);
        var yMax = points.Max(point => point.Y);
        var paddingX = Math.Max(minimumPadding, (xMax - xMin) * paddingRatio);
        var paddingY = Math.Max(minimumPadding, (yMax - yMin) * paddingRatio);
        return new GeoKernelExtent(xMin - paddingX, yMin - paddingY, xMax + paddingX, yMax + paddingY);
    }

    private static string ExtentText(GeoKernelExtent extent) =>
        $"({extent.XMin:F3}, {extent.YMin:F3}) - ({extent.XMax:F3}, {extent.YMax:F3})";
}
