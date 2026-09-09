using GeoKernel.NET.WinForms;

namespace GeoKernel.Scalebar.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        var progress = new ControlProgress<SampleDataProgress>(this, value => SetProgress(value.Message, value.Percentage));
        var shapefilePath = await SampleData.EnsureWorldLayerAsync(this, progress);
        if (string.IsNullOrEmpty(shapefilePath)) { SetProgress("World data could not be prepared.", 0); return; }

        SetProgress("Loading world boundaries...", null);
        if (!geoKernelViewerControl.AddLayerFile(shapefilePath))
        {
            SetProgress("World boundaries could not be loaded.", 0);
            MessageBox.Show(this, $"Shapefile could not be loaded:{Environment.NewLine}{shapefilePath}", "Scalebar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        ConfigureScaleBar();
        geoKernelViewerControl.FullExtent();
        SetProgress("Map loaded.", 100);
        await Task.Delay(900);
        SetProgress("Ready", 0);
    }

    private void ConfigureScaleBar()
    {
        geoKernelViewerControl.ScaleBarVisible = true;
        geoKernelViewerControl.SetScaleBarAnchor(GeoKernelOverlayAnchor.BottomRight);
        geoKernelViewerControl.SetScaleBarColors(Color.FromArgb(235, 255, 255, 255), Color.FromArgb(50, 74, 72), Color.FromArgb(35, 50, 48));
    }

    private void SetProgress(string message, int? percentage)
    {
        progressLabel.Text = message;
        progressBar.MarqueeAnimationSpeed = percentage.HasValue ? 0 : 30;
        progressBar.Style = percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (percentage.HasValue) progressBar.Value = Math.Clamp(percentage.Value, 0, 100);
    }

    private sealed class ControlProgress<T>(Control control, Action<T> callback) : IProgress<T>
    {
        public void Report(T value)
        {
            if (control.IsDisposed) return;
            if (control.InvokeRequired) control.Invoke(() => callback(value)); else callback(value);
        }
    }
}
