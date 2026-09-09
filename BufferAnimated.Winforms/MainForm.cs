using GeoKernel.NET.WinForms;

namespace GeoKernel.BufferAnimated.Winforms;

public sealed partial class MainForm : Form
{
    private const double MinDistance = 0.35;
    private const double MaxDistance = 3.00;
    private const double DistanceStep = 0.08;
    private static readonly GeoKernelPoint SourcePoint = new(0.0, 0.0);

    private double _distance = MinDistance;
    private int _direction = 1;
    private int _frame;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        RenderFrame();
        SetSampleExtent();
        animationTimer.Start();
    }

    private void animationTimer_Tick(object? sender, EventArgs e)
    {
        _distance += DistanceStep * _direction;
        if (_distance >= MaxDistance)
        {
            _distance = MaxDistance;
            _direction = -1;
        }
        else if (_distance <= MinDistance)
        {
            _distance = MinDistance;
            _direction = 1;
        }

        ++_frame;
        RenderFrame();
    }

    private void playPauseButton_Click(object? sender, EventArgs e)
    {
        if (animationTimer.Enabled)
        {
            animationTimer.Stop();
            playPauseButton.Text = "Play";
        }
        else
        {
            animationTimer.Start();
            playPauseButton.Text = "Pause";
        }
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void intervalTrackBar_ValueChanged(object? sender, EventArgs e)
    {
        animationTimer.Interval = intervalTrackBar.Value;
    }

    private void RenderFrame()
    {
        geoKernelViewerControl.ClearShapes();

        var bufferCreated = geoKernelViewerControl.AddPointBufferShape(
            SourcePoint,
            _distance,
            18,
            BufferStyle(_distance));

        geoKernelViewerControl.AddPointBufferShape(
            SourcePoint,
            Math.Max(MinDistance, _distance - 0.28),
            18,
            PulseRingStyle());

        geoKernelViewerControl.AddPointShape(SourcePoint, PointStyle());

        distanceValueLabel.Text = $"{_distance:F2} units";
        detailsTextBox.Text =
            $"Timer animated buffer{Environment.NewLine}" +
            $"Operation: MakeBuffer(point, distance){Environment.NewLine}" +
            $"Frame: {_frame}{Environment.NewLine}" +
            $"Distance: {_distance:F2} map units{Environment.NewLine}" +
            $"Source point: ({SourcePoint.X:F2}, {SourcePoint.Y:F2}){Environment.NewLine}" +
            $"Buffer created: {bufferCreated}{Environment.NewLine}" +
            $"Segments per quadrant: 18";

        statusLabel.Text = $"Animated point buffer: frame {_frame}, distance {_distance:F2}";
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-4.2, -3.5, 4.2, 3.5);
    }

    private static GeoKernelLayerStyle BufferStyle(double distance)
    {
        var t = (distance - MinDistance) / (MaxDistance - MinDistance);
        return new GeoKernelLayerStyle
        {
            FillColor = "#78B7D0",
            FillOpacity = 55 + (int)(t * 90.0),
            LineColor = "#1E6F8C",
            LineWidth = 2.2
        };
    }

    private static GeoKernelLayerStyle PulseRingStyle() => new()
    {
        FillColor = "#FFFFFF",
        FillOpacity = 0,
        LineColor = "#D95D39",
        LineWidth = 1.3
    };

    private static GeoKernelLayerStyle PointStyle() => new()
    {
        FillColor = "#D95D39",
        FillOpacity = 255,
        LineColor = "#7A2F1E",
        LineWidth = 1.2,
        PointColor = "#D95D39",
        PointSize = 13.0
    };
}
