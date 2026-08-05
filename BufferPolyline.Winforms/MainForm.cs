using GeoKernel.NET.WinForms;

namespace GeoKernel.BufferPolyline.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] SourceLine =
    [
        new(-4.6, -1.5),
        new(-2.8, 0.4),
        new(-1.0, -0.8),
        new(0.7, 1.2),
        new(2.5, 0.1),
        new(4.4, 1.6)
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

        var bufferCreated = geoKernelViewerControl.AddPolylineBufferShape(
            SourceLine,
            distance,
            12,
            CorridorStyle());

        geoKernelViewerControl.AddPolylineShape(SourceLine, LineStyle());

        detailsTextBox.Text =
            $"MakeBuffer(polyline, distance){Environment.NewLine}" +
            $"Source parts: 1{Environment.NewLine}" +
            $"Source vertices: {SourceLine.Length}{Environment.NewLine}" +
            $"Distance: {distance:F2} map units{Environment.NewLine}" +
            $"Result type: polygon{Environment.NewLine}" +
            $"Source extent: {ExtentText(SourceLine)}{Environment.NewLine}" +
            $"Segments per quadrant: 12";

        statusLabel.Text = bufferCreated
            ? $"Polyline buffer distance: {distance:F2} map units"
            : "Polyline buffer could not be created.";
    }

    private static string ExtentText(IReadOnlyList<GeoKernelPoint> points) =>
        $"({points.Min(point => point.X):F2}, {points.Min(point => point.Y):F2}) - " +
        $"({points.Max(point => point.X):F2}, {points.Max(point => point.Y):F2})";

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.8, -3.2, 5.8, 3.4);
    }

    private static GeoKernelLayerStyle CorridorStyle() => new()
    {
        FillColor = "#F9C74F",
        FillOpacity = 105,
        LineColor = "#D95D39",
        LineWidth = 2.0
    };

    private static GeoKernelLayerStyle LineStyle() => new()
    {
        FillColor = "#FFFFFF",
        FillOpacity = 0,
        LineColor = "#1E5678",
        LineWidth = 3.0,
        PointColor = "#1E5678",
        PointSize = 8.0
    };
}
