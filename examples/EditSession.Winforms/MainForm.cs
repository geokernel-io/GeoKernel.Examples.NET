using GeoKernel.NET.WinForms;

namespace GeoKernel.EditSession.Winforms;

public sealed partial class MainForm : Form
{
    private const string EditableLayerName = "Editable Cities";

    private int _editLayerIndex = -1;
    private int _initialFeatureCount;
    private int _pendingAdds;
    private int _editPointCursor;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        SetSampleExtent();
        UpdateUi("Ready. Start an edit session, add points, then commit or rollback.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "EditSession",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
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
            MessageBox.Show(
                this,
                $"World layer could not be loaded:{Environment.NewLine}{path}",
                "EditSession",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
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
        _initialFeatureCount = FeatureCount();
        _pendingAdds = 0;
        _editPointCursor = 0;
    }

    private void beginEditButton_Click(object sender, EventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        if (geoKernelViewerControl.BeginEditLayer(_editLayerIndex))
        {
            _initialFeatureCount = FeatureCount();
            _pendingAdds = 0;
            UpdateUi("Edit session started.");
        }
        else
        {
            UpdateUi("Edit session could not be started.");
        }
    }

    private void addFeatureButton_Click(object sender, EventArgs e)
    {
        if (_editLayerIndex < 0 || !geoKernelViewerControl.IsLayerEditing(_editLayerIndex))
            return;

        var point = GeneratedEditPoint(_editPointCursor);
        if (!geoKernelViewerControl.AddPointToEditLayer(_editLayerIndex, point.X, point.Y))
        {
            UpdateUi("Feature could not be added to the active edit session.");
            return;
        }

        ++_editPointCursor;
        ++_pendingAdds;
        RefreshMap();
        UpdateUi("Feature added inside the active edit session.");
    }

    private void commitEditButton_Click(object sender, EventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        if (geoKernelViewerControl.CommitEditLayer(_editLayerIndex))
        {
            _initialFeatureCount = FeatureCount();
            _pendingAdds = 0;
            RefreshMap();
            UpdateUi("Edit session committed. Added features remain in the layer.");
        }
        else
        {
            UpdateUi("Edit session could not be committed.");
        }
    }

    private void rollbackEditButton_Click(object sender, EventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        if (geoKernelViewerControl.RollbackEditLayer(_editLayerIndex))
        {
            _pendingAdds = 0;
            RefreshMap();
            UpdateUi("Edit session rolled back. Uncommitted features were removed.");
        }
        else
        {
            UpdateUi("Edit session could not be rolled back.");
        }
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void UpdateUi(string message)
    {
        var editing = _editLayerIndex >= 0 && geoKernelViewerControl.IsLayerEditing(_editLayerIndex);
        var dirty = _editLayerIndex >= 0 && geoKernelViewerControl.IsLayerDirty(_editLayerIndex);
        var featureCount = FeatureCount();

        beginEditButton.Enabled = !editing;
        addFeatureButton.Enabled = editing;
        commitEditButton.Enabled = editing;
        rollbackEditButton.Enabled = editing;

        editStateLabel.Text = $"Editing: {(editing ? "ON" : "OFF")} | Dirty: {(dirty ? "YES" : "NO")} | Feature count: {featureCount} | Pending adds: {_pendingAdds}";
        statusLabel.Text = message;
    }

    private int FeatureCount()
    {
        return _editLayerIndex >= 0
            ? geoKernelViewerControl.GetLayerInfo(_editLayerIndex)?.FeatureCount ?? geoKernelViewerControl.GetLayerFeatureCount(_editLayerIndex)
            : 0;
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
        var column = index % 11;
        var row = index / 11 % 6;
        var cycle = index / 66;

        return new GeoKernelPoint(
            -124.0 + column * 5.6 + cycle * 0.35,
            25.0 + row * 4.2 + cycle * 0.35);
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
            PointColor = "#D85B35",
            LineColor = "#8C321D",
            PointSize = 9.0,
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
