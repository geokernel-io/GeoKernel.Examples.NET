using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerRefresh.Winforms;

public sealed partial class MainForm : Form
{
    private readonly string[] _fillColors = ["#D8E5E1", "#D9C7A5", "#C7D7EA", "#D7C5DE"];
    private readonly string[] _outlineColors = ["#6F8883", "#A24A3D", "#356780", "#6F4D8C"];
    private readonly int[] _opacities = [210, 160, 110, 235];
    private int _fillIndex;
    private int _outlineIndex;
    private int _opacityIndex;

    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        SetStyleControlsEnabled(false);
        var progress = new ControlProgress<SampleDataProgress>(this, p => SetProgress(p.Message, p.Percentage));
        var path = await SampleData.EnsureCaliforniaAsync(this, progress);
        if (string.IsNullOrEmpty(path)) { SetProgress("California data could not be prepared.", 0); return; }

        SetProgress("Loading California...", null);
        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = CurrentStyle() }))
        {
            SetProgress("California layer could not be loaded.", 0);
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "LayerRefresh", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null) geoKernelViewerControl.SetLayerName(layer.Index, "California");
        geoKernelViewerControl.RefreshLayers();
        geoKernelViewerControl.FullExtent();
        SetStyleControlsEnabled(true);
        SetProgress("Layer loaded.", 100);
        await Task.Delay(900);
        progressBar.Visible = false;
        UpdateStatus("Layer loaded. Change style, then press Refresh Layer.");
    }

    private void changeFillButton_Click(object sender, EventArgs e) { _fillIndex = (_fillIndex + 1) % _fillColors.Length; UpdateStatus("Fill changed. Press Refresh Layer to redraw."); }
    private void changeOutlineButton_Click(object sender, EventArgs e) { _outlineIndex = (_outlineIndex + 1) % _outlineColors.Length; UpdateStatus("Outline changed. Press Refresh Layer to redraw."); }
    private void changeOpacityButton_Click(object sender, EventArgs e) { _opacityIndex = (_opacityIndex + 1) % _opacities.Length; UpdateStatus("Opacity changed. Press Refresh Layer to redraw."); }

    private void refreshLayerButton_Click(object sender, EventArgs e)
    {
        if (!geoKernelViewerControl.SetLayerStyle(0, CurrentStyle())) { UpdateStatus("Style could not be applied."); return; }
        geoKernelViewerControl.RefreshLayers();
        UpdateStatus("Layer refreshed.");
    }

    private void fullExtentButton_Click(object sender, EventArgs e) => geoKernelViewerControl.FullExtent();

    private GeoKernelLayerStyle CurrentStyle() => new()
    {
        FillColor = _fillColors[_fillIndex], FillOpacity = _opacities[_opacityIndex],
        LineColor = _outlineColors[_outlineIndex], LineWidth = _outlineIndex == 0 ? 0.9 : 1.6
    };

    private void UpdateStatus(string message) => statusLabel.Text =
        $"{message} Fill: {_fillColors[_fillIndex]} | Outline: {_outlineColors[_outlineIndex]} | Opacity: {_opacities[_opacityIndex]}";

    private void SetStyleControlsEnabled(bool enabled)
    {
        changeFillButton.Enabled = enabled; changeOutlineButton.Enabled = enabled;
        changeOpacityButton.Enabled = enabled; refreshLayerButton.Enabled = enabled;
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
