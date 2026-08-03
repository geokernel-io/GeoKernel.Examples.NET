using GeoKernel.NET.WinForms;

namespace GeoKernel.AddLayers.Winforms;

public sealed partial class MainForm : Form
{
    private const string SampleDataBaseUrl =
        "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/";

    public MainForm()
    {
        InitializeComponent();
        SetTool(GeoKernelViewerTool.Pan);
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        var progress = new ControlProgress<SampleDataProgress>(this, UpdateDownloadProgress);
        geoKernelViewerControl.ClearLayers();

        if (!await AddSampleLayerAsync(
                "world_8km_png.zip",
                "world_8km_png",
                "world_8km.png",
                "World raster",
                progress))
            return;

        if (!await AddSampleLayerAsync(
                "world_4326.zip",
                "world_4326",
                "world_4326.shp",
                "Countries",
                progress,
                new GeoKernelLayerStyle
                {
                    FillColor = "#35475B",
                    FillOpacity = 172,
                    LineColor = "#B7E8FF",
                    LineWidth = 0.85
                }))
            return;

        if (!await AddSampleLayerAsync(
                "world_cities_4326.zip",
                "world_cities_4326",
                "world_cities_4326.shp",
                "Cities",
                progress,
                new GeoKernelLayerStyle
                {
                    PointColor = "#1D8FC7",
                    PointSize = 4.2,
                    LineColor = "#74C3E8",
                    LineWidth = 0.9
                }))
            return;

        geoKernelViewerControl.FullExtent();
        downloadProgressBar.Visible = false;
        statusLabel.Text = "Raster, country and city layers loaded.";
    }

    private async Task<bool> AddSampleLayerAsync(
        string archiveName,
        string extractFolderName,
        string requiredFileName,
        string displayName,
        IProgress<SampleDataProgress> progress,
        GeoKernelLayerStyle? style = null)
    {
        statusLabel.Text = $"Preparing {displayName}...";
        var path = await SampleData.EnsureSampleFileAsync(
            new Uri($"{SampleDataBaseUrl}{archiveName}"),
            archiveName,
            extractFolderName,
            requiredFileName,
            displayName,
            this,
            progress);

        if (string.IsNullOrWhiteSpace(path))
        {
            FinishWithError($"{displayName} could not be prepared.");
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
            FinishWithError($"{displayName} could not be loaded.");
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, displayName);

        statusLabel.Text = $"{displayName} loaded.";
        return true;
    }

    private void UpdateDownloadProgress(SampleDataProgress progress)
    {
        statusLabel.Text = progress.Message;
        downloadProgressBar.Visible = true;

        if (progress.Percentage.HasValue)
        {
            downloadProgressBar.Style = ProgressBarStyle.Continuous;
            downloadProgressBar.Value = Math.Clamp(progress.Percentage.Value, 0, 100);
        }
        else
        {
            downloadProgressBar.Style = ProgressBarStyle.Marquee;
        }
    }

    private void FinishWithError(string message)
    {
        downloadProgressBar.Visible = false;
        statusLabel.Text = message;
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

    private void zoomInButton_Click(object sender, EventArgs e) => geoKernelViewerControl.ZoomIn();

    private void zoomOutButton_Click(object sender, EventArgs e) => geoKernelViewerControl.ZoomOut();

    private void fullExtentButton_Click(object sender, EventArgs e) => geoKernelViewerControl.FullExtent();

    private void zoomRectButton_Click(object sender, EventArgs e) => SetTool(GeoKernelViewerTool.ZoomBox);

    private void panButton_Click(object sender, EventArgs e) => SetTool(GeoKernelViewerTool.Pan);

    private sealed class ControlProgress<T>(Control control, Action<T> callback) : IProgress<T>
    {
        public void Report(T value)
        {
            if (control.IsDisposed)
                return;

            if (control.InvokeRequired)
                control.Invoke(() => callback(value));
            else
                callback(value);
        }
    }
}
