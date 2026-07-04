using System.IO;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.EditAndSave.Wpf;

public partial class MainWindow
{
    private const string WorldLayerName = "World";
    private const string WorkingLayerName = "Clicked Points Working Copy";
    private static readonly string[] ShapefileExtensions = [".shp", ".shx", ".dbf", ".prj", ".cpg"];
    private static readonly string[] OutputCleanupExtensions = [".shp", ".shx", ".dbf", ".prj", ".cpg", ".qix", ".pgidx"];

    private int _pointLayerIndex = -1;
    private int _lastCommittedCount;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;

        if (!PrepareWorkingCopy())
            return;

        LoadMap();
        SetSampleExtent();
        UpdateUi("Add Point active. Click the map, then Commit To File.");
    }

    private void LoadMap()
    {
        viewerControl.ClearLayers();
        LoadLayer();
        LoadWorkingLayer(beginEdit: true);
        RefreshMap();
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
            return false;

        if (!viewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = WorldStyle()
                }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", "EditAndSave");
            return false;
        }

        viewerControl.SetLayerName(0, WorldLayerName);
        return true;
    }

    private bool LoadWorkingLayer(bool beginEdit)
    {
        _pointLayerIndex = -1;
        if (!viewerControl.AddLayerFile(
                WorkingShapefilePath(),
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = PointStyle()
                }))
        {
            MessageBox.Show(this, $"Working shapefile could not be loaded:{Environment.NewLine}{WorkingShapefilePath()}", "EditAndSave");
            return false;
        }

        var info = viewerControl.GetLayerInfo(viewerControl.LayerCount - 1);
        if (info is not null)
        {
            _pointLayerIndex = info.Index;
            viewerControl.SetLayerName(_pointLayerIndex, WorkingLayerName);
        }

        _lastCommittedCount = FeatureCount();

        if (beginEdit)
            BeginPointEditing();

        return true;
    }

    private void BeginPointEditing()
    {
        if (_pointLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_pointLayerIndex))
            viewerControl.BeginEditLayer(_pointLayerIndex);

        viewerControl.SetActiveEditLayerIndex(_pointLayerIndex);
        viewerControl.SetNewFeatureAttributes(NewPointAttributes());
    }

    private void ResetCopy_Click(object sender, RoutedEventArgs e)
    {
        CloseWorkingLayer();

        if (!PrepareWorkingCopy())
            return;

        LoadMap();
        SetSampleExtent();
        UpdateUi("Working copy reset from assets/data/cities_4326.shp.");
    }

    private void AddPoint_Click(object sender, RoutedEventArgs e)
    {
        addPointButton.IsChecked = true;
        panButton.IsChecked = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        BeginPointEditing();
        UpdateUi("Add Point active. Click the map to add points.");
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        addPointButton.IsChecked = false;
        panButton.IsChecked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateUi("Pan active.");
    }

    private void Commit_Click(object sender, RoutedEventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        if (!viewerControl.IsLayerEditing(_pointLayerIndex))
        {
            UpdateUi("Nothing to commit. The working layer is not editing.");
            return;
        }

        if (!viewerControl.CommitEditLayer(_pointLayerIndex))
        {
            UpdateUi("CommitEditLayer failed.");
            return;
        }

        _lastCommittedCount = FeatureCount();
        RefreshMap();
        UpdateUi("CommitEditLayer wrote changes to the working shapefile.");
    }

    private void Reload_Click(object sender, RoutedEventArgs e)
    {
        CloseWorkingLayer();
        LoadWorkingLayer(beginEdit: true);
        viewerControl.ActiveTool = addPointButton.IsChecked == true ? GeoKernelViewerTool.AddPoint : GeoKernelViewerTool.Pan;
        RefreshMap();
        UpdateUi("Working shapefile reloaded from disk.");
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        SetSampleExtent();
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _pointLayerIndex)
            return;

        RefreshMap();
        viewerControl.SetNewFeatureAttributes(NewPointAttributes());
        UpdateUi("Working layer edit state changed.");
    }

    private void CloseWorkingLayer()
    {
        if (_pointLayerIndex >= 0 && viewerControl.IsLayerEditing(_pointLayerIndex))
            viewerControl.RollbackEditLayer(_pointLayerIndex);

        viewerControl.RemoveLayerByName(WorkingLayerName);
        _pointLayerIndex = -1;
    }

    private bool PrepareWorkingCopy()
    {
        var source = Path.Combine(FindRepositoryRoot(), "assets", "data", "cities_4326.shp");
        if (!File.Exists(source))
        {
            MessageBox.Show(this, $"Source point shapefile could not be found:{Environment.NewLine}{source}", "EditAndSave");
            return false;
        }

        Directory.CreateDirectory(OutputDirectory());
        RemoveShapefileFiles(WorkingShapefilePath(), OutputCleanupExtensions);

        var sourceBase = Path.Combine(Path.GetDirectoryName(source)!, Path.GetFileNameWithoutExtension(source));
        var targetBase = Path.Combine(OutputDirectory(), Path.GetFileNameWithoutExtension(WorkingShapefilePath()));
        foreach (var extension in ShapefileExtensions)
        {
            var sourceFile = sourceBase + extension;
            if (File.Exists(sourceFile))
                File.Copy(sourceFile, targetBase + extension, overwrite: true);
        }

        return File.Exists(WorkingShapefilePath());
    }

    private void UpdateUi(string message)
    {
        var editing = _pointLayerIndex >= 0 && viewerControl.IsLayerEditing(_pointLayerIndex);
        var dirty = _pointLayerIndex >= 0 && viewerControl.IsLayerDirty(_pointLayerIndex);
        var count = FeatureCount();

        stateText.Text = $"Editing: {(editing ? "ON" : "OFF")} | Dirty: {(dirty ? "YES" : "NO")} | Feature count: {count}";
        infoTextBox.Text = InfoText(count, editing, dirty);
        statusText.Text = message;
    }

    private string InfoText(int count, bool editing, bool dirty)
    {
        return string.Join(Environment.NewLine,
            "EditAndSave sample",
            "",
            "Workflow:",
            "1. Reset Working Copy copies assets/data/cities_4326.shp under the output folder.",
            "2. Add Point edits that copied shapefile, not the original asset.",
            "3. Commit To File calls CommitEditLayer(index).",
            "4. Reload From File opens the same copied shapefile again.",
            "",
            "Working shapefile:",
            WorkingShapefilePath(),
            "",
            $"Editing: {editing}",
            $"Dirty: {dirty}",
            $"Feature count: {count}",
            $"Last committed count: {_lastCommittedCount}",
            "",
            "Files:",
            FileState(".shp"),
            FileState(".dbf"),
            FileState(".shx"));
    }

    private string FileState(string extension)
    {
        var path = Path.ChangeExtension(WorkingShapefilePath(), extension);
        if (!File.Exists(path))
            return $"{extension}: missing";

        var info = new FileInfo(path);
        return $"{extension}: {info.Length} bytes, modified {info.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
    }

    private int FeatureCount()
    {
        return _pointLayerIndex >= 0 ? viewerControl.GetLayerFeatureCount(_pointLayerIndex) : 0;
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

    private Dictionary<string, object?> NewPointAttributes()
    {
        return new Dictionary<string, object?>
        {
            ["NAME"] = $"Added {DateTime.Now:HHmmss}",
            ["POP_MAX"] = 0,
            ["POP_MIN"] = 0
        };
    }

    private static void RemoveShapefileFiles(string shpPath, IEnumerable<string> extensions)
    {
        var basePath = Path.Combine(Path.GetDirectoryName(shpPath)!, Path.GetFileNameWithoutExtension(shpPath));
        foreach (var extension in extensions)
        {
            var path = basePath + extension;
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static string OutputDirectory()
    {
        return Path.Combine(AppContext.BaseDirectory, "EditAndSaveData");
    }

    private static string WorkingShapefilePath()
    {
        return Path.Combine(OutputDirectory(), "cities_4326_working.shp");
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

    private static GeoKernelLayerStyle PointStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            LineColor = "#8C321D",
            PointSize = 8.0,
            LineWidth = 1.1
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
