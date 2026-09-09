using GeoKernel.NET.WinForms;

namespace GeoKernel.LabelOffset.Winforms;

public sealed partial class MainForm : Form
{
    private int _worldLayerIndex = -1;
    private bool _loading = true;

    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        var path = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(path) || !geoKernelViewerControl.AddLayerFile(path))
        {
            MessageBox.Show(this, "World layer could not be loaded.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        geoKernelViewerControl.SetLayerName(0, "World - label offset");
        _worldLayerIndex = geoKernelViewerControl.GetLayerInfoByName("World - label offset")?.Index ?? 0;
        geoKernelViewerControl.SetLayerStyle(_worldLayerIndex, OffsetStyle());
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-180.0, -58.0, 180.0, 82.0);
        _loading = false;
        offsetXNumeric.Enabled = true;
        offsetYNumeric.Enabled = true;
        resetButton.Enabled = true;
        statusLabel.Text = "Labels use labelOffsetX and labelOffsetY.";
    }

    private void resetButton_Click(object? sender, EventArgs e)
    {
        offsetXNumeric.Value = 0;
        offsetYNumeric.Value = 0;
        offsetControl_ValueChanged(sender, e);
    }

    private void offsetControl_ValueChanged(object? sender, EventArgs e)
    {
        if (_loading || _worldLayerIndex < 0) return;
        geoKernelViewerControl.SetLayerStyle(_worldLayerIndex, OffsetStyle());
        geoKernelViewerControl.InvalidateRenderCache(true, true);
        geoKernelViewerControl.RefreshLayers();
        statusLabel.Text = $"Label offset X: {offsetXNumeric.Value:0.0}, Y: {offsetYNumeric.Value:0.0}";
    }

    private GeoKernelLayerStyle OffsetStyle() => new()
    {
        FillColor = "#D8E5E1", FillOpacity = 215, LineColor = "#6F8380", LineWidth = 0.8,
        ShowLabels = true, LabelField = "COUNTRY", LabelFontSize = 12.0, LabelColor = "#253238",
        LabelHaloEnabled = true, LabelHaloColor = "#FFFFFF", LabelHaloWidth = 2.0,
        LabelOffsetX = (double)offsetXNumeric.Value, LabelOffsetY = (double)offsetYNumeric.Value
    };

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, p =>
    { statusLabel.Text = p.Message; downloadProgressBar.Visible = true; downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee; if (p.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(p.Percentage.Value, 0, 100); });
}
