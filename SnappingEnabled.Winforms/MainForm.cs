using GeoKernel.NET.WinForms;

namespace GeoKernel.SnappingEnabled.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string LineLayerName = "Snapping Lines";
    private int _lineLayerIndex = -1;
    private bool _addLineMode = true;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        _worldSamplePath = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(_worldSamplePath)) return;        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        geoKernelViewerControl.EditSnappingEnabled = true;
        geoKernelViewerControl.EditSnappingTolerancePixels = 14.0;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreateLineLayer();
        ResetGuideLine();
        SetSampleExtent();
        UpdateStatus("Add Polyline active. Draw near the guide line to test snapping.");
    }

    private bool LoadLayer()
    {
        var path = _worldSamplePath;
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "SnappingEnabled");
            return false;
        }

        return geoKernelViewerControl.AddLayerFile(
            path,
            new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = WorldStyle()
            });
    }

    private void CreateLineLayer()
    {
        _lineLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(
            LineLayerName,
            GeoKernelShapeType.Polyline,
            LineStyle());

        _lineLayerIndex = geoKernelViewerControl.GetLayerInfoByName(LineLayerName)?.Index ?? _lineLayerIndex;
    }

    private void BeginLineEditing()
    {
        if (_lineLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_lineLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_lineLayerIndex);

        geoKernelViewerControl.SetActiveEditLayerIndex(_lineLayerIndex);
    }

    private void ResetGuideLine()
    {
        if (_lineLayerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_lineLayerIndex);
        BeginLineEditing();

        geoKernelViewerControl.AddPolylineToEditLayer(
            _lineLayerIndex,
            GuideLine(),
            new Dictionary<string, object?>
            {
                ["Name"] = "Guide line",
                ["Kind"] = "Snap target"
            });

        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        _addLineMode = true;
        addLineButton.Checked = true;
        panButton.Checked = false;
        RefreshMap();
        UpdateStatus("Guide line reset. Draw near its vertices/segments to test snapping.");
    }

    private void addLineButton_Click(object sender, EventArgs e)
    {
        _addLineMode = true;
        addLineButton.Checked = true;
        panButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        BeginLineEditing();
        UpdateStatus("Add Polyline active. Click vertices, then Enter or double-click to finish.");
    }

    private void panButton_Click(object sender, EventArgs e)
    {
        _addLineMode = false;
        addLineButton.Checked = false;
        panButton.Checked = true;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan active.");
    }

    private void snappingButton_CheckedChanged(object sender, EventArgs e)
    {
        geoKernelViewerControl.EditSnappingEnabled = snappingButton.Checked;
        snappingButton.Text = snappingButton.Checked ? "Snapping ON" : "Snapping OFF";
        UpdateStatus(snappingButton.Checked ? "Snapping enabled." : "Snapping disabled.");
    }

    private void toleranceNumeric_ValueChanged(object sender, EventArgs e)
    {
        geoKernelViewerControl.EditSnappingTolerancePixels = (double)toleranceNumeric.Value;
        UpdateStatus($"editSnappingTolerancePixels = {toleranceNumeric.Value:0}");
    }

    private void resetGuideButton_Click(object sender, EventArgs e)
    {
        ResetGuideLine();
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _lineLayerIndex)
            return;

        RefreshMap();
        UpdateStatus(_addLineMode ? "Line layer updated. Keep drawing with snapping." : "Line layer updated.");
    }

    private void RefreshMap()
    {
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-132.0, 18.0, -60.0, 55.0);
    }

    private void UpdateStatus(string message)
    {
        var featureCount = _lineLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_lineLayerIndex) : 0;
        var drawnLineCount = Math.Max(0, featureCount - 1);
        lineCountLabel.Text = $"Drawn lines: {drawnLineCount}";
        infoTextBox.Text = StateText(featureCount, drawnLineCount);
        statusLabel.Text = message;
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
            $"Snapping enabled: {geoKernelViewerControl.EditSnappingEnabled}",
            $"Tolerance: {geoKernelViewerControl.EditSnappingTolerancePixels:0} px",
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

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, p =>
    { statusLabel.Text = p.Message; downloadProgressBar.Visible = true; downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee; if (p.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(p.Percentage.Value, 0, 100); });

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
