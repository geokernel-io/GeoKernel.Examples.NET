using GeoKernel.NET.WinForms;

namespace GeoKernel.FindDeleteLoops.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[][] SourceRings =
    [
        [new(-5.0, -1.7), new(-1.7, -1.7), new(-1.7, 1.6), new(-5.0, 1.6), new(-5.0, -1.7)],
        [new(0.4, -1.7), new(4.5, 1.6), new(0.4, 1.6), new(4.5, -1.7), new(0.4, -1.7)]
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

    private void runButton_Click(object? sender, EventArgs e)
    {
        RenderScene(showResult: true);
    }

    private void RenderScene(bool showResult)
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddPolygonLayer("Source: valid part + loop", SourceRings, SourceStyle());

        var details =
            $"FindAndDeleteLoops - remove self-intersecting polygon parts{Environment.NewLine}{Environment.NewLine}" +
            $"Source geometry:{Environment.NewLine}" +
            $"- left part is a normal valid rectangle{Environment.NewLine}" +
            $"- right part is a bow-tie loop that crosses itself{Environment.NewLine}{Environment.NewLine}" +
            $"Source parts: {SourceRings.Length}{Environment.NewLine}" +
            $"Source vertices: {VertexCount(SourceRings)}{Environment.NewLine}" +
            $"Source extent: {ExtentText(SourceRings)}{Environment.NewLine}" +
            $"Source part details:{Environment.NewLine}{PartSummary(SourceRings)}";

        if (!showResult)
        {
            details += $"{Environment.NewLine}{Environment.NewLine}Click Run FindAndDeleteLoops to remove the self-intersecting part.";
            statusLabel.Text = "Source polygon is ready. Click Run FindAndDeleteLoops.";
        }
        else
        {
            var layerIndex = geoKernelViewerControl.AddPolygonFindAndDeleteLoopsLayer(
                "Result: loop removed",
                SourceRings,
                ResultStyle());

            details +=
                $"{Environment.NewLine}{Environment.NewLine}Result:" +
                $"{Environment.NewLine}Result layer index: {layerIndex}" +
                $"{Environment.NewLine}Expected result: the self-intersecting bow-tie part is removed; the valid part remains.";
            statusLabel.Text = "FindAndDeleteLoops result created.";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.7, -2.8, 5.2, 2.6);
    }

    private static int VertexCount(IEnumerable<IReadOnlyList<GeoKernelPoint>> rings) => rings.Sum(ring => ring.Count);

    private static string PartSummary(IReadOnlyList<IReadOnlyList<GeoKernelPoint>> rings) =>
        string.Join(Environment.NewLine, rings.Select((ring, index) => $"part {index + 1}: {ring.Count} vertices"));

    private static string ExtentText(IEnumerable<IReadOnlyList<GeoKernelPoint>> rings)
    {
        var points = rings.SelectMany(ring => ring).ToArray();
        return $"({points.Min(point => point.X):F2}, {points.Min(point => point.Y):F2}) - ({points.Max(point => point.X):F2}, {points.Max(point => point.Y):F2})";
    }

    private static GeoKernelLayerStyle SourceStyle() => new()
    {
        FillColor = "#F6D6AD",
        FillOpacity = 115,
        LineColor = "#D95D39",
        LineWidth = 2.4
    };

    private static GeoKernelLayerStyle ResultStyle() => new()
    {
        FillColor = "#CDE7D8",
        FillOpacity = 170,
        LineColor = "#2A9D8F",
        LineWidth = 4.0
    };
}
