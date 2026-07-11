using GeoKernel.NET.WinForms;

namespace GeoKernel.Minimap.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {        
        var shapefilePath = Path.Combine(FindRepositoryRoot(), "data", "world_4326.shp");

        if (!File.Exists(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be found:{Environment.NewLine}{shapefilePath}",
                "Minimap",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (!geoKernelViewerControl.AddLayerFile(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be loaded:{Environment.NewLine}{shapefilePath}",
                "Minimap",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        ConfigureMiniMap();
        geoKernelViewerControl.FullExtent();
    }

    private void ConfigureMiniMap()
    {
        geoKernelViewerControl.MiniMapVisible = true;
        geoKernelViewerControl.SetMiniMapAnchor(GeoKernelOverlayAnchor.TopRight);
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
