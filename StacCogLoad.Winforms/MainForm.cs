using System.Globalization;
using System.Text.Json;
using GeoKernel.NET.WinForms;

namespace GeoKernel.StacCogLoad.Winforms;

public sealed class MainForm : Form
{
    readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    readonly TextBox bbox = new() { Text = "18.00, 59.25, 18.20, 59.40" };
    readonly Button load = new() { Text = "Search STAC and stream visual COG" };
    readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100, Height = 20, Visible = false };
    readonly ListBox items = new() { Height = 90 };
    readonly TextBox details = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both };
    readonly ToolStripStatusLabel status = new();

    public MainForm()
    {
        Text = "StacCogLoad"; Width = 1280; Height = 820;
        Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "resources", "GeoKernelAppIcon.ico"));
        ApplyGdalOptions();
        var right = new Panel { Dock = DockStyle.Right, Width = 390, Padding = new Padding(10) };
        var header = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 325, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        Control Full(Control c) { c.Width = 365; c.Margin = new Padding(0, 2, 0, 2); return c; }
        header.Controls.Add(new Label { Text = "STAC COG streaming", AutoSize = true, Font = new Font(Font, FontStyle.Bold) });
        header.Controls.Add(new Label { Text = "Catalog", AutoSize = true }); header.Controls.Add(Full(new TextBox { Text = "https://earth-search.aws.element84.com/v1", ReadOnly = true }));
        header.Controls.Add(new Label { Text = "Collection", AutoSize = true }); header.Controls.Add(Full(new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Items = { "Sentinel-2 L2A" }, SelectedIndex = 0 }));
        header.Controls.Add(new Label { Text = "BBOX", AutoSize = true }); header.Controls.Add(Full(bbox)); header.Controls.Add(Full(load)); header.Controls.Add(Full(progress));
        header.Controls.Add(new Label { Text = "Selected STAC item", AutoSize = true }); header.Controls.Add(Full(items));
        right.Controls.Add(details); right.Controls.Add(new Label { Text = "Cloud diagnostics", Dock = DockStyle.Top, Height = 22 }); right.Controls.Add(header);
        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        tools.Items.Add("Zoom In", null, (_, _) => viewer.ZoomIn()); tools.Items.Add("Zoom Out", null, (_, _) => viewer.ZoomOut()); tools.Items.Add("Full Extent", null, (_, _) => viewer.FullExtent()); tools.Items.Add("Pan", null, (_, _) => viewer.ActiveTool = GeoKernelViewerTool.Pan);
        var bar = new StatusStrip(); bar.Items.Add(status); Controls.Add(viewer); Controls.Add(right); Controls.Add(tools); Controls.Add(bar); viewer.ActiveTool = GeoKernelViewerTool.Pan;
        viewer.BusyChanged += (_, e) => BeginInvoke(() => ShowRenderProgress(e.Busy));
        load.Click += async (_, _) => await Run(); Shown += async (_, _) => await Run(); details.Text = "Ready. Search the STAC catalog to select COG assets.";
    }

    async Task Run()
    {
        if (!TryBbox(out var box)) { MessageBox.Show(this, "Enter a valid WGS84 BBOX as: xmin, ymin, xmax, ymax", Text); return; }
        load.Enabled = false; SetProgress(2, "Searching STAC and probing visual COGs..."); items.Items.Clear();
        var reporter = new Progress<LoadProgress>(x => SetProgress(x.Value, x.Text));
        try
        {
            var result = await Task.Run(() => Search(box, reporter)); viewer.ClearLayers(); var options = RasterOptions();
            for (var i = 0; i < result.Count; i++)
            {
                var asset = result[i]; SetProgress(70 + (i + 1) * 25 / result.Count, $"Opening COG tile {i + 1} of {result.Count}..."); await Task.Yield();
                if (!viewer.AddLayerFile(asset.Path, options)) throw new InvalidOperationException($"Could not open {asset.Tile}");
                items.Items.Add($"{asset.Tile} | {asset.Date} | cloud {asset.Cloud}");
            }
            if (!viewer.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.WebMercator)) throw new InvalidOperationException("Viewer Web Mercator CRS could not be applied.");
            viewer.RefreshLayers(); viewer.ViewExtent = WebMercatorExtent(box); details.Text = Report(result); progress.Value = 100; status.Text = $"{result.Count} visual COG tiles are streaming through HTTP byte ranges.";
        }
        catch (Exception ex) { progress.Style = ProgressBarStyle.Continuous; progress.Value = 0; details.Text = "Load failed:\r\n" + ex.Message; status.Text = "STAC COG load failed."; MessageBox.Show(this, ex.Message, Text); }
        finally { load.Enabled = true; }
    }

    List<Asset> Search(double[] box, IProgress<LoadProgress> reporter)
    {
        using var cloud = new GeoKernelCloudClient(new { maximumMemoryBytes = 64L * 1024 * 1024, maximumDiskBytes = 1024L * 1024 * 1024 }); cloud.SetTimeout(15000);
        reporter.Report(new(10, "Searching the Earth Search STAC catalog..."));
        var root = cloud.StacSearch("https://earth-search.aws.element84.com/v1", new { collections = new[] { "sentinel-2-l2a" }, bbox = box, datetime = "2024-01-01T00:00:00Z/..", limit = 100, query = new Dictionary<string, object> { { "eo:cloud_cover", new { lt = 20 } } } });
        reporter.Report(new(25, "Selecting one recent scene per MGRS tile...")); var candidates = SelectCandidates(root); var output = new List<Asset>();
        for (var i = 0; i < candidates.Count; i++)
        {
            var c = candidates[i]; reporter.Report(new(30 + (i + 1) * 35 / Math.Max(1, candidates.Count), $"Probing COG tile {i + 1} of {candidates.Count}..."));
            var url = c.Visual.GetProperty("href").GetString()!; var probe = cloud.CogProbe(url); if (!probe.GetProperty("cloudReadable").GetBoolean()) continue;
            output.Add(new(c.Tile, c.Item.GetProperty("id").GetString()!, c.Properties.GetProperty("datetime").GetString()!, c.Properties.GetProperty("eo:cloud_cover").GetDouble().ToString("F1") + "%", cloud.CogGdalVirtualPath(url), probe.GetProperty("contentLength").GetInt64()));
        }
        if (output.Count == 0) throw new InvalidOperationException("STAC search returned no visual COG assets."); reporter.Report(new(68, $"{output.Count} COG tiles verified. Preparing Viewer layers...")); return output;
    }

    static List<Candidate> SelectCandidates(JsonElement root)
    {
        var seen = new HashSet<string>(); var result = new List<Candidate>();
        foreach (var item in root.GetProperty("items").EnumerateArray())
        {
            var p = item.GetProperty("properties"); var tile = $"{p.GetProperty("mgrs:utm_zone").GetInt32()}{p.GetProperty("mgrs:latitude_band").GetString()}{p.GetProperty("mgrs:grid_square").GetString()}";
            if (!seen.Add(tile) || !item.GetProperty("assets").TryGetProperty("visual", out var visual)) continue;
            result.Add(new(tile, item.Clone(), p.Clone(), visual.Clone())); if (result.Count >= 16) break;
        }
        return result;
    }

    void SetProgress(int value, string text) { progress.Visible = true; progress.Style = ProgressBarStyle.Continuous; progress.Value = Math.Clamp(value, 0, 100); status.Text = text; details.Text = text; }
    void ShowRenderProgress(bool busy) { if (!load.Enabled) return; progress.Visible = true; if (busy) { progress.Style = ProgressBarStyle.Marquee; status.Text = "Rendering map..."; } else { progress.Style = ProgressBarStyle.Continuous; progress.Value = 100; status.Text = "Map ready."; } }
    static void ApplyGdalOptions() { Environment.SetEnvironmentVariable("GDAL_DISABLE_READDIR_ON_OPEN", "EMPTY_DIR"); Environment.SetEnvironmentVariable("CPL_VSIL_CURL_ALLOWED_EXTENSIONS", ".tif,.tiff"); Environment.SetEnvironmentVariable("GDAL_CACHEMAX", "256"); Environment.SetEnvironmentVariable("VSI_CACHE", "TRUE"); Environment.SetEnvironmentVariable("VSI_CACHE_SIZE", "67108864"); }
    bool TryBbox(out double[] v) { v = bbox.Text.Split(',').Select(x => double.TryParse(x.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : double.NaN).ToArray(); return v.Length == 4 && v.All(double.IsFinite) && v[0] < v[2] && v[1] < v[3]; }
    static GeoKernelExtent WebMercatorExtent(double[] b) { static double X(double x) => x * 20037508.342789244 / 180; static double Y(double y) => Math.Log(Math.Tan((90 + Math.Clamp(y, -85.05112878, 85.05112878)) * Math.PI / 360)) * 20037508.342789244 / Math.PI; var x1 = X(b[0]); var x2 = X(b[2]); var y1 = Y(b[1]); var y2 = Y(b[3]); var px = (x2 - x1) * .04; var py = (y2 - y1) * .04; return new(x1 - px, y1 - py, x2 + px, y2 + py); }
    static GeoKernelLayerLoadOptions RasterOptions() => new() { PrepareRasterOverviews = false, RasterTileCacheEnabled = false, RasterTileCachePixelBudget = 0, RasterTileCacheMaximumItemPixels = 0 };
    static string Report(List<Asset> a) => "STAC + COG streaming mosaic\r\n\r\nCatalog: Earth Search v1\r\nCollection: sentinel-2-l2a\r\nUnique MGRS tiles: " + a.Count + "\r\n\r\n" + string.Join("\r\n", a.Select(x => $"{x.Tile} | {x.Id}\r\nDate/time: {x.Date} | Cloud cover: {x.Cloud}\r\nContent: {x.Bytes} bytes | Range: yes\r\n")) + "\r\nOnly metadata and visible ranges are transferred; complete COG files are not downloaded.";
    sealed record LoadProgress(int Value, string Text); sealed record Candidate(string Tile, JsonElement Item, JsonElement Properties, JsonElement Visual); sealed record Asset(string Tile, string Id, string Date, string Cloud, string Path, long Bytes);
}
