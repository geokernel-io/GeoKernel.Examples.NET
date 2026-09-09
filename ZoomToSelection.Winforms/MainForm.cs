using GeoKernel.NET.WinForms;

namespace GeoKernel.ZoomToSelection.Winforms;

public sealed partial class MainForm : Form
{
    private readonly Dictionary<string, string> _samplePaths = new(StringComparer.OrdinalIgnoreCase);
    private SelectionMode _selectionMode = SelectionMode.Add;

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

        RefreshSelectedFeatures("No selected features.");
        SetSampleExtent();
        UpdateStatus("Click features to select them, then use Zoom To Selection.");
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
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "ZoomToSelection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "ZoomToSelection", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(geoKernelViewerControl.LayerCount - 1);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void addButton_Click(object? sender, EventArgs e)
    {
        SetSelectionMode(SelectionMode.Add);
    }

    private void toggleButton_Click(object? sender, EventArgs e)
    {
        SetSelectionMode(SelectionMode.Toggle);
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        addButton.Checked = false;
        toggleButton.Checked = false;
        panButton.Checked = true;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        toolStateLabel.Text = "Tool: Pan";
        UpdateStatus("Pan mode.");
    }

    private void clearButton_Click(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ClearSelectedFeatures();
        RefreshSelectedFeatures("Selection cleared.");
        UpdateStatus("Selection cleared.");
    }

    private void zoomToSelectionButton_Click(object? sender, EventArgs e)
    {
        var selected = geoKernelViewerControl.GetSelectedFeatures();
        if (selected.Count == 0)
        {
            UpdateStatus("No selected features to zoom.");
            return;
        }

        var extent = SelectedExtent(selected);
        if (geoKernelViewerControl.ZoomToSelectedFeatures())
            UpdateStatus($"zoomToSelectedFeatures ok. selectedFeaturesExtent={ExtentText(extent)}");
        else
            UpdateStatus("No selected feature extent to zoom.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_MapMouseUp(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (e.Tool != GeoKernelViewerTool.Info)
            return;

        var ok = _selectionMode == SelectionMode.Toggle
            ? geoKernelViewerControl.ToggleTopFeatureSelectionAt(e.ScreenPoint.X, e.ScreenPoint.Y, 8)
            : geoKernelViewerControl.AddTopFeatureToSelectionAt(e.ScreenPoint.X, e.ScreenPoint.Y, 8);

        if (!ok)
        {
            UpdateStatus("No feature hit.");
            return;
        }

        RefreshSelectedFeatures();
        UpdateStatus(_selectionMode == SelectionMode.Toggle
            ? $"toggleSelectedFeature applied. selectedFeatureCount={geoKernelViewerControl.SelectedFeatureCount}."
            : $"addSelectedFeature applied. selectedFeatureCount={geoKernelViewerControl.SelectedFeatureCount}.");
    }

    private void SetSelectionMode(SelectionMode mode)
    {
        _selectionMode = mode;
        addButton.Checked = mode == SelectionMode.Add;
        toggleButton.Checked = mode == SelectionMode.Toggle;
        panButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;
        toolStateLabel.Text = mode == SelectionMode.Toggle
            ? "Click = toggleSelectedFeature"
            : "Click = addSelectedFeature";
        UpdateStatus(mode == SelectionMode.Toggle
            ? "Click a feature to toggle it in selection."
            : "Click a feature to add it to selection.");
    }

    private void ConfigureDetailsGrid()
    {
        detailsGrid.Columns.Clear();
        detailsGrid.Columns.Add("Number", "#");
        detailsGrid.Columns.Add("Layer", "Layer");
        detailsGrid.Columns.Add("ShapeId", "Shape id");
        detailsGrid.Columns.Add("FeatureId", "Feature id");
        detailsGrid.Columns.Add("ShapeType", "Type");
    }

    private void RefreshSelectedFeatures(string? emptyMessage = null)
    {
        var selected = geoKernelViewerControl.GetSelectedFeatures();
        detailsGrid.Rows.Clear();

        for (var i = 0; i < selected.Count; i++)
        {
            var hit = selected[i];
            detailsGrid.Rows.Add(i + 1, hit.LayerName, hit.ShapeId, hit.FeatureId, hit.ShapeType);
        }

        if (selected.Count == 0)
            detailsGrid.Rows.Add("-", emptyMessage ?? "No selected features.", "-", "-", "-");
    }

    private static GeoKernelExtent SelectedExtent(IReadOnlyList<GeoKernelFeatureHitTestResult> selected)
    {
        var xMin = selected.Min(hit => hit.Extent.XMin);
        var yMin = selected.Min(hit => hit.Extent.YMin);
        var xMax = selected.Max(hit => hit.Extent.XMax);
        var yMax = selected.Max(hit => hit.Extent.YMax);
        return new GeoKernelExtent(xMin, yMin, xMax, yMax);
    }

    private static string ExtentText(GeoKernelExtent extent)
    {
        return $"({extent.XMin:F4}, {extent.YMin:F4}) - ({extent.XMax:F4}, {extent.YMax:F4})";
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
        statusLabel.Text = text;
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
        FillOpacity = 160,
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

    private enum SelectionMode
    {
        Add,
        Toggle
    }
}

