using System.Windows;
using System.Windows.Threading;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.BufferAnimated.Wpf;

public partial class MainWindow
{
    private const double MinDistance = 0.35;
    private const double MaxDistance = 3.00;
    private const double DistanceStep = 0.08;
    private static readonly GeoKernelPoint SourcePoint = new(0.0, 0.0);

    private readonly DispatcherTimer _timer = new();
    private double _distance = MinDistance;
    private int _direction = 1;
    private int _frame;

    public MainWindow()
    {
        InitializeComponent();
        _timer.Interval = TimeSpan.FromMilliseconds(60);
        _timer.Tick += Timer_Tick;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        RenderFrame();
        SetSampleExtent();
        _timer.Start();
    }

    private void Timer_Tick(object? sender, EventArgs e)
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

    private void PlayPause_Click(object sender, RoutedEventArgs e)
    {
        if (_timer.IsEnabled)
        {
            _timer.Stop();
            playPauseButton.Content = "Play";
        }
        else
        {
            _timer.Start();
            playPauseButton.Content = "Pause";
        }
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void IntervalSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _timer.Interval = TimeSpan.FromMilliseconds(e.NewValue);
    }

    private void RenderFrame()
    {
        viewerControl.ClearLayers();

        var bufferLayerIndex = viewerControl.AddPointBufferLayer(
            "Animated Buffer",
            SourcePoint,
            _distance,
            18,
            BufferStyle(_distance));

        viewerControl.AddPointBufferLayer(
            "Pulse Ring",
            SourcePoint,
            Math.Max(MinDistance, _distance - 0.28),
            18,
            PulseRingStyle());

        viewerControl.AddPointLayer(
            "Source Point",
            [SourcePoint],
            PointStyle());

        distanceText.Text = $"{_distance:F2} units";
        detailsTextBox.Text =
            $"Timer animated buffer{Environment.NewLine}" +
            $"Operation: MakeBuffer(point, distance){Environment.NewLine}" +
            $"Frame: {_frame}{Environment.NewLine}" +
            $"Distance: {_distance:F2} map units{Environment.NewLine}" +
            $"Source point: ({SourcePoint.X:F2}, {SourcePoint.Y:F2}){Environment.NewLine}" +
            $"Result layer index: {bufferLayerIndex}{Environment.NewLine}" +
            $"Segments per quadrant: 18";

        statusText.Text = $"Animated point buffer: frame {_frame}, distance {_distance:F2}";
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-4.2, -3.5, 4.2, 3.5);
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
