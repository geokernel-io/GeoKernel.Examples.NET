using GeoKernel.NET.WinForms;

namespace GeoKernel.MoveFeatureProgrammatic.Winforms;

public sealed partial class MainForm : Form
{
    private const string EditableLayerName = "Movable Points";

    private readonly List<FeatureRow> _rows = [];
    private int _editLayerIndex = -1;
    private bool _syncingSelection;
    private bool _populating;

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
        UpdateStatus("Select one or more points, then use direction buttons to call MoveSelectedFeaturesInEditLayer(deltaX, deltaY).");
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
                var location = SamplePointAt(i);
                var row = new FeatureRow(i + 1, $"Point {i + 1}", i % 2 == 0 ? "North" : "South", location);
                var attributes = new Dictionary<string, object?>
                {
                    ["Name"] = row.Name,
                    ["Group"] = row.Group
                };

                if (geoKernelViewerControl.AddPointToEditLayer(_editLayerIndex, location.X, location.Y, attributes))
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
        geoKernelViewerControl.ActiveTool = selectButton.Checked ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan;
        UpdateStatus(selectButton.Checked ? "Select mode. Click a point on the map." : "Pan mode.");
    }

    private void moveWestButton_Click(object? sender, EventArgs e)
    {
        MoveSelection(-DeltaValue(), 0.0);
    }

    private void moveEastButton_Click(object? sender, EventArgs e)
    {
        MoveSelection(DeltaValue(), 0.0);
    }

    private void moveNorthButton_Click(object? sender, EventArgs e)
    {
        MoveSelection(0.0, DeltaValue());
    }

    private void moveSouthButton_Click(object? sender, EventArgs e)
    {
        MoveSelection(0.0, -DeltaValue());
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
            ? $"List selected feature {shapeId}. Select it on the map, then move with direction buttons."
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

        SyncRowsFromLayer();
        RebuildFeatureList();
        RefreshMap();
        UpdateStatus("Feature geometry changed.");
    }

    private void MoveSelection(double deltaX, double deltaY)
    {
        BeginEditing();

        if (geoKernelViewerControl.SelectedFeatureCount == 0)
        {
            UpdateStatus("Select one or more movable points first.");
            return;
        }

        if (!geoKernelViewerControl.CanMoveSelectedFeatures())
        {
            UpdateStatus("Current selection cannot be moved.");
            return;
        }

        if (!geoKernelViewerControl.MoveSelectedFeaturesInEditLayer(deltaX, deltaY))
        {
            UpdateStatus("MoveSelectedFeaturesInEditLayer failed.");
            return;
        }

        SyncRowsFromLayer();
        RebuildFeatureList();
        RefreshMap();
        UpdateStatus($"MoveSelectedFeaturesInEditLayer({deltaX:0.00}, {deltaY:0.00})");
    }

    private double DeltaValue()
    {
        return (double)deltaNumeric.Value;
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

    private void SyncRowsFromLayer()
    {
        if (_editLayerIndex < 0)
            return;

        for (var i = 0; i < _rows.Count; ++i)
        {
            var shapeId = _rows[i].ShapeId;
            var feature = geoKernelViewerControl.GetSelectedFeatures().FirstOrDefault(selected => selected.ShapeId == shapeId && selected.LayerIndex == _editLayerIndex);
            var location = feature is null
                ? _rows[i].Location
                : new GeoKernelPoint(
                    (feature.Extent.XMin + feature.Extent.XMax) / 2.0,
                    (feature.Extent.YMin + feature.Extent.YMax) / 2.0);
            _rows[i] = _rows[i] with { Location = location };
        }
    }

    private void RebuildFeatureList()
    {
        var selectedShapeId = geoKernelViewerControl.GetSelectedFeatures().FirstOrDefault(feature => feature.LayerIndex == _editLayerIndex)?.ShapeId ?? -1;
        featureListView.Items.Clear();

        foreach (var row in _rows)
        {
            var item = new ListViewItem(row.ShapeId.ToString()) { Tag = row.ShapeId };
            item.SubItems.Add(row.Name);
            item.SubItems.Add(row.Group);
            item.SubItems.Add($"{row.Location.X:0.000}, {row.Location.Y:0.000}");
            featureListView.Items.Add(item);
        }

        if (selectedShapeId > 0)
            SelectListRow(selectedShapeId);

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
