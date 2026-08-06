using GeoKernel.Examples.Common;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.MoveFeatureTool.Wpf;

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
        UpdateStatus("Select a point, switch to Move Feature, then drag it on the map.");
    }

    private bool LoadLayer()
    {
        var path = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "MoveFeatureTool", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", "MoveFeatureTool", MessageBoxButton.OK, MessageBoxImage.Error);
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
                var row = new FeatureRow(i + 1, $"Point {i + 1}", i % 2 == 0 ? "North" : "South", SamplePointAt(i));
                var attributes = new Dictionary<string, object?>
                {
                    ["Name"] = row.Name,
                    ["Group"] = row.Group
                };

                if (viewerControl.AddPointToEditLayer(_editLayerIndex, row.Location.X, row.Location.Y, attributes))
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
        if (selectButton.IsChecked == true)
            moveButton.IsChecked = false;

        viewerControl.ActiveTool = selectButton.IsChecked == true ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan;
        UpdateStatus(selectButton.IsChecked == true ? "Select mode. Click a point." : "Pan mode.");
    }

    private void MoveFeature_Click(object sender, RoutedEventArgs e)
    {
        if (moveButton.IsChecked == true)
            selectButton.IsChecked = false;

        BeginEditing();
        viewerControl.ActiveTool = moveButton.IsChecked == true ? GeoKernelViewerTool.MoveFeature : GeoKernelViewerTool.Pan;
        UpdateStatus(moveButton.IsChecked == true ? "Move Feature mode. Drag a selected point." : "Pan mode.");
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

        UpdateStatus($"List selected feature {row.ShapeId}. Click Select, then click it on the map before moving.");
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

        RebuildFeatureList();
        RefreshMap();
        UpdateStatus("Feature geometry changed by Move Feature tool.");
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
