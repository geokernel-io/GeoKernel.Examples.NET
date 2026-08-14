using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.StylePerFeature.Wpf;

public partial class MainWindow : Window
{
    private const string LayerName = "California counties - style from zone attribute";
    private const string ZoneFieldName = "zone";
    private static readonly string[] Zones = ["Residential", "Commercial", "Industrial", "Park", "Mixed"];
    private readonly Dictionary<int, FeatureState> _featureStates = [];
    private int _layerIndex = -1;
    private bool _loadingSelection;

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        zoneComboBox.ItemsSource = Zones;
        featureListBox.ItemsSource = new[] { new FeatureListItem(-1, "Preparing California sample data...", null, null) };

        var path = SampleData.EnsureKnownWpfSampleFile("california/california.shp", this);
        if (string.IsNullOrWhiteSpace(path))
            return;

        viewerControl.AddOpenStreetMapLayer();
        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions
            {
                BuildFeatureSource = true,
                ApplyDefaultStyle = true,
                DefaultStyle = CountyStyle("#E5E7EB", "#6B7280")
            }))
        {
            System.Windows.MessageBox.Show(this, "california.shp could not be loaded.", "StylePerFeature",
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        _layerIndex = viewerControl.GetLayerInfo(0)?.Index ?? -1;
        if (_layerIndex < 0)
            return;
        viewerControl.SetLayerName(_layerIndex, LayerName);
        viewerControl.AddLayerAttributeDefinition(_layerIndex, new GeoKernelAttributeDefinition
            { Name = "name", Type = GeoKernelAttributeType.String, Length = 120 });
        viewerControl.AddLayerAttributeDefinition(_layerIndex, new GeoKernelAttributeDefinition
            { Name = ZoneFieldName, Type = GeoKernelAttributeType.String, Length = 32 });
        SeedFeatureAttributes();
        ApplyZoneRenderer();
        RefreshFeatureList(1);
        viewerControl.ZoomToLayer(_layerIndex);
        statusText.Text = "Per-feature style is driven by each shape's zone attribute.";
    }

    private void SeedFeatureAttributes()
    {
        var featureCount = viewerControl.GetLayerFeatureCount(_layerIndex);
        for (var row = 0; row < featureCount; row++)
        {
            var shapeId = row + 1;
            var source = viewerControl.GetLayerFeatureAttributes(_layerIndex, row);
            var name = AttributeText(source, "NAME", AttributeText(source, "name", $"Feature {shapeId}"));
            _featureStates[shapeId] = new FeatureState(name, Zones[row % Zones.Length]);
        }
        if (!viewerControl.BeginEditLayer(_layerIndex)) return;
        foreach (var pair in _featureStates)
            viewerControl.SetFeatureAttributesInEditLayer(_layerIndex, 0, pair.Key,
                new Dictionary<string, object?> { ["name"] = pair.Value.Name, [ZoneFieldName] = pair.Value.Zone });
    }

    private void ApplyZoneRenderer()
    {
        var rules = Zones.Select(zone => new GeoKernelSymbolRule
        {
            FieldName = ZoneFieldName, Operator = GeoKernelSymbolRuleOperator.Equals, Value = zone,
            Label = zone, Style = StyleForZone(zone)
        });
        if (!viewerControl.SetLayerRuleBasedRenderer(_layerIndex, rules, CountyStyle("#E5E7EB", "#6B7280")))
            System.Windows.MessageBox.Show(this, $"Could not apply rule based renderer from {ZoneFieldName} attribute.",
                "StylePerFeature", MessageBoxButton.OK, MessageBoxImage.Error);
        viewerControl.InvalidateRenderCache(false, true);
        viewerControl.RefreshLayers();
    }

    private void RefreshFeatureList(int selectedShapeId)
    {
        _loadingSelection = true;
        var items = _featureStates.OrderBy(pair => pair.Key).Select(pair => new FeatureListItem(pair.Key,
            $"{pair.Value.Name} - {pair.Value.Zone}", Brush(StyleForZone(pair.Value.Zone).FillColor),
            Brush(StyleForZone(pair.Value.Zone).LineColor))).ToArray();
        featureListBox.ItemsSource = items;
        featureListBox.SelectedItem = items.FirstOrDefault(item => item.ShapeId == selectedShapeId) ?? items.FirstOrDefault();
        _loadingSelection = false;
        SyncComboToSelectedFeature();
    }

    private void FeatureListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    { if (!_loadingSelection) SyncComboToSelectedFeature(); }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        var shapeId = SelectedShapeId();
        if (!_featureStates.TryGetValue(shapeId, out var state) || zoneComboBox.SelectedItem is not string zone) return;
        _featureStates[shapeId] = state with { Zone = zone };
        viewerControl.SetFeatureAttributesInEditLayer(_layerIndex, 0, shapeId,
            new Dictionary<string, object?> { ["name"] = state.Name, [ZoneFieldName] = zone });
        ApplyZoneRenderer(); RefreshFeatureList(shapeId);
        statusText.Text = $"{state.Name} style updated from zone={zone}.";
    }

    private void SyncComboToSelectedFeature()
    {
        if (_featureStates.TryGetValue(SelectedShapeId(), out var state)) zoneComboBox.SelectedItem = state.Zone;
        else zoneComboBox.SelectedIndex = -1;
    }
    private int SelectedShapeId() => featureListBox.SelectedItem is FeatureListItem item ? item.ShapeId : -1;
    private static string AttributeText(IReadOnlyDictionary<string, object?> values, string key, string fallback) =>
        values.TryGetValue(key, out var value) && value is not null ? Convert.ToString(value) ?? fallback : fallback;
    private static GeoKernelLayerStyle StyleForZone(string zone) => zone switch
    {
        "Residential" => CountyStyle("#F5DFA1", "#A16207"), "Commercial" => CountyStyle("#9DD7F5", "#0369A1"),
        "Industrial" => CountyStyle("#C4B5FD", "#6D28D9"), "Park" => CountyStyle("#9AD9A8", "#15803D"),
        "Mixed" => CountyStyle("#FDBA9A", "#C2410C"), _ => CountyStyle("#E5E7EB", "#6B7280")
    };
    private static GeoKernelLayerStyle CountyStyle(string fill, string line) => new()
        { FillColor = WithAlpha(fill, 170), FillOpacity = 170, LineColor = WithAlpha(line, 235), LineWidth = 1.2 };
    private static string WithAlpha(string rgb, byte alpha) => $"#{alpha:X2}{rgb.Trim().TrimStart('#').ToUpperInvariant()}";
    private static System.Windows.Media.Brush? Brush(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length != 9 || !uint.TryParse(value[1..], NumberStyles.HexNumber,
            CultureInfo.InvariantCulture, out var argb)) return null;
        return new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb((byte)(argb >> 24),
            (byte)(argb >> 16), (byte)(argb >> 8), (byte)argb));
    }
    private sealed record FeatureState(string Name, string Zone);
    private sealed record FeatureListItem(int ShapeId, string DisplayText, System.Windows.Media.Brush? FillBrush,
        System.Windows.Media.Brush? LineBrush);
}
