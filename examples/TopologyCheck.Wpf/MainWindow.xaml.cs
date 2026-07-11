using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.TopologyCheck.Wpf;

public partial class MainWindow
{
    private static readonly GeoKernelPoint[] ValidPolygon =
    [
        new(-5.0, -1.6),
        new(-2.0, -1.6),
        new(-2.0, 1.4),
        new(-5.0, 1.4),
        new(-5.0, -1.6)
    ];

    private static readonly GeoKernelPoint[] SelfIntersectingPolygon =
    [
        new(0.0, -1.6),
        new(3.3, 1.4),
        new(0.0, 1.4),
        new(3.3, -1.6),
        new(0.0, -1.6)
    ];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        RenderScene(checkedShape: false);
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void RunCheck_Click(object sender, RoutedEventArgs e)
    {
        RenderScene(checkedShape: true);
    }

    private void RenderScene(bool checkedShape)
    {
        viewerControl.ClearLayers();
        viewerControl.AddPolygonLayer("A - Valid Polygon", ValidPolygon, PolygonStyle("#BFD7EA", "#2F80C2"));
        viewerControl.AddPolygonLayer("B - Self-intersecting Polygon", SelfIntersectingPolygon, PolygonStyle("#F6D6AD", "#D95D39"));

        var details =
            $"CheckShape - geometry validation{Environment.NewLine}{Environment.NewLine}" +
            $"This sample compares two polygon rings:{Environment.NewLine}{Environment.NewLine}" +
            $"A - valid polygon{Environment.NewLine}" +
            $"Closed ring, non-zero area, no self-intersection.{Environment.NewLine}" +
            $"Extent: {ExtentText(ValidPolygon)}{Environment.NewLine}{Environment.NewLine}" +
            $"B - self-intersecting polygon{Environment.NewLine}" +
            $"Bow-tie ring crosses itself, so CheckShape must reject it.{Environment.NewLine}" +
            $"Extent: {ExtentText(SelfIntersectingPolygon)}";

        if (checkedShape)
        {
            var validOk = viewerControl.CheckPolygonRing(ValidPolygon);
            var bowTieOk = viewerControl.CheckPolygonRing(SelfIntersectingPolygon);

            viewerControl.AddPolygonLayer("A - CheckShape: valid", ValidPolygon, CheckedStyle("#CDE7D8", "#2A9D8F"));
            viewerControl.AddPolygonLayer("B - CheckShape: invalid", SelfIntersectingPolygon, CheckedStyle("#F4A261", "#D62828"));

            details +=
                $"{Environment.NewLine}{Environment.NewLine}Result:" +
                $"{Environment.NewLine}A - valid polygon: CheckShape = {BoolText(validOk)}" +
                $"{Environment.NewLine}B - self-intersecting polygon: CheckShape = {BoolText(bowTieOk)}" +
                $"{Environment.NewLine}{Environment.NewLine}Invalid means the geometry should be fixed or rejected before topology operations.";

            statusText.Text = "Topology check completed.";
        }
        else
        {
            details += $"{Environment.NewLine}{Environment.NewLine}Click Run CheckShape to validate both polygons.";
            statusText.Text = "Two polygons are ready. Click Run CheckShape.";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-5.8, -2.7, 5.9, 2.4);
    }

    private static string BoolText(bool value) => value ? "valid" : "invalid";

    private static string ExtentText(IReadOnlyList<GeoKernelPoint> points)
    {
        var minX = points.Min(point => point.X);
        var minY = points.Min(point => point.Y);
        var maxX = points.Max(point => point.X);
        var maxY = points.Max(point => point.Y);
        return $"({minX:F2}, {minY:F2}) - ({maxX:F2}, {maxY:F2})";
    }

    private static GeoKernelLayerStyle PolygonStyle(string fill, string line) => new()
    {
        FillColor = fill,
        FillOpacity = 125,
        LineColor = line,
        LineWidth = 2.4
    };

    private static GeoKernelLayerStyle CheckedStyle(string fill, string line) => new()
    {
        FillColor = fill,
        FillOpacity = 165,
        LineColor = line,
        LineWidth = 4.0
    };
}
