using GeoKernel.NET.WinForms;

namespace GeoKernel.ClickHitTest.Winforms;

public sealed partial class MainForm : Form
{
    private readonly Dictionary<string, string> _samplePaths = new(StringComparer.OrdinalIgnoreCase);
    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        if (!await PrepareSampleDataAsync())
            return;


        ConfigureDetailsGrid();
        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!LoadSampleLayers())
            return;

        ShowEmptyHit();
        SetSampleExtent();
        UpdateStatus("Click the map to run HitTestTopFeatureAt(screenX, screenY, 8).");
    }

    private async Task<bool> PrepareSampleDataAsync()
    {
        var progress = CreateSampleProgress();
        var world = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, progress);
        var states = await SampleData.EnsureFileAsync("usa_states.zip", "usa_states", "usa_states.shp", "USA states", this, progress);
        var cities = await SampleData.EnsureFileAsync("cities_4326.zip", "cities_4326", "cities_4326.shp", "Cities", this, progress);
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(world) || string.IsNullOrEmpty(states) || string.IsNullOrEmpty(cities))
            return false;

        _samplePaths["world_4326.shp"] = world;
        _samplePaths["usa_states.shp"] = states;
        _samplePaths["cities_4326.shp"] = cities;
        return true;
    }

    private bool LoadSampleLayers()
    {
        return AddLayer("world_4326.shp", "World", WorldStyle())
            && AddLayer("usa_states.shp", "USA States", StateStyle())
            && AddLayer("cities_4326.shp", "Cities", CityStyle());
    }

    private bool AddLayer(string fileName, string displayName, GeoKernelLayerStyle style)
    {
        if (!_samplePaths.TryGetValue(fileName, out var path))
            return false;
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "ClickHitTest", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "ClickHitTest", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(geoKernelViewerControl.LayerCount - 1);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void hitTestButton_Click(object? sender, EventArgs e)
    {
        hitTestButton.Checked = true;
        panButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;
        toolStateLabel.Text = "Tool: hitTestTopFeatureAt";
        UpdateStatus("Click a feature to inspect the top-most hit.");
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        panButton.Checked = true;
        hitTestButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        toolStateLabel.Text = "Tool: Pan";
        UpdateStatus("Pan mode.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_MapMouseUp(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (e.Tool != GeoKernelViewerTool.Info)
            return;

        var hit = geoKernelViewerControl.HitTestTopFeatureAt(e.ScreenPoint.X, e.ScreenPoint.Y, 8);
        if (hit is null || !hit.IsValid)
        {
            geoKernelViewerControl.ClearSelectedFeatures();
            ShowEmptyHit();
            UpdateStatus("No feature hit.");
            return;
        }

        ShowHit(hit);
        UpdateStatus($"Top hit: {hit.LayerName} feature {hit.ShapeId}");
    }

    private void ConfigureDetailsGrid()
    {
        detailsGrid.Columns.Clear();
        detailsGrid.Columns.Add("Property", "Property / Field");
        detailsGrid.Columns.Add("Value", "Value");
    }

    private void ShowEmptyHit()
    {
        detailsGrid.Rows.Clear();
        detailsGrid.Rows.Add("Hit", "No feature at clicked point");
    }

    private void ShowHit(GeoKernelFeatureHitTestResult hit)
    {
        detailsGrid.Rows.Clear();
        detailsGrid.Rows.Add("Layer", hit.LayerName);
        detailsGrid.Rows.Add("Layer index", hit.LayerIndex);
        detailsGrid.Rows.Add("Shape id", hit.ShapeId);
        detailsGrid.Rows.Add("Feature id", hit.FeatureId);
        detailsGrid.Rows.Add("Shape type", hit.ShapeType);
        detailsGrid.Rows.Add("Extent", ExtentText(hit.Extent));

        foreach (var pair in hit.Attributes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            detailsGrid.Rows.Add(pair.Key, pair.Value?.ToString() ?? "<null>");
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
        statusLabel.Text = text;
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

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, value =>
    {
        statusLabel.Text = value.Message;
        downloadProgressBar.Visible = true;
        downloadProgressBar.Style = value.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (value.Percentage.HasValue)
            downloadProgressBar.Value = Math.Clamp(value.Percentage.Value, 0, 100);
    });

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
