using GeoKernel.NET.WinForms;

namespace GeoKernel.AddWithAttributes.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string PointLayerName = "Points With Attributes";

    private readonly List<FeatureRow> _rows = [];
    private int _pointLayerIndex = -1;
    private int _pointCursor;
    private bool _syncingGridSelection;
    private bool _infoMode;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        _worldSamplePath = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(_worldSamplePath)) return;
        ConfigureGrid();

        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.SelectionChanged += geoKernelViewerControl_SelectionChanged;

        if (!LoadLayer())
            return;

        CreatePointLayer();
        BeginPointEditing();
        SetSampleExtent();
        UpdateInfoText("Click Add Point With Attributes, then use Info and click an added point.");
        UpdateStatus("AddPointToEditLayer(index, x, y, attributes) sample.");
    }

    private bool LoadLayer()
    {
        var path = _worldSamplePath;
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:\n{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = WorldStyle()
                }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:\n{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var worldLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            geoKernelViewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreatePointLayer()
    {
        _pointLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(
            PointLayerName,
            GeoKernelShapeType.Point,
            PointStyle());

        _pointLayerIndex = geoKernelViewerControl.GetLayerInfoByName(PointLayerName)?.Index ?? _pointLayerIndex;
        if (_pointLayerIndex < 0)
            MessageBox.Show(this, "Point layer could not be created.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void BeginPointEditing()
    {
        if (_pointLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_pointLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_pointLayerIndex);

        geoKernelViewerControl.SetActiveEditLayerIndex(_pointLayerIndex);
        UpdateFeatureCount();
    }

    private void addPointButton_Click(object? sender, EventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        BeginPointEditing();

        var featureNo = _pointCursor + 1;
        var point = SamplePointAt(_pointCursor);
        var attributes = CreateAttributes(featureNo);

        if (!geoKernelViewerControl.AddPointToEditLayer(_pointLayerIndex, point.X, point.Y, attributes))
        {
            UpdateStatus("Point with attributes could not be added.");
            return;
        }

        _pointCursor++;
        _rows.Add(new FeatureRow(
            featureNo,
            Convert.ToString(attributes["Name"]) ?? "",
            Convert.ToString(attributes["Category"]) ?? "",
            Convert.ToString(attributes["Score"]) ?? "",
            Convert.ToString(attributes["Source"]) ?? ""));
        RebuildGrid();
        SelectGridRow(featureNo);
        RefreshMap();
        UpdateFeatureCount();
        UpdateStatus($"AddPointToEditLayer({_pointLayerIndex}, {point.X:F4}, {point.Y:F4}, attributes)");
    }

    private void infoButton_Click(object? sender, EventArgs e)
    {
        _infoMode = !_infoMode;
        geoKernelViewerControl.ActiveTool = _infoMode ? GeoKernelViewerTool.Info : GeoKernelViewerTool.Pan;
        SetInfoButtonState();
        UpdateStatus(_infoMode ? "Info mode: click an added point to read attributes." : "Pan mode.");
    }

    private void clearPointsButton_Click(object? sender, EventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_pointLayerIndex);
        _pointCursor = 0;
        _rows.Clear();
        RebuildGrid();
        BeginPointEditing();
        RefreshMap();
        UpdateInfoText("Click Add Point With Attributes, then use Info and click an added point.");
        UpdateStatus("Points with attributes cleared.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_MapMouseUp(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (e.Tool != GeoKernelViewerTool.Info)
            return;

        var result = geoKernelViewerControl.HitTestTopFeatureAt(e.ScreenPoint.X, e.ScreenPoint.Y, 8);
        if (result is null || !result.IsValid)
        {
            attributesGrid.ClearSelection();
            UpdateInfoText("No feature found.");
            UpdateStatus("No feature found under cursor.");
            return;
        }

        UpdateInfoText(FormatFeatureAttributes(result));
        if (result.LayerIndex == _pointLayerIndex)
            SelectGridRow(result.ShapeId);

        UpdateStatus($"Attributes read from layer '{result.LayerName}', feature {result.ShapeId}.");
    }

    private void geoKernelViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        if (_syncingGridSelection)
            return;

        var selected = geoKernelViewerControl.GetSelectedFeatures().FirstOrDefault(feature => feature.LayerIndex == _pointLayerIndex);
        if (selected is not null)
            SelectGridRow(selected.ShapeId);
    }

    private void ConfigureGrid()
    {
        attributesGrid.Columns.Clear();
        attributesGrid.Columns.Add("ShapeId", "#");
        attributesGrid.Columns.Add("Name", "Name");
        attributesGrid.Columns.Add("Category", "Category");
        attributesGrid.Columns.Add("Score", "Score");
        attributesGrid.Columns.Add("Source", "Source");
    }

    private void RebuildGrid()
    {
        attributesGrid.Rows.Clear();
        foreach (var row in _rows)
            attributesGrid.Rows.Add(row.ShapeId, row.Name, row.Category, row.Score, row.Source);
    }

    private void SelectGridRow(int shapeId)
    {
        _syncingGridSelection = true;
        try
        {
            attributesGrid.ClearSelection();
            foreach (DataGridViewRow row in attributesGrid.Rows)
            {
                if (row.Cells[0].Value is not int rowShapeId || rowShapeId != shapeId)
                    continue;

                row.Selected = true;
                attributesGrid.FirstDisplayedScrollingRowIndex = row.Index;
                break;
            }
        }
        finally
        {
            _syncingGridSelection = false;
        }
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

    private void UpdateFeatureCount()
    {
        var count = _pointLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_pointLayerIndex) : 0;
        pointCountLabel.Text = $"Feature count: {count}";
    }

    private void UpdateInfoText(string text)
    {
        infoTextBox.Text = text;
    }

    private void UpdateStatus(string text)
    {
        statusLabel.Text = text;
    }

    private void SetInfoButtonState()
    {
        infoButton.BackColor = _infoMode ? System.Drawing.Color.FromArgb(210, 232, 247) : System.Drawing.SystemColors.Control;
        infoButton.FlatAppearance.BorderSize = _infoMode ? 1 : 0;
    }

    private static GeoKernelPoint SamplePointAt(int index)
    {
        const double xMin = -123.0;
        const double yMin = 29.0;
        const double xStep = 5.0;
        const double yStep = 4.0;
        const int columns = 12;

        return new GeoKernelPoint(xMin + index % columns * xStep, yMin + index / columns * yStep);
    }

    private static Dictionary<string, object?> CreateAttributes(int featureNo)
    {
        return new Dictionary<string, object?>
        {
            ["Name"] = $"Site {featureNo}",
            ["Category"] = featureNo % 2 == 0 ? "Even" : "Odd",
            ["Score"] = featureNo * 10,
            ["Source"] = ".NET Dictionary"
        };
    }

    private static string FormatFeatureAttributes(GeoKernelFeatureHitTestResult result)
    {
        var lines = new List<string>
        {
            $"Layer: {result.LayerName}",
            $"Shape ID: {result.ShapeId}",
            $"Feature ID: {result.FeatureId}",
            ""
        };

        foreach (var pair in result.Attributes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            lines.Add($"{pair.Key} = {pair.Value}");

        return string.Join(Environment.NewLine, lines);
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

    private static GeoKernelLayerStyle PointStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            LineColor = "#8C321D",
            PointSize = 9.5,
            LineWidth = 1.2,
            ShowLabels = true,
            LabelField = "Name",
            LabelFontSize = 10.0,
            LabelColor = "#263238",
            LabelHaloEnabled = true,
            LabelHaloColor = "#FFFFFF",
            LabelHaloWidth = 2.0,
            LabelOffsetY = -11.0,
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

    private sealed record FeatureRow(int ShapeId, string Name, string Category, string Score, string Source);
}
