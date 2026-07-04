using GeoKernel.NET.WinForms;

namespace GeoKernel.AddLayers.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        var dataDirectory = Path.Combine(FindRepositoryRoot(), "data");

        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddOpenStreetMapLayer();

        if (!AddSampleLayer(Path.Combine(dataDirectory, "usa_3857.tif")))
            return;

        geoKernelViewerControl.ZoomToLayer(0);

        if (!AddSampleLayer(
            Path.Combine(dataDirectory, "usa_states_3857.shp"),
            new GeoKernelLayerStyle
            {
                FillColor = "#D8E5E1",
                FillOpacity = 140,
                LineColor = "#5F7874",
                LineWidth = 1.0
            }))
            return;

        if (!AddSampleLayer(
            Path.Combine(dataDirectory, "usa_cities_4326.kml"),
            new GeoKernelLayerStyle
            {
                PointColor = "#D95D39",
                PointSize = 8.0,
                LineColor = "#D95D39",
                LineWidth = 1.5
            }))
            return;

        SetTool(GeoKernelViewerTool.Pan);
    }

    private bool AddSampleLayer(string path, GeoKernelLayerStyle? style = null)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "AddLayers",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var loaded = style is null
            ? geoKernelViewerControl.AddLayerFile(path)
            : geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                });

        if (!loaded)
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{path}",
                "AddLayers",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        return true;
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

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "data")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
