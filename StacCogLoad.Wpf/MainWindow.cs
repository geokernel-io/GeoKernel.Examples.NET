using System.Globalization;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.NET.Wpf;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.StacCogLoad.Wpf;

public sealed class MainWindow : Window
{
    readonly GeoKernelViewerControl viewer = new(); readonly TextBox bbox = new() { Text = "18.00, 59.25, 18.20, 59.40" };
    readonly Button load = new() { Content = "Search STAC and stream visual COG", Height = 30 };
    readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100, Height = 20, Visibility = Visibility.Collapsed };
    readonly ListBox items = new() { Height = 90 };
    readonly TextBox details = new() { IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    readonly TextBlock status = new();

    public MainWindow()
    {
        Title = "StacCogLoad"; Width = 1280; Height = 820; Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Images/GeoKernelAppIcon.ico")); ApplyGdalOptions();
        var root = new DockPanel(); var tools = new StackPanel { Orientation = Orientation.Horizontal, Height = 34, Background = Brushes.WhiteSmoke };
        foreach (var x in new (string, Action)[] { ("Zoom In", () => viewer.ZoomIn()), ("Zoom Out", () => viewer.ZoomOut()), ("Full Extent", viewer.FullExtent), ("Pan", () => viewer.ActiveTool = GeoKernelViewerTool.Pan) }) { var b = new Button { Content = x.Item1, Margin = new Thickness(2) }; b.Click += (_, _) => x.Item2(); tools.Children.Add(b); }
        DockPanel.SetDock(tools, Dock.Top); root.Children.Add(tools); var footer = new Border { Child = status, Padding = new Thickness(4) }; DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);
        var right = new StackPanel { Width = 390, Margin = new Thickness(10) };
        right.Children.Add(new TextBlock { Text = "STAC COG streaming", FontWeight = FontWeights.Bold, FontSize = 14 }); right.Children.Add(new TextBlock { Text = "Catalog" }); right.Children.Add(new TextBox { Text = "https://earth-search.aws.element84.com/v1", IsReadOnly = true });
        right.Children.Add(new TextBlock { Text = "Collection: Sentinel-2 L2A", Margin = new Thickness(0, 5, 0, 0) }); right.Children.Add(new TextBlock { Text = "BBOX" }); right.Children.Add(bbox); right.Children.Add(load); right.Children.Add(progress);
        right.Children.Add(new TextBlock { Text = "Selected STAC item", Margin = new Thickness(0, 7, 0, 0) }); right.Children.Add(items); right.Children.Add(new TextBlock { Text = "Cloud diagnostics", Margin = new Thickness(0, 7, 0, 0) }); details.Height = 450; right.Children.Add(details);
        DockPanel.SetDock(right, Dock.Right); root.Children.Add(right); root.Children.Add(viewer); Content = root; viewer.ActiveTool = GeoKernelViewerTool.Pan;
        viewer.BusyChanged += (_, e) => Dispatcher.BeginInvoke(() => ShowRenderProgress(e.Busy));
        details.Text = "Ready. Search the STAC catalog to select COG assets."; load.Click += async (_, _) => await Run(); Loaded += async (_, _) => await Run();
    }

    async Task Run()
    {
        if (!TryBox(out var box)) { MessageBox.Show(this, "Enter a valid WGS84 BBOX as: xmin, ymin, xmax, ymax", Title); return; }
        load.IsEnabled = false; SetProgress(2, "Searching STAC and probing visual COGs..."); items.Items.Clear(); var reporter = new Progress<LoadProgress>(x => SetProgress(x.Value, x.Text));
        try
        {
            var result = await Task.Run(() => Search(box, reporter)); viewer.ClearLayers(); var options = RasterOptions();
            for (var i = 0; i < result.Count; i++)
            {
                var asset = result[i]; SetProgress(70 + (i + 1) * 25 / result.Count, $"Opening COG tile {i + 1} of {result.Count}..."); await System.Windows.Threading.Dispatcher.Yield();
                if (!viewer.AddLayerFile(asset.Path, options)) throw new InvalidOperationException($"Could not open {asset.Tile}"); items.Items.Add($"{asset.Tile} | {asset.Date} | cloud {asset.Cloud}");
            }
            if (!viewer.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.WebMercator)) throw new InvalidOperationException("Viewer Web Mercator CRS could not be applied.");
            viewer.RefreshLayers(); viewer.ViewExtent = WebMercatorExtent(box); details.Text = Report(result); progress.Value = 100; status.Text = $"{result.Count} visual COG tiles are streaming through HTTP byte ranges.";
        }
        catch (Exception ex) { progress.IsIndeterminate = false; progress.Value = 0; details.Text = "Load failed:\n" + ex.Message; status.Text = "STAC COG load failed."; MessageBox.Show(this, ex.Message, Title); }
        finally { load.IsEnabled = true; }
    }

    List<Asset> Search(double[] box, IProgress<LoadProgress> reporter)
    {
        using var cloud = new GeoKernelCloudClient(new { maximumMemoryBytes = 67108864, maximumDiskBytes = 1073741824 }); cloud.SetTimeout(15000); reporter.Report(new(10, "Searching the Earth Search STAC catalog..."));
        var root = cloud.StacSearch("https://earth-search.aws.element84.com/v1", new { collections = new[] { "sentinel-2-l2a" }, bbox = box, datetime = "2024-01-01T00:00:00Z/..", limit = 100, query = new Dictionary<string, object> { { "eo:cloud_cover", new { lt = 20 } } } });
        reporter.Report(new(25, "Selecting one recent scene per MGRS tile...")); var candidates = SelectCandidates(root); var output = new List<Asset>();
        for (var i = 0; i < candidates.Count; i++) { var c = candidates[i]; reporter.Report(new(30 + (i + 1) * 35 / Math.Max(1, candidates.Count), $"Probing COG tile {i + 1} of {candidates.Count}...")); var url = c.Visual.GetProperty("href").GetString()!; var probe = cloud.CogProbe(url); if (!probe.GetProperty("cloudReadable").GetBoolean()) continue; output.Add(new(c.Tile, c.Item.GetProperty("id").GetString()!, c.Properties.GetProperty("datetime").GetString()!, c.Properties.GetProperty("eo:cloud_cover").GetDouble().ToString("F1") + "%", cloud.CogGdalVirtualPath(url), probe.GetProperty("contentLength").GetInt64())); }
        if (output.Count == 0) throw new InvalidOperationException("STAC search returned no visual COG assets."); reporter.Report(new(68, $"{output.Count} COG tiles verified. Preparing Viewer layers...")); return output;
    }

    static List<Candidate> SelectCandidates(JsonElement root) { var seen = new HashSet<string>(); var result = new List<Candidate>(); foreach (var item in root.GetProperty("items").EnumerateArray()) { var p = item.GetProperty("properties"); var tile = $"{p.GetProperty("mgrs:utm_zone").GetInt32()}{p.GetProperty("mgrs:latitude_band").GetString()}{p.GetProperty("mgrs:grid_square").GetString()}"; if (!seen.Add(tile) || !item.GetProperty("assets").TryGetProperty("visual", out var visual)) continue; result.Add(new(tile, item.Clone(), p.Clone(), visual.Clone())); if (result.Count >= 16) break; } return result; }
    void SetProgress(int value, string text) { progress.Visibility = Visibility.Visible; progress.IsIndeterminate = false; progress.Value = Math.Clamp(value, 0, 100); status.Text = text; details.Text = text; }
    void ShowRenderProgress(bool busy) { if (!load.IsEnabled) return; progress.Visibility = Visibility.Visible; progress.IsIndeterminate = busy; if (busy) status.Text = "Rendering map..."; else { progress.Value = 100; status.Text = "Map ready."; } }
    static void ApplyGdalOptions() { Environment.SetEnvironmentVariable("GDAL_DISABLE_READDIR_ON_OPEN", "EMPTY_DIR"); Environment.SetEnvironmentVariable("CPL_VSIL_CURL_ALLOWED_EXTENSIONS", ".tif,.tiff"); Environment.SetEnvironmentVariable("GDAL_CACHEMAX", "256"); Environment.SetEnvironmentVariable("VSI_CACHE", "TRUE"); Environment.SetEnvironmentVariable("VSI_CACHE_SIZE", "67108864"); }
    bool TryBox(out double[] v) { v = bbox.Text.Split(',').Select(x => double.TryParse(x.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var n) ? n : double.NaN).ToArray(); return v.Length == 4 && v.All(double.IsFinite) && v[0] < v[2] && v[1] < v[3]; }
    static GeoKernelExtent WebMercatorExtent(double[] b) { static double X(double x) => x * 20037508.342789244 / 180; static double Y(double y) => Math.Log(Math.Tan((90 + Math.Clamp(y, -85.05112878, 85.05112878)) * Math.PI / 360)) * 20037508.342789244 / Math.PI; var x1 = X(b[0]); var x2 = X(b[2]); var y1 = Y(b[1]); var y2 = Y(b[3]); var px = (x2 - x1) * .04; var py = (y2 - y1) * .04; return new(x1 - px, y1 - py, x2 + px, y2 + py); }
    static GeoKernelLayerLoadOptions RasterOptions() => new() { PrepareRasterOverviews = false, RasterTileCacheEnabled = false, RasterTileCachePixelBudget = 0, RasterTileCacheMaximumItemPixels = 0 };
    static string Report(List<Asset> a) => "STAC + COG streaming mosaic\n\nCatalog: Earth Search v1\nCollection: sentinel-2-l2a\nUnique MGRS tiles: " + a.Count + "\n\n" + string.Join("\n", a.Select(x => $"{x.Tile} | {x.Id}\nDate/time: {x.Date} | Cloud cover: {x.Cloud}\nContent: {x.Bytes} bytes | Range: yes\n")) + "\nOnly metadata and visible ranges are transferred; complete COG files are not downloaded.";
    sealed record LoadProgress(int Value, string Text); sealed record Candidate(string Tile, JsonElement Item, JsonElement Properties, JsonElement Visual); sealed record Asset(string Tile, string Id, string Date, string Cloud, string Path, long Bytes);
}
