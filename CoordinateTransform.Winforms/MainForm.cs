using GeoKernel.NET.WinForms;

namespace GeoKernel.CoordinateTransform.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        geoKernelViewerControl.MapMouseMove += geoKernelViewerControl_MapMouseMove;
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.Wgs84);

        var path = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        if (string.IsNullOrEmpty(path) || !LoadLayer(path))
            return;

        SetWorldExtent();
        downloadProgressBar.Visible = false;
        statusLabel.Text = "Move the mouse over the map. The status bar shows EPSG:4326 -> EPSG:3857.";
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
            geoKernelViewerControl.SetLayerName(worldLayer.Index, "World - EPSG:4326");

        return true;
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        SetWorldExtent();
    }

    private void SetWorldExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-180.0, -85.0, 180.0, 85.0);
    }

    private void geoKernelViewerControl_MapMouseMove(object? sender, GeoKernelMapMouseEventArgs e)
    {
        var mercator = ToWebMercator(e.WorldPoint.X, e.WorldPoint.Y);
        statusLabel.Text =
            $"Screen: {e.ScreenPoint.X:0}, {e.ScreenPoint.Y:0} | " +
            $"EPSG:4326 lon/lat: {e.WorldPoint.X:0.000000}, {e.WorldPoint.Y:0.000000} | " +
            $"EPSG:3857 meters: {mercator.X:0.00}, {mercator.Y:0.00}";
    }

    private static (double X, double Y) ToWebMercator(double longitude, double latitude)
    {
        const double earthRadius = 6378137.0;
        var clampedLatitude = Math.Clamp(latitude, -85.05112878, 85.05112878);
        var x = earthRadius * longitude * Math.PI / 180.0;
        var y = earthRadius * Math.Log(Math.Tan(Math.PI / 4.0 + clampedLatitude * Math.PI / 360.0));
        return (x, y);
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
