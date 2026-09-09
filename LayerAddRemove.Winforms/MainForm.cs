using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerAddRemove.Winforms;

public sealed partial class MainForm : Form
{
    private readonly Dictionary<string, SampleLayer> _layers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["World"] = new("World", "world_4326.zip", "world_4326", "world_4326.shp", new GeoKernelLayerStyle
        { FillColor = "#D8E5E1", FillOpacity = 210, LineColor = "#7B918D", LineWidth = 0.8 }),
        ["States"] = new("States", "usa_states.zip", "usa_states", "usa_states.shp", new GeoKernelLayerStyle
        { FillColor = "#A9C8DB", FillOpacity = 100, LineColor = "#356780", LineWidth = 1.2 }),
        ["Cities"] = new("Cities", "usa_cities.zip", "usa_cities", "usa_cities.shp", new GeoKernelLayerStyle
        { PointColor = "#D95D39", PointSize = 7.0, LineColor = "#D95D39", LineWidth = 1.5 })
    };

    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        if (await AddLayerAsync("World")) geoKernelViewerControl.FullExtent();
    }

    private async void addWorldButton_Click(object sender, EventArgs e) => await AddLayerAsync("World");
    private async void addStatesButton_Click(object sender, EventArgs e) => await AddLayerAsync("States");
    private async void addCitiesButton_Click(object sender, EventArgs e) => await AddLayerAsync("Cities");
    private void removeWorldButton_Click(object sender, EventArgs e) => RemoveLayer("World");
    private void removeStatesButton_Click(object sender, EventArgs e) => RemoveLayer("States");
    private void removeCitiesButton_Click(object sender, EventArgs e) => RemoveLayer("Cities");

    private void clearLayersButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.InvalidateRenderCache(false, true);
        UpdateStatus("Layers cleared.");
    }

    private async Task<bool> AddLayerAsync(string key)
    {
        var layer = _layers[key];
        if (FindLayerIndex(layer) >= 0) { UpdateStatus($"{layer.Name} is already loaded."); return true; }
        SetToolbarEnabled(false);
        var progress = new ControlProgress<SampleDataProgress>(this, p => SetProgress(p.Message, p.Percentage));
        try
        {
            var path = await SampleData.EnsureSampleFileAsync(layer, this, progress);
            if (string.IsNullOrEmpty(path)) return false;
            SetProgress($"Loading {layer.Name}...", null);
            if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = layer.Style }))
            {
                MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "LayerAddRemove", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            geoKernelViewerControl.InvalidateRenderCache(false, true);
            UpdateStatus($"{layer.Name} added.");
            return true;
        }
        finally { SetToolbarEnabled(true); progressBar.Visible = false; }
    }

    private void RemoveLayer(string key)
    {
        var layer = _layers[key];
        var removed = false;
        for (var index = geoKernelViewerControl.LayerCount - 1; index >= 0; index--)
            if (MatchesLayer(geoKernelViewerControl.GetLayerInfo(index), layer)) removed = geoKernelViewerControl.RemoveLayer(index) || removed;
        geoKernelViewerControl.InvalidateRenderCache(false, true);
        UpdateStatus(removed ? $"{layer.Name} removed." : $"{layer.Name} is not loaded.");
    }

    private int FindLayerIndex(SampleLayer layer)
    {
        for (var index = geoKernelViewerControl.LayerCount - 1; index >= 0; index--)
            if (MatchesLayer(geoKernelViewerControl.GetLayerInfo(index), layer)) return index;
        return -1;
    }

    private static bool MatchesLayer(GeoKernelLayerInfo? info, SampleLayer layer) => info is not null &&
        (string.Equals(info.Name, layer.Name, StringComparison.OrdinalIgnoreCase) ||
         string.Equals(info.DisplayText, layer.Name, StringComparison.OrdinalIgnoreCase) ||
         (!string.IsNullOrWhiteSpace(info.Path) && string.Equals(Path.GetFileName(info.Path), layer.FileName, StringComparison.OrdinalIgnoreCase)));

    private void SetProgress(string message, int? percentage)
    {
        statusLabel.Text = message;
        progressBar.Visible = true;
        progressBar.Style = percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (percentage.HasValue) progressBar.Value = Math.Clamp(percentage.Value, 0, 100);
    }

    private void UpdateStatus(string message) => statusLabel.Text = $"{message} Layers: {geoKernelViewerControl.LayerCount}";
    private void SetToolbarEnabled(bool enabled) { foreach (Control control in toolbarPanel.Controls) if (control is Button) control.Enabled = enabled; }

    private sealed class ControlProgress<T>(Control control, Action<T> callback) : IProgress<T>
    {
        public void Report(T value) { if (control.IsDisposed) return; if (control.InvokeRequired) control.Invoke(() => callback(value)); else callback(value); }
    }
}
