using GeoKernel.NET.WinForms;

namespace GeoKernel.WktOverlay.Winforms;

public sealed partial class MainForm : Form
{
    private const string PointWkt = "POINT(-122.4194 37.7749)";
    private const string LineWkt =
        "LINESTRING(-123.0 37.1, -122.5 37.8, -121.9 37.3, -121.2 38.0)";
    private const string PolygonWkt =
        "POLYGON((-123.25 37.15, -122.15 36.95, -121.55 37.65, -122.05 38.35, -123.05 38.15, -123.25 37.15))";

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        var point = geoKernelViewerControl.ReadWktPoint(PointWkt);
        var line = geoKernelViewerControl.ReadWktLineString(LineWkt);
        var polygon = geoKernelViewerControl.ReadWktPolygon(PolygonWkt);

        geoKernelViewerControl.AddPolygonLayer("WKT Polygons", polygon, PolygonStyle());
        geoKernelViewerControl.AddPolylineLayer("WKT Lines", line, LineStyle());
        geoKernelViewerControl.AddPointLayer("WKT Points", [point], PointStyle());

        detailsTextBox.Text = string.Join(
            Environment.NewLine,
            "WktOverlay sample",
            "",
            "API",
            "GisWktReader::readPoint/readLineString/readPolygon",
            "GisViewer::addLayer(layer)",
            "",
            "Three WKT strings are parsed and displayed as overlay layers.");
        statusLabel.Text = "WktOverlay ready.";
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-124.0, 36.4, -120.3, 38.7);
    }

    private static GeoKernelLayerStyle PointStyle() => new()
    {
        PointColor = "#D95D39",
        LineColor = "#8C321D",
        PointSize = 12.0
    };

    private static GeoKernelLayerStyle LineStyle() => new()
    {
        LineColor = "#E4572E",
        LineWidth = 3.0
    };

    private static GeoKernelLayerStyle PolygonStyle() => new()
    {
        FillColor = "#88D18A",
        FillOpacity = 128,
        LineColor = "#1F7A4D",
        LineWidth = 2.2
    };
}
