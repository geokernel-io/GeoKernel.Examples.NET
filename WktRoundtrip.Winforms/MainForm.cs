using GeoKernel.NET.WinForms;

namespace GeoKernel.WktRoundtrip.Winforms;

public sealed partial class MainForm : Form
{
    private const string InputWkt =
        "POLYGON((-123.25 37.15, -122.15 36.95, -121.55 37.65, -122.05 38.35, -123.05 38.15, -123.25 37.15))";

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        var polygon = geoKernelViewerControl.ReadWktPolygon(InputWkt);
        var outputWkt = geoKernelViewerControl.WriteWktPolygon(polygon);

        geoKernelViewerControl.AddPolygonLayer("Roundtrip Polygon", polygon, PolygonStyle());
        detailsTextBox.Text = string.Join(
            Environment.NewLine,
            "WktRoundtrip sample",
            "",
            "API",
            "GisWktReader::readPolygon(wkt)",
            "GisWktWriter::writePolygon(shape)",
            "",
            "Input WKT",
            InputWkt,
            "",
            "Output WKT",
            outputWkt);
        statusLabel.Text = "WktRoundtrip ready.";
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-124.0, 36.4, -120.3, 38.7);
    }

    private static GeoKernelLayerStyle PolygonStyle() => new()
    {
        FillColor = "#88D18A",
        FillOpacity = 128,
        LineColor = "#1F7A4D",
        LineWidth = 2.2
    };
}
