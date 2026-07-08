using GeoKernel.NET.WinForms;

namespace GeoKernel.Wgs84Setup.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        geoKernelViewerControl.MouseCoordinatesChanged += geoKernelViewerControl_MouseCoordinatesChanged;
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        detailsTextBox.Text = Wgs84Details();

        if (!LoadLayer())
            return;

        SetWorldExtent();
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        statusLabel.Text = "Move the mouse over the map to inspect screen/world coordinates.";
    }

    private bool LoadLayer()
    {
        var shapefilePath = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be found:{Environment.NewLine}{shapefilePath}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                shapefilePath,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = WorldStyle()
                }))
        {
            MessageBox.Show(
                this,
                $"World layer could not be loaded:{Environment.NewLine}{shapefilePath}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var worldLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            geoKernelViewerControl.SetLayerName(worldLayer.Index, "World - EPSG:4326");

        return true;
    }

    private void SetWorldExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-180.0, -85.0, 180.0, 85.0);
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        SetWorldExtent();
    }

    private void geoKernelViewerControl_MouseCoordinatesChanged(object? sender, GeoKernelMouseCoordinatesChangedEventArgs e)
    {
        statusLabel.Text =
            $"Screen: {e.ScreenPoint.X:0}, {e.ScreenPoint.Y:0} | World: {e.WorldPoint.X:0.000000}, {e.WorldPoint.Y:0.000000}";
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

    private static string Wgs84Details()
    {
        return string.Join(
            Environment.NewLine,
            "KnownCoordinateSystems::wgs84()",
            "",
            "Coordinate system",
            "EPSG: 4326",
            "Name: WGS 84",
            "Type: Geographic",
            "",
            "Datum",
            "EPSG: 6326",
            "Name: World Geodetic System 1984",
            "Ellipsoid: WGS 84",
            "",
            "Angular unit",
            "Name: degree",
            "Axes:",
            "1. Longitude (Lon), direction: east",
            "2. Latitude (Lat), direction: north",
            "",
            "WinForms wrapper usage",
            "geoKernelViewerControl.AddLayerFile(\"world_4326.shp\", options)",
            "geoKernelViewerControl.ScreenToWorld(screenX, screenY)",
            "",
            "Mouse status shows longitude/latitude degrees.");
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
