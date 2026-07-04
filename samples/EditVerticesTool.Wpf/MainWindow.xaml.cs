using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.EditVerticesTool.Wpf;

public partial class MainWindow
{
    private const string LineLayerName = "Editable Lines";
    private const string PolygonLayerName = "Editable Polygons";

    private int _lineLayerIndex = -1;
    private int _polygonLayerIndex = -1;
    private bool _populating;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.EditVertices;
        viewerControl.SelectionChanged += ViewerControl_SelectionChanged;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreateEditableLayers();
        PopulateShapes();
        SetSampleExtent();
        UpdateStatus("Edit Vertices is active. Drag vertices, double-click segments to insert, use Delete Vertex to remove.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "EditVerticesTool", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", "EditVerticesTool", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            viewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreateEditableLayers()
    {
        _lineLayerIndex = viewerControl.AddEmptyVectorLayer(LineLayerName, GeoKernelShapeType.Polyline, LineStyle());
        _lineLayerIndex = viewerControl.GetLayerInfoByName(LineLayerName)?.Index ?? _lineLayerIndex;

        _polygonLayerIndex = viewerControl.AddEmptyVectorLayer(PolygonLayerName, GeoKernelShapeType.Polygon, PolygonStyle());
        _polygonLayerIndex = viewerControl.GetLayerInfoByName(PolygonLayerName)?.Index ?? _polygonLayerIndex;
    }

    private void PopulateShapes()
    {
        if (_lineLayerIndex < 0 || _polygonLayerIndex < 0)
            return;

        _populating = true;
        try
        {
            viewerControl.RollbackEditLayer(_lineLayerIndex);
            viewerControl.RollbackEditLayer(_polygonLayerIndex);
            BeginEditing();
            viewerControl.ClearSelectedFeatures();

            viewerControl.AddPolylineToEditLayer(
                _lineLayerIndex,
                [
                    new GeoKernelPoint(-127.0, 31.0),
                    new GeoKernelPoint(-118.0, 40.0),
                    new GeoKernelPoint(-107.0, 34.0),
                    new GeoKernelPoint(-96.0, 43.0),
                    new GeoKernelPoint(-86.0, 37.0)
                ],
                new Dictionary<string, object?> { ["Name"] = "Pacific route" });

            viewerControl.AddPolylineToEditLayer(
                _lineLayerIndex,
                [
                    new GeoKernelPoint(-113.0, 24.0),
                    new GeoKernelPoint(-101.0, 29.0),
                    new GeoKernelPoint(-90.0, 27.0),
                    new GeoKernelPoint(-80.0, 33.0)
                ],
                new Dictionary<string, object?> { ["Name"] = "Gulf route" });

            viewerControl.AddPolygonToEditLayer(
                _polygonLayerIndex,
                [
                    new GeoKernelPoint(-118.0, 30.0),
                    new GeoKernelPoint(-109.0, 45.0),
                    new GeoKernelPoint(-91.0, 42.0),
                    new GeoKernelPoint(-94.0, 27.0),
                    new GeoKernelPoint(-111.0, 24.0),
                    new GeoKernelPoint(-118.0, 30.0)
                ],
                new Dictionary<string, object?> { ["Name"] = "Edit polygon A" });

            viewerControl.AddPolygonToEditLayer(
                _polygonLayerIndex,
                [
                    new GeoKernelPoint(-83.0, 24.0),
                    new GeoKernelPoint(-73.0, 31.0),
                    new GeoKernelPoint(-65.0, 25.0),
                    new GeoKernelPoint(-72.0, 18.0),
                    new GeoKernelPoint(-83.0, 24.0)
                ],
                new Dictionary<string, object?> { ["Name"] = "Edit polygon B" });
        }
        finally
        {
            _populating = false;
        }

        SetTool(GeoKernelViewerTool.EditVertices);
        RefreshMap();
        UpdateInfo();
    }

    private void BeginEditing()
    {
        if (_lineLayerIndex >= 0 && !viewerControl.IsLayerEditing(_lineLayerIndex))
            viewerControl.BeginEditLayer(_lineLayerIndex);

        if (_polygonLayerIndex >= 0 && !viewerControl.IsLayerEditing(_polygonLayerIndex))
            viewerControl.BeginEditLayer(_polygonLayerIndex);

        if (_polygonLayerIndex >= 0)
            viewerControl.SetActiveEditLayerIndex(_polygonLayerIndex);
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        viewerControl.ActiveTool = tool;
        panButton.IsChecked = tool == GeoKernelViewerTool.Pan;
        editVerticesButton.IsChecked = tool == GeoKernelViewerTool.EditVertices;
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        SetTool(panButton.IsChecked == true ? GeoKernelViewerTool.Pan : GeoKernelViewerTool.EditVertices);
    }

    private void EditVertices_Click(object sender, RoutedEventArgs e)
    {
        BeginEditing();
        SetTool(editVerticesButton.IsChecked == true ? GeoKernelViewerTool.EditVertices : GeoKernelViewerTool.Pan);
    }

    private void DeleteVertex_Click(object sender, RoutedEventArgs e)
    {
        DeleteSelectedVertex();
    }

    private void ResetShapes_Click(object sender, RoutedEventArgs e)
    {
        PopulateShapes();
        UpdateStatus("Shapes reset.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Delete)
            return;

        DeleteSelectedVertex();
        e.Handled = true;
    }

    private void DeleteSelectedVertex()
    {
        BeginEditing();
        if (viewerControl.DeleteSelectedVertexFromEditLayer())
            UpdateStatus("Selected vertex deleted.");
        else
            UpdateStatus("No active vertex to delete. Click a vertex first.");

        RefreshMap();
        UpdateInfo();
    }

    private void ViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        UpdateInfo();
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (_populating || (e.LayerIndex != _lineLayerIndex && e.LayerIndex != _polygonLayerIndex))
            return;

        UpdateInfo();
        UpdateStatus("Vertex geometry changed.");
    }

    private void UpdateInfo()
    {
        var lineCount = _lineLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_lineLayerIndex) : 0;
        var polygonCount = _polygonLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_polygonLayerIndex) : 0;
        var selected = viewerControl.GetSelectedFeatures();

