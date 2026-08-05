using GeoKernel.NET.WinForms;

namespace GeoKernel.WktRoundtrip.Winforms;

public sealed partial class MainForm : Form
{
    private const string PointWkt = "POINT(-122.4194 37.7749)";

    private const string PolylineWkt =
        "LINESTRING(-123.00 37.10, -122.55 37.65, -122.05 37.30, -121.55 38.10, -120.90 37.55)";

    private const string PolygonWkt =
        "POLYGON((-123.25 37.15, -122.15 36.95, -121.55 37.65, -122.05 38.35, -123.05 38.15, -123.25 37.15))";

    private bool _loaded;

    public MainForm()
    {
        InitializeComponent();
        geometryComboBox.SelectedIndex = 0;
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {        
        _loaded = true;
        ResetWkt();
        RunRoundtrip();
    }

    private void geometryComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ResetWkt();
        if (_loaded)
            RunRoundtrip();
    }

    private void roundtripButton_Click(object? sender, EventArgs e) => RunRoundtrip();

    private void resetButton_Click(object? sender, EventArgs e)
    {
        ResetWkt();
        RunRoundtrip();
    }

    private void wktTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.Handled = true;
        e.SuppressKeyPress = true;
        RunRoundtrip();
    }

    private void ResetWkt()
    {
        wktTextBox.Text = (GeometryMode)geometryComboBox.SelectedIndex switch
        {
            GeometryMode.Point => PointWkt,
            GeometryMode.Polyline => PolylineWkt,
            _ => PolygonWkt
        };
    }

    private void RunRoundtrip()
    {
        var inputWkt = wktTextBox.Text.Trim();
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddOpenStreetMapLayer();

        try
        {
            var mode = (GeometryMode)geometryComboBox.SelectedIndex;
            var apiName = $"{ReadApiName(mode)} -> {WriteApiName(mode)}";
            string outputWkt;
            GeoKernelExtent viewExtent;
            int vertexCount;

            switch (mode)
            {
                case GeometryMode.Point:
                    var point = geoKernelViewerControl.ReadWktPoint(inputWkt);
                    outputWkt = geoKernelViewerControl.WriteWktPoint(point);
                    var webPoint = ToWebMercator(point);
                    geoKernelViewerControl.AddPointLayer("Roundtrip Point", [webPoint], PointStyle());
                    viewExtent = PaddedExtent([webPoint]);
                    vertexCount = 1;
                    break;
                case GeometryMode.Polyline:
                    var line = geoKernelViewerControl.ReadWktLineString(inputWkt);
                    outputWkt = geoKernelViewerControl.WriteWktLineString(line);
                    var webLine = line.Select(ToWebMercator).ToArray();
                    geoKernelViewerControl.AddPolylineLayer("Roundtrip Polyline", webLine, LineStyle());
                    viewExtent = PaddedExtent(webLine);
                    vertexCount = line.Count;
                    break;
                default:
                    var rings = geoKernelViewerControl.ReadWktPolygon(inputWkt);
                    outputWkt = geoKernelViewerControl.WriteWktPolygon(rings);
                    var webRings = rings
                        .Select(ring => (IReadOnlyList<GeoKernelPoint>)ring.Select(ToWebMercator).ToArray())
                        .ToArray();
                    geoKernelViewerControl.AddPolygonLayer("Roundtrip Polygon", webRings, PolygonStyle());
                    viewExtent = PaddedExtent(webRings.SelectMany(ring => ring).ToArray());
                    vertexCount = rings.Sum(ring => ring.Count);
                    break;
            }

            geoKernelViewerControl.ViewExtent = viewExtent;
            detailsTextBox.Text = DetailsText(apiName, inputWkt, outputWkt, vertexCount);
            statusLabel.Text = string.Equals(inputWkt, outputWkt, StringComparison.Ordinal)
                ? "Roundtrip completed. Output is identical."
                : "Roundtrip completed. Output is normalized by GisWktWriter.";
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            detailsTextBox.Text = $"WKT roundtrip failed:{Environment.NewLine}{ex.Message}";
            statusLabel.Text = "WKT roundtrip failed.";
        }
    }

    private static string DetailsText(string apiName, string inputWkt, string outputWkt, int vertexCount) =>
        string.Join(
            Environment.NewLine,
            "WktRoundtrip sample",
            "",
            "API",
            apiName,
            "",
            "Input WKT",
            inputWkt,
            "",
            "Output WKT",
            outputWkt,
            "",
            "Comparison",
            $"Identical: {string.Equals(inputWkt, outputWkt, StringComparison.Ordinal)}",
            $"Vertex count: {vertexCount}",
            "",
            "Note",
            "GisWktWriter can normalize formatting even when geometry is unchanged.");

    private static string ReadApiName(GeometryMode mode) =>
        mode switch
        {
            GeometryMode.Point => "GisWktReader::readPoint(wkt)",
            GeometryMode.Polyline => "GisWktReader::readLineString(wkt)",
            _ => "GisWktReader::readPolygon(wkt)"
        };

    private static string WriteApiName(GeometryMode mode) =>
        mode switch
        {
            GeometryMode.Point => "GisWktWriter::writePoint(shape)",
            GeometryMode.Polyline => "GisWktWriter::writePolyline(shape)",
            _ => "GisWktWriter::writePolygon(shape)"
        };

    private static GeoKernelLayerStyle PointStyle() => new()
    {
        PointColor = "#D95D39",
        LineColor = "#8C321D",
        PointSize = 13.0,
        LineWidth = 1.4
    };

    private static GeoKernelLayerStyle LineStyle() => new()
    {
        LineColor = "#E4572E",
        LineWidth = 3.4,
        PointColor = "#F3A712",
        PointSize = 7.0
    };

    private static GeoKernelLayerStyle PolygonStyle() => new()
    {
        FillColor = "#88D18A",
        FillOpacity = 130,
        LineColor = "#1F7A4D",
        LineWidth = 2.4
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

    private static GeoKernelExtent PaddedExtent(IReadOnlyList<GeoKernelPoint> points)
    {
        var xMin = points.Min(point => point.X);
        var yMin = points.Min(point => point.Y);
        var xMax = points.Max(point => point.X);
        var yMax = points.Max(point => point.Y);
        var paddingX = Math.Max(350_000.0, (xMax - xMin) * 0.45);
        var paddingY = Math.Max(350_000.0, (yMax - yMin) * 0.45);
        return new GeoKernelExtent(xMin - paddingX, yMin - paddingY, xMax + paddingX, yMax + paddingY);
    }

    private enum GeometryMode
    {
        Point,
        Polyline,
        Polygon
    }
}
