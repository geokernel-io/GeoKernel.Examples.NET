using System.IO;
using System.Drawing;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LayerRefresh.Wpf;

public partial class MainWindow
{
    private readonly string[] _fillColors = ["#D8E5E1", "#D9C7A5", "#C7D7EA", "#D7C5DE"];
    private readonly string[] _outlineColors = ["#6F8883", "#A24A3D", "#356780", "#6F4D8C"];
    private readonly int[] _opacities = [210, 160, 110, 235];
    private int _fillIndex;
    private int _outlineIndex;
    private int _opacityIndex;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadLayer())
            return;

        viewerControl.RefreshLayers();
        viewerControl.FullExtent();
        UpdateStatus("Layer loaded. Change style, then press Refresh Layer.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "california", "california.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "LayerRefresh",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = CurrentStyle()
                }))
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{path}",
                "LayerRefresh",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(0);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, "California");

        return true;
    }

    private void ChangeFill_Click(object sender, RoutedEventArgs e)
    {
        _fillIndex = (_fillIndex + 1) % _fillColors.Length;
        UpdateStatus("Fill changed. Press Refresh Layer to redraw.");
    }

    private void ChangeOutline_Click(object sender, RoutedEventArgs e)
    {
        _outlineIndex = (_outlineIndex + 1) % _outlineColors.Length;
        UpdateStatus("Outline changed. Press Refresh Layer to redraw.");
    }

    private void ChangeOpacity_Click(object sender, RoutedEventArgs e)
    {
        _opacityIndex = (_opacityIndex + 1) % _opacities.Length;
        UpdateStatus("Opacity changed. Press Refresh Layer to redraw.");
    }

    private void RefreshLayer_Click(object sender, RoutedEventArgs e)
    {
        if (!viewerControl.SetLayerStyle(0, CurrentStyle()))
        {
            UpdateStatus("Style could not be applied.");
            return;
        }

        viewerControl.RefreshLayers();
        UpdateStatus("Layer refreshed.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.FullExtent();
    }

    private GeoKernelLayerStyle CurrentStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = _fillColors[_fillIndex],
            FillOpacity = _opacities[_opacityIndex],
            LineColor = _outlineColors[_outlineIndex],
            LineWidth = _outlineIndex == 0 ? 0.9 : 1.6
        };
    }

    private void UpdateStatus(string message)
    {
        statusText.Text =
            $"{message} Fill: {_fillColors[_fillIndex]} | Outline: {_outlineColors[_outlineIndex]} | Opacity: {_opacities[_opacityIndex]}";
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
