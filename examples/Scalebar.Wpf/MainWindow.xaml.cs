using System.IO;
using System.Drawing;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.Scalebar.Wpf;

public partial class MainWindow
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        var shapefilePath = Path.Combine(FindRepositoryRoot(), "data", "world_4326.shp");
        if (!File.Exists(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be found:{Environment.NewLine}{shapefilePath}",
                "Scalebar",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (!viewerControl.AddLayerFile(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be loaded:{Environment.NewLine}{shapefilePath}",
                "Scalebar",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        ConfigureScaleBar();
        viewerControl.FullExtent();
    }

    private void ConfigureScaleBar()
    {
        viewerControl.ScaleBarVisible = true;
        viewerControl.SetScaleBarAnchor(GeoKernelOverlayAnchor.BottomRight);
        viewerControl.SetScaleBarColors(
            Color.FromArgb(235, 255, 255, 255),
            Color.FromArgb(50, 74, 72),
            Color.FromArgb(35, 50, 48));
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
