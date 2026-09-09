using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerVisibility.Winforms;

public sealed partial class MainForm : Form
{
    private bool _refreshingLayerList;
    private readonly SampleLayer[] _layers =
    [
        new("World", "world_4326.zip", "world_4326", "world_4326.shp", new GeoKernelLayerStyle
        { FillColor = "#D8E5E1", FillOpacity = 220, LineColor = "#7B918D", LineWidth = 0.8 }),
        new("States", "usa_states.zip", "usa_states", "usa_states.shp", new GeoKernelLayerStyle
        { FillColor = "#A9C8DB", FillOpacity = 115, LineColor = "#356780", LineWidth = 1.2 }),
        new("Cities", "usa_cities.zip", "usa_cities", "usa_cities.shp", new GeoKernelLayerStyle
        { PointColor = "#D95D39", PointSize = 7.0 })
    ];

    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);
        SetUiEnabled(false);
        try
        {
            var progress = new ControlProgress<SampleDataProgress>(this, p => SetProgress(p.Message, p.Percentage));
            foreach (var layer in _layers)
            {
                var path = await SampleData.EnsureSampleFileAsync(layer, this, progress);
                if (string.IsNullOrEmpty(path) || !AddLayer(layer, path)) return;
            }
            RefreshLayerList();
            geoKernelViewerControl.InvalidateRenderCache(false, true);
            geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-151.2, 16.4, -41.6, 55.6);
            SetProgress("Layers loaded.", 100);
            await Task.Delay(900);
            statusLabel.Text = $"Layers: {geoKernelViewerControl.LayerCount}";
            progressBar.Visible = false;
        }
        finally { SetUiEnabled(true); UpdateVisibilityButton(); }
    }

    private bool AddLayer(SampleLayer layer, string path)
    {
        SetProgress($"Loading {layer.Name}...", null);
        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = layer.Style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "LayerVisibility", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        var info = geoKernelViewerControl.GetLayerInfo(0);
        if (info is not null) geoKernelViewerControl.SetLayerName(info.Index, layer.Name);
        return true;
    }

    private void RefreshLayerList(int selectedIndex = -1)
    {
        _refreshingLayerList = true;
        try
        {
            layerListBox.Items.Clear();
            foreach (var layer in geoKernelViewerControl.GetLayersInfo()) layerListBox.Items.Add($"{(layer.Visible ? "[x]" : "[ ]")} {layer.DisplayText}");
            if (layerListBox.Items.Count > 0) layerListBox.SelectedIndex = selectedIndex >= 0 && selectedIndex < layerListBox.Items.Count ? selectedIndex : 0;
        }
        finally { _refreshingLayerList = false; }
        UpdateVisibilityButton();
    }

    private void ToggleSelectedLayerVisibility()
    {
        var index = layerListBox.SelectedIndex;
        var layer = index >= 0 ? geoKernelViewerControl.GetLayerInfo(index) : null;
        if (layer is null || !geoKernelViewerControl.SetLayerVisible(index, !layer.Visible)) return;
        geoKernelViewerControl.InvalidateRenderCache(false, true);
        RefreshLayerList(index);
        statusLabel.Text = $"{layer.DisplayText} {(layer.Visible ? "hidden" : "shown")}. Layers: {geoKernelViewerControl.LayerCount}";
    }

    private void UpdateVisibilityButton()
    {
        var index = layerListBox.SelectedIndex;
        var layer = index >= 0 ? geoKernelViewerControl.GetLayerInfo(index) : null;
        visibilityButton.Enabled = layer is not null;
        visibilityButton.Text = layer is null ? "Change Visibility" : layer.Visible ? "Change Visibility: Hide" : "Change Visibility: Show";
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        geoKernelViewerControl.ActiveTool = tool;
        zoomRectButton.BackColor = tool == GeoKernelViewerTool.ZoomBox ? Color.FromArgb(200, 230, 255) : SystemColors.Control;
        panButton.BackColor = tool == GeoKernelViewerTool.Pan ? Color.FromArgb(200, 230, 255) : SystemColors.Control;
    }

    private void SetProgress(string message, int? percentage)
    {
        statusLabel.Text = message;
        progressBar.Visible = true;
        progressBar.Style = percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (percentage.HasValue) progressBar.Value = Math.Clamp(percentage.Value, 0, 100);
    }

    private void SetUiEnabled(bool enabled) { toolbarPanel.Enabled = enabled; sidePanelLayout.Enabled = enabled; }
    private void zoomInButton_Click(object sender, EventArgs e) => geoKernelViewerControl.ZoomIn();
    private void zoomOutButton_Click(object sender, EventArgs e) => geoKernelViewerControl.ZoomOut();
    private void fullExtentButton_Click(object sender, EventArgs e) => geoKernelViewerControl.FullExtent();
    private void zoomRectButton_Click(object sender, EventArgs e) => SetTool(GeoKernelViewerTool.ZoomBox);
    private void panButton_Click(object sender, EventArgs e) => SetTool(GeoKernelViewerTool.Pan);
    private void visibilityButton_Click(object sender, EventArgs e) => ToggleSelectedLayerVisibility();
    private void layerListBox_SelectedIndexChanged(object sender, EventArgs e) { if (!_refreshingLayerList) UpdateVisibilityButton(); }

    private sealed class ControlProgress<T>(Control control, Action<T> callback) : IProgress<T>
    {
        public void Report(T value) { if (control.IsDisposed) return; if (control.InvokeRequired) control.Invoke(() => callback(value)); else callback(value); }
    }
}
