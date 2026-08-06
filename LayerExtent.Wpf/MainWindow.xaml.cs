using GeoKernel.Examples.Common;
using System.IO;
using System.Drawing;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LayerExtent.Wpf;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadLayer())
            return;

        if (!AddLayerExtentRectangle(0))
            return;

        viewerControl.FullExtent();
    }

    private bool LoadLayer()
    {
        var path = SampleData.EnsureKnownWpfSampleFile("california/california.shp", this);
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "LayerExtent",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var style = new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 210,
            LineColor = "#6F8883",
            LineWidth = 0.9
        };

        if (!viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                }))
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{path}",
                "LayerExtent",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(0);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, "California");

        return true;
    }

    private bool AddLayerExtentRectangle(int layerIndex)
    {
        var extent = viewerControl.GetLayerProjectedExtent(layerIndex);
        if (extent.XMax <= extent.XMin || extent.YMax <= extent.YMin)
        {
            MessageBox.Show(
                this,
                "Layer extent could not be calculated.",
                "LayerExtent",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        GeoKernelPoint[] rectangle =
        [
            new(extent.XMin, extent.YMin),
            new(extent.XMax, extent.YMin),
            new(extent.XMax, extent.YMax),
            new(extent.XMin, extent.YMax),
            new(extent.XMin, extent.YMin)
        ];

        var style = new GeoKernelLayerStyle
        {
            FillColor = "#FFFFFF",
            FillOpacity = 0,
            LineColor = "#E2453D",
            LineWidth = 2.2
        };

        var index = viewerControl.AddPolygonLayer("Layer Extent", rectangle, style);
        if (index < 0)
        {
            MessageBox.Show(
                this,
                "Layer extent rectangle could not be created.",
                "LayerExtent",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        statusText.Text = $"California extent: {extent.XMin:0.###}, {extent.YMin:0.###} - {extent.XMax:0.###}, {extent.YMax:0.###}";
        return true;
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
