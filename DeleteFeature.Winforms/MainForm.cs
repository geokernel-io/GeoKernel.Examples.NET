using GeoKernel.NET.WinForms;

namespace GeoKernel.DeleteFeature.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string EditableLayerName = "Editable Points";

    private readonly List<FeatureRow> _rows = [];
    private int _editLayerIndex = -1;
    private bool _syncingSelection;

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

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        PopulatePoints();
        SetSampleExtent();
        UpdateStatus("Select a point on the map or in the list, then delete one feature or all selected features.");
    }

    private bool LoadLayer()
    {
        var path = _worldSamplePath;
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
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

        for (var i = 0; i < 16; ++i)
        {
            var row = new FeatureRow(i + 1, $"Point {i + 1}", i % 2 == 0 ? "A" : "B", (i + 1) * 5);
            var point = SamplePointAt(i);
            var attributes = new Dictionary<string, object?>
            {
                ["Name"] = row.Name,
                ["Group"] = row.Group,
                ["Value"] = row.Value
            };

            if (geoKernelViewerControl.AddPointToEditLayer(_editLayerIndex, point.X, point.Y, attributes))
                _rows.Add(row);
        }

        RebuildFeatureList();
        RefreshMap();
        UpdateCount();
    }

    private void BeginEditing()
    {
        if (_editLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_editLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_editLayerIndex);

        geoKernelViewerControl.SetActiveEditLayerIndex(_editLayerIndex);
    }

    private void selectButton_Click(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = selectButton.Checked ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan;
        UpdateStatus(selectButton.Checked ? "Select mode. Click points on the map." : "Pan mode.");
    }

    private void deleteFeatureButton_Click(object? sender, EventArgs e)
    {
        var shapeId = SelectedShapeIdFromList();
        if (shapeId < 0)
            shapeId = geoKernelViewerControl.GetSelectedFeatures().FirstOrDefault(f => f.LayerIndex == _editLayerIndex)?.ShapeId ?? -1;

        if (shapeId < 0)
        {
            UpdateStatus("Select a feature first.");
            return;
        }

        BeginEditing();
        if (!geoKernelViewerControl.DeleteShapeFromEditLayer(_editLayerIndex, shapeId))
        {
            UpdateStatus("DeleteShapeFromEditLayer failed.");
            return;
        }

        _rows.RemoveAll(row => row.ShapeId == shapeId);
        geoKernelViewerControl.ClearSelectedFeatures();
        RebuildFeatureList();
        RefreshMap();
        UpdateStatus($"Deleted feature {shapeId} with DeleteShapeFromEditLayer(index, shapeId).");
    }

    private void deleteSelectedButton_Click(object? sender, EventArgs e)
    {
        var selected = geoKernelViewerControl.GetSelectedFeatures()
            .Where(feature => feature.LayerIndex == _editLayerIndex)
            .ToArray();
        if (selected.Length == 0)
        {
            UpdateStatus("Select one or more features first.");
            return;
        }

        BeginEditing();
        if (!geoKernelViewerControl.DeleteSelectedFeaturesFromEditLayer())
        {
            UpdateStatus("DeleteSelectedFeaturesFromEditLayer failed.");
            return;
        }

        var deletedIds = selected.Select(feature => feature.ShapeId).ToHashSet();
        _rows.RemoveAll(row => deletedIds.Contains(row.ShapeId));
        RebuildFeatureList();
        RefreshMap();
        UpdateStatus($"Deleted {deletedIds.Count} selected feature(s).");
    }

    private void resetButton_Click(object? sender, EventArgs e)
    {
        PopulatePoints();
        UpdateStatus("Points reset.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void featureListView_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_syncingSelection || featureListView.SelectedItems.Count == 0)
            return;

        var shapeId = SelectedShapeIdFromList();
        UpdateStatus(shapeId > 0
            ? $"List selected feature {shapeId}. Use Delete Feature for a single delete."
            : "No feature selected.");
    }

    private void geoKernelViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        var selected = geoKernelViewerControl.GetSelectedFeatures().FirstOrDefault(feature => feature.LayerIndex == _editLayerIndex);
        if (selected is not null)
            SelectListRow(selected.ShapeId);

        UpdateCount();
    }

    private int SelectedShapeIdFromList()
    {
        return featureListView.SelectedItems.Count > 0 && featureListView.SelectedItems[0].Tag is int shapeId ? shapeId : -1;
    }

    private void SelectListRow(int shapeId)
    {
        _syncingSelection = true;
        try
        {
            featureListView.SelectedItems.Clear();
            foreach (ListViewItem item in featureListView.Items)
            {
                if (item.Tag is not int rowShapeId || rowShapeId != shapeId)
                    continue;

                item.Selected = true;
                item.EnsureVisible();
                break;
            }
        }
        finally
        {
            _syncingSelection = false;
        }
    }

    private void RebuildFeatureList()
    {
        featureListView.Items.Clear();
        foreach (var row in _rows)
        {
            var item = new ListViewItem(row.ShapeId.ToString()) { Tag = row.ShapeId };
            item.SubItems.Add(row.Name);
            item.SubItems.Add(row.Group);
            item.SubItems.Add(row.Value.ToString());
            featureListView.Items.Add(item);
        }

        UpdateCount();
    }

    private void UpdateCount()
    {
        countLabel.Text = $"Feature count: {_rows.Count} | Selected: {geoKernelViewerControl.SelectedFeatureCount}";
    }

    private void UpdateStatus(string message)
    {
        UpdateCount();
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

    private static GeoKernelPoint SamplePointAt(int index)
    {
        const double xMin = -122.0;
        const double yMin = 30.0;
        const double xStep = 7.0;
        const double yStep = 5.0;
        const int columns = 8;

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

    private sealed record FeatureRow(int ShapeId, string Name, string Group, int Value);
}
