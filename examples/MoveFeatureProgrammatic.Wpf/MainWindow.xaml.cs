using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.MoveFeatureProgrammatic.Wpf;

public partial class MainWindow
{
    private const string EditableLayerName = "Movable Points";

    private readonly List<FeatureRow> _rows = [];
    private int _editLayerIndex = -1;
    private bool _syncingSelection;
    private bool _populating;

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
        UpdateStatus("Select one or more points, then use direction buttons to call MoveSelectedFeaturesInEditLayer(deltaX, deltaY).");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "MoveFeatureProgrammatic", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", "MoveFeatureProgrammatic", MessageBoxButton.OK, MessageBoxImage.Error);
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

        _populating = true;
        try
        {
            viewerControl.RollbackEditLayer(_editLayerIndex);
            BeginEditing();
            viewerControl.ClearSelectedFeatures();
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

                if (viewerControl.AddPointToEditLayer(_editLayerIndex, location.X, location.Y, attributes))
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

        if (!viewerControl.IsLayerEditing(_editLayerIndex))
            viewerControl.BeginEditLayer(_editLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_editLayerIndex);
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = selectButton.IsChecked == true ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan;
        UpdateStatus(selectButton.IsChecked == true ? "Select mode. Click a point on the map." : "Pan mode.");
    }

    private void MoveWest_Click(object sender, RoutedEventArgs e)
    {
        MoveSelection(-DeltaValue(), 0.0);
    }

    private void MoveEast_Click(object sender, RoutedEventArgs e)
    {
        MoveSelection(DeltaValue(), 0.0);
    }

    private void MoveNorth_Click(object sender, RoutedEventArgs e)
    {
        MoveSelection(0.0, DeltaValue());
    }

    private void MoveSouth_Click(object sender, RoutedEventArgs e)
    {
        MoveSelection(0.0, -DeltaValue());
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

        UpdateStatus($"List selected feature {row.ShapeId}. Select it on the map, then move with direction buttons.");
    }

    private void ViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        var selected = viewerControl.GetSelectedFeatures().FirstOrDefault(feature => feature.LayerIndex == _editLayerIndex);
        if (selected is not null)
            SelectListRow(selected.ShapeId);

        UpdateCount();
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
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

        if (viewerControl.SelectedFeatureCount == 0)
        {
            UpdateStatus("Select one or more movable points first.");
            return;
        }

        if (!viewerControl.CanMoveSelectedFeatures())
        {
            UpdateStatus("Current selection cannot be moved.");
            return;
        }

        if (!viewerControl.MoveSelectedFeaturesInEditLayer(deltaX, deltaY))
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
        if (double.TryParse(deltaTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            return Math.Clamp(value, 0.10, 30.0);

        deltaTextBox.Text = "3.00";
        return 3.0;
    }

    private void SelectListRow(int shapeId)
    {
        _syncingSelection = true;
        try
        {
            featureListView.SelectedItem = _rows.FirstOrDefault(row => row.ShapeId == shapeId);
            if (featureListView.SelectedItem is not null)
                featureListView.ScrollIntoView(featureListView.SelectedItem);
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
            var feature = viewerControl.GetSelectedFeatures().FirstOrDefault(selected => selected.ShapeId == shapeId && selected.LayerIndex == _editLayerIndex);
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
        var selectedShapeId = viewerControl.GetSelectedFeatures().FirstOrDefault(feature => feature.LayerIndex == _editLayerIndex)?.ShapeId ?? -1;
        featureListView.ItemsSource = null;
        featureListView.ItemsSource = _rows;

        if (selectedShapeId > 0)
            SelectListRow(selectedShapeId);

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

    private sealed record FeatureRow(int ShapeId, string Name, string Group, GeoKernelPoint Location)
    {
        public string LocationText => $"{Location.X:0.000}, {Location.Y:0.000}";
    }
}
