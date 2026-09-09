using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerEvents.Winforms;

public sealed partial class MainForm : Form
{
    private bool _refreshingLayerList;
    private readonly Dictionary<string, SampleLayer> _layers = new(StringComparer.OrdinalIgnoreCase)
    {
        ["World"] = new("World", "world_4326.zip", "world_4326", "world_4326.shp", WorldStyle()),
        ["States"] = new("States", "usa_states.zip", "usa_states", "usa_states.shp", StatesStyle()),
        ["Cities"] = new("Cities", "usa_cities.zip", "usa_cities", "usa_cities.shp", CitiesStyle())
    };

    public MainForm()
    {
        InitializeComponent();
        ConnectLayerEvents();
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        sidePanelLayout.Enabled = false;
        try
        {
            foreach (var key in new[] { "World", "States", "Cities" })
                if (!await AddLayerAsync(key)) return;
        }
        finally
        {
            sidePanelLayout.Enabled = true;
        }
        RefreshLayerList();
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-151.2, 16.4, -41.6, 55.6);
        progressBar.Visible = false;
    }

    private void ConnectLayerEvents()
    {
        geoKernelViewerControl.LayersChanged += (_, _) =>
        {
            var selectedIndex = layerListBox.SelectedIndex;
            RefreshLayerList(selectedIndex);
            AppendLog($"Event: LayersChanged(count={geoKernelViewerControl.LayerCount})");
        };
        geoKernelViewerControl.LayerAdded += (_, e) => AppendLog($"Event: LayerAdded({LayerText(e)})");
        geoKernelViewerControl.LayerRemoved += (_, e) => AppendLog($"Event: LayerRemoved({LayerText(e)})");
        geoKernelViewerControl.LayerVisibilityChanged += (_, e) =>
            AppendLog($"Event: LayerVisibilityChanged(index={e.LayerIndex}, visible={e.Visible})");
        geoKernelViewerControl.LayerEditStateChanged += (_, e) => AppendLog($"Event: LayerEditStateChanged({LayerText(e)})");
        geoKernelViewerControl.LayerEditSessionStarted += (_, e) => AppendLog($"Event: LayerEditSessionStarted({LayerText(e)})");
        geoKernelViewerControl.LayerEditSessionCommitted += (_, e) => AppendLog($"Event: LayerEditSessionCommitted({LayerText(e)})");
        geoKernelViewerControl.LayerEditSessionRolledBack += (_, e) => AppendLog($"Event: LayerEditSessionRolledBack({LayerText(e)})");
        geoKernelViewerControl.LayerOrderChanged += (_, _) => AppendLog("Event: LayerOrderChanged()");
        geoKernelViewerControl.BusyChanged += (_, e) => AppendLog($"Event: BusyChanged({e.Busy})");
        geoKernelViewerControl.ViewChanged += (_, _) => UpdateStatus();
    }

    private async Task<bool> AddLayerAsync(string key)
    {
        var request = _layers[key];
        if (geoKernelViewerControl.GetLayerInfoByName(request.Name) is not null)
        {
            AppendLog($"Action skipped: {request.Name} already exists");
            return true;
        }
        AppendLog($"Action: prepareSampleData({request.ArchiveName})");
        var progress = new ControlProgress<SampleDataProgress>(this, value => SetProgress(value.Message, value.Percentage));
        var path = await SampleData.EnsureSampleFileAsync(request, this, progress);
        if (string.IsNullOrEmpty(path)) return false;
        AppendLog($"Action: AddLayerFile({request.FileName})");
        if (!geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = request.Style
                }))
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{path}",
                "LayerEvents",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, request.Name);

        geoKernelViewerControl.RefreshLayers();
        return true;
    }

    private void RemoveSelectedLayer()
    {
        var index = layerListBox.SelectedIndex;
        var layer = index >= 0 ? geoKernelViewerControl.GetLayerInfo(index) : null;
        if (layer is null)
            return;

        AppendLog($"Action: RemoveLayer({layer.Name})");
        geoKernelViewerControl.RemoveLayer(index);
    }

    private void ToggleSelectedLayer()
    {
        var index = layerListBox.SelectedIndex;
        var layer = index >= 0 ? geoKernelViewerControl.GetLayerInfo(index) : null;
        if (layer is null)
            return;

        AppendLog($"Action: SetLayerVisible({layer.Name}, {!layer.Visible})");
        geoKernelViewerControl.SetLayerVisible(index, !layer.Visible);
    }

    private void MoveSelectedLayer(int delta)
    {
        var fromIndex = layerListBox.SelectedIndex;
        var toIndex = fromIndex + delta;
        if (fromIndex < 0 || toIndex < 0 || toIndex >= geoKernelViewerControl.LayerCount)
            return;

        AppendLog($"Action: MoveLayer({fromIndex} -> {toIndex})");
        if (geoKernelViewerControl.MoveLayer(fromIndex, toIndex))
            RefreshLayerList(toIndex);
    }

    private void RefreshLayerList(int selectedIndex = -1)
    {
        _refreshingLayerList = true;
        try
        {
            layerListBox.Items.Clear();
            foreach (var layer in geoKernelViewerControl.GetLayersInfo())
                layerListBox.Items.Add($"{(layer.Visible ? "[x]" : "[ ]")} {layer.DisplayText}");

            if (selectedIndex >= 0 && selectedIndex < layerListBox.Items.Count)
                layerListBox.SelectedIndex = selectedIndex;
            else if (layerListBox.Items.Count > 0)
                layerListBox.SelectedIndex = 0;
        }
        finally
        {
            _refreshingLayerList = false;
        }

        UpdateStatus();
    }

    private void AppendLog(string text)
    {
        eventLogTextBox.AppendText($"{DateTime.Now:HH:mm:ss.fff}  {text}{Environment.NewLine}");
    }

    private void UpdateStatus()
    {
        if (_refreshingLayerList)
            return;

        statusLabel.Text = $"Layers: {geoKernelViewerControl.LayerCount}";
    }

    private async void addWorldButton_Click(object sender, EventArgs e)
    {
        await AddLayerFromButtonAsync("World");
    }

    private async void addStatesButton_Click(object sender, EventArgs e)
    {
        await AddLayerFromButtonAsync("States");
    }

    private async void addCitiesButton_Click(object sender, EventArgs e)
    {
        await AddLayerFromButtonAsync("Cities");
    }

    private void removeSelectedButton_Click(object sender, EventArgs e)
    {
        RemoveSelectedLayer();
    }

    private void clearLayersButton_Click(object sender, EventArgs e)
    {
        AppendLog("Action: ClearLayers()");
        geoKernelViewerControl.ClearLayers();
    }

    private void toggleVisibilityButton_Click(object sender, EventArgs e)
    {
        ToggleSelectedLayer();
    }

    private void moveUpButton_Click(object sender, EventArgs e)
    {
        MoveSelectedLayer(-1);
    }

    private void moveDownButton_Click(object sender, EventArgs e)
    {
        MoveSelectedLayer(1);
    }

    private void refreshButton_Click(object sender, EventArgs e)
    {
        AppendLog("Action: RefreshLayers()");
        geoKernelViewerControl.RefreshLayers();
    }

    private void clearLogButton_Click(object sender, EventArgs e)
    {
        eventLogTextBox.Clear();
    }

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 220,
            LineColor = "#7B918D",
            LineWidth = 0.8
        };
    }

    private static GeoKernelLayerStyle StatesStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#A9C8DB",
            FillOpacity = 115,
            LineColor = "#356780",
            LineWidth = 1.2
        };
    }

    private static GeoKernelLayerStyle CitiesStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            PointSize = 7.0,
            LineColor = "#D95D39",
            LineWidth = 1.5
        };
    }

    private static string LayerText(GeoKernelLayerEventArgs e)
    {
        return string.IsNullOrWhiteSpace(e.LayerName)
            ? $"index={e.LayerIndex}"
            : $"{e.LayerName}, index={e.LayerIndex}";
    }

    private async Task AddLayerFromButtonAsync(string key)
    {
        sidePanelLayout.Enabled = false;
        try { await AddLayerAsync(key); }
        finally { sidePanelLayout.Enabled = true; progressBar.Visible = false; }
    }

    private void SetProgress(string message, int? percentage)
    {
        statusLabel.Text = message;
        progressBar.Visible = true;
        progressBar.Style = percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (percentage.HasValue) progressBar.Value = Math.Clamp(percentage.Value, 0, 100);
    }

    private sealed class ControlProgress<T>(Control control, Action<T> callback) : IProgress<T>
    {
        public void Report(T value)
        {
            if (control.IsDisposed) return;
            if (control.InvokeRequired) control.Invoke(() => callback(value)); else callback(value);
        }
    }
}
