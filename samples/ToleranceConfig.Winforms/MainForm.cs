using GeoKernel.NET.WinForms;

namespace GeoKernel.ToleranceConfig.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] Baseline =
    [
        new(-4.5, 0.0),
        new(4.5, 0.0)
    ];

    private static readonly GeoKernelPoint TestPoint = new(0.0, 0.35);

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
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
        var tolerance = toleranceTrackBar.Value / 100.0;
        toleranceValueLabel.Text = $"{tolerance:F2} units";

        var info = geoKernelViewerControl.GetLinePointToleranceInfo(Baseline, TestPoint, tolerance);
        var active = info.Crossings.Count > 0 || info.Intersects;

        geoKernelViewerControl.ClearLayers();
        if (tolerance > 0)
            geoKernelViewerControl.AddPolygonLayer("topology tolerance", ToleranceCircle(TestPoint, tolerance), ToleranceStyle(active));

        geoKernelViewerControl.AddPolylineLayer("baseline", Baseline, LineStyle());
        geoKernelViewerControl.AddPointLayer("test point", [TestPoint], PointStyle(active));

        if (info.Crossings.Count > 0)
            geoKernelViewerControl.AddPointLayer("accepted by tolerance", info.Crossings, CrossingStyle());

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

        statusLabel.Text = $"Topology tolerance: {tolerance:F2} map units.";
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.2, -1.8, 5.2, 2.4);
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
