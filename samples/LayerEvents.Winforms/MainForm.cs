using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerEvents.Winforms;

public sealed partial class MainForm : Form
{
    private bool _refreshingLayerList;

    public MainForm()
    {
        InitializeComponent();
        ConnectLayerEvents();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        AddLayer("World", "world_4326.shp", WorldStyle());
        AddLayer("States", "us_state_boundaries.shp", StatesStyle());
        AddLayer("Cities", "usa_cities_4326.kml", CitiesStyle());
        RefreshLayerList();
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-151.2, 16.4, -41.6, 55.6);
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

    private bool AddLayer(string name, string fileName, GeoKernelLayerStyle style)
    {
        if (geoKernelViewerControl.GetLayerInfoByName(name) is not null)
        {
            AppendLog($"Action skipped: {name} already exists");
            return true;
        }

        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", fileName);
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "LayerEvents",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        AppendLog($"Action: AddLayerFile({fileName})");
        if (!geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
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
            geoKernelViewerControl.SetLayerName(layer.Index, name);

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

    private void addWorldButton_Click(object sender, EventArgs e)
    {
        AddLayer("World", "world_4326.shp", WorldStyle());
    }

    private void addStatesButton_Click(object sender, EventArgs e)
    {
        AddLayer("States", "us_state_boundaries.shp", StatesStyle());
    }

    private void addCitiesButton_Click(object sender, EventArgs e)
    {
        AddLayer("Cities", "usa_cities_4326.kml", CitiesStyle());
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
