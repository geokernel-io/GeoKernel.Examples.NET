using GeoKernel.NET.WinForms;

namespace GeoKernel.EditVerticesTool.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string LineLayerName = "Editable Lines";
    private const string PolygonLayerName = "Editable Polygons";

    private int _lineLayerIndex = -1;
    private int _polygonLayerIndex = -1;
    private bool _populating;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        _worldSamplePath = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(_worldSamplePath)) return;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.EditVertices;
        geoKernelViewerControl.SelectionChanged += geoKernelViewerControl_SelectionChanged;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreateEditableLayers();
        PopulateShapes();
        SetSampleExtent();
        UpdateStatus("Edit Vertices is active. Drag vertices, double-click segments to insert, use Delete Vertex to remove.");
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

        var worldLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            geoKernelViewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreateEditableLayers()
    {
        _lineLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(LineLayerName, GeoKernelShapeType.Polyline, LineStyle());
        _lineLayerIndex = geoKernelViewerControl.GetLayerInfoByName(LineLayerName)?.Index ?? _lineLayerIndex;

        _polygonLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(PolygonLayerName, GeoKernelShapeType.Polygon, PolygonStyle());
        _polygonLayerIndex = geoKernelViewerControl.GetLayerInfoByName(PolygonLayerName)?.Index ?? _polygonLayerIndex;
    }

    private void PopulateShapes()
    {
        if (_lineLayerIndex < 0 || _polygonLayerIndex < 0)
            return;

        _populating = true;
        try
        {
            geoKernelViewerControl.RollbackEditLayer(_lineLayerIndex);
            geoKernelViewerControl.RollbackEditLayer(_polygonLayerIndex);
            BeginEditing();
            geoKernelViewerControl.ClearSelectedFeatures();

            geoKernelViewerControl.AddPolylineToEditLayer(
                _lineLayerIndex,
                [
                    new GeoKernelPoint(-127.0, 31.0),
                    new GeoKernelPoint(-118.0, 40.0),
                    new GeoKernelPoint(-107.0, 34.0),
                    new GeoKernelPoint(-96.0, 43.0),
                    new GeoKernelPoint(-86.0, 37.0)
                ],
                new Dictionary<string, object?> { ["Name"] = "Pacific route" });

            geoKernelViewerControl.AddPolylineToEditLayer(
                _lineLayerIndex,
                [
                    new GeoKernelPoint(-113.0, 24.0),
                    new GeoKernelPoint(-101.0, 29.0),
                    new GeoKernelPoint(-90.0, 27.0),
                    new GeoKernelPoint(-80.0, 33.0)
                ],
                new Dictionary<string, object?> { ["Name"] = "Gulf route" });

            geoKernelViewerControl.AddPolygonToEditLayer(
                _polygonLayerIndex,
                [
                    new GeoKernelPoint(-118.0, 30.0),
                    new GeoKernelPoint(-109.0, 45.0),
                    new GeoKernelPoint(-91.0, 42.0),
                    new GeoKernelPoint(-94.0, 27.0),
                    new GeoKernelPoint(-111.0, 24.0),
                    new GeoKernelPoint(-118.0, 30.0)
                ],
                new Dictionary<string, object?> { ["Name"] = "Edit polygon A" });

            geoKernelViewerControl.AddPolygonToEditLayer(
                _polygonLayerIndex,
                [
                    new GeoKernelPoint(-83.0, 24.0),
                    new GeoKernelPoint(-73.0, 31.0),
                    new GeoKernelPoint(-65.0, 25.0),
                    new GeoKernelPoint(-72.0, 18.0),
                    new GeoKernelPoint(-83.0, 24.0)
                ],
                new Dictionary<string, object?> { ["Name"] = "Edit polygon B" });
        }
        finally
        {
            _populating = false;
        }

        SetTool(GeoKernelViewerTool.EditVertices);
        RefreshMap();
        UpdateInfo();
    }

    private void BeginEditing()
    {
        if (_lineLayerIndex >= 0 && !geoKernelViewerControl.IsLayerEditing(_lineLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_lineLayerIndex);

        if (_polygonLayerIndex >= 0 && !geoKernelViewerControl.IsLayerEditing(_polygonLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_polygonLayerIndex);

        if (_polygonLayerIndex >= 0)
            geoKernelViewerControl.SetActiveEditLayerIndex(_polygonLayerIndex);
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        geoKernelViewerControl.ActiveTool = tool;
        panButton.Checked = tool == GeoKernelViewerTool.Pan;
        editVerticesButton.Checked = tool == GeoKernelViewerTool.EditVertices;
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        SetTool(panButton.Checked ? GeoKernelViewerTool.Pan : GeoKernelViewerTool.EditVertices);
    }

    private void editVerticesButton_Click(object? sender, EventArgs e)
    {
        BeginEditing();
        SetTool(editVerticesButton.Checked ? GeoKernelViewerTool.EditVertices : GeoKernelViewerTool.Pan);
    }

    private void deleteVertexButton_Click(object? sender, EventArgs e)
    {
        BeginEditing();
        if (geoKernelViewerControl.DeleteSelectedVertexFromEditLayer())
            UpdateStatus("Selected vertex deleted.");
        else
            UpdateStatus("No active vertex to delete. Click a vertex first.");

        RefreshMap();
        UpdateInfo();
    }

    private void resetButton_Click(object? sender, EventArgs e)
    {
        PopulateShapes();
        UpdateStatus("Shapes reset.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void MainForm_KeyDown(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Delete)
            return;

        deleteVertexButton_Click(sender, e);
        e.Handled = true;
    }

    private void geoKernelViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        UpdateInfo();
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (_populating || (e.LayerIndex != _lineLayerIndex && e.LayerIndex != _polygonLayerIndex))
            return;

        UpdateInfo();
        UpdateStatus("Vertex geometry changed.");
    }

    private void UpdateInfo()
    {
        var lineCount = _lineLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_lineLayerIndex) : 0;
        var polygonCount = _polygonLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_polygonLayerIndex) : 0;
        var selected = geoKernelViewerControl.GetSelectedFeatures();

        countLabel.Text = $"Lines: {lineCount} | Polygons: {polygonCount} | Selected: {selected.Count}";
        infoTextBox.Text = string.Join(
            Environment.NewLine,
            [
                "Tool usage:",
                "- Edit Vertices: click a feature or one of its vertices.",
                "- Drag an active vertex to move it.",
                "- Double-click a selected segment to insert a vertex.",
                "- Press Delete or click Delete Vertex to remove the active vertex.",
                "",
                $"Line feature count: {lineCount}",
                $"Polygon feature count: {polygonCount}",
                $"Selected feature count: {selected.Count}",
                "",
                "Selected features:",
                .. selected.Select(feature => $"- {feature.LayerName} / shape {feature.ShapeId} / feature {feature.FeatureId}")
            ]);
    }

    private void UpdateStatus(string message)
    {
        UpdateInfo();
        statusLabel.Text = message;
    }

    private void RefreshMap()
    {
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-132.0, 15.0, -55.0, 55.0);
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

    private static GeoKernelLayerStyle PolygonStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#F2D27A",
            FillOpacity = 145,
            LineColor = "#D95D39",
            LineWidth = 2.4,
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
