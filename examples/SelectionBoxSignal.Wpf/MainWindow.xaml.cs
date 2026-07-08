using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.SelectionBoxSignal.Wpf;

public partial class MainWindow
{
    private readonly ObservableCollection<SignalRow> _signalRows = [];
    private readonly ObservableCollection<HitRow> _hitRows = [];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        signalGrid.ItemsSource = _signalRows;
        hitsGrid.ItemsSource = _hitRows;
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.Select;
        viewerControl.MapSelectionBoxFinished += ViewerControl_MapSelectionBoxFinished;

        if (!LoadSampleLayers())
            return;

        ShowEmptyHits();
        SetSampleExtent();
        UpdateStatus("Drag a box with Select to emit MapSelectionBoxFinished(rect, extent, modifiers).");
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
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "SelectionBoxSignal", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "SelectionBoxSignal", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(viewerControl.LayerCount - 1);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void Select_Click(object sender, RoutedEventArgs e)
    {
        selectButton.IsChecked = true;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Select;
        UpdateStatus("Drag a box to emit MapSelectionBoxFinished.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        panButton.IsChecked = true;
        selectButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan mode.");
    }

    private void ClearSelection_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ClearSelectedFeatures();
        ShowEmptyHits();
        UpdateStatus("Selection cleared.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_MapSelectionBoxFinished(object? sender, GeoKernelSelectionBoxFinishedEventArgs e)
    {
        var hits = viewerControl.HitTestFeaturesInScreenRectangle(e.ScreenRectangle)
            .Where(hit => hit.IsValid)
            .ToList();

        AppendSignalLog(e, hits.Count);
        ShowHits(hits);
        UpdateStatus($"MapSelectionBoxFinished: rect={RectText(e.ScreenRectangle)} extent={ExtentText(e.WorldExtent)} modifiers={ModifiersText(e.Modifiers)} hits={hits.Count}.");
    }

    private void AppendSignalLog(GeoKernelSelectionBoxFinishedEventArgs e, int hitCount)
    {
        _signalRows.Add(new SignalRow(
            DateTime.Now.ToString("HH:mm:ss.fff"),
            RectText(e.ScreenRectangle),
            ExtentText(e.WorldExtent),
            ModifiersText(e.Modifiers),
            hitCount.ToString()));

        signalGrid.ScrollIntoView(_signalRows[^1]);
    }

    private void ShowEmptyHits()
    {
        _hitRows.Clear();
        _hitRows.Add(new HitRow("-", "Drag a selection box to list matching features.", "-", "-", "-"));
    }

    private void ShowHits(IReadOnlyList<GeoKernelFeatureHitTestResult> hits)
    {
        _hitRows.Clear();

        for (var i = 0; i < hits.Count; i++)
        {
            var hit = hits[i];
            _hitRows.Add(new HitRow((i + 1).ToString(), hit.LayerName, hit.ShapeId.ToString(), hit.FeatureId.ToString(), hit.ShapeType.ToString()));
        }

        if (hits.Count == 0)
            _hitRows.Add(new HitRow("-", "No hits", "-", "-", "-"));
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
        statusText.Text = text;
    }

    private static string RectText(GeoKernelScreenRectangle rect)
    {
        return $"left={rect.Left} top={rect.Top} width={rect.Width} height={rect.Height}";
    }

    private static string ExtentText(GeoKernelExtent extent)
    {
        return $"({extent.XMin:F6}, {extent.YMin:F6}) - ({extent.XMax:F6}, {extent.YMax:F6})";
    }

    private static string ModifiersText(int modifiers)
    {
        if (modifiers == 0)
            return "-";

        var parts = new List<string>();
        if ((modifiers & 0x02000000) != 0)
            parts.Add("Shift");
        if ((modifiers & 0x04000000) != 0)
            parts.Add("Ctrl");
        if ((modifiers & 0x08000000) != 0)
            parts.Add("Alt");
        if ((modifiers & 0x10000000) != 0)
            parts.Add("Meta");
        return parts.Count == 0 ? modifiers.ToString() : string.Join("+", parts);
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
        FillOpacity = 155,
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

    private sealed record SignalRow(string Time, string ScreenRect, string WorldExtent, string Modifiers, string HitCount);
    private sealed record HitRow(string Number, string Layer, string ShapeId, string FeatureId, string ShapeType);
}
