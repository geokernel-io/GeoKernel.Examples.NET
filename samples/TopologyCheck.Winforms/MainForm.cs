using GeoKernel.NET.WinForms;

namespace GeoKernel.TopologyCheck.Winforms;

public sealed partial class MainForm : Form
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

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        RenderScene(checkedShape: false);
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void runCheckButton_Click(object? sender, EventArgs e)
    {
        RenderScene(checkedShape: true);
    }

    private void RenderScene(bool checkedShape)
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddPolygonLayer("A - Valid Polygon", ValidPolygon, PolygonStyle("#BFD7EA", "#2F80C2"));
        geoKernelViewerControl.AddPolygonLayer("B - Self-intersecting Polygon", SelfIntersectingPolygon, PolygonStyle("#F6D6AD", "#D95D39"));

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
            var validOk = geoKernelViewerControl.CheckPolygonRing(ValidPolygon);
            var bowTieOk = geoKernelViewerControl.CheckPolygonRing(SelfIntersectingPolygon);

            geoKernelViewerControl.AddPolygonLayer("A - CheckShape: valid", ValidPolygon, CheckedStyle("#CDE7D8", "#2A9D8F"));
            geoKernelViewerControl.AddPolygonLayer("B - CheckShape: invalid", SelfIntersectingPolygon, CheckedStyle("#F4A261", "#D62828"));

            details +=
                $"{Environment.NewLine}{Environment.NewLine}Result:" +
                $"{Environment.NewLine}A - valid polygon: CheckShape = {BoolText(validOk)}" +
                $"{Environment.NewLine}B - self-intersecting polygon: CheckShape = {BoolText(bowTieOk)}" +
                $"{Environment.NewLine}{Environment.NewLine}Invalid means the geometry should be fixed or rejected before topology operations.";

            statusLabel.Text = "Topology check completed.";
        }
        else
        {
            details += $"{Environment.NewLine}{Environment.NewLine}Click Run CheckShape to validate both polygons.";
            statusLabel.Text = "Two polygons are ready. Click Run CheckShape.";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.8, -2.7, 5.9, 2.4);
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
