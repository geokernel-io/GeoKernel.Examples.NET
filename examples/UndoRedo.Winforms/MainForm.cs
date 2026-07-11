using GeoKernel.NET.WinForms;

namespace GeoKernel.UndoRedo.Winforms;

public sealed partial class MainForm : Form
{
    private const string PointLayerName = "Undo Redo Points";

    private int _pointLayerIndex = -1;
    private int _clickSteps;
    private bool _addPointMode = true;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

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
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var worldLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            geoKernelViewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreatePointLayer()
    {
        _pointLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(PointLayerName, GeoKernelShapeType.Point, PointStyle());
        _pointLayerIndex = geoKernelViewerControl.GetLayerInfoByName(PointLayerName)?.Index ?? _pointLayerIndex;
    }

    private void BeginPointEditing()
    {
        if (_pointLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_pointLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_pointLayerIndex);

        geoKernelViewerControl.SetActiveEditLayerIndex(_pointLayerIndex);
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _pointLayerIndex)
            return;

        if (_addPointMode)
            _clickSteps = Math.Max(_clickSteps, FeatureCount());

        RefreshMap();
        UpdateStatus(_addPointMode ? "Point edit command created. Keep clicking or use Undo/Redo." : "Undo/redo state changed.");
    }

    private void addPointButton_Click(object? sender, EventArgs e)
    {
        _addPointMode = true;
        addPointButton.Checked = true;
        panButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        BeginPointEditing();
        UpdateStatus("Add Point active. Click the map to create undoable edit commands.");
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        _addPointMode = false;
        addPointButton.Checked = false;
        panButton.Checked = true;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan active.");
    }

    private void undoButton_Click(object? sender, EventArgs e)
    {
        RunUndo(1);
    }

    private void redoButton_Click(object? sender, EventArgs e)
    {
        RunRedo(1);
    }

    private void undoFiveButton_Click(object? sender, EventArgs e)
    {
        RunUndo(5);
    }

    private void redoFiveButton_Click(object? sender, EventArgs e)
    {
        RunRedo(5);
    }

    private void resetButton_Click(object? sender, EventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_pointLayerIndex);
        _clickSteps = 0;
        BeginPointEditing();
        RefreshMap();
        UpdateStatus("Reset complete. Click the map to create a fresh undo stack.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void RunUndo(int maxSteps)
    {
        var count = 0;
        for (var i = 0; i < maxSteps; ++i)
        {
            if (!geoKernelViewerControl.UndoEditLayer(_pointLayerIndex))
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
            if (!geoKernelViewerControl.RedoEditLayer(_pointLayerIndex))
                break;

            ++count;
        }

        RefreshMap();
        UpdateStatus(count > 0 ? $"RedoEditLayer({_pointLayerIndex}) succeeded {count} time(s)." : "Nothing to redo.");
    }

    private void RefreshMap()
    {
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-132.0, 18.0, -60.0, 55.0);
    }

    private void UpdateStatus(string message)
    {
        var pointCount = FeatureCount();
        var canUndo = _pointLayerIndex >= 0 && geoKernelViewerControl.CanUndoEditLayer(_pointLayerIndex);
        var canRedo = _pointLayerIndex >= 0 && geoKernelViewerControl.CanRedoEditLayer(_pointLayerIndex);

        undoButton.Enabled = canUndo;
        undoFiveButton.Enabled = canUndo;
        redoButton.Enabled = canRedo;
        redoFiveButton.Enabled = canRedo;

        stateLabel.Text = $"Points: {pointCount} | Click steps: {_clickSteps} | Undo: {YesNo(canUndo)} | Redo: {YesNo(canRedo)}";
        statusLabel.Text = message;
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
        return _pointLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_pointLayerIndex) : 0;
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
