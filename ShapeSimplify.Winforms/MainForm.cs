using GeoKernel.NET.WinForms;

namespace GeoKernel.ShapeSimplify.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] SourcePolygon =
    [
        new(-5.8, -1.8), new(-5.4, -0.6), new(-4.9, 0.2), new(-4.2, 1.0),
        new(-3.5, 1.6), new(-2.7, 1.9), new(-2.0, 1.5), new(-1.2, 2.1),
        new(-0.3, 1.7), new(0.5, 2.0), new(1.4, 1.2), new(2.2, 1.4),
        new(3.0, 0.6), new(3.8, 0.9), new(4.7, 0.1), new(5.2, -0.9),
        new(4.2, -1.4), new(3.1, -1.1), new(2.1, -1.8), new(1.1, -1.3),
        new(0.1, -2.0), new(-0.9, -1.5), new(-1.9, -2.1), new(-2.8, -1.5),
        new(-3.8, -1.9), new(-4.7, -1.2), new(-5.8, -1.8)
    ];

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        RenderScene();
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void toleranceTrackBar_ValueChanged(object? sender, EventArgs e)
    {
        RenderScene();
    }

    private void RenderScene()
    {
        if (!IsHandleCreated)
            return;

        var tolerance = toleranceTrackBar.Value / 100.0;
        toleranceValueLabel.Text = $"{tolerance:F2} units";

        geoKernelViewerControl.ClearShapes();
        geoKernelViewerControl.AddPolygonShape(SourcePolygon, SourceStyle());

        var simplified = geoKernelViewerControl.SimplifyPolygonRing(SourcePolygon, tolerance);
        if (simplified.Count > 0)
            geoKernelViewerControl.AddPolygonShape(simplified, ResultStyle());

        detailsTextBox.Text =
            $"shape.simplify(tolerance){Environment.NewLine}" +
            $"Algorithm: Douglas-Peucker{Environment.NewLine}{Environment.NewLine}" +
            $"Tolerance: {tolerance:F2} map units{Environment.NewLine}" +
            $"Source polygon vertices: {SourcePolygon.Length}{Environment.NewLine}" +
            $"Source extent: {ExtentText(SourcePolygon)}{Environment.NewLine}{Environment.NewLine}" +
            $"Simplified polygon vertices: {simplified.Count}{Environment.NewLine}" +
            $"Removed vertices: {SourcePolygon.Length - simplified.Count}{Environment.NewLine}" +
            $"Simplified extent: {ExtentText(simplified)}";

        statusLabel.Text = $"Simplify applied with tolerance {tolerance:F2}.";
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-7.2, -3.0, 6.8, 3.1);
    }

    private static string ExtentText(IReadOnlyList<GeoKernelPoint> points)
    {
        if (points.Count == 0)
            return "(empty)";

        var xMin = points.Min(point => point.X);
        var yMin = points.Min(point => point.Y);
        var xMax = points.Max(point => point.X);
        var yMax = points.Max(point => point.Y);
        return $"({xMin:F2}, {yMin:F2}) - ({xMax:F2}, {yMax:F2})";
    }

    private static GeoKernelLayerStyle SourceStyle() => new()
    {
        FillColor = "#BFD7EA",
        FillOpacity = 80,
        LineColor = "#6C757D",
        LineWidth = 2.0,
        PointColor = "#2F80C2",
        PointSize = 7.0
    };

    private static GeoKernelLayerStyle ResultStyle() => new()
    {
        FillColor = "#F6D6AD",
        FillOpacity = 150,
        LineColor = "#D95D39",
        LineWidth = 4.0,
        PointColor = "#C1121F",
        PointSize = 10.0
    };
}
