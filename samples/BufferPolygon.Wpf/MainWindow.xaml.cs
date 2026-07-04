using System.Globalization;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.BufferPolygon.Wpf;

public partial class MainWindow
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

        var bufferLayerIndex = viewerControl.AddPolygonBufferLayer(
            "Polygon Buffer",
            SourcePolygon,
            distance,
            12,
            BufferStyle());

        viewerControl.AddPolygonLayer(
            "Source Polygon",
            SourcePolygon,
            PolygonStyle());

        detailsTextBox.Text =
            $"MakeBuffer(polygon, distance){Environment.NewLine}" +
            $"Source parts: 1{Environment.NewLine}" +
            $"Source vertices: {SourcePolygon.Length}{Environment.NewLine}" +
            $"Distance: {distance:F2} map units{Environment.NewLine}" +
            $"Result layer index: {bufferLayerIndex}{Environment.NewLine}" +
            $"Result type: polygon{Environment.NewLine}" +
            $"Segments per quadrant: 12";

        statusText.Text = bufferLayerIndex >= 0
            ? $"Polygon buffer distance: {distance:F2} map units"
            : "Polygon buffer could not be created.";
    }

    private double ParseDistance()
    {
        if (double.TryParse(distanceTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) &&
            value > 0.0)
        {
            return Math.Clamp(value, 0.10, 2.0);
        }

        return 0.60;
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-6.0, -4.0, 6.0, 4.0);
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
