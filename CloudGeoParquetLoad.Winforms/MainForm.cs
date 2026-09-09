using System.Text.Json;
using GeoKernel.NET.WinForms;

namespace GeoKernel.CloudGeoParquetLoad.Winforms;

public sealed class MainForm : Form
{
    readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    readonly TextBox url = new() { Text = "https://raw.githubusercontent.com/opengeospatial/geoparquet/main/examples/example.parquet" };
    readonly Button load = new() { Text = "Probe and stream GeoParquet" };
    readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100, Height = 20, Visible = false };
    readonly TextBox details = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both };
    readonly ToolStripStatusLabel status = new();

    public MainForm()
    {
        Text = "CloudGeoParquetLoad"; Width = 1280; Height = 820; Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "resources", "GeoKernelAppIcon.ico")); ApplyGdalOptions();
        var right = new Panel { Dock = DockStyle.Right, Width = 390, Padding = new Padding(10) };
        var header = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 145, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        Control Full(Control c) { c.Width = 365; c.Margin = new Padding(0, 2, 0, 2); return c; }
        header.Controls.Add(new Label { Text = "Cloud GeoParquet streaming", AutoSize = true, Font = new Font(Font, FontStyle.Bold) });
        header.Controls.Add(new Label { Text = "Remote GeoParquet URL", AutoSize = true }); header.Controls.Add(Full(url)); header.Controls.Add(Full(load)); header.Controls.Add(Full(progress));
        right.Controls.Add(details); right.Controls.Add(new Label { Text = "Cloud diagnostics", Dock = DockStyle.Top, Height = 22 }); right.Controls.Add(header);
        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        tools.Items.Add("Zoom In", null, (_, _) => viewer.ZoomIn()); tools.Items.Add("Zoom Out", null, (_, _) => viewer.ZoomOut()); tools.Items.Add("Full Extent", null, (_, _) => viewer.FullExtent()); tools.Items.Add("Pan", null, (_, _) => viewer.ActiveTool = GeoKernelViewerTool.Pan);
        var bar = new StatusStrip(); bar.Items.Add(status); Controls.Add(viewer); Controls.Add(right); Controls.Add(tools); Controls.Add(bar); viewer.ActiveTool = GeoKernelViewerTool.Pan;
        viewer.BusyChanged += (_, e) => BeginInvoke(() => ShowRenderProgress(e.Busy)); load.Click += async (_, _) => await Run(); Shown += async (_, _) => await Run(); details.Text = "Ready.";
    }

    async Task Run()
    {
        if (!Uri.TryCreate(url.Text.Trim(), UriKind.Absolute, out var remote) || (remote.Scheme != "http" && remote.Scheme != "https")) { MessageBox.Show(this, "Enter a valid HTTP or HTTPS URL.", Text); return; }
        load.Enabled = false; SetProgress(10, "Probing the remote GeoParquet header and footer...");
        try
        {
            var result = await Task.Run(() => Probe(remote.ToString())); SetProgress(60, "Opening GeoParquet layer..."); await Task.Yield();
            viewer.ClearLayers(); if (!viewer.AddLayerFile(result.Path)) throw new InvalidOperationException("The remote GeoParquet layer could not be opened."); viewer.RefreshLayers(); viewer.ZoomToLayer(0);
            details.Text = Report(result.Probe); progress.Value = 100; status.Text = "GeoParquet is streaming through HTTP byte ranges.";
        }
        catch (Exception ex) { progress.Style = ProgressBarStyle.Continuous; progress.Value = 0; details.Text = "Load failed:\r\n" + ex.Message; status.Text = "Cloud GeoParquet load failed."; MessageBox.Show(this, ex.Message, Text); }
        finally { load.Enabled = true; }
    }

    static Result Probe(string remote)
    {
        using var cloud = new GeoKernelCloudClient(new { maximumMemoryBytes = 64L * 1024 * 1024, maximumDiskBytes = 1024L * 1024 * 1024 }); cloud.SetTimeout(30000);
        var probe = cloud.ProbeGeoParquet(remote); if (!probe.GetProperty("cloudReadable").GetBoolean()) throw new InvalidOperationException(probe.GetProperty("diagnostic").GetString());
        return new(cloud.GeoParquetGdalVirtualPath(remote), probe.Clone());
    }

    void SetProgress(int value, string text) { progress.Visible = true; progress.Style = ProgressBarStyle.Continuous; progress.Value = Math.Clamp(value, 0, 100); status.Text = text; details.Text = text; }
    void ShowRenderProgress(bool busy) { if (!load.Enabled) return; progress.Visible = true; if (busy) { progress.Style = ProgressBarStyle.Marquee; status.Text = "Rendering map..."; } else { progress.Style = ProgressBarStyle.Continuous; progress.Value = 100; status.Text = "Map ready."; } }
    static void ApplyGdalOptions() { Environment.SetEnvironmentVariable("GDAL_DISABLE_READDIR_ON_OPEN", "EMPTY_DIR"); Environment.SetEnvironmentVariable("CPL_VSIL_CURL_ALLOWED_EXTENSIONS", ".parquet,.pmtiles"); Environment.SetEnvironmentVariable("GDAL_CACHEMAX", "256"); Environment.SetEnvironmentVariable("VSI_CACHE", "TRUE"); Environment.SetEnvironmentVariable("VSI_CACHE_SIZE", "67108864"); Environment.SetEnvironmentVariable("GDAL_HTTP_CONNECTTIMEOUT", "10"); Environment.SetEnvironmentVariable("GDAL_HTTP_TIMEOUT", "30"); }
    static string Report(JsonElement p) => $"Cloud GeoParquet streaming\r\n\r\nURL: {p.GetProperty("url").GetString()}\r\nContent length: {p.GetProperty("contentLength").GetInt64()} bytes\r\nContent type: {p.GetProperty("contentType").GetString()}\r\nAccept-Ranges: {(p.GetProperty("acceptsRanges").GetBoolean() ? "yes" : "no")}\r\nPAR1 header: {(p.GetProperty("headerValid").GetBoolean() ? "valid" : "invalid")}\r\nPAR1 footer: {(p.GetProperty("footerValid").GetBoolean() ? "valid" : "invalid")}\r\nGDAL source: /vsicurl/\r\n\r\n{p.GetProperty("diagnostic").GetString()}\r\n\r\nOnly metadata and requested byte ranges are transferred; the complete GeoParquet file is not downloaded.";
    sealed record Result(string Path, JsonElement Probe);
}
