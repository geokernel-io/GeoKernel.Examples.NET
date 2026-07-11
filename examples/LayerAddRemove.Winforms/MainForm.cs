using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerAddRemove.Winforms;

public sealed partial class MainForm : Form
{
    private readonly Dictionary<string, SampleLayer> _layers;

    public MainForm()
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

    private void MainForm_Shown(object sender, EventArgs e)
    {        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus();
    }

    private void addWorldButton_Click(object sender, EventArgs e)
    {
        AddLayer("World");
    }

    private void addStatesButton_Click(object sender, EventArgs e)
    {
        AddLayer("States");
    }

    private void addCitiesButton_Click(object sender, EventArgs e)
    {
        AddLayer("Cities");
    }

    private void removeWorldButton_Click(object sender, EventArgs e)
    {
        RemoveLayer("World");
    }

    private void removeStatesButton_Click(object sender, EventArgs e)
    {
        RemoveLayer("States");
    }

    private void removeCitiesButton_Click(object sender, EventArgs e)
    {
        RemoveLayer("Cities");
    }

    private void clearLayersButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
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
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var loaded = geoKernelViewerControl.AddLayerFile(
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
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        UpdateStatus($"{layer.Name} added.");
    }

    private void RemoveLayer(string key)
    {
        var layer = _layers[key];
        var removed = false;
        for (var index = geoKernelViewerControl.LayerCount - 1; index >= 0; --index)
        {
            if (!MatchesLayer(geoKernelViewerControl.GetLayerInfo(index), layer))
                continue;

            removed = geoKernelViewerControl.RemoveLayer(index) || removed;
        }

        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        UpdateStatus(removed ? $"{layer.Name} removed." : $"{layer.Name} is not loaded.");
    }

    private int FindLayerIndex(SampleLayer layer)
    {
        for (var index = geoKernelViewerControl.LayerCount - 1; index >= 0; --index)
        {
            if (MatchesLayer(geoKernelViewerControl.GetLayerInfo(index), layer))
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
        statusLabel.Text = message is null
            ? $"Layers: {geoKernelViewerControl.LayerCount}"
            : $"{message} Layers: {geoKernelViewerControl.LayerCount}";
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
