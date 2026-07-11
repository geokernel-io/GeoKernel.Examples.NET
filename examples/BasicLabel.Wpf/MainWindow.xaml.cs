using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.BasicLabel.Wpf;

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

        if (!LoadLayer())
            return;

        FillLabelFields();
        ApplyLabelStyle();
        viewerControl.ViewExtent = new GeoKernelExtent(-180.0, -58.0, 180.0, 82.0);
        _loading = false;
        statusText.Text = "Labels use showLabels, labelField and labelFontSize.";
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", Title);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = LabeledWorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Title);
            return false;
        }

        viewerControl.SetLayerName(0, "World - labels");
        _worldLayerIndex = viewerControl.GetLayerInfoByName("World - labels")?.Index ?? 0;
        return true;
    }

    private void FillLabelFields()
    {
        fieldComboBox.Items.Clear();

        foreach (var definition in viewerControl.GetLayerAttributeDefinitions(_worldLayerIndex))
            fieldComboBox.Items.Add(definition.Name);

        var countryIndex = fieldComboBox.Items.IndexOf("COUNTRY");
        fieldComboBox.SelectedIndex = countryIndex >= 0 ? countryIndex : fieldComboBox.Items.Count > 0 ? 0 : -1;
    }

    private void LabelControl_Changed(object sender, RoutedEventArgs e)
    {
        if (_loading)
            return;

        ApplyLabelStyle();
        statusText.Text = showLabelsCheckBox.IsChecked == true
            ? $"Label field: {fieldComboBox.Text}, font size: {FontSizeValue():0.0}"
            : "Labels disabled.";
    }

    private void ApplyLabelStyle()
    {
        if (_worldLayerIndex < 0 || fieldComboBox.SelectedItem is null)
            return;

        viewerControl.SetLayerStyle(_worldLayerIndex, LabeledWorldStyle());
        viewerControl.InvalidateRenderCache(clearTileCache: true, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private GeoKernelLayerStyle LabeledWorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 215,
            LineColor = "#6F8380",
            LineWidth = 0.8,
            ShowLabels = showLabelsCheckBox.IsChecked == true,
            LabelField = fieldComboBox.SelectedItem as string ?? "COUNTRY",
            LabelFontSize = FontSizeValue(),
            LabelColor = "#FFFF00",
            LabelHaloEnabled = true,
            LabelHaloColor = "#000000",
            LabelHaloWidth = 2.0
        };
    }

    private double FontSizeValue()
    {
        return double.TryParse(fontSizeTextBox.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? Math.Clamp(value, 5.0, 32.0)
            : 9.0;
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
