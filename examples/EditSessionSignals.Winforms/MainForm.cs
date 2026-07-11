using GeoKernel.NET.WinForms;

namespace GeoKernel.EditSessionSignals.Winforms;

public sealed partial class MainForm : Form
{
    private const string EditableLayerName = "Session Signal Points";

    private int _editLayerIndex = -1;
    private int _editPointCursor;
    private int _startedCount;
    private int _committedCount;
    private int _rolledBackCount;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        ConnectEditSessionEvents();

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        SetSampleExtent();
        AppendLog("Ready. Waiting for edit session signals.");
        UpdateUi("Begin an edit session, add a feature, then commit or rollback.");
    }

    private void ConnectEditSessionEvents()
    {
        geoKernelViewerControl.LayerEditSessionStarted += (_, e) =>
        {
            if (e.LayerIndex != _editLayerIndex)
                return;

            ++_startedCount;
            AppendLog($"signal LayerEditSessionStarted(index={e.LayerIndex}, name={e.LayerName})");
            UpdateUi("LayerEditSessionStarted signal received.");
        };

        geoKernelViewerControl.LayerEditSessionCommitted += (_, e) =>
        {
            if (e.LayerIndex != _editLayerIndex)
                return;

            ++_committedCount;
            AppendLog($"signal LayerEditSessionCommitted(index={e.LayerIndex}, name={e.LayerName})");
            UpdateUi("LayerEditSessionCommitted signal received.");
        };

        geoKernelViewerControl.LayerEditSessionRolledBack += (_, e) =>
        {
            if (e.LayerIndex != _editLayerIndex)
                return;

            ++_rolledBackCount;
            AppendLog($"signal LayerEditSessionRolledBack(index={e.LayerIndex}, name={e.LayerName})");
            UpdateUi("LayerEditSessionRolledBack signal received.");
        };
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "EditSessionSignals");
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", "EditSessionSignals");
            return false;
        }

        geoKernelViewerControl.SetLayerName(0, "World");
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

    private void beginEditButton_Click(object sender, EventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        AppendLog($"call BeginEditLayer({_editLayerIndex})");
        if (!geoKernelViewerControl.BeginEditLayer(_editLayerIndex))
            AppendLog("result BeginEditLayer = false");
    }

    private void addFeatureButton_Click(object sender, EventArgs e)
    {
        if (_editLayerIndex < 0 || !geoKernelViewerControl.IsLayerEditing(_editLayerIndex))
            return;

        var point = GeneratedEditPoint(_editPointCursor);
        AppendLog($"call AddPointToEditLayer({_editLayerIndex}, {point.X:0.###}, {point.Y:0.###})");
        if (!geoKernelViewerControl.AddPointToEditLayer(_editLayerIndex, point.X, point.Y))
        {
            AppendLog("result AddPointToEditLayer = false");
            return;
        }

        ++_editPointCursor;
        RefreshMap();
        UpdateUi("Feature added inside the active edit session.");
    }

    private void commitEditButton_Click(object sender, EventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        AppendLog($"call CommitEditLayer({_editLayerIndex})");
        if (geoKernelViewerControl.CommitEditLayer(_editLayerIndex))
            RefreshMap();
        else
            AppendLog("result CommitEditLayer = false");
    }

    private void rollbackEditButton_Click(object sender, EventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        AppendLog($"call RollbackEditLayer({_editLayerIndex})");
        if (geoKernelViewerControl.RollbackEditLayer(_editLayerIndex))
            RefreshMap();
        else
            AppendLog("result RollbackEditLayer = false");
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void UpdateUi(string message)
    {
        var editing = _editLayerIndex >= 0 && geoKernelViewerControl.IsLayerEditing(_editLayerIndex);
        var featureCount = _editLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_editLayerIndex) : 0;

        beginEditButton.Enabled = _editLayerIndex >= 0 && !editing;
        addFeatureButton.Enabled = editing;
        commitEditButton.Enabled = editing;
        rollbackEditButton.Enabled = editing;
        stateLabel.Text = $"Editing: {(editing ? "ON" : "OFF")} | Started: {_startedCount} | Committed: {_committedCount} | Rolled back: {_rolledBackCount} | Feature count: {featureCount}";
        statusLabel.Text = message;
    }

    private void AppendLog(string text)
    {
        logTextBox.AppendText($"{DateTime.Now:HH:mm:ss.fff} | {text}{Environment.NewLine}");
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
