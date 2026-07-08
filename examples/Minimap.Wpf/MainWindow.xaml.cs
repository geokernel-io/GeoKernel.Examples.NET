using System.IO;
using System.Drawing;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.Minimap.Wpf;

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

        var shapefilePath = Path.Combine(FindRepositoryRoot(), "data", "world_4326.shp");
        if (!File.Exists(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be found:{Environment.NewLine}{shapefilePath}",
                "Minimap",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (!viewerControl.AddLayerFile(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be loaded:{Environment.NewLine}{shapefilePath}",
                "Minimap",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        ConfigureMiniMap();
        viewerControl.FullExtent();
    }

    private void ConfigureMiniMap()
    {
        viewerControl.MiniMapVisible = true;
        viewerControl.SetMiniMapAnchor(GeoKernelOverlayAnchor.TopRight);
        viewerControl.SetMiniMapBackgroundColor(Color.FromArgb(235, 244, 246, 245));
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
