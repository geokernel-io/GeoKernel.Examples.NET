using GeoKernel.Examples.Common;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.InsertVertex.Wpf;

public partial class MainWindow
{
    private const string PolygonLayerName = "Editable Polygons";

    private readonly List<GeoKernelPoint> _vertices = [];
    private int _polygonLayerIndex = -1;
    private bool _populating;
    private bool _loaded;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _loaded = true;
        
        viewerControl.ActiveTool = GeoKernelViewerTool.Select;
        viewerControl.SelectionChanged += ViewerControl_SelectionChanged;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        PopulateShape();
        SetSampleExtent();
        UpdateStatus("Select the polygon, choose an insert index, then click Insert Vertex.");
    }

    private bool LoadLayer()
    {
        var path = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "InsertVertex", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", "InsertVertex", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            viewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreateEditableLayer()
    {
        _polygonLayerIndex = viewerControl.AddEmptyVectorLayer(PolygonLayerName, GeoKernelShapeType.Polygon, PolygonStyle());
        _polygonLayerIndex = viewerControl.GetLayerInfoByName(PolygonLayerName)?.Index ?? _polygonLayerIndex;
    }

    private void PopulateShape()
    {
        if (_polygonLayerIndex < 0)
            return;

        _populating = true;
        try
        {
            viewerControl.RollbackEditLayer(_polygonLayerIndex);
            BeginEditing();
            viewerControl.ClearSelectedFeatures();

            _vertices.Clear();
            _vertices.AddRange(
            [
                new GeoKernelPoint(-119.0, 28.0),
                new GeoKernelPoint(-109.0, 45.0),
                new GeoKernelPoint(-91.0, 42.0),
                new GeoKernelPoint(-83.0, 30.0),
                new GeoKernelPoint(-99.0, 22.0),
                new GeoKernelPoint(-115.0, 23.5)
            ]);

            viewerControl.AddPolygonToEditLayer(
                _polygonLayerIndex,
                ClosedVertices(),
                new Dictionary<string, object?> { ["Name"] = "Insert target" });
        }
        finally
        {
            _populating = false;
        }

        ConfigureInsertRange();
        SetTool(GeoKernelViewerTool.Select);
        RefreshMap();
        UpdateInfo();
    }

    private void BeginEditing()
    {
        if (_polygonLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_polygonLayerIndex))
            viewerControl.BeginEditLayer(_polygonLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_polygonLayerIndex);
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        viewerControl.ActiveTool = tool;
        panButton.IsChecked = tool == GeoKernelViewerTool.Pan;
        selectButton.IsChecked = tool == GeoKernelViewerTool.Select;
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        SetTool(panButton.IsChecked == true ? GeoKernelViewerTool.Pan : GeoKernelViewerTool.Select);
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        SetTool(selectButton.IsChecked == true ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan);
    }

    private void InsertVertex_Click(object sender, RoutedEventArgs e)
    {
        BeginEditing();

        if (!viewerControl.GetSelectedFeatures().Any(feature => feature.LayerIndex == _polygonLayerIndex))
        {
            UpdateStatus("Select the editable polygon first.");
            return;
        }

        var partIndex = PartIndex();
        var insertIndex = InsertIndex();
        var point = InsertionPointForSegment(insertIndex);
        if (!viewerControl.InsertSelectedFeatureVertexInEditLayer(partIndex, insertIndex, point.X, point.Y))
        {
            UpdateStatus("InsertSelectedFeatureVertexInEditLayer failed.");
            return;
        }

        _vertices.Insert(insertIndex, point);
        ConfigureInsertRange();
        insertIndexTextBox.Text = Math.Min(insertIndex + 1, _vertices.Count).ToString(CultureInfo.InvariantCulture);
        RefreshMap();
        UpdateStatus($"InsertSelectedFeatureVertexInEditLayer({partIndex}, {insertIndex}, {point.X:0.000}, {point.Y:0.000})");
    }

    private void ResetShape_Click(object sender, RoutedEventArgs e)
    {
        PopulateShape();
        UpdateStatus("Shape reset.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void InsertIndexTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (_loaded)
            UpdateInfo();
    }

    private void ViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        UpdateInfo();
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (_populating || e.LayerIndex != _polygonLayerIndex)
            return;

        UpdateInfo();
    }

    private void ConfigureInsertRange()
    {
        var index = Math.Clamp(InsertIndex(), 1, Math.Max(1, _vertices.Count));
        insertIndexTextBox.Text = index.ToString(CultureInfo.InvariantCulture);
        countText.Text = $"Vertex count: {_vertices.Count}";
    }

    private IReadOnlyList<GeoKernelPoint> ClosedVertices()
    {
        return [.. _vertices, _vertices[0]];
    }

    private GeoKernelPoint InsertionPointForSegment(int insertIndex)
    {
        if (_vertices.Count < 2)
            return new GeoKernelPoint();

        var safeIndex = Math.Clamp(insertIndex, 1, _vertices.Count);
        var a = _vertices[safeIndex - 1];
        var b = _vertices[safeIndex == _vertices.Count ? 0 : safeIndex];
        var dx = b.X - a.X;
        var dy = b.Y - a.Y;
        var length = Math.Sqrt(dx * dx + dy * dy);
        var safeLength = length > 0.0 ? length : 1.0;
        var offset = safeLength * 0.22;

        return new GeoKernelPoint(
            (a.X + b.X) * 0.5 - dy / safeLength * offset,
            (a.Y + b.Y) * 0.5 + dx / safeLength * offset);
    }

    private int PartIndex()
    {
        return int.TryParse(partTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? Math.Max(0, value)
            : 0;
    }

    private int InsertIndex()
    {
        return int.TryParse(insertIndexTextBox.Text, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? Math.Clamp(value, 1, Math.Max(1, _vertices.Count))
            : 1;
    }

    private void UpdateInfo()
    {
        if (!_loaded)
            return;

        var selected = viewerControl.GetSelectedFeatures();
        var insertIndex = InsertIndex();
        var point = InsertionPointForSegment(insertIndex);

        countText.Text = $"Vertex count: {_vertices.Count}";
        infoTextBox.Text = string.Join(
            Environment.NewLine,
            [
                "Usage:",
                "- Select: click the polygon.",
                "- Part is 0 for this sample.",
                "- Insert index means insert before that vertex index.",
                "- The sample computes a visible point near the selected segment.",
                "- Click Insert Vertex to call InsertSelectedFeatureVertexInEditLayer(part, index, point).",
                "",
                $"Selected feature count: {selected.Count}",
                $"Vertex count: {_vertices.Count}",
                $"Part index: {PartIndex()}",
                $"Insert index: {insertIndex}",
                $"Calculated point: {point.X:0.000}, {point.Y:0.000}"
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

    private static GeoKernelLayerStyle PolygonStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#F2D27A",
            FillOpacity = 140,
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
