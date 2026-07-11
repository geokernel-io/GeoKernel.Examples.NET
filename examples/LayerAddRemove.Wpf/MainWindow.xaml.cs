using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LayerAddRemove.Wpf;

public partial class MainWindow
{
    private readonly Dictionary<string, SampleLayer> _layers;

    public MainWindow()
    {
        InitializeComponent();

        var dataDirectory = Path.Combine(FindRepositoryRoot(), "assets", "data");
        _layers = new Dictionary<string, SampleLayer>(StringComparer.OrdinalIgnoreCase)
        {
            ["World"] = new(
                "World",
                Path.Combine(dataDirectory, "world_4326.shp"),
                new GeoKernelLayerStyle
                {
                    FillColor = "#D8E5E1",
                    FillOpacity = 210,
                    LineColor = "#7B918D",
                    LineWidth = 0.8
                }),
            ["States"] = new(
                "States",
                Path.Combine(dataDirectory, "us_state_boundaries.shp"),
                new GeoKernelLayerStyle
                {
                    FillColor = "#A9C8DB",
                    FillOpacity = 100,
                    LineColor = "#356780",
                    LineWidth = 1.2
                }),
            ["Cities"] = new(
                "Cities",
                Path.Combine(dataDirectory, "usa_cities_4326.kml"),
                new GeoKernelLayerStyle
                {
                    PointColor = "#D95D39",
                    PointSize = 7.0,
                    LineColor = "#D95D39",
                    LineWidth = 1.5
                })
        };
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus();
    }

    private void AddWorld_Click(object sender, RoutedEventArgs e)
    {
        AddLayer("World");
    }

    private void AddStates_Click(object sender, RoutedEventArgs e)
    {
        AddLayer("States");
    }

    private void AddCities_Click(object sender, RoutedEventArgs e)
    {
        AddLayer("Cities");
    }

    private void RemoveWorld_Click(object sender, RoutedEventArgs e)
    {
        RemoveLayer("World");
    }

    private void RemoveStates_Click(object sender, RoutedEventArgs e)
    {
        RemoveLayer("States");
    }

    private void RemoveCities_Click(object sender, RoutedEventArgs e)
    {
        RemoveLayer("Cities");
    }

    private void ClearLayers_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ClearLayers();
        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        UpdateStatus();
    }

    private void AddLayer(string key)
    {
        var layer = _layers[key];
        if (FindLayerIndex(layer) >= 0)
        {
            UpdateStatus($"{layer.Name} is already loaded.");
            return;
        }

        if (!File.Exists(layer.Path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{layer.Path}",
                "LayerAddRemove",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var loaded = viewerControl.AddLayerFile(
            layer.Path,
            new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = layer.Style
            });

        if (!loaded)
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{layer.Path}",
                "LayerAddRemove",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        UpdateStatus($"{layer.Name} added.");
    }

    private void RemoveLayer(string key)
    {
        var layer = _layers[key];
        var removed = false;
        for (var index = viewerControl.LayerCount - 1; index >= 0; --index)
        {
            if (!MatchesLayer(viewerControl.GetLayerInfo(index), layer))
                continue;

            removed = viewerControl.RemoveLayer(index) || removed;
        }

        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        UpdateStatus(removed ? $"{layer.Name} removed." : $"{layer.Name} is not loaded.");
    }

    private int FindLayerIndex(SampleLayer layer)
    {
        for (var index = viewerControl.LayerCount - 1; index >= 0; --index)
        {
            if (MatchesLayer(viewerControl.GetLayerInfo(index), layer))
                return index;
        }

        return -1;
    }

    private static bool MatchesLayer(GeoKernelLayerInfo? info, SampleLayer layer)
    {
        if (info is null)
            return false;

        if (string.Equals(info.Name, layer.Name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(info.DisplayText, layer.Name, StringComparison.OrdinalIgnoreCase))
            return true;

        return !string.IsNullOrWhiteSpace(info.Path) &&
            string.Equals(Path.GetFullPath(info.Path), Path.GetFullPath(layer.Path), StringComparison.OrdinalIgnoreCase);
    }

    private void UpdateStatus(string? message = null)
    {
        statusText.Text = message is null
            ? $"Layers: {viewerControl.LayerCount}"
            : $"{message} Layers: {viewerControl.LayerCount}";
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

    private sealed record SampleLayer(string Name, string Path, GeoKernelLayerStyle Style);
}
