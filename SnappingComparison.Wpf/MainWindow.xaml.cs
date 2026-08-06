using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;
using System.IO;
using System.Windows;

namespace GeoKernel.SnappingComparison.Wpf;

public partial class MainWindow
{
    private const string LineLayerName = "Snapping Lines";
    private int _lineLayerIndex = -1;
    private bool _addLineMode = true;

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        viewerControl.EditSnappingEnabled = true;
        viewerControl.EditSnappingTolerancePixels = 14;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;
        if (!LoadWorld()) return;
        CreateLineLayer();
        ResetGuideLine();
        SetSampleExtent();
        UpdateStatus("Draw with Snapping ON, then switch it OFF and repeat to compare the result.");
    }

    private bool LoadWorld()
    {
        var path = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "SnappingComparison");
            return false;
        }
        return viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() });
    }

    private void CreateLineLayer()
    {
        _lineLayerIndex = viewerControl.AddEmptyVectorLayer(LineLayerName, GeoKernelShapeType.Polyline, LineStyle());
        _lineLayerIndex = viewerControl.GetLayerInfoByName(LineLayerName)?.Index ?? _lineLayerIndex;
    }

    private void BeginLineEditing()
    {
        if (_lineLayerIndex < 0) return;
        if (!viewerControl.IsLayerEditing(_lineLayerIndex)) viewerControl.BeginEditLayer(_lineLayerIndex);
        viewerControl.SetActiveEditLayerIndex(_lineLayerIndex);
    }

    private void ResetGuideLine()
    {
        if (_lineLayerIndex < 0) return;
        viewerControl.RollbackEditLayer(_lineLayerIndex);
        BeginLineEditing();
        viewerControl.AddPolylineToEditLayer(_lineLayerIndex, GuideLine(), new Dictionary<string, object?> { ["Name"] = "Guide line", ["Kind"] = "Snap target" });
        _addLineMode = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        addLineButton.IsChecked = true;
        panButton.IsChecked = false;
        RefreshMap();
        UpdateStatus("Guide line reset. Draw near its vertices/segments to test snapping.");
    }

    private void AddLine_Click(object sender, RoutedEventArgs e)
    {
        _addLineMode = true; addLineButton.IsChecked = true; panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline; BeginLineEditing();
        UpdateStatus("Add Polyline active. Click vertices, then Enter or double-click to finish.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        _addLineMode = false; addLineButton.IsChecked = false; panButton.IsChecked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan; UpdateStatus("Pan active.");
    }

    private void Snapping_Click(object sender, RoutedEventArgs e)
    {
        var enabled = snappingButton.IsChecked == true;
        viewerControl.EditSnappingEnabled = enabled;
        snappingButton.Content = enabled ? "Snapping ON" : "Snapping OFF";
        UpdateStatus(enabled ? "Snapping enabled." : "Snapping disabled.");
    }

    private void ToleranceSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (!IsLoaded) return;
        var tolerance = Math.Round(toleranceSlider.Value);
        viewerControl.EditSnappingTolerancePixels = tolerance;
        toleranceText.Text = tolerance.ToString("0");
        UpdateStatus($"EditSnappingTolerancePixels = {tolerance:0}");
    }

    private void ResetGuide_Click(object sender, RoutedEventArgs e) => ResetGuideLine();
    private void FullExtent_Click(object sender, RoutedEventArgs e) => SetSampleExtent();

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _lineLayerIndex) return;
        RefreshMap();
        UpdateStatus(_addLineMode ? "Line layer updated. Keep drawing with snapping." : "Line layer updated.");
    }

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private void SetSampleExtent() => viewerControl.ViewExtent = new GeoKernelExtent(-132, 18, -60, 55);

    private void UpdateStatus(string message)
    {
        var featureCount = _lineLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_lineLayerIndex) : 0;
        var drawn = Math.Max(0, featureCount - 1);
        lineCountText.Text = $"Drawn lines: {drawn}";
        infoTextBox.Text = string.Join(Environment.NewLine,
            "Snapping APIs:", "- EditSnappingEnabled", "- EditSnappingTolerancePixels", "",
            "How to test:", "- Add Polyline is active.", "- Draw near the existing guide line vertices/segments.",
            "- Toggle Snapping off and draw again.", "- Increase/decrease tolerance and compare.",
            "- Finish a line with Enter or double-click.", "",
            $"Snapping enabled: {viewerControl.EditSnappingEnabled}",
            $"Tolerance: {viewerControl.EditSnappingTolerancePixels:0} px", $"Line feature count: {featureCount}", $"User-drawn lines: {drawn}");
        statusText.Text = message;
    }

    private static GeoKernelPoint[] GuideLine() =>
    [
        new(-123, 31), new(-116, 42), new(-106, 34), new(-96, 43), new(-86, 35), new(-76, 41)
    ];

    private static GeoKernelLayerStyle WorldStyle() => new() { FillColor = "#D8E5E1", FillOpacity = 210, LineColor = "#6F8883", LineWidth = 0.7 };
    private static GeoKernelLayerStyle LineStyle() => new()
    {
        LineColor = "#2B6F8E", LineWidth = 3, SelectedLineColor = "#F59E0B", SelectedLineWidth = 5,
        ShowLabels = true, LabelField = "Name", LabelFontSize = 10, LabelHaloEnabled = true,
        LabelHaloColor = "#FFFFFF", LabelHaloWidth = 2, LabelOffsetY = -12, LabelAllowOverlap = true
    };
}
