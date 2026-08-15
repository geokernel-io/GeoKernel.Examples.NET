using GeoKernel.NET.WinForms;

namespace GeoKernel.XyzPresets.Winforms;

public sealed partial class MainForm : Form
{
    private sealed record XyzPreset(string Name, string UrlTemplate, int MinZoom = 0, int MaxZoom = 19, int TileSize = 256, string Attribution = "");

    private static readonly XyzPreset[] Presets =
    {
        new("Bing Virtual Earth", "http://ecn.t3.tiles.virtualearth.net/tiles/a{q}.jpeg?g=1"),
        new("CartoDb Dark Matter", "http://basemaps.cartocdn.com/dark_all/{z}/{x}/{y}.png"),
        new("CartoDb Dark Matter (No Labels)", "http://basemaps.cartocdn.com/dark_nolabels/{z}/{x}/{y}.png"),
        new("CartoDb Positron", "http://basemaps.cartocdn.com/light_all/{z}/{x}/{y}.png"),
        new("CartoDb Positron (No Labels)", "http://basemaps.cartocdn.com/light_nolabels/{z}/{x}/{y}.png"),
        new("Esri Boundaries Places", "https://server.arcgisonline.com/ArcGIS/rest/services/Reference/World_Boundaries_and_Places/MapServer/tile/{z}/{y}/{x}"),
        new("Esri Gray (dark)", "http://services.arcgisonline.com/ArcGIS/rest/services/Canvas/World_Dark_Gray_Base/MapServer/tile/{z}/{y}/{x}"),
        new("Esri Gray (light)", "http://services.arcgisonline.com/ArcGIS/rest/services/Canvas/World_Light_Gray_Base/MapServer/tile/{z}/{y}/{x}"),
        new("Esri Hillshade", "http://services.arcgisonline.com/ArcGIS/rest/services/Elevation/World_Hillshade/MapServer/tile/{z}/{y}/{x}"),
        new("Esri National Geographic", "http://services.arcgisonline.com/ArcGIS/rest/services/NatGeo_World_Map/MapServer/tile/{z}/{y}/{x}"),
        new("Esri Navigation Charts", "http://services.arcgisonline.com/ArcGIS/rest/services/Specialty/World_Navigation_Charts/MapServer/tile/{z}/{y}/{x}"),
        new("Esri Ocean", "https://services.arcgisonline.com/ArcGIS/rest/services/Ocean/World_Ocean_Base/MapServer/tile/{z}/{y}/{x}"),
        new("Esri Physical Map", "https://services.arcgisonline.com/ArcGIS/rest/services/World_Physical_Map/MapServer/tile/{z}/{y}/{x}"),
        new("Esri Satellite", "https://server.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer/tile/{z}/{y}/{x}"),
        new("Esri Shaded Relief", "https://server.arcgisonline.com/ArcGIS/rest/services/World_Shaded_Relief/MapServer/tile/{z}/{y}/{x}"),
        new("Esri Standard", "https://server.arcgisonline.com/ArcGIS/rest/services/World_Street_Map/MapServer/tile/{z}/{y}/{x}"),
        new("Esri Topo World", "http://services.arcgisonline.com/ArcGIS/rest/services/World_Topo_Map/MapServer/tile/{z}/{y}/{x}"),
        new("Esri Transportation", "https://server.arcgisonline.com/ArcGIS/rest/services/Reference/World_Transportation/MapServer/tile/{z}/{y}/{x}"),
        new("Google Maps", "https://mt1.google.com/vt/lyrs=m&x={x}&y={y}&z={z}"),
        new("Google Satellite", "https://mt1.google.com/vt/lyrs=s&x={x}&y={y}&z={z}"),
        new("Google Satellite Hybrid", "https://mt1.google.com/vt/lyrs=y&x={x}&y={y}&z={z}"),
        new("Google Terrain", "https://mt1.google.com/vt/lyrs=t&x={x}&y={y}&z={z}"),
        new("Google Terrain Hybrid", "https://mt1.google.com/vt/lyrs=p&x={x}&y={y}&z={z}"),
        new("Mapzen Global Terrain", "https://s3.amazonaws.com/elevation-tiles-prod/terrarium/{z}/{x}/{y}.png"),
        new("Gempa", "https://demo.gempa.de/gaps/tiles/{z}/{y}/{x}"),
        new("OpenStreetMap", "https://tile.openstreetmap.org/{z}/{x}/{y}.png"),
        new("OpenTopoMap", "https://tile.opentopomap.org/{z}/{x}/{y}.png")
    };

    private bool initialized;

    public MainForm()
    {
        InitializeComponent();
        presetComboBox.DisplayMember = nameof(XyzPreset.Name);
        presetComboBox.DataSource = Presets;
        presetComboBox.SelectedItem = Presets.First(preset => preset.Name == "OpenStreetMap");
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        initialized = true;
        LoadSelectedPreset();
    }

    private void zoomInButton_Click(object? sender, EventArgs e) => viewerControl.ZoomIn();
    private void zoomOutButton_Click(object? sender, EventArgs e) => viewerControl.ZoomOut();
    private void zoomRectButton_Click(object? sender, EventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.ZoomBox;
    private void panButton_Click(object? sender, EventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.Pan;


    private void secondaryButton_Click(object? sender, EventArgs e) => viewerControl.FullExtent();

    private void presetComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (initialized)
            LoadSelectedPreset();
    }

    private void localCacheCheckBox_CheckedChanged(object? sender, EventArgs e)
    {
        if (initialized)
            LoadSelectedPreset();
    }

    private void LoadSelectedPreset()
    {
        viewerControl.ClearLayers();
        if (presetComboBox.SelectedItem is not XyzPreset preset)
            return;
        try
        {
            var index = viewerControl.AddXyzLayer(preset.Name, preset.UrlTemplate, preset.MinZoom, preset.MaxZoom, preset.TileSize, preset.Attribution, localCacheCheckBox.Checked);
            if (index < 0)
                throw new InvalidOperationException("XYZ preset could not be loaded.");
            viewerControl.ViewExtent = EuropeExtent3857();
            detailsTextBox.Text = PresetDetails(preset);
            statusLabel.Text = $"XYZ preset loaded: {preset.Name}";
        }
        catch (Exception ex)
        {
            detailsTextBox.Text = ex.Message;
            statusLabel.Text = "XYZ preset could not be loaded.";
            MessageBox.Show(this, $"XYZ preset could not be loaded:\n{ex.Message}", "XyzPresets", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private string PresetDetails(XyzPreset preset) => string.Join(Environment.NewLine,
        "XYZ preset layer", "", $"Preset count: {Presets.Length}", $"Selected: {preset.Name}", "",
        "URL template:", preset.UrlTemplate, "", $"Min zoom: {preset.MinZoom}", $"Max zoom: {preset.MaxZoom}",
        $"Tile size: {preset.TileSize}", $"Local cache: {(localCacheCheckBox.Checked ? "enabled" : "disabled")}",
        string.IsNullOrWhiteSpace(preset.Attribution) ? "" : $"\nAttribution:\n{preset.Attribution}", "",
        "The sample creates the layer from:", "the predefined XYZ preset catalog", "AddXyzLayer(name, urlTemplate, minZoom, maxZoom, tileSize, attribution)");

    private static GeoKernelExtent EuropeExtent3857() => new(-1400000.0, 4100000.0, 4200000.0, 7800000.0);
}
