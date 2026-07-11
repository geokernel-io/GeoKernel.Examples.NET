using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ClearRenderer.Wpf;

public partial class MainWindow : Window
{
    private const string StateLayerName = "USA States";
    private int _statesLayerIndex = -1;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadStatesLayer())
            return;

        ApplyCategorizedRenderer();
        viewerControl.ViewExtent = new GeoKernelExtent(-16831516.0, 1856556.0, -4631023.0, 7472472.0);
        statusText.Text = "Categorized renderer applied. Use Clear Renderer to return to the default layer style.";
    }

    private bool LoadStatesLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "usa_states_3857.shp");
        if (!File.Exists(path))
        {
            System.Windows.MessageBox.Show(
                this,
                $"States shapefile could not be found:{Environment.NewLine}{path}",
                "ClearRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = DefaultStateStyle()
            }))
        {
            System.Windows.MessageBox.Show(
                this,
                $"States layer could not be loaded:{Environment.NewLine}{path}",
                "ClearRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(0);
        if (layer is null)
        {
            System.Windows.MessageBox.Show(
                this,
                "Loaded states layer could not be inspected.",
                "ClearRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        _statesLayerIndex = layer.Index;
        viewerControl.SetLayerName(_statesLayerIndex, StateLayerName);
        ApplyBaseStateStyle();
        return true;
    }

    private void ApplyRendererButton_Click(object sender, RoutedEventArgs e)
    {
        ApplyCategorizedRenderer();
    }

    private void ClearRendererButton_Click(object sender, RoutedEventArgs e)
    {
        if (_statesLayerIndex < 0)
            return;

        if (!viewerControl.ClearLayerSymbolRenderer(_statesLayerIndex))
        {
            statusText.Text = "Renderer could not be cleared.";
            return;
        }

        ApplyBaseStateStyle();
        rendererStateText.Text = "Renderer: none, default layer style";
        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        viewerControl.RefreshLayers();
        statusText.Text = "Symbol renderer cleared. Layer is back to the default style.";
    }

    private void ApplyCategorizedRenderer()
    {
        if (_statesLayerIndex < 0)
            return;

        ApplyBaseStateStyle();

        if (!viewerControl.ApplyLayerCategorizedRenderer(
                _statesLayerIndex,
                "STATE",
                GeoKernelColorRampNames.Unique,
                categoryLimit: 64))
        {
            System.Windows.MessageBox.Show(
                this,
                "Could not create categorized renderer from STATE field.",
                "ClearRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        rendererStateText.Text = "Renderer: categorized by STATE";
        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        viewerControl.RefreshLayers();
        statusText.Text = "Categorized renderer applied.";
    }

    private void ApplyBaseStateStyle()
    {
        if (_statesLayerIndex < 0)
            return;

        viewerControl.SetLayerStyle(_statesLayerIndex, DefaultStateStyle());
    }

    private static GeoKernelLayerStyle DefaultStateStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 220,
            LineColor = "#536B68",
            LineWidth = 0.9
        };
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
