using GeoKernel.NET.WinForms;

namespace GeoKernel.Project.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);

        var dataProgress = new ControlProgress<SampleDataProgress>(this, value =>
            SetProgress(value.Message, value.Percentage));
        var projectPath = await SampleData.EnsureProjectAsync(this, dataProgress);
        if (string.IsNullOrEmpty(projectPath))
        {
            SetProgress("Project data could not be prepared.", 0);
            return;
        }

        SetProgress("Loading andalucia.geokernel...", null);
        var loadProgress = new ControlProgress<GeoKernelLayerLoadProgress>(this, value =>
            SetProgress(
                string.IsNullOrWhiteSpace(value.Status)
                    ? "Loading andalucia.geokernel..."
                    : value.Status,
                value.Progress));

        if (!geoKernelViewerControl.OpenProject(projectPath, loadProgress))
        {
            SetProgress("Project could not be loaded.", 0);
            MessageBox.Show(
                this,
                $"Project could not be loaded:{Environment.NewLine}{projectPath}",
                "Project",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        SetProgress("Project loaded.", 100);
        await Task.Delay(900);
        SetProgress("Ready", 0);
    }

    private void SetProgress(string message, int? percentage)
    {
        progressLabel.Text = message;
        progressBar.MarqueeAnimationSpeed = percentage.HasValue ? 0 : 30;
        progressBar.Style = percentage.HasValue
            ? ProgressBarStyle.Blocks
            : ProgressBarStyle.Marquee;
        if (percentage.HasValue)
            progressBar.Value = Math.Clamp(percentage.Value, 0, 100);
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        geoKernelViewerControl.ActiveTool = tool;
        zoomRectButton.BackColor = tool == GeoKernelViewerTool.ZoomBox
            ? Color.FromArgb(200, 230, 255)
            : SystemColors.Control;
        panButton.BackColor = tool == GeoKernelViewerTool.Pan
            ? Color.FromArgb(200, 230, 255)
            : SystemColors.Control;
    }

    private void zoomInButton_Click(object sender, EventArgs e) => geoKernelViewerControl.ZoomIn();
    private void zoomOutButton_Click(object sender, EventArgs e) => geoKernelViewerControl.ZoomOut();
    private void fullExtentButton_Click(object sender, EventArgs e) => geoKernelViewerControl.FullExtent();
    private void zoomRectButton_Click(object sender, EventArgs e) => SetTool(GeoKernelViewerTool.ZoomBox);
    private void panButton_Click(object sender, EventArgs e) => SetTool(GeoKernelViewerTool.Pan);

    private sealed class ControlProgress<T>(Control control, Action<T> callback) : IProgress<T>
    {
        public void Report(T value)
        {
            if (control.IsDisposed)
                return;

            if (control.InvokeRequired)
                control.Invoke(() => callback(value));
            else
                callback(value);
        }
    }
}
