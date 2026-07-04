using System.IO;
using System.Drawing;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ScaleBasedLayerVisibility.Wpf;

public partial class MainWindow : Window
{
    private double _currentScale;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.ZoomChanged += (_, e) =>
        {
            _currentScale = e.ZoomScale;
            RefreshLayerList();
        };
        viewerControl.LayersChanged += (_, _) => RefreshLayerList();

        LoadLayers();
        viewerControl.ViewExtent = new GeoKernelExtent(-151.2, 16.4, -41.6, 55.6);
        RefreshLayerList();
    }

    private void LoadLayers()
    {
        var dataDirectory = Path.Combine(FindRepositoryRoot(), "assets", "data");

        AddLayer(
            "World",
            Path.Combine(dataDirectory, "world_4326.shp"),
            WorldStyle(),
            minVisibleScale: 0.0,
            maxVisibleScale: 11.0);

        AddLayer(
            "States",
            Path.Combine(dataDirectory, "usa_states_3857.shp"),
            StatesStyle(),
            minVisibleScale: 5.0,
            maxVisibleScale: 45.0);

        AddLayer(
            "Cities",
            Path.Combine(dataDirectory, "usa_cities_4326.kml"),
            CitiesStyle(),
            minVisibleScale: 28.0,
            maxVisibleScale: 0.0);
    }

    private void AddLayer(
        string name,
        string path,
        GeoKernelLayerStyle style,
        double minVisibleScale,
        double maxVisibleScale)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException("Layer file could not be found.", path);

        if (!viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                }))
        {
            throw new InvalidOperationException($"Layer could not be loaded: {path}");
        }

        var layer = viewerControl.GetLayerInfo(0)
            ?? throw new InvalidOperationException("Loaded layer info could not be read.");

        viewerControl.SetLayerName(layer.Index, name);
        viewerControl.SetLayerVisibleScaleRange(layer.Index, minVisibleScale, maxVisibleScale);
        viewerControl.RefreshLayers();
    }

    private void RefreshLayerList()
    {
        scaleText.Text = $"Current scale: {ScaleText(_currentScale)} px/map unit";
        layerListBox.Items.Clear();

        foreach (var layer in viewerControl.GetLayersInfo())
            layerListBox.Items.Add(LayerListText(layer));
    }

    private string LayerListText(GeoKernelLayerInfo layer)
    {
        var visibleAtScale = IsVisibleAtScale(layer, _currentScale);
        return $"{(visibleAtScale ? "[x]" : "[ ]")} [{ScaleText(layer.MinVisibleScale),5} - {ScaleText(layer.MaxVisibleScale),5}] {layer.DisplayText}";
    }

    private static bool IsVisibleAtScale(GeoKernelLayerInfo layer, double scale)
    {
        if (!layer.Visible)
            return false;

        if (layer.MinVisibleScale > 0.0 && scale < layer.MinVisibleScale)
            return false;

        if (layer.MaxVisibleScale > 0.0 && scale > layer.MaxVisibleScale)
            return false;

        return true;
    }

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 225,
            LineColor = "#7B918D",
            LineWidth = 0.8
        };
    }

    private static GeoKernelLayerStyle StatesStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#A9C8DB",
            FillOpacity = 135,
            LineColor = "#356780",
            LineWidth = 1.1
        };
    }

    private static GeoKernelLayerStyle CitiesStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            PointSize = 7.0,
            LineColor = "#873A24",
            LineWidth = 1.0
        };
    }

    private static string ScaleText(double value)
    {
        if (value <= 0.0)
            return "-";

        return value < 10.0 ? value.ToString("0.00") : value.ToString("0");
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
