using GeoKernel.NET.WinForms;

namespace GeoKernel.BoxSelect.Winforms;

public sealed partial class MainForm : Form
{
    private readonly Dictionary<string, string> _samplePaths = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<GeoKernelFeatureHitTestResult> _hits = [];
    private bool _updatingHits;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        if (!await PrepareSampleDataAsync())
            return;


        ConfigureGrids();
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Select;
        geoKernelViewerControl.MapSelectionBoxFinished += geoKernelViewerControl_MapSelectionBoxFinished;

        if (!LoadSampleLayers())
            return;

        ShowEmptyHits();
        SetSampleExtent();
        UpdateStatus("Drag a selection box to run HitTestFeaturesInScreenRectangle(screenRect).");
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
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "BoxSelect", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "BoxSelect", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(geoKernelViewerControl.LayerCount - 1);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void selectButton_Click(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Select;
        SetToolMode(selectActive: true);
        UpdateStatus("Drag a box to select features.");
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        SetToolMode(selectActive: false);
        UpdateStatus("Pan mode.");
    }

    private void clearSelectionButton_Click(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ClearSelectedFeatures();
        ShowEmptyHits();
        UpdateStatus("Selection cleared.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_MapSelectionBoxFinished(object? sender, GeoKernelSelectionBoxFinishedEventArgs e)
    {
        var selectionMode = (e.Modifiers & 0x04000000) != 0
            ? GeoKernelFeatureSelectionMode.Toggle
            : (e.Modifiers & 0x02000000) != 0
                ? GeoKernelFeatureSelectionMode.Add
                : GeoKernelFeatureSelectionMode.Replace;

        geoKernelViewerControl.SelectFeaturesInScreenRectangle(e.ScreenRectangle, selectionMode);
        var hits = geoKernelViewerControl.GetSelectedFeatures()
            .Where(hit => hit.IsValid)
            .ToList();

        ShowHits(hits);
        if (hits.Count == 0)
        {
            ShowEmptyAttributes("No features intersect the selection box.");
            UpdateStatus($"No features in screen rect {RectText(e.ScreenRectangle)}.");
            return;
        }

        hitsGrid.Rows[0].Selected = true;
        ShowAttributes(hits[0]);
        UpdateStatus($"{hits.Count} feature hit(s), screen rect {RectText(e.ScreenRectangle)}.");
    }

    private void hitsGrid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_updatingHits || hitsGrid.CurrentRow is null)
            return;

        var index = hitsGrid.CurrentRow.Index;
        if (index < 0 || index >= _hits.Count)
            return;

        ShowAttributes(_hits[index]);
        UpdateStatus($"Selected row {index + 1}/{_hits.Count}: {_hits[index].LayerName} feature {_hits[index].ShapeId}.");
    }

    private void ConfigureGrids()
    {
        hitsGrid.Columns.Clear();
        hitsGrid.Columns.Add("Number", "#");
        hitsGrid.Columns.Add("Layer", "Layer");
        hitsGrid.Columns.Add("ShapeId", "Shape id");
        hitsGrid.Columns.Add("FeatureId", "Feature id");
        hitsGrid.Columns.Add("ShapeType", "Type");

        attributesGrid.Columns.Clear();
        attributesGrid.Columns.Add("Property", "Property / Field");
        attributesGrid.Columns.Add("Value", "Value");
    }

    private void ShowEmptyHits()
    {
        _hits.Clear();
        hitsGrid.Rows.Clear();
        hitsGrid.Rows.Add("-", "No hits", "-", "-", "-");
        ShowEmptyAttributes("Drag a selection box to list matching features.");
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
                hitsGrid.Rows.Add(i + 1, hit.LayerName, hit.ShapeId, hit.FeatureId, hit.ShapeType);
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

    private void SetToolMode(bool selectActive)
    {
        selectButton.BackColor = selectActive ? System.Drawing.Color.FromArgb(210, 232, 247) : System.Drawing.SystemColors.Control;
        selectButton.FlatAppearance.BorderSize = selectActive ? 1 : 0;
        panButton.BackColor = selectActive ? System.Drawing.SystemColors.Control : System.Drawing.Color.FromArgb(210, 232, 247);
        panButton.FlatAppearance.BorderSize = selectActive ? 0 : 1;
    }

    private static string RectText(GeoKernelScreenRectangle rect) => $"{rect.Left},{rect.Top} - {rect.Right},{rect.Bottom}";

    private static string ExtentText(GeoKernelExtent extent)
    {
        return $"({extent.XMin:F6}, {extent.YMin:F6}) - ({extent.XMax:F6}, {extent.YMax:F6})";
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
