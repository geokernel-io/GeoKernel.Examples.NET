using GeoKernel.NET.WinForms;

namespace GeoKernel.HelloMap.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        SetTool(GeoKernelViewerTool.Pan);
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        var progress = new ControlProgress<SampleDataProgress>(this, UpdateDownloadProgress);
        var shapefilePath = await SampleData.EnsureWorldLayerAsync(this, progress);
        downloadProgressBar.Visible = false;

        if (string.IsNullOrWhiteSpace(shapefilePath))
        {
            statusLabel.Text = "Sample data could not be prepared.";
            return;
        }

        if (!geoKernelViewerControl.AddLayerFile(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be loaded:{Environment.NewLine}{shapefilePath}",
                "HelloMap",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        geoKernelViewerControl.FullExtent();
        statusLabel.Text = "World layer loaded.";
    }

    private void UpdateDownloadProgress(SampleDataProgress progress)
    {
        statusLabel.Text = progress.Message;
        downloadProgressBar.Visible = true;

        if (progress.Percentage.HasValue)
        {
            downloadProgressBar.Style = ProgressBarStyle.Continuous;
            downloadProgressBar.Value = Math.Clamp(progress.Percentage.Value, 0, 100);
        }
        else
        {
            downloadProgressBar.Style = ProgressBarStyle.Marquee;
        }
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

    private void zoomInButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ZoomIn();
    }

    private void zoomOutButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ZoomOut();
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.FullExtent();
    }

    private void zoomRectButton_Click(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.ZoomBox);
    }

    private void panButton_Click(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);
    }

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
