using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.SetFeatureAttributes.Wpf;

public partial class MainWindow
{
    private const string LayerName = "Editable Attributes";
    private static readonly string[] StatusValues = ["Planned", "Active", "Done"];
    private static readonly SiteDefinition[] Sites =
    [
        new("Survey North", "Planned", 1, -122.4, 37.8), new("Depot West", "Active", 2, -118.2, 34.1),
        new("Field Team", "Planned", 3, -105.0, 39.7), new("Control East", "Active", 4, -95.4, 29.8),
        new("Archive South", "Done", 5, -80.2, 25.8)
    ];
    private readonly Dictionary<int, SiteState> _sites = [];
    private int _layerIndex = -1, _selectedShapeId = 1;
    private bool _loadingSelection;

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        statusComboBox.ItemsSource = StatusValues;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.LayerEditStateChanged += (_, e) => { if (e.LayerIndex == _layerIndex) Dispatcher.BeginInvoke(UpdateButtons); };
        var world = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
        if (!File.Exists(world) || !viewerControl.AddLayerFile(world, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        { MessageBox.Show(this, "World layer could not be loaded.", Title); return; }
        CreatePointLayer(); BeginEditing(); RefreshGrid(1); SetExtent(); UpdateButtons();
        statusText.Text = "Select a row, edit the attributes, then click Apply Attributes.";
    }

    private void CreatePointLayer()
    {
        _layerIndex = viewerControl.AddPointLayer(LayerName, Sites.Select(s => new GeoKernelPoint(s.X, s.Y)).ToArray(), PointStyle());
        _layerIndex = viewerControl.GetLayerInfoByName(LayerName)?.Index ?? _layerIndex;
        SeedAttributes(); viewerControl.SetLayerStyle(_layerIndex, PointStyle());
    }

    private void SeedAttributes()
    {
        _sites.Clear(); for (var i = 0; i < Sites.Length; i++) _sites[i + 1] = new(Sites[i].Name, Sites[i].Status, Sites[i].Priority);
        if (!viewerControl.BeginEditLayer(_layerIndex)) return;
        try
        {
            foreach (var pair in _sites) viewerControl.SetFeatureAttributesInEditLayer(_layerIndex, pair.Key, -1, Attributes(pair.Value));
            viewerControl.CommitEditLayer(_layerIndex);
        }
        catch { viewerControl.RollbackEditLayer(_layerIndex); throw; }
    }

    private void BeginEditing()
    {
        if (_layerIndex >= 0 && !viewerControl.IsLayerEditing(_layerIndex)) viewerControl.BeginEditLayer(_layerIndex);
        if (_layerIndex >= 0) viewerControl.SetActiveEditLayerIndex(_layerIndex);
    }

    private void Apply_Click(object sender, RoutedEventArgs e)
    {
        if (_layerIndex < 0 || !_sites.ContainsKey(_selectedShapeId)) return;
        BeginEditing(); if (!int.TryParse(priorityTextBox.Text, out var priority)) priority = 1;
        var state = new SiteState(nameTextBox.Text.Trim(), statusComboBox.SelectedItem as string ?? StatusValues[0], Math.Clamp(priority, 1, 10));
        if (!viewerControl.SetFeatureAttributesInEditLayer(_layerIndex, _selectedShapeId, -1, Attributes(state)))
        { statusText.Text = "SetFeatureAttributesInEditLayer returned false."; return; }
        _sites[_selectedShapeId] = state; RefreshAfterChange(); statusText.Text = $"Shape {_selectedShapeId} attributes updated.";
    }

    private void Undo_Click(object sender, RoutedEventArgs e) { if (viewerControl.UndoEditLayer(_layerIndex)) { SyncFromLayer(); RefreshAfterChange(); statusText.Text = "UndoEditLayer applied."; } }
    private void Redo_Click(object sender, RoutedEventArgs e) { if (viewerControl.RedoEditLayer(_layerIndex)) { SyncFromLayer(); RefreshAfterChange(); statusText.Text = "RedoEditLayer applied."; } }
    private void Reset_Click(object sender, RoutedEventArgs e)
    { viewerControl.RollbackEditLayer(_layerIndex); SeedAttributes(); BeginEditing(); _selectedShapeId = 1; RefreshAfterChange(); statusText.Text = "Attributes reset to the sample baseline."; }
    private void FullExtent_Click(object sender, RoutedEventArgs e) => SetExtent();

    private void FeatureGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
    { if (!_loadingSelection && featureGrid.SelectedItem is FeatureRow row) { _selectedShapeId = row.ShapeId; LoadFields(); UpdateButtons(); } }

    private void RefreshAfterChange()
    { RefreshGrid(_selectedShapeId); viewerControl.InvalidateRenderCache(false, true); viewerControl.RefreshLayers(); UpdateButtons(); }
    private void RefreshGrid(int selected)
    {
        _loadingSelection = true;
        var rows = _sites.OrderBy(p => p.Key).Select(p => new FeatureRow(p.Key, p.Value.Name, p.Value.Status, p.Value.Priority)).ToArray();
        featureGrid.ItemsSource = rows; featureGrid.SelectedItem = rows.FirstOrDefault(r => r.ShapeId == selected); _loadingSelection = false;
        _selectedShapeId = selected; LoadFields();
    }
    private void LoadFields()
    { if (_sites.TryGetValue(_selectedShapeId, out var s)) { nameTextBox.Text = s.Name; statusComboBox.SelectedItem = s.Status; priorityTextBox.Text = s.Priority.ToString(); } }
    private void SyncFromLayer()
    {
        for (var i = 0; i < _sites.Count; i++)
        {
            var id = i + 1; var a = viewerControl.GetLayerFeatureAttributes(_layerIndex, i);
            _sites[id] = new(Text(a, "Name", _sites[id].Name), Text(a, "Status", _sites[id].Status), Number(a, "Priority", _sites[id].Priority));
        }
    }
    private void UpdateButtons()
    { var edit = _layerIndex >= 0 && viewerControl.IsLayerEditing(_layerIndex); stateText.Text = $"Editing: {(edit ? "ON" : "OFF")} | Selected: {_selectedShapeId}"; undoButton.IsEnabled = edit && viewerControl.CanUndoEditLayer(_layerIndex); redoButton.IsEnabled = edit && viewerControl.CanRedoEditLayer(_layerIndex); }
    private void SetExtent() => viewerControl.ViewExtent = new GeoKernelExtent(-132, 18, -60, 55);
    private static Dictionary<string, object?> Attributes(SiteState s) => new() { ["Name"] = s.Name, ["Status"] = s.Status, ["Priority"] = s.Priority };
    private static string Text(IReadOnlyDictionary<string, object?> a, string k, string f) => a.TryGetValue(k, out var v) && v is not null ? (v is JsonElement j ? j.ToString() : Convert.ToString(v) ?? f) : f;
    private static int Number(IReadOnlyDictionary<string, object?> a, string k, int f) => int.TryParse(Text(a, k, f.ToString()), out var n) ? n : f;
    private static GeoKernelLayerStyle WorldStyle() => new() { FillColor="#D8E5E1", FillOpacity=210, LineColor="#6F8883", LineWidth=.7 };
    private static GeoKernelLayerStyle PointStyle() => new() { PointColor="#D95D39", LineColor="#8C321D", PointSize=12, LineWidth=1.3, ShowLabels=true, LabelField="Name", LabelColor="#111827", LabelFontSize=11, LabelHaloEnabled=true, LabelHaloColor="#FFFFB8", LabelHaloWidth=2, LabelOffsetY=-14 };
    private sealed record SiteState(string Name, string Status, int Priority);
    private sealed record SiteDefinition(string Name, string Status, int Priority, double X, double Y);
    private sealed record FeatureRow(int ShapeId, string Name, string Status, int Priority);
}
