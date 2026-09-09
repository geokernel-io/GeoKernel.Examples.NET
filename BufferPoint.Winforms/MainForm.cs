using GeoKernel.NET.WinForms;

namespace GeoKernel.BufferPoint.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint SourcePoint = new(0.0, 0.0);

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

        var bufferCreated = geoKernelViewerControl.AddPointBufferShape(
            SourcePoint,
            distance,
            16,
            BufferStyle());

        geoKernelViewerControl.AddPointShape(SourcePoint, PointStyle());

        detailsTextBox.Text =
            $"MakeBuffer(point, distance){Environment.NewLine}" +
            $"Source point: ({SourcePoint.X:F2}, {SourcePoint.Y:F2}){Environment.NewLine}" +
            $"Distance: {distance:F2} map units{Environment.NewLine}" +
            $"Result type: polygon{Environment.NewLine}" +
            $"Result parts: 1{Environment.NewLine}" +
            $"Result extent: ({-distance:F2}, {-distance:F2}) - ({distance:F2}, {distance:F2}){Environment.NewLine}" +
            $"Segments per quadrant: 16";

        statusLabel.Text = bufferCreated
            ? $"Point buffer distance: {distance:F2} map units"
            : "Point buffer could not be created.";
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.0, -4.0, 5.0, 4.0);
    }

    private static GeoKernelLayerStyle BufferStyle() => new()
    {
        FillColor = "#78B7D0",
        FillOpacity = 85,
        LineColor = "#1E6F8C",
        LineWidth = 2.0
    };

    private static GeoKernelLayerStyle PointStyle() => new()
    {
        FillColor = "#D95D39",
        FillOpacity = 255,
        LineColor = "#7A2F1E",
        LineWidth = 1.3,
        PointColor = "#D95D39",
        PointSize = 13.0
    };
}
