using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerReorder.Winforms;

public sealed partial class MainForm : Form
{
    private bool _refreshingLayerList;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        SetTool(GeoKernelViewerTool.Pan);

        if (!LoadLayers())
            return;

        RefreshLayerList();
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-151.2, 16.4, -41.6, 55.6);
    }

    private bool LoadLayers()
    {
        var dataDirectory = Path.Combine(FindRepositoryRoot(), "assets", "data");

        return AddLayer(
                "World",
                Path.Combine(dataDirectory, "world_4326.shp"),
                new GeoKernelLayerStyle
                {
                    FillColor = "#D8E5E1",
                    FillOpacity = 220,
                    LineColor = "#7B918D",
                    LineWidth = 0.8
                })
            && AddLayer(
                "States",
                Path.Combine(dataDirectory, "us_state_boundaries.shp"),
                new GeoKernelLayerStyle
                {
                    FillColor = "#A9C8DB",
                    FillOpacity = 115,
                    LineColor = "#356780",
                    LineWidth = 1.2
                })
            && AddLayer(
                "Cities",
                Path.Combine(dataDirectory, "usa_cities_4326.kml"),
                new GeoKernelLayerStyle
                {
                    PointColor = "#D95D39",
                    PointSize = 7.0,
                    LineColor = "#D95D39",
                    LineWidth = 1.5
                });
    }

    private bool AddLayer(string name, string path, GeoKernelLayerStyle style)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "LayerReorder",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                }))
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{path}",
                "LayerReorder",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var info = geoKernelViewerControl.GetLayerInfo(0);
        if (info is not null)
            geoKernelViewerControl.SetLayerName(info.Index, name);

        return true;
    }

    private void RefreshLayerList(int selectedIndex = -1)
    {
        _refreshingLayerList = true;
        try
        {
            layerListBox.Items.Clear();

            foreach (var layer in geoKernelViewerControl.GetLayersInfo())
                layerListBox.Items.Add(layer.DisplayText);

            if (selectedIndex >= 0 && selectedIndex < layerListBox.Items.Count)
                layerListBox.SelectedIndex = selectedIndex;
            else if (layerListBox.Items.Count > 0)
                layerListBox.SelectedIndex = 0;
        }
        finally
        {
            _refreshingLayerList = false;
        }

        UpdateButtons();
    }

    private void MoveSelectedLayer(int delta)
    {
        var fromIndex = layerListBox.SelectedIndex;
        if (fromIndex < 0)
            return;

        var toIndex = fromIndex + delta;
        if (toIndex < 0 || toIndex >= geoKernelViewerControl.LayerCount)
            return;

        if (!geoKernelViewerControl.MoveLayer(fromIndex, toIndex))
            return;

        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        RefreshLayerList(toIndex);
        UpdateStatus();
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        geoKernelViewerControl.ActiveTool = tool;
        zoomRectButton.BackColor = tool == GeoKernelViewerTool.ZoomBox
            ? Color.FromArgb(200, 230, 255)
            : SystemColors.Control;
        panButton.BackColor = tool == GeoKernelViewerTool.Pan
            ? Color.FromArgb(200, 230, 255)
            : SystemColors.Control;
    }

    private void UpdateButtons()
    {
        var selectedIndex = layerListBox.SelectedIndex;
        moveUpButton.Enabled = selectedIndex > 0;
        moveDownButton.Enabled = selectedIndex >= 0 && selectedIndex < layerListBox.Items.Count - 1;
    }

    private void UpdateStatus()
    {
        statusLabel.Text = $"Layers: {geoKernelViewerControl.LayerCount}";
    }

    private void zoomInButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ZoomIn();
    }

    private void zoomOutButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ZoomOut();
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.FullExtent();
    }

    private void zoomRectButton_Click(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.ZoomBox);
    }

    private void panButton_Click(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);
    }

    private void moveUpButton_Click(object sender, EventArgs e)
    {
        MoveSelectedLayer(-1);
    }

    private void moveDownButton_Click(object sender, EventArgs e)
    {
        MoveSelectedLayer(1);
    }

    private void layerListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!_refreshingLayerList)
            UpdateButtons();
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
