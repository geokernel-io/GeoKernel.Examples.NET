using GeoKernel.NET.WinForms;

namespace GeoKernel.Measure.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly Uri WorldUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/world_4326.zip");
    private static readonly Uri CitiesUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/world_cities_4326.zip");

    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        SelectTool(panButton);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.ScaleBarVisible = true;
        geoKernelViewerControl.SetScaleBarAnchor(GeoKernelOverlayAnchor.BottomLeft);

        var progress = new ControlProgress<SampleDataProgress>(this, p => SetProgress(p.Message, p.Percentage));
        var world = await SampleData.EnsureSampleFileAsync(WorldUrl, "world_4326.zip", "world_4326", "world_4326.shp", "world boundaries", this, progress);
        if (string.IsNullOrEmpty(world)) { SetProgress("World data could not be prepared.", 0); return; }
        var cities = await SampleData.EnsureSampleFileAsync(CitiesUrl, "world_cities_4326.zip", "world_cities_4326", "world_cities_4326.shp", "world cities", this, progress);
        if (string.IsNullOrEmpty(cities)) { SetProgress("City data could not be prepared.", 0); return; }

        SetProgress("Loading sample layers...", null);
        if (!geoKernelViewerControl.AddLayerFile(world) || !geoKernelViewerControl.AddLayerFile(cities))
        {
            SetProgress("Sample layers could not be loaded.", 0);
            MessageBox.Show(this, "Sample layers could not be loaded.", "Measure", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }
        geoKernelViewerControl.FullExtent();
        SetProgress("Map loaded.", 100);
        await Task.Delay(900);
        SetProgress("Ready", 0);
    }

    private void panButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.MeasureToolActive = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        SelectTool(panButton);
    }

    private void distanceButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.StartMeasureDistance();
        SelectTool(distanceButton);
    }

    private void areaButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.StartMeasureArea();
        SelectTool(areaButton);
    }

    private void clearButton_Click(object sender, EventArgs e) => geoKernelViewerControl.ClearMeasure();
    private void fullExtentButton_Click(object sender, EventArgs e) => geoKernelViewerControl.FullExtent();

    private void SelectTool(Button selected)
    {
        foreach (var button in new[] { panButton, distanceButton, areaButton })
            button.BackColor = button == selected ? Color.FromArgb(200, 230, 255) : SystemColors.Control;
    }

    private void SetProgress(string message, int? percentage)
    {
        statusLabel.Text = message;
        progressBar.Visible = message != "Ready";
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
