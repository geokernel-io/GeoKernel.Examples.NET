using GeoKernel.NET.WinForms;

namespace GeoKernel.EditAndSave.Winforms;

public sealed partial class MainForm : Form
{
    private const string PointLayerName = "Clicked Points";
    private static readonly string[] ShapefileExtensions = [".shp", ".shx", ".dbf", ".prj", ".cpg", ".qix"];
    private int _pointLayerIndex = -1;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        var worldPath = await SampleData.EnsureFileAsync(
            "world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(worldPath))
            return;

        if (!geoKernelViewerControl.AddLayerFile(worldPath, new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = WorldStyle()
            }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{worldPath}",
                "EditAndSave", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        geoKernelViewerControl.SetLayerName(0, "World");
        _pointLayerIndex = geoKernelViewerControl.AddPointLayer(PointLayerName, [], PointStyle());
        if (_pointLayerIndex < 0
            || !geoKernelViewerControl.AddLayerAttributeDefinition(_pointLayerIndex, new GeoKernelAttributeDefinition
                { Name = "NAME", Type = GeoKernelAttributeType.String, Length = 80 })
            || !geoKernelViewerControl.AddLayerAttributeDefinition(_pointLayerIndex, new GeoKernelAttributeDefinition
                { Name = "CREATED", Type = GeoKernelAttributeType.String, Length = 32 }))
        {
            MessageBox.Show(this, "Clicked Points layer could not be created.",
                "EditAndSave", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;
        geoKernelViewerControl.MapMouseUp += geoKernelViewerControl_MapMouseUp;
        ActivatePointEditing();
        SetNextPointAttributes();
        addPointButton.Checked = true;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        UpdateCount();
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 20.0, -65.0, 52.0);
        statusLabel.Text = "Add Point tool active. Click the map to add points.";
    }

    private bool ActivatePointEditing()
    {
        if (_pointLayerIndex < 0)
            return false;
        if (!geoKernelViewerControl.IsLayerEditing(_pointLayerIndex)
            && !geoKernelViewerControl.BeginEditLayer(_pointLayerIndex))
        {
            statusLabel.Text = "Clicked Points layer could not enter edit mode.";
            return false;
        }
        if (!geoKernelViewerControl.SetActiveEditLayerIndex(_pointLayerIndex))
        {
            statusLabel.Text = "Clicked Points layer could not be activated for editing.";
            return false;
        }
        return true;
    }

    private void fullExtentButton_Click(object sender, EventArgs e) => geoKernelViewerControl.FullExtent();

    private void addPointButton_Click(object sender, EventArgs e)
    {
        if (!ActivatePointEditing())
            return;
        addPointButton.Checked = true;
        panButton.Checked = false;
        SetNextPointAttributes();
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        statusLabel.Text = "Add Point tool active. Click the map to add points.";
    }

    private void panButton_Click(object sender, EventArgs e)
    {
        addPointButton.Checked = false;
        panButton.Checked = true;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        statusLabel.Text = "Pan tool active.";
    }

    private void saveButton_Click(object sender, EventArgs e)
    {
        var count = PointCount();
        if (count == 0)
        {
            statusLabel.Text = "There are no points to save.";
            return;
        }

        var path = OutputShapefilePath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        RemoveExistingShapefile(path);
        if (!geoKernelViewerControl.SaveLayerAsShapefile(_pointLayerIndex, path))
        {
            MessageBox.Show(this, $"Shapefile could not be saved:{Environment.NewLine}{path}",
                "EditAndSave", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        statusLabel.Text = $"Saved shapefile: {path}";
        MessageBox.Show(this, $"Saved {count} points to:{Environment.NewLine}{path}",
            "EditAndSave", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void clearButton_Click(object sender, EventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;
        geoKernelViewerControl.RollbackEditLayer(_pointLayerIndex);
        ActivatePointEditing();
        SetNextPointAttributes();
        RefreshMap();
        UpdateCount();
        statusLabel.Text = "Clicked points cleared.";
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _pointLayerIndex)
            return;
        RefreshMap();
        UpdateCount();
        SetNextPointAttributes();
    }

    private void geoKernelViewerControl_MapMouseUp(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (geoKernelViewerControl.ActiveTool != GeoKernelViewerTool.AddPoint)
            return;
        SetNextPointAttributes();
        statusLabel.Text = $"Point click at x={e.WorldPoint.X:F4}, y={e.WorldPoint.Y:F4}";
    }

    private void SetNextPointAttributes()
    {
        if (_pointLayerIndex < 0)
            return;
        geoKernelViewerControl.SetNewFeatureAttributes(new Dictionary<string, object?>
        {
            ["NAME"] = $"Point {PointCount() + 1}",
            ["CREATED"] = DateTime.Now.ToString("O")
        });
    }

    private int PointCount() => _pointLayerIndex < 0 ? 0 : geoKernelViewerControl.GetLayerFeatureCount(_pointLayerIndex);

    private void UpdateCount()
    {
        var count = PointCount();
        countLabel.Text = $"Point count: {count}";
        saveButton.Enabled = count > 0;
    }

    private void RefreshMap()
    {
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: true, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private static string OutputShapefilePath() =>
        Path.Combine(AppContext.BaseDirectory, "EditAndSaveData", "clicked_points.shp");

    private static void RemoveExistingShapefile(string path)
    {
        var basePath = Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path));
        foreach (var extension in ShapefileExtensions)
            File.Delete(basePath + extension);
    }

    private static GeoKernelLayerStyle WorldStyle() => new()
    {
        FillColor = "#D8E5E1", FillOpacity = 210, LineColor = "#6F8883", LineWidth = 0.7
    };

    private static GeoKernelLayerStyle PointStyle() => new()
    {
        PointColor = "#D95D39", LineColor = "#8C321D", PointSize = 9.0, LineWidth = 1.2
    };

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, p =>
    {
        statusLabel.Text = p.Message;
        downloadProgressBar.Visible = true;
        downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (p.Percentage.HasValue)
            downloadProgressBar.Value = Math.Clamp(p.Percentage.Value, 0, 100);
    });
}
