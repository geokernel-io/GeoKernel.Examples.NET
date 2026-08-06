using GeoKernel.NET.Wpf.Controls;
using System.Windows;
using System.Windows.Controls;

namespace GeoKernel.Topology.Wpf;

public partial class MainWindow
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

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        operationComboBox.SelectedIndex = 0;
        _ready = true;
        RenderOperation();
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e) => SetSampleExtent();
    private void Pan_Click(object sender, RoutedEventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
    private void ZoomBox_Click(object sender, RoutedEventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.ZoomBox;
    private void OperationComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) { if (_ready) RenderOperation(); }

    private void RenderOperation()
    {
        var preservedExtent = viewerControl.ViewExtent;
        var restoreExtent = preservedExtent.XMax > preservedExtent.XMin && preservedExtent.YMax > preservedExtent.YMin;

        viewerControl.ClearLayers();
        AddSources();
        var result = -1;
        var details = ((ComboBoxItem)operationComboBox.SelectedItem).Content?.ToString() ?? string.Empty;

        switch (operationComboBox.SelectedIndex)
        {
            case 0:
                result = viewerControl.AddPolygonBufferLayer("Buffer A", PolygonA, .75, 12, ResultStyle());
                details += $"{Environment.NewLine}MakeBuffer(Polygon A, 0.75)";
                break;
            case 1: result = viewerControl.AddPolygonUnionLayer("Union A + B", PolygonA, PolygonB, ResultStyle()); break;
            case 2: result = viewerControl.AddPolygonIntersectionLayer("Intersection A / B", PolygonA, PolygonB, ResultStyle()); break;
            case 3: result = viewerControl.AddPolygonDifferenceLayer("Difference A - B", PolygonA, PolygonB, ResultStyle()); break;
            case 4: result = viewerControl.AddPolygonSymmetricalDifferenceLayer("Sym Difference A / B", PolygonA, PolygonB, ResultStyle()); break;
            case 5: result = viewerControl.AddPolygonConvexHullTwoLayer("Convex Hull A + B", PolygonA, PolygonB, ResultStyle()); break;
            case 6:
                var crossings = viewerControl.GetPolylineCrossings(DiagonalLine, PolygonB);
                result = viewerControl.AddPointLayer("Crossings", crossings, PointResultStyle());
                details += $"{Environment.NewLine}GetCrossings(Line, Polygon B boundary){Environment.NewLine}Crossings: {crossings.Count}";
                break;
            case 7:
                var valid = viewerControl.CheckPolygonRing(InvalidPolygon);
                details += $"{Environment.NewLine}CheckShape(bow-tie): {valid.ToString().ToLowerInvariant()}";
                break;
            case 8: result = viewerControl.AddArcMakeConnectedLayer("Connected Arc", ArcA, [ArcB], ResultLineStyle()); break;
            case 9: result = viewerControl.AddArcSplitOnCrossLayer("Split Arc", SplitArc, [SplitCutter], ResultLineStyle()); break;
            case 10:
                var matrix = viewerControl.RelatePolygonRings(PolygonA, PolygonB);
                details += $"{Environment.NewLine}Relate matrix: {matrix}" +
                           $"{Environment.NewLine}Intersect: {viewerControl.RelatePolygonRings(PolygonA, PolygonB, "T").ToString().ToLowerInvariant()}" +
                           $"{Environment.NewLine}Overlap: {viewerControl.RelatePolygonRings(PolygonA, PolygonB, "T*T***T").ToString().ToLowerInvariant()}" +
                           $"{Environment.NewLine}CheckShape(A): {viewerControl.CheckPolygonRing(PolygonA).ToString().ToLowerInvariant()}" +
                           $"{Environment.NewLine}CheckShape(bow-tie): {viewerControl.CheckPolygonRing(InvalidPolygon).ToString().ToLowerInvariant()}";
                break;
        }

        if (result >= 0) details += $"{Environment.NewLine}Result layer index: {result}";
        detailsTextBox.Text = details;
        statusText.Text = result >= 0 ? "Topology operation result is displayed." : "Topology operation report is ready.";
        if (restoreExtent) viewerControl.ViewExtent = preservedExtent;
    }

    private void AddSources()
    {
        viewerControl.AddPolygonLayer("Polygon A", PolygonA, PolygonAStyle());
        viewerControl.AddPolygonLayer("Polygon B", PolygonB, PolygonBStyle());
        viewerControl.AddPolylineLayer("Diagonal Line", DiagonalLine, SourceLineStyle());
        viewerControl.AddPolygonLayer("Invalid Polygon", InvalidPolygon, InvalidStyle());
        viewerControl.AddPolylineLayer("Arc A", ArcA, ArcStyle());
        viewerControl.AddPolylineLayer("Arc B", ArcB, ArcStyle());
        viewerControl.AddPolylineLayer("Split Arc", SplitArc, ArcStyle());
        viewerControl.AddPolylineLayer("Split Cutter", SplitCutter, SourceLineStyle());
    }

    private void SetSampleExtent() => viewerControl.ViewExtent = new GeoKernelExtent(-7.3, -7.4, 7, 5);
    private static GeoKernelPoint[] Rectangle(double x1,double y1,double x2,double y2) => [new(x1,y1),new(x2,y1),new(x2,y2),new(x1,y2),new(x1,y1)];
    private static GeoKernelLayerStyle PolygonAStyle() => CreateStyle("#BFD7EA",165,"#2F80C2",2);
    private static GeoKernelLayerStyle PolygonBStyle() => CreateStyle("#CDE7D8",165,"#2D6A4F",2);
    private static GeoKernelLayerStyle InvalidStyle() => CreateStyle("#F8D7DA",115,"#B23A48",2);
    private static GeoKernelLayerStyle SourceLineStyle() => CreateStyle("#FFFFFF",0,"#2F2F2F",2);
    private static GeoKernelLayerStyle ArcStyle() => CreateStyle("#FFFFFF",0,"#6C4AB6",2);
    private static GeoKernelLayerStyle ResultStyle() => CreateStyle("#F9C74F",155,"#D95D39",3);
    private static GeoKernelLayerStyle ResultLineStyle() => CreateStyle("#FFFFFF",0,"#D95D39",4);
    private static GeoKernelLayerStyle PointResultStyle() => new() { PointColor="#D95D39",PointSize=12,LineColor="#8C321D",LineWidth=1.5 };
    private static GeoKernelLayerStyle CreateStyle(string fill,int opacity,string line,double width) => new() { FillColor=fill,FillOpacity=opacity,LineColor=line,LineWidth=width,PointColor=line,PointSize=9 };
}
