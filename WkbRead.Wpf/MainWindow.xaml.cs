using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.WkbRead.Wpf;

public partial class MainWindow
{
    private const string PointWkb =
        "010100000050FC1873D79A5EC0D0D556EC2FE34240";

    private const string LineStringWkb =
        "01020000000400000050FC1873D79A5EC0D0D556EC2FE34240789CA223B9785EC0ECC039234AAB4240" +
        "1DC9E53FA45F5EC043AD69DE714A434041F163CC5D2F5EC0D26F5F07CED14240";

    private const string PolygonWkb =
        "010300000001000000060000000000000000D05EC033333333339342409A99999999895EC09A999999" +
        "997942403333333333635EC03333333333D342403333333333835EC0CDCCCCCCCC2C43403333333333" +
        "C35EC033333333331343400000000000D05EC03333333333934240";

    private const string MultiPolygonWkb =
        "010600000002000000010300000001000000060000000000000000D05EC033333333339342400000000" +
        "000905EC09A999999997942406666666666765EC03333333333D34240CDCCCCCCCC9C5EC09A999999" +
        "991943409A99999999C95EC09A99999999F942400000000000D05EC033333333339342400103000000" +
        "01000000050000006666666666665EC00000000000604240CDCCCCCCCC2C5EC09A99999999594240CD" +
        "CCCCCCCC1C5EC0CDCCCCCCCCAC42400000000000505EC03333333333D342406666666666665EC00000" +
        "000000604240";

