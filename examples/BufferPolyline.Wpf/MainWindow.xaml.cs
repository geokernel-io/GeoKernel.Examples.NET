using System.Globalization;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.BufferPolyline.Wpf;

public partial class MainWindow
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

    private bool _loaded;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _loaded = true;        
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

        var bufferLayerIndex = viewerControl.AddPolylineBufferLayer(
            "Polyline Buffer",
            SourceLine,
            distance,
            12,
            CorridorStyle());

        viewerControl.AddPolylineLayer(
            "Source Polyline",
            SourceLine,
            LineStyle());

        detailsTextBox.Text =
            $"MakeBuffer(polyline, distance){Environment.NewLine}" +
            $"Source parts: 1{Environment.NewLine}" +
            $"Source vertices: {SourceLine.Length}{Environment.NewLine}" +
            $"Distance: {distance:F2} map units{Environment.NewLine}" +
            $"Result layer index: {bufferLayerIndex}{Environment.NewLine}" +
            $"Result type: polygon{Environment.NewLine}" +
            $"Segments per quadrant: 12";

        statusText.Text = bufferLayerIndex >= 0
            ? $"Polyline buffer distance: {distance:F2} map units"
            : "Polyline buffer could not be created.";
    }

    private double ParseDistance()
    {
        if (double.TryParse(distanceTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) &&
            value > 0.0)
        {
            return Math.Clamp(value, 0.10, 2.0);
        }

        return 0.55;
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-5.8, -3.2, 5.8, 3.4);
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
