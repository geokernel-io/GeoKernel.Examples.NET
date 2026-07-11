using System.IO;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LabelFont.Wpf;

public partial class MainWindow
{
    private int _worldLayerIndex = -1;
    private bool _loading = true;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        FillFonts();

        if (!LoadLayer())
            return;

        ApplyLabelFont();
        viewerControl.ViewExtent = new GeoKernelExtent(-180.0, -58.0, 180.0, 82.0);
        _loading = false;
        statusText.Text = "Labels use labelFontFamily, labelBold and labelItalic.";
    }

    private void FillFonts()
    {
        foreach (var family in Fonts.SystemFontFamilies.Select(family => family.Source).OrderBy(name => name))
            fontFamilyComboBox.Items.Add(family);

        var arialIndex = fontFamilyComboBox.Items.IndexOf("Arial");
        fontFamilyComboBox.SelectedIndex = arialIndex >= 0 ? arialIndex : fontFamilyComboBox.Items.Count > 0 ? 0 : -1;
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", Title);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = LabelFontStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Title);
            return false;
        }

        viewerControl.SetLayerName(0, "World - label font");
        _worldLayerIndex = viewerControl.GetLayerInfoByName("World - label font")?.Index ?? 0;
        return true;
    }

    private void LabelFontControl_Changed(object sender, RoutedEventArgs e)
    {
        if (_loading)
            return;

        ApplyLabelFont();
        statusText.Text = $"Font: {fontFamilyComboBox.Text}, bold: {boldCheckBox.IsChecked == true}, italic: {italicCheckBox.IsChecked == true}";
    }

    private void ApplyLabelFont()
    {
        if (_worldLayerIndex < 0 || fontFamilyComboBox.SelectedItem is null)
            return;

        viewerControl.SetLayerStyle(_worldLayerIndex, LabelFontStyle());
        viewerControl.InvalidateRenderCache(clearTileCache: true, clearLayerCache: true);
        viewerControl.RefreshLayers();
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
            LabelBold = boldCheckBox.IsChecked == true,
            LabelItalic = italicCheckBox.IsChecked == true
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
