using GeoKernel.NET.WinForms;

namespace GeoKernel.Scalebar.Winforms;

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
                "Scalebar",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (!geoKernelViewerControl.AddLayerFile(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be loaded:{Environment.NewLine}{shapefilePath}",
                "Scalebar",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        ConfigureScaleBar();

        geoKernelViewerControl.FullExtent();
    }

    private void ConfigureScaleBar()
    {
        geoKernelViewerControl.ScaleBarVisible = true;
        geoKernelViewerControl.SetScaleBarAnchor(GeoKernelOverlayAnchor.BottomRight);
        geoKernelViewerControl.SetScaleBarColors(
            Color.FromArgb(235, 255, 255, 255),
            Color.FromArgb(50, 74, 72),
            Color.FromArgb(35, 50, 48));
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
