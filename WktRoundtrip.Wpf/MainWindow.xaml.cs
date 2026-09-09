using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.WktRoundtrip.Wpf;

public partial class MainWindow
{
    private const string InputWkt =
        "POLYGON((-123.25 37.15, -122.15 36.95, -121.55 37.65, -122.05 38.35, -123.05 38.15, -123.25 37.15))";

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        var polygon = viewerControl.ReadWktPolygon(InputWkt);
        var outputWkt = viewerControl.WriteWktPolygon(polygon);

        viewerControl.AddPolygonLayer("Roundtrip Polygon", polygon, PolygonStyle());
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
        statusText.Text = "WktRoundtrip ready.";
        viewerControl.ViewExtent = new GeoKernelExtent(-124.0, 36.4, -120.3, 38.7);
    }

    private static GeoKernelLayerStyle PolygonStyle() => new()
    {
        FillColor = "#88D18A",
        FillOpacity = 128,
        LineColor = "#1F7A4D",
        LineWidth = 2.2
    };
}
