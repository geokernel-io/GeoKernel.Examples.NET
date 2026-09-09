using GeoKernel.NET.WinForms;

namespace GeoKernel.DeleteVertex.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string PolygonLayerName = "Editable Polygons";

    private readonly List<GeoKernelPoint> _vertices = [];
    private int _polygonLayerIndex = -1;
    private bool _populating;
    private int _activeVertexIndex = -1;

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
        geoKernelViewerControl.MapMouseUp += geoKernelViewerControl_MapMouseUp;
        geoKernelViewerControl.ViewerEvent += geoKernelViewerControl_ViewerEvent;

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        PopulateShape();
        SetSampleExtent();
        UpdateStatus("Use Edit Vertices for active vertex delete, or Select + index for direct API delete.");
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
            _activeVertexIndex = -1;
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
                new Dictionary<string, object?> { ["Name"] = "Delete target" });
        }
        finally
        {
            _populating = false;
        }

        ConfigureVertexRange();
        vertexIndexNumeric.Value = 2;
        SetTool(GeoKernelViewerTool.EditVertices);
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
        selectButton.Checked = tool == GeoKernelViewerTool.Info;
        editVerticesButton.Checked = tool == GeoKernelViewerTool.EditVertices;
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);
    }

    private void selectButton_Click(object? sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.Info);
    }

    private void editVerticesButton_Click(object? sender, EventArgs e)
    {
        BeginEditing();
        SetTool(GeoKernelViewerTool.EditVertices);
    }

    private void deleteSelectedVertexButton_Click(object? sender, EventArgs e)
    {
        BeginEditing();

        if (!geoKernelViewerControl.DeleteSelectedVertexFromEditLayer())
        {
            UpdateStatus("No active vertex. Use Edit Vertices and click a vertex first.");
            return;
        }

        if (_activeVertexIndex >= 0 && _activeVertexIndex < _vertices.Count)
            _vertices.RemoveAt(_activeVertexIndex);
        _activeVertexIndex = -1;
        ConfigureVertexRange();
        RefreshMap();
        UpdateStatus("DeleteSelectedVertexFromEditLayer() succeeded.");
    }

    private void deleteByIndexButton_Click(object? sender, EventArgs e)
    {
        BeginEditing();

        if (!geoKernelViewerControl.GetSelectedFeatures().Any(feature => feature.LayerIndex == _polygonLayerIndex))
        {
            UpdateStatus("Select the editable polygon first.");
            return;
        }

        var partIndex = (int)partNumeric.Value;
        var vertexIndex = (int)vertexIndexNumeric.Value;
        if (!geoKernelViewerControl.DeleteSelectedFeatureVertexInEditLayer(partIndex, vertexIndex))
        {
            UpdateStatus("DeleteSelectedFeatureVertexInEditLayer failed.");
            return;
        }

        if (vertexIndex >= 0 && vertexIndex < _vertices.Count)
            _vertices.RemoveAt(vertexIndex);

        ConfigureVertexRange();
        RefreshMap();
        UpdateStatus($"DeleteSelectedFeatureVertexInEditLayer({partIndex}, {vertexIndex}) succeeded.");
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

    private void vertexIndexNumeric_ValueChanged(object? sender, EventArgs e)
    {
        UpdateInfo();
    }

    private void geoKernelViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        ConfigureVertexRange();
        UpdateInfo();
    }

    private void geoKernelViewerControl_ViewerEvent(object? sender, GeoKernelViewerEventArgs e)
    {
        if (e.EventType == GeoKernelViewerEventType.ActiveToolChanged)
            SetToolChecks((GeoKernelViewerTool)e.IntValue);
    }

    private void geoKernelViewerControl_MapMouseUp(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (e.Tool == GeoKernelViewerTool.Info)
        {
            var hit = geoKernelViewerControl.HitTestTopFeatureAt(e.ScreenPoint.X, e.ScreenPoint.Y, 8);
            if (hit is null || !hit.IsValid || hit.LayerIndex != _polygonLayerIndex)
            {
                geoKernelViewerControl.ClearSelectedFeatures();
                UpdateStatus("No editable polygon selected.");
                return;
            }

            geoKernelViewerControl.SelectTopFeatureAt(e.ScreenPoint.X, e.ScreenPoint.Y, 8);
            UpdateStatus($"Selected feature {hit.FeatureId}.");
            return;
        }

        if (e.Tool != GeoKernelViewerTool.EditVertices)
            return;

        _activeVertexIndex = FindVertexAt(e.ScreenPoint, 10.0);
        if (_activeVertexIndex >= 0)
            vertexIndexNumeric.Value = Math.Min(_activeVertexIndex, (int)vertexIndexNumeric.Maximum);
        UpdateInfo();
    }

    private void SetToolChecks(GeoKernelViewerTool tool)
    {
        panButton.Checked = tool == GeoKernelViewerTool.Pan;
        selectButton.Checked = tool == GeoKernelViewerTool.Info;
        editVerticesButton.Checked = tool == GeoKernelViewerTool.EditVertices;
    }

    private int FindVertexAt(GeoKernelPoint screenPoint, double tolerancePixels)
    {
        var bestIndex = -1;
        var bestDistanceSquared = tolerancePixels * tolerancePixels;
        for (var i = 0; i < _vertices.Count; i++)
        {
            var candidate = geoKernelViewerControl.WorldToScreen(_vertices[i].X, _vertices[i].Y);
            var dx = candidate.X - screenPoint.X;
            var dy = candidate.Y - screenPoint.Y;
            var distanceSquared = dx * dx + dy * dy;
            if (distanceSquared > bestDistanceSquared)
                continue;

            bestIndex = i;
            bestDistanceSquared = distanceSquared;
        }

        return bestIndex;
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (_populating || e.LayerIndex != _polygonLayerIndex)
            return;

        UpdateInfo();
    }

    private void ConfigureVertexRange()
    {
        var hasSelectedPolygon = geoKernelViewerControl.GetSelectedFeatures()
            .Any(feature => feature.LayerIndex == _polygonLayerIndex);
        vertexIndexNumeric.Minimum = 0;
        vertexIndexNumeric.Maximum = hasSelectedPolygon
            ? Math.Max(0, _vertices.Count - 1)
            : 5;
        vertexIndexNumeric.Value = Math.Min(vertexIndexNumeric.Value, vertexIndexNumeric.Maximum);
        countLabel.Text = $"Vertex count: {_vertices.Count}";
    }

    private IReadOnlyList<GeoKernelPoint> ClosedVertices()
    {
        return [.. _vertices, _vertices[0]];
    }

    private void UpdateInfo()
    {
        var selected = geoKernelViewerControl.GetSelectedFeatures();
        var vertexIndex = (int)vertexIndexNumeric.Value;
        var pointText = vertexIndex >= 0 && vertexIndex < _vertices.Count
            ? $"{_vertices[vertexIndex].X:0.000}, {_vertices[vertexIndex].Y:0.000}"
            : "-";

        countLabel.Text = $"Vertex count: {_vertices.Count}";
        var details = new List<string>
        {
            "Usage:",
            "- Edit Vertices: click a vertex to make it active.",
            "- Delete Selected Vertex calls DeleteSelectedVertexFromEditLayer().",
            "- Select: click polygon, choose part/index, then Delete By Index.",
            "- Delete By Index calls DeleteSelectedFeatureVertexInEditLayer(part, vertexIndex).",
            ""
        };

        if (!selected.Any(feature => feature.LayerIndex == _polygonLayerIndex))
        {
            details.Add("Selected feature: none");
            infoTextBox.Text = string.Join(Environment.NewLine, details);
            return;
        }

        details.AddRange(
        [
            $"Selected feature id: {selected.First(feature => feature.LayerIndex == _polygonLayerIndex).FeatureId}",
            $"Vertex count: {_vertices.Count}",
            $"Part {(int)partNumeric.Value} vertex count: {_vertices.Count}",
            $"Delete index: {vertexIndex}",
            $"Vertex point: {pointText}"
        ]);
        infoTextBox.Text = string.Join(
            Environment.NewLine,
            details);
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