        countText.Text = $"Lines: {lineCount} | Polygons: {polygonCount} | Selected: {selected.Count}";
        infoTextBox.Text = string.Join(
            Environment.NewLine,
            [
                "Tool usage:",
                "- Edit Vertices: click a feature or one of its vertices.",
                "- Drag an active vertex to move it.",
                "- Double-click a selected segment to insert a vertex.",
                "- Press Delete or click Delete Vertex to remove the active vertex.",
                "",
                $"Line feature count: {lineCount}",
                $"Polygon feature count: {polygonCount}",
                $"Selected feature count: {selected.Count}",
                "",
                "Selected features:",
                .. selected.Select(feature => $"- {feature.LayerName} / shape {feature.ShapeId} / feature {feature.FeatureId}")
            ]);
    }

    private void UpdateStatus(string message)
    {
        UpdateInfo();
        statusText.Text = message;
    }

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-132.0, 15.0, -55.0, 55.0);
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

    private static GeoKernelLayerStyle LineStyle()
    {
        return new GeoKernelLayerStyle
        {
            LineColor = "#2B6F8E",
            LineWidth = 3.0,
            SelectedLineColor = "#F59E0B",
            SelectedLineWidth = 5.0,
            ShowLabels = true,
            LabelField = "Name",
            LabelFontSize = 10.0,
            LabelHaloEnabled = true,
            LabelHaloColor = "#FFFFFF",
            LabelHaloWidth = 2.0,
            LabelOffsetY = -12.0,
            LabelAllowOverlap = true
        };
    }

    private static GeoKernelLayerStyle PolygonStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#F2D27A",
            FillOpacity = 145,
            LineColor = "#D95D39",
            LineWidth = 2.4,
            SelectedLineColor = "#F59E0B",
            SelectedLineWidth = 5.0,
            ShowLabels = true,
            LabelField = "Name",
            LabelFontSize = 10.0,
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
}
