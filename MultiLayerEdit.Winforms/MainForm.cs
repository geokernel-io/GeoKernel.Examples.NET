using GeoKernel.NET.WinForms;

namespace GeoKernel.MultiLayerEdit.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string RedLayerName = "Red Points";
    private const string BlueLayerName = "Blue Points";

    private int _redLayerIndex = -1;
    private int _blueLayerIndex = -1;
    private int _redCursor;
    private int _blueCursor;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        _worldSamplePath = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(_worldSamplePath)) return;        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreateEditLayers();
        BeginBothLayers();
        SetActiveLayer(_redLayerIndex);
        SetSampleExtent();
        UpdateUi("Switch active edit layer, then add points to that layer.");
    }

    private bool LoadLayer()
    {
        var path = _worldSamplePath;
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        geoKernelViewerControl.SetLayerName(0, "World");
        return true;
    }

    private void CreateEditLayers()
    {
        _redLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(RedLayerName, GeoKernelShapeType.Point, PointStyle("#D95D39", "#8C321D"));

        _blueLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(BlueLayerName, GeoKernelShapeType.Point, PointStyle("#2563EB", "#1E3A8A"));

        // Adding the second layer shifts the first layer's index. Resolve both only
        // after both layers are in the viewer, matching the Qt sample's pointer lookup.
        _redLayerIndex = geoKernelViewerControl.GetLayerInfoByName(RedLayerName)?.Index ?? _redLayerIndex;
        _blueLayerIndex = geoKernelViewerControl.GetLayerInfoByName(BlueLayerName)?.Index ?? _blueLayerIndex;
    }

    private void BeginBothLayers()
    {
        BeginLayer(_redLayerIndex);
        BeginLayer(_blueLayerIndex);
    }

    private void BeginLayer(int layerIndex)
    {
        if (layerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(layerIndex))
            geoKernelViewerControl.BeginEditLayer(layerIndex);
    }

    private void SetActiveLayer(int layerIndex)
    {
        BeginBothLayers();
        if (layerIndex >= 0)
            geoKernelViewerControl.SetActiveEditLayerIndex(layerIndex);

        redLayerButton.Checked = layerIndex == _redLayerIndex;
        blueLayerButton.Checked = layerIndex == _blueLayerIndex;
        UpdateUi($"SetActiveEditLayerIndex({layerIndex})");
    }

    private void redLayerButton_Click(object? sender, EventArgs e)
    {
        SetActiveLayer(_redLayerIndex);
    }

    private void blueLayerButton_Click(object? sender, EventArgs e)
    {
        SetActiveLayer(_blueLayerIndex);
    }

    private void addButton_Click(object? sender, EventArgs e)
    {
        BeginBothLayers();

        var activeIndex = geoKernelViewerControl.ActiveEditLayerIndex;
        if (activeIndex != _redLayerIndex && activeIndex != _blueLayerIndex)
        {
            UpdateUi("No active edit layer.");
            return;
        }

        var redActive = activeIndex == _redLayerIndex;
        var point = redActive ? RedPointAt(_redCursor) : BluePointAt(_blueCursor);
        var nextNumber = redActive ? _redCursor + 1 : _blueCursor + 1;
        var layerName = redActive ? RedLayerName : BlueLayerName;
        var attributes = new Dictionary<string, object?>
        {
            ["Name"] = $"{layerName} {nextNumber}",
            ["Layer"] = layerName
        };

        if (!geoKernelViewerControl.AddPointToEditLayer(activeIndex, point.X, point.Y, attributes))
        {
            UpdateUi($"AddPointToEditLayer({activeIndex}, ...) failed.");
            return;
        }

        if (redActive)
            ++_redCursor;
        else
            ++_blueCursor;

        RefreshMap();
        UpdateUi($"Added point to active layer: {layerName}.");
    }

    private void commitButton_Click(object? sender, EventArgs e)
    {
        CommitIfEditing(_redLayerIndex);
        CommitIfEditing(_blueLayerIndex);
        BeginBothLayers();
        SetActiveLayer(redLayerButton.Checked ? _redLayerIndex : _blueLayerIndex);
        RefreshMap();
        UpdateUi("Both edit layers committed and reopened for editing.");
    }

    private void rollbackButton_Click(object? sender, EventArgs e)
    {
        ResetLayers("Both edit layers rolled back.");
    }

    private void resetButton_Click(object? sender, EventArgs e)
    {
        ResetLayers("Both edit layers reset. Red Points is active.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _redLayerIndex && e.LayerIndex != _blueLayerIndex)
            return;

        UpdateUi("Layer edit state changed.");
    }

    private void CommitIfEditing(int layerIndex)
    {
        if (layerIndex >= 0 && geoKernelViewerControl.IsLayerEditing(layerIndex))
            geoKernelViewerControl.CommitEditLayer(layerIndex);
    }

    private void RollbackIfEditing(int layerIndex)
    {
        if (layerIndex >= 0 && geoKernelViewerControl.IsLayerEditing(layerIndex))
            geoKernelViewerControl.RollbackEditLayer(layerIndex);
    }

    private void ResetLayers(string message)
    {
        RollbackIfEditing(_redLayerIndex);
        RollbackIfEditing(_blueLayerIndex);
        geoKernelViewerControl.RemoveLayerByName(RedLayerName);
        geoKernelViewerControl.RemoveLayerByName(BlueLayerName);
        _redCursor = 0;
        _blueCursor = 0;
        _redLayerIndex = -1;
        _blueLayerIndex = -1;
        CreateEditLayers();
        BeginBothLayers();
        SetActiveLayer(_redLayerIndex);
        RefreshMap();
        UpdateUi(message);
    }

    private void UpdateUi(string message)
    {
        var activeIndex = geoKernelViewerControl.ActiveEditLayerIndex;
        var activeName = activeIndex == _redLayerIndex ? RedLayerName : activeIndex == _blueLayerIndex ? BlueLayerName : "-";
        var redCount = _redLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_redLayerIndex) : 0;
        var blueCount = _blueLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_blueLayerIndex) : 0;

        stateLabel.Text = $"Active edit layer: {activeName} ({activeIndex}) | Red: {redCount} | Blue: {blueCount}";
        infoTextBox.Text = string.Join(Environment.NewLine,
            "MultiLayerEdit sample",
            "",
            "Workflow:",
            "1. Red Points and Blue Points are both editing.",
            "2. Active layer buttons call SetActiveEditLayerIndex(index).",
            "3. Add To Active Layer writes to the current active edit layer index.",
            "4. Commit Both commits both edit sessions and reopens them.",
            "5. Rollback Both discards uncommitted additions.",
            "",
            $"ActiveEditLayerIndex: {activeIndex}",
            $"Active layer: {activeName}",
            $"Red layer index: {_redLayerIndex}",
            $"Blue layer index: {_blueLayerIndex}",
            $"Red feature count: {redCount}",
            $"Blue feature count: {blueCount}",
            "",
            "APIs:",
            "BeginEditLayer(index)",
            "SetActiveEditLayerIndex(index)",
            "ActiveEditLayerIndex",
            "AddPointToEditLayer(activeIndex, x, y, attributes)",
            "CommitEditLayer(index)",
            "RollbackEditLayer(index)");
        statusLabel.Text = message;
    }

    private void RefreshMap()
    {
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 20.0, -65.0, 55.0);
    }

    private static GeoKernelPoint RedPointAt(int index)
    {
        const double xMin = -124.0;
        const double yMin = 31.0;
        const double xStep = 7.5;
        const double yStep = 5.0;
        const int columns = 7;
        return new GeoKernelPoint(xMin + index % columns * xStep, yMin + index / columns * yStep);
    }

    private static GeoKernelPoint BluePointAt(int index)
    {
        const double xMin = -121.5;
        const double yMin = 33.0;
        const double xStep = 7.5;
        const double yStep = 5.0;
        const int columns = 7;
        return new GeoKernelPoint(xMin + index % columns * xStep, yMin + index / columns * yStep);
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

    private static GeoKernelLayerStyle PointStyle(string pointColor, string lineColor)
    {
        return new GeoKernelLayerStyle
        {
            PointColor = pointColor,
            LineColor = lineColor,
            PointSize = 11.0,
            LineWidth = 1.3,
            SelectedLineColor = "#F59E0B",
            SelectedLineWidth = 4.0,
            ShowLabels = true,
            LabelField = "Name",
            LabelFontSize = 10.0,
            LabelColor = "#263238",
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