    private bool _loaded;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        _loaded = true;
        ResetWkb();
        ParseAndRender();
    }

    private void ReadWkb_Click(object sender, RoutedEventArgs e) => ParseAndRender();

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        ResetWkb();
        ParseAndRender();
    }

    private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (readButton is null)
            return;

        readButton.Content = $"Read {GeometryName}";
        ResetWkb();
        if (_loaded)
            ParseAndRender();
    }

    private void WkbTextBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
            return;

        e.Handled = true;
        ParseAndRender();
    }

    private string GeometryName => modeComboBox.SelectedIndex switch
    {
        1 => "LineString",
        2 => "Polygon",
        3 => "MultiPolygon",
        _ => "Point"
    };

    private void ResetWkb()
    {
        wkbTextBox.Text = modeComboBox.SelectedIndex switch
        {
            1 => LineStringWkb,
            2 => PolygonWkb,
            3 => MultiPolygonWkb,
            _ => PointWkb
        };
    }

    private void ParseAndRender()
    {
        var inputHex = wkbTextBox.Text.Trim();
        viewerControl.ClearLayers();

        try
        {
            var wkb = ParseHexWkb(inputHex);
            viewerControl.AddOpenStreetMapLayer();

            var lonLatParts = ParseGeometry(wkb);
            var allLonLatPoints = lonLatParts.SelectMany(part => part).ToArray();
            var webMercatorParts = lonLatParts
                .Select(part => (IReadOnlyList<GeoKernelPoint>)part.Select(ToWebMercator).ToArray())
                .ToArray();
            var viewExtent = PaddedExtent(webMercatorParts.SelectMany(part => part).ToArray());

            AddGeometryLayer(webMercatorParts);
            viewerControl.ViewExtent = viewExtent;

            detailsTextBox.Text = DetailsText(inputHex, wkb.Length, allLonLatPoints, lonLatParts.Count, viewExtent);
            statusText.Text = $"GisWkbReader::read parsed {GeometryName} from {wkb.Length} bytes.";
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            detailsTextBox.Text = $"WKB parse failed:{Environment.NewLine}{ex.Message}";
            statusText.Text = "WKB parse failed.";
        }
    }

    private IReadOnlyList<IReadOnlyList<GeoKernelPoint>> ParseGeometry(byte[] wkb) =>
        modeComboBox.SelectedIndex switch
        {
            1 => [viewerControl.ReadWkbLineString(wkb)],
            2 => viewerControl.ReadWkbPolygon(wkb),
            3 => viewerControl.ReadWkbPolygon(wkb, multiPolygon: true),
            _ => [[viewerControl.ReadWkbPoint(wkb)]]
        };

    private void AddGeometryLayer(IReadOnlyList<IReadOnlyList<GeoKernelPoint>> parts)
    {
        switch (modeComboBox.SelectedIndex)
        {
            case 1:
                viewerControl.AddPolylineLayer("WKB LineString", parts, LineStyle());
                break;
            case 2:
            case 3:
                viewerControl.AddPolygonLayer("WKB Polygon", parts, PolygonStyle());
                break;
            default:
                viewerControl.AddPointLayer("WKB Point", parts[0], PointStyle());
                break;
        }
    }

    private string DetailsText(
        string inputHex,
        int byteCount,
        IReadOnlyList<GeoKernelPoint> lonLatPoints,
        int partCount,
        GeoKernelExtent webMercatorExtent) =>
        string.Join(
            Environment.NewLine,
            "WkbRead sample",
            "",
            "API",
            "GisWkbReader::read(byteArray)",
            "",
            "Input WKB",
            $"Hex characters: {inputHex.Count(c => !char.IsWhiteSpace(c))}",
            $"Byte count: {byteCount}",
            "",
            "Parsed geometry",
            $"Type: {GeometryName}",
            $"Parts/rings: {partCount}",
            $"Vertices: {lonLatPoints.Count}",
            $"Lon/lat extent: {ExtentText(PaddedExtent(lonLatPoints, 0.0, 0.0))}",
            $"Centroid: {Centroid(lonLatPoints).X:F6}, {Centroid(lonLatPoints).Y:F6}",
            "",
            "Displayed over OSM",
            "Input CRS: EPSG:4326",
            "Viewer/OSM CRS: EPSG:3857",
            $"WebMercator view extent: {ExtentText(webMercatorExtent)}",
            "",
            "Input hex",
            inputHex);

    private static byte[] ParseHexWkb(string text)
    {
        var cleaned = new string(text.Where(c => !char.IsWhiteSpace(c)).ToArray());
        if (cleaned.Length == 0)
            throw new FormatException("WKB hex input is empty.");
        if ((cleaned.Length % 2) != 0)
            throw new FormatException("WKB hex input must contain an even number of characters.");

        try
        {
            return Convert.FromHexString(cleaned);
        }
        catch (FormatException)
        {
            throw new FormatException("WKB input must be hexadecimal.");
        }
    }

    private static GeoKernelLayerStyle PointStyle() => new()
    {
        FillColor = "#D95F35",
        FillOpacity = 220,
        LineColor = "#6B2A17",
        LineWidth = 1.5,
        PointSize = 13
    };

    private static GeoKernelLayerStyle LineStyle() => new()
    {
        LineColor = "#22668D",
        LineWidth = 3.0
    };

    private static GeoKernelLayerStyle PolygonStyle() => new()
    {
        FillColor = "#88D18A",
        FillOpacity = 130,
        LineColor = "#1F7A4D",
        LineWidth = 2.5
    };

    private static GeoKernelPoint Centroid(IReadOnlyList<GeoKernelPoint> points) =>
        new(points.Average(point => point.X), points.Average(point => point.Y));

    private static GeoKernelPoint ToWebMercator(GeoKernelPoint lonLat)
    {
        const double originShift = 20037508.342789244;
        var lon = Math.Clamp(lonLat.X, -180.0, 180.0);
        var lat = Math.Clamp(lonLat.Y, -85.05112878, 85.05112878);
        var x = lon * originShift / 180.0;
        var y = Math.Log(Math.Tan((90.0 + lat) * Math.PI / 360.0)) * originShift / Math.PI;
        return new GeoKernelPoint(x, y);
    }

    private static GeoKernelExtent PaddedExtent(
        IReadOnlyList<GeoKernelPoint> points,
        double paddingRatio = 0.35,
        double minimumPadding = 300_000.0)
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
