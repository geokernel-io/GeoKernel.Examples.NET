using GeoKernel.NET.WinForms;

namespace GeoKernel.XyzLocalCache.Winforms;

public sealed partial class MainForm : Form
{
    private const string Url = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
    private static readonly GeoKernelExtent DefaultExtent = new(-1400000, 4100000, 4200000, 7800000);
    public MainForm() => InitializeComponent();

    private void MainForm_Shown(object? s, EventArgs e)
    {
        cachePathTextBox.Text = Path.Combine(AppContext.BaseDirectory, "XyzLocalCacheData", "osm");
        cacheCheckBox.Checked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        ApplyCache();
    }
    private void zoomInButton_Click(object? s, EventArgs e) => viewerControl.ZoomIn();
    private void zoomOutButton_Click(object? s, EventArgs e) => viewerControl.ZoomOut();
    private void zoomRectButton_Click(object? s, EventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.ZoomBox;
    private void panButton_Click(object? s, EventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
    private void secondaryButton_Click(object? s, EventArgs e) => viewerControl.ViewExtent = DefaultExtent;
    private void applyButton_Click(object? s, EventArgs e) => ApplyCache();
    private void refreshButton_Click(object? s, EventArgs e) => UpdateDetails();
    private void browseButton_Click(object? s, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog { SelectedPath = CacheDirectory(), Description = "Select XYZ cache directory" };
        if (dialog.ShowDialog(this) == DialogResult.OK) cachePathTextBox.Text = dialog.SelectedPath;
    }
    private void clearButton_Click(object? s, EventArgs e)
    {
        var path = CacheDirectory();
        if (MessageBox.Show(this, $"Clear all cached tiles under:\n{path}", "XyzLocalCache", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        if (Directory.Exists(path)) Directory.Delete(path, true);
        UpdateDetails(); viewerControl.Invalidate(); statusLabel.Text = "Cache directory cleared.";
    }
    private void ApplyCache()
    {
        try
        {
            var path = CacheDirectory(); Directory.CreateDirectory(path); cachePathTextBox.Text = path;
            viewerControl.ClearLayers();
            var index = viewerControl.AddXyzLayer("OSM with Local Cache", Url, 0, 19, 256, "OpenStreetMap contributors", cacheCheckBox.Checked, path);
            if (index < 0) throw new InvalidOperationException("XYZ layer could not be loaded.");
            viewerControl.ViewExtent = DefaultExtent; UpdateDetails();
            statusLabel.Text = cacheCheckBox.Checked ? "XYZ layer loaded with local disk cache." : "XYZ layer loaded with local cache disabled.";
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "XyzLocalCache", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }
    private string CacheDirectory() => Path.GetFullPath(string.IsNullOrWhiteSpace(cachePathTextBox.Text) ? Path.Combine(AppContext.BaseDirectory, "XyzLocalCacheData", "osm") : cachePathTextBox.Text.Trim());
    private void UpdateDetails()
    {
        var path = CacheDirectory(); var files = Directory.Exists(path) ? Directory.EnumerateFiles(path, "*.tile", SearchOption.AllDirectories).ToArray() : [];
        var bytes = files.Sum(file => new FileInfo(file).Length);
        detailsTextBox.Text = string.Join(Environment.NewLine, "XYZ local cache sample", "", "URL template:", Url, "",
            $"Local cache: {(cacheCheckBox.Checked ? "enabled" : "disabled")}", "Configured cache directory:", path, "",
            "Cache contents:", $"Tile files: {files.Length}", $"Size: {FormatBytes(bytes)}", "", "SDK flow:",
            "viewerControl.AddXyzLayer(..., localCacheEnabled, cacheDirectory)", "", "Pan or zoom the map to request tiles. Cached tiles are reused on later runs.");
    }
    private static string FormatBytes(long value) => value >= 1048576 ? $"{value / 1048576d:F2} MB" : value >= 1024 ? $"{value / 1024d:F1} KB" : $"{value} bytes";
}
