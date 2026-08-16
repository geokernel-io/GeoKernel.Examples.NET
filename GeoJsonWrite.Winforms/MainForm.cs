using GeoKernel.NET.WinForms;

namespace GeoKernel.GeoJsonWrite.Winforms;

public sealed partial class MainForm : Form
{
    private const string LayerName = "Drawn Polygon";
    private int _polygonLayerIndex = -1;
    private bool _drawingSketch;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.AddOpenStreetMapLayer();
        _polygonLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(
            LayerName,
            GeoKernelShapeType.Polygon,
            PolygonStyle());
        if (_polygonLayerIndex >= 0)
            geoKernelViewerControl.SetLayerCoordinateSystemPreset(
                _polygonLayerIndex,
                GeoKernelCoordinateSystemPreset.Wgs84);

        geoKernelViewerControl.LayerEditStateChanged += Viewer_LayerEditStateChanged;
        geoKernelViewerControl.MapMouseDown += Viewer_MapMouseDown;
        ActivatePolygonTool();
        geoKernelViewerControl.ViewExtent = InitialViewExtent();
    }

    private void clearButton_Click(object? sender, EventArgs e)
    {
        _drawingSketch = false;
        ClearLayer();
        ShowInitialDetails();
        ActivatePolygonTool();
        statusLabel.Text = "Polygon cleared.";
    }

    private void Viewer_MapMouseDown(object? sender, GeoKernelMapMouseEventArgs e)
    {
        const int leftButton = 1;
        if ((e.ButtonOrButtons & leftButton) == 0 ||
            e.Tool != GeoKernelViewerTool.AddPolygon ||
            _drawingSketch)
            return;

        if (geoKernelViewerControl.GetLayerFeatureCount(_polygonLayerIndex) > 0)
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
            statusLabel.Text = "Editable polygon layer is not in the viewer.";
            return;
        }

        if (!geoKernelViewerControl.IsLayerEditing(_polygonLayerIndex) &&
            !geoKernelViewerControl.BeginEditLayer(_polygonLayerIndex))
        {
            statusLabel.Text = "Polygon layer could not enter edit mode.";
            return;
        }

        if (!geoKernelViewerControl.SetActiveEditLayerIndex(_polygonLayerIndex))
        {
            statusLabel.Text = "Polygon layer could not be activated.";
            return;
        }

        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPolygon;
        statusLabel.Text = "Add Polygon active. Finish with Enter or double-click.";
    }

    private void ClearLayer()
    {
        if (_polygonLayerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_polygonLayerIndex);
        geoKernelViewerControl.BeginEditLayer(_polygonLayerIndex);
        RefreshMap();
    }

    private void WriteGeoJson()
    {
        var wkt = geoKernelViewerControl.WriteLayerLastShapeWkt(_polygonLayerIndex);
        if (string.IsNullOrWhiteSpace(wkt))
        {
            ShowEmptyDetails();
            return;
        }

        var rings = geoKernelViewerControl.ReadWktPolygon(wkt);
        var geoJson = geoKernelViewerControl.WriteGeoJsonPolygon(rings);
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
        statusLabel.Text = "GisGeoJsonWriter::writePolygonString wrote polygon GeoJSON.";
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
        geoKernelViewerControl.InvalidateRenderCache(true, true);
        geoKernelViewerControl.RefreshLayers();
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
