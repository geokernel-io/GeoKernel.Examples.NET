using System.Windows;
using System.Windows.Controls;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.WkbWrite.Wpf;

public partial class MainWindow
{
    private static readonly GeoKernelPoint Point = new(-122.4194, 37.7749);

    private static readonly GeoKernelPoint[] Polyline =
    [
        new(-123.00, 37.10),
        new(-122.55, 37.65),
        new(-122.05, 37.30),
        new(-121.55, 38.10),
        new(-120.90, 37.55)
    ];

    private static readonly GeoKernelPoint[] Polygon =
    [
        new(-123.25, 37.15),
        new(-122.15, 36.95),
        new(-121.55, 37.65),
        new(-122.05, 38.35),
        new(-123.05, 38.15),
        new(-123.25, 37.15)
    ];

    private bool _loaded;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        _loaded = true;
        RenderSelectedGeometry();
    }

    private void GeometryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loaded)
            RenderSelectedGeometry();
    }

    private void Reset_Click(object sender, RoutedEventArgs e) => RenderSelectedGeometry();

    private void RenderSelectedGeometry()
    {
        viewerControl.ClearLayers();
        viewerControl.AddOpenStreetMapLayer();

        var mode = (GeometryMode)geometryComboBox.SelectedIndex;
        string apiName;
        byte[] wkb;
        GeoKernelExtent viewExtent;

        switch (mode)
        {
            case GeometryMode.Point:
                apiName = "GisWkbWriter::writePoint(shape)";
                wkb = viewerControl.WriteWkbPoint(Point);
                var webPoint = ToWebMercator(Point);
                viewerControl.AddPointLayer("WKB Point", [webPoint], PointStyle());
                viewExtent = PaddedExtent([webPoint]);
                break;
            case GeometryMode.Polyline:
                apiName = "GisWkbWriter::writePolyline(shape)";
                wkb = viewerControl.WriteWkbLineString(Polyline);
                var webLine = Polyline.Select(ToWebMercator).ToArray();
                viewerControl.AddPolylineLayer("WKB Polyline", webLine, LineStyle());
                viewExtent = PaddedExtent(webLine);
                break;
            default:
                apiName = "GisWkbWriter::writePolygon(shape)";
                wkb = viewerControl.WriteWkbPolygon(Polygon);
                var webRing = Polygon.Select(ToWebMercator).ToArray();
                viewerControl.AddPolygonLayer("WKB Polygon", webRing, PolygonStyle());
                viewExtent = PaddedExtent(webRing);
                break;
        }

        viewerControl.ViewExtent = viewExtent;
        detailsTextBox.Text = DetailsText(mode, apiName, wkb);
        statusText.Text = $"{apiName} wrote {wkb.Length} WKB bytes.";
    }

    private static string DetailsText(GeometryMode mode, string apiName, byte[] wkb) =>
        string.Join(
            Environment.NewLine,
            "WkbWrite sample",
            "",
            "API",
            apiName,
            "",
            "Geometry",
            mode,
            "",
            "Output WKB",
            $"Byte count: {wkb.Length}",
            "Endian: little endian",
            "",
            "Hex view",
            Convert.ToHexString(wkb).Chunk(2).Select(chunk => new string(chunk)).Aggregate(string.Empty, (text, pair) => text.Length == 0 ? pair : $"{text} {pair}"),
            "",
            "Workflow",
            "1. Choose a geometry type.",
            "2. The sample creates a GeoKernel shape.",
            "3. The wrapper calls the native GisWkbWriter.",
            "4. The generated WKB byte array is shown as hexadecimal.");

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
