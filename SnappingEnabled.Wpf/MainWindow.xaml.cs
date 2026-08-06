using GeoKernel.Examples.Common;
using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.SnappingEnabled.Wpf;

public partial class MainWindow
{
    private const string LineLayerName = "Snapping Lines";
    private int _lineLayerIndex = -1;
    private bool _addLineMode = true;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        viewerControl.EditSnappingEnabled = true;
        viewerControl.EditSnappingTolerancePixels = 14.0;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreateLineLayer();
        ResetGuideLine();
        SetSampleExtent();
        UpdateStatus("Add Polyline active. Draw near the guide line to test snapping.");
    }

    private bool LoadLayer()
    {
        var path = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "SnappingEnabled");
            return false;
        }

        return viewerControl.AddLayerFile(
            path,
            new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = WorldStyle()
            });
    }

    private void CreateLineLayer()
    {
        _lineLayerIndex = viewerControl.AddEmptyVectorLayer(
            LineLayerName,
            GeoKernelShapeType.Polyline,
            LineStyle());

        _lineLayerIndex = viewerControl.GetLayerInfoByName(LineLayerName)?.Index ?? _lineLayerIndex;
    }

    private void BeginLineEditing()
    {
        if (_lineLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_lineLayerIndex))
            viewerControl.BeginEditLayer(_lineLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_lineLayerIndex);
    }

    private void ResetGuideLine()
    {
        if (_lineLayerIndex < 0)
            return;

        viewerControl.RollbackEditLayer(_lineLayerIndex);
        BeginLineEditing();

        viewerControl.AddPolylineToEditLayer(
            _lineLayerIndex,
            GuideLine(),
            new Dictionary<string, object?>
            {
                ["Name"] = "Guide line",
                ["Kind"] = "Snap target"
            });

        viewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        _addLineMode = true;
        addLineButton.IsChecked = true;
        panButton.IsChecked = false;
        RefreshMap();
        UpdateStatus("Guide line reset. Draw near its vertices/segments to test snapping.");
    }

    private void AddLine_Click(object sender, RoutedEventArgs e)
    {
        _addLineMode = true;
        addLineButton.IsChecked = true;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        BeginLineEditing();
        UpdateStatus("Add Polyline active. Click vertices, then Enter or double-click to finish.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        _addLineMode = false;
        addLineButton.IsChecked = false;
        panButton.IsChecked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan active.");
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
        if (!IsLoaded)
            return;

        var tolerance = Math.Round(toleranceSlider.Value);
        viewerControl.EditSnappingTolerancePixels = tolerance;
        toleranceText.Text = tolerance.ToString("0");
        UpdateStatus($"editSnappingTolerancePixels = {tolerance:0}");
    }

    private void ResetGuide_Click(object sender, RoutedEventArgs e)
    {
        ResetGuideLine();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _lineLayerIndex)
            return;

        RefreshMap();
        UpdateStatus(_addLineMode ? "Line layer updated. Keep drawing with snapping." : "Line layer updated.");
    }

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-132.0, 18.0, -60.0, 55.0);
    }

    private void UpdateStatus(string message)
    {
        var featureCount = _lineLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_lineLayerIndex) : 0;
        var drawnLineCount = Math.Max(0, featureCount - 1);
        lineCountText.Text = $"Drawn lines: {drawnLineCount}";
        infoTextBox.Text = StateText(featureCount, drawnLineCount);
        statusText.Text = message;
    }

    private string StateText(int featureCount, int drawnLineCount)
    {
        return string.Join(Environment.NewLine,
            "Snapping APIs:",
            "- EditSnappingEnabled",
            "- EditSnappingTolerancePixels",
            "",
            "How to test:",
            "- Add Polyline is active.",
            "- Draw near the existing guide line vertices/segments.",
            "- Toggle Snapping off and draw again.",
            "- Increase/decrease tolerance and compare.",
            "- Finish a line with Enter or double-click.",
            "",
            $"Snapping enabled: {viewerControl.EditSnappingEnabled}",
            $"Tolerance: {viewerControl.EditSnappingTolerancePixels:0} px",
            $"Line feature count: {featureCount}",
            $"User-drawn lines: {drawnLineCount}");
    }

    private static GeoKernelPoint[] GuideLine()
    {
        return
        [
            new GeoKernelPoint(-123.0, 31.0),
            new GeoKernelPoint(-116.0, 42.0),
            new GeoKernelPoint(-106.0, 34.0),
            new GeoKernelPoint(-96.0, 43.0),
            new GeoKernelPoint(-86.0, 35.0),
            new GeoKernelPoint(-76.0, 41.0)
        ];
    }

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 210,
            LineColor = "#6F8883",
            LineWidth = 0.7
        };
    }

    private static GeoKernelLayerStyle LineStyle()
    {
        return new GeoKernelLayerStyle
        {
            LineColor = "#2B6F8E",
            LineWidth = 3.0,
            SelectedLineColor = "#F59E0B",
            SelectedLineWidth = 5.0,
            ShowLabels = true,
            LabelField = "Name",
            LabelFontSize = 10.0,
            LabelHaloEnabled = true,
            LabelHaloColor = "#FFFFFF",
            LabelHaloWidth = 2.0,
            LabelOffsetY = -12.0,
            LabelAllowOverlap = true
        };
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "assets", "data")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
