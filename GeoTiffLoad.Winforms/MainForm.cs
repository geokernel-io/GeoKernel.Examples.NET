using GeoKernel.NET.WinForms;

namespace GeoKernel.GeoTiffLoad.Winforms;

public sealed partial class MainForm : Form
{
    private string _rasterPath = string.Empty;
    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        _rasterPath = await SampleData.EnsureFileAsync("world_8km_tif.zip", "world_8km_tif", "world_8km.tif", "World GeoTIFF", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(_rasterPath))
            return;
        LoadSample();
    }

    private void zoomInButton_Click(object? sender, EventArgs e) => viewerControl.ZoomIn();
    private void zoomOutButton_Click(object? sender, EventArgs e) => viewerControl.ZoomOut();
    private void zoomRectButton_Click(object? sender, EventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.ZoomBox;
    private void panButton_Click(object? sender, EventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

    private void primaryButton_Click(object? sender, EventArgs e) => LoadSample();
    private void secondaryButton_Click(object? sender, EventArgs e) => viewerControl.FullExtent();

    private void LoadSample()
    {
        viewerControl.ClearLayers();
        try
        {
            if (!File.Exists(_rasterPath))
                throw new FileNotFoundException("GeoTIFF sample data could not be found.", _rasterPath);

            if (!viewerControl.AddLayerFile(_rasterPath))
                throw new InvalidOperationException($"GeoTIFF could not be loaded: {_rasterPath}");

            var layer = viewerControl.GetLayerInfo(0)
                ?? throw new InvalidOperationException("GeoTIFF layer information could not be read.");
            var extent = layer.ProjectedExtent;
            var file = new FileInfo(_rasterPath);

            detailsTextBox.Text = string.Join(Environment.NewLine,
                "GeoTIFF load sample",
                "",
                "File",
                $"Path: {layer.Path}",
                $"Exists: {(file.Exists ? "yes" : "no")}",
                $"Size: {(file.Exists ? file.Length : 0)} bytes",
                "",
                "Raster",
                $"Layer: {layer.Name}",
                $"Open: {(layer.IsOpen ? "yes" : "no")}",
                $"EPSG: {(layer.CoordinateSystem.EpsgCode == 0 ? "unknown" : layer.CoordinateSystem.EpsgCode)}",
                $"Coordinate system: {layer.CoordinateSystem.Name}",
                $"Projected extent: ({extent.XMin:F2}, {extent.YMin:F2}) - ({extent.XMax:F2}, {extent.YMax:F2})",
                "",
                "SDK flow",
                "viewerControl.AddLayerFile(path);",
                "viewerControl.GetLayerInfo(index);",
                "viewerControl.FullExtent();");

            viewerControl.FullExtent();
            statusLabel.Text = "GeoTIFF loaded: world_8km.tif";
        }
        catch (Exception ex)
        {
            detailsTextBox.Text = ex.Message;
            statusLabel.Text = "GeoTiffLoad failed.";
            MessageBox.Show(this, ex.Message, "GeoTiffLoad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, value =>
    {
        statusLabel.Text = value.Message;
        downloadProgressBar.Visible = true;
        downloadProgressBar.Style = value.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (value.Percentage.HasValue)
            downloadProgressBar.Value = Math.Clamp(value.Percentage.Value, 0, 100);
    });

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
