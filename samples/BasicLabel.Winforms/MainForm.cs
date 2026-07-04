using GeoKernel.NET.WinForms;

namespace GeoKernel.BasicLabel.Winforms;

public sealed partial class MainForm : Form
{
    private int _worldLayerIndex = -1;
    private bool _loading = true;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadLayer())
            return;

        FillLabelFields();
        ApplyLabelStyle();
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-180.0, -58.0, 180.0, 82.0);
        _loading = false;
        statusLabel.Text = "Labels use showLabels, labelField and labelFontSize.";
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = LabeledWorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        geoKernelViewerControl.SetLayerName(0, "World - labels");
        _worldLayerIndex = geoKernelViewerControl.GetLayerInfoByName("World - labels")?.Index ?? 0;
        return true;
    }

    private void FillLabelFields()
    {
        fieldComboBox.Items.Clear();

        foreach (var definition in geoKernelViewerControl.GetLayerAttributeDefinitions(_worldLayerIndex))
            fieldComboBox.Items.Add(definition.Name);

        var countryIndex = fieldComboBox.Items.IndexOf("COUNTRY");
        fieldComboBox.SelectedIndex = countryIndex >= 0 ? countryIndex : fieldComboBox.Items.Count > 0 ? 0 : -1;
    }

    private void labelControl_Changed(object? sender, EventArgs e)
    {
        if (_loading)
            return;

        ApplyLabelStyle();
        statusLabel.Text = showLabelsCheckBox.Checked
            ? $"Label field: {fieldComboBox.Text}, font size: {fontSizeNumeric.Value:0.0}"
            : "Labels disabled.";
    }

    private void ApplyLabelStyle()
    {
        if (_worldLayerIndex < 0 || fieldComboBox.SelectedItem is null)
            return;

        geoKernelViewerControl.SetLayerStyle(_worldLayerIndex, LabeledWorldStyle());
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: true, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private GeoKernelLayerStyle LabeledWorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 215,
            LineColor = "#6F8380",
            LineWidth = 0.8,
            ShowLabels = showLabelsCheckBox.Checked,
            LabelField = fieldComboBox.SelectedItem as string ?? "COUNTRY",
            LabelFontSize = (double)fontSizeNumeric.Value,
            LabelColor = "#FFFF00",
            LabelHaloEnabled = true,
            LabelHaloColor = "#000000",
            LabelHaloWidth = 2.0
        };
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "assets", "data")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
