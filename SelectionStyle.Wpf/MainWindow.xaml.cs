using System.Globalization;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.SelectionStyle.Wpf;

public partial class MainWindow : Window
{
    private readonly Color _defaultSelectedLineColor = Color.FromArgb(245, 158, 11);

    private Color _selectedLineColor;
    private int _polygonLayerIndex = -1;
    private int _lineLayerIndex = -1;
    private int _pointLayerIndex = -1;
    private bool _loading = true;

    public MainWindow()
    {
        InitializeComponent();
        _selectedLineColor = _defaultSelectedLineColor;
        _loading = false;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Select;
        viewerControl.SelectionChanged += ViewerControl_SelectionChanged;

        _loading = true;
        try
        {
            selectedLineWidthTextBox.Text = "4.0";
            UpdateColorSwatch();
            CreateSampleLayers();
            ApplySelectionStyle();
            viewerControl.ViewExtent = new GeoKernelExtent(-15.0, -9.0, 15.0, 11.0);
        }
        finally
        {
            _loading = false;
        }

        UpdateStatus();
    }

    private void CreateSampleLayers()
    {
        _polygonLayerIndex = viewerControl.AddPolygonLayer(
            "Selectable Polygons",
            [PolygonA(), PolygonB()],
            PolygonStyle());

        _lineLayerIndex = viewerControl.AddPolylineLayer(
            "Selectable Polyline",
            SampleLine(),
            LineStyle());

        _pointLayerIndex = viewerControl.AddPointLayer(
            "Selectable Points",
            SamplePoints(),
            PointStyle());

        _polygonLayerIndex = LayerIndexByName("Selectable Polygons");
        _lineLayerIndex = LayerIndexByName("Selectable Polyline");
        _pointLayerIndex = LayerIndexByName("Selectable Points");
    }

    private void ApplySelectionStyle()
    {
        if (_loading || _polygonLayerIndex < 0)
            return;

        if (_polygonLayerIndex >= 0)
            viewerControl.SetLayerStyle(_polygonLayerIndex, PolygonStyle());

        if (_lineLayerIndex >= 0)
            viewerControl.SetLayerStyle(_lineLayerIndex, LineStyle());

        if (_pointLayerIndex >= 0)
            viewerControl.SetLayerStyle(_pointLayerIndex, PointStyle());

        viewerControl.RefreshLayers();
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

    private int LayerIndexByName(string name)
    {
        return viewerControl.GetLayerInfoByName(name)?.Index ?? -1;
    }

    private double SelectedLineWidth()
    {
        return ReadDouble(selectedLineWidthTextBox.Text, 4.0, 1.0, 16.0);
    }

    private void UpdateColorSwatch()
    {
        selectedLineColorButton.Background = BrushFromColor(_selectedLineColor);
    }

    private void UpdateStatus()
    {
        statusText.Text = $"Tool: Select | Selected: {viewerControl.SelectedFeatureCount} | selectedLineColor={ColorToHex(_selectedLineColor)} | selectedLineWidth={SelectedLineWidth():0.0}";
    }

    private void SelectedLineColorButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryPickColor(_selectedLineColor, out var color))
            return;

        _selectedLineColor = color;
        UpdateColorSwatch();
        ApplySelectionStyle();
    }

    private void SelectedLineWidthTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (IsLoaded && !_loading)
            ApplySelectionStyle();
    }

    private void ClearSelectionButton_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ClearSelectedFeatures();
        UpdateStatus();
    }

    private void ResetStyleButton_Click(object sender, RoutedEventArgs e)
    {
        _loading = true;
        try
        {
            _selectedLineColor = _defaultSelectedLineColor;
            selectedLineWidthTextBox.Text = "4.0";
            UpdateColorSwatch();
        }
        finally
        {
            _loading = false;
        }

        ApplySelectionStyle();
    }

    private void ViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        UpdateStatus();
    }

    private static double ReadDouble(string text, double fallback, double minimum, double maximum)
    {
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return fallback;

        if (!double.IsFinite(value))
            return fallback;

        return Math.Clamp(value, minimum, maximum);
    }

    private static bool TryPickColor(Color current, out Color color)
    {
        using var dialog = new ColorDialog
        {
            Color = current,
            FullOpen = true
        };

        if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        {
            color = dialog.Color;
            return true;
        }

        color = current;
        return false;
    }

    private static System.Windows.Media.SolidColorBrush BrushFromColor(Color color)
    {
        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(color.R, color.G, color.B));
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
