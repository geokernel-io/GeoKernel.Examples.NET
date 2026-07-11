using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ConvexHullTwo.Wpf;

public partial class MainWindow
{
    private static readonly GeoKernelPoint[] LeftPolygon =
    [
        new(-4.5, -1.6),
        new(-3.1, 1.8),
        new(-1.9, -0.5),
        new(-0.5, 1.4),
        new(-0.1, -1.8),
        new(-2.1, -0.9),
        new(-3.5, -2.0),
        new(-4.5, -1.6)
    ];

    private static readonly GeoKernelPoint[] RightPolygon =
    [
        new(0.9, -1.4),
        new(2.3, -2.0),
        new(4.2, -0.2),
        new(3.4, 2.3),
        new(1.6, 1.3),
        new(0.4, 2.7),
        new(0.9, -1.4)
    ];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        RenderScene(showHull: false);
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void RunHull_Click(object sender, RoutedEventArgs e)
    {
        RenderScene(showHull: true);
    }

    private void RenderScene(bool showHull)
    {
        viewerControl.ClearLayers();
        viewerControl.AddPolygonLayer("Left Polygon", LeftPolygon, LeftStyle());
        viewerControl.AddPolygonLayer("Right Polygon", RightPolygon, RightStyle());

        var details =
            $"ConvexHull(left, right){Environment.NewLine}" +
            $"Source geometry count: 2{Environment.NewLine}" +
            $"Left vertices: {LeftPolygon.Length}{Environment.NewLine}" +
            $"Left extent: (-4.50, -2.00) - (-0.10, 1.80){Environment.NewLine}" +
            $"Right vertices: {RightPolygon.Length}{Environment.NewLine}" +
            $"Right extent: (0.40, -2.00) - (4.20, 2.70)";

        if (showHull)
        {
            var resultLayerIndex = viewerControl.AddPolygonConvexHullTwoLayer(
                "Convex Hull",
                LeftPolygon,
                RightPolygon,
                HullStyle());

            details +=
                $"{Environment.NewLine}Hull layer index: {resultLayerIndex}{Environment.NewLine}" +
                "Hull type: polygon";

            statusText.Text = resultLayerIndex >= 0
                ? "Convex hull result created from two polygons."
                : "Convex hull returned an empty result.";
        }
        else
        {
            details += $"{Environment.NewLine}Result: click Run Convex Hull to calculate";
            statusText.Text = "Two source geometries are ready. Click Run Convex Hull.";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-5.4, -3.1, 5.3, 3.5);
    }

    private static GeoKernelLayerStyle LeftStyle() => new()
    {
        FillColor = "#BFD7EA",
        FillOpacity = 110,
        LineColor = "#2F80C2",
        LineWidth = 2.2
    };

    private static GeoKernelLayerStyle RightStyle() => new()
    {
        FillColor = "#CDE7D8",
        FillOpacity = 110,
        LineColor = "#2D6A4F",
        LineWidth = 2.2
    };

    private static GeoKernelLayerStyle HullStyle() => new()
    {
        FillColor = "#F9C74F",
        FillOpacity = 105,
        LineColor = "#D95D39",
        LineWidth = 3.0
    };
}
