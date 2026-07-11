using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.SelectAdd.Wpf;

public partial class MainWindow
{
    private readonly ObservableCollection<SelectedRow> _selectedRows = [];
    private SelectionMode _selectionMode = SelectionMode.Add;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        detailsGrid.ItemsSource = _selectedRows;        
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!LoadSampleLayers())
            return;

        RefreshSelectedFeatures("No selected features.");
        SetSampleExtent();
        UpdateStatus("Click a feature to add it to selection.");
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
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "SelectAdd", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "SelectAdd", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(0);
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
        toolStateText.Text = "Tool: Pan";
        UpdateStatus("Pan mode.");
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ClearSelectedFeatures();
        RefreshSelectedFeatures("Selection cleared.");
        UpdateStatus("Selection cleared.");
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

        if (!ok)
        {
            UpdateStatus("No feature hit.");
            return;
        }

        RefreshSelectedFeatures();
        UpdateStatus(_selectionMode == SelectionMode.Toggle
            ? "toggleSelectedFeature applied."
            : "addSelectedFeature applied.");
    }

    private void SetSelectionMode(SelectionMode mode)
    {
        _selectionMode = mode;
        addButton.IsChecked = mode == SelectionMode.Add;
        toggleButton.IsChecked = mode == SelectionMode.Toggle;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;
        toolStateText.Text = mode == SelectionMode.Toggle
            ? "Click = toggleSelectedFeature"
            : "Click = addSelectedFeature";
        UpdateStatus(mode == SelectionMode.Toggle
            ? "Click a feature to toggle it in selection."
            : "Click a feature to add it to selection.");
    }

    private void RefreshSelectedFeatures(string? emptyMessage = null)
    {
        var selected = viewerControl.GetSelectedFeatures();
        _selectedRows.Clear();

        for (var i = 0; i < selected.Count; i++)
        {
            var hit = selected[i];
            _selectedRows.Add(new SelectedRow(
                (i + 1).ToString(),
                hit.LayerName,
                hit.ShapeId.ToString(),
                hit.FeatureId.ToString(),
                hit.ShapeType.ToString()));
        }

        if (selected.Count == 0)
            _selectedRows.Add(new SelectedRow("-", emptyMessage ?? "No selected features.", "-", "-", "-"));
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
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

    private sealed record SelectedRow(string Number, string Layer, string ShapeId, string FeatureId, string ShapeType);
}
