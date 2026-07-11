using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.DeleteFeature.Wpf;

public partial class MainWindow
{
    private const string EditableLayerName = "Editable Points";

    private readonly List<FeatureRow> _rows = [];
    private int _editLayerIndex = -1;
    private bool _syncingSelection;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.Select;
        viewerControl.SelectionChanged += ViewerControl_SelectionChanged;

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        PopulatePoints();
        SetSampleExtent();
        UpdateStatus("Select a point on the map or in the list, then delete one feature or all selected features.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "DeleteFeature", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", "DeleteFeature", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            viewerControl.SetLayerName(worldLayer.Index, "World");

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

            if (viewerControl.AddPointToEditLayer(_editLayerIndex, point.X, point.Y, attributes))
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

        if (!viewerControl.IsLayerEditing(_editLayerIndex))
            viewerControl.BeginEditLayer(_editLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_editLayerIndex);
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = selectButton.IsChecked == true ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan;
        UpdateStatus(selectButton.IsChecked == true ? "Select mode. Click points on the map." : "Pan mode.");
    }

    private void DeleteFeature_Click(object sender, RoutedEventArgs e)
    {
        var shapeId = SelectedShapeIdFromList();
        if (shapeId < 0)
            shapeId = viewerControl.GetSelectedFeatures().FirstOrDefault(f => f.LayerIndex == _editLayerIndex)?.ShapeId ?? -1;

        if (shapeId < 0)
        {
            UpdateStatus("Select a feature first.");
            return;
        }

        BeginEditing();
        if (!viewerControl.DeleteShapeFromEditLayer(_editLayerIndex, shapeId))
        {
            UpdateStatus("DeleteShapeFromEditLayer failed.");
            return;
        }

        _rows.RemoveAll(row => row.ShapeId == shapeId);
        viewerControl.ClearSelectedFeatures();
        RebuildFeatureList();
        RefreshMap();
        UpdateStatus($"Deleted feature {shapeId} with DeleteShapeFromEditLayer(index, shapeId).");
    }

    private void DeleteSelected_Click(object sender, RoutedEventArgs e)
    {
        var selected = viewerControl.GetSelectedFeatures()
            .Where(feature => feature.LayerIndex == _editLayerIndex)
            .ToArray();
        if (selected.Length == 0)
        {
            UpdateStatus("Select one or more features first.");
            return;
        }

        BeginEditing();
        if (!viewerControl.DeleteSelectedFeaturesFromEditLayer())
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

    private void ResetPoints_Click(object sender, RoutedEventArgs e)
    {
        PopulatePoints();
        UpdateStatus("Points reset.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void FeatureListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_syncingSelection || featureListView.SelectedItem is not FeatureRow row)
            return;

        UpdateStatus($"List selected feature {row.ShapeId}. Use Delete Feature for a single delete.");
    }

    private void ViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        var selected = viewerControl.GetSelectedFeatures().FirstOrDefault(feature => feature.LayerIndex == _editLayerIndex);
        if (selected is not null)
            SelectListRow(selected.ShapeId);

        UpdateCount();
    }

    private int SelectedShapeIdFromList()
    {
        return featureListView.SelectedItem is FeatureRow row ? row.ShapeId : -1;
    }

    private void SelectListRow(int shapeId)
    {
        _syncingSelection = true;
        try
        {
            featureListView.SelectedItem = _rows.FirstOrDefault(row => row.ShapeId == shapeId);
            featureListView.ScrollIntoView(featureListView.SelectedItem);
        }
        finally
        {
            _syncingSelection = false;
        }
    }

    private void RebuildFeatureList()
    {
        featureListView.ItemsSource = null;
        featureListView.ItemsSource = _rows;
        UpdateCount();
    }

    private void UpdateCount()
    {
        countText.Text = $"Feature count: {_rows.Count} | Selected: {viewerControl.SelectedFeatureCount}";
    }

    private void UpdateStatus(string message)
    {
        UpdateCount();
        statusText.Text = message;
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
