using GeoKernel.NET.WinForms;

namespace GeoKernel.SymDifference.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] PolygonA =
    [
        new(-4.4, -1.8),
        new(1.2, -1.8),
        new(1.2, 2.2),
        new(-4.4, 2.2),
        new(-4.4, -1.8)
    ];

    private static readonly GeoKernelPoint[] PolygonB =
    [
        new(-0.2, 3.0),
        new(0.6, 1.2),
        new(3.2, 1.2),
        new(1.1, -0.1),
        new(2.0, -2.0),
        new(-0.2, -0.8),
        new(-2.4, -2.0),
        new(-1.5, -0.1),
        new(-3.6, 1.2),
        new(-1.0, 1.2),
        new(-0.2, 3.0)
    ];

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        RenderScene(showResult: false);
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void runSymDifferenceButton_Click(object? sender, EventArgs e)
    {
        RenderScene(showResult: true);
    }

    private void RenderScene(bool showResult)
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddPolygonLayer("Polygon A", PolygonA, SourceAStyle());
        geoKernelViewerControl.AddPolygonLayer("Polygon B", PolygonB, SourceBStyle());

        var details =
            $"SymmetricalDifference(left, right){Environment.NewLine}" +
            $"This keeps areas that belong to only one source polygon.{Environment.NewLine}" +
            $"Left vertices: {PolygonA.Length}{Environment.NewLine}" +
            $"Right vertices: {PolygonB.Length}{Environment.NewLine}" +
            $"Left extent: (-4.40, -1.80) - (1.20, 2.20){Environment.NewLine}" +
            $"Right extent: (-3.60, -2.00) - (3.20, 3.00)";

        if (showResult)
        {
            var resultLayerIndex = geoKernelViewerControl.AddPolygonSymmetricalDifferenceLayer(
                "SymDifference Result",
                PolygonA,
                PolygonB,
                ResultStyle());

            details +=
                $"{Environment.NewLine}Result layer index: {resultLayerIndex}{Environment.NewLine}" +
                "Result type: polygon";

            statusLabel.Text = resultLayerIndex >= 0
                ? "Symmetrical difference result created."
                : "Symmetrical difference returned an empty result.";
        }
        else
        {
            details += $"{Environment.NewLine}Result: click Run Sym Difference to calculate";
            statusLabel.Text = "Source polygons are ready. Click Run Sym Difference.";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.2, -3.2, 5.0, 4.0);
    }

    private static GeoKernelLayerStyle SourceAStyle() => new()
    {
        FillColor = "#BFD7EA",
        FillOpacity = 115,
        LineColor = "#2F80C2",
        LineWidth = 2.0
    };

    private static GeoKernelLayerStyle SourceBStyle() => new()
    {
        FillColor = "#CDE7D8",
        FillOpacity = 115,
        LineColor = "#2D6A4F",
        LineWidth = 2.0
    };

    private static GeoKernelLayerStyle ResultStyle() => new()
    {
        FillColor = "#F9C74F",
        FillOpacity = 155,
        LineColor = "#D95D39",
        LineWidth = 3.0
    };
}
