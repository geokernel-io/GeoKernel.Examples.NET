using GeoKernel.NET.WinForms;

namespace GeoKernel.MultiWindowSync.Winforms;

public sealed partial class MainForm : Form
{
    private bool _syncing;

    public MainForm()
    {
        InitializeComponent();
        ConnectViewSynchronization();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);

        if (!LoadLayer(leftViewerControl, "World A") ||
            !LoadLayer(rightViewerControl, "World B"))
        {
            return;
        }

        var initialExtent = new GeoKernelExtent(-151.2, 16.4, -41.6, 55.6);
        leftViewerControl.ViewExtent = initialExtent;
        rightViewerControl.ViewExtent = initialExtent;
        UpdateStatus();
    }

    private void ConnectViewSynchronization()
    {
        leftViewerControl.VisibleExtentChanged += (_, e) => SyncExtent(leftViewerControl, rightViewerControl, e.Extent);
        rightViewerControl.VisibleExtentChanged += (_, e) => SyncExtent(rightViewerControl, leftViewerControl, e.Extent);
    }

    private void SyncExtent(GeoKernelViewerControl source, GeoKernelViewerControl target, GeoKernelExtent extent)
    {
        if (!syncButton.Checked || _syncing)
            return;

        try
        {
            _syncing = true;
            target.ViewExtent = extent;
            statusLabel.Text = source == leftViewerControl
                ? "Viewer A -> Viewer B"
                : "Viewer B -> Viewer A";
        }
        finally
        {
            _syncing = false;
        }
    }

    private bool LoadLayer(GeoKernelViewerControl viewer, string layerName)
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "MultiWindowSync",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var loaded = viewer.AddLayerFile(
            path,
            new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = new GeoKernelLayerStyle
                {
                    FillColor = "#D8E5E1",
                    FillOpacity = 220,
                    LineColor = "#6F8883",
                    LineWidth = 0.8
                }
            });

        if (!loaded)
        {
            MessageBox.Show(
                this,
                $"World layer could not be loaded:{Environment.NewLine}{path}",
                "MultiWindowSync",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var layer = viewer.GetLayerInfo(0);
        if (layer is not null)
            viewer.SetLayerName(layer.Index, layerName);

        viewer.RefreshLayers();
        return true;
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        leftViewerControl.ActiveTool = tool;
        rightViewerControl.ActiveTool = tool;
        zoomRectButton.Checked = tool == GeoKernelViewerTool.ZoomBox;
        panButton.Checked = tool == GeoKernelViewerTool.Pan;
    }

    private void UpdateStatus()
    {
        statusLabel.Text = syncButton.Checked
            ? "Sync enabled. Drive either viewer."
            : "Sync disabled.";
    }

    private void zoomInButton_Click(object sender, EventArgs e)
    {
        leftViewerControl.ZoomIn();
    }

    private void zoomOutButton_Click(object sender, EventArgs e)
    {
        leftViewerControl.ZoomOut();
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        leftViewerControl.FullExtent();
    }

    private void syncButton_Click(object sender, EventArgs e)
    {
        syncButton.Text = syncButton.Checked ? "Sync On" : "Sync Off";
        if (syncButton.Checked)
        {
            try
            {
                _syncing = true;
                rightViewerControl.ViewExtent = leftViewerControl.ViewExtent;
            }
            finally
            {
                _syncing = false;
            }
        }

        UpdateStatus();
    }

    private void zoomRectButton_Click(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.ZoomBox);
    }

    private void panButton_Click(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);
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
