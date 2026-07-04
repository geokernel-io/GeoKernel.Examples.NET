using GeoKernel.NET.WinForms;

namespace GeoKernel.ConvexHullShape.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] SourcePolygon =
    [
        new(-4.4, -1.6),
        new(-3.4, 1.6),
        new(-1.9, -0.7),
        new(-0.4, 2.3),
        new(0.8, -1.2),
        new(2.0, 1.7),
        new(3.9, -0.5),
        new(2.5, -2.1),
        new(0.4, -0.2),
        new(-1.2, -2.0),
        new(-2.7, 0.0),
        new(-4.4, -1.6)
    ];

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        RenderScene(showHull: false);
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void runHullButton_Click(object? sender, EventArgs e)
    {
        RenderScene(showHull: true);
    }

    private void RenderScene(bool showHull)
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddPolygonLayer("Source Polygon", SourcePolygon, SourceStyle());

        var details =
            $"ConvexHull(shape){Environment.NewLine}" +
            $"Source type: polygon{Environment.NewLine}" +
            $"Source geometry count: 1{Environment.NewLine}" +
            $"Source vertices: {SourcePolygon.Length}{Environment.NewLine}" +
            $"Source extent: (-4.40, -2.10) - (3.90, 2.30)";

        if (showHull)
        {
            var resultLayerIndex = geoKernelViewerControl.AddPolygonConvexHullLayer(
                "Convex Hull",
                SourcePolygon,
                HullStyle());

            details +=
                $"{Environment.NewLine}Hull layer index: {resultLayerIndex}{Environment.NewLine}" +
                "Hull type: polygon";

            statusLabel.Text = resultLayerIndex >= 0
                ? "Convex hull result created."
                : "Convex hull returned an empty result.";
        }
        else
        {
            details += $"{Environment.NewLine}Result: click Run Convex Hull to calculate";
            statusLabel.Text = "Source geometry is ready. Click Run Convex Hull.";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.3, -3.1, 5.2, 3.4);
    }

    private static GeoKernelLayerStyle SourceStyle() => new()
    {
        FillColor = "#BFD7EA",
        FillOpacity = 100,
        LineColor = "#1F6F9F",
        LineWidth = 2.4
    };

    private static GeoKernelLayerStyle HullStyle() => new()
    {
        FillColor = "#F9C74F",
        FillOpacity = 115,
        LineColor = "#D95D39",
        LineWidth = 3.0
    };
}
