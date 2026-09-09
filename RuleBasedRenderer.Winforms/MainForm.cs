using System.Globalization;
using System.Text.Json;
using GeoKernel.NET.WinForms;

namespace GeoKernel.RuleBasedRenderer.Winforms;

public sealed partial class MainForm : Form
{
    private const string PopClassFieldName = "POP_CLASS";

    private static readonly RuleDefinition[] RuleDefinitions =
    [
        new("Less than 50,000", "Less than 50,000", "#A8ADB7", "#626975", 4.0),
        new("50,000 to 100,000", "50,000 to 100,000", "#5DADE2", "#21618C", 5.5),
        new("100,000 to 250,000", "100,000 to 250,000", "#58D68D", "#1E8449", 7.0),
        new("250,000 to 500,000", "250,000 to 500,000", "#F5B041", "#935116", 9.0),
        new("500,000 to 1,000,000", "500,000 to 1,000,000", "#EC7063", "#943126", 11.5),
        new("1,000,000 to 5,000,000", "1,000,000 to 5,000,000", "#8E2C1B", "#4A160E", 15.0)
    ];

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.AddOpenStreetMapLayer();

        var citiesPath = await SampleData.EnsureFileAsync("usa_cities.zip", "usa_cities", "usa_cities.shp", "USA cities", this, CreateSampleProgress());
        if (!File.Exists(citiesPath))
        {
            MessageBox.Show(
                this,
                $"Cities shapefile could not be found:{Environment.NewLine}{citiesPath}",
                "RuleBasedRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (!geoKernelViewerControl.AddLayerFile(citiesPath, new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = DefaultCityStyle()
            }))
        {
            MessageBox.Show(
                this,
                $"Cities layer could not be loaded:{Environment.NewLine}{citiesPath}",
                "RuleBasedRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var citiesLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (citiesLayer is null)
        {
            MessageBox.Show(
                this,
                "Loaded cities layer could not be inspected.",
                "RuleBasedRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        geoKernelViewerControl.SetLayerName(citiesLayer.Index, "Cities - rule based by POP_CLASS");
        ApplyRenderer(citiesLayer.Index);
        geoKernelViewerControl.ZoomToLayer(citiesLayer.Index);
        statusLabel.Text = "Rule based renderer applied: POP_CLASS";
        downloadProgressBar.Visible = false;
    }

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, p =>
    { statusLabel.Text = p.Message; downloadProgressBar.Visible = true; downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee; if (p.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(p.Percentage.Value, 0, 100); });

    private void ApplyRenderer(int layerIndex)
    {
        var rules = RuleDefinitions.Select(rule => new GeoKernelSymbolRule
        {
            FieldName = PopClassFieldName,
            Operator = GeoKernelSymbolRuleOperator.Equals,
            Value = rule.Value,
            Label = rule.Label,
            Style = CityStyle(rule.FillColor, rule.OutlineColor, rule.PointSize)
        });

        if (!geoKernelViewerControl.SetLayerRuleBasedRenderer(layerIndex, rules, DefaultCityStyle()))
        {
            MessageBox.Show(
                this,
                $"Could not apply rule based renderer from {PopClassFieldName} field.",
                "RuleBasedRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        UpdateLegend(layerIndex);
        geoKernelViewerControl.RefreshLayers();
    }

    private void UpdateLegend(int layerIndex)
    {
        legendListView.BeginUpdate();
        legendListView.Items.Clear();
        legendImageList.Images.Clear();

        var rendererJson = geoKernelViewerControl.GetLayerSymbolRendererJson(layerIndex);
        using var document = JsonDocument.Parse(rendererJson);

        if (!document.RootElement.TryGetProperty("rules", out var rules) ||
            rules.ValueKind != JsonValueKind.Array)
        {
            legendListView.EndUpdate();
            return;
        }

        var imageIndex = 0;
        foreach (var rule in rules.EnumerateArray())
        {
            if (rule.TryGetProperty("enabled", out var enabled) && !enabled.GetBoolean())
                continue;

            var label = ReadString(rule, "label");
            if (string.IsNullOrWhiteSpace(label))
                label = ReadString(rule, "value");

            var style = rule.TryGetProperty("style", out var styleElement)
                ? LegendStyle.FromJson(styleElement)
                : LegendStyle.Default;

            legendImageList.Images.Add(CreateLegendBitmap(style));
            legendListView.Items.Add(new ListViewItem(label, imageIndex));
            imageIndex++;
        }

        UpdateLegendColumnWidth();
        legendListView.EndUpdate();
    }

    private static GeoKernelLayerStyle DefaultCityStyle()
    {
        return CityStyle("#CBD5E1", "#64748B", 4.0);
    }

    private static GeoKernelLayerStyle CityStyle(string fillColor, string outlineColor, double pointSize)
    {
        return new GeoKernelLayerStyle
        {
            PointColor = WithAlpha(fillColor, 165),
            LineColor = WithAlpha(outlineColor, 220),
            PointSize = pointSize,
            LineWidth = 1.0
        };
    }

    private static Bitmap CreateLegendBitmap(LegendStyle style)
    {
        var bitmap = new Bitmap(42, 24);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        var radius = Math.Clamp(style.PointSize / 2.0, 3.0, 10.0);
        var bounds = new RectangleF(
            (float)(21.0 - radius),
            (float)(12.0 - radius),
            (float)(radius * 2.0),
            (float)(radius * 2.0));

        using var brush = new SolidBrush(style.PointColor);
        using var pen = new Pen(style.LineColor, 1.2f);
        graphics.FillEllipse(brush, bounds);
        graphics.DrawEllipse(pen, bounds);
        return bitmap;
    }

    private void legendListView_Resize(object sender, EventArgs e)
    {
        UpdateLegendColumnWidth();
    }

    private void UpdateLegendColumnWidth()
    {
        if (legendListView.Columns.Count == 0)
            return;

        legendListView.Columns[0].Width = Math.Max(60, legendListView.ClientSize.Width - 4);
    }

    private static string WithAlpha(string rgb, byte alpha)
    {
        var hex = rgb.Trim().TrimStart('#');
        if (hex.Length != 6)
            return rgb;

        return $"#{alpha:X2}{hex.ToUpperInvariant()}";
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var property))
            return string.Empty;

        return property.ValueKind switch
        {
            JsonValueKind.String => property.GetString() ?? string.Empty,
            JsonValueKind.Number => property.GetRawText(),
            _ => string.Empty
        };
    }

    private static double ReadDouble(JsonElement element, string propertyName, double fallback)
    {
        return element.TryGetProperty(propertyName, out var property) && property.TryGetDouble(out var value)
            ? value
            : fallback;
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

    private sealed record RuleDefinition(string Label, string Value, string FillColor, string OutlineColor, double PointSize);

    private readonly record struct LegendStyle(Color PointColor, Color LineColor, double PointSize)
    {
        public static LegendStyle Default { get; } = new(
            Color.FromArgb(165, 203, 213, 225),
            Color.FromArgb(220, 100, 116, 139),
            4.0);

        public static LegendStyle FromJson(JsonElement style)
        {
            return new LegendStyle(
                ReadColor(style, "pointColor", Default.PointColor),
                ReadColor(style, "lineColor", Default.LineColor),
                ReadDouble(style, "pointSize", Default.PointSize));
        }

        private static Color ReadColor(JsonElement element, string propertyName, Color fallback)
        {
            var value = ReadString(element, propertyName);
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            return TryParseColor(value, out var color) ? color : fallback;
        }

        private static bool TryParseColor(string value, out Color color)
        {
            color = Color.Empty;
            var text = value.Trim();

            if (text.StartsWith('#') && text.Length == 9 &&
                uint.TryParse(text[1..], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var argb))
            {
                color = Color.FromArgb(
                    (int)((argb >> 24) & 0xFF),
                    (int)((argb >> 16) & 0xFF),
                    (int)((argb >> 8) & 0xFF),
                    (int)(argb & 0xFF));
                return true;
            }

            try
            {
                color = ColorTranslator.FromHtml(text);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
