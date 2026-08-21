using System.Text.Json;
using GeoKernel.NET.WinForms;

namespace GeoKernel.CloudPmTilesLoad.Winforms;

public sealed class MainForm : Form
{
    readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    readonly TextBox url = new() { Text = "https://pmtiles.io/protomaps(vector)ODbL_firenze.pmtiles" };
    readonly Button load = new() { Text = "Probe and stream PMTiles" };
    readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100, Height = 20, Visible = false };
    readonly TextBox details = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both };
    readonly ToolStripStatusLabel status = new();

    public MainForm()
    {
        Text = "CloudPmTilesLoad"; Width = 1280; Height = 820; Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "resources", "GeoKernelAppIcon.ico")); ApplyGdalOptions();
        var right = new Panel { Dock = DockStyle.Right, Width = 390, Padding = new Padding(10) };
        var header = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 145, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        Control Full(Control c) { c.Width = 365; c.Margin = new Padding(0, 2, 0, 2); return c; }
        header.Controls.Add(new Label { Text = "Cloud PMTiles streaming", AutoSize = true, Font = new Font(Font, FontStyle.Bold) });
        header.Controls.Add(new Label { Text = "Remote PMTiles URL", AutoSize = true }); header.Controls.Add(Full(url)); header.Controls.Add(Full(load)); header.Controls.Add(Full(progress));
        right.Controls.Add(details); right.Controls.Add(new Label { Text = "Cloud diagnostics", Dock = DockStyle.Top, Height = 22 }); right.Controls.Add(header);
        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        tools.Items.Add("Zoom In", null, (_, _) => viewer.ZoomIn()); tools.Items.Add("Zoom Out", null, (_, _) => viewer.ZoomOut()); tools.Items.Add("Full Extent", null, (_, _) => viewer.FullExtent()); tools.Items.Add("Pan", null, (_, _) => viewer.ActiveTool = GeoKernelViewerTool.Pan);
        var bar = new StatusStrip(); bar.Items.Add(status); Controls.Add(viewer); Controls.Add(right); Controls.Add(tools); Controls.Add(bar); viewer.ActiveTool = GeoKernelViewerTool.Pan;
        viewer.BusyChanged += (_, e) => BeginInvoke(() => ShowRenderProgress(e.Busy)); load.Click += async (_, _) => await Run(); Shown += async (_, _) => await Run(); details.Text = "Ready.";
    }

    async Task Run()
    {
        if (!Uri.TryCreate(url.Text.Trim(), UriKind.Absolute, out var remote) || (remote.Scheme != "http" && remote.Scheme != "https")) { MessageBox.Show(this, "Enter a valid HTTP or HTTPS URL.", Text); return; }
        load.Enabled = false; SetProgress(10, "Probing the remote PMTiles v3 header...");
        try
        {
            var result = await Task.Run(() => Probe(remote.ToString())); SetProgress(35, "Discovering PMTiles source layers...");
            var sourceLayers = await Task.Run(() => GeoKernelViewerControl.PmTilesSourceLayers(result.Path));
            if (sourceLayers.Count == 0) throw new InvalidOperationException("PMTiles contains no drawable source layers.");
            viewer.ClearLayers();
            for (var i = 0; i < sourceLayers.Count; i++) { var sourceLayer = sourceLayers[i]; SetProgress(35 + ((i + 1) * 60 / sourceLayers.Count), $"Opening {sourceLayer.Name}..."); await Task.Yield(); if (!viewer.AddLayerFile(result.Path, new GeoKernelLayerLoadOptions { SourceLayerIndex = sourceLayer.Index, UseSpatialIndex = false })) throw new InvalidOperationException($"The PMTiles source layer '{sourceLayer.Name}' could not be opened."); }
            viewer.RefreshLayers(); viewer.FullExtent();
            details.Text = Report(result.Probe); progress.Value = 100; status.Text = $"{sourceLayers.Count} PMTiles source layers are streaming through HTTP byte ranges.";
        }
        catch (Exception ex) { progress.Style = ProgressBarStyle.Continuous; progress.Value = 0; details.Text = "Load failed:\r\n" + ex.Message; status.Text = "Cloud PMTiles load failed."; MessageBox.Show(this, ex.Message, Text); }
        finally { load.Enabled = true; }
    }

    static Result Probe(string remote)
    {
        using var cloud = new GeoKernelCloudClient(new { maximumMemoryBytes = 64L * 1024 * 1024, maximumDiskBytes = 1024L * 1024 * 1024 }); cloud.SetTimeout(30000);
        var probe = cloud.ProbePmTiles(remote); if (!probe.GetProperty("cloudReadable").GetBoolean()) throw new InvalidOperationException(probe.GetProperty("diagnostic").GetString());
        return new(cloud.PmTilesGdalVirtualPath(remote), probe.Clone());
    }

    void SetProgress(int value, string text) { progress.Visible = true; progress.Style = ProgressBarStyle.Continuous; progress.Value = Math.Clamp(value, 0, 100); status.Text = text; details.Text = text; }
    void ShowRenderProgress(bool busy) { if (!load.Enabled) return; progress.Visible = true; if (busy) { progress.Style = ProgressBarStyle.Marquee; status.Text = "Rendering map..."; } else { progress.Style = ProgressBarStyle.Continuous; progress.Value = 100; status.Text = "Map ready."; } }
    static void ApplyGdalOptions() { Environment.SetEnvironmentVariable("GDAL_DISABLE_READDIR_ON_OPEN", "EMPTY_DIR"); Environment.SetEnvironmentVariable("CPL_VSIL_CURL_ALLOWED_EXTENSIONS", ".parquet,.pmtiles"); Environment.SetEnvironmentVariable("GDAL_CACHEMAX", "256"); Environment.SetEnvironmentVariable("VSI_CACHE", "TRUE"); Environment.SetEnvironmentVariable("VSI_CACHE_SIZE", "67108864"); Environment.SetEnvironmentVariable("GDAL_HTTP_CONNECTTIMEOUT", "10"); Environment.SetEnvironmentVariable("GDAL_HTTP_TIMEOUT", "30"); }
    static string Report(JsonElement p) => $"Cloud PMTiles streaming\r\n\r\nURL: {p.GetProperty("url").GetString()}\r\nContent length: {p.GetProperty("contentLength").GetInt64()} bytes\r\nContent type: {p.GetProperty("contentType").GetString()}\r\nAccept-Ranges: {(p.GetProperty("acceptsRanges").GetBoolean() ? "yes" : "no")}\r\nPMTiles header: {(p.GetProperty("headerValid").GetBoolean() ? "valid" : "invalid")}\r\nSpecification: v{p.GetProperty("specificationVersion").GetInt32()}\r\nZoom range: {p.GetProperty("minimumZoom").GetInt32()}-{p.GetProperty("maximumZoom").GetInt32()}\r\nRoot directory: {p.GetProperty("rootDirectoryLength").GetUInt64()} bytes\r\nGDAL source: /vsicurl/\r\n\r\n{p.GetProperty("diagnostic").GetString()}\r\n\r\nOnly metadata and requested byte ranges are transferred; the complete PMTiles archive is not downloaded.";
    sealed record Result(string Path, JsonElement Probe);
}
