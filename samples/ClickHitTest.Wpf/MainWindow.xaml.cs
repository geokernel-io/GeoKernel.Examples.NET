using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ClickHitTest.Wpf;

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
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!LoadSampleLayers())
            return;

        ShowEmptyHit();
        SetSampleExtent();
        UpdateStatus("Click the map to run HitTestTopFeatureAt(screenX, screenY, 8).");
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
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "ClickHitTest", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "ClickHitTest", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(0);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void HitTest_Click(object sender, RoutedEventArgs e)
    {
        hitTestButton.IsChecked = true;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Info;
        toolStateText.Text = "Tool: hitTestTopFeatureAt";
        UpdateStatus("Click a feature to inspect the top-most hit.");
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

    private void ViewerControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (viewerControl.ActiveTool != GeoKernelViewerTool.Info)
            return;

        var position = e.GetPosition(viewerControl);
        var hit = viewerControl.HitTestTopFeatureAt(position.X, position.Y, 8);
        if (hit is null || !hit.IsValid)
        {
            viewerControl.ClearSelectedFeatures();
            ShowEmptyHit();
            UpdateStatus("No feature hit.");
            return;
        }

        ShowHit(hit);
        UpdateStatus($"Top hit: {hit.LayerName} feature {hit.ShapeId}");
    }

    private void ShowEmptyHit()
    {
        _details.Clear();
        _details.Add(new DetailRow("Hit", "No feature at clicked point"));
    }

    private void ShowHit(GeoKernelFeatureHitTestResult hit)
    {
        _details.Clear();
        _details.Add(new DetailRow("Layer", hit.LayerName));
        _details.Add(new DetailRow("Layer index", hit.LayerIndex.ToString()));
        _details.Add(new DetailRow("Shape id", hit.ShapeId.ToString()));
        _details.Add(new DetailRow("Feature id", hit.FeatureId.ToString()));
        _details.Add(new DetailRow("Shape type", hit.ShapeType.ToString()));
        _details.Add(new DetailRow("Extent", ExtentText(hit.Extent)));

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

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 210,
            LineColor = "#708984",
            LineWidth = 0.6,
            SelectedLineColor = "#F59E0B",
            SelectedLineWidth = 3.0
        };
    }

    private static GeoKernelLayerStyle StateStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#C7DEE7",
            FillOpacity = 160,
            LineColor = "#2D6F8E",
            LineWidth = 1.0,
            SelectedLineColor = "#F59E0B",
            SelectedLineWidth = 4.0
        };
    }

    private static GeoKernelLayerStyle CityStyle()
    {
        return new GeoKernelLayerStyle
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

    private sealed record DetailRow(string Name, string Value);
}
