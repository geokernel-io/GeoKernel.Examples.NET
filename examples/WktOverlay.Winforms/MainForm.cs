using GeoKernel.NET.WinForms;

namespace GeoKernel.WktOverlay.Winforms;

public sealed partial class MainForm : Form
{
    private const string DefaultWkt = """
        POLYGON((-123.25 37.15, -122.15 36.95, -121.55 37.65, -122.05 38.35, -123.05 38.15, -123.25 37.15))
        LINESTRING(-123.00 37.10, -122.55 37.65, -122.05 37.30, -121.55 38.10, -120.90 37.55)
        POINT(-122.4194 37.7749)
        """;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {        
        ResetInput();
        RenderOverlay();
    }

    private void renderButton_Click(object? sender, EventArgs e) => RenderOverlay();

    private void resetButton_Click(object? sender, EventArgs e)
    {
        ResetInput();
        RenderOverlay();
    }

    private void ResetInput()
    {
        wktTextBox.Text = DefaultWkt.Replace("\n", Environment.NewLine);
    }

    private void RenderOverlay()
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddOpenStreetMapLayer();

        var details = new List<string>
        {
            "WktOverlay sample",
            "",
            "API",
            "ReadWktPoint / ReadWktLineString / ReadWktPolygon",
            "AddPointLayer / AddPolylineLayer / AddPolygonLayer",
            "",
            "Rendered geometries"
        };
        var allWebPoints = new List<GeoKernelPoint>();
        var renderedCount = 0;

        foreach (var line in WktLines())
        {
            try
            {
                if (line.StartsWith("POINT", StringComparison.OrdinalIgnoreCase))
                {
                    var point = geoKernelViewerControl.ReadWktPoint(line);
                    var webPoint = ToWebMercator(point);
                    geoKernelViewerControl.AddPointLayer("WKT Points", [webPoint], PointStyle());
                    allWebPoints.Add(webPoint);
                    details.Add($"Point: {line}");
                }
                else if (line.StartsWith("LINESTRING", StringComparison.OrdinalIgnoreCase))
                {
                    var points = geoKernelViewerControl.ReadWktLineString(line);
                    var webPoints = points.Select(ToWebMercator).ToArray();
                    geoKernelViewerControl.AddPolylineLayer("WKT Lines", webPoints, LineStyle());
                    allWebPoints.AddRange(webPoints);
                    details.Add($"LineString vertices: {points.Count}");
                }
                else if (line.StartsWith("POLYGON", StringComparison.OrdinalIgnoreCase))
                {
                    var rings = geoKernelViewerControl.ReadWktPolygon(line);
                    var webRings = rings
                        .Select(ring => (IReadOnlyList<GeoKernelPoint>)ring.Select(ToWebMercator).ToArray())
                        .ToArray();
                    geoKernelViewerControl.AddPolygonLayer("WKT Polygons", webRings, PolygonStyle());
                    allWebPoints.AddRange(webRings.SelectMany(ring => ring));
                    details.Add($"Polygon rings: {rings.Count}; vertices: {rings.Sum(ring => ring.Count)}");
                }
                else
                {
                    details.Add($"Skipped unsupported WKT: {line}");
                    continue;
                }

                ++renderedCount;
            }
            catch (Exception ex) when (ex is FormatException or ArgumentException)
            {
                details.Add($"Failed: {line}");
                details.Add(ex.Message);
            }
        }

        if (allWebPoints.Count > 0)
            geoKernelViewerControl.ViewExtent = PaddedExtent(allWebPoints);

        detailsTextBox.Text = string.Join(Environment.NewLine, details);
        statusLabel.Text = $"Rendered {renderedCount} WKT overlay geometries.";
    }

    private IEnumerable<string> WktLines() =>
        wktTextBox.Lines
            .Select(line => line.Trim())
            .Where(line => !string.IsNullOrWhiteSpace(line));

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
        LineWidth = 3.2,
        PointColor = "#F3A712",
        PointSize = 6.5
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
}
