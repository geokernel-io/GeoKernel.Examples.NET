using GeoKernel.NET.WinForms;

namespace GeoKernel.HelloMap.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);

        var shapefilePath = Path.Combine(FindRepositoryRoot(), "data", "world_4326.shp");

        if (!File.Exists(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be found:{Environment.NewLine}{shapefilePath}",
                "HelloMap",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (!geoKernelViewerControl.AddLayerFile(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be loaded:{Environment.NewLine}{shapefilePath}",
                "HelloMap",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        geoKernelViewerControl.FullExtent();
        SetTool(GeoKernelViewerTool.Pan);
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
