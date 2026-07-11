using System.Drawing.Text;
using GeoKernel.NET.WinForms;

namespace GeoKernel.LabelFont.Winforms;

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
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        FillFonts();

        if (!LoadLayer())
            return;

        ApplyLabelFont();
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-180.0, -58.0, 180.0, 82.0);
        _loading = false;
        statusLabel.Text = "Labels use labelFontFamily, labelBold and labelItalic.";
    }

    private void FillFonts()
    {
        using var fonts = new InstalledFontCollection();
        foreach (var family in fonts.Families.Select(family => family.Name).OrderBy(name => name))
            fontFamilyComboBox.Items.Add(family);

        var arialIndex = fontFamilyComboBox.Items.IndexOf("Arial");
        fontFamilyComboBox.SelectedIndex = arialIndex >= 0 ? arialIndex : fontFamilyComboBox.Items.Count > 0 ? 0 : -1;
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = LabelFontStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        geoKernelViewerControl.SetLayerName(0, "World - label font");
        _worldLayerIndex = geoKernelViewerControl.GetLayerInfoByName("World - label font")?.Index ?? 0;
        return true;
    }

    private void labelFontControl_Changed(object? sender, EventArgs e)
    {
        if (_loading)
            return;

        ApplyLabelFont();
        statusLabel.Text = $"Font: {fontFamilyComboBox.Text}, bold: {boldCheckBox.Checked}, italic: {italicCheckBox.Checked}";
    }

    private void ApplyLabelFont()
    {
        if (_worldLayerIndex < 0 || fontFamilyComboBox.SelectedItem is null)
            return;

        geoKernelViewerControl.SetLayerStyle(_worldLayerIndex, LabelFontStyle());
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: true, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private GeoKernelLayerStyle LabelFontStyle()
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
            LabelColor = "#1F2933",
            LabelHaloEnabled = true,
            LabelHaloColor = "#FFFFFF",
            LabelHaloWidth = 2.0,
            LabelFontFamily = fontFamilyComboBox.SelectedItem as string ?? "Arial",
            LabelBold = boldCheckBox.Checked,
            LabelItalic = italicCheckBox.Checked
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
