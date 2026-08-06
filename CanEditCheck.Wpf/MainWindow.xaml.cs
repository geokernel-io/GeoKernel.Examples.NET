using GeoKernel.Examples.Common;
using System.IO;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.CanEditCheck.Wpf;

public partial class MainWindow
{
    private const string EditableLayerName = "Editable Points";

    private readonly List<FeatureRow> _rows = [];
    private int _editLayerIndex = -1;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.Select;
        viewerControl.SelectionChanged += ViewerControl_SelectionChanged;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        PopulatePoints();
        SetSampleExtent();
        UpdateUi("Use Begin Edit and Select to see canEdit* capability checks change.");
    }

    private bool LoadLayer()
    {
        var path = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", Title);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Title);
            return false;
        }

        viewerControl.SetLayerName(0, "World");
        return true;
    }

    private void CreateEditableLayer()
    {
        _editLayerIndex = viewerControl.AddEmptyVectorLayer(EditableLayerName, GeoKernelShapeType.Point, PointStyle());
        _editLayerIndex = viewerControl.GetLayerInfoByName(EditableLayerName)?.Index ?? _editLayerIndex;
    }

    private void PopulatePoints()
    {
        if (_editLayerIndex < 0)
            return;

        viewerControl.RollbackEditLayer(_editLayerIndex);
        BeginEditing();
        viewerControl.ClearSelectedFeatures();
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

            if (viewerControl.AddPointToEditLayer(_editLayerIndex, point.X, point.Y, attributes))
                _rows.Add(row);
        }

        viewerControl.CommitEditLayer(_editLayerIndex);
        featureListView.ItemsSource = null;
        featureListView.ItemsSource = _rows;
        RefreshMap();
        UpdateUi("Points reset. Begin Edit, then click a point to enable selected-feature checks.");
    }

    private void BeginEditing()
    {
        if (_editLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_editLayerIndex))
            viewerControl.BeginEditLayer(_editLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_editLayerIndex);
    }

    private void BeginEdit_Click(object sender, RoutedEventArgs e)
    {
        BeginEditing();
        UpdateUi("Edit session started. Select a point on the map.");
    }

    private void CommitEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_editLayerIndex >= 0 && viewerControl.CommitEditLayer(_editLayerIndex))
            UpdateUi("Edit session committed. Selected-feature checks are false until editing starts again.");
    }

    private void RollbackEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_editLayerIndex >= 0 && viewerControl.RollbackEditLayer(_editLayerIndex))
        {
            viewerControl.ClearSelectedFeatures();
            RefreshMap();
            UpdateUi("Edit session rolled back.");
        }
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = selectButton.IsChecked == true ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan;
        UpdateUi(selectButton.IsChecked == true ? "Select mode. Click a point." : "Pan mode.");
    }

    private void ClearSelection_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ClearSelectedFeatures();
        featureListView.SelectedItems.Clear();
        UpdateUi("Selection cleared.");
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        PopulatePoints();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        SelectFeatureListRows();
        UpdateUi("Selection changed.");
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _editLayerIndex)
            return;

        UpdateUi("Edit state changed.");
    }

    private void UpdateUi(string message)
    {
        var canEditLayer = _editLayerIndex >= 0 && viewerControl.CanEditLayer(_editLayerIndex);
        var editing = _editLayerIndex >= 0 && viewerControl.IsLayerEditing(_editLayerIndex);
        var selectedCount = viewerControl.SelectedFeatureCount;
        var canEditSelection = viewerControl.CanEditSelectedFeatures();
        var canMoveSelection = viewerControl.CanMoveSelectedFeatures();

        beginEditButton.IsEnabled = canEditLayer && !editing;
        commitEditButton.IsEnabled = editing;
        rollbackEditButton.IsEnabled = editing;
        clearSelectionButton.IsEnabled = selectedCount > 0;

        checkListView.ItemsSource = new[]
        {
            new CheckRow("CanEditLayer(index)", canEditLayer ? "true" : "false", "Layer must exist and support editing."),
            new CheckRow("CanEditSelectedFeatures()", canEditSelection ? "true" : "false", "Requires selected features from an editing layer."),
            new CheckRow("CanMoveSelectedFeatures()", canMoveSelection ? "true" : "false", "Requires selected editable features with valid geometry.")
        };

        selectionTextBox.Text = SelectionText();
        stateText.Text = $"Editing: {(editing ? "ON" : "OFF")} | Selected: {selectedCount}";
        statusText.Text = message;
    }

    private string SelectionText()
    {
        var selected = viewerControl.GetSelectedFeatures()
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

    private void SelectFeatureListRows()
    {
        var selectedIds = viewerControl.GetSelectedFeatures()
            .Where(feature => feature.LayerIndex == _editLayerIndex)
            .Select(feature => feature.ShapeId)
            .ToHashSet();

        featureListView.SelectedItems.Clear();
        foreach (var row in _rows.Where(row => selectedIds.Contains(row.ShapeId)))
            featureListView.SelectedItems.Add(row);
    }

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-130.0, 20.0, -65.0, 55.0);
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
    private sealed record CheckRow(string Api, string Result, string Why);
}
