using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GetCrossings.Wpf;

public partial class MainWindow
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

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        RenderScene(showResult: false);
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void Run_Click(object sender, RoutedEventArgs e)
    {
        RenderScene(showResult: true);
        SetSampleExtent();
    }

    private void RenderScene(bool showResult)
    {
        viewerControl.ClearLayers();
        viewerControl.AddPolylineLayer("Left polyline", LeftLine, LeftStyle());
        viewerControl.AddPolylineLayer("Right polyline", RightLine, RightStyle());

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
            statusText.Text = "Source polylines are ready. Click Run GetCrossings.";
        }
        else
        {
            var crossings = viewerControl.GetPolylineCrossings(LeftLine, RightLine);
            if (crossings.Count > 0)
                viewerControl.AddPointLayer("Crossing points", crossings, CrossingStyle());

            details += $"{Environment.NewLine}Crossing count: {crossings.Count}{Environment.NewLine}";
            for (var i = 0; i < crossings.Count; ++i)
                details += $"P{i + 1}: ({crossings[i].X:F3}, {crossings[i].Y:F3}){Environment.NewLine}";

            statusText.Text = $"GetCrossings found {crossings.Count} point(s).";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-7.0, -3.2, 6.8, 3.2);
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
