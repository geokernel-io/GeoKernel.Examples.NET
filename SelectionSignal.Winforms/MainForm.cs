using GeoKernel.NET.WinForms;

namespace GeoKernel.SelectionSignal.Winforms;

public sealed partial class MainForm : Form
{
    private readonly Dictionary<string, string> _samplePaths = new(StringComparer.OrdinalIgnoreCase);
    private SelectionMode _selectionMode = SelectionMode.Add;
    private int _eventNumber;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        if (!await PrepareSampleDataAsync())
            return;


        ConfigureDetailsGrid();

        geoKernelViewerControl.SelectionChanged += geoKernelViewerControl_SelectionChanged;        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!LoadSampleLayers())
            return;

        SetSampleExtent();
        AppendEvent("ready", geoKernelViewerControl.SelectedFeatureCount, "Click = add, Ctrl-like toggle button = toggle.");
        UpdateSelectionState("Click a feature to trigger SelectionChanged.");
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
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "SelectionSignal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "SelectionSignal", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        UpdateSelectionState("Pan mode.");
    }

    private void clearButton_Click(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ClearSelectedFeatures();
        UpdateSelectionState("Selection cleared.");
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

        UpdateSelectionState(ok
            ? (_selectionMode == SelectionMode.Toggle ? "toggleSelectedFeature applied." : "addSelectedFeature applied.")
            : "No feature hit.");
    }

    private void geoKernelViewerControl_SelectionChanged(object? sender, GeoKernelSelectionChangedEventArgs e)
    {
        AppendEvent("SelectionChanged", e.SelectedFeatureCount, "Native viewer selectionChanged signal fired.");
        UpdateSelectionState($"SelectionChanged({e.SelectedFeatureCount})");
    }

    private void SetSelectionMode(SelectionMode mode)
    {
        _selectionMode = mode;
        addButton.Checked = mode == SelectionMode.Add;
        toggleButton.Checked = mode == SelectionMode.Toggle;
        panButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;
        UpdateSelectionState(mode == SelectionMode.Toggle
            ? "Click a feature to toggle it in selection."
            : "Click a feature to add it to selection.");
    }

    private void ConfigureDetailsGrid()
    {
        detailsGrid.Columns.Clear();
        detailsGrid.Columns.Add("Number", "#");
        detailsGrid.Columns.Add("Event", "Event");
        detailsGrid.Columns.Add("SelectedCount", "Selected");
        detailsGrid.Columns.Add("Message", "Message");
        detailsGrid.Columns[0].Width = 44;
        detailsGrid.Columns[2].Width = 72;
    }

    private void AppendEvent(string eventName, int selectedCount, string message)
    {
        detailsGrid.Rows.Add(++_eventNumber, eventName, selectedCount, message);
        if (detailsGrid.Rows.Count > 0)
            detailsGrid.FirstDisplayedScrollingRowIndex = detailsGrid.Rows.Count - 1;
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateSelectionState(string text)
    {
        toolStateLabel.Text = $"Selected: {geoKernelViewerControl.SelectedFeatureCount} | Signal: SelectionChanged";
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
