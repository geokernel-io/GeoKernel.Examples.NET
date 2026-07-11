using System.IO;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.UndoRedo.Wpf;

public partial class MainWindow
{
    private const string PointLayerName = "Undo Redo Points";

    private int _pointLayerIndex = -1;
    private int _clickSteps;
    private bool _addPointMode = true;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePointLayer();
        BeginPointEditing();
        SetSampleExtent();
        UpdateStatus("Add Point active. Click the map several times, then use Undo/Redo.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "UndoRedo", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", "UndoRedo", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            viewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreatePointLayer()
    {
        _pointLayerIndex = viewerControl.AddEmptyVectorLayer(PointLayerName, GeoKernelShapeType.Point, PointStyle());
        _pointLayerIndex = viewerControl.GetLayerInfoByName(PointLayerName)?.Index ?? _pointLayerIndex;
    }

    private void BeginPointEditing()
    {
        if (_pointLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_pointLayerIndex))
            viewerControl.BeginEditLayer(_pointLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_pointLayerIndex);
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _pointLayerIndex)
            return;

        if (_addPointMode)
            _clickSteps = Math.Max(_clickSteps, FeatureCount());

        RefreshMap();
        UpdateStatus(_addPointMode ? "Point edit command created. Keep clicking or use Undo/Redo." : "Undo/redo state changed.");
    }

    private void AddPoint_Click(object sender, RoutedEventArgs e)
    {
        _addPointMode = true;
        addPointButton.IsChecked = true;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        BeginPointEditing();
        UpdateStatus("Add Point active. Click the map to create undoable edit commands.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        _addPointMode = false;
        addPointButton.IsChecked = false;
        panButton.IsChecked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan active.");
    }

    private void Undo_Click(object sender, RoutedEventArgs e)
    {
        RunUndo(1);
    }

    private void Redo_Click(object sender, RoutedEventArgs e)
    {
        RunRedo(1);
    }

    private void UndoFive_Click(object sender, RoutedEventArgs e)
    {
        RunUndo(5);
    }

    private void RedoFive_Click(object sender, RoutedEventArgs e)
    {
        RunRedo(5);
    }

    private void Reset_Click(object sender, RoutedEventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        viewerControl.RollbackEditLayer(_pointLayerIndex);
        _clickSteps = 0;
        BeginPointEditing();
        RefreshMap();
        UpdateStatus("Reset complete. Click the map to create a fresh undo stack.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void RunUndo(int maxSteps)
    {
        var count = 0;
        for (var i = 0; i < maxSteps; ++i)
        {
            if (!viewerControl.UndoEditLayer(_pointLayerIndex))
                break;

            ++count;
        }

        RefreshMap();
        UpdateStatus(count > 0 ? $"UndoEditLayer({_pointLayerIndex}) succeeded {count} time(s)." : "Nothing to undo.");
    }

    private void RunRedo(int maxSteps)
    {
        var count = 0;
        for (var i = 0; i < maxSteps; ++i)
        {
            if (!viewerControl.RedoEditLayer(_pointLayerIndex))
                break;

            ++count;
        }

        RefreshMap();
        UpdateStatus(count > 0 ? $"RedoEditLayer({_pointLayerIndex}) succeeded {count} time(s)." : "Nothing to redo.");
    }

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-132.0, 18.0, -60.0, 55.0);
    }

    private void UpdateStatus(string message)
    {
        var pointCount = FeatureCount();
        var canUndo = _pointLayerIndex >= 0 && viewerControl.CanUndoEditLayer(_pointLayerIndex);
        var canRedo = _pointLayerIndex >= 0 && viewerControl.CanRedoEditLayer(_pointLayerIndex);

        undoButton.IsEnabled = canUndo;
        undoFiveButton.IsEnabled = canUndo;
        redoButton.IsEnabled = canRedo;
        redoFiveButton.IsEnabled = canRedo;

        stateText.Text = $"Points: {pointCount} | Click steps: {_clickSteps} | Undo: {YesNo(canUndo)} | Redo: {YesNo(canRedo)}";
        statusText.Text = message;
        infoTextBox.Text = string.Join(
            Environment.NewLine,
            [
                "Interactive undo/redo:",
                "- Add Point is active by default.",
                "- Click the map several times to create separate edit commands.",
                "- Undo calls UndoEditLayer(index).",
                "- Redo calls RedoEditLayer(index).",
                "- Undo 5 / Redo 5 call the same API repeatedly.",
                "",
                $"Layer index: {_pointLayerIndex}",
                $"Visible point count: {pointCount}",
                $"Click steps created: {_clickSteps}",
                $"Can undo: {canUndo}",
                $"Can redo: {canRedo}"
            ]);
    }

    private int FeatureCount()
    {
        return _pointLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_pointLayerIndex) : 0;
    }

    private static string YesNo(bool value)
    {
        return value ? "yes" : "no";
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
            PointSize = 13.0,
            LineWidth = 1.4
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
