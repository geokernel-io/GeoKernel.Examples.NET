using GeoKernel.NET.WinForms;

namespace GeoKernel.SimpleStyle.Winforms;

public sealed partial class MainForm : Form
{
    private readonly Color _defaultFillColor = Color.FromArgb(241, 213, 138);
    private readonly Color _defaultLineColor = Color.FromArgb(38, 109, 143);

    private Color _fillColor;
    private Color _lineColor;
    private int _polygonLayerIndex = -1;
    private int _lineLayerIndex = -1;
    private int _pointLayerIndex = -1;
    private bool _loading;

    public MainForm()
    {
        InitializeComponent();
        _fillColor = _defaultFillColor;
        _lineColor = _defaultLineColor;
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = Color.FromArgb(247, 248, 250);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        _loading = true;
        try
        {
            lineWidthNumeric.Value = 2.0m;
            pointSizeNumeric.Value = 10.0m;
            UpdateColorSwatches();
            CreateSampleLayers();
            ApplyStyle();
            geoKernelViewerControl.ViewExtent = InitialViewExtent();
        }
        finally
        {
            _loading = false;
        }
    }

    private void CreateSampleLayers()
    {
        _polygonLayerIndex = geoKernelViewerControl.AddPolygonLayer(
            "Styled Polygon", SamplePolygon(),
            PolygonStyle());

        _lineLayerIndex = geoKernelViewerControl.AddPolylineLayer(
            "Styled Polyline",
            SamplePolyline(),
            LineStyle());

        _pointLayerIndex = geoKernelViewerControl.AddPointLayer(
            "Styled Points",
            SamplePoints(),
            PointStyle());

        _polygonLayerIndex = LayerIndexByName("Styled Polygon");
        _lineLayerIndex = LayerIndexByName("Styled Polyline");
        _pointLayerIndex = LayerIndexByName("Styled Points");
    }

    private void ApplyStyle()
    {
        if (_polygonLayerIndex >= 0)
            geoKernelViewerControl.SetLayerStyle(_polygonLayerIndex, PolygonStyle());

        if (_lineLayerIndex >= 0)
            geoKernelViewerControl.SetLayerStyle(_lineLayerIndex, LineStyle());

        if (_pointLayerIndex >= 0)
            geoKernelViewerControl.SetLayerStyle(_pointLayerIndex, PointStyle());

        geoKernelViewerControl.RefreshLayers();
    }

    private GeoKernelLayerStyle PolygonStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = ColorToHex(_fillColor),
            FillOpacity = 185,
            LineColor = ColorToHex(_lineColor),
            LineWidth = (double)lineWidthNumeric.Value
        };
    }

    private GeoKernelLayerStyle LineStyle()
    {
        return new GeoKernelLayerStyle
        {
            LineColor = ColorToHex(_lineColor),
            LineWidth = (double)lineWidthNumeric.Value
        };
    }

    private GeoKernelLayerStyle PointStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95F35",
            PointSize = (double)pointSizeNumeric.Value
        };
    }

    private int LayerIndexByName(string name)
    {
        return geoKernelViewerControl.GetLayerInfoByName(name)?.Index ?? -1;
    }

    private void UpdateColorSwatches()
    {
        fillColorButton.BackColor = _fillColor;
        lineColorButton.BackColor = _lineColor;
    }

    private void fillColorButton_Click(object sender, EventArgs e)
    {
        colorDialog.Color = _fillColor;
        if (colorDialog.ShowDialog(this) != DialogResult.OK)
            return;

        _fillColor = colorDialog.Color;
        UpdateColorSwatches();
        ApplyStyle();
    }

    private void lineColorButton_Click(object sender, EventArgs e)
    {
        colorDialog.Color = _lineColor;
        if (colorDialog.ShowDialog(this) != DialogResult.OK)
            return;

        _lineColor = colorDialog.Color;
        UpdateColorSwatches();
        ApplyStyle();
    }

    private void styleNumeric_ValueChanged(object sender, EventArgs e)
    {
        if (!_loading)
            ApplyStyle();
    }

    private void resetStyleButton_Click(object sender, EventArgs e)
    {
        _loading = true;
        try
        {
            _fillColor = _defaultFillColor;
            _lineColor = _defaultLineColor;
            lineWidthNumeric.Value = 2.0m;
            pointSizeNumeric.Value = 10.0m;
            UpdateColorSwatches();
        }
        finally
        {
            _loading = false;
        }

        ApplyStyle();
    }

    private static IReadOnlyList<GeoKernelPoint> SamplePolygon()
    {
        return
        [
            new GeoKernelPoint(-8.0, -3.0),
            new GeoKernelPoint(1.0, -3.0),
            new GeoKernelPoint(3.0, 4.0),
            new GeoKernelPoint(-6.0, 6.0),
            new GeoKernelPoint(-10.0, 2.0),
            new GeoKernelPoint(-8.0, -3.0)
        ];
    }

    private static IReadOnlyList<GeoKernelPoint> SamplePolyline()
    {
        return
        [
            new GeoKernelPoint(-12.0, -7.0),
            new GeoKernelPoint(-5.0, -1.0),
            new GeoKernelPoint(1.0, -5.0),
            new GeoKernelPoint(8.0, 2.0),
            new GeoKernelPoint(13.0, -2.0)
        ];
    }

    private static IReadOnlyList<GeoKernelPoint> SamplePoints()
    {
        return
        [
            new GeoKernelPoint(-6.0, 9.0),
            new GeoKernelPoint(0.0, 8.0),
            new GeoKernelPoint(7.0, 7.0)
        ];
    }

    private static string ColorToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }

    private static GeoKernelExtent InitialViewExtent()
    {
        return new GeoKernelExtent(-19.5, -14.2, 20.5, 18.9);
    }
}
