using System.IO;
using System.Drawing;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.MultiWindowSync.Wpf;

public partial class MainWindow : Window
{
    private bool _syncing;

    public MainWindow()
    {
        InitializeComponent();
        ConnectViewSynchronization();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);

        if (!LoadLayer(leftViewerControl, "World A") ||
            !LoadLayer(rightViewerControl, "World B"))
        {
            return;
        }

        var initialExtent = new GeoKernelExtent(-151.2, 16.4, -41.6, 55.6);
        leftViewerControl.ViewExtent = initialExtent;
        rightViewerControl.ViewExtent = initialExtent;
        UpdateStatus();
    }

    private void ConnectViewSynchronization()
    {
        leftViewerControl.VisibleExtentChanged += (_, e) => SyncExtent(leftViewerControl, rightViewerControl, e.Extent);
        rightViewerControl.VisibleExtentChanged += (_, e) => SyncExtent(rightViewerControl, leftViewerControl, e.Extent);
    }

    private void SyncExtent(GeoKernelViewerControl source, GeoKernelViewerControl target, GeoKernelExtent extent)
    {
        if (syncButton.IsChecked != true || _syncing)
            return;

        try
        {
            _syncing = true;
            target.ViewExtent = extent;
            statusText.Text = source == leftViewerControl
                ? "Viewer A -> Viewer B"
                : "Viewer B -> Viewer A";
        }
        finally
        {
            _syncing = false;
        }
    }

    private bool LoadLayer(GeoKernelViewerControl viewer, string layerName)
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "MultiWindowSync",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var loaded = viewer.AddLayerFile(
            path,
            new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = new GeoKernelLayerStyle
                {
                    FillColor = "#D8E5E1",
                    FillOpacity = 220,
                    LineColor = "#6F8883",
                    LineWidth = 0.8
                }
            });

        if (!loaded)
        {
            MessageBox.Show(
                this,
                $"World layer could not be loaded:{Environment.NewLine}{path}",
                "MultiWindowSync",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var layer = viewer.GetLayerInfo(0);
        if (layer is not null)
            viewer.SetLayerName(layer.Index, layerName);

        viewer.RefreshLayers();
        return true;
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        leftViewerControl.ActiveTool = tool;
        rightViewerControl.ActiveTool = tool;
        zoomRectButton.IsChecked = tool == GeoKernelViewerTool.ZoomBox;
        panButton.IsChecked = tool == GeoKernelViewerTool.Pan;
    }

    private void UpdateStatus()
    {
        statusText.Text = syncButton.IsChecked == true
            ? "Sync enabled. Drive either viewer."
            : "Sync disabled.";
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        leftViewerControl.ZoomIn();
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        leftViewerControl.ZoomOut();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        leftViewerControl.FullExtent();
    }

    private void Sync_Click(object sender, RoutedEventArgs e)
    {
        syncButton.Content = syncButton.IsChecked == true ? "Sync On" : "Sync Off";
        if (syncButton.IsChecked == true)
        {
            try
            {
                _syncing = true;
                rightViewerControl.ViewExtent = leftViewerControl.ViewExtent;
            }
            finally
            {
                _syncing = false;
            }
        }

        UpdateStatus();
    }

    private void ZoomRect_Click(object sender, RoutedEventArgs e)
    {
        SetTool(GeoKernelViewerTool.ZoomBox);
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);
    }

    private static string FindRepositoryRoot()
    {
        var directory = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(directory))
        {
            if (Directory.Exists(Path.Combine(directory, "assets", "data")))
                return directory;

            var parent = Directory.GetParent(directory);
            directory = parent?.FullName ?? string.Empty;
        }

        throw new DirectoryNotFoundException("Could not locate GeoKernel repository root.");
    }
}
