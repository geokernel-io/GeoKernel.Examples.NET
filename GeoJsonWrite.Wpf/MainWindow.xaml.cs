using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GeoJsonWrite.Wpf;

public partial class MainWindow
{
    private const string LayerName = "Drawn Polygon";
    private int _polygonLayerIndex = -1;
    private bool _drawingSketch;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.AddOpenStreetMapLayer();
        _polygonLayerIndex = viewerControl.AddEmptyVectorLayer(
            LayerName,
            GeoKernelShapeType.Polygon,
            PolygonStyle());
        if (_polygonLayerIndex >= 0)
            viewerControl.SetLayerCoordinateSystemPreset(
                _polygonLayerIndex,
                GeoKernelCoordinateSystemPreset.Wgs84);

        viewerControl.LayerEditStateChanged += Viewer_LayerEditStateChanged;
        viewerControl.MapMouseDown += Viewer_MapMouseDown;
        ActivatePolygonTool();
        viewerControl.ViewExtent = InitialViewExtent();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        _drawingSketch = false;
        ClearLayer();
        ShowInitialDetails();
        ActivatePolygonTool();
        statusText.Text = "Polygon cleared.";
    }

    private void Viewer_MapMouseDown(object? sender, GeoKernelMapMouseEventArgs e)
    {
        const int leftButton = 1;
        if ((e.ButtonOrButtons & leftButton) == 0 ||
            e.Tool != GeoKernelViewerTool.AddPolygon ||
            _drawingSketch)
            return;

        if (viewerControl.GetLayerFeatureCount(_polygonLayerIndex) > 0)
        {
            ClearLayer();
            detailsTextBox.Clear();
            ActivatePolygonTool();
        }

        _drawingSketch = true;
    }

    private void Viewer_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _polygonLayerIndex)
            return;

        _drawingSketch = false;
        RefreshMap();
        WriteGeoJson();
    }

    private void ActivatePolygonTool()
    {
        if (_polygonLayerIndex < 0)
        {
            statusText.Text = "Editable polygon layer is not in the viewer.";
            return;
        }

        if (!viewerControl.IsLayerEditing(_polygonLayerIndex) &&
            !viewerControl.BeginEditLayer(_polygonLayerIndex))
        {
            statusText.Text = "Polygon layer could not enter edit mode.";
            return;
        }

        if (!viewerControl.SetActiveEditLayerIndex(_polygonLayerIndex))
        {
            statusText.Text = "Polygon layer could not be activated.";
            return;
        }

        viewerControl.ActiveTool = GeoKernelViewerTool.AddPolygon;
        statusText.Text = "Add Polygon active. Finish with Enter or double-click.";
    }

    private void ClearLayer()
    {
        if (_polygonLayerIndex < 0)
            return;

        viewerControl.RollbackEditLayer(_polygonLayerIndex);
        viewerControl.BeginEditLayer(_polygonLayerIndex);
        RefreshMap();
    }

    private void WriteGeoJson()
    {
        var wkt = viewerControl.WriteLayerLastShapeWkt(_polygonLayerIndex);
        if (string.IsNullOrWhiteSpace(wkt))
        {
            ShowEmptyDetails();
            return;
        }

        var rings = viewerControl.ReadWktPolygon(wkt);
        var geoJson = viewerControl.WriteGeoJsonPolygon(rings);
        var points = rings.SelectMany(ring => ring).ToArray();
        var extent = new GeoKernelExtent(
            points.Min(point => point.X),
            points.Min(point => point.Y),
            points.Max(point => point.X),
            points.Max(point => point.Y));

        detailsTextBox.Text = string.Join(
            Environment.NewLine,
            "GeoJsonWrite sample",
            "",
            "API",
            "GisGeoJsonWriter::writePolygonString(shape)",
            "",
            "Drawn polygon",
            $"Rings: {rings.Count}",
            $"Vertices: {points.Length}",
            $"Lon/lat extent: {extent}",
            "",
            "Output GeoJSON",
            geoJson,
            "",
            "Workflow",
            "1. Click polygon vertices on the map.",
            "2. Press Enter or double-click to finish.",
            "3. GeoJSON is written automatically.");
        statusText.Text = "GisGeoJsonWriter::writePolygonString wrote polygon GeoJSON.";
    }

    private void ShowInitialDetails() => detailsTextBox.Text = string.Join(
        Environment.NewLine,
        "GisGeoJsonWriter::writePolygonString(shape)",
        "",
        "Draw a polygon on the map. The GeoJSON string will appear here.");

    private void ShowEmptyDetails() => detailsTextBox.Text = string.Join(
        Environment.NewLine,
        "GisGeoJsonWriter::writePolygonString(shape)",
        "",
        "Draw a polygon first. GeoJSON will be written automatically.");

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(true, true);
        viewerControl.RefreshLayers();
    }

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
        return new GeoKernelPoint(
            point.X * originShift / 180.0,
            Math.Log(Math.Tan((90.0 + latitude) * Math.PI / 360.0)) *
            originShift / Math.PI);
    }

    private static GeoKernelLayerStyle PolygonStyle() => new()
    {
        FillColor = "#88D18A",
        FillOpacity = 128,
        LineColor = "#1F7A4D",
        LineWidth = 2.4
    };
}
