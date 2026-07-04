using GeoKernel.NET.WinForms;

namespace GeoKernel.WktReadPolygon.Winforms;

public sealed partial class MainForm : Form
{
    private const string PolygonWkt =
        "POLYGON((-123.25 37.15, -122.15 36.95, -121.55 37.65, -122.05 38.35, -123.05 38.15, -123.25 37.15))";

    private const string MultiPolygonWkt =
        "MULTIPOLYGON(((-123.25 37.15, -122.25 36.95, -121.85 37.65, -122.45 38.20, -123.15 37.95, -123.25 37.15)),((-121.60 36.75, -120.70 36.70, -120.45 37.35, -121.25 37.65, -121.60 36.75)))";

    private bool _loaded;

    public MainForm()
    {
        InitializeComponent();
        modeComboBox.SelectedIndex = 0;
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        _loaded = true;
        ResetWkt();
        ParseAndRender();
    }

    private void readButton_Click(object? sender, EventArgs e) => ParseAndRender();

    private void resetButton_Click(object? sender, EventArgs e)
    {
        ResetWkt();
        ParseAndRender();
    }

    private void modeComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        readButton.Text = IsMultiPolygon ? "Read MultiPolygon" : "Read Polygon";
        ResetWkt();
        if (_loaded)
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

    private bool IsMultiPolygon => modeComboBox.SelectedIndex == 1;

    private void ResetWkt()
    {
        wktTextBox.Text = IsMultiPolygon ? MultiPolygonWkt : PolygonWkt;
    }

    private void ParseAndRender()
    {
        var input = wktTextBox.Text.Trim();
        geoKernelViewerControl.ClearLayers();

        try
        {
            var lonLatRings = geoKernelViewerControl.ReadWktPolygon(input, IsMultiPolygon);
            var webMercatorRings = lonLatRings
                .Select(ring => (IReadOnlyList<GeoKernelPoint>)ring.Select(ToWebMercator).ToArray())
                .ToArray();
            var viewExtent = PaddedExtent(webMercatorRings.SelectMany(ring => ring).ToArray());
            var apiName = IsMultiPolygon
                ? "GisWktReader::readMultiPolygon(wkt)"
                : "GisWktReader::readPolygon(wkt)";

            geoKernelViewerControl.AddOpenStreetMapLayer();
            geoKernelViewerControl.AddPolygonLayer("WKT Polygon", webMercatorRings, PolygonStyle());
            geoKernelViewerControl.ViewExtent = viewExtent;

            detailsTextBox.Text = DetailsText(input, apiName, lonLatRings, viewExtent);
            statusLabel.Text = $"{apiName} parsed {lonLatRings.Count} ring(s) and {VertexCount(lonLatRings)} vertices.";
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            detailsTextBox.Text = $"WKT parse failed:{Environment.NewLine}{ex.Message}";
            statusLabel.Text = "WKT parse failed.";
        }
    }

    private static string DetailsText(
        string inputWkt,
        string apiName,
        IReadOnlyList<IReadOnlyList<GeoKernelPoint>> lonLatRings,
        GeoKernelExtent webMercatorExtent) =>
        string.Join(
            Environment.NewLine,
            "WktReadPolygon sample",
            "",
            "API",
            apiName,
            "",
            "Input WKT",
            inputWkt,
            "",
            "Parsed polygon",
            $"Parts/rings: {lonLatRings.Count}",
            $"Vertices: {VertexCount(lonLatRings)}",
            $"Lon/lat extent: {ExtentText(PaddedExtent(lonLatRings.SelectMany(ring => ring).ToArray(), 0.0, 0.0))}",
            $"Centroid: {Centroid(lonLatRings).X:F6}, {Centroid(lonLatRings).Y:F6}",
            "",
            "Displayed over OSM",
            "Input CRS: EPSG:4326",
            "Viewer/OSM CRS: EPSG:3857",
            $"WebMercator view extent: {ExtentText(webMercatorExtent)}",
            "",
            "Round-trip WKT",
            WktText(lonLatRings));

    private static GeoKernelLayerStyle PolygonStyle() => new()
    {
        FillColor = "#88D18A",
        FillOpacity = 130,
        LineColor = "#1F7A4D",
        LineWidth = 2.5
    };

    private static int VertexCount(IReadOnlyList<IReadOnlyList<GeoKernelPoint>> rings) =>
        rings.Sum(ring => ring.Count);

    private static GeoKernelPoint Centroid(IReadOnlyList<IReadOnlyList<GeoKernelPoint>> rings)
    {
        var points = rings.SelectMany(ring => ring).ToArray();
        return new GeoKernelPoint(points.Average(point => point.X), points.Average(point => point.Y));
    }

    private static string WktText(IReadOnlyList<IReadOnlyList<GeoKernelPoint>> rings)
    {
        static string RingText(IReadOnlyList<GeoKernelPoint> ring) =>
            $"({string.Join(", ", ring.Select(point => $"{point.X:0.######} {point.Y:0.######}"))})";

        return rings.Count == 1
            ? $"POLYGON({RingText(rings[0])})"
            : $"MULTIPOLYGON({string.Join(", ", rings.Select(ring => $"({RingText(ring)})"))})";
    }

    private static GeoKernelPoint ToWebMercator(GeoKernelPoint lonLat)
    {
        const double originShift = 20037508.342789244;
        var lon = Math.Clamp(lonLat.X, -180.0, 180.0);
        var lat = Math.Clamp(lonLat.Y, -85.05112878, 85.05112878);
        var x = lon * originShift / 180.0;
        var y = Math.Log(Math.Tan((90.0 + lat) * Math.PI / 360.0)) * originShift / Math.PI;
        return new GeoKernelPoint(x, y);
    }

    private static GeoKernelExtent PaddedExtent(IReadOnlyList<GeoKernelPoint> points, double paddingRatio = 0.35, double minimumPadding = 300_000.0)
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
