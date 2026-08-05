using GeoKernel.NET.WinForms;

namespace GeoKernel.GeoJsonRoundtrip.Winforms;

public sealed partial class MainForm : Form
{
    private const string PointGeoJson = "{\"type\":\"Point\",\"coordinates\":[-122.4194,37.7749]}";

    private const string PolylineGeoJson =
        "{\"type\":\"LineString\",\"coordinates\":[[-123.00,37.10],[-122.55,37.65],[-122.05,37.30],[-121.55,38.10],[-120.90,37.55]]}";

    private const string PolygonGeoJson =
        "{\"type\":\"Polygon\",\"coordinates\":[[[-123.25,37.15],[-122.15,36.95],[-121.55,37.65],[-122.05,38.35],[-123.05,38.15],[-123.25,37.15]]]}";

    private bool _loaded;

    public MainForm()
    {
        InitializeComponent();
        geometryComboBox.SelectedIndex = 0;
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {        
        _loaded = true;
        ResetGeoJson();
        RunRoundtrip();
    }

    private void geometryComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        ResetGeoJson();
        if (_loaded)
            RunRoundtrip();
    }

    private void roundtripButton_Click(object? sender, EventArgs e) => RunRoundtrip();

    private void resetButton_Click(object? sender, EventArgs e)
    {
        ResetGeoJson();
        RunRoundtrip();
    }

    private void geoJsonTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter)
            return;

        e.Handled = true;
        e.SuppressKeyPress = true;
        RunRoundtrip();
    }

    private void ResetGeoJson()
    {
        geoJsonTextBox.Text = (GeometryMode)geometryComboBox.SelectedIndex switch
        {
            GeometryMode.Point => PointGeoJson,
            GeometryMode.Polyline => PolylineGeoJson,
            _ => PolygonGeoJson
        };
    }

    private void RunRoundtrip()
    {
        var inputGeoJson = geoJsonTextBox.Text.Trim();
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddOpenStreetMapLayer();

        try
        {
            var mode = (GeometryMode)geometryComboBox.SelectedIndex;
            var apiName = $"{GeoJsonReadApiName(mode)} -> {GeoJsonWriteApiName(mode)}";
            string outputGeoJson;
            GeoKernelExtent viewExtent;
            int vertexCount;

            switch (mode)
            {
                case GeometryMode.Point:
                    var point = geoKernelViewerControl.ReadGeoJsonPoint(inputGeoJson);
                    outputGeoJson = geoKernelViewerControl.WriteGeoJsonPoint(point);
                    var webPoint = ToWebMercator(point);
                    geoKernelViewerControl.AddPointLayer("Roundtrip Point", [webPoint], PointStyle());
                    viewExtent = PaddedExtent([webPoint]);
                    vertexCount = 1;
                    break;
                case GeometryMode.Polyline:
                    var line = geoKernelViewerControl.ReadGeoJsonLineString(inputGeoJson);
                    outputGeoJson = geoKernelViewerControl.WriteGeoJsonLineString(line);
                    var webLine = line.Select(ToWebMercator).ToArray();
                    geoKernelViewerControl.AddPolylineLayer("Roundtrip Polyline", webLine, LineStyle());
                    viewExtent = PaddedExtent(webLine);
                    vertexCount = line.Count;
                    break;
                default:
                    var rings = geoKernelViewerControl.ReadGeoJsonPolygon(inputGeoJson);
                    outputGeoJson = geoKernelViewerControl.WriteGeoJsonPolygon(rings);
                    var webRings = rings
                        .Select(ring => (IReadOnlyList<GeoKernelPoint>)ring.Select(ToWebMercator).ToArray())
                        .ToArray();
                    geoKernelViewerControl.AddPolygonLayer("Roundtrip Polygon", webRings, PolygonStyle());
                    viewExtent = PaddedExtent(webRings.SelectMany(ring => ring).ToArray());
                    vertexCount = rings.Sum(ring => ring.Count);
                    break;
            }

            geoKernelViewerControl.ViewExtent = viewExtent;
            detailsTextBox.Text = DetailsText(apiName, inputGeoJson, outputGeoJson, vertexCount);
            statusLabel.Text = string.Equals(inputGeoJson, outputGeoJson, StringComparison.Ordinal)
                ? "Roundtrip completed. Output is identical."
                : "Roundtrip completed. Output is normalized by GisGeoJsonWriter.";
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            detailsTextBox.Text = $"GeoJSON roundtrip failed:{Environment.NewLine}{ex.Message}";
            statusLabel.Text = "GeoJSON roundtrip failed.";
        }
    }

    private static string DetailsText(string apiName, string inputGeoJson, string outputGeoJson, int vertexCount) =>
        string.Join(
            Environment.NewLine,
            "GeoJsonRoundtrip sample",
            "",
            "API",
            apiName,
            "",
            "Input GeoJSON",
            inputGeoJson,
            "",
            "Output GeoJSON",
            outputGeoJson,
            "",
            "Comparison",
            $"Identical: {string.Equals(inputGeoJson, outputGeoJson, StringComparison.Ordinal)}",
            $"Vertex count: {vertexCount}",
            "",
            "Note",
            "GisGeoJsonWriter can normalize formatting even when geometry is unchanged.");

    private static string GeoJsonReadApiName(GeometryMode mode) =>
        mode switch
        {
            GeometryMode.Point => "GisGeoJsonReader::readPoint(geoJson)",
            GeometryMode.Polyline => "GisGeoJsonReader::readLineString(geoJson)",
            _ => "GisGeoJsonReader::readPolygon(geoJson)"
        };

    private static string GeoJsonWriteApiName(GeometryMode mode) =>
        mode switch
        {
            GeometryMode.Point => "GisGeoJsonWriter::writePoint(shape)",
            GeometryMode.Polyline => "GisGeoJsonWriter::writePolyline(shape)",
            _ => "GisGeoJsonWriter::writePolygon(shape)"
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
