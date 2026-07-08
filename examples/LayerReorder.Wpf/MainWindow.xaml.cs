using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LayerReorder.Wpf;

public partial class MainWindow
{
    private bool _refreshingLayerList;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        SetTool(GeoKernelViewerTool.Pan);

        if (!LoadLayers())
            return;

        RefreshLayerList();
        viewerControl.ViewExtent = new GeoKernelExtent(-151.2, 16.4, -41.6, 55.6);
    }

    private bool LoadLayers()
    {
        var dataDirectory = Path.Combine(FindRepositoryRoot(), "assets", "data");

        return AddLayer(
                "World",
                Path.Combine(dataDirectory, "world_4326.shp"),
                new GeoKernelLayerStyle
                {
                    FillColor = "#D8E5E1",
                    FillOpacity = 220,
                    LineColor = "#7B918D",
                    LineWidth = 0.8
                })
            && AddLayer(
                "States",
                Path.Combine(dataDirectory, "us_state_boundaries.shp"),
                new GeoKernelLayerStyle
                {
                    FillColor = "#A9C8DB",
                    FillOpacity = 115,
                    LineColor = "#356780",
                    LineWidth = 1.2
                })
            && AddLayer(
                "Cities",
                Path.Combine(dataDirectory, "usa_cities_4326.kml"),
                new GeoKernelLayerStyle
                {
                    PointColor = "#D95D39",
                    PointSize = 7.0,
                    LineColor = "#D95D39",
                    LineWidth = 1.5
                });
    }

    private bool AddLayer(string name, string path, GeoKernelLayerStyle style)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "LayerReorder",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

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
                "LayerReorder",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var info = viewerControl.GetLayerInfo(0);
        if (info is not null)
            viewerControl.SetLayerName(info.Index, name);

        return true;
    }

    private void RefreshLayerList(int selectedIndex = -1)
    {
        _refreshingLayerList = true;
        try
        {
            layerListBox.Items.Clear();

            foreach (var layer in viewerControl.GetLayersInfo())
                layerListBox.Items.Add(layer.DisplayText);

            if (selectedIndex >= 0 && selectedIndex < layerListBox.Items.Count)
                layerListBox.SelectedIndex = selectedIndex;
            else if (layerListBox.Items.Count > 0)
                layerListBox.SelectedIndex = 0;
        }
        finally
        {
            _refreshingLayerList = false;
        }

        UpdateButtons();
        UpdateStatus();
    }

    private void MoveSelectedLayer(int delta)
    {
        var fromIndex = layerListBox.SelectedIndex;
        if (fromIndex < 0)
            return;

        var toIndex = fromIndex + delta;
        if (toIndex < 0 || toIndex >= viewerControl.LayerCount)
            return;

        if (!viewerControl.MoveLayer(fromIndex, toIndex))
            return;

        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        RefreshLayerList(toIndex);
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

    private void MoveUp_Click(object sender, RoutedEventArgs e)
    {
        MoveSelectedLayer(-1);
    }

    private void MoveDown_Click(object sender, RoutedEventArgs e)
    {
        MoveSelectedLayer(1);
    }

    private void LayerListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_refreshingLayerList)
            UpdateButtons();
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        viewerControl.ActiveTool = tool;
        zoomRectButton.IsChecked = tool == GeoKernelViewerTool.ZoomBox;
        panButton.IsChecked = tool == GeoKernelViewerTool.Pan;
    }

    private void UpdateButtons()
    {
        var selectedIndex = layerListBox.SelectedIndex;
        moveUpButton.IsEnabled = selectedIndex > 0;
        moveDownButton.IsEnabled = selectedIndex >= 0 && selectedIndex < layerListBox.Items.Count - 1;
    }

    private void UpdateStatus()
    {
        statusText.Text = $"Layers: {viewerControl.LayerCount}";
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
