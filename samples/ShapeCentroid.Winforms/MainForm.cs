using GeoKernel.NET.WinForms;

namespace GeoKernel.ShapeCentroid.Winforms;

public sealed partial class MainForm : Form
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

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        RenderScene();
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void RenderScene()
    {
        var info = geoKernelViewerControl.GetPolygonCentroidInfo(PolygonRing);

        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddPolygonLayer("concave polygon", PolygonRing, PolygonStyle());
        geoKernelViewerControl.AddPointLayer("centroid()", [info.Centroid], CentroidStyle());
        geoKernelViewerControl.AddPointLayer("labelPoint()", [info.LabelPoint], LabelPointStyle());

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

        statusLabel.Text = "Centroid and label point rendered.";
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.4, -3.0, 4.8, 3.0);
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
