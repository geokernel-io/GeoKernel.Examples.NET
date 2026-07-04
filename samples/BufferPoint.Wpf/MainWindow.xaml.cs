using System.Globalization;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.BufferPoint.Wpf;

public partial class MainWindow
{
    private static readonly GeoKernelPoint SourcePoint = new(0.0, 0.0);
    private bool _loaded;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _loaded = true;
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        UpdateBuffer();
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void DistanceTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_loaded)
            UpdateBuffer();
    }

    private void UpdateBuffer()
    {
        var distance = ParseDistance();
        viewerControl.ClearLayers();

        var bufferLayerIndex = viewerControl.AddPointBufferLayer(
            "Point Buffer",
            SourcePoint,
            distance,
            16,
            BufferStyle());

        viewerControl.AddPointLayer(
            "Source Point",
            [SourcePoint],
            PointStyle());

        detailsTextBox.Text =
            $"MakeBuffer(point, distance){Environment.NewLine}" +
            $"Source point: ({SourcePoint.X:F2}, {SourcePoint.Y:F2}){Environment.NewLine}" +
            $"Distance: {distance:F2} map units{Environment.NewLine}" +
            $"Result layer index: {bufferLayerIndex}{Environment.NewLine}" +
            $"Result type: polygon{Environment.NewLine}" +
            $"Segments per quadrant: 16";

        statusText.Text = bufferLayerIndex >= 0
            ? $"Point buffer distance: {distance:F2} map units"
            : "Point buffer could not be created.";
    }

    private double ParseDistance()
    {
        if (double.TryParse(distanceTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) &&
            value > 0.0)
        {
            return Math.Clamp(value, 0.25, 5.0);
        }

        return 2.0;
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-5.0, -4.0, 5.0, 4.0);
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
