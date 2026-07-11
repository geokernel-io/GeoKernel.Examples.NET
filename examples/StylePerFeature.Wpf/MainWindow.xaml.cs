using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.StylePerFeature.Wpf;

public partial class MainWindow : Window
{
    private const string LayerName = "Parcels - style from zone attribute";
    private const string ZoneFieldName = "zone";

    private static readonly string[] Zones =
    [
        "Residential",
        "Commercial",
        "Industrial",
        "Park",
        "Mixed"
    ];

    private static readonly ParcelDefinition[] Parcels =
    [
        new("Parcel A", "Residential", 0.0, 3.0, 3.0, 5.7),
        new("Parcel B", "Commercial", 3.4, 3.3, 6.8, 5.4),
        new("Parcel C", "Industrial", 7.1, 3.1, 10.4, 5.7),
        new("Parcel D", "Park", 1.0, 0.5, 4.8, 2.7),
        new("Parcel E", "Mixed", 5.2, 0.7, 9.8, 2.8)
    ];

    private readonly Dictionary<int, ParcelState> _parcelStates = [];
    private int _layerIndex = -1;
    private bool _loadingSelection;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        zoneComboBox.ItemsSource = Zones;

        CreateParcelLayer();
        ApplyZoneRenderer();
        RefreshFeatureList(selectedShapeId: 1);
        viewerControl.ViewExtent = new GeoKernelExtent(-0.8, -0.2, 11.2, 6.4);
        statusText.Text = "Per-feature style is driven by each shape's zone attribute.";
    }

    private void CreateParcelLayer()
    {
        _layerIndex = viewerControl.AddPolygonLayer(
            LayerName,
            Parcels.Select(parcel => ParcelRing(parcel)).ToArray(),
            ParcelStyle("#E5E7EB", "#6B7280"));

        if (_layerIndex < 0)
        {
            System.Windows.MessageBox.Show(
                this,
                "Parcel layer could not be created.",
                "StylePerFeature",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        _layerIndex = viewerControl.GetLayerInfoByName(LayerName)?.Index ?? _layerIndex;

        for (var i = 0; i < Parcels.Length; ++i)
        {
            var shapeId = i + 1;
            var parcel = Parcels[i];
            _parcelStates[shapeId] = new ParcelState(parcel.Name, parcel.Zone);
        }

        if (!viewerControl.BeginEditLayer(_layerIndex))
            return;

        try
        {
            foreach (var pair in _parcelStates)
            {
                viewerControl.SetShapeAttributesInEditLayer(
                    _layerIndex,
                    pair.Key,
                    new Dictionary<string, object?>
                    {
                        ["name"] = pair.Value.Name,
                        [ZoneFieldName] = pair.Value.Zone
                    });
            }

            viewerControl.CommitEditLayer(_layerIndex);
        }
        catch (Exception)
        {
            viewerControl.RollbackEditLayer(_layerIndex);
            throw;
        }
    }

    private void ApplyZoneRenderer()
    {
        if (_layerIndex < 0)
            return;

        var rules = Zones.Select(zone => new GeoKernelSymbolRule
        {
            FieldName = ZoneFieldName,
            Operator = GeoKernelSymbolRuleOperator.Equals,
            Value = zone,
            Label = zone,
            Style = StyleForZone(zone)
        });

        if (!viewerControl.SetLayerRuleBasedRenderer(
                _layerIndex,
                rules,
                ParcelStyle("#E5E7EB", "#6B7280")))
        {
            System.Windows.MessageBox.Show(
                this,
                $"Could not apply rule based renderer from {ZoneFieldName} attribute.",
                "StylePerFeature",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        viewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private void RefreshFeatureList(int selectedShapeId)
    {
        _loadingSelection = true;
        try
        {
            var items = _parcelStates
                .OrderBy(pair => pair.Key)
                .Select(pair => new FeatureListItem(
                    pair.Key,
                    $"{pair.Value.Name} - {pair.Value.Zone}",
                    BrushFromColor(ParseColor(
                        StyleForZone(pair.Value.Zone).FillColor,
                        Color.FromArgb(170, 229, 231, 235))),
                    BrushFromColor(ParseColor(
                        StyleForZone(pair.Value.Zone).LineColor,
                        Color.FromArgb(235, 107, 114, 128)))))
                .ToArray();

            featureListBox.ItemsSource = items;
            featureListBox.SelectedItem = items.FirstOrDefault(item => item.ShapeId == selectedShapeId) ?? items.FirstOrDefault();
        }
        finally
        {
            _loadingSelection = false;
        }

        SyncComboToSelectedFeature();
    }

    private void FeatureListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_loadingSelection)
            SyncComboToSelectedFeature();
    }

    private void ApplyButton_Click(object sender, RoutedEventArgs e)
    {
        var shapeId = SelectedShapeId();
        if (shapeId < 0 || _layerIndex < 0 || !_parcelStates.TryGetValue(shapeId, out var state))
            return;

        var zone = zoneComboBox.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(zone))
            return;

        _parcelStates[shapeId] = state with { Zone = zone };

        if (!viewerControl.BeginEditLayer(_layerIndex))
            return;

        try
        {
            viewerControl.SetShapeAttributesInEditLayer(
                _layerIndex,
                shapeId,
                new Dictionary<string, object?>
                {
                    ["name"] = state.Name,
                    [ZoneFieldName] = zone
                });
            viewerControl.CommitEditLayer(_layerIndex);
        }
        catch (Exception)
        {
            viewerControl.RollbackEditLayer(_layerIndex);
            throw;
        }

        ApplyZoneRenderer();
        RefreshFeatureList(shapeId);
        statusText.Text = $"{state.Name} style updated from zone={zone}.";
    }

    private void SyncComboToSelectedFeature()
    {
        var shapeId = SelectedShapeId();
        if (shapeId < 0 || !_parcelStates.TryGetValue(shapeId, out var state))
        {
            zoneComboBox.SelectedIndex = -1;
            return;
        }

        zoneComboBox.SelectedItem = state.Zone;
    }

    private int SelectedShapeId()
    {
        return featureListBox.SelectedItem is FeatureListItem item ? item.ShapeId : -1;
    }

    private static GeoKernelLayerStyle StyleForZone(string zone)
    {
        return zone switch
        {
            "Residential" => ParcelStyle("#F5DFA1", "#A16207"),
            "Commercial" => ParcelStyle("#9DD7F5", "#0369A1"),
            "Industrial" => ParcelStyle("#C4B5FD", "#6D28D9"),
            "Park" => ParcelStyle("#9AD9A8", "#15803D"),
            "Mixed" => ParcelStyle("#FDBA9A", "#C2410C"),
            _ => ParcelStyle("#E5E7EB", "#6B7280")
        };
    }

    private static GeoKernelLayerStyle ParcelStyle(string fillColor, string lineColor)
    {
        return new GeoKernelLayerStyle
        {
            FillColor = WithAlpha(fillColor, 170),
            FillOpacity = 170,
            LineColor = WithAlpha(lineColor, 235),
            LineWidth = 2.0
        };
    }

    private static GeoKernelPoint[] ParcelRing(ParcelDefinition parcel)
    {
        return
        [
            new GeoKernelPoint(parcel.XMin, parcel.YMin),
            new GeoKernelPoint(parcel.XMax, parcel.YMin),
            new GeoKernelPoint(parcel.XMax, parcel.YMax),
            new GeoKernelPoint(parcel.XMin, parcel.YMax),
            new GeoKernelPoint(parcel.XMin, parcel.YMin)
        ];
    }

    private static string WithAlpha(string rgb, byte alpha)
    {
        var hex = rgb.Trim().TrimStart('#');
        if (hex.Length != 6)
            return rgb;

        return $"#{alpha:X2}{hex.ToUpperInvariant()}";
    }

    private static Color ParseColor(string? value, Color fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        var text = value.Trim();
        if (text.StartsWith('#') && text.Length == 9 &&
            uint.TryParse(text[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var argb))
        {
            return Color.FromArgb(
                (int)((argb >> 24) & 0xFF),
                (int)((argb >> 16) & 0xFF),
                (int)((argb >> 8) & 0xFF),
                (int)(argb & 0xFF));
        }

        try
        {
            return ColorTranslator.FromHtml(text);
        }
        catch (Exception)
        {
            return fallback;
        }
    }

    private static System.Windows.Media.SolidColorBrush BrushFromColor(Color color)
    {
        return new System.Windows.Media.SolidColorBrush(
            System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B));
    }

    private sealed record ParcelState(string Name, string Zone);

    private sealed record ParcelDefinition(
        string Name,
        string Zone,
        double XMin,
        double YMin,
        double XMax,
        double YMax);

    private sealed record FeatureListItem(
        int ShapeId,
        string DisplayText,
        System.Windows.Media.Brush FillBrush,
        System.Windows.Media.Brush LineBrush);
}
