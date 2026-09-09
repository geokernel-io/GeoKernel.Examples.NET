using GeoKernel.NET.WinForms;

namespace GeoKernel.BufferPolygon.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] SourcePolygon =
    [
        new(-3.6, -1.7),
        new(-1.5, -2.2),
        new(0.1, -1.1),
        new(2.9, -1.5),
        new(3.6, 0.6),
        new(1.1, 2.1),
        new(-0.9, 1.2),
        new(-3.0, 1.8),
        new(-4.0, 0.0),
        new(-3.6, -1.7)
    ];

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        UpdateBuffer();
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void distanceNumeric_ValueChanged(object? sender, EventArgs e)
    {
        if (geoKernelViewerControl.IsHandleCreated)
            UpdateBuffer();
    }

    private void UpdateBuffer()
    {
        var distance = (double)distanceNumeric.Value;
        geoKernelViewerControl.ClearShapes();

        var bufferCreated = geoKernelViewerControl.AddPolygonBufferShape(
            SourcePolygon,
            distance,
            12,
            BufferStyle());

        geoKernelViewerControl.AddPolygonShape(SourcePolygon, PolygonStyle());

        detailsTextBox.Text =
            $"MakeBuffer(polygon, distance){Environment.NewLine}" +
            $"Source parts: 1{Environment.NewLine}" +
            $"Source vertices: {SourcePolygon.Length}{Environment.NewLine}" +
            $"Distance: {distance:F2} map units{Environment.NewLine}" +
            $"Result type: polygon{Environment.NewLine}" +
            $"Source extent: {ExtentText(SourcePolygon)}{Environment.NewLine}" +
            $"Segments per quadrant: 12";

        statusLabel.Text = bufferCreated
            ? $"Polygon buffer distance: {distance:F2} map units"
            : "Polygon buffer could not be created.";
    }

    private static string ExtentText(IReadOnlyList<GeoKernelPoint> points) =>
        $"({points.Min(point => point.X):F2}, {points.Min(point => point.Y):F2}) - " +
        $"({points.Max(point => point.X):F2}, {points.Max(point => point.Y):F2})";

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-6.0, -4.0, 6.0, 4.0);
    }

    private static GeoKernelLayerStyle BufferStyle() => new()
    {
        FillColor = "#86D0A8",
        FillOpacity = 95,
        LineColor = "#2D6A4F",
        LineWidth = 2.0
    };

    private static GeoKernelLayerStyle PolygonStyle() => new()
    {
        FillColor = "#F9C74F",
        FillOpacity = 145,
        LineColor = "#D95D39",
        LineWidth = 2.4
    };
}
