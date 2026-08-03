using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerExtent.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        var progress = new ControlProgress<SampleDataProgress>(this, p => SetProgress(p.Message, p.Percentage));
        var path = await SampleData.EnsureCaliforniaAsync(this, progress);
        if (string.IsNullOrEmpty(path)) { SetProgress("California data could not be prepared.", 0); return; }

        SetProgress("Loading California...", null);
        var style = new GeoKernelLayerStyle { FillColor = "#D8E5E1", FillOpacity = 210, LineColor = "#6F8883", LineWidth = 0.9 };
        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            SetProgress("California layer could not be loaded.", 0);
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "LayerExtent", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null) geoKernelViewerControl.SetLayerName(layer.Index, "California");
        if (!AddLayerExtentRectangle(0)) return;

        geoKernelViewerControl.InvalidateRenderCache(false, true);
        geoKernelViewerControl.FullExtent();
        progressBar.Value = 100;
        await Task.Delay(900);
        progressBar.Visible = false;
    }

    private bool AddLayerExtentRectangle(int layerIndex)
    {
        var extent = geoKernelViewerControl.GetLayerProjectedExtent(layerIndex);
        if (extent.XMax <= extent.XMin || extent.YMax <= extent.YMin)
        {
            MessageBox.Show(this, "Layer extent could not be calculated.", "LayerExtent", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        GeoKernelPoint[] rectangle =
        [
            new(extent.XMin, extent.YMin), new(extent.XMax, extent.YMin), new(extent.XMax, extent.YMax),
            new(extent.XMin, extent.YMax), new(extent.XMin, extent.YMin)
        ];
        var style = new GeoKernelLayerStyle { FillColor = "#FFFFFF", FillOpacity = 0, LineColor = "#E2453D", LineWidth = 2.2 };
        if (geoKernelViewerControl.AddPolygonLayer("Layer Extent", rectangle, style) < 0)
        {
            MessageBox.Show(this, "Layer extent rectangle could not be created.", "LayerExtent", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }
        statusLabel.Text = $"California extent: {extent.XMin:0.###}, {extent.YMin:0.###} - {extent.XMax:0.###}, {extent.YMax:0.###}";
        return true;
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
