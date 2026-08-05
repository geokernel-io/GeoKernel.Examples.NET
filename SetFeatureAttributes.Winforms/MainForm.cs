using System.Text.Json;
using GeoKernel.NET.WinForms;

namespace GeoKernel.SetFeatureAttributes.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string LayerName = "Editable Attributes";
    private const string NameField = "Name";
    private const string StatusField = "Status";
    private const string PriorityField = "Priority";

    private static readonly string[] StatusValues = ["Planned", "Active", "Done"];

    private static readonly SiteDefinition[] Sites =
    [
        new("Survey North", "Planned", 1, -122.4, 37.8),
        new("Depot West", "Active", 2, -118.2, 34.1),
        new("Field Team", "Planned", 3, -105.0, 39.7),
        new("Control East", "Active", 4, -95.4, 29.8),
        new("Archive South", "Done", 5, -80.2, 25.8)
    ];

    private readonly Dictionary<int, SiteState> _sites = [];
    private int _layerIndex = -1;
    private int _selectedShapeId = 1;
    private bool _loadingSelection;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        _worldSamplePath = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(_worldSamplePath)) return;
        statusComboBox.Items.AddRange(StatusValues.Cast<object>().ToArray());        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePointLayer();
        BeginAttributeEditing();
        RefreshFeatureGrid(_selectedShapeId);
        SetSampleExtent();
        UpdateButtons();
        statusText.Text = "Select a row, edit the attributes, then click Apply Attributes.";
    }

    private bool LoadLayer()
    {
        var path = _worldSamplePath;
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "SetFeatureAttributes");
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

    private void CreatePointLayer()
    {
        _layerIndex = geoKernelViewerControl.AddPointLayer(
            LayerName,
            Sites.Select(site => new GeoKernelPoint(site.X, site.Y)).ToArray(),
            PointStyle());

        _layerIndex = geoKernelViewerControl.GetLayerInfoByName(LayerName)?.Index ?? _layerIndex;
        SeedAttributes();
        geoKernelViewerControl.SetLayerStyle(_layerIndex, PointStyle());
    }

    private void SeedAttributes()
    {
        _sites.Clear();
        for (var i = 0; i < Sites.Length; ++i)
        {
            var shapeId = i + 1;
            var site = Sites[i];
            _sites[shapeId] = new SiteState(site.Name, site.Status, site.Priority);
        }

        if (!geoKernelViewerControl.BeginEditLayer(_layerIndex))
            return;

        try
        {
            foreach (var pair in _sites)
                geoKernelViewerControl.SetFeatureAttributesInEditLayer(_layerIndex, pair.Key, -1, ToAttributes(pair.Value));

            geoKernelViewerControl.CommitEditLayer(_layerIndex);
        }
        catch (Exception)
        {
            geoKernelViewerControl.RollbackEditLayer(_layerIndex);
            throw;
        }
    }

    private void BeginAttributeEditing()
    {
        if (_layerIndex >= 0 && !geoKernelViewerControl.IsLayerEditing(_layerIndex))
        {
            geoKernelViewerControl.BeginEditLayer(_layerIndex);
            geoKernelViewerControl.SetActiveEditLayerIndex(_layerIndex);
        }
    }

    private void applyButton_Click(object sender, EventArgs e)
    {
        if (_layerIndex < 0 || !_sites.ContainsKey(_selectedShapeId))
            return;

        BeginAttributeEditing();

        var state = new SiteState(
            nameTextBox.Text.Trim(),
            statusComboBox.SelectedItem as string ?? StatusValues[0],
            (int)priorityNumeric.Value);

        if (!geoKernelViewerControl.SetFeatureAttributesInEditLayer(_layerIndex, _selectedShapeId, -1, ToAttributes(state)))
        {
            statusText.Text = "SetFeatureAttributesInEditLayer returned false.";
            return;
        }

        _sites[_selectedShapeId] = state;
        RefreshAfterAttributeChange(_selectedShapeId);
        statusText.Text = $"Shape {_selectedShapeId} attributes updated.";
    }

    private void undoButton_Click(object sender, EventArgs e)
    {
        if (_layerIndex < 0 || !geoKernelViewerControl.UndoEditLayer(_layerIndex))
            return;

        SyncSitesFromLayer();
        RefreshAfterAttributeChange(_selectedShapeId);
        statusText.Text = "UndoEditLayer applied.";
    }

    private void redoButton_Click(object sender, EventArgs e)
    {
        if (_layerIndex < 0 || !geoKernelViewerControl.RedoEditLayer(_layerIndex))
            return;

        SyncSitesFromLayer();
        RefreshAfterAttributeChange(_selectedShapeId);
        statusText.Text = "RedoEditLayer applied.";
    }

    private void resetButton_Click(object sender, EventArgs e)
    {
        if (_layerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_layerIndex);
        SeedAttributes();
        BeginAttributeEditing();
        _selectedShapeId = 1;
        RefreshAfterAttributeChange(_selectedShapeId);
        statusText.Text = "Attributes reset to the sample baseline.";
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void featureGrid_SelectionChanged(object sender, EventArgs e)
    {
        if (_loadingSelection || featureGrid.CurrentRow?.Tag is not int shapeId)
            return;

        _selectedShapeId = shapeId;
        LoadSelectedAttributes();
        UpdateButtons();
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex == _layerIndex)
            UpdateButtons();
    }

    private void RefreshAfterAttributeChange(int selectedShapeId)
    {
        RefreshFeatureGrid(selectedShapeId);
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
        UpdateButtons();
    }

    private void RefreshFeatureGrid(int selectedShapeId)
    {
        _loadingSelection = true;
        try
        {
            featureGrid.Rows.Clear();
            foreach (var pair in _sites.OrderBy(pair => pair.Key))
            {
                var rowIndex = featureGrid.Rows.Add(pair.Key, pair.Value.Name, pair.Value.Status, pair.Value.Priority);
                featureGrid.Rows[rowIndex].Tag = pair.Key;
                if (pair.Key == selectedShapeId)
                    featureGrid.Rows[rowIndex].Selected = true;
            }
        }
        finally
        {
            _loadingSelection = false;
        }

        _selectedShapeId = selectedShapeId;
        LoadSelectedAttributes();
    }

    private void LoadSelectedAttributes()
    {
        if (!_sites.TryGetValue(_selectedShapeId, out var state))
            return;

        nameTextBox.Text = state.Name;
        statusComboBox.SelectedItem = state.Status;
        priorityNumeric.Value = Math.Clamp(state.Priority, (int)priorityNumeric.Minimum, (int)priorityNumeric.Maximum);
    }

    private void SyncSitesFromLayer()
    {
        for (var rowIndex = 0; rowIndex < _sites.Count; ++rowIndex)
        {
            var shapeId = rowIndex + 1;
            var attributes = geoKernelViewerControl.GetLayerFeatureAttributes(_layerIndex, rowIndex);
            _sites[shapeId] = new SiteState(
                AttributeString(attributes, NameField, _sites[shapeId].Name),
                AttributeString(attributes, StatusField, _sites[shapeId].Status),
                AttributeInt(attributes, PriorityField, _sites[shapeId].Priority));
        }
    }

    private void UpdateButtons()
    {
        var editing = _layerIndex >= 0 && geoKernelViewerControl.IsLayerEditing(_layerIndex);
        stateLabel.Text = $"Editing: {(editing ? "ON" : "OFF")} | Selected: {_selectedShapeId}";
        undoButton.Enabled = _layerIndex >= 0 && geoKernelViewerControl.CanUndoEditLayer(_layerIndex);
        redoButton.Enabled = _layerIndex >= 0 && geoKernelViewerControl.CanRedoEditLayer(_layerIndex);
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-132.0, 18.0, -60.0, 55.0);
    }

    private static Dictionary<string, object?> ToAttributes(SiteState state)
    {
        return new Dictionary<string, object?>
        {
            [NameField] = state.Name,
            [StatusField] = state.Status,
            [PriorityField] = state.Priority
        };
    }

    private static string AttributeString(IReadOnlyDictionary<string, object?> attributes, string key, string fallback)
    {
        if (!attributes.TryGetValue(key, out var value) || value is null)
            return fallback;

        return value is JsonElement json ? json.ToString() : Convert.ToString(value) ?? fallback;
    }

    private static int AttributeInt(IReadOnlyDictionary<string, object?> attributes, string key, int fallback)
    {
        if (!attributes.TryGetValue(key, out var value) || value is null)
            return fallback;

        return value switch
        {
            JsonElement json when json.ValueKind == JsonValueKind.Number && json.TryGetInt32(out var number) => number,
            JsonElement json when int.TryParse(json.ToString(), out var number) => number,
            int number => number,
            long number => checked((int)number),
            double number => (int)Math.Round(number),
            _ when int.TryParse(Convert.ToString(value), out var number) => number,
            _ => fallback
        };
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
            SelectedLineColor = "#111827",
            SelectedLineWidth = 3.0,
            ShowLabels = true,
            LabelField = NameField,
            LabelColor = "#111827",
            LabelFontSize = 11.0,
            LabelHaloEnabled = true,
            LabelHaloColor = "#FFFFB8",
            LabelHaloWidth = 2.0,
            LabelOffsetY = -14.0
        };
    }

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, p =>
    { statusText.Text = p.Message; downloadProgressBar.Visible = true; downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee; if (p.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(p.Percentage.Value, 0, 100); });

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

    private sealed record SiteState(string Name, string Status, int Priority);

    private sealed record SiteDefinition(string Name, string Status, int Priority, double X, double Y);
}
