using System.IO;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.EditAndSave.Wpf;

public partial class MainWindow
{
    private const string PointLayerName = "Clicked Points";
    private static readonly string[] ShapefileExtensions = [".shp", ".shx", ".dbf", ".prj", ".cpg", ".qix"];
    private int _pointLayerIndex = -1;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        var worldPath = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
        if (string.IsNullOrWhiteSpace(worldPath))
            return;

        if (!viewerControl.AddLayerFile(worldPath, new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = WorldStyle()
            }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{worldPath}", Title,
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        viewerControl.SetLayerName(0, "World");
        _pointLayerIndex = viewerControl.AddPointLayer(PointLayerName, [], PointStyle());
        if (_pointLayerIndex < 0
            || !viewerControl.AddLayerAttributeDefinition(_pointLayerIndex, new GeoKernelAttributeDefinition
                { Name = "NAME", Type = GeoKernelAttributeType.String, Length = 80 })
            || !viewerControl.AddLayerAttributeDefinition(_pointLayerIndex, new GeoKernelAttributeDefinition
                { Name = "CREATED", Type = GeoKernelAttributeType.String, Length = 32 }))
        {
            MessageBox.Show(this, "Clicked Points layer could not be created.", Title,
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        viewerControl.LayerEditStateChanged += ViewerControl_LayerEditStateChanged;
        if (!ActivatePointEditing())
            return;
        SetNextPointAttributes();
        addPointButton.IsChecked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        UpdateCount();
        viewerControl.ViewExtent = new GeoKernelExtent(-130.0, 20.0, -65.0, 52.0);
        statusText.Text = "Add Point tool active. Click the map to add points.";
    }

    private bool ActivatePointEditing()
    {
        if (_pointLayerIndex < 0)
            return false;
        if (!viewerControl.IsLayerEditing(_pointLayerIndex) && !viewerControl.BeginEditLayer(_pointLayerIndex))
        {
            statusText.Text = "Clicked Points layer could not enter edit mode.";
            return false;
        }
        if (!viewerControl.SetActiveEditLayerIndex(_pointLayerIndex))
        {
            statusText.Text = "Clicked Points layer could not be activated for editing.";
            return false;
        }
        return true;
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e) => viewerControl.FullExtent();

    private void AddPoint_Click(object sender, RoutedEventArgs e)
    {
        if (!ActivatePointEditing())
            return;
        addPointButton.IsChecked = true;
        panButton.IsChecked = false;
        SetNextPointAttributes();
        viewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        statusText.Text = "Add Point tool active. Click the map to add points.";
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        addPointButton.IsChecked = false;
        panButton.IsChecked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        statusText.Text = "Pan tool active.";
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        var count = PointCount();
        if (count == 0)
        {
            statusText.Text = "There are no points to save.";
            return;
        }

        var path = OutputShapefilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        RemoveExistingShapefile(path);
        if (!viewerControl.SaveLayerAsShapefile(_pointLayerIndex, path))
        {
            MessageBox.Show(this, $"Shapefile could not be saved:{Environment.NewLine}{path}", Title,
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        statusText.Text = $"Saved shapefile: {path}";
        MessageBox.Show(this, $"Saved {count} points to:{Environment.NewLine}{path}", Title,
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;
        viewerControl.RollbackEditLayer(_pointLayerIndex);
        ActivatePointEditing();
        SetNextPointAttributes();
        RefreshMap();
        UpdateCount();
        statusText.Text = "Clicked points cleared.";
    }

    private void ViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _pointLayerIndex)
            return;
        RefreshMap();
        UpdateCount();
        SetNextPointAttributes();
    }

    private void ViewerControl_MapMouseUp(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (e.Tool != GeoKernelViewerTool.AddPoint)
            return;
        SetNextPointAttributes();
        statusText.Text = $"Point click at x={e.WorldPoint.X:F4}, y={e.WorldPoint.Y:F4}";
    }

    private void SetNextPointAttributes()
    {
        if (_pointLayerIndex < 0)
            return;
        viewerControl.SetNewFeatureAttributes(new Dictionary<string, object?>
        {
            ["NAME"] = $"Point {PointCount() + 1}",
            ["CREATED"] = DateTime.Now.ToString("O")
        });
    }

    private int PointCount() =>
        _pointLayerIndex < 0 ? 0 : viewerControl.GetLayerFeatureCount(_pointLayerIndex);

    private void UpdateCount()
    {
        var count = PointCount();
        countText.Text = $"Point count: {count}";
        saveButton.IsEnabled = count > 0;
    }

    private void RefreshMap()
    {
        viewerControl.InvalidateRenderCache(clearTileCache: true, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private static string OutputShapefilePath() =>
        Path.Combine(AppContext.BaseDirectory, "EditAndSaveData", "clicked_points.shp");

    private static void RemoveExistingShapefile(string path)
    {
        var basePath = Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path));
        foreach (var extension in ShapefileExtensions)
        {
            var file = basePath + extension;
            if (File.Exists(file))
                File.Delete(file);
        }
    }

    private static GeoKernelLayerStyle WorldStyle() => new()
    {
        FillColor = "#D8E5E1", FillOpacity = 210, LineColor = "#6F8883", LineWidth = 0.7
    };

    private static GeoKernelLayerStyle PointStyle() => new()
    {
        PointColor = "#D95D39", LineColor = "#8C321D", PointSize = 9.0, LineWidth = 1.2
    };
}
