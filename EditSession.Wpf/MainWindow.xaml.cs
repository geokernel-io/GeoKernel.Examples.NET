using GeoKernel.Examples.Common;
using System.IO;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.EditSession.Wpf;

public partial class MainWindow
{
    private const string EditableLayerName = "Editable Cities";

    private int _editLayerIndex = -1;
    private int _initialFeatureCount;
    private int _pendingAdds;
    private int _editPointCursor;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadLayer())
            return;

        CreateEditableLayer();
        SetSampleExtent();
        UpdateUi("Ready. Start an edit session, add points, then commit or rollback.");
    }

    private bool LoadLayer()
    {
        var path = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "EditSession",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        if (!viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = WorldStyle()
                }))
        {
            MessageBox.Show(
                this,
                $"World layer could not be loaded:{Environment.NewLine}{path}",
                "EditSession",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var worldLayer = viewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            viewerControl.SetLayerName(worldLayer.Index, "World");

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
        _initialFeatureCount = FeatureCount();
        _pendingAdds = 0;
        _editPointCursor = 0;
    }

    private void BeginEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        if (viewerControl.BeginEditLayer(_editLayerIndex))
        {
            _initialFeatureCount = FeatureCount();
            _pendingAdds = 0;
            UpdateUi("Edit session started.");
        }
        else
        {
            UpdateUi("Edit session could not be started.");
        }
    }

    private void AddFeature_Click(object sender, RoutedEventArgs e)
    {
        if (_editLayerIndex < 0 || !viewerControl.IsLayerEditing(_editLayerIndex))
            return;

        var point = GeneratedEditPoint(_editPointCursor);
        if (!viewerControl.AddPointToEditLayer(_editLayerIndex, point.X, point.Y))
        {
            UpdateUi("Feature could not be added to the active edit session.");
            return;
        }

        ++_editPointCursor;
        ++_pendingAdds;
        RefreshMap();
        UpdateUi("Feature added inside the active edit session.");
    }

    private void CommitEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        if (viewerControl.CommitEditLayer(_editLayerIndex))
        {
            _initialFeatureCount = FeatureCount();
            _pendingAdds = 0;
            RefreshMap();
            UpdateUi("Edit session committed. Added features remain in the layer.");
        }
        else
        {
            UpdateUi("Edit session could not be committed.");
        }
    }

    private void RollbackEdit_Click(object sender, RoutedEventArgs e)
    {
        if (_editLayerIndex < 0)
            return;

        if (viewerControl.RollbackEditLayer(_editLayerIndex))
        {
            _pendingAdds = 0;
            RefreshMap();
            UpdateUi("Edit session rolled back. Uncommitted features were removed.");
        }
        else
        {
            UpdateUi("Edit session could not be rolled back.");
        }
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void UpdateUi(string message)
    {
        var editing = _editLayerIndex >= 0 && viewerControl.IsLayerEditing(_editLayerIndex);
        var dirty = _editLayerIndex >= 0 && viewerControl.IsLayerDirty(_editLayerIndex);
        var featureCount = FeatureCount();

        beginEditButton.IsEnabled = !editing;
        addFeatureButton.IsEnabled = editing;
        commitEditButton.IsEnabled = editing;
        rollbackEditButton.IsEnabled = editing;

        editStateText.Text = $"Editing: {(editing ? "ON" : "OFF")} | Dirty: {(dirty ? "YES" : "NO")} | Feature count: {featureCount} | Pending adds: {_pendingAdds}";
        statusText.Text = message;
    }

    private int FeatureCount()
    {
        return _editLayerIndex >= 0
            ? viewerControl.GetLayerInfo(_editLayerIndex)?.FeatureCount ?? viewerControl.GetLayerFeatureCount(_editLayerIndex)
            : 0;
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
        var column = index % 11;
        var row = index / 11 % 6;
        var cycle = index / 66;

        return new GeoKernelPoint(
            -124.0 + column * 5.6 + cycle * 0.35,
            25.0 + row * 4.2 + cycle * 0.35);
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
            PointColor = "#D85B35",
            LineColor = "#8C321D",
            PointSize = 9.0,
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
