using GeoKernel.NET.WinForms;

namespace GeoKernel.SpatialRelate.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] PolygonA =
    [
        new(-4.0, -1.4),
        new(0.7, -1.4),
        new(0.7, 2.0),
        new(-4.0, 2.0),
        new(-4.0, -1.4)
    ];

    private static readonly GeoKernelPoint[] PolygonB =
    [
        new(-1.0, -2.1),
        new(3.9, -2.1),
        new(3.9, 1.3),
        new(-1.0, 1.3),
        new(-1.0, -2.1)
    ];

    private static readonly (string Name, string Pattern)[] Patterns =
    [
        ("EQUALITY", "T*F**FF*"),
        ("DISJOINT", "FF*FF"),
        ("INTERSECT", "T"),
        ("WITHIN", "T*F**F"),
        ("CONTAINS", "T*****FF*"),
        ("TOUCH", "F***T"),
        ("CROSS", "T*T"),
        ("OVERLAP", "T*T***T")
    ];

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        RenderScene(showRelate: false);
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void runRelateButton_Click(object? sender, EventArgs e)
    {
        RenderScene(showRelate: true);
    }

    private void RenderScene(bool showRelate)
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddPolygonLayer("Polygon A", PolygonA, StyleA());
        geoKernelViewerControl.AddPolygonLayer("Polygon B", PolygonB, StyleB());

        var details =
            $"Relate(left, right){Environment.NewLine}" +
            $"DE-9IM style relation string returned by GisTopology.Relate.{Environment.NewLine}{Environment.NewLine}" +
            $"Polygon A extent: {ExtentText(PolygonA)}{Environment.NewLine}" +
            $"Polygon B extent: {ExtentText(PolygonB)}";

        if (showRelate)
        {
            var matrix = geoKernelViewerControl.RelatePolygonRings(PolygonA, PolygonB);

            details +=
                $"{Environment.NewLine}{Environment.NewLine}" +
                $"Relate matrix: {matrix}{Environment.NewLine}{Environment.NewLine}" +
                "Pattern matches:";

            foreach (var (name, pattern) in Patterns)
            {
                var matched = geoKernelViewerControl.RelatePolygonRings(PolygonA, PolygonB, pattern);
                details += $"{Environment.NewLine}{name} ({pattern}): {matched.ToString().ToLowerInvariant()}";
            }

            statusLabel.Text = $"Relate matrix calculated: {matrix}";
        }
        else
        {
            details += $"{Environment.NewLine}{Environment.NewLine}Click Run Relate to calculate the relation matrix.";
            statusLabel.Text = "Source polygons are ready. Click Run Relate.";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.1, -3.0, 5.0, 3.0);
    }

    private static string ExtentText(IReadOnlyList<GeoKernelPoint> ring)
    {
        var xMin = ring.Min(point => point.X);
        var yMin = ring.Min(point => point.Y);
        var xMax = ring.Max(point => point.X);
        var yMax = ring.Max(point => point.Y);
        return $"({xMin:F2}, {yMin:F2}) - ({xMax:F2}, {yMax:F2})";
    }

    private static GeoKernelLayerStyle StyleA() => new()
    {
        FillColor = "#BFD7EA",
        FillOpacity = 140,
        LineColor = "#2F80C2",
        LineWidth = 2.2
    };

    private static GeoKernelLayerStyle StyleB() => new()
    {
        FillColor = "#F6D6AD",
        FillOpacity = 135,
        LineColor = "#D95D39",
        LineWidth = 2.2
    };
}
