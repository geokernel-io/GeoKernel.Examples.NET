using System.IO;
using System.Drawing;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AddLayers.Wpf;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        var dataDirectory = Path.Combine(FindRepositoryRoot(), "data");

        viewerControl.ClearLayers();
        viewerControl.AddOpenStreetMapLayer();

        if (!AddSampleLayer(Path.Combine(dataDirectory, "usa_3857.tif")))
            return;

        if (!AddSampleLayer(
            Path.Combine(dataDirectory, "usa_states_3857.shp"),
            new GeoKernelLayerStyle
            {
                FillColor = "#D8E5E1",
                FillOpacity = 140,
                LineColor = "#5F7874",
                LineWidth = 1.0
            }))
            return;

        if (!AddSampleLayer(
            Path.Combine(dataDirectory, "usa_cities_4326.kml"),
            new GeoKernelLayerStyle
            {
                PointColor = "#D95D39",
                PointSize = 8.0,
                LineColor = "#D95D39",
                LineWidth = 1.5
            }))
            return;

        viewerControl.ZoomToLayer(2);
    }

    private bool AddSampleLayer(string path, GeoKernelLayerStyle? style = null)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "AddLayers",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var loaded = style is null
            ? viewerControl.AddLayerFile(path)
            : viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                });

        if (!loaded)
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{path}",
                "AddLayers",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        return true;
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ZoomIn();
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ZoomOut();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.FullExtent();
    }

    private void ZoomRect_Click(object sender, RoutedEventArgs e)
    {
        SetTool(GeoKernelViewerTool.ZoomBox);
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        viewerControl.ActiveTool = tool;
        zoomRectButton.IsChecked = tool == GeoKernelViewerTool.ZoomBox;
        panButton.IsChecked = tool == GeoKernelViewerTool.Pan;
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "data")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
