using GeoKernel.NET.WinForms;

namespace GeoKernel.UnionOnList.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[][] SourcePolygons =
    [
        [new(-4.8, -1.4), new(-0.8, -1.4), new(-0.8, 1.8), new(-4.8, 1.8), new(-4.8, -1.4)],
        [new(-2.6, -2.3), new(1.2, -2.3), new(1.2, 0.6), new(-2.6, 0.6), new(-2.6, -2.3)],
        [new(0.3, 2.8), new(0.9, 1.1), new(2.8, 1.1), new(1.3, 0.1), new(2.1, -1.6), new(0.3, -0.6), new(-1.5, -1.6), new(-0.7, 0.1), new(-2.2, 1.1), new(-0.3, 1.1), new(0.3, 2.8)],
        [new(1.5, -0.2), new(4.6, -0.2), new(4.6, 2.0), new(1.5, 2.0), new(1.5, -0.2)],
        [new(2.0, -2.4), new(4.8, -1.2), new(3.3, 0.7), new(2.0, -2.4)]
    ];

    private static readonly string[] FillColors = ["#BFD7EA", "#D8EAC4", "#F3D6A3", "#D9C8F0", "#BFE3D9"];
    private static readonly string[] LineColors = ["#2F80C2", "#5B8E3E", "#B7791F", "#7048A8", "#2D6A4F"];

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

    private void fullExtentButton_Click(object? sender, EventArgs e) => SetSampleExtent();

    private void runUnionButton_Click(object? sender, EventArgs e) => RenderScene(showResult: true);

    private void RenderScene(bool showResult)
    {
        geoKernelViewerControl.ClearLayers();
        for (var i = 0; i < SourcePolygons.Length; ++i)
            geoKernelViewerControl.AddPolygonLayer($"Source {i + 1}", SourcePolygons[i], SourceStyle(i));

        var details =
            $"UnionOnList(shapes){Environment.NewLine}" +
            $"Source polygons: {SourcePolygons.Length}";

        for (var i = 0; i < SourcePolygons.Length; ++i)
            details += $"{Environment.NewLine}Source {i + 1} vertices: {SourcePolygons[i].Length}";

        if (showResult)
        {
            var resultLayerIndex = geoKernelViewerControl.AddPolygonUnionOnListLayer(
                "UnionOnList Result",
                SourcePolygons,
                ResultStyle());

            details +=
                $"{Environment.NewLine}Result layer index: {resultLayerIndex}{Environment.NewLine}" +
                "Result type: polygon";

            statusLabel.Text = resultLayerIndex >= 0
                ? "UnionOnList result created."
                : "UnionOnList returned an empty result.";
        }
        else
        {
            details += $"{Environment.NewLine}Result: click Run UnionOnList to calculate";
            statusLabel.Text = "Source polygons are ready. Click Run UnionOnList.";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.8, -3.3, 5.8, 4.0);
    }

    private static GeoKernelLayerStyle SourceStyle(int index) => new()
    {
        FillColor = FillColors[index % FillColors.Length],
        FillOpacity = 110,
        LineColor = LineColors[index % LineColors.Length],
        LineWidth = 2.0
    };

    private static GeoKernelLayerStyle ResultStyle() => new()
    {
        FillColor = "#F9C74F",
        FillOpacity = 135,
        LineColor = "#D95D39",
        LineWidth = 3.0
    };
}
