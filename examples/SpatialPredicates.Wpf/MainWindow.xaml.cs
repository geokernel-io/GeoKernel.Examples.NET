using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.SpatialPredicates.Wpf;

public partial class MainWindow
{
    private sealed record PredicateCase(
        string Name,
        string Pattern,
        IReadOnlyList<GeoKernelPoint> Left,
        IReadOnlyList<GeoKernelPoint> Right,
        bool IsPolyline);

    private static readonly PredicateCase[] PredicateCases =
    [
        new("Contains", "T*****FF*", Rectangle(-8.2, 3.5, -4.8, 6.3), Rectangle(-7.4, 4.1, -5.7, 5.5), false),
        new("Within", "T*F**F", Rectangle(-2.8, 4.1, -1.1, 5.5), Rectangle(-3.6, 3.5, -0.2, 6.3), false),
        new("Touches", "F***T", Rectangle(1.2, 3.6, 3.4, 6.1), Rectangle(3.4, 3.6, 5.6, 6.1), false),
        new("Overlaps", "T*T***T", Rectangle(-8.2, -2.0, -5.0, 0.8), Rectangle(-6.3, -0.8, -3.1, 2.0), false),
        new("Cross", "T*T", [new(-2.9, -1.7), new(0.4, 1.6)], [new(-2.9, 1.6), new(0.4, -1.7)], true),
        new("Disjoint", "FF*FF", Rectangle(1.4, -2.0, 3.0, -0.2), Rectangle(4.2, 0.2, 5.8, 2.0), false),
        new("Intersects", "T", Rectangle(-0.3, -2.1, 1.3, -0.2), Rectangle(0.7, -1.5, 2.4, 0.7), false)
    ];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        RenderScene();
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void RenderScene()
    {
        viewerControl.ClearLayers();

        var details = $"Spatial predicate examples{Environment.NewLine}" +
                      $"Each pair is arranged so the named predicate evaluates to true.{Environment.NewLine}";

        var index = 1;
        foreach (var predicateCase in PredicateCases)
        {
            var leftName = $"{index}. {predicateCase.Name} A";
            var rightName = $"{index}. {predicateCase.Name} B";

            if (predicateCase.IsPolyline)
            {
                viewerControl.AddPolylineLayer(leftName, predicateCase.Left, LineAStyle());
                viewerControl.AddPolylineLayer(rightName, predicateCase.Right, LineBStyle());
            }
            else
            {
                viewerControl.AddPolygonLayer(leftName, predicateCase.Left, StyleA());
                viewerControl.AddPolygonLayer(rightName, predicateCase.Right, StyleB());
            }

            var matrix = predicateCase.IsPolyline
                ? viewerControl.RelatePolylines(predicateCase.Left, predicateCase.Right)
                : viewerControl.RelatePolygonRings(predicateCase.Left, predicateCase.Right);

            var matched = predicateCase.IsPolyline
                ? viewerControl.RelatePolylines(predicateCase.Left, predicateCase.Right, predicateCase.Pattern)
                : viewerControl.RelatePolygonRings(predicateCase.Left, predicateCase.Right, predicateCase.Pattern);

            details +=
                $"{Environment.NewLine}{predicateCase.Name} ({predicateCase.Pattern}){Environment.NewLine}" +
                $"  result: {matched.ToString().ToLowerInvariant()}{Environment.NewLine}" +
                $"  matrix: {matrix}{Environment.NewLine}" +
                $"  left extent: {ExtentText(predicateCase.Left)}{Environment.NewLine}" +
                $"  right extent: {ExtentText(predicateCase.Right)}{Environment.NewLine}";

            ++index;
        }

        detailsTextBox.Text = details;
        statusText.Text = "Spatial predicates evaluated.";
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-9.2, -3.2, 6.8, 7.2);
    }

    private static GeoKernelPoint[] Rectangle(double xMin, double yMin, double xMax, double yMax) =>
    [
        new(xMin, yMin),
        new(xMax, yMin),
        new(xMax, yMax),
        new(xMin, yMax),
        new(xMin, yMin)
    ];

    private static string ExtentText(IReadOnlyList<GeoKernelPoint> points)
    {
        var xMin = points.Min(point => point.X);
        var yMin = points.Min(point => point.Y);
        var xMax = points.Max(point => point.X);
        var yMax = points.Max(point => point.Y);
        return $"({xMin:F2}, {yMin:F2}) - ({xMax:F2}, {yMax:F2})";
    }

    private static GeoKernelLayerStyle StyleA() => new()
    {
        FillColor = "#BFD7EA",
        FillOpacity = 135,
        LineColor = "#2F80C2",
        LineWidth = 2.0
    };

    private static GeoKernelLayerStyle StyleB() => new()
    {
        FillColor = "#F6D6AD",
        FillOpacity = 130,
        LineColor = "#D95D39",
        LineWidth = 2.0
    };

    private static GeoKernelLayerStyle LineAStyle() => new()
    {
        LineColor = "#2F80C2",
        LineWidth = 3.0
    };

    private static GeoKernelLayerStyle LineBStyle() => new()
    {
        LineColor = "#D95D39",
        LineWidth = 3.0
    };
}
