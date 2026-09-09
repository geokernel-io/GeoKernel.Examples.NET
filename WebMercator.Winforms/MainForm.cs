using GeoKernel.NET.WinForms;

namespace GeoKernel.WebMercator.Winforms;

public sealed partial class MainForm : Form
{
    private const double OriginShift = 20037508.342789244;

    public MainForm()
    {
        InitializeComponent();
        geoKernelViewerControl.MapMouseMove += geoKernelViewerControl_MapMouseMove;
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        detailsTextBox.Text = WebMercatorDetails();

        var path = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        if (string.IsNullOrEmpty(path) || !LoadLayer(path))
            return;

        geoKernelViewerControl.SetLayerCoordinateSystemPreset(0, GeoKernelCoordinateSystemPreset.Wgs84);
        geoKernelViewerControl.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.WebMercator);
        SetWorldExtent();
        downloadProgressBar.Visible = false;
        statusLabel.Text = "Move the mouse over the map to inspect EPSG:3857 meter coordinates.";
    }

    private bool LoadLayer(string shapefilePath)
    {
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
            geoKernelViewerControl.SetLayerName(worldLayer.Index, "World countries - source EPSG:4326");

        return true;
    }

    private void CreateCityLayer()
    {
        var layerIndex = geoKernelViewerControl.AddEmptyVectorLayer(
            "City points - EPSG:3857",
            GeoKernelShapeType.Point,
            CityStyle());

        if (layerIndex < 0)
            return;

        geoKernelViewerControl.BeginEditLayer(layerIndex);
        geoKernelViewerControl.SetActiveEditLayerIndex(layerIndex);

        foreach (var city in Cities())
        {
            var point = ToWebMercator(city.Longitude, city.Latitude);
            geoKernelViewerControl.AddPointToEditLayer(layerIndex, point.X, point.Y);
        }

        geoKernelViewerControl.CommitEditLayer(layerIndex);
        geoKernelViewerControl.RefreshLayers();
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        SetWorldExtent();
    }

    private void SetWorldExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-OriginShift, -OriginShift, OriginShift, OriginShift);
    }

    private void geoKernelViewerControl_MapMouseMove(object? sender, GeoKernelMapMouseEventArgs e)
    {
        statusLabel.Text =
            $"Screen: {e.ScreenPoint.X:0}, {e.ScreenPoint.Y:0} | WebMercator meters: {e.WorldPoint.X:0.00}, {e.WorldPoint.Y:0.00}";
    }

    private static GeoKernelPoint ToWebMercator(double longitude, double latitude)
    {
        var clampedLatitude = Math.Clamp(latitude, -85.05112878, 85.05112878);
        var x = longitude * OriginShift / 180.0;
        var y = Math.Log(Math.Tan((90.0 + clampedLatitude) * Math.PI / 360.0)) / (Math.PI / 180.0);
        y *= OriginShift / 180.0;
        return new GeoKernelPoint(x, y);
    }

    private static IEnumerable<(string Name, double Longitude, double Latitude)> Cities()
    {
        yield return ("Istanbul", 28.9784, 41.0082);
        yield return ("London", -0.1276, 51.5072);
        yield return ("New York", -74.0060, 40.7128);
        yield return ("Tokyo", 139.6917, 35.6895);
        yield return ("Sydney", 151.2093, -33.8688);
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

    private static GeoKernelLayerStyle CityStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            LineColor = "#8C321D",
            PointSize = 10.0,
            LineWidth = 1.2
        };
    }

    private static string WebMercatorDetails()
    {
        return string.Join(
            Environment.NewLine,
            "KnownCoordinateSystems::webMercator()",
            "",
            "Coordinate system",
            "EPSG: 3857",
            "Name: WGS 84 / Pseudo-Mercator",
            "Type: Projected",
            "",
            "Base geographic coordinate system",
            "EPSG: 4326",
            "Name: WGS 84",
            "",
            "Projection",
            "Name: Popular Visualisation Pseudo Mercator",
            "Method: WebMercator",
            "",
            "Linear unit",
            "Name: Meter",
            "Meters per unit: 1",
            "",
            "Axes",
            "1. Easting (E), direction: east",
            "2. Northing (N), direction: north",
            "",
            "WinForms sample setup",
            "geoKernelViewerControl.AddLayerFile(\"world_4326.shp\", options)",
            "geoKernelViewerControl.SetLayerCoordinateSystemPreset(0, Wgs84)",
            "geoKernelViewerControl.SetCoordinateSystemPreset(WebMercator)",
            "",
            "The EPSG:4326 source layer is reprojected on the fly to EPSG:3857.");
    }

    private IProgress<SampleDataProgress> CreateSampleProgress()
    {
        return new ControlProgress<SampleDataProgress>(this, progress =>
        {
            statusLabel.Text = progress.Message;
            downloadProgressBar.Visible = true;
            downloadProgressBar.Style = progress.Percentage.HasValue ? ProgressBarStyle.Continuous : ProgressBarStyle.Marquee;
            if (progress.Percentage.HasValue) downloadProgressBar.Value = progress.Percentage.Value;
        });
    }
}
