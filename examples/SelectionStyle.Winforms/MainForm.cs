using GeoKernel.NET.WinForms;

namespace GeoKernel.SelectionStyle.Winforms;

public sealed partial class MainForm : Form
{
    private readonly Color _defaultSelectedLineColor = Color.FromArgb(245, 158, 11);

    private Color _selectedLineColor;
    private int _polygonLayerIndex = -1;
    private int _lineLayerIndex = -1;
    private int _pointLayerIndex = -1;
    private bool _loading;

    public MainForm()
    {
        InitializeComponent();
        _selectedLineColor = _defaultSelectedLineColor;
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = Color.FromArgb(247, 248, 250);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Select;
        geoKernelViewerControl.SelectionChanged += geoKernelViewerControl_SelectionChanged;

        _loading = true;
        try
        {
            selectedLineWidthNumeric.Value = 4.0m;
            UpdateColorSwatch();
            CreateSampleLayers();
            ApplySelectionStyle();
            geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-15.0, -9.0, 15.0, 11.0);
        }
        finally
        {
            _loading = false;
        }

        UpdateStatus();
    }

    private void CreateSampleLayers()
    {
        _polygonLayerIndex = geoKernelViewerControl.AddPolygonLayer(
            "Selectable Polygons",
            [PolygonA(), PolygonB()],
            PolygonStyle());

        _lineLayerIndex = geoKernelViewerControl.AddPolylineLayer(
            "Selectable Polyline",
            SampleLine(),
            LineStyle());

        _pointLayerIndex = geoKernelViewerControl.AddPointLayer(
            "Selectable Points",
            SamplePoints(),
            PointStyle());

        _polygonLayerIndex = LayerIndexByName("Selectable Polygons");
        _lineLayerIndex = LayerIndexByName("Selectable Polyline");
        _pointLayerIndex = LayerIndexByName("Selectable Points");
    }

    private void ApplySelectionStyle()
    {
        if (_polygonLayerIndex >= 0)
            geoKernelViewerControl.SetLayerStyle(_polygonLayerIndex, PolygonStyle());

        if (_lineLayerIndex >= 0)
            geoKernelViewerControl.SetLayerStyle(_lineLayerIndex, LineStyle());

        if (_pointLayerIndex >= 0)
            geoKernelViewerControl.SetLayerStyle(_pointLayerIndex, PointStyle());

        geoKernelViewerControl.RefreshLayers();
        UpdateStatus();
    }

    private GeoKernelLayerStyle PolygonStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#F1D58A",
            FillOpacity = 180,
            LineColor = "#266D8F",
            LineWidth = 1.8,
            SelectedLineColor = ColorToHex(_selectedLineColor),
            SelectedLineWidth = SelectedLineWidth()
        };
    }

    private GeoKernelLayerStyle LineStyle()
    {
        return new GeoKernelLayerStyle
        {
            LineColor = "#266D8F",
            LineWidth = 2.2,
            SelectedLineColor = ColorToHex(_selectedLineColor),
            SelectedLineWidth = SelectedLineWidth()
        };
    }

    private GeoKernelLayerStyle PointStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95F35",
            PointSize = 10.0,
            SelectedLineColor = ColorToHex(_selectedLineColor),
            SelectedLineWidth = SelectedLineWidth()
        };
    }

    private double SelectedLineWidth()
    {
        return (double)selectedLineWidthNumeric.Value;
    }

    private int LayerIndexByName(string name)
    {
        return geoKernelViewerControl.GetLayerInfoByName(name)?.Index ?? -1;
    }

    private void UpdateColorSwatch()
    {
        selectedLineColorButton.BackColor = _selectedLineColor;
    }

    private void UpdateStatus()
    {
        statusLabel.Text = $"Tool: Select | Selected: {geoKernelViewerControl.SelectedFeatureCount} | selectedLineColor={ColorToHex(_selectedLineColor)} | selectedLineWidth={SelectedLineWidth():0.0}";
    }

    private void selectedLineColorButton_Click(object sender, EventArgs e)
    {
        colorDialog.Color = _selectedLineColor;
        if (colorDialog.ShowDialog(this) != DialogResult.OK)
            return;

        _selectedLineColor = colorDialog.Color;
        UpdateColorSwatch();
        ApplySelectionStyle();
    }

    private void selectedLineWidthNumeric_ValueChanged(object sender, EventArgs e)
    {
        if (!_loading)
            ApplySelectionStyle();
    }

    private void clearSelectionButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ClearSelectedFeatures();
        UpdateStatus();
    }

    private void resetStyleButton_Click(object sender, EventArgs e)
    {
        _loading = true;
        try
        {
            _selectedLineColor = _defaultSelectedLineColor;
            selectedLineWidthNumeric.Value = 4.0m;
            UpdateColorSwatch();
        }
        finally
        {
            _loading = false;
        }

        ApplySelectionStyle();
    }

    private void geoKernelViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        UpdateStatus();
    }

    private static IReadOnlyList<GeoKernelPoint> PolygonA()
    {
        return
        [
            new GeoKernelPoint(-11.0, -4.0),
            new GeoKernelPoint(-4.0, -4.0),
            new GeoKernelPoint(-3.0, 2.0),
            new GeoKernelPoint(-8.0, 5.0),
            new GeoKernelPoint(-12.0, 1.0),
            new GeoKernelPoint(-11.0, -4.0)
        ];
    }

    private static IReadOnlyList<GeoKernelPoint> PolygonB()
    {
        return
        [
            new GeoKernelPoint(2.0, -4.0),
            new GeoKernelPoint(10.0, -4.0),
            new GeoKernelPoint(12.0, 2.0),
            new GeoKernelPoint(6.0, 5.0),
            new GeoKernelPoint(1.0, 1.0),
            new GeoKernelPoint(2.0, -4.0)
        ];
    }

    private static IReadOnlyList<GeoKernelPoint> SampleLine()
    {
        return
        [
            new GeoKernelPoint(-12.0, -7.0),
            new GeoKernelPoint(-6.0, -1.0),
            new GeoKernelPoint(0.0, -5.5),
            new GeoKernelPoint(6.0, -0.5),
            new GeoKernelPoint(13.0, -5.0)
        ];
    }

    private static IReadOnlyList<GeoKernelPoint> SamplePoints()
    {
        return
        [
            new GeoKernelPoint(-8.0, 8.0),
            new GeoKernelPoint(0.0, 7.0),
            new GeoKernelPoint(8.0, 8.0)
        ];
    }

    private static string ColorToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
