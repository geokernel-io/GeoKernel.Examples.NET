using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.SelectionSignal.Wpf;

public partial class MainWindow
{
    private readonly ObservableCollection<SignalRow> _signalRows = [];
    private SelectionMode _selectionMode = SelectionMode.Add;
    private int _eventNumber;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        detailsGrid.ItemsSource = _signalRows;
        viewerControl.SelectionChanged += ViewerControl_SelectionChanged;
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!LoadSampleLayers())
            return;

        SetSampleExtent();
        AppendEvent("ready", viewerControl.SelectedFeatureCount, "Click = add, toggle button = toggle.");
        UpdateSelectionState("Click a feature to trigger SelectionChanged.");
    }

    private bool LoadSampleLayers()
    {
        return AddLayer("world_4326.shp", "World", WorldStyle())
            && AddLayer("usa_states_4326.shp", "USA States", StateStyle())
            && AddLayer("cities_4326.shp", "Cities", CityStyle());
    }

    private bool AddLayer(string fileName, string displayName, GeoKernelLayerStyle style)
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", fileName);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "SelectionSignal", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "SelectionSignal", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(viewerControl.LayerCount - 1);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void Add_Click(object sender, RoutedEventArgs e)
    {
        SetSelectionMode(SelectionMode.Add);
    }

    private void Toggle_Click(object sender, RoutedEventArgs e)
    {
        SetSelectionMode(SelectionMode.Toggle);
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        addButton.IsChecked = false;
        toggleButton.IsChecked = false;
        panButton.IsChecked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateSelectionState("Pan mode.");
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ClearSelectedFeatures();
        UpdateSelectionState("Selection cleared.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (viewerControl.ActiveTool != GeoKernelViewerTool.Info)
            return;

        var position = e.GetPosition(viewerControl);
        var ok = _selectionMode == SelectionMode.Toggle
            ? viewerControl.ToggleTopFeatureSelectionAt(position.X, position.Y, 8)
            : viewerControl.AddTopFeatureToSelectionAt(position.X, position.Y, 8);

        UpdateSelectionState(ok
            ? (_selectionMode == SelectionMode.Toggle ? "toggleSelectedFeature applied." : "addSelectedFeature applied.")
            : "No feature hit.");
    }

    private void ViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        AppendEvent("SelectionChanged", e.SelectedFeatureCount, "Native viewer selectionChanged signal fired.");
        UpdateSelectionState($"SelectionChanged({e.SelectedFeatureCount})");
    }

    private void SetSelectionMode(SelectionMode mode)
    {
        _selectionMode = mode;
        addButton.IsChecked = mode == SelectionMode.Add;
        toggleButton.IsChecked = mode == SelectionMode.Toggle;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;
        UpdateSelectionState(mode == SelectionMode.Toggle
            ? "Click a feature to toggle it in selection."
            : "Click a feature to add it to selection.");
    }

    private void AppendEvent(string eventName, int selectedCount, string message)
    {
        _signalRows.Add(new SignalRow((++_eventNumber).ToString(), eventName, selectedCount.ToString(), message));
        detailsGrid.ScrollIntoView(_signalRows[^1]);
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateSelectionState(string text)
    {
        toolStateText.Text = $"Selected: {viewerControl.SelectedFeatureCount} | Signal: SelectionChanged";
        statusText.Text = text;
    }

    private static GeoKernelLayerStyle WorldStyle() => new()
    {
        FillColor = "#D8E5E1",
        FillOpacity = 210,
        LineColor = "#708984",
        LineWidth = 0.6,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 3.0
    };

    private static GeoKernelLayerStyle StateStyle() => new()
    {
        FillColor = "#C7DEE7",
        FillOpacity = 160,
        LineColor = "#2D6F8E",
        LineWidth = 1.0,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 4.0
    };

    private static GeoKernelLayerStyle CityStyle() => new()
    {
        PointColor = "#D95D39",
        LineColor = "#8C321D",
        PointSize = 8.0,
        LineWidth = 1.0,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 4.0,
        ShowLabels = true,
        LabelField = "NAME",
        LabelFontSize = 9.0,
        LabelColor = "#263238",
        LabelHaloEnabled = true,
        LabelHaloColor = "#FFFFFF",
        LabelHaloWidth = 2.0
    };

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

    private enum SelectionMode
    {
        Add,
        Toggle
    }

    private sealed record SignalRow(string Number, string EventName, string SelectedCount, string Message);
}
