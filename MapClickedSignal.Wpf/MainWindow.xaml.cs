using GeoKernel.Examples.Common;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.MapClickedSignal.Wpf;

public partial class MainWindow
{
    private readonly ObservableCollection<ClickLogRow> _logRows = [];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        logGrid.ItemsSource = _logRows;        
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!LoadSampleLayers())
            return;

        SetSampleExtent();
        UpdateStatus("Click the map to log mapClicked(tool, screenPoint, worldPoint, modifiers).");
    }

    private bool LoadSampleLayers()
    {
        return AddLayer("world_4326.shp", "World", WorldStyle())
            && AddLayer("usa_states.shp", "USA States", StateStyle())
            && AddLayer("cities_4326.shp", "Cities", CityStyle());
    }

    private bool AddLayer(string fileName, string displayName, GeoKernelLayerStyle style)
    {
        var path = SampleData.EnsureKnownWpfSampleFile(fileName, this);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "MapClickedSignal", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "MapClickedSignal", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(viewerControl.LayerCount - 1);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void Info_Click(object sender, RoutedEventArgs e)
    {
        infoButton.IsChecked = true;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;
        UpdateStatus("Info tool active. Click to emit/log mapClicked.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        panButton.IsChecked = true;
        infoButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan mode. Mouse clicks still report the active tool when released.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_MapMouseUp(object? sender, GeoKernelMapMouseEventArgs e)
    {
        var screenX = e.ScreenPoint.X;
        var screenY = e.ScreenPoint.Y;
        var worldPoint = viewerControl.ScreenToWorld(screenX, screenY);
        var hit = viewerControl.HitTestTopFeatureAt(screenX, screenY, 8);

        if (hit is not null && hit.IsValid)
        {
            viewerControl.ClearSelectedFeatures();
            viewerControl.AddTopFeatureToSelectionAt(screenX, screenY, 8);
        }
        else
        {
            viewerControl.ClearSelectedFeatures();
        }

        AppendClickLog(screenX, screenY, worldPoint, hit);
        UpdateStatus($"mapClicked: tool={e.Tool} screen={PointText(screenX, screenY)} world={PointText(worldPoint.X, worldPoint.Y)} modifiers={ModifiersText()}");
    }

    private void AppendClickLog(double screenX, double screenY, GeoKernelPoint worldPoint, GeoKernelFeatureHitTestResult? hit)
    {
        _logRows.Add(new ClickLogRow(
            DateTime.Now.ToString("HH:mm:ss.fff"),
            viewerControl.ActiveTool.ToString(),
            PointText(screenX, screenY),
            PointText(worldPoint.X, worldPoint.Y),
            ModifiersText(),
            hit is { IsValid: true } ? hit.LayerName : "-",
            hit is { IsValid: true } ? hit.FeatureId.ToString() : "-",
            hit is { IsValid: true } ? hit.ShapeType.ToString() : "-"));

        logGrid.ScrollIntoView(_logRows[^1]);
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
        statusText.Text = text;
    }

    private static string PointText(double x, double y)
    {
        return $"({x:F6}, {y:F6})";
    }

    private static string ModifiersText()
    {
        var modifiers = Keyboard.Modifiers;
        var parts = new List<string>();
        if (modifiers.HasFlag(ModifierKeys.Shift))
            parts.Add("Shift");
        if (modifiers.HasFlag(ModifierKeys.Control))
            parts.Add("Ctrl");
        if (modifiers.HasFlag(ModifierKeys.Alt))
            parts.Add("Alt");
        if (modifiers.HasFlag(ModifierKeys.Windows))
            parts.Add("Windows");
        return parts.Count == 0 ? "-" : string.Join("+", parts);
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

    private sealed record ClickLogRow(
        string Time,
        string Tool,
        string ScreenPoint,
        string WorldPoint,
        string Modifiers,
        string HitLayer,
        string FeatureId,
        string ShapeType);
}
