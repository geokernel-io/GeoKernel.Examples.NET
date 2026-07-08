using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LayerEvents.Wpf;

public partial class MainWindow : Window
{
    private bool _refreshingLayerList;

    public MainWindow()
    {
        InitializeComponent();
        ConnectLayerEvents();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        AddLayer("World", "world_4326.shp", WorldStyle());
        AddLayer("States", "us_state_boundaries.shp", StatesStyle());
        AddLayer("Cities", "usa_cities_4326.kml", CitiesStyle());
        RefreshLayerList();
        viewerControl.ViewExtent = new GeoKernelExtent(-151.2, 16.4, -41.6, 55.6);
    }

    private void ConnectLayerEvents()
    {
        viewerControl.LayersChanged += (_, _) =>
        {
            var selectedIndex = layerListBox.SelectedIndex;
            RefreshLayerList(selectedIndex);
            AppendLog($"Event: LayersChanged(count={viewerControl.LayerCount})");
        };
        viewerControl.LayerAdded += (_, e) => AppendLog($"Event: LayerAdded({LayerText(e)})");
        viewerControl.LayerRemoved += (_, e) => AppendLog($"Event: LayerRemoved({LayerText(e)})");
        viewerControl.LayerVisibilityChanged += (_, e) =>
            AppendLog($"Event: LayerVisibilityChanged(index={e.LayerIndex}, visible={e.Visible})");
        viewerControl.LayerEditStateChanged += (_, e) => AppendLog($"Event: LayerEditStateChanged({LayerText(e)})");
        viewerControl.LayerEditSessionStarted += (_, e) => AppendLog($"Event: LayerEditSessionStarted({LayerText(e)})");
        viewerControl.LayerEditSessionCommitted += (_, e) => AppendLog($"Event: LayerEditSessionCommitted({LayerText(e)})");
        viewerControl.LayerEditSessionRolledBack += (_, e) => AppendLog($"Event: LayerEditSessionRolledBack({LayerText(e)})");
        viewerControl.LayerOrderChanged += (_, _) => AppendLog("Event: LayerOrderChanged()");
        viewerControl.BusyChanged += (_, e) => AppendLog($"Event: BusyChanged({e.Busy})");
        viewerControl.ViewChanged += (_, _) => UpdateStatus();
    }

    private bool AddLayer(string name, string fileName, GeoKernelLayerStyle style)
    {
        if (viewerControl.GetLayerInfoByName(name) is not null)
        {
            AppendLog($"Action skipped: {name} already exists");
            return true;
        }

        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", fileName);
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "LayerEvents",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        AppendLog($"Action: AddLayerFile({fileName})");
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
                "LayerEvents",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(0);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, name);

        viewerControl.RefreshLayers();
        return true;
    }

    private void RemoveSelectedLayer()
    {
        var index = layerListBox.SelectedIndex;
        var layer = index >= 0 ? viewerControl.GetLayerInfo(index) : null;
        if (layer is null)
            return;

        AppendLog($"Action: RemoveLayer({layer.Name})");
        viewerControl.RemoveLayer(index);
    }

    private void ToggleSelectedLayer()
    {
        var index = layerListBox.SelectedIndex;
        var layer = index >= 0 ? viewerControl.GetLayerInfo(index) : null;
        if (layer is null)
            return;

        AppendLog($"Action: SetLayerVisible({layer.Name}, {!layer.Visible})");
        viewerControl.SetLayerVisible(index, !layer.Visible);
    }

    private void MoveSelectedLayer(int delta)
    {
        var fromIndex = layerListBox.SelectedIndex;
        var toIndex = fromIndex + delta;
        if (fromIndex < 0 || toIndex < 0 || toIndex >= viewerControl.LayerCount)
            return;

        AppendLog($"Action: MoveLayer({fromIndex} -> {toIndex})");
        if (viewerControl.MoveLayer(fromIndex, toIndex))
            RefreshLayerList(toIndex);
    }

    private void RefreshLayerList(int selectedIndex = -1)
    {
        _refreshingLayerList = true;
        try
        {
            layerListBox.Items.Clear();
            foreach (var layer in viewerControl.GetLayersInfo())
                layerListBox.Items.Add($"{(layer.Visible ? "[x]" : "[ ]")} {layer.DisplayText}");

            if (selectedIndex >= 0 && selectedIndex < layerListBox.Items.Count)
                layerListBox.SelectedIndex = selectedIndex;
            else if (layerListBox.Items.Count > 0)
                layerListBox.SelectedIndex = 0;
        }
        finally
        {
            _refreshingLayerList = false;
        }

        UpdateStatus();
    }

    private void AppendLog(string text)
    {
        eventLogTextBox.AppendText($"{DateTime.Now:HH:mm:ss.fff}  {text}{Environment.NewLine}");
        eventLogTextBox.ScrollToEnd();
    }

    private void UpdateStatus()
    {
        if (_refreshingLayerList)
            return;

        statusText.Text = $"Layers: {viewerControl.LayerCount}";
    }

    private void AddWorld_Click(object sender, RoutedEventArgs e)
    {
        AddLayer("World", "world_4326.shp", WorldStyle());
    }

    private void AddStates_Click(object sender, RoutedEventArgs e)
    {
        AddLayer("States", "us_state_boundaries.shp", StatesStyle());
    }

    private void AddCities_Click(object sender, RoutedEventArgs e)
    {
        AddLayer("Cities", "usa_cities_4326.kml", CitiesStyle());
    }

    private void RemoveSelected_Click(object sender, RoutedEventArgs e)
    {
        RemoveSelectedLayer();
    }

    private void ClearLayers_Click(object sender, RoutedEventArgs e)
    {
        AppendLog("Action: ClearLayers()");
        viewerControl.ClearLayers();
    }

    private void ToggleVisibility_Click(object sender, RoutedEventArgs e)
    {
        ToggleSelectedLayer();
    }

    private void MoveUp_Click(object sender, RoutedEventArgs e)
    {
        MoveSelectedLayer(-1);
    }

    private void MoveDown_Click(object sender, RoutedEventArgs e)
    {
        MoveSelectedLayer(1);
    }

    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        AppendLog("Action: RefreshLayers()");
        viewerControl.RefreshLayers();
    }

    private void ClearLog_Click(object sender, RoutedEventArgs e)
    {
        eventLogTextBox.Clear();
    }

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 220,
            LineColor = "#7B918D",
            LineWidth = 0.8
        };
    }

    private static GeoKernelLayerStyle StatesStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#A9C8DB",
            FillOpacity = 115,
            LineColor = "#356780",
            LineWidth = 1.2
        };
    }

    private static GeoKernelLayerStyle CitiesStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            PointSize = 7.0,
            LineColor = "#D95D39",
            LineWidth = 1.5
        };
    }

    private static string LayerText(GeoKernelLayerEventArgs e)
    {
        return string.IsNullOrWhiteSpace(e.LayerName)
            ? $"index={e.LayerIndex}"
            : $"{e.LayerName}, index={e.LayerIndex}";
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
