using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ArcOperations.Wpf;

public partial class MainWindow
{
    private static readonly GeoKernelPoint[] FindQuery = [new(-5.2, 2.2), new(-3.2, 2.2)];
    private static readonly GeoKernelPoint[] FindCandidateA = [new(-1.8, 2.7), new(0.4, 2.7)];
    private static readonly GeoKernelPoint[] FindCandidateB = [new(-5.2, 2.2), new(-3.2, 2.2)];
    private static readonly GeoKernelPoint[] ConnectBase = [new(-5.2, 0.2), new(-3.6, 0.2), new(-2.6, 0.8)];
    private static readonly GeoKernelPoint[] ConnectContinuation = [new(-2.6, 0.8), new(-1.1, 0.1), new(0.4, 0.4)];
    private static readonly GeoKernelPoint[] SplitArc = [new(-5.2, -2.0), new(-1.0, -2.0)];
    private static readonly GeoKernelPoint[] SplitCutter = [new(-3.1, -3.0), new(-3.1, -1.0)];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        RenderScene(showResults: false);
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void RunOperations_Click(object sender, RoutedEventArgs e)
    {
        RenderScene(showResults: true);
        SetSampleExtent();
    }

    private void RenderScene(bool showResults)
    {
        viewerControl.ClearLayers();
        AddSourceArcs();

        var details =
            $"ArcFind / ArcMakeConnected / ArcSplitOnCross{Environment.NewLine}{Environment.NewLine}" +
            $"1. ArcFind{Environment.NewLine}" +
            $"Query arc vertices: {FindQuery.Length}{Environment.NewLine}" +
            $"Candidate count: 2{Environment.NewLine}{Environment.NewLine}" +
            $"2. ArcMakeConnected{Environment.NewLine}" +
            $"Base vertices: {ConnectBase.Length}{Environment.NewLine}" +
            $"Continuation vertices: {ConnectContinuation.Length}{Environment.NewLine}{Environment.NewLine}" +
            $"3. ArcSplitOnCross{Environment.NewLine}" +
            $"Split arc vertices: {SplitArc.Length}{Environment.NewLine}" +
            $"Cutter vertices: {SplitCutter.Length}";

        if (showResults)
        {
            IReadOnlyList<IReadOnlyList<GeoKernelPoint>> candidates = [FindCandidateA, FindCandidateB];
            var foundIndex = viewerControl.FindMatchingArcIndex(FindQuery, candidates);
            if (foundIndex >= 0 && foundIndex < candidates.Count)
                viewerControl.AddPolylineLayer("ArcFind Match", candidates[foundIndex], ResultStyle(0));

            var connectedIndex = viewerControl.AddArcMakeConnectedLayer(
                "Connected Arc",
                ConnectBase,
                [ConnectContinuation],
                ResultStyle(1));

            var splitIndex = viewerControl.AddArcSplitOnCrossLayer(
                "Split Arc Pieces",
                SplitArc,
                [SplitCutter],
                ResultStyle(2));
            viewerControl.AddPolylineLayer("Split Cutter Overlay", SplitCutter, CutterStyle());

            details +=
                $"{Environment.NewLine}{Environment.NewLine}ArcFind result: {(foundIndex >= 0 ? "found" : "not found")}, index: {foundIndex}" +
                $"{Environment.NewLine}ArcMakeConnected result: {(connectedIndex >= 0 ? "created" : "empty")}" +
                $"{Environment.NewLine}ArcSplitOnCross result: {(splitIndex >= 0 ? "created" : "empty")}";

            statusText.Text = "Arc operations calculated.";
        }
        else
        {
            details += $"{Environment.NewLine}{Environment.NewLine}Result: click Run Arc Operations to calculate";
            statusText.Text = "Source arcs are ready. Click Run Arc Operations.";
        }

        detailsTextBox.Text = details;
    }

    private void AddSourceArcs()
    {
        viewerControl.AddPolylineLayer("Find Candidate A", FindCandidateA, SourceStyle());
        viewerControl.AddPolylineLayer("Find Candidate B", FindCandidateB, SourceStyle());
        viewerControl.AddPolylineLayer("Find Query", FindQuery, QueryStyle());
        viewerControl.AddPolylineLayer("Connect Base", ConnectBase, SourceStyle());
        viewerControl.AddPolylineLayer("Connect Continuation", ConnectContinuation, SourceStyle());
        viewerControl.AddPolylineLayer("Split Arc", SplitArc, QueryStyle());
        viewerControl.AddPolylineLayer("Split Cutter", SplitCutter, CutterStyle());
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-5.8, -3.3, 1.0, 3.2);
    }

    private static GeoKernelLayerStyle SourceStyle() => LineStyle("#6C757D", 2.0);
    private static GeoKernelLayerStyle QueryStyle() => LineStyle("#2F80C2", 3.0);
    private static GeoKernelLayerStyle CutterStyle() => LineStyle("#212529", 2.6);

    private static GeoKernelLayerStyle ResultStyle(int index)
    {
        var colors = new[] { "#D95D39", "#2A9D8F", "#7B2CBF" };
        return LineStyle(colors[index % colors.Length], 4.0);
    }

    private static GeoKernelLayerStyle LineStyle(string color, double width) => new()
    {
        FillOpacity = 0,
        LineColor = color,
        LineWidth = width,
        PointColor = color,
        PointSize = width + 4.0
    };
}
