using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AddWithAttributes.Wpf;

public partial class MainWindow
{
    private const string PointLayerName = "Points With Attributes";

    private readonly ObservableCollection<FeatureRow> _rows = [];
    private int _pointLayerIndex = -1;
    private int _pointCursor;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        attributesGrid.ItemsSource = _rows;
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.SelectionChanged += ViewerControl_SelectionChanged;

        if (!LoadLayer())
            return;

        CreatePointLayer();
        BeginPointEditing();
        SetSampleExtent();
        UpdateInfoText("Click Add Point With Attributes, then use Info and click an added point.");
        UpdateStatus("AddPointToEditLayer(index, x, y, attributes) sample.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "AddWithAttributes",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = WorldStyle()
                }))
        {
            MessageBox.Show(
                this,
                $"World layer could not be loaded:{Environment.NewLine}{path}",
                "AddWithAttributes",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            viewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreatePointLayer()
    {
        _pointLayerIndex = viewerControl.AddEmptyVectorLayer(
            PointLayerName,
            GeoKernelShapeType.Point,
            PointStyle());

        _pointLayerIndex = viewerControl.GetLayerInfoByName(PointLayerName)?.Index ?? _pointLayerIndex;
        if (_pointLayerIndex < 0)
            MessageBox.Show(this, "Point layer could not be created.", "AddWithAttributes", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void BeginPointEditing()
    {
        if (_pointLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_pointLayerIndex))
            viewerControl.BeginEditLayer(_pointLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_pointLayerIndex);
        UpdateFeatureCount();
    }

    private void AddPoint_Click(object sender, RoutedEventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        BeginPointEditing();

        var featureNo = _pointCursor + 1;
        var point = SamplePointAt(_pointCursor);
        var attributes = CreateAttributes(featureNo);

        if (!viewerControl.AddPointToEditLayer(_pointLayerIndex, point.X, point.Y, attributes))
        {
            UpdateStatus("Point with attributes could not be added.");
            return;
        }

        _pointCursor++;
        var row = new FeatureRow(featureNo, attributes["Name"]?.ToString() ?? "", attributes["Category"]?.ToString() ?? "", attributes["Score"]?.ToString() ?? "", attributes["Source"]?.ToString() ?? "");
        _rows.Add(row);
        SelectGridRow(featureNo);
        RefreshMap();
        UpdateFeatureCount();
        UpdateStatus($"AddPointToEditLayer({_pointLayerIndex}, {point.X:F4}, {point.Y:F4}, attributes)");
    }

    private void Info_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = infoButton.IsChecked == true ? GeoKernelViewerTool.Info : GeoKernelViewerTool.Pan;
        UpdateStatus(infoButton.IsChecked == true ? "Info mode: click an added point to read attributes." : "Pan mode.");
    }

    private void ClearPoints_Click(object sender, RoutedEventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        viewerControl.RollbackEditLayer(_pointLayerIndex);
        _pointCursor = 0;
        _rows.Clear();
        BeginPointEditing();
        RefreshMap();
        UpdateInfoText("Click Add Point With Attributes, then use Info and click an added point.");
        UpdateStatus("Points with attributes cleared.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (viewerControl.ActiveTool != GeoKernelViewerTool.Info)
            return;

        var position = e.GetPosition(viewerControl);
        var result = viewerControl.HitTestTopFeatureAt(position.X, position.Y, 8);
        if (result is null || !result.IsValid)
        {
            attributesGrid.SelectedItem = null;
            UpdateInfoText("No feature found.");
            UpdateStatus("No feature found under cursor.");
            return;
        }

        UpdateInfoText(FormatFeatureAttributes(result));
        if (result.LayerIndex == _pointLayerIndex)
            SelectGridRow(result.ShapeId);

        UpdateStatus($"Attributes read from layer '{result.LayerName}', feature {result.ShapeId}.");
    }

    private void ViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        var selected = viewerControl.GetSelectedFeatures().FirstOrDefault(feature => feature.LayerIndex == _pointLayerIndex);
        if (selected is not null)
            SelectGridRow(selected.ShapeId);
    }

    private void SelectGridRow(int shapeId)
    {
        var row = _rows.FirstOrDefault(row => row.ShapeId == shapeId);
        attributesGrid.SelectedItem = row;
        if (row is not null)
            attributesGrid.ScrollIntoView(row);
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

    private void UpdateFeatureCount()
    {
        var count = _pointLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_pointLayerIndex) : 0;
        featureCountText.Text = $"Feature count: {count}";
    }

    private void UpdateInfoText(string text)
    {
        infoTextBox.Text = text;
    }

    private void UpdateStatus(string text)
    {
        statusText.Text = text;
    }

    private static GeoKernelPoint SamplePointAt(int index)
    {
        const double xMin = -123.0;
        const double yMin = 29.0;
        const double xStep = 5.0;
        const double yStep = 4.0;
        const int columns = 12;

        return new GeoKernelPoint(xMin + index % columns * xStep, yMin + index / columns * yStep);
    }

    private static Dictionary<string, object?> CreateAttributes(int featureNo)
    {
        return new Dictionary<string, object?>
        {
            ["Name"] = $"Site {featureNo}",
            ["Category"] = featureNo % 2 == 0 ? "Even" : "Odd",
            ["Score"] = featureNo * 10,
            ["Source"] = ".NET Dictionary"
        };
    }

    private static string FormatFeatureAttributes(GeoKernelFeatureHitTestResult result)
    {
        var lines = new List<string>
        {
            $"Layer: {result.LayerName}",
            $"Shape ID: {result.ShapeId}",
            $"Feature ID: {result.FeatureId}",
            ""
        };

        foreach (var pair in result.Attributes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            lines.Add($"{pair.Key} = {pair.Value}");

        return string.Join(Environment.NewLine, lines);
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
            PointSize = 9.5,
            LineWidth = 1.2,
            ShowLabels = true,
            LabelField = "Name",
            LabelFontSize = 10.0,
            LabelColor = "#263238",
            LabelHaloEnabled = true,
            LabelHaloColor = "#FFFFFF",
            LabelHaloWidth = 2.0,
            LabelOffsetY = -11.0,
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

    private sealed record FeatureRow(int ShapeId, string Name, string Category, string Score, string Source);
}
