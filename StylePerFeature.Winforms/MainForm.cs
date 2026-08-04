using System.Globalization;
using GeoKernel.NET.WinForms;

namespace GeoKernel.StylePerFeature.Winforms;

public sealed partial class MainForm : Form
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

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        zoneComboBox.Items.AddRange(Zones.Cast<object>().ToArray());

        CreateParcelLayer();
        ApplyZoneRenderer();
        RefreshFeatureList(selectedShapeId: 1);
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-0.8, -0.2, 11.2, 6.4);
        statusLabel.Text = "Per-feature style is driven by each shape's zone attribute.";
    }

    private void CreateParcelLayer()
    {
        _layerIndex = geoKernelViewerControl.AddPolygonLayer(
            LayerName,
            Parcels.Select(parcel => ParcelRing(parcel)).ToArray(),
            ParcelStyle("#E5E7EB", "#6B7280"));

        if (_layerIndex < 0)
        {
            MessageBox.Show(
                this,
                "Parcel layer could not be created.",
                "StylePerFeature",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        _layerIndex = geoKernelViewerControl.GetLayerInfoByName(LayerName)?.Index ?? _layerIndex;

        for (var i = 0; i < Parcels.Length; ++i)
        {
            var shapeId = i + 1;
            var parcel = Parcels[i];
            _parcelStates[shapeId] = new ParcelState(parcel.Name, parcel.Zone);
        }

        if (!geoKernelViewerControl.BeginEditLayer(_layerIndex))
            return;

        try
        {
            foreach (var pair in _parcelStates)
            {
                geoKernelViewerControl.SetShapeAttributesInEditLayer(
                    _layerIndex,
                    pair.Key,
                    new Dictionary<string, object?>
                    {
                        ["name"] = pair.Value.Name,
                        [ZoneFieldName] = pair.Value.Zone
                    });
            }

            geoKernelViewerControl.CommitEditLayer(_layerIndex);
        }
        catch (Exception)
        {
            geoKernelViewerControl.RollbackEditLayer(_layerIndex);
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

        if (!geoKernelViewerControl.SetLayerRuleBasedRenderer(
                _layerIndex,
                rules,
                ParcelStyle("#E5E7EB", "#6B7280")))
        {
            MessageBox.Show(
                this,
                $"Could not apply rule based renderer from {ZoneFieldName} attribute.",
                "StylePerFeature",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private void RefreshFeatureList(int selectedShapeId)
    {
        _loadingSelection = true;
        try
        {
            featureListView.BeginUpdate();
            featureListView.Items.Clear();
            featureImageList.Images.Clear();

            var imageIndex = 0;
            var rowToSelect = -1;
            foreach (var pair in _parcelStates.OrderBy(pair => pair.Key))
            {
                var state = pair.Value;
                featureImageList.Images.Add(CreateLegendBitmap(StyleForZone(state.Zone)));

                var item = new ListViewItem($"{state.Name} - {state.Zone}", imageIndex)
                {
                    Tag = pair.Key
                };
                featureListView.Items.Add(item);

                if (pair.Key == selectedShapeId)
                    rowToSelect = featureListView.Items.Count - 1;

                imageIndex++;
            }

            UpdateFeatureColumnWidth();

            if (rowToSelect >= 0)
                featureListView.Items[rowToSelect].Selected = true;
            else if (featureListView.Items.Count > 0)
                featureListView.Items[0].Selected = true;
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
        if (!_loadingSelection)
            SyncComboToSelectedFeature();
    }

    private void applyButton_Click(object sender, EventArgs e)
    {
        var shapeId = SelectedShapeId();
        if (shapeId < 0 || _layerIndex < 0 || !_parcelStates.TryGetValue(shapeId, out var state))
            return;

        var zone = zoneComboBox.SelectedItem as string;
        if (string.IsNullOrWhiteSpace(zone))
            return;

        _parcelStates[shapeId] = state with { Zone = zone };

        if (!geoKernelViewerControl.BeginEditLayer(_layerIndex))
            return;

        try
        {
            geoKernelViewerControl.SetShapeAttributesInEditLayer(
                _layerIndex,
                shapeId,
                new Dictionary<string, object?>
                {
                    ["name"] = state.Name,
                    [ZoneFieldName] = zone
                });
            geoKernelViewerControl.CommitEditLayer(_layerIndex);
        }
        catch (Exception)
        {
            geoKernelViewerControl.RollbackEditLayer(_layerIndex);
            throw;
        }

        ApplyZoneRenderer();
        RefreshFeatureList(shapeId);
        statusLabel.Text = $"{state.Name} style updated from zone={zone}.";
    }

    private void SyncComboToSelectedFeature()
    {
        var shapeId = SelectedShapeId();
        if (shapeId < 0 || !_parcelStates.TryGetValue(shapeId, out var state))
        {
            zoneComboBox.SelectedIndex = -1;
            return;
        }

        var index = zoneComboBox.FindStringExact(state.Zone);
        if (index >= 0)
            zoneComboBox.SelectedIndex = index;
    }

    private int SelectedShapeId()
    {
        return featureListView.SelectedItems.Count == 1 && featureListView.SelectedItems[0].Tag is int shapeId
            ? shapeId
            : -1;
    }

    private void featureListView_Resize(object sender, EventArgs e)
    {
        UpdateFeatureColumnWidth();
    }

    private void UpdateFeatureColumnWidth()
    {
        if (featureListView.Columns.Count == 0)
            return;

        featureListView.Columns[0].Width = Math.Max(80, featureListView.ClientSize.Width - 4);
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

    private static Bitmap CreateLegendBitmap(GeoKernelLayerStyle style)
    {
        var bitmap = new Bitmap(46, 22);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var brush = new SolidBrush(ParseColor(style.FillColor, Color.FromArgb(170, 229, 231, 235)));
        using var pen = new Pen(ParseColor(style.LineColor, Color.FromArgb(235, 107, 114, 128)), 2.0f);
        graphics.FillRectangle(brush, 7, 4, 32, 14);
        graphics.DrawRectangle(pen, 7, 4, 32, 14);
        return bitmap;
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

    private sealed record ParcelState(string Name, string Zone);

    private sealed record ParcelDefinition(
        string Name,
        string Zone,
        double XMin,
        double YMin,
        double XMax,
        double YMax);
}
