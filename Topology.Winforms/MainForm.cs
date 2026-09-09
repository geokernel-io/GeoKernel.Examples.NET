using GeoKernel.NET.WinForms;

namespace GeoKernel.Topology.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] PolygonA = Rectangle(-5, -2, 1, 3);
    private static readonly GeoKernelPoint[] PolygonB = Rectangle(-1, -1, 5, 4);
    private static readonly GeoKernelPoint[] DiagonalLine = [new(-6, -3), new(6, 4)];
    private static readonly GeoKernelPoint[] InvalidPolygon = [new(3, -6.4), new(6.2, -3.2), new(3, -3.2), new(6.2, -6.4), new(3, -6.4)];
    private static readonly GeoKernelPoint[] ArcA = [new(-6, -5.5), new(-4.4, -4.4), new(-2.7, -5.4)];
    private static readonly GeoKernelPoint[] ArcB = [new(-2.7, -5.4), new(-0.7, -4.2), new(1.5, -5.3)];
    private static readonly GeoKernelPoint[] SplitArc = [new(-5.7, -6.7), new(2.2, -4.1)];
    private static readonly GeoKernelPoint[] SplitCutter = [new(-2, -7.1), new(-2, -3.7)];
    private bool _ready;

    public MainForm()
    {
        InitializeComponent();
        operationComboBox.Items.AddRange([
            "Buffer A", "Union A + B", "Intersection A / B", "Difference A - B",
            "Sym Difference A / B", "Convex Hull A + B", "Crossings Line / B",
            "Check Invalid Polygon", "Arc Make Connected", "Arc Split On Cross", "Predicate Report"]);
        operationComboBox.SelectedIndex = 0;
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        _ready = true;
        RenderOperation();
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e) => SetSampleExtent();
    private void panButton_Click(object? sender, EventArgs e) => geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
    private void zoomBoxButton_Click(object? sender, EventArgs e) => geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.ZoomBox;
    private void operationComboBox_SelectedIndexChanged(object? sender, EventArgs e) { if (_ready) RenderOperation(); }

    private void RenderOperation()
    {
        var preservedExtent = geoKernelViewerControl.ViewExtent;
        var restoreExtent = preservedExtent.XMax > preservedExtent.XMin &&
                            preservedExtent.YMax > preservedExtent.YMin;

        geoKernelViewerControl.ClearLayers();
        AddSources();
        var result = -1;
        var details = operationComboBox.Text;

        switch (operationComboBox.SelectedIndex)
        {
            case 0:
                result = geoKernelViewerControl.AddPolygonBufferLayer("Buffer A", PolygonA, 0.75, 12, ResultStyle());
                details += $"{Environment.NewLine}MakeBuffer(Polygon A, 0.75)";
                break;
            case 1:
                result = geoKernelViewerControl.AddPolygonUnionLayer("Union A + B", PolygonA, PolygonB, ResultStyle());
                break;
            case 2:
                result = geoKernelViewerControl.AddPolygonIntersectionLayer("Intersection A / B", PolygonA, PolygonB, ResultStyle());
                break;
            case 3:
                result = geoKernelViewerControl.AddPolygonDifferenceLayer("Difference A - B", PolygonA, PolygonB, ResultStyle());
                break;
            case 4:
                result = geoKernelViewerControl.AddPolygonSymmetricalDifferenceLayer("Sym Difference A / B", PolygonA, PolygonB, ResultStyle());
                break;
            case 5:
                result = geoKernelViewerControl.AddPolygonConvexHullTwoLayer("Convex Hull A + B", PolygonA, PolygonB, ResultStyle());
                break;
            case 6:
                var crossings = geoKernelViewerControl.GetPolylineCrossings(DiagonalLine, PolygonB);
                result = geoKernelViewerControl.AddPointLayer("Crossings", crossings, PointResultStyle());
                details += $"{Environment.NewLine}GetCrossings(Line, Polygon B boundary){Environment.NewLine}Crossings: {crossings.Count}";
                break;
            case 7:
                var valid = geoKernelViewerControl.CheckPolygonRing(InvalidPolygon);
                details += $"{Environment.NewLine}CheckShape(bow-tie): {valid.ToString().ToLowerInvariant()}";
                break;
            case 8:
                result = geoKernelViewerControl.AddArcMakeConnectedLayer("Connected Arc", ArcA, [ArcB], ResultLineStyle());
                break;
            case 9:
                result = geoKernelViewerControl.AddArcSplitOnCrossLayer("Split Arc", SplitArc, [SplitCutter], ResultLineStyle());
                break;
            case 10:
                var matrix = geoKernelViewerControl.RelatePolygonRings(PolygonA, PolygonB);
                details += $"{Environment.NewLine}Relate matrix: {matrix}" +
                           $"{Environment.NewLine}Intersect: {geoKernelViewerControl.RelatePolygonRings(PolygonA, PolygonB, "T").ToString().ToLowerInvariant()}" +
                           $"{Environment.NewLine}Overlap: {geoKernelViewerControl.RelatePolygonRings(PolygonA, PolygonB, "T*T***T").ToString().ToLowerInvariant()}" +
                           $"{Environment.NewLine}CheckShape(A): {geoKernelViewerControl.CheckPolygonRing(PolygonA).ToString().ToLowerInvariant()}" +
                           $"{Environment.NewLine}CheckShape(bow-tie): {geoKernelViewerControl.CheckPolygonRing(InvalidPolygon).ToString().ToLowerInvariant()}";
                break;
        }

        if (result >= 0) details += $"{Environment.NewLine}Result layer index: {result}";
        detailsTextBox.Text = details;
        statusLabel.Text = result >= 0 ? "Topology operation result is displayed." : "Topology operation report is ready.";

        // The layer-producing topology helpers can update the native full
        // extent. Qt renders result shapes without changing the current view,
        // so preserve the user's current view across operation changes.
        if (restoreExtent)
            geoKernelViewerControl.ViewExtent = preservedExtent;
    }

    private void AddSources()
    {
        geoKernelViewerControl.AddPolygonLayer("Polygon A", PolygonA, PolygonAStyle());
        geoKernelViewerControl.AddPolygonLayer("Polygon B", PolygonB, PolygonBStyle());
        geoKernelViewerControl.AddPolylineLayer("Diagonal Line", DiagonalLine, SourceLineStyle());
        geoKernelViewerControl.AddPolygonLayer("Invalid Polygon", InvalidPolygon, InvalidStyle());
        geoKernelViewerControl.AddPolylineLayer("Arc A", ArcA, ArcStyle());
        geoKernelViewerControl.AddPolylineLayer("Arc B", ArcB, ArcStyle());
        geoKernelViewerControl.AddPolylineLayer("Split Arc", SplitArc, ArcStyle());
        geoKernelViewerControl.AddPolylineLayer("Split Cutter", SplitCutter, SourceLineStyle());
    }

    private void SetSampleExtent() => geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-7.3, -7.4, 7, 5);
    private static GeoKernelPoint[] Rectangle(double x1, double y1, double x2, double y2) => [new(x1,y1),new(x2,y1),new(x2,y2),new(x1,y2),new(x1,y1)];
    private static GeoKernelLayerStyle PolygonAStyle() => Style("#BFD7EA", 165, "#2F80C2", 2);
    private static GeoKernelLayerStyle PolygonBStyle() => Style("#CDE7D8", 165, "#2D6A4F", 2);
    private static GeoKernelLayerStyle InvalidStyle() => Style("#F8D7DA", 115, "#B23A48", 2);
    private static GeoKernelLayerStyle SourceLineStyle() => Style("#FFFFFF", 0, "#2F2F2F", 2);
    private static GeoKernelLayerStyle ArcStyle() => Style("#FFFFFF", 0, "#6C4AB6", 2);
    private static GeoKernelLayerStyle ResultStyle() => Style("#F9C74F", 155, "#D95D39", 3);
    private static GeoKernelLayerStyle ResultLineStyle() => Style("#FFFFFF", 0, "#D95D39", 4);
    private static GeoKernelLayerStyle PointResultStyle() => new() { PointColor="#D95D39", PointSize=12, LineColor="#8C321D", LineWidth=1.5 };
    private static GeoKernelLayerStyle Style(string fill, int opacity, string line, double width) => new() { FillColor=fill, FillOpacity=opacity, LineColor=line, LineWidth=width, PointColor=line, PointSize=9 };
}
