using GeoKernel.Examples.Common;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.InfoTool.Wpf;

public partial class MainWindow
{
    private readonly ObservableCollection<DetailRow> _details = [];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        detailsGrid.ItemsSource = _details;        
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!LoadSampleLayers())
            return;

        ShowEmptyHit();
        SetSampleExtent();
        UpdateStatus("Info tool is active. Click the map to inspect the top feature.");
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
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "InfoTool", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "InfoTool", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(viewerControl.LayerCount - 1);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void HitTest_Click(object sender, RoutedEventArgs e)
    {
        hitTestButton.IsChecked = true;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;
        toolStateText.Text = "Tool: Info | API: HitTestTopFeatureAt";
        UpdateStatus("Click the map to inspect the top feature.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        panButton.IsChecked = true;
        hitTestButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        toolStateText.Text = "Tool: Pan";
        UpdateStatus("Pan mode.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_MapMouseUp(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (e.Tool != GeoKernelViewerTool.Info)
            return;

        var screenX = e.ScreenPoint.X;
        var screenY = e.ScreenPoint.Y;
        var worldPoint = viewerControl.ScreenToWorld(screenX, screenY);
        var hit = viewerControl.HitTestTopFeatureAt(screenX, screenY, 8);
        if (hit is null || !hit.IsValid)
        {
            viewerControl.ClearSelectedFeatures();
            ShowMapClick(screenX, screenY, worldPoint);
            UpdateStatus("Info click received. No feature hit.");
            return;
        }

        viewerControl.ClearSelectedFeatures();
        viewerControl.AddTopFeatureToSelectionAt(screenX, screenY, 8);
        ShowHit(screenX, screenY, worldPoint, hit);
        UpdateStatus($"Info click: {hit.LayerName} feature {hit.ShapeId}, fields={hit.Attributes.Count}.");
    }

    private void ShowEmptyHit()
    {
        _details.Clear();
        _details.Add(new DetailRow("Tool", "GeoKernelViewerTool.Info"));
        _details.Add(new DetailRow("Click", "Click the map to inspect the top feature."));
    }

    private void ShowMapClick(double screenX, double screenY, GeoKernelPoint worldPoint)
    {
        _details.Clear();
        _details.Add(new DetailRow("Tool", "GeoKernelViewerTool.Info"));
        _details.Add(new DetailRow("Event", "Mouse click while Info tool is active"));
        _details.Add(new DetailRow("Screen point", PointText(screenX, screenY)));
        _details.Add(new DetailRow("World point", PointText(worldPoint.X, worldPoint.Y)));
        _details.Add(new DetailRow("Hit", "No feature"));
    }

    private void ShowHit(double screenX, double screenY, GeoKernelPoint worldPoint, GeoKernelFeatureHitTestResult hit)
    {
        _details.Clear();
        _details.Add(new DetailRow("Tool", "GeoKernelViewerTool.Info"));
        _details.Add(new DetailRow("Event", "Mouse click + HitTestTopFeatureAt"));
        _details.Add(new DetailRow("Screen point", PointText(screenX, screenY)));
        _details.Add(new DetailRow("World point", PointText(worldPoint.X, worldPoint.Y)));
        _details.Add(new DetailRow("Layer", hit.LayerName));
        _details.Add(new DetailRow("Layer index", hit.LayerIndex.ToString()));
        _details.Add(new DetailRow("Shape id", hit.ShapeId.ToString()));
        _details.Add(new DetailRow("Feature id", hit.FeatureId.ToString()));
        _details.Add(new DetailRow("Shape type", hit.ShapeType.ToString()));
        _details.Add(new DetailRow("Extent", ExtentText(hit.Extent)));
        _details.Add(new DetailRow("Attribute count", hit.Attributes.Count.ToString()));

        foreach (var pair in hit.Attributes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            _details.Add(new DetailRow(pair.Key, pair.Value?.ToString() ?? "<null>"));
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
        statusText.Text = text;
    }

    private static string ExtentText(GeoKernelExtent extent)
    {
        return $"({extent.XMin:F6}, {extent.YMin:F6}) - ({extent.XMax:F6}, {extent.YMax:F6})";
    }

    private static string PointText(double x, double y)
    {
        return $"({x:F6}, {y:F6})";
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

    private sealed record DetailRow(string Name, string Value);
}
