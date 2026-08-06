using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GeoJsonRead.Wpf;

public partial class MainWindow
{
    private const string PointGeoJson =
        """
        {"type":"Point","coordinates":[-122.4194,37.7749]}
        """;

    private const string LineStringGeoJson =
        """
        {"type":"LineString","coordinates":[[-123.10,37.15],[-122.55,37.75],[-121.80,37.25],[-121.30,38.05],[-120.70,37.70]]}
        """;

    private const string PolygonGeoJson =
        """
        {"type":"Polygon","coordinates":[[[-123.25,37.15],[-122.15,36.95],[-121.55,37.65],[-122.05,38.35],[-123.05,38.15],[-123.25,37.15]]]}
        """;

    private bool _loaded;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        _loaded = true;
        ResetGeoJson();
        ParseAndRender();
    }

    private void ReadGeoJson_Click(object sender, RoutedEventArgs e) => ParseAndRender();

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        ResetGeoJson();
        ParseAndRender();
    }

    private void ModeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (readButton is null)
            return;

        readButton.Content = $"Read {GeometryName}";
        ResetGeoJson();
        if (_loaded)
            ParseAndRender();
    }

    private void GeoJsonTextBox_KeyDown(object sender, KeyEventArgs e)
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
        _ => "Point"
    };

    private void ResetGeoJson()
    {
        geoJsonTextBox.Text = modeComboBox.SelectedIndex switch
        {
            1 => LineStringGeoJson,
            2 => PolygonGeoJson,
            _ => PointGeoJson
        };
    }

    private void ParseAndRender()
    {
        var input = geoJsonTextBox.Text.Trim();
        viewerControl.ClearLayers();

        try
        {
            viewerControl.AddOpenStreetMapLayer();

            var lonLatPoints = ParseGeometry(input);
            var allLonLatPoints = lonLatPoints.SelectMany(part => part).ToArray();
            var webMercatorParts = lonLatPoints
                .Select(part => (IReadOnlyList<GeoKernelPoint>)part.Select(ToWebMercator).ToArray())
                .ToArray();
            var viewExtent = PaddedExtent(webMercatorParts.SelectMany(part => part).ToArray());

            AddGeometryLayer(webMercatorParts);
            viewerControl.ViewExtent = viewExtent;

            detailsTextBox.Text = DetailsText(input, allLonLatPoints, lonLatPoints.Count, viewExtent);
            statusText.Text = $"GisGeoJsonReader::read parsed {GeometryName} with {allLonLatPoints.Length} vertices.";
        }
        catch (Exception ex) when (ex is FormatException or ArgumentException)
        {
            detailsTextBox.Text = $"GeoJSON parse failed:{Environment.NewLine}{ex.Message}";
            statusText.Text = "GeoJSON parse failed.";
        }
    }

    private IReadOnlyList<IReadOnlyList<GeoKernelPoint>> ParseGeometry(string input) =>
        modeComboBox.SelectedIndex switch
        {
            1 => [viewerControl.ReadGeoJsonLineString(input)],
            2 => viewerControl.ReadGeoJsonPolygon(input),
            _ => [[viewerControl.ReadGeoJsonPoint(input)]]
        };

    private void AddGeometryLayer(IReadOnlyList<IReadOnlyList<GeoKernelPoint>> parts)
    {
        switch (modeComboBox.SelectedIndex)
        {
            case 1:
                viewerControl.AddPolylineLayer("GeoJSON LineString", parts, LineStyle());
                break;
            case 2:
                viewerControl.AddPolygonLayer("GeoJSON Polygon", parts, PolygonStyle());
                break;
            default:
                viewerControl.AddPointLayer("GeoJSON Point", parts[0], PointStyle());
                break;
        }
    }

    private string DetailsText(
        string inputGeoJson,
        IReadOnlyList<GeoKernelPoint> lonLatPoints,
        int partCount,
        GeoKernelExtent webMercatorExtent) =>
        string.Join(
            Environment.NewLine,
            "GeoJsonRead sample",
            "",
            "API",
            "GisGeoJsonReader::read(jsonString)",
            "",
            "Input GeoJSON",
            inputGeoJson,
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
            $"WebMercator view extent: {ExtentText(webMercatorExtent)}");

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
