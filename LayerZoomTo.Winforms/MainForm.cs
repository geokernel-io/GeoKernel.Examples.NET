using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerZoomTo.Winforms;

public sealed partial class MainForm : Form
{
    private readonly List<CityLayer> _cities = [];
    private bool _loading;

    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        cityComboBox.Enabled = false;
        var progress = new ControlProgress<SampleDataProgress>(this, p => SetProgress(p.Message, p.Percentage));
        var cities = await SampleData.EnsureCityLayersAsync(this, progress);
        if (cities.Count == 0) { SetProgress("California city data could not be prepared.", 0); return; }

        _cities.AddRange(cities);
        _loading = true;
        cityComboBox.Items.Add("-");
        foreach (var city in _cities) cityComboBox.Items.Add(city.Name);
        cityComboBox.SelectedIndex = 0;
        _loading = false;

        for (var index = 0; index < _cities.Count; index++)
        {
            var city = _cities[index];
            SetProgress($"Loading {city.Name}...", (index * 100) / _cities.Count);
            if (!AddLayer(city)) return;
        }
        geoKernelViewerControl.InvalidateRenderCache(false, true);
        geoKernelViewerControl.FullExtent();
        cityComboBox.Enabled = true;
        SetProgress("Layers loaded.", 100);
        await Task.Delay(900);
        statusLabel.Text = $"Layers: {geoKernelViewerControl.LayerCount} | Labels: NAME";
        progressBar.Visible = false;
    }

    private bool AddLayer(CityLayer city)
    {
        var style = new GeoKernelLayerStyle
        {
            FillColor = city.FillColor, FillOpacity = 150, LineColor = "#5F7772", LineWidth = 0.8,
            ShowLabels = true, LabelFontSize = 12, LabelAllowOverlap = true, LabelAvoidObstacles = false,
            LabelField = "NAME", LabelColor = "#000000", LabelHaloEnabled = true, LabelHaloColor = "#FFFF00", LabelHaloWidth = 2.0
        };
        if (!geoKernelViewerControl.AddLayerFile(city.Path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{city.Path}", "LayerZoomTo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null) geoKernelViewerControl.SetLayerName(layer.Index, city.Name);
        return true;
    }

    private void cityComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_loading) return;
        var selected = cityComboBox.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(selected) || selected == "-") { geoKernelViewerControl.FullExtent(); statusLabel.Text = "Full extent"; return; }
        var layer = geoKernelViewerControl.GetLayersInfo().FirstOrDefault(item =>
            string.Equals(item.Name, selected, StringComparison.OrdinalIgnoreCase) || string.Equals(item.DisplayText, selected, StringComparison.OrdinalIgnoreCase));
        statusLabel.Text = layer is not null && geoKernelViewerControl.ZoomToLayer(layer.Index) ? $"Zoomed to {selected}" : $"Layer not found: {selected}";
    }

    private void SetProgress(string message, int? percentage)
    {
        statusLabel.Text = message;
        progressBar.Visible = true;
        progressBar.Style = percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (percentage.HasValue) progressBar.Value = Math.Clamp(percentage.Value, 0, 100);
    }

    private sealed class ControlProgress<T>(Control control, Action<T> callback) : IProgress<T>
    {
        public void Report(T value) { if (control.IsDisposed) return; if (control.InvokeRequired) control.Invoke(() => callback(value)); else callback(value); }
    }
}
