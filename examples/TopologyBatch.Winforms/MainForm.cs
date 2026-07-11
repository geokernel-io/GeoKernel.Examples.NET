using System.Diagnostics;
using GeoKernel.NET.WinForms;

namespace GeoKernel.TopologyBatch.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[][] SourcePolygons = CreateSourcePolygons();

    private static readonly string[] FillColors =
    [
        "#BFD7EA",
        "#D8EAC4",
        "#F3D6A3",
        "#D9C8F0",
        "#BFE3D9",
        "#F0C7C7"
    ];

    private static readonly string[] LineColors =
    [
        "#2F80C2",
        "#5B8E3E",
        "#B7791F",
        "#7048A8",
        "#2D6A4F",
        "#B23A48"
    ];

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        RenderScene(runBatch: false);
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e) => SetSampleExtent();

    private void runBatchButton_Click(object? sender, EventArgs e) => RenderScene(runBatch: true);

    private void RenderScene(bool runBatch)
    {
        geoKernelViewerControl.ClearLayers();

        var details = new List<string>
        {
            "TopologyBatch",
            "Batch flow: CheckShape each polygon, then UnionOnList(valid polygons).",
            "",
            $"Source polygon count: {SourcePolygons.Length}"
        };

        if (!runBatch)
        {
            AddSourceLayers(validated: false);
            details.Add("");
            details.Add("Click Run Batch to validate all polygons and build the union.");
            statusLabel.Text = "Source polygons are ready. Click Run Batch.";
            detailsTextBox.Text = string.Join(Environment.NewLine, details);
            return;
        }

        var validPolygons = new List<GeoKernelPoint[]>();
        var invalidCount = 0;
        var sourceVertexCount = 0;

        details.Add("");
        details.Add("Validation:");

        for (var i = 0; i < SourcePolygons.Length; ++i)
        {
            var polygon = SourcePolygons[i];
            var isValid = geoKernelViewerControl.CheckPolygonRing(polygon);
            sourceVertexCount += polygon.Length;

            details.Add($"P{i + 1}: {(isValid ? "valid" : "invalid")}, vertices={polygon.Length}");

            if (isValid)
            {
                validPolygons.Add(polygon);
                geoKernelViewerControl.AddPolygonLayer($"P{i + 1}", polygon, SourceStyle(i, validated: true));
            }
            else
            {
                invalidCount++;
                geoKernelViewerControl.AddPolygonLayer($"P{i + 1} invalid", polygon, InvalidStyle());
            }
        }

        details.Add("");
        details.Add($"Valid polygons used for union: {validPolygons.Count}");
        details.Add($"Invalid polygons skipped: {invalidCount}");
        details.Add($"Source vertex total: {sourceVertexCount}");

        var stopwatch = Stopwatch.StartNew();
        var resultLayerIndex = geoKernelViewerControl.AddPolygonUnionOnListLayer(
            "Batch Union Result",
            validPolygons,
            UnionStyle());
        stopwatch.Stop();

        details.Add("");
        details.Add("Union result:");
        details.Add(resultLayerIndex >= 0 ? $"Result layer index: {resultLayerIndex}" : "Result: empty");
        details.Add($"Elapsed: {stopwatch.ElapsedMilliseconds} ms");

        statusLabel.Text = resultLayerIndex >= 0
            ? $"Batch topology completed: {validPolygons.Count} valid polygon(s), {stopwatch.ElapsedMilliseconds} ms."
            : "Batch topology returned an empty result.";

        detailsTextBox.Text = string.Join(Environment.NewLine, details);
    }

    private void AddSourceLayers(bool validated)
    {
        for (var i = 0; i < SourcePolygons.Length; ++i)
            geoKernelViewerControl.AddPolygonLayer($"P{i + 1}", SourcePolygons[i], SourceStyle(i, validated));
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-6.5, -3.0, 5.8, 3.6);
    }

    private static GeoKernelPoint[][] CreateSourcePolygons()
    {
        var polygons = new List<GeoKernelPoint[]>();

        for (var row = 0; row < 3; ++row)
        {
            for (var col = 0; col < 4; ++col)
            {
                var x = -5.4 + col * 2.35 + row % 2 * 0.45;
                var y = -2.2 + row * 1.55;
                polygons.Add(Rectangle(x, y, x + 2.15, y + 1.35));
            }
        }

        polygons.Add(Diamond(-2.8, 1.2, 1.45, 1.0));
        polygons.Add(Diamond(2.2, -0.9, 1.35, 0.9));

        return polygons.ToArray();
    }

    private static GeoKernelPoint[] Rectangle(double xMin, double yMin, double xMax, double yMax) =>
    [
        new(xMin, yMin),
        new(xMax, yMin),
        new(xMax, yMax),
        new(xMin, yMax),
        new(xMin, yMin)
    ];

    private static GeoKernelPoint[] Diamond(double cx, double cy, double rx, double ry) =>
    [
        new(cx, cy + ry),
        new(cx + rx, cy),
        new(cx, cy - ry),
        new(cx - rx, cy),
        new(cx, cy + ry)
    ];

    private static GeoKernelLayerStyle SourceStyle(int index, bool validated = false) => new()
    {
        FillColor = FillColors[index % FillColors.Length],
        FillOpacity = validated ? 135 : 90,
        LineColor = LineColors[index % LineColors.Length],
        LineWidth = validated ? 2.2 : 1.5,
        ShowLabels = true,
        LabelField = "LABEL",
        LabelFontSize = 9.5,
        LabelColor = "#202124",
        LabelHaloEnabled = true,
        LabelHaloColor = "#FFFFFF",
        LabelHaloWidth = 2.0
    };

    private static GeoKernelLayerStyle InvalidStyle() => new()
    {
        FillColor = "#F4A261",
        FillOpacity = 165,
        LineColor = "#D62828",
        LineWidth = 3.0
    };

    private static GeoKernelLayerStyle UnionStyle() => new()
    {
        FillColor = "#F9C74F",
        FillOpacity = 135,
        LineColor = "#D95D39",
        LineWidth = 4.0
    };
}
