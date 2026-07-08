using GeoKernel.NET.WinForms;

namespace GeoKernel.SplitByArc.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] SourcePolygon =
    [
        new(-4.0, -2.0),
        new(3.8, -2.0),
        new(4.5, 0.5),
        new(2.5, 2.4),
        new(-1.5, 2.1),
        new(-4.4, 0.6),
        new(-4.0, -2.0)
    ];

    private static readonly GeoKernelPoint[] SplitArc =
    [
        new(-5.2, 1.4),
        new(-1.8, 0.7),
        new(0.2, -0.2),
        new(2.0, -0.6),
        new(5.1, -1.0)
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

    private void runSplitButton_Click(object? sender, EventArgs e)
    {
        RenderScene(showResult: true);
    }

    private void RenderScene(bool showResult)
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddPolygonLayer("Source Polygon", SourcePolygon, PolygonStyle());
        geoKernelViewerControl.AddPolylineLayer("Split Arc", SplitArc, ArcStyle());

        var details =
            $"SplitByArc(polygon, line){Environment.NewLine}" +
            $"Source polygon parts: 1{Environment.NewLine}" +
            $"Split arc parts: 1{Environment.NewLine}" +
            $"Polygon vertices: {SourcePolygon.Length}{Environment.NewLine}" +
            $"Arc vertices: {SplitArc.Length}{Environment.NewLine}" +
            $"Polygon extent: (-4.40, -2.00) - (4.50, 2.40){Environment.NewLine}" +
            $"Arc extent: (-5.20, -1.00) - (5.10, 1.40)";

        if (showResult)
        {
            var resultLayerIndex = geoKernelViewerControl.AddPolygonSplitByArcLayer(
                "SplitByArc Result",
                SourcePolygon,
                SplitArc,
                ResultStyle());

            geoKernelViewerControl.AddPolylineLayer("Split Arc Overlay", SplitArc, ArcStyle());

            details +=
                $"{Environment.NewLine}Result layer index: {resultLayerIndex}{Environment.NewLine}" +
                "Result type: polygon";

            statusLabel.Text = resultLayerIndex >= 0
                ? "SplitByArc result created."
                : "SplitByArc returned an empty result.";
        }
        else
        {
            details += $"{Environment.NewLine}Result: click Run SplitByArc to calculate";
            statusLabel.Text = "Source polygon and split arc are ready. Click Run SplitByArc.";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.7, -3.0, 5.7, 3.2);
    }

    private static GeoKernelLayerStyle PolygonStyle() => new()
    {
        FillColor = "#BFD7EA",
        FillOpacity = 115,
        LineColor = "#2F80C2",
        LineWidth = 2.2
    };

    private static GeoKernelLayerStyle ArcStyle() => new()
    {
        LineColor = "#2D3436",
        LineWidth = 2.8
    };

    private static GeoKernelLayerStyle ResultStyle() => new()
    {
        FillColor = "#F9C74F",
        FillOpacity = 155,
        LineColor = "#D95D39",
        LineWidth = 2.8
    };
}
