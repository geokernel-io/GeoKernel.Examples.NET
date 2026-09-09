using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ShapeCentroid.Wpf;

public partial class MainWindow
{
    private static readonly GeoKernelPoint[] PolygonRing =
    [
        new(-4.4, -2.0),
        new(3.8, -2.0),
        new(3.8, 2.0),
        new(1.0, 2.0),
        new(1.0, -0.4),
        new(-1.1, -0.4),
        new(-1.1, 2.0),
        new(-4.4, 2.0),
        new(-4.4, -2.0)
    ];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        RenderScene();
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void RenderScene()
    {
        var info = viewerControl.GetPolygonCentroidInfo(PolygonRing);

        viewerControl.ClearLayers();
        viewerControl.AddPolygonLayer("concave polygon", PolygonRing, PolygonStyle());
        viewerControl.AddPointLayer("centroid()", [info.Centroid], CentroidStyle());
        viewerControl.AddPointLayer("labelPoint()", [info.LabelPoint], LabelPointStyle());

        detailsTextBox.Text =
            $"GisShapePolygon::centroid() / labelPoint(){Environment.NewLine}{Environment.NewLine}" +
            $"Centroid: {PointText(info.Centroid)}{Environment.NewLine}" +
            $"Centroid inside polygon: {info.CentroidInside.ToString().ToLowerInvariant()}{Environment.NewLine}{Environment.NewLine}" +
            $"Label point: {PointText(info.LabelPoint)}{Environment.NewLine}" +
            $"Label point inside polygon: {info.LabelPointInside.ToString().ToLowerInvariant()}{Environment.NewLine}{Environment.NewLine}" +
            $"Visual guide:{Environment.NewLine}" +
            $"Blue polygon: source concave polygon{Environment.NewLine}" +
            $"Orange point: centroid(){Environment.NewLine}" +
            $"Green point: labelPoint(){Environment.NewLine}{Environment.NewLine}" +
            $"For concave polygons the mathematical centroid can fall outside the visible area. labelPoint() is selected as an interior point suitable for labels.";

        statusText.Text = "Centroid and label point rendered.";
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-5.4, -3.0, 4.8, 3.0);
    }

    private static string PointText(GeoKernelPoint point) => $"({point.X:F3}, {point.Y:F3})";

    private static GeoKernelLayerStyle PolygonStyle() => new()
    {
        FillColor = "#BFD7EA",
        FillOpacity = 110,
        LineColor = "#1F6F8B",
        LineWidth = 2.2
    };

    private static GeoKernelLayerStyle CentroidStyle() => new()
    {
        PointColor = "#D95D39",
        PointSize = 12.0,
        LineColor = "#8F2D1B",
        LineWidth = 1.4
    };

    private static GeoKernelLayerStyle LabelPointStyle() => new()
    {
        PointColor = "#2A9D8F",
        PointSize = 12.0,
        LineColor = "#145A4B",
        LineWidth = 1.4
    };
}
