using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ToleranceConfig.Wpf;

public partial class MainWindow
{
    private static readonly GeoKernelPoint[] Baseline =
    [
        new(-4.5, 0.0),
        new(4.5, 0.0)
    ];

    private static readonly GeoKernelPoint TestPoint = new(0.0, 0.35);

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        RenderScene();
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ToleranceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded)
            return;

        RenderScene();
    }

    private void RenderScene()
    {
        var tolerance = toleranceSlider.Value / 100.0;
        toleranceValueText.Text = $"{tolerance:F2} units";

        var info = viewerControl.GetLinePointToleranceInfo(Baseline, TestPoint, tolerance);
        var active = info.Crossings.Count > 0 || info.Intersects;

        viewerControl.ClearLayers();
        if (tolerance > 0)
            viewerControl.AddPolygonLayer("topology tolerance", ToleranceCircle(TestPoint, tolerance), ToleranceStyle(active));

        viewerControl.AddPolylineLayer("baseline", Baseline, LineStyle());
        viewerControl.AddPointLayer("test point", [TestPoint], PointStyle(active));

        if (info.Crossings.Count > 0)
            viewerControl.AddPointLayer("accepted by tolerance", info.Crossings, CrossingStyle());

        detailsTextBox.Text =
            $"GisTopology::SetTolerance{Environment.NewLine}{Environment.NewLine}" +
            $"Scenario:{Environment.NewLine}" +
            $"- Baseline is y = 0.{Environment.NewLine}" +
            $"- Test point is at (0.00, 0.35).{Environment.NewLine}" +
            $"- Point-to-line distance is 0.35 map units.{Environment.NewLine}{Environment.NewLine}" +
            $"Configured tolerance: {info.Tolerance:F2}{Environment.NewLine}" +
            $"GetCrossings(line, point): {info.Crossings.Count} point(s){Environment.NewLine}" +
            $"Intersect(line, point): {info.Intersects.ToString().ToLowerInvariant()}{Environment.NewLine}{Environment.NewLine}" +
            $"Result:{Environment.NewLine}" +
            (active
                ? "The point is accepted as touching/intersecting the line within tolerance."
                : "The point is outside the configured tolerance.") +
            $"{Environment.NewLine}{Environment.NewLine}" +
            $"Visual guide:{Environment.NewLine}" +
            $"Circle: current tolerance radius around the point{Environment.NewLine}" +
            $"Green: tolerance reaches the line{Environment.NewLine}" +
            $"Orange/red: tolerance is too small";

        statusText.Text = $"Topology tolerance: {tolerance:F2} map units.";
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-5.2, -1.8, 5.2, 2.4);
    }

    private static GeoKernelPoint[] ToleranceCircle(GeoKernelPoint center, double radius)
    {
        const int segments = 72;
        var ring = new GeoKernelPoint[segments + 1];
        for (var i = 0; i <= segments; ++i)
        {
            var angle = 2.0 * Math.PI * i / segments;
            ring[i] = new GeoKernelPoint(
                center.X + Math.Cos(angle) * radius,
                center.Y + Math.Sin(angle) * radius);
        }

        return ring;
    }

    private static GeoKernelLayerStyle LineStyle() => new()
    {
        LineColor = "#1F6F8B",
        LineWidth = 3.0,
        PointColor = "#1F6F8B",
        PointSize = 7.0
    };

    private static GeoKernelLayerStyle ToleranceStyle(bool active) => new()
    {
        FillColor = active ? "#CDE7D8" : "#F6D6AD",
        FillOpacity = 75,
        LineColor = active ? "#2A9D8F" : "#D95D39",
        LineWidth = 2.0
    };

    private static GeoKernelLayerStyle PointStyle(bool active) => new()
    {
        PointColor = active ? "#2A9D8F" : "#C1121F",
        LineColor = active ? "#145A4B" : "#7A0010",
        LineWidth = 1.3,
        PointSize = 12.0
    };

    private static GeoKernelLayerStyle CrossingStyle() => new()
    {
        PointColor = "#FFD166",
        LineColor = "#9A6700",
        LineWidth = 1.5,
        PointSize = 15.0
    };
}
