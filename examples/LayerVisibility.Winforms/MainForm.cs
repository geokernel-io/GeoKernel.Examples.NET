using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerVisibility.Winforms;

public sealed partial class MainForm : Form
{
    private bool _refreshingLayerList;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
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
                "LayerVisibility",
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
                "LayerVisibility",
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
                layerListBox.Items.Add(LayerListText(layer));

            if (selectedIndex >= 0 && selectedIndex < layerListBox.Items.Count)
                layerListBox.SelectedIndex = selectedIndex;
            else if (layerListBox.Items.Count > 0)
                layerListBox.SelectedIndex = 0;
        }
        finally
        {
            _refreshingLayerList = false;
        }

        UpdateVisibilityButton();
        UpdateStatus();
    }

    private void ToggleSelectedLayerVisibility()
    {
        var index = layerListBox.SelectedIndex;
        if (index < 0)
            return;

        var layer = geoKernelViewerControl.GetLayerInfo(index);
        if (layer is null)
            return;

        if (!geoKernelViewerControl.SetLayerVisible(index, !layer.Visible))
            return;

        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        RefreshLayerList(index);
    }

    private void UpdateVisibilityButton()
    {
        var index = layerListBox.SelectedIndex;
        var layer = index >= 0 ? geoKernelViewerControl.GetLayerInfo(index) : null;
        if (layer is null)
        {
            visibilityButton.Enabled = false;
            visibilityButton.Text = "Change Visibility";
            return;
        }

        visibilityButton.Enabled = true;
        visibilityButton.Text = layer.Visible
            ? "Change Visibility: Hide"
            : "Change Visibility: Show";
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

    private void visibilityButton_Click(object sender, EventArgs e)
    {
        ToggleSelectedLayerVisibility();
    }

    private void layerListBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!_refreshingLayerList)
            UpdateVisibilityButton();
    }

    private static string LayerListText(GeoKernelLayerInfo layer)
    {
        return $"{(layer.Visible ? "[x]" : "[ ]")} {layer.DisplayText}";
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
