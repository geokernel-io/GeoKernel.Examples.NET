using System.Windows;
using System.Windows.Controls;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.TopologyFix.Wpf;

public partial class MainWindow
{
    private static readonly GeoKernelPoint[][] SourceParts =
    [
        [new(-5.2, -1.3), new(-4.0, -0.2), new(-4.0, -0.2), new(-2.6, -1.1), new(-1.2, 0.5), new(-1.2, 0.5), new(0.4, 0.1)],
        [new(1.5, 1.0)],
        [new(2.8, -0.8), new(2.8, -0.8)],
        [new(3.7, -1.1), new(4.8, 0.3), new(5.4, -0.9)]
    ];

    private bool _loaded;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _loaded = true;
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        RenderScene();
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void OperationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loaded)
            RenderScene();
    }

    private void RenderScene()
    {
        viewerControl.ClearLayers();
        AddSourceGeometry();

        var operation = operationComboBox.SelectedIndex;
        var details =
            $"Topology fix functions{Environment.NewLine}{Environment.NewLine}" +
            $"Source: messy multipart polyline{Environment.NewLine}" +
            $"- part 1 has duplicate consecutive vertices{Environment.NewLine}" +
            $"- part 2 has only one vertex{Environment.NewLine}" +
            $"- part 3 collapses to one vertex after duplicate cleanup{Environment.NewLine}" +
            $"- part 4 is already valid{Environment.NewLine}{Environment.NewLine}" +
            $"Source parts: {SourceParts.Length}{Environment.NewLine}" +
            $"Source vertices: {VertexCount(SourceParts)}{Environment.NewLine}" +
            $"Source extent: {ExtentText(SourceParts)}{Environment.NewLine}" +
            $"Source part details:{Environment.NewLine}{PartSummary(SourceParts)}";

        if (operation == 0)
        {
            details += $"{Environment.NewLine}{Environment.NewLine}Choose an operation from the combo box to see the cleaned result.";
            statusText.Text = "Source geometry is shown. Choose a fix operation.";
        }
        else
        {
            var fixOperation = operation switch
            {
                2 => GeoKernelTopologyFixOperation.FixShapeEx,
                3 => GeoKernelTopologyFixOperation.ClearShape,
                _ => GeoKernelTopologyFixOperation.FixShape
            };
            var layerIndex = viewerControl.AddFixedPolylineLayer(
                OperationName(fixOperation),
                SourceParts,
                fixOperation,
                ResultStyle(fixOperation));

            details +=
                $"{Environment.NewLine}{Environment.NewLine}Operation: {OperationName(fixOperation)}" +
                $"{Environment.NewLine}Result layer index: {layerIndex}" +
                $"{Environment.NewLine}{OperationDescription(fixOperation)}";

            statusText.Text = $"{OperationName(fixOperation)} applied.";
        }

        detailsTextBox.Text = details;
    }

    private void AddSourceGeometry()
    {
        viewerControl.AddPolylineLayer("Source valid line parts", [SourceParts[0], SourceParts[3]], SourceStyle());
        viewerControl.AddPointLayer("Source invalid short parts", [SourceParts[1][0], SourceParts[2][0]], InvalidPointStyle());
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-5.9, -2.4, 5.9, 1.8);
    }

    private static int VertexCount(IEnumerable<IReadOnlyList<GeoKernelPoint>> parts) => parts.Sum(part => part.Count);

    private static string PartSummary(IReadOnlyList<IReadOnlyList<GeoKernelPoint>> parts) =>
        string.Join(Environment.NewLine, parts.Select((part, index) => $"part {index + 1}: {part.Count} vertices"));

    private static string ExtentText(IEnumerable<IReadOnlyList<GeoKernelPoint>> parts)
    {
        var points = parts.SelectMany(part => part).ToArray();
        return $"({points.Min(point => point.X):F2}, {points.Min(point => point.Y):F2}) - ({points.Max(point => point.X):F2}, {points.Max(point => point.Y):F2})";
    }

    private static string OperationName(GeoKernelTopologyFixOperation operation) => operation switch
    {
        GeoKernelTopologyFixOperation.FixShapeEx => "FixShapeEx(preserveEmptyParts=true)",
        GeoKernelTopologyFixOperation.ClearShape => "ClearShape",
        _ => "FixShape"
    };

    private static string OperationDescription(GeoKernelTopologyFixOperation operation) => operation switch
    {
        GeoKernelTopologyFixOperation.FixShapeEx => "FixShapeEx keeps short/empty parts for diagnostics, while drawable pieces remain visible.",
        GeoKernelTopologyFixOperation.ClearShape => "ClearShape currently follows the same cleanup path as FixShape.",
        _ => "FixShape removes duplicate vertices and drops invalid short parts."
    };

    private static GeoKernelLayerStyle SourceStyle() => new()
    {
        FillOpacity = 0,
        LineColor = "#6C757D",
        LineWidth = 2.0,
        PointColor = "#6C757D",
        PointSize = 9.0
    };

    private static GeoKernelLayerStyle InvalidPointStyle() => new()
    {
        FillColor = "#D95D39",
        FillOpacity = 255,
        LineColor = "#8A2C1C",
        LineWidth = 1.4,
        PointColor = "#D95D39",
        PointSize = 11.0
    };

    private static GeoKernelLayerStyle ResultStyle(GeoKernelTopologyFixOperation operation)
    {
        var color = operation switch
        {
            GeoKernelTopologyFixOperation.FixShapeEx => "#7B2CBF",
            GeoKernelTopologyFixOperation.ClearShape => "#D95D39",
            _ => "#2A9D8F"
        };

        return new GeoKernelLayerStyle
        {
            FillOpacity = 0,
            LineColor = color,
            LineWidth = 4.0,
            PointColor = color,
            PointSize = 12.0
        };
    }
}
