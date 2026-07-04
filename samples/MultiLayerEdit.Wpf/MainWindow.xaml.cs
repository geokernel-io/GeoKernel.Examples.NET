using System.IO;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.MultiLayerEdit.Wpf;

public partial class MainWindow
{
    private const string RedLayerName = "Red Points";
    private const string BlueLayerName = "Blue Points";

    private int _redLayerIndex = -1;
    private int _blueLayerIndex = -1;
    private int _redCursor;
    private int _blueCursor;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreateEditLayers();
        BeginBothLayers();
        SetActiveLayer(_redLayerIndex);
        SetSampleExtent();
        UpdateUi("Switch active edit layer, then add points to that layer.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
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

    private void CreateEditLayers()
    {
        _redLayerIndex = viewerControl.AddEmptyVectorLayer(RedLayerName, GeoKernelShapeType.Point, PointStyle("#D95D39", "#8C321D"));
        _redLayerIndex = viewerControl.GetLayerInfoByName(RedLayerName)?.Index ?? _redLayerIndex;

        _blueLayerIndex = viewerControl.AddEmptyVectorLayer(BlueLayerName, GeoKernelShapeType.Point, PointStyle("#2563EB", "#1E3A8A"));
        _blueLayerIndex = viewerControl.GetLayerInfoByName(BlueLayerName)?.Index ?? _blueLayerIndex;
    }

    private void BeginBothLayers()
    {
        BeginLayer(_redLayerIndex);
        BeginLayer(_blueLayerIndex);
    }

    private void BeginLayer(int layerIndex)
    {
        if (layerIndex >= 0 && !viewerControl.IsLayerEditing(layerIndex))
            viewerControl.BeginEditLayer(layerIndex);
    }

    private void SetActiveLayer(int layerIndex)
    {
        BeginBothLayers();
        if (layerIndex >= 0)
            viewerControl.SetActiveEditLayerIndex(layerIndex);

        redLayerButton.IsChecked = layerIndex == _redLayerIndex;
        blueLayerButton.IsChecked = layerIndex == _blueLayerIndex;
        UpdateUi($"SetActiveEditLayerIndex({layerIndex})");
    }

    private void RedLayer_Click(object sender, RoutedEventArgs e)
    {
        SetActiveLayer(_redLayerIndex);
    }

    private void BlueLayer_Click(object sender, RoutedEventArgs e)
    {
        SetActiveLayer(_blueLayerIndex);
    }

    private void AddToActiveLayer_Click(object sender, RoutedEventArgs e)
    {
        BeginBothLayers();

        var activeIndex = viewerControl.ActiveEditLayerIndex;
        if (activeIndex != _redLayerIndex && activeIndex != _blueLayerIndex)
        {
            UpdateUi("No active edit layer.");
            return;
        }

        var redActive = activeIndex == _redLayerIndex;
        var point = redActive ? RedPointAt(_redCursor) : BluePointAt(_blueCursor);
        var nextNumber = redActive ? _redCursor + 1 : _blueCursor + 1;
        var layerName = redActive ? RedLayerName : BlueLayerName;
        var attributes = new Dictionary<string, object?>
        {
            ["Name"] = $"{layerName} {nextNumber}",
            ["Layer"] = layerName
        };

        if (!viewerControl.AddPointToEditLayer(activeIndex, point.X, point.Y, attributes))
        {
            UpdateUi($"AddPointToEditLayer({activeIndex}, ...) failed.");
            return;
        }

        if (redActive)
            ++_redCursor;
        else
            ++_blueCursor;

        RefreshMap();
        UpdateUi($"Added point to active layer: {layerName}.");
    }

    private void CommitBoth_Click(object sender, RoutedEventArgs e)
    {
        CommitIfEditing(_redLayerIndex);
        CommitIfEditing(_blueLayerIndex);
        BeginBothLayers();
        SetActiveLayer(redLayerButton.IsChecked == true ? _redLayerIndex : _blueLayerIndex);
        RefreshMap();
        UpdateUi("Both edit layers committed and reopened for editing.");
    }

    private void RollbackBoth_Click(object sender, RoutedEventArgs e)
    {
        ResetLayers("Both edit layers rolled back.");
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        ResetLayers("Both edit layers reset. Red Points is active.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex == _redLayerIndex || e.LayerIndex == _blueLayerIndex)
            UpdateUi("Layer edit state changed.");
    }

    private void CommitIfEditing(int layerIndex)
    {
        if (layerIndex >= 0 && viewerControl.IsLayerEditing(layerIndex))
            viewerControl.CommitEditLayer(layerIndex);
    }

    private void RollbackIfEditing(int layerIndex)
    {
        if (layerIndex >= 0 && viewerControl.IsLayerEditing(layerIndex))
            viewerControl.RollbackEditLayer(layerIndex);
    }

    private void ResetLayers(string message)
    {
        RollbackIfEditing(_redLayerIndex);
        RollbackIfEditing(_blueLayerIndex);
        viewerControl.RemoveLayerByName(RedLayerName);
        viewerControl.RemoveLayerByName(BlueLayerName);
        _redCursor = 0;
        _blueCursor = 0;
        _redLayerIndex = -1;
        _blueLayerIndex = -1;
        CreateEditLayers();
        BeginBothLayers();
        SetActiveLayer(_redLayerIndex);
        RefreshMap();
        UpdateUi(message);
    }

    private void UpdateUi(string message)
    {
        var activeIndex = viewerControl.ActiveEditLayerIndex;
        var activeName = activeIndex == _redLayerIndex ? RedLayerName : activeIndex == _blueLayerIndex ? BlueLayerName : "-";
        var redCount = _redLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_redLayerIndex) : 0;
        var blueCount = _blueLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_blueLayerIndex) : 0;

        stateText.Text = $"Active edit layer: {activeName} ({activeIndex}) | Red: {redCount} | Blue: {blueCount}";
        infoTextBox.Text = string.Join(Environment.NewLine,
            "MultiLayerEdit sample",
            "",
            "Workflow:",
            "1. Red Points and Blue Points are both editing.",
            "2. Active layer buttons call SetActiveEditLayerIndex(index).",
            "3. Add To Active Layer writes to the current active edit layer index.",
            "4. Commit Both commits both edit sessions and reopens them.",
            "5. Rollback Both discards uncommitted additions.",
            "",
            $"ActiveEditLayerIndex: {activeIndex}",
            $"Active layer: {activeName}",
            $"Red layer index: {_redLayerIndex}",
            $"Blue layer index: {_blueLayerIndex}",
            $"Red feature count: {redCount}",
            $"Blue feature count: {blueCount}",
            "",
            "APIs:",
            "BeginEditLayer(index)",
            "SetActiveEditLayerIndex(index)",
            "ActiveEditLayerIndex",
            "AddPointToEditLayer(activeIndex, x, y, attributes)",
            "CommitEditLayer(index)",
            "RollbackEditLayer(index)");
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

    private static GeoKernelPoint RedPointAt(int index)
    {
        const double xMin = -124.0;
        const double yMin = 31.0;
        const double xStep = 7.5;
        const double yStep = 5.0;
        const int columns = 7;
        return new GeoKernelPoint(xMin + index % columns * xStep, yMin + index / columns * yStep);
    }

    private static GeoKernelPoint BluePointAt(int index)
    {
        const double xMin = -121.5;
        const double yMin = 33.0;
        const double xStep = 7.5;
        const double yStep = 5.0;
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

    private static GeoKernelLayerStyle PointStyle(string pointColor, string lineColor)
    {
        return new GeoKernelLayerStyle
        {
            PointColor = pointColor,
            LineColor = lineColor,
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
}
