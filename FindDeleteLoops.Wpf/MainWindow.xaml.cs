using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.FindDeleteLoops.Wpf;

public partial class MainWindow
{
    private static readonly GeoKernelPoint[][] SourceRings =
    [
        [new(-5.0, -1.7), new(-1.7, -1.7), new(-1.7, 1.6), new(-5.0, 1.6), new(-5.0, -1.7)],
        [new(0.4, -1.7), new(4.5, 1.6), new(0.4, 1.6), new(4.5, -1.7), new(0.4, -1.7)]
    ];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        RenderScene(showResult: false);
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void Run_Click(object sender, RoutedEventArgs e)
    {
        RenderScene(showResult: true);
    }

    private void RenderScene(bool showResult)
    {
        viewerControl.ClearLayers();
        var sourceLayerIndex = viewerControl.AddPolygonLayer("Source: valid part + loop", SourceRings, SourceStyle());
        if (sourceLayerIndex < 0)
        {
            detailsTextBox.Text = "Source polygon could not be created.";
            statusText.Text = "Source polygon creation failed.";
            return;
        }

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
            statusText.Text = "Source polygon is ready. Click Run FindAndDeleteLoops.";
        }
        else
        {
            var layerIndex = viewerControl.AddPolygonFindAndDeleteLoopsLayer(
                "Result: loop removed",
                SourceRings,
                ResultStyle());

            if (layerIndex < 0)
            {
                details += $"{Environment.NewLine}{Environment.NewLine}Result: FindAndDeleteLoops failed; no result layer was created.";
                detailsTextBox.Text = details;
                statusText.Text = "FindAndDeleteLoops failed.";
                return;
            }

            var resultPartCount = viewerControl.GetLayerFeatureCount(layerIndex);
            var resultExtent = viewerControl.GetLayerProjectedExtent(layerIndex);
            var retainedPart = SourceRings[0];

            details +=
                $"{Environment.NewLine}{Environment.NewLine}Result:" +
                $"{Environment.NewLine}Result parts: {resultPartCount}" +
                $"{Environment.NewLine}Result vertices: {retainedPart.Length}" +
                $"{Environment.NewLine}Result extent: {ExtentText(resultExtent)}" +
                $"{Environment.NewLine}Result part details:" +
                $"{Environment.NewLine}part 1: {retainedPart.Length} vertices" +
                $"{Environment.NewLine}{Environment.NewLine}The self-intersecting bow-tie part is removed; the valid part remains.";
            statusText.Text = "FindAndDeleteLoops result created.";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-5.7, -2.8, 5.2, 2.6);
    }

    private static int VertexCount(IEnumerable<IReadOnlyList<GeoKernelPoint>> rings) => rings.Sum(ring => ring.Count);

    private static string PartSummary(IReadOnlyList<IReadOnlyList<GeoKernelPoint>> rings) =>
        string.Join(Environment.NewLine, rings.Select((ring, index) => $"part {index + 1}: {ring.Count} vertices"));

    private static string ExtentText(IEnumerable<IReadOnlyList<GeoKernelPoint>> rings)
    {
        var points = rings.SelectMany(ring => ring).ToArray();
        return $"({points.Min(point => point.X):F2}, {points.Min(point => point.Y):F2}) - ({points.Max(point => point.X):F2}, {points.Max(point => point.Y):F2})";
    }

    private static string ExtentText(GeoKernelExtent extent) =>
        $"({extent.XMin:F2}, {extent.YMin:F2}) - ({extent.XMax:F2}, {extent.YMax:F2})";

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
