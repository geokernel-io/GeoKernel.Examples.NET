using GeoKernel.Examples.Common;
using System.IO;
using System.Windows;
using System.Drawing;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.XyzPresets.Wpf;

public sealed partial class MainWindow
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

    public MainWindow()
    {
        InitializeComponent();
        presetComboBox.ItemsSource = Presets;
        presetComboBox.SelectedItem = Presets.First(preset => preset.Name == "OpenStreetMap");
    }

    private void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        initialized = true;
        LoadSelectedPreset();
    }

    private void FullExtent_Click(object? sender, RoutedEventArgs e) => viewerControl.FullExtent();

    private void PresetComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
    {
        if (initialized)
            LoadSelectedPreset();
    }

    private void LocalCacheCheckBox_Changed(object sender, RoutedEventArgs e)
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
            var index = viewerControl.AddXyzLayer(preset.Name, preset.UrlTemplate, preset.MinZoom, preset.MaxZoom, preset.TileSize, preset.Attribution, localCacheCheckBox.IsChecked == true);
            if (index < 0)
                throw new InvalidOperationException("XYZ preset could not be loaded.");
            viewerControl.ViewExtent = EuropeExtent3857();
            detailsTextBox.Text = PresetDetails(preset);
            statusText.Text = $"XYZ preset loaded: {preset.Name}";
        }
        catch (Exception ex)
        {
            detailsTextBox.Text = ex.Message;
            statusText.Text = "XYZ preset could not be loaded.";
            MessageBox.Show(this, $"XYZ preset could not be loaded:\n{ex.Message}", "XyzPresets", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string PresetDetails(XyzPreset preset) => string.Join(Environment.NewLine,
        "XYZ preset layer", "", $"Preset count: {Presets.Length}", $"Selected: {preset.Name}", "",
        "URL template:", preset.UrlTemplate, "", $"Min zoom: {preset.MinZoom}", $"Max zoom: {preset.MaxZoom}",
        $"Tile size: {preset.TileSize}", $"Local cache: {(localCacheCheckBox.IsChecked == true ? "enabled" : "disabled")}",
        string.IsNullOrWhiteSpace(preset.Attribution) ? "" : $"\nAttribution:\n{preset.Attribution}", "",
        "The sample creates the layer from:", "the predefined XYZ preset catalog", "AddXyzLayer(name, urlTemplate, minZoom, maxZoom, tileSize, attribution)");

    private static GeoKernelExtent EuropeExtent3857() => new(-1400000.0, 4100000.0, 4200000.0, 7800000.0);
}
