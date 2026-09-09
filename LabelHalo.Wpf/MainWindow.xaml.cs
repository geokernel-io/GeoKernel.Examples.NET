using GeoKernel.Examples.Common;
using System.IO;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LabelHalo.Wpf;

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
        FillHaloColors();

        if (!LoadLayer())
            return;

        ApplyHaloStyle();
        viewerControl.ViewExtent = new GeoKernelExtent(-180.0, -58.0, 180.0, 82.0);
        _loading = false;
        statusText.Text = "Labels use labelHaloEnabled, labelHaloColor and labelHaloWidth.";
    }

    private void FillHaloColors()
    {
        haloColorComboBox.Items.Add(new HaloColor("White", "#FFFFFF"));
        haloColorComboBox.Items.Add(new HaloColor("Black", "#000000"));
        haloColorComboBox.Items.Add(new HaloColor("Yellow", "#FFF2A8"));
        haloColorComboBox.Items.Add(new HaloColor("Blue", "#BAE6FD"));
        haloColorComboBox.SelectedIndex = 0;
    }

    private bool LoadLayer()
    {
        var path = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", Title);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = HaloStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Title);
            return false;
        }

        viewerControl.SetLayerName(0, "World - label halo");
        _worldLayerIndex = viewerControl.GetLayerInfoByName("World - label halo")?.Index ?? 0;
        return true;
    }

    private void HaloControl_Changed(object sender, RoutedEventArgs e)
    {
        if (_loading)
            return;

        haloWidthText.Text = haloWidthSlider.Value.ToString("0.0");
        ApplyHaloStyle();
        statusText.Text = haloEnabledCheckBox.IsChecked == true
            ? $"Halo color: {HaloColorValue()}, width: {haloWidthSlider.Value:0.0}"
            : "Label halo disabled.";
    }

    private void ApplyHaloStyle()
    {
        if (_worldLayerIndex < 0 || haloColorComboBox.SelectedItem is null)
            return;

        viewerControl.SetLayerStyle(_worldLayerIndex, HaloStyle());
        viewerControl.InvalidateRenderCache(clearTileCache: true, clearLayerCache: true);
        viewerControl.RefreshLayers();
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
            LabelHaloEnabled = haloEnabledCheckBox.IsChecked == true,
            LabelHaloColor = HaloColorValue(),
            LabelHaloWidth = haloWidthSlider.Value
        };
    }

    private string HaloColorValue()
    {
        return haloColorComboBox.SelectedItem is HaloColor color ? color.Hex : "#FFFFFF";
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

    private sealed record HaloColor(string Name, string Hex)
    {
        public override string ToString() => Name;
    }
}
