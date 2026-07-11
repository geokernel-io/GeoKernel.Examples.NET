using GeoKernel.NET.WinForms;

namespace GeoKernel.EditDirtyState.Winforms;

public sealed partial class MainForm : Form
{
    private const string EditableLayerName = "Dirty State Points";

    private int _editLayerIndex = -1;
    private int _editPointCursor;
    private int _editStateSignalCount;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        SetSampleExtent();
        AppendLog($"Ready. Initial isLayerDirty={IsDirtyText()}");
        UpdateUi("Use Add Feature to turn isLayerDirty on, then commit or rollback.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "EditDirtyState", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = WorldStyle()
                }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", "EditDirtyState", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var worldLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            geoKernelViewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreateEditableLayer()
    {
        _editLayerIndex = geoKernelViewerControl.AddPointLayer(
            EditableLayerName,
            [
                new GeoKernelPoint(-122.4194, 37.7749),
                new GeoKernelPoint(-118.2437, 34.0522),
                new GeoKernelPoint(-112.0740, 33.4484)
            ],
            EditPointStyle());

        _editLayerIndex = geoKernelViewerControl.GetLayerInfoByName(EditableLayerName)?.Index ?? _editLayerIndex;
    }

    private void beginEditButton_Click(object? sender, EventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        if (geoKernelViewerControl.BeginEditLayer(_editLayerIndex))
        {
            AppendLog($"BeginEditLayer({_editLayerIndex}); isLayerDirty={IsDirtyText()}");
            UpdateUi("Edit session started. Dirty remains false until a change is made.");
        }
        else
        {
            UpdateUi("BeginEditLayer returned false.");
        }
    }

    private void addFeatureButton_Click(object? sender, EventArgs e)
    {
        if (_editLayerIndex < 0 || !geoKernelViewerControl.IsLayerEditing(_editLayerIndex))
            return;

        var point = GeneratedEditPoint(_editPointCursor++);
        if (geoKernelViewerControl.AddPointToEditLayer(_editLayerIndex, point.X, point.Y))
        {
            RefreshMap();
            AppendLog($"AddPointToEditLayer({_editLayerIndex}); isLayerDirty={IsDirtyText()}");
            UpdateUi("Feature added. isLayerDirty(index) is now true.");
        }
        else
        {
            UpdateUi("AddPointToEditLayer returned false.");
        }
    }

    private void commitEditButton_Click(object? sender, EventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        if (geoKernelViewerControl.CommitEditLayer(_editLayerIndex))
        {
            RefreshMap();
            AppendLog($"CommitEditLayer({_editLayerIndex}); isLayerDirty={IsDirtyText()}");
            UpdateUi("Edit committed. isLayerDirty(index) returned to false.");
        }
        else
        {
            UpdateUi("CommitEditLayer returned false.");
        }
    }

    private void rollbackEditButton_Click(object? sender, EventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        if (geoKernelViewerControl.RollbackEditLayer(_editLayerIndex))
        {
            RefreshMap();
            AppendLog($"RollbackEditLayer({_editLayerIndex}); isLayerDirty={IsDirtyText()}");
            UpdateUi("Edit rolled back. isLayerDirty(index) returned to false.");
        }
        else
        {
            UpdateUi("RollbackEditLayer returned false.");
        }
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _editLayerIndex)
            return;

        _editStateSignalCount++;
        AppendLog($"signal LayerEditStateChanged({e.LayerName}); isLayerDirty={IsDirtyText()}; editing={IsEditingText()}");
        UpdateUi("layerEditStateChanged signal received.");
    }

    private void UpdateUi(string message)
    {
        var editing = _editLayerIndex >= 0 && geoKernelViewerControl.IsLayerEditing(_editLayerIndex);
        var dirty = _editLayerIndex >= 0 && geoKernelViewerControl.IsLayerDirty(_editLayerIndex);
        var featureCount = FeatureCount();

        beginEditButton.Enabled = _editLayerIndex >= 0 && !editing;
        addFeatureButton.Enabled = editing;
        commitEditButton.Enabled = editing;
        rollbackEditButton.Enabled = editing;
        editStateLabel.Text = $"Layer index: {_editLayerIndex} | Editing: {(editing ? "ON" : "OFF")} | Dirty: {(dirty ? "YES" : "NO")} | Signals: {_editStateSignalCount} | Feature count: {featureCount}";
        statusLabel.Text = message;
    }

    private int FeatureCount()
    {
        return _editLayerIndex >= 0
            ? geoKernelViewerControl.GetLayerInfo(_editLayerIndex)?.FeatureCount ?? geoKernelViewerControl.GetLayerFeatureCount(_editLayerIndex)
            : 0;
    }

    private string IsDirtyText()
    {
        return _editLayerIndex >= 0 && geoKernelViewerControl.IsLayerDirty(_editLayerIndex) ? "true" : "false";
    }

    private string IsEditingText()
    {
        return _editLayerIndex >= 0 && geoKernelViewerControl.IsLayerEditing(_editLayerIndex) ? "true" : "false";
    }

    private void AppendLog(string message)
    {
        logTextBox.AppendText($"{DateTime.Now:HH:mm:ss.fff} | {message}{Environment.NewLine}");
    }

    private void RefreshMap()
    {
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 20.0, -65.0, 52.0);
    }

    private static GeoKernelPoint GeneratedEditPoint(int index)
    {
        var column = index % 8;
        var row = index / 8 % 4;
        return new GeoKernelPoint(-124.0 + column * 7.5, 25.0 + row * 5.2);
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

    private static GeoKernelLayerStyle EditPointStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            LineColor = "#8C321D",
            PointSize = 9.5,
            LineWidth = 1.2
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
