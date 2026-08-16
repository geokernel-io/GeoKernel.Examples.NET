using System.Windows;
using System.Windows.Controls;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.WktWrite.Wpf;

public partial class MainWindow
{
    private const string PointLayerName = "Drawn Point";
    private const string PolylineLayerName = "Drawn Polyline";
    private const string PolygonLayerName = "Drawn Polygon";

    private readonly int[] _layerIndexes = [-1, -1, -1];
    private bool _loaded;
    private bool _drawingSketch;

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.AddOpenStreetMapLayer();
        CreateLayer(PointLayerName, GeoKernelShapeType.Point, PointStyle());
        CreateLayer(PolylineLayerName, GeoKernelShapeType.Polyline, LineStyle());
        CreateLayer(PolygonLayerName, GeoKernelShapeType.Polygon, PolygonStyle());
        ResolveLayerIndexes();
        viewerControl.LayerEditStateChanged += Viewer_LayerEditStateChanged;
        viewerControl.MapMouseDown += Viewer_MapMouseDown;
        _loaded = true;
        ActivateSelectedMode();
        viewerControl.ViewExtent = InitialViewExtent();
    }

    private int CreateLayer(string name, GeoKernelShapeType shapeType, GeoKernelLayerStyle style)
    {
        var index = viewerControl.AddEmptyVectorLayer(name, shapeType, style);
        if (index >= 0)
            viewerControl.SetLayerCoordinateSystemPreset(index, GeoKernelCoordinateSystemPreset.Wgs84);
        return index;
    }

    private void ResolveLayerIndexes()
    {
        _layerIndexes[(int)GeometryMode.Point] = viewerControl.GetLayerInfoByName(PointLayerName)?.Index ?? -1;
        _layerIndexes[(int)GeometryMode.Polyline] = viewerControl.GetLayerInfoByName(PolylineLayerName)?.Index ?? -1;
        _layerIndexes[(int)GeometryMode.Polygon] = viewerControl.GetLayerInfoByName(PolygonLayerName)?.Index ?? -1;
    }

    private void GeometryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_loaded) return;
        _drawingSketch = false;
        ClearAllLayers();
        detailsTextBox.Clear();
        ActivateSelectedMode();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _drawingSketch = false;
        ClearAllLayers();
        detailsTextBox.Clear();
        ActivateSelectedMode();
        statusText.Text = "Drawn geometries cleared.";
    }

    private void Viewer_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != ActiveLayerIndex) return;
        _drawingSketch = false;
        RefreshMap();
        WriteWkt();
    }

    private void Viewer_MapMouseDown(object? sender, GeoKernelMapMouseEventArgs e)
    {
        const int leftButton = 1;
        if ((e.ButtonOrButtons & leftButton) == 0 || e.Tool != ActiveTool)
            return;

        var startsNewGeometry = CurrentMode == GeometryMode.Point || !_drawingSketch;
        if (!startsNewGeometry)
            return;

        if (viewerControl.GetLayerFeatureCount(ActiveLayerIndex) > 0)
        {
            ClearLayer(ActiveLayerIndex);
            detailsTextBox.Clear();
            ActivateSelectedMode();
            RefreshMap();
        }

        if (CurrentMode != GeometryMode.Point)
            _drawingSketch = true;
    }

    private void ActivateSelectedMode()
    {
        var index = ActiveLayerIndex;
        if (index < 0)
        {
            statusText.Text = "Editable layer could not be created.";
            return;
        }
        if (!viewerControl.IsLayerEditing(index)) viewerControl.BeginEditLayer(index);
        viewerControl.SetActiveEditLayerIndex(index);
        viewerControl.ActiveTool = ActiveTool;
        hintText.Text = HelpText(CurrentMode);
        statusText.Text = hintText.Text;
    }

    private void ClearAllLayers()
    {
        foreach (var index in _layerIndexes.Where(index => index >= 0))
            ClearLayer(index);
        RefreshMap();
    }

    private void ClearLayer(int index)
    {
        viewerControl.RollbackEditLayer(index);
        viewerControl.BeginEditLayer(index);
    }

    private void WriteWkt()
    {
        var wkt = viewerControl.WriteLayerLastShapeWkt(ActiveLayerIndex);
        if (string.IsNullOrWhiteSpace(wkt))
        {
            ShowEmptyDetails();
            return;
        }
        var api = ApiName(CurrentMode);
        detailsTextBox.Text = string.Join(Environment.NewLine,
            "WktWrite sample", "", "API", api, "", "Selected geometry", GeometryName,
            $"Layer feature count: {viewerControl.GetLayerFeatureCount(ActiveLayerIndex)}", "", "Output WKT", wkt, "",
            "Workflow", "1. Choose geometry type.", "2. Draw geometry on map.",
            "3. WKT is written automatically when drawing finishes.");
        statusText.Text = $"{api} wrote WKT from the drawn {GeometryName}.";
    }

    private void ShowEmptyDetails() => detailsTextBox.Text =
        $"{ApiName(CurrentMode)}{Environment.NewLine}{Environment.NewLine}Draw a geometry first. WKT will be written automatically.";

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(true, true);
        viewerControl.RefreshLayers();
    }

    private GeometryMode CurrentMode => (GeometryMode)geometryComboBox.SelectedIndex;
    private int ActiveLayerIndex => _layerIndexes[(int)CurrentMode];
    private string GeometryName => ((ComboBoxItem)geometryComboBox.SelectedItem).Content?.ToString() ?? CurrentMode.ToString();
    private GeoKernelViewerTool ActiveTool => CurrentMode switch
    {
        GeometryMode.Point => GeoKernelViewerTool.AddPoint,
        GeometryMode.Polyline => GeoKernelViewerTool.AddPolyline,
        _ => GeoKernelViewerTool.AddPolygon
    };

    private static string ApiName(GeometryMode mode) => mode switch
    {
        GeometryMode.Point => "GisWktWriter::writePoint(shape)",
        GeometryMode.Polyline => "GisWktWriter::writePolyline(shape)",
        _ => "GisWktWriter::writePolygon(shape)"
    };

    private static string HelpText(GeometryMode mode) => mode switch
    {
        GeometryMode.Point => "Click on the map to draw a point. WKT is written automatically.",
        GeometryMode.Polyline => "Click line vertices, then press Enter or double-click to finish. WKT is written automatically.",
        _ => "Click polygon vertices, then press Enter or double-click to finish. WKT is written automatically."
    };

    private static GeoKernelExtent InitialViewExtent()
    {
        var min = ToWebMercator(new GeoKernelPoint(-124.8, 32.0));
        var max = ToWebMercator(new GeoKernelPoint(-114.0, 42.5));
        return new GeoKernelExtent(min.X, min.Y, max.X, max.Y);
    }

    private static GeoKernelPoint ToWebMercator(GeoKernelPoint point)
    {
        const double originShift = 20037508.342789244;
        var latitude = Math.Clamp(point.Y, -85.05112878, 85.05112878);
        return new GeoKernelPoint(point.X * originShift / 180.0,
            Math.Log(Math.Tan((90.0 + latitude) * Math.PI / 360.0)) * originShift / Math.PI);
    }

    private static GeoKernelLayerStyle PointStyle() => new() { PointColor = "#D95D39", LineColor = "#8C321D", PointSize = 13, LineWidth = 1.4 };
    private static GeoKernelLayerStyle LineStyle() => new() { LineColor = "#E4572E", LineWidth = 3.4, PointColor = "#F3A712", PointSize = 7 };
    private static GeoKernelLayerStyle PolygonStyle() => new() { FillColor = "#88D18A", FillOpacity = 128, LineColor = "#1F7A4D", LineWidth = 2.4 };

    private enum GeometryMode { Point, Polyline, Polygon }
}
