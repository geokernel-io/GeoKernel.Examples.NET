using GeoKernel.NET.WinForms;

namespace GeoKernel.LabelHalo.Winforms;

public sealed partial class MainForm : Form
{
    private int _worldLayerIndex = -1;
    private bool _loading = true;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        FillHaloColors();

        var path = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (!LoadLayer(path))
            return;

        ApplyHaloStyle();
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-180.0, -58.0, 180.0, 82.0);
        _loading = false;
        statusLabel.Text = "Labels use labelHaloEnabled, labelHaloColor and labelHaloWidth.";
    }

    private void FillHaloColors()
    {
        haloColorComboBox.Items.Add(new HaloColor("White", "#FFFFFF"));
        haloColorComboBox.Items.Add(new HaloColor("Black", "#000000"));
        haloColorComboBox.Items.Add(new HaloColor("Yellow", "#FFF2A8"));
        haloColorComboBox.Items.Add(new HaloColor("Blue", "#BAE6FD"));
        haloColorComboBox.SelectedIndex = 0;
    }

    private bool LoadLayer(string path)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        geoKernelViewerControl.SetLayerName(0, "World - label halo");
        _worldLayerIndex = geoKernelViewerControl.GetLayerInfoByName("World - label halo")?.Index ?? 0;
        return true;
    }

    private void haloControl_Changed(object? sender, EventArgs e)
    {
        if (_loading)
            return;

        ApplyHaloStyle();
        statusLabel.Text = haloEnabledCheckBox.Checked
            ? $"Halo color: {HaloColorValue()}, width: {haloWidthNumeric.Value:0.0}"
            : "Label halo disabled.";
    }

    private void ApplyHaloStyle()
    {
        if (_worldLayerIndex < 0 || haloColorComboBox.SelectedItem is null)
            return;

        geoKernelViewerControl.SetLayerStyle(_worldLayerIndex, HaloStyle());
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: true, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private GeoKernelLayerStyle HaloStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 215,
            LineColor = "#6F8380",
            LineWidth = 0.8,
            ShowLabels = true,
            LabelField = "COUNTRY",
            LabelFontSize = 12.0,
            LabelColor = "#253238",
            LabelHaloEnabled = haloEnabledCheckBox.Checked,
            LabelHaloColor = HaloColorValue(),
            LabelHaloWidth = (double)haloWidthNumeric.Value
        };
    }

    private string HaloColorValue()
    {
        return haloColorComboBox.SelectedItem is HaloColor color ? color.Hex : "#FFFFFF";
    }

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, p =>
    { statusLabel.Text = p.Message; downloadProgressBar.Visible = true; downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee; if (p.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(p.Percentage.Value, 0, 100); });

    private sealed record HaloColor(string Name, string Hex)
    {
        public override string ToString() => Name;
    }
}
