using GeoKernel.NET.WinForms;

namespace GeoKernel.Union.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] PolygonA =
    [
        new(-4.2, -1.7),
        new(0.8, -1.7),
        new(0.8, 2.2),
        new(-4.2, 2.2),
        new(-4.2, -1.7)
    ];

    private static readonly GeoKernelPoint[] PolygonB =
    [
        new(1.0, 3.0),
        new(1.7, 1.2),
        new(3.7, 1.2),
        new(2.1, 0.1),
        new(2.8, -1.8),
        new(1.0, -0.7),
        new(-0.8, -1.8),
        new(-0.1, 0.1),
        new(-1.7, 1.2),
        new(0.3, 1.2),
        new(1.0, 3.0)
    ];

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        RenderScene(showResult: false);
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void runUnionButton_Click(object? sender, EventArgs e)
    {
        RenderScene(showResult: true);
    }

    private void RenderScene(bool showResult)
    {
        geoKernelViewerControl.ClearShapes();
        geoKernelViewerControl.AddPolygonShape(PolygonA, SourceAStyle());
        geoKernelViewerControl.AddPolygonShape(PolygonB, SourceBStyle());

        var details =
            $"Union(left, right){Environment.NewLine}" +
            $"Left vertices: {PolygonA.Length}{Environment.NewLine}" +
            $"Right vertices: {PolygonB.Length}{Environment.NewLine}" +
            $"Left extent: (-4.20, -1.70) - (0.80, 2.20){Environment.NewLine}" +
            $"Right extent: (-1.70, -1.80) - (3.70, 3.00)";

        if (showResult)
        {
            var resultParts = geoKernelViewerControl.UnionPolygons(PolygonA, PolygonB);
            foreach (var part in resultParts)
                geoKernelViewerControl.AddPolygonShape(part, ResultStyle());

            details +=
                $"{Environment.NewLine}Result type: polygon" +
                $"{Environment.NewLine}Result parts: {resultParts.Count}" +
                $"{Environment.NewLine}Result extent: {ExtentText(resultParts)}";

            statusLabel.Text = resultParts.Count > 0
                ? "Union result created."
                : "Union returned an empty result.";
        }
        else
        {
            details += $"{Environment.NewLine}Result: click Run Union to calculate";
            statusLabel.Text = "Source polygons are ready. Click Run Union.";
        }

        detailsTextBox.Text = details;
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.2, -3.2, 5.2, 4.0);
    }

    private static string ExtentText(IEnumerable<IReadOnlyList<GeoKernelPoint>> parts)
    {
        var points = parts.SelectMany(part => part).ToArray();
        return points.Length == 0
            ? "(empty)"
            : $"({points.Min(point => point.X):F2}, {points.Min(point => point.Y):F2}) - ({points.Max(point => point.X):F2}, {points.Max(point => point.Y):F2})";
    }

    private static GeoKernelLayerStyle SourceAStyle() => new()
    {
        FillColor = "#BFD7EA",
        FillOpacity = 135,
        LineColor = "#2F80C2",
        LineWidth = 2.0
    };

    private static GeoKernelLayerStyle SourceBStyle() => new()
    {
        FillColor = "#CDE7D8",
        FillOpacity = 135,
        LineColor = "#2D6A4F",
        LineWidth = 2.0
    };

    private static GeoKernelLayerStyle ResultStyle() => new()
    {
        FillColor = "#F9C74F",
        FillOpacity = 120,
        LineColor = "#D95D39",
        LineWidth = 3.0
    };
}
