using GeoKernel.Examples.Common;
using GeoKernel.NET.WinForms;

namespace GeoKernel.AddLayers.Winforms;

public sealed partial class MainForm : Form
{
    private const string SampleDataBaseUrl = "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/";
    private const string RasterLayerName = "World Raster";
    private const string PolygonLayerName = "Countries";
    private const string CityLayerName = "Cities";

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddOpenStreetMapLayer();

        if (!AddSampleLayer(
            "world_8km_png.zip",
            "world_8km_png",
            "world_8km.png",
            RasterLayerName))
            return;

        if (!AddSampleLayer(
            "world_4326.zip",
            "world_4326",
            "world_4326.shp",
            PolygonLayerName,
            new GeoKernelLayerStyle
            {
                FillColor = "#35475B",
                FillOpacity = 172,
                LineColor = "#B7E8FF",
                LineWidth = 0.85,
                LabelColor = "#FFFFFF",
                LabelHaloColor = "#10263A"
            }))
            return;

        if (!AddSampleLayer(
            "world_cities_4326.zip",
            "world_cities_4326",
            "world_cities_4326.shp",
            CityLayerName,
            new GeoKernelLayerStyle
            {
                PointColor = "#1D8FC7",
                PointSize = 4.2,
                LineColor = "#B9E6F5",
                LineWidth = 0.9
            }))
            return;

        ZoomToLayer(RasterLayerName);
        SetTool(GeoKernelViewerTool.Pan);
    }

    private bool AddSampleLayer(string archiveName, string extractFolderName, string requiredFileName, string displayName, GeoKernelLayerStyle? style = null)
    {
        var path = SampleData.EnsureSampleFile(
            new Uri($"{SampleDataBaseUrl}{archiveName}"),
            archiveName,
            extractFolderName,
            requiredFileName,
            this);

        if (string.IsNullOrWhiteSpace(path))
            return false;

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

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void ZoomToLayer(string layerName)
    {
        var layer = geoKernelViewerControl.GetLayerInfoByName(layerName);
        if (layer is null || !geoKernelViewerControl.ZoomToLayer(layer.Index))
            geoKernelViewerControl.FullExtent();
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

}
