using GeoKernel.Examples.Common;
using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LayerVisibility.Wpf;

public partial class MainWindow : Window
{
    private bool _refreshingLayerList;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        LoadLayers();
        SetTool(GeoKernelViewerTool.Pan);
    }

    private void LoadLayers()
    {
        AddLayer(SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this), "World", new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 220,
            LineColor = "#7B918D",
            LineWidth = 0.8
        });

        AddLayer(SampleData.EnsureKnownWpfSampleFile("usa_states.shp", this), "States", new GeoKernelLayerStyle
        {
            FillColor = "#A9C8DB",
            FillOpacity = 115,
            LineColor = "#356780",
            LineWidth = 1.2
        });

        AddLayer(SampleData.EnsureKnownWpfSampleFile("usa_cities.shp", this), "Cities", new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            PointSize = 7.0,
            LineColor = "#D95D39",
            LineWidth = 1.5
        });

        RefreshLayerList();
        viewerControl.ViewExtent = new GeoKernelExtent(-151.2, 16.4, -41.6, 55.6);
    }

    private void AddLayer(string path, string name, GeoKernelLayerStyle style)
    {
        var options = new GeoKernelLayerLoadOptions
        {
            ApplyDefaultStyle = true,
            DefaultStyle = style
        };

        if (!viewerControl.AddLayerFile(path, options))
            throw new InvalidOperationException($"Layer could not be loaded: {path}");

        var info = viewerControl.GetLayerInfo(0);
        if (info is not null)
            viewerControl.SetLayerName(info.Index, name);
    }

    private void RefreshLayerList(int selectedIndex = -1)
    {
        _refreshingLayerList = true;
        try
        {
            layerListBox.Items.Clear();
            foreach (var layer in viewerControl.GetLayersInfo())
                layerListBox.Items.Add(LayerListText(layer));

            if (layerListBox.Items.Count > 0)
                layerListBox.SelectedIndex = Math.Clamp(selectedIndex < 0 ? 0 : selectedIndex, 0, layerListBox.Items.Count - 1);
        }
        finally
        {
            _refreshingLayerList = false;
        }

        UpdateVisibilityButton();
        UpdateStatus();
    }

    private static string LayerListText(GeoKernelLayerInfo layer)
    {
        return $"{(layer.Visible ? "[x]" : "[ ]")} {layer.DisplayText}";
    }

    private void ToggleSelectedLayerVisibility()
    {
        var index = layerListBox.SelectedIndex;
        if (index < 0)
            return;

        var layer = viewerControl.GetLayerInfo(index);
        if (layer is null)
            return;

        if (!viewerControl.SetLayerVisible(index, !layer.Visible))
            return;

        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        RefreshLayerList(index);
    }

    private void UpdateVisibilityButton()
    {
        var index = layerListBox.SelectedIndex;
        var layer = index >= 0 ? viewerControl.GetLayerInfo(index) : null;
        visibilityButton.IsEnabled = layer is not null;
        visibilityButton.Content = layer is null
            ? "Change Visibility"
            : $"Change Visibility: {(layer.Visible ? "Hide" : "Show")}";
    }

    private void UpdateStatus()
    {
        statusText.Text = $"Layers: {viewerControl.LayerCount}";
    }

    private void Visibility_Click(object sender, RoutedEventArgs e)
    {
        ToggleSelectedLayerVisibility();
    }

    private void LayerListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (!_refreshingLayerList)
            UpdateVisibilityButton();
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
        var directory = AppContext.BaseDirectory;
        while (!string.IsNullOrEmpty(directory))
        {
            if (Directory.Exists(Path.Combine(directory, "assets")))
                return directory;

            var parent = Directory.GetParent(directory);
            directory = parent?.FullName ?? string.Empty;
        }

        throw new DirectoryNotFoundException("Could not locate GeoKernel repository root.");
    }
}
