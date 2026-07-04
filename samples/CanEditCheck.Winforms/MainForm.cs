using GeoKernel.NET.WinForms;

namespace GeoKernel.CanEditCheck.Winforms;

public sealed partial class MainForm : Form
{
    private const string EditableLayerName = "Editable Points";

    private readonly List<FeatureRow> _rows = [];
    private int _editLayerIndex = -1;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Select;
        geoKernelViewerControl.SelectionChanged += geoKernelViewerControl_SelectionChanged;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        PopulatePoints();
        SetSampleExtent();
        UpdateUi("Use Begin Edit and Select to see canEdit* capability checks change.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
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

    private void CreateEditableLayer()
    {
        _editLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(EditableLayerName, GeoKernelShapeType.Point, PointStyle());
        _editLayerIndex = geoKernelViewerControl.GetLayerInfoByName(EditableLayerName)?.Index ?? _editLayerIndex;
    }

    private void PopulatePoints()
    {
        if (_editLayerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_editLayerIndex);
        BeginEditing();
        geoKernelViewerControl.ClearSelectedFeatures();
        _rows.Clear();

        for (var i = 0; i < 14; ++i)
        {
            var row = new FeatureRow(i + 1, $"Point {i + 1}", i % 2 == 0 ? "North" : "South");
            var point = SamplePointAt(i);
            var attributes = new Dictionary<string, object?>
            {
                ["Name"] = row.Name,
                ["Group"] = row.Group
            };

            if (geoKernelViewerControl.AddPointToEditLayer(_editLayerIndex, point.X, point.Y, attributes))
                _rows.Add(row);
        }

        geoKernelViewerControl.CommitEditLayer(_editLayerIndex);
        RebuildFeatureList();
        RefreshMap();
        UpdateUi("Points reset. Begin Edit, then click a point to enable selected-feature checks.");
    }

    private void BeginEditing()
    {
        if (_editLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_editLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_editLayerIndex);

        geoKernelViewerControl.SetActiveEditLayerIndex(_editLayerIndex);
    }

    private void beginEditButton_Click(object? sender, EventArgs e)
    {
        BeginEditing();
        UpdateUi("Edit session started. Select a point on the map.");
    }

    private void commitEditButton_Click(object? sender, EventArgs e)
    {
        if (_editLayerIndex >= 0 && geoKernelViewerControl.CommitEditLayer(_editLayerIndex))
            UpdateUi("Edit session committed. Selected-feature checks are false until editing starts again.");
    }

    private void rollbackEditButton_Click(object? sender, EventArgs e)
    {
        if (_editLayerIndex >= 0 && geoKernelViewerControl.RollbackEditLayer(_editLayerIndex))
        {
            geoKernelViewerControl.ClearSelectedFeatures();
            RefreshMap();
            UpdateUi("Edit session rolled back.");
        }
    }

    private void selectButton_Click(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = selectButton.Checked ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan;
        UpdateUi(selectButton.Checked ? "Select mode. Click a point." : "Pan mode.");
    }

    private void clearSelectionButton_Click(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ClearSelectedFeatures();
        featureListView.SelectedItems.Clear();
        UpdateUi("Selection cleared.");
    }

    private void resetButton_Click(object? sender, EventArgs e)
    {
        PopulatePoints();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        SelectFeatureListRows();
        UpdateUi("Selection changed.");
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _editLayerIndex)
            return;

        UpdateUi("Edit state changed.");
    }

    private void UpdateUi(string message)
    {
        var canEditLayer = _editLayerIndex >= 0 && geoKernelViewerControl.CanEditLayer(_editLayerIndex);
        var editing = _editLayerIndex >= 0 && geoKernelViewerControl.IsLayerEditing(_editLayerIndex);
        var selectedCount = geoKernelViewerControl.SelectedFeatureCount;
        var canEditSelection = geoKernelViewerControl.CanEditSelectedFeatures();
        var canMoveSelection = geoKernelViewerControl.CanMoveSelectedFeatures();

        beginEditButton.Enabled = canEditLayer && !editing;
        commitEditButton.Enabled = editing;
        rollbackEditButton.Enabled = editing;
        clearSelectionButton.Enabled = selectedCount > 0;

        SetCheckRows(canEditLayer, canEditSelection, canMoveSelection);
        selectionTextBox.Text = SelectionText();
        stateLabel.Text = $"Editing: {(editing ? "ON" : "OFF")} | Selected: {selectedCount}";
        statusLabel.Text = message;
    }

    private void SetCheckRows(bool canEditLayer, bool canEditSelection, bool canMoveSelection)
    {
        checkListView.Items.Clear();
        AddCheckRow("CanEditLayer(index)", canEditLayer, "Layer must exist and support editing.");
        AddCheckRow("CanEditSelectedFeatures()", canEditSelection, "Requires selected features from an editing layer.");
        AddCheckRow("CanMoveSelectedFeatures()", canMoveSelection, "Requires selected editable features with valid geometry.");
    }

    private void AddCheckRow(string api, bool result, string why)
    {
        var item = new ListViewItem(api);
        item.SubItems.Add(result ? "true" : "false");
        item.SubItems.Add(why);
        checkListView.Items.Add(item);
    }

    private string SelectionText()
    {
        var selected = geoKernelViewerControl.GetSelectedFeatures()
            .Where(feature => feature.LayerIndex == _editLayerIndex)
            .ToArray();

        if (selected.Length == 0)
            return "No selected feature.\r\n\r\nCanEditSelectedFeatures and CanMoveSelectedFeatures require at least one selected feature while the layer is editing.";

        return string.Join(Environment.NewLine, selected.Select(feature =>
        {
            var row = _rows.FirstOrDefault(item => item.ShapeId == feature.ShapeId);
            var name = row?.Name ?? $"Feature {feature.ShapeId}";
            return $"Feature {feature.ShapeId}: {name}";
        }));
    }

    private void RebuildFeatureList()
    {
        featureListView.Items.Clear();
        foreach (var row in _rows)
        {
            var item = new ListViewItem(row.ShapeId.ToString()) { Tag = row.ShapeId };
            item.SubItems.Add(row.Name);
            item.SubItems.Add(row.Group);
            featureListView.Items.Add(item);
        }
    }

    private void SelectFeatureListRows()
    {
        var selectedIds = geoKernelViewerControl.GetSelectedFeatures()
            .Where(feature => feature.LayerIndex == _editLayerIndex)
            .Select(feature => feature.ShapeId)
            .ToHashSet();

        foreach (ListViewItem item in featureListView.Items)
            item.Selected = item.Tag is int shapeId && selectedIds.Contains(shapeId);
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

    private static GeoKernelPoint SamplePointAt(int index)
    {
        const double xMin = -121.0;
        const double yMin = 31.0;
        const double xStep = 8.0;
        const double yStep = 5.5;
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

    private static GeoKernelLayerStyle PointStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            LineColor = "#8C321D",
            PointSize = 12.0,
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
            LabelOffsetY = -13.0,
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

    private sealed record FeatureRow(int ShapeId, string Name, string Group);
}
