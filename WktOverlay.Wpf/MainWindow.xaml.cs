using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.WktOverlay.Wpf;

public partial class MainWindow
{
    private const string DefaultWkt = """
        POLYGON((-123.25 37.15, -122.15 36.95, -121.55 37.65, -122.05 38.35, -123.05 38.15, -123.25 37.15))
        LINESTRING(-123.00 37.10, -122.55 37.65, -122.05 37.30, -121.55 38.10, -120.90 37.55)
        POINT(-122.4194 37.7749)
        """;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        ResetInput();
        RenderOverlay();
    }

    private void RenderOverlay_Click(object sender, RoutedEventArgs e) => RenderOverlay();

    private void Reset_Click(object sender, RoutedEventArgs e)
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
        viewerControl.ClearLayers();
        viewerControl.AddOpenStreetMapLayer();

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
                    var point = viewerControl.ReadWktPoint(line);
                    var webPoint = ToWebMercator(point);
                    viewerControl.AddPointLayer("WKT Points", [webPoint], PointStyle());
                    allWebPoints.Add(webPoint);
                    details.Add($"Point: {line}");
                }
                else if (line.StartsWith("LINESTRING", StringComparison.OrdinalIgnoreCase))
                {
                    var points = viewerControl.ReadWktLineString(line);
                    var webPoints = points.Select(ToWebMercator).ToArray();
                    viewerControl.AddPolylineLayer("WKT Lines", webPoints, LineStyle());
                    allWebPoints.AddRange(webPoints);
                    details.Add($"LineString vertices: {points.Count}");
                }
                else if (line.StartsWith("POLYGON", StringComparison.OrdinalIgnoreCase))
                {
                    var rings = viewerControl.ReadWktPolygon(line);
                    var webRings = rings
                        .Select(ring => (IReadOnlyList<GeoKernelPoint>)ring.Select(ToWebMercator).ToArray())
                        .ToArray();
                    viewerControl.AddPolygonLayer("WKT Polygons", webRings, PolygonStyle());
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
            viewerControl.ViewExtent = PaddedExtent(allWebPoints);

        detailsTextBox.Text = string.Join(Environment.NewLine, details);
        statusText.Text = $"Rendered {renderedCount} WKT overlay geometries.";
    }

    private IEnumerable<string> WktLines() =>
        wktTextBox.Text
            .Split(["\r\n", "\n"], StringSplitOptions.None)
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
