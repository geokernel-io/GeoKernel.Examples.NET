using System.Globalization;
using GeoKernel.Examples.Common;
using GeoKernel.NET.WinForms;

namespace GeoKernel.StylePerFeature.Winforms;

public sealed partial class MainForm : Form
{
    private const string LayerName = "California counties - style from zone attribute";
    private const string ZoneFieldName = "zone";
    private static readonly string[] Zones = ["Residential", "Commercial", "Industrial", "Park", "Mixed"];
    private readonly Dictionary<int, FeatureState> _featureStates = [];
    private int _layerIndex = -1;
    private bool _loadingSelection;

    public MainForm() => InitializeComponent();

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        zoneComboBox.Items.AddRange(Zones.Cast<object>().ToArray());
        featureListView.Items.Add("Preparing California sample data...");

        var path = SampleData.EnsureSampleFile(
            new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/california.zip"),
            "california.zip", "california", "california.shp", this);
        if (string.IsNullOrWhiteSpace(path))
            return;

        geoKernelViewerControl.AddOpenStreetMapLayer();
        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions
            {
                BuildFeatureSource = true,
                ApplyDefaultStyle = true,
                DefaultStyle = CountyStyle("#E5E7EB", "#6B7280")
            }))
        {
            MessageBox.Show(this, "california.shp could not be loaded.", "StylePerFeature",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _layerIndex = geoKernelViewerControl.GetLayerInfo(0)?.Index ?? -1;
        if (_layerIndex < 0)
            return;

        geoKernelViewerControl.SetLayerName(_layerIndex, LayerName);
        geoKernelViewerControl.AddLayerAttributeDefinition(_layerIndex, new GeoKernelAttributeDefinition
            { Name = "name", Type = GeoKernelAttributeType.String, Length = 120 });
        geoKernelViewerControl.AddLayerAttributeDefinition(_layerIndex, new GeoKernelAttributeDefinition
            { Name = ZoneFieldName, Type = GeoKernelAttributeType.String, Length = 32 });
        SeedFeatureAttributes();
        ApplyZoneRenderer();
        RefreshFeatureList(1);
        geoKernelViewerControl.ZoomToLayer(_layerIndex);
        statusLabel.Text = "Per-feature style is driven by each shape's zone attribute.";
    }

    private void SeedFeatureAttributes()
    {
        var featureCount = geoKernelViewerControl.GetLayerFeatureCount(_layerIndex);
        for (var row = 0; row < featureCount; row++)
        {
            var shapeId = row + 1;
            var source = geoKernelViewerControl.GetLayerFeatureAttributes(_layerIndex, row);
            var name = AttributeText(source, "NAME", AttributeText(source, "name", $"Feature {shapeId}"));
            _featureStates[shapeId] = new FeatureState(name, Zones[row % Zones.Length]);
        }

        if (!geoKernelViewerControl.BeginEditLayer(_layerIndex))
            return;
        foreach (var pair in _featureStates)
            geoKernelViewerControl.SetFeatureAttributesInEditLayer(_layerIndex, 0, pair.Key,
                new Dictionary<string, object?> { ["name"] = pair.Value.Name, [ZoneFieldName] = pair.Value.Zone });
    }

    private void ApplyZoneRenderer()
    {
        var rules = Zones.Select(zone => new GeoKernelSymbolRule
        {
            FieldName = ZoneFieldName,
            Operator = GeoKernelSymbolRuleOperator.Equals,
            Value = zone,
            Label = zone,
            Style = StyleForZone(zone)
        });
        if (!geoKernelViewerControl.SetLayerRuleBasedRenderer(_layerIndex, rules, CountyStyle("#E5E7EB", "#6B7280")))
            MessageBox.Show(this, $"Could not apply rule based renderer from {ZoneFieldName} attribute.",
                "StylePerFeature", MessageBoxButtons.OK, MessageBoxIcon.Error);
        geoKernelViewerControl.InvalidateRenderCache(false, true);
        geoKernelViewerControl.RefreshLayers();
    }

    private void RefreshFeatureList(int selectedShapeId)
    {
        _loadingSelection = true;
        featureListView.BeginUpdate();
        try
        {
            featureListView.Items.Clear();
            featureImageList.Images.Clear();
            foreach (var pair in _featureStates.OrderBy(pair => pair.Key))
            {
                featureImageList.Images.Add(CreateLegendBitmap(StyleForZone(pair.Value.Zone)));
                featureListView.Items.Add(new ListViewItem($"{pair.Value.Name} - {pair.Value.Zone}", featureImageList.Images.Count - 1)
                    { Tag = pair.Key });
            }
            UpdateFeatureColumnWidth();
            var selected = featureListView.Items.Cast<ListViewItem>()
                .FirstOrDefault(item => item.Tag is int shapeId && shapeId == selectedShapeId);
            if (selected is not null) selected.Selected = true;
        }
        finally
        {
            featureListView.EndUpdate();
            _loadingSelection = false;
        }
        SyncComboToSelectedFeature();
    }

    private void featureListView_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (!_loadingSelection) SyncComboToSelectedFeature();
    }

    private void applyButton_Click(object sender, EventArgs e)
    {
        var shapeId = SelectedShapeId();
        if (!_featureStates.TryGetValue(shapeId, out var state) || zoneComboBox.SelectedItem is not string zone)
            return;
        _featureStates[shapeId] = state with { Zone = zone };
        geoKernelViewerControl.SetFeatureAttributesInEditLayer(_layerIndex, 0, shapeId,
            new Dictionary<string, object?> { ["name"] = state.Name, [ZoneFieldName] = zone });
        ApplyZoneRenderer();
        RefreshFeatureList(shapeId);
        statusLabel.Text = $"{state.Name} style updated from zone={zone}.";
    }

    private void SyncComboToSelectedFeature()
    {
        if (_featureStates.TryGetValue(SelectedShapeId(), out var state))
            zoneComboBox.SelectedItem = state.Zone;
        else
            zoneComboBox.SelectedIndex = -1;
    }

    private int SelectedShapeId() => featureListView.SelectedItems.Count == 1 &&
        featureListView.SelectedItems[0].Tag is int id ? id : -1;
    private void featureListView_Resize(object sender, EventArgs e) => UpdateFeatureColumnWidth();
    private void UpdateFeatureColumnWidth() => featureListView.Columns[0].Width = Math.Max(80, featureListView.ClientSize.Width - 4);
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
    private static Bitmap CreateLegendBitmap(GeoKernelLayerStyle style)
    {
        var bitmap = new Bitmap(46, 22); using var graphics = Graphics.FromImage(bitmap); graphics.Clear(Color.Transparent);
        using var brush = new SolidBrush(ParseColor(style.FillColor, Color.LightGray)); using var pen = new Pen(ParseColor(style.LineColor, Color.Gray), 1.2f);
        graphics.FillRectangle(brush, 7, 4, 32, 14); graphics.DrawRectangle(pen, 7, 4, 32, 14); return bitmap;
    }
    private static string WithAlpha(string rgb, byte alpha) => $"#{alpha:X2}{rgb.Trim().TrimStart('#').ToUpperInvariant()}";
    private static Color ParseColor(string? value, Color fallback)
    {
        var text = value?.Trim() ?? "";
        if (text.StartsWith('#') && text.Length == 9 && uint.TryParse(text[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var argb))
            return Color.FromArgb((int)(argb >> 24), (int)((argb >> 16) & 255), (int)((argb >> 8) & 255), (int)(argb & 255));
        return fallback;
    }
    private sealed record FeatureState(string Name, string Zone);
}
