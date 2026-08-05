using GeoKernel.NET.WinForms;

namespace GeoKernel.InsertVertex.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string PolygonLayerName = "Editable Polygons";

    private readonly List<GeoKernelPoint> _vertices = [];
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
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Select;
        geoKernelViewerControl.SelectionChanged += geoKernelViewerControl_SelectionChanged;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        PopulateShape();
        SetSampleExtent();
        UpdateStatus("Select the polygon, choose an insert index, then click Insert Vertex.");
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

    private void CreateEditableLayer()
    {
        _polygonLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(PolygonLayerName, GeoKernelShapeType.Polygon, PolygonStyle());
        _polygonLayerIndex = geoKernelViewerControl.GetLayerInfoByName(PolygonLayerName)?.Index ?? _polygonLayerIndex;
    }

    private void PopulateShape()
    {
        if (_polygonLayerIndex < 0)
            return;

        _populating = true;
        try
        {
            geoKernelViewerControl.RollbackEditLayer(_polygonLayerIndex);
            BeginEditing();
            geoKernelViewerControl.ClearSelectedFeatures();

            _vertices.Clear();
            _vertices.AddRange(
            [
                new GeoKernelPoint(-119.0, 28.0),
                new GeoKernelPoint(-109.0, 45.0),
                new GeoKernelPoint(-91.0, 42.0),
                new GeoKernelPoint(-83.0, 30.0),
                new GeoKernelPoint(-99.0, 22.0),
                new GeoKernelPoint(-115.0, 23.5)
            ]);

            geoKernelViewerControl.AddPolygonToEditLayer(
                _polygonLayerIndex,
                ClosedVertices(),
                new Dictionary<string, object?> { ["Name"] = "Insert target" });
        }
        finally
        {
            _populating = false;
        }

        ConfigureInsertRange();
        SetTool(GeoKernelViewerTool.Select);
        RefreshMap();
        UpdateInfo();
    }

    private void BeginEditing()
    {
        if (_polygonLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_polygonLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_polygonLayerIndex);

        geoKernelViewerControl.SetActiveEditLayerIndex(_polygonLayerIndex);
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        geoKernelViewerControl.ActiveTool = tool;
        panButton.Checked = tool == GeoKernelViewerTool.Pan;
        selectButton.Checked = tool == GeoKernelViewerTool.Select;
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        SetTool(panButton.Checked ? GeoKernelViewerTool.Pan : GeoKernelViewerTool.Select);
    }

    private void selectButton_Click(object? sender, EventArgs e)
    {
        SetTool(selectButton.Checked ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan);
    }

    private void insertVertexButton_Click(object? sender, EventArgs e)
    {
        BeginEditing();

        if (!geoKernelViewerControl.GetSelectedFeatures().Any(feature => feature.LayerIndex == _polygonLayerIndex))
        {
            UpdateStatus("Select the editable polygon first.");
            return;
        }

        var partIndex = (int)partNumeric.Value;
        var insertIndex = (int)insertIndexNumeric.Value;
        var point = InsertionPointForSegment(insertIndex);
        if (!geoKernelViewerControl.InsertSelectedFeatureVertexInEditLayer(partIndex, insertIndex, point.X, point.Y))
        {
            UpdateStatus("InsertSelectedFeatureVertexInEditLayer failed.");
            return;
        }

        _vertices.Insert(insertIndex, point);
        ConfigureInsertRange();
        insertIndexNumeric.Value = Math.Min(insertIndex + 1, (int)insertIndexNumeric.Maximum);
        RefreshMap();
        UpdateStatus($"InsertSelectedFeatureVertexInEditLayer({partIndex}, {insertIndex}, {point.X:0.000}, {point.Y:0.000})");
    }

    private void resetButton_Click(object? sender, EventArgs e)
    {
        PopulateShape();
        UpdateStatus("Shape reset.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void partNumeric_ValueChanged(object? sender, EventArgs e)
    {
        UpdateInfo();
    }

    private void insertIndexNumeric_ValueChanged(object? sender, EventArgs e)
    {
        UpdateInfo();
    }

    private void geoKernelViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        UpdateInfo();
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (_populating || e.LayerIndex != _polygonLayerIndex)
            return;

        UpdateInfo();
    }

    private void ConfigureInsertRange()
    {
        insertIndexNumeric.Minimum = 1;
        insertIndexNumeric.Maximum = Math.Max(1, _vertices.Count);
        insertIndexNumeric.Value = Math.Min(Math.Max(insertIndexNumeric.Value, insertIndexNumeric.Minimum), insertIndexNumeric.Maximum);
        countLabel.Text = $"Vertex count: {_vertices.Count}";
    }

    private IReadOnlyList<GeoKernelPoint> ClosedVertices()
    {
        return [.. _vertices, _vertices[0]];
    }

    private GeoKernelPoint InsertionPointForSegment(int insertIndex)
    {
        if (_vertices.Count < 2)
            return new GeoKernelPoint();

        var a = _vertices[insertIndex - 1];
        var b = _vertices[insertIndex == _vertices.Count ? 0 : insertIndex];
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var length = Math.Sqrt(dx * dx + dy * dy);
        var safeLength = length > 0.0 ? length : 1.0;
        var offset = safeLength * 0.22;

        return new GeoKernelPoint(
            (a.X + b.X) * 0.5 - dy / safeLength * offset,
            (a.Y + b.Y) * 0.5 + dx / safeLength * offset);
    }

    private void UpdateInfo()
    {
        var selected = geoKernelViewerControl.GetSelectedFeatures();
        var insertIndex = (int)insertIndexNumeric.Value;
        var point = InsertionPointForSegment(insertIndex);

        countLabel.Text = $"Vertex count: {_vertices.Count}";
        infoTextBox.Text = string.Join(
            Environment.NewLine,
            [
                "Usage:",
                "- Select: click the polygon.",
                "- Part is 0 for this sample.",
                "- Insert index means insert before that vertex index.",
                "- The sample computes a visible point near the selected segment.",
                "- Click Insert Vertex to call InsertSelectedFeatureVertexInEditLayer(part, index, point).",
                "",
                $"Selected feature count: {selected.Count}",
                $"Vertex count: {_vertices.Count}",
                $"Part index: {(int)partNumeric.Value}",
                $"Insert index: {insertIndex}",
                $"Calculated point: {point.X:0.000}, {point.Y:0.000}"
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

    private static GeoKernelLayerStyle PolygonStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#F2D27A",
            FillOpacity = 140,
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
