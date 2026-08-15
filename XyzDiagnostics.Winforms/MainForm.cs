using GeoKernel.NET.WinForms;

namespace GeoKernel.XyzDiagnostics.Winforms;

public sealed partial class MainForm : Form
{
    private const string OSM_URL = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
    private readonly System.Windows.Forms.Timer diagnosticsTimer = new() { Interval = 750 };
    private int xyzLayerIndex = -1;

    public MainForm()
    {
        InitializeComponent();
        diagnosticsTimer.Tick += (_, _) => RefreshDiagnostics();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        try
        {
            var cacheDirectory = Path.Combine(AppContext.BaseDirectory, "XyzDiagnosticsCache", "osm");
            xyzLayerIndex = viewerControl.AddXyzLayer(
                "OSM Diagnostics", OSM_URL, 0, 19, 256, "OpenStreetMap", true, cacheDirectory);
            if (xyzLayerIndex < 0)
                throw new InvalidOperationException("XYZ diagnostics layer could not be created.");

            ShowDefaultExtent();
            RefreshDiagnostics();
            diagnosticsTimer.Start();
        }
        catch (Exception ex)
        {
            detailsTextBox.Text = $"XYZ diagnostics layer could not be loaded:{Environment.NewLine}{ex.Message}";
            MessageBox.Show(this, detailsTextBox.Text, "XyzDiagnostics", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void zoomInButton_Click(object? sender, EventArgs e) => viewerControl.ZoomIn();
    private void zoomOutButton_Click(object? sender, EventArgs e) => viewerControl.ZoomOut();
    private void zoomRectButton_Click(object? sender, EventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.ZoomBox;
    private void panButton_Click(object? sender, EventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
    private void secondaryButton_Click(object? sender, EventArgs e) => ShowDefaultExtent();
    private void refreshButton_Click(object? sender, EventArgs e) => RefreshDiagnostics();

    private void ShowDefaultExtent() =>
        viewerControl.ViewExtent = new GeoKernelExtent(-1400000.0, 4100000.0, 4200000.0, 7800000.0);

    private void RefreshDiagnostics()
    {
        if (xyzLayerIndex < 0)
        {
            detailsTextBox.Text = "XYZ layer is not available.";
            return;
        }

        try
        {
            var snapshot = viewerControl.GetXyzLayerDiagnostics(xyzLayerIndex);
            if (snapshot is null)
            {
                detailsTextBox.Text = "XYZ layer diagnostics are not available.";
                return;
            }

            detailsTextBox.Text = DetailsText(snapshot);
            statusLabel.Text = $"XYZ diagnostics: {snapshot.DownloadsStarted} requests, " +
                $"{snapshot.DownloadsSucceeded} downloads, {snapshot.DiskHits} disk hits, " +
                $"{snapshot.MemoryHits} memory hits";
        }
        catch (Exception ex)
        {
            diagnosticsTimer.Stop();
            detailsTextBox.Text = $"Diagnostics could not be read:{Environment.NewLine}{ex.Message}";
        }
    }

    private static string DetailsText(GeoKernelXyzLayerDiagnostics value)
    {
        var memoryTotal = value.MemoryHits + value.MemoryMisses;
        var diskTotal = value.DiskHits + value.DiskMisses;
        var downloadTotal = value.DownloadsSucceeded + value.DownloadsFailed;
        return string.Join(Environment.NewLine,
            "XYZ diagnosticsSnapshot sample", $"Updated: {DateTime.Now:HH:mm:ss.fff}", "",
            "Layer", $"Name: {value.Name}", $"URL template: {value.UrlTemplate}",
            $"Tile size: {value.TileSize}", $"Zoom range: {value.MinZoom} - {value.MaxZoom}",
            $"Local cache: {(value.LocalCacheEnabled ? "enabled" : "disabled")}", $"Cache directory: {value.CacheDirectory}", "",
            "Memory cache", $"Hits: {value.MemoryHits}", $"Misses: {value.MemoryMisses}", $"Total lookups: {memoryTotal}", "",
            "Disk cache", $"Hits: {value.DiskHits}", $"Misses: {value.DiskMisses}", $"Total lookups: {diskTotal}",
            $"Read time total: {value.DiskReadMs} ms", $"Decode time total: {value.DecodeMs} ms",
            $"Average read: {Average(value.DiskReadMs, value.DiskHits)}", $"Average decode: {Average(value.DecodeMs, value.DiskHits)}", "",
            "Network", $"Downloads started: {value.DownloadsStarted}", $"Downloads succeeded: {value.DownloadsSucceeded}",
            $"Downloads failed: {value.DownloadsFailed}", $"Downloads completed: {downloadTotal}",
            $"Bytes downloaded: {Bytes(value.BytesDownloaded)}", $"Download time total: {value.DownloadMs} ms",
            $"Average download: {Average(value.DownloadMs, value.DownloadsSucceeded)}", $"Queue depth: {value.NetworkQueueDepth}",
            $"Max queue depth: {value.MaxNetworkQueueDepth}", "", "How to test",
            "- Pan or zoom the map to request new tiles.", "- First pass usually increases downloads and disk misses.",
            "- Revisit the same area to see memory/disk cache hits.");
    }

    private static string Average(long total, long count) => count == 0 ? "0.00 ms" : $"{(double)total / count:F2} ms";
    private static string Bytes(long count) => $"{count} bytes ({count / (1024.0 * 1024.0):F2} MiB)";

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        diagnosticsTimer.Stop();
        diagnosticsTimer.Dispose();
        base.OnFormClosed(e);
    }
}
