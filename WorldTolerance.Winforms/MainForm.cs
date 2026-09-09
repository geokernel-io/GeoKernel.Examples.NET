using GeoKernel.NET.WinForms;

namespace GeoKernel.WorldTolerance.Winforms;

public sealed partial class MainForm : Form
{
    private readonly Dictionary<string, string> _samplePaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<GeoKernelFeatureHitTestResult> _hits = [];
    private bool _updatingHits;
    private double _hitTolerance = 1.0;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        if (!await PrepareSampleDataAsync())
            return;


        ConfigureGrids();
        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!LoadSampleLayers())
            return;

        ShowEmptyHits();
        SetSampleExtent();
        UpdateStatus("Click the map to call HitTestFeatures(worldX, worldY, worldTolerance).");
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
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "WorldTolerance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "WorldTolerance", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(geoKernelViewerControl.LayerCount - 1);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void identifyButton_Click(object? sender, EventArgs e)
    {
        identifyButton.Checked = true;
        panButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;
        UpdateStatus("Click a world point to search using the selected world tolerance.");
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        panButton.Checked = true;
        identifyButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
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

        var worldPoint = geoKernelViewerControl.ScreenToWorld(e.ScreenPoint.X, e.ScreenPoint.Y);
        var tolerance = (double)toleranceUpDown.Value;
        _hitTolerance = tolerance;
        var hits = geoKernelViewerControl.HitTestFeatures(worldPoint.X, worldPoint.Y, tolerance)
            .Where(hit => hit.IsValid)
            .ToList();

        ShowHits(hits);
        if (hits.Count == 0)
        {
            geoKernelViewerControl.ClearSelectedFeatures();
            ShowEmptyAttributes("No feature inside world tolerance.");
            UpdateStatus($"No feature hit at {worldPoint.X:F6}, {worldPoint.Y:F6} with tolerance {tolerance:F2} deg.");
            return;
        }

        hitsGrid.Rows[0].Selected = true;
        geoKernelViewerControl.SelectFeatureHit(hits[0], tolerance);
        ShowAttributes(hits[0]);
        UpdateStatus($"{hits.Count} feature hit(s) at {worldPoint.X:F6}, {worldPoint.Y:F6} with tolerance {tolerance:F2} deg.");
    }

    private void hitsGrid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_updatingHits || hitsGrid.CurrentRow is null)
            return;

        var index = hitsGrid.CurrentRow.Index;
        if (index < 0 || index >= _hits.Count)
            return;

        ShowAttributes(_hits[index]);
        geoKernelViewerControl.SelectFeatureHit(_hits[index], _hitTolerance);
        UpdateStatus($"Selected hit {index + 1}/{_hits.Count}: {_hits[index].LayerName} feature {_hits[index].ShapeId}.");
    }

    private void ConfigureGrids()
    {
        hitsGrid.Columns.Clear();
        hitsGrid.Columns.Add("Number", "#");
        hitsGrid.Columns.Add("Layer", "Layer");
        hitsGrid.Columns.Add("FeatureId", "Feature id");
        hitsGrid.Columns.Add("ShapeType", "Type");
        hitsGrid.Columns.Add("Display", "Display");

        attributesGrid.Columns.Clear();
        attributesGrid.Columns.Add("Property", "Property / Field");
        attributesGrid.Columns.Add("Value", "Value");
    }

    private void ShowEmptyHits()
    {
        _hits.Clear();
        hitsGrid.Rows.Clear();
        hitsGrid.Rows.Add("-", "No hits", "-", "-", "-");
        ShowEmptyAttributes("Click the map to inspect features inside world tolerance.");
    }

    private void ShowHits(IReadOnlyList<GeoKernelFeatureHitTestResult> hits)
    {
        _updatingHits = true;
        try
        {
            _hits.Clear();
            _hits.AddRange(hits);
            hitsGrid.Rows.Clear();

            for (var i = 0; i < _hits.Count; i++)
            {
                var hit = _hits[i];
                hitsGrid.Rows.Add(i + 1, hit.LayerName, hit.FeatureId, hit.ShapeType, BestDisplayName(hit));
            }

            if (_hits.Count == 0)
                hitsGrid.Rows.Add("-", "No hits", "-", "-", "-");
        }
        finally
        {
            _updatingHits = false;
        }
    }

    private void ShowEmptyAttributes(string text)
    {
        attributesGrid.Rows.Clear();
        attributesGrid.Rows.Add("Hit", text);
    }

    private void ShowAttributes(GeoKernelFeatureHitTestResult hit)
    {
        attributesGrid.Rows.Clear();
        attributesGrid.Rows.Add("Layer", hit.LayerName);
        attributesGrid.Rows.Add("Layer index", hit.LayerIndex);
        attributesGrid.Rows.Add("Shape id", hit.ShapeId);
        attributesGrid.Rows.Add("Feature id", hit.FeatureId);
        attributesGrid.Rows.Add("Shape type", hit.ShapeType);
        attributesGrid.Rows.Add("World point", $"{hit.WorldPoint.X:F6}, {hit.WorldPoint.Y:F6}");
        attributesGrid.Rows.Add("Extent", ExtentText(hit.Extent));

        foreach (var pair in hit.Attributes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            attributesGrid.Rows.Add(pair.Key, pair.Value?.ToString() ?? "<null>");
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

    private static string BestDisplayName(GeoKernelFeatureHitTestResult hit)
    {
        foreach (var key in new[] { "NAME", "Name", "STATE", "STATE_NAME", "COUNTRY", "ADMIN" })
        {
            if (hit.Attributes.TryGetValue(key, out var value) && !string.IsNullOrWhiteSpace(value?.ToString()))
                return value!.ToString()!;
        }
        return $"Feature {hit.FeatureId}";
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
