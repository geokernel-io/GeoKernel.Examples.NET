using GeoKernel.NET.WinForms;

namespace GeoKernel.GetCrossings.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] LeftLine =
    [
        new(-6.0, -2.2),
        new(-4.2, 1.6),
        new(-2.0, -0.5),
        new(0.2, 2.1),
        new(2.4, -0.7),
        new(5.8, 2.2)
    ];

    private static readonly GeoKernelPoint[] RightLine =
    [
        new(-6.2, 1.9),
        new(-3.8, -1.6),
        new(-1.4, 1.5),
        new(1.2, -1.9),
        new(3.2, 1.3),
        new(5.8, -1.2)
    ];

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        RenderScene(showResult: false);
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void runButton_Click(object? sender, EventArgs e)
    {
        RenderScene(showResult: true);
        SetSampleExtent();
    }

    private void RenderScene(bool showResult)
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddPolylineLayer("Left polyline", LeftLine, LeftStyle());
        geoKernelViewerControl.AddPolylineLayer("Right polyline", RightLine, RightStyle());

        var details =
            $"GetCrossings(left, right){Environment.NewLine}" +
            $"The two polylines are arranged to cross at multiple segment intersections.{Environment.NewLine}{Environment.NewLine}" +
            $"Left vertices: {LeftLine.Length}{Environment.NewLine}" +
            $"Right vertices: {RightLine.Length}{Environment.NewLine}" +
            $"Left extent: {ExtentText(LeftLine)}{Environment.NewLine}" +
            $"Right extent: {ExtentText(RightLine)}{Environment.NewLine}";

        if (!showResult)
        {
            details += $"{Environment.NewLine}Click Run GetCrossings to calculate intersection points.";
            statusLabel.Text = "Source polylines are ready. Click Run GetCrossings.";
        }
        else
        {
            var crossings = geoKernelViewerControl.GetPolylineCrossings(LeftLine, RightLine);
            if (crossings.Count > 0)
                geoKernelViewerControl.AddPointLayer("Crossing points", crossings, CrossingStyle());

            details += $"{Environment.NewLine}Crossing count: {crossings.Count}{Environment.NewLine}";
            for (var i = 0; i < crossings.Count; ++i)
                details += $"P{i + 1}: ({crossings[i].X:F3}, {crossings[i].Y:F3}){Environment.NewLine}";

            statusLabel.Text = $"GetCrossings found {crossings.Count} point(s).";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-7.0, -3.2, 6.8, 3.2);
    }

    private static string ExtentText(IReadOnlyList<GeoKernelPoint> points)
    {
        var xMin = points.Min(point => point.X);
        var yMin = points.Min(point => point.Y);
        var xMax = points.Max(point => point.X);
        var yMax = points.Max(point => point.Y);
        return $"({xMin:F2}, {yMin:F2}) - ({xMax:F2}, {yMax:F2})";
    }

    private static GeoKernelLayerStyle LeftStyle() => new()
    {
        LineColor = "#2F80C2",
        LineWidth = 3.0,
        PointColor = "#2F80C2",
        PointSize = 7.0
    };

    private static GeoKernelLayerStyle RightStyle() => new()
    {
        LineColor = "#D95D39",
        LineWidth = 3.0,
        PointColor = "#D95D39",
        PointSize = 7.0
    };

    private static GeoKernelLayerStyle CrossingStyle() => new()
    {
        PointColor = "#C1121F",
        PointSize = 12.0,
        LineColor = "#7A0010",
        LineWidth = 1.0
    };
}
