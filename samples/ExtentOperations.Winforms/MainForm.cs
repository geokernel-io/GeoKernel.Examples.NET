using GeoKernel.NET.WinForms;

namespace GeoKernel.ExtentOperations.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelExtent BaseExtent = new(-4.4, -1.8, 0.8, 1.8);
    private static readonly GeoKernelExtent OtherExtent = new(-0.8, -0.6, 4.2, 2.6);
    private static readonly GeoKernelPoint InsidePoint = new(-2.0, 0.4);
    private static readonly GeoKernelPoint OutsidePoint = new(2.8, -1.2);

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

    private void RenderScene()
    {
        var expanded = Expand(BaseExtent, OtherExtent);
        var inflated = Inflate(BaseExtent, dx: 0.9, dy: 0.7);

        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddPolygonLayer("A.expand(B)", ExtentRing(expanded), ExpandedStyle());
        geoKernelViewerControl.AddPolygonLayer("A.inflate(0.9, 0.7)", ExtentRing(inflated), InflatedStyle());
        geoKernelViewerControl.AddPolygonLayer("A", ExtentRing(BaseExtent), BaseStyle());
        geoKernelViewerControl.AddPolygonLayer("B", ExtentRing(OtherExtent), OtherStyle());
        geoKernelViewerControl.AddPointLayer("A contains inside point", [InsidePoint], InsidePointStyle());
        geoKernelViewerControl.AddPointLayer("A contains outside point", [OutsidePoint], OutsidePointStyle());

        detailsTextBox.Text =
            $"GisExtent operations{Environment.NewLine}{Environment.NewLine}" +
            $"A: {ExtentText(BaseExtent)}{Environment.NewLine}" +
            $"B: {ExtentText(OtherExtent)}{Environment.NewLine}{Environment.NewLine}" +
            $"A.expand(B): {ExtentText(expanded)}{Environment.NewLine}" +
            $"A.inflate(0.9, 0.7): {ExtentText(inflated)}{Environment.NewLine}{Environment.NewLine}" +
            $"A.intersects(B): {Intersects(BaseExtent, OtherExtent).ToString().ToLowerInvariant()}{Environment.NewLine}" +
            $"A.contains(inside point): {Contains(BaseExtent, InsidePoint).ToString().ToLowerInvariant()}{Environment.NewLine}" +
            $"A.contains(outside point): {Contains(BaseExtent, OutsidePoint).ToString().ToLowerInvariant()}{Environment.NewLine}{Environment.NewLine}" +
            $"Visual guide:{Environment.NewLine}" +
            $"Blue: base extent A{Environment.NewLine}" +
            $"Orange: extent B{Environment.NewLine}" +
            $"Green: A expanded to include B{Environment.NewLine}" +
            $"Purple: A inflated by dx/dy";

        statusLabel.Text = "Extent operations rendered.";
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.8, -3.0, 5.4, 3.6);
    }

    private static GeoKernelPoint[] ExtentRing(GeoKernelExtent extent) =>
    [
        new(extent.XMin, extent.YMin),
        new(extent.XMax, extent.YMin),
        new(extent.XMax, extent.YMax),
        new(extent.XMin, extent.YMax),
        new(extent.XMin, extent.YMin)
    ];

    private static GeoKernelExtent Expand(GeoKernelExtent left, GeoKernelExtent right) =>
        new(
            Math.Min(left.XMin, right.XMin),
            Math.Min(left.YMin, right.YMin),
            Math.Max(left.XMax, right.XMax),
            Math.Max(left.YMax, right.YMax));

    private static GeoKernelExtent Inflate(GeoKernelExtent extent, double dx, double dy) =>
        new(extent.XMin - dx, extent.YMin - dy, extent.XMax + dx, extent.YMax + dy);

    private static bool Intersects(GeoKernelExtent left, GeoKernelExtent right) =>
        left.XMin <= right.XMax &&
        left.XMax >= right.XMin &&
        left.YMin <= right.YMax &&
        left.YMax >= right.YMin;

    private static bool Contains(GeoKernelExtent extent, GeoKernelPoint point) =>
        point.X >= extent.XMin &&
        point.X <= extent.XMax &&
        point.Y >= extent.YMin &&
        point.Y <= extent.YMax;

    private static string ExtentText(GeoKernelExtent extent) =>
        $"({extent.XMin:F2}, {extent.YMin:F2}) - ({extent.XMax:F2}, {extent.YMax:F2}), w={extent.XMax - extent.XMin:F2}, h={extent.YMax - extent.YMin:F2}";

    private static GeoKernelLayerStyle BaseStyle() => ExtentStyle("#BFD7EA", "#2F80C2", 90, 2.2);
    private static GeoKernelLayerStyle OtherStyle() => ExtentStyle("#F6D6AD", "#D95D39", 90, 2.2);
    private static GeoKernelLayerStyle ExpandedStyle() => ExtentStyle("#CDE7D8", "#2A9D8F", 55, 3.0);
    private static GeoKernelLayerStyle InflatedStyle() => ExtentStyle("#E6D5F7", "#7B2CBF", 35, 3.0);

    private static GeoKernelLayerStyle ExtentStyle(string fill, string line, int fillOpacity, double lineWidth) => new()
    {
        FillColor = fill,
        FillOpacity = fillOpacity,
        LineColor = line,
        LineWidth = lineWidth
    };

    private static GeoKernelLayerStyle InsidePointStyle() => new()
    {
        PointColor = "#2A9D8F",
        PointSize = 11.0,
        LineColor = "#145A4B",
        LineWidth = 1.0
    };

    private static GeoKernelLayerStyle OutsidePointStyle() => new()
    {
        PointColor = "#C1121F",
        PointSize = 11.0,
        LineColor = "#7A0010",
        LineWidth = 1.0
    };
}
