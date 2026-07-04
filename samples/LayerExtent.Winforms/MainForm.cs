using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerExtent.Winforms;

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

        if (!LoadLayer())
            return;

        if (!AddLayerExtentRectangle(0))
            return;

        geoKernelViewerControl.FullExtent();
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "california", "california.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "LayerExtent",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var style = new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 210,
            LineColor = "#6F8883",
            LineWidth = 0.9
        };

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
                "LayerExtent",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, "California");

        return true;
    }

    private bool AddLayerExtentRectangle(int layerIndex)
    {
        var extent = geoKernelViewerControl.GetLayerProjectedExtent(layerIndex);
        if (extent.XMax <= extent.XMin || extent.YMax <= extent.YMin)
        {
            MessageBox.Show(
                this,
                "Layer extent could not be calculated.",
                "LayerExtent",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        GeoKernelPoint[] rectangle =
        [
            new(extent.XMin, extent.YMin),
            new(extent.XMax, extent.YMin),
            new(extent.XMax, extent.YMax),
            new(extent.XMin, extent.YMax),
            new(extent.XMin, extent.YMin)
        ];

        var style = new GeoKernelLayerStyle
        {
            FillColor = "#FFFFFF",
            FillOpacity = 0,
            LineColor = "#E2453D",
            LineWidth = 2.2
        };

        var index = geoKernelViewerControl.AddPolygonLayer("Layer Extent", rectangle, style);
        if (index < 0)
        {
            MessageBox.Show(
                this,
                "Layer extent rectangle could not be created.",
                "LayerExtent",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        statusLabel.Text = $"California extent: {extent.XMin:0.###}, {extent.YMin:0.###} - {extent.XMax:0.###}, {extent.YMax:0.###}";
        return true;
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
