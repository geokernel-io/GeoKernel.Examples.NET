using System.IO;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.EditSessionSignals.Wpf;

public partial class MainWindow
{
    private const string EditableLayerName = "Session Signal Points";

    private int _editLayerIndex = -1;
    private int _editPointCursor;
    private int _startedCount;
    private int _committedCount;
    private int _rolledBackCount;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        ConnectEditSessionEvents();

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        SetSampleExtent();
        AppendLog("Ready. Waiting for edit session signals.");
        UpdateUi("Begin an edit session, add a feature, then commit or rollback.");
    }

    private void ConnectEditSessionEvents()
    {
        viewerControl.LayerEditSessionStarted += (_, e) =>
        {
            if (e.LayerIndex != _editLayerIndex)
                return;

            ++_startedCount;
            AppendLog($"signal LayerEditSessionStarted(index={e.LayerIndex}, name={e.LayerName})");
            UpdateUi("LayerEditSessionStarted signal received.");
        };

        viewerControl.LayerEditSessionCommitted += (_, e) =>
        {
            if (e.LayerIndex != _editLayerIndex)
                return;

            ++_committedCount;
            AppendLog($"signal LayerEditSessionCommitted(index={e.LayerIndex}, name={e.LayerName})");
            UpdateUi("LayerEditSessionCommitted signal received.");
        };

        viewerControl.LayerEditSessionRolledBack += (_, e) =>
        {
            if (e.LayerIndex != _editLayerIndex)
                return;

            ++_rolledBackCount;
            AppendLog($"signal LayerEditSessionRolledBack(index={e.LayerIndex}, name={e.LayerName})");
            UpdateUi("LayerEditSessionRolledBack signal received.");
        };
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"World shapefile could not be found:{Environment.NewLine}{path}", "EditSessionSignals");
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = WorldStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", "EditSessionSignals");
            return false;
        }

        viewerControl.SetLayerName(0, "World");
        return true;
    }

    private void CreateEditableLayer()
    {
        _editLayerIndex = viewerControl.AddPointLayer(
            EditableLayerName,
            [
                new GeoKernelPoint(-122.4194, 37.7749),
                new GeoKernelPoint(-118.2437, 34.0522),
                new GeoKernelPoint(-112.0740, 33.4484)
            ],
            EditPointStyle());

        _editLayerIndex = viewerControl.GetLayerInfoByName(EditableLayerName)?.Index ?? _editLayerIndex;
    }

    private void BeginEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        AppendLog($"call BeginEditLayer({_editLayerIndex})");
        if (!viewerControl.BeginEditLayer(_editLayerIndex))
            AppendLog("result BeginEditLayer = false");
    }

    private void AddFeature_Click(object sender, RoutedEventArgs e)
    {
        if (_editLayerIndex < 0 || !viewerControl.IsLayerEditing(_editLayerIndex))
            return;

        var point = GeneratedEditPoint(_editPointCursor);
        AppendLog($"call AddPointToEditLayer({_editLayerIndex}, {point.X:0.###}, {point.Y:0.###})");
        if (!viewerControl.AddPointToEditLayer(_editLayerIndex, point.X, point.Y))
        {
            AppendLog("result AddPointToEditLayer = false");
            return;
        }

        ++_editPointCursor;
        RefreshMap();
        UpdateUi("Feature added inside the active edit session.");
    }

    private void CommitEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        AppendLog($"call CommitEditLayer({_editLayerIndex})");
        if (viewerControl.CommitEditLayer(_editLayerIndex))
            RefreshMap();
        else
            AppendLog("result CommitEditLayer = false");
    }

    private void RollbackEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        AppendLog($"call RollbackEditLayer({_editLayerIndex})");
        if (viewerControl.RollbackEditLayer(_editLayerIndex))
            RefreshMap();
        else
            AppendLog("result RollbackEditLayer = false");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void UpdateUi(string message)
    {
        var editing = _editLayerIndex >= 0 && viewerControl.IsLayerEditing(_editLayerIndex);
        var featureCount = _editLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_editLayerIndex) : 0;

        beginEditButton.IsEnabled = _editLayerIndex >= 0 && !editing;
        addFeatureButton.IsEnabled = editing;
        commitEditButton.IsEnabled = editing;
        rollbackEditButton.IsEnabled = editing;
        stateText.Text = $"Editing: {(editing ? "ON" : "OFF")} | Started: {_startedCount} | Committed: {_committedCount} | Rolled back: {_rolledBackCount} | Feature count: {featureCount}";
        statusText.Text = message;
    }

    private void AppendLog(string text)
    {
        logTextBox.AppendText($"{DateTime.Now:HH:mm:ss.fff} | {text}{Environment.NewLine}");
        logTextBox.ScrollToEnd();
    }

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        viewerControl.ViewExtent = new GeoKernelExtent(-130.0, 20.0, -65.0, 52.0);
    }

    private static GeoKernelPoint GeneratedEditPoint(int index)
    {
        var column = index % 8;
        var row = index / 8 % 4;
        return new GeoKernelPoint(-124.0 + column * 7.5, 25.0 + row * 5.2);
    }

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 210,
            LineColor = "#6F8883",
            LineWidth = 0.7
        };
    }

    private static GeoKernelLayerStyle EditPointStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            LineColor = "#8C321D",
            PointSize = 9.5,
            LineWidth = 1.2
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
