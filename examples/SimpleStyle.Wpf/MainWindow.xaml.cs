using System.Windows;
using System.Globalization;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.SimpleStyle.Wpf;

public partial class MainWindow : Window
{
    private readonly Color _defaultFillColor = Color.FromArgb(241, 213, 138);
    private readonly Color _defaultLineColor = Color.FromArgb(38, 109, 143);

    private Color _fillColor;
    private Color _lineColor;
    private int _polygonLayerIndex = -1;
    private int _lineLayerIndex = -1;
    private int _pointLayerIndex = -1;
    private bool _loading = true;

    public MainWindow()
    {
        InitializeComponent();
        _fillColor = _defaultFillColor;
        _lineColor = _defaultLineColor;
        _loading = false;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = Color.FromArgb(247, 248, 250);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        _loading = true;

        try
        {
            lineWidthTextBox.Text = "2.0";
            pointSizeTextBox.Text = "10.0";
            UpdateColorSwatches();
            CreateSampleLayers();
            ApplyStyle();
            viewerControl.ViewExtent = InitialViewExtent();
        }
        finally
        {
            _loading = false;
        }
    }

    private void CreateSampleLayers()
    {
        _polygonLayerIndex = viewerControl.AddPolygonLayer(
            "Styled Polygon",
            SamplePolygon(),
            PolygonStyle());

        _lineLayerIndex = viewerControl.AddPolylineLayer(
            "Styled Polyline",
            SamplePolyline(),
            LineStyle());

        _pointLayerIndex = viewerControl.AddPointLayer(
            "Styled Points",
            SamplePoints(),
            PointStyle());

        _polygonLayerIndex = LayerIndexByName("Styled Polygon");
        _lineLayerIndex = LayerIndexByName("Styled Polyline");
        _pointLayerIndex = LayerIndexByName("Styled Points");
    }

    private void ApplyStyle()
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
    }

    private GeoKernelLayerStyle PolygonStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = ColorToHex(_fillColor),
            FillOpacity = 185,
            LineColor = ColorToHex(_lineColor),
            LineWidth = LineWidth()
        };
    }

    private GeoKernelLayerStyle LineStyle()
    {
        return new GeoKernelLayerStyle
        {
            LineColor = ColorToHex(_lineColor),
            LineWidth = LineWidth()
        };
    }

    private GeoKernelLayerStyle PointStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95F35",
            PointSize = PointSize()
        };
    }

    private int LayerIndexByName(string name)
    {
        return viewerControl.GetLayerInfoByName(name)?.Index ?? -1;
    }

    private void UpdateColorSwatches()
    {
        fillColorButton.Background = BrushFromColor(_fillColor);
        lineColorButton.Background = BrushFromColor(_lineColor);
    }

    private void FillColorButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryPickColor(_fillColor, out var color))
            return;

        _fillColor = color;
        UpdateColorSwatches();
        ApplyStyle();
    }

    private void LineColorButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryPickColor(_lineColor, out var color))
            return;

        _lineColor = color;
        UpdateColorSwatches();
        ApplyStyle();
    }

    private void StyleTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (IsLoaded && !_loading)
            ApplyStyle();
    }

    private void ResetStyleButton_Click(object sender, RoutedEventArgs e)
    {
        _loading = true;

        try
        {
            _fillColor = _defaultFillColor;
            _lineColor = _defaultLineColor;
            lineWidthTextBox.Text = "2.0";
            pointSizeTextBox.Text = "10.0";
            UpdateColorSwatches();
        }
        finally
        {
            _loading = false;
        }

        ApplyStyle();
    }

    private double LineWidth()
    {
        return ReadDouble(lineWidthTextBox.Text, 2.0, 0.5, 12.0);
    }

    private double PointSize()
    {
        return ReadDouble(pointSizeTextBox.Text, 10.0, 2.0, 32.0);
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

    private static GeoKernelExtent InitialViewExtent()
    {
        return new GeoKernelExtent(-19.5, -14.2, 20.5, 18.9);
    }

    private static string ColorToHex(Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
    }
}
