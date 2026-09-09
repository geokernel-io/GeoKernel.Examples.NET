using GeoKernel.NET.WinForms;

namespace GeoKernel.MoveFeatureTool.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string EditableLayerName = "Movable Points";

    private readonly List<FeatureRow> _rows = [];
    private int _editLayerIndex = -1;
    private bool _syncingSelection;
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
        PopulatePoints();
        SetSampleExtent();
        UpdateStatus("Select a point, switch to Move Feature, then drag it on the map.");
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
        _editLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(EditableLayerName, GeoKernelShapeType.Point, PointStyle());
        _editLayerIndex = geoKernelViewerControl.GetLayerInfoByName(EditableLayerName)?.Index ?? _editLayerIndex;
    }

    private void PopulatePoints()
    {
        if (_editLayerIndex < 0)
            return;

        _populating = true;
        try
        {
            geoKernelViewerControl.RollbackEditLayer(_editLayerIndex);
            BeginEditing();
            geoKernelViewerControl.ClearSelectedFeatures();
            _rows.Clear();

            for (var i = 0; i < 14; ++i)
            {
                var row = new FeatureRow(i + 1, $"Point {i + 1}", i % 2 == 0 ? "North" : "South", SamplePointAt(i));
                var attributes = new Dictionary<string, object?>
                {
                    ["Name"] = row.Name,
                    ["Group"] = row.Group
                };

                if (geoKernelViewerControl.AddPointToEditLayer(_editLayerIndex, row.Location.X, row.Location.Y, attributes))
                    _rows.Add(row);
            }
        }
        finally
        {
            _populating = false;
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
        if (selectButton.Checked)
            moveButton.Checked = false;

        geoKernelViewerControl.ActiveTool = selectButton.Checked ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan;
        UpdateStatus(selectButton.Checked ? "Select mode. Click a point." : "Pan mode.");
    }

    private void moveButton_Click(object? sender, EventArgs e)
    {
        if (moveButton.Checked)
            selectButton.Checked = false;

        BeginEditing();
        geoKernelViewerControl.ActiveTool = moveButton.Checked ? GeoKernelViewerTool.MoveFeature : GeoKernelViewerTool.Pan;
        UpdateStatus(moveButton.Checked ? "Move Feature mode. Drag a selected point." : "Pan mode.");
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
            ? $"List selected feature {shapeId}. Click Select on the toolbar, then click it on the map before moving."
            : "No feature selected.");
    }

    private void geoKernelViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        var selected = geoKernelViewerControl.GetSelectedFeatures().FirstOrDefault(feature => feature.LayerIndex == _editLayerIndex);
        if (selected is not null)
            SelectListRow(selected.ShapeId);

        UpdateCount();
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (_populating || e.LayerIndex != _editLayerIndex)
            return;

        RebuildFeatureList();
        RefreshMap();
        UpdateStatus("Feature geometry changed by Move Feature tool.");
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
            item.SubItems.Add($"{row.Location.X:0.000}, {row.Location.Y:0.000}");
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

    private sealed record FeatureRow(int ShapeId, string Name, string Group, GeoKernelPoint Location);
}
