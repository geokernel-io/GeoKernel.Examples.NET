using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.NET.Wpf;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.CloudPmTilesLoad.Wpf;

public sealed class MainWindow : Window
{
    readonly GeoKernelViewerControl viewer = new();
    readonly TextBox url = new() { Text = "https://pmtiles.io/protomaps(vector)ODbL_firenze.pmtiles" };
    readonly Button load = new() { Content = "Probe and stream PMTiles", Height = 30 };
    readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100, Height = 20, Visibility = Visibility.Collapsed };
    readonly TextBox details = new() { IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    readonly TextBlock status = new();

    public MainWindow()
    {
        Title = "CloudPmTilesLoad"; Width = 1280; Height = 820; Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Images/GeoKernelAppIcon.ico")); ApplyGdalOptions();
        var root = new DockPanel(); var tools = new StackPanel { Orientation = Orientation.Horizontal, Height = 34, Background = Brushes.WhiteSmoke };
        foreach (var x in new (string, Action)[] { ("Zoom In", () => viewer.ZoomIn()), ("Zoom Out", () => viewer.ZoomOut()), ("Full Extent", viewer.FullExtent), ("Pan", () => viewer.ActiveTool = GeoKernelViewerTool.Pan) }) { var b = new Button { Content = x.Item1, Margin = new Thickness(2) }; b.Click += (_, _) => x.Item2(); tools.Children.Add(b); }
        DockPanel.SetDock(tools, Dock.Top); root.Children.Add(tools); var footer = new Border { Child = status, Padding = new Thickness(4) }; DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);
        var right = new StackPanel { Width = 390, Margin = new Thickness(10) }; right.Children.Add(new TextBlock { Text = "Cloud PMTiles streaming", FontWeight = FontWeights.Bold, FontSize = 14 }); right.Children.Add(new TextBlock { Text = "Remote PMTiles URL" }); right.Children.Add(url); right.Children.Add(load); right.Children.Add(progress); right.Children.Add(new TextBlock { Text = "Cloud diagnostics", Margin = new Thickness(0, 7, 0, 0) }); details.Height = 600; right.Children.Add(details);
        DockPanel.SetDock(right, Dock.Right); root.Children.Add(right); root.Children.Add(viewer); Content = root; viewer.ActiveTool = GeoKernelViewerTool.Pan;
        viewer.BusyChanged += (_, e) => Dispatcher.BeginInvoke(() => ShowRenderProgress(e.Busy)); load.Click += async (_, _) => await Run(); Loaded += async (_, _) => await Run(); details.Text = "Ready.";
    }

    async Task Run()
    {
        if (!Uri.TryCreate(url.Text.Trim(), UriKind.Absolute, out var remote) || (remote.Scheme != "http" && remote.Scheme != "https")) { MessageBox.Show(this, "Enter a valid HTTP or HTTPS URL.", Title); return; }
        load.IsEnabled = false; SetProgress(10, "Probing the remote PMTiles v3 header...");
        try { var result = await Task.Run(() => Probe(remote.ToString())); SetProgress(35, "Discovering PMTiles source layers..."); var sourceLayers = await Task.Run(() => GeoKernelViewerControl.PmTilesSourceLayers(result.Path)); if (sourceLayers.Count == 0) throw new InvalidOperationException("PMTiles contains no drawable source layers."); viewer.ClearLayers(); for (var i = 0; i < sourceLayers.Count; i++) { var sourceLayer = sourceLayers[i]; SetProgress(35 + ((i + 1) * 60 / sourceLayers.Count), $"Opening {sourceLayer.Name}..."); await System.Windows.Threading.Dispatcher.Yield(); if (!viewer.AddLayerFile(result.Path, new GeoKernelLayerLoadOptions { SourceLayerIndex = sourceLayer.Index, UseSpatialIndex = false })) throw new InvalidOperationException($"The PMTiles source layer '{sourceLayer.Name}' could not be opened."); } viewer.RefreshLayers(); viewer.FullExtent(); details.Text = Report(result.Probe); progress.Value = 100; status.Text = $"{sourceLayers.Count} PMTiles source layers are streaming through HTTP byte ranges."; }
        catch (Exception ex) { progress.IsIndeterminate = false; progress.Value = 0; details.Text = "Load failed:\n" + ex.Message; status.Text = "Cloud PMTiles load failed."; MessageBox.Show(this, ex.Message, Title); }
        finally { load.IsEnabled = true; }
    }
    static Result Probe(string remote) { using var cloud = new GeoKernelCloudClient(new { maximumMemoryBytes = 67108864L, maximumDiskBytes = 1073741824L }); cloud.SetTimeout(30000); var probe = cloud.ProbePmTiles(remote); if (!probe.GetProperty("cloudReadable").GetBoolean()) throw new InvalidOperationException(probe.GetProperty("diagnostic").GetString()); return new(cloud.PmTilesGdalVirtualPath(remote), probe.Clone()); }
    void SetProgress(int value, string text) { progress.Visibility = Visibility.Visible; progress.IsIndeterminate = false; progress.Value = Math.Clamp(value, 0, 100); status.Text = text; details.Text = text; }
    void ShowRenderProgress(bool busy) { if (!load.IsEnabled) return; progress.Visibility = Visibility.Visible; progress.IsIndeterminate = busy; if (busy) status.Text = "Rendering map..."; else { progress.Value = 100; status.Text = "Map ready."; } }
    static void ApplyGdalOptions() { Environment.SetEnvironmentVariable("GDAL_DISABLE_READDIR_ON_OPEN", "EMPTY_DIR"); Environment.SetEnvironmentVariable("CPL_VSIL_CURL_ALLOWED_EXTENSIONS", ".parquet,.pmtiles"); Environment.SetEnvironmentVariable("GDAL_CACHEMAX", "256"); Environment.SetEnvironmentVariable("VSI_CACHE", "TRUE"); Environment.SetEnvironmentVariable("VSI_CACHE_SIZE", "67108864"); Environment.SetEnvironmentVariable("GDAL_HTTP_CONNECTTIMEOUT", "10"); Environment.SetEnvironmentVariable("GDAL_HTTP_TIMEOUT", "30"); }
    static string Report(JsonElement p) => $"Cloud PMTiles streaming\n\nURL: {p.GetProperty("url").GetString()}\nContent length: {p.GetProperty("contentLength").GetInt64()} bytes\nContent type: {p.GetProperty("contentType").GetString()}\nAccept-Ranges: {(p.GetProperty("acceptsRanges").GetBoolean() ? "yes" : "no")}\nPMTiles header: {(p.GetProperty("headerValid").GetBoolean() ? "valid" : "invalid")}\nSpecification: v{p.GetProperty("specificationVersion").GetInt32()}\nZoom range: {p.GetProperty("minimumZoom").GetInt32()}-{p.GetProperty("maximumZoom").GetInt32()}\nRoot directory: {p.GetProperty("rootDirectoryLength").GetUInt64()} bytes\nGDAL source: /vsicurl/\n\n{p.GetProperty("diagnostic").GetString()}\n\nOnly metadata and requested byte ranges are transferred; the complete PMTiles archive is not downloaded.";
    sealed record Result(string Path, JsonElement Probe);
}
