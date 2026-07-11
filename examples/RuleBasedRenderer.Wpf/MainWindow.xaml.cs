using System.IO;
using System.Text.Json;
using System.Globalization;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.RuleBasedRenderer.Wpf;

public partial class MainWindow : Window
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

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        var citiesPath = Path.Combine(FindRepositoryRoot(), "assets", "data", "cities_4326.shp");
        if (!File.Exists(citiesPath))
        {
            System.Windows.MessageBox.Show(
                this,
                $"Cities shapefile could not be found:{Environment.NewLine}{citiesPath}",
                "RuleBasedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (!viewerControl.AddLayerFile(citiesPath, new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = DefaultCityStyle()
            }))
        {
            System.Windows.MessageBox.Show(
                this,
                $"Cities layer could not be loaded:{Environment.NewLine}{citiesPath}",
                "RuleBasedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var citiesLayer = viewerControl.GetLayerInfo(0);
        if (citiesLayer is null)
        {
            System.Windows.MessageBox.Show(
                this,
                "Loaded cities layer could not be inspected.",
                "RuleBasedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        viewerControl.SetLayerName(citiesLayer.Index, "Cities - rule based by POP_CLASS");
        ApplyRenderer(citiesLayer.Index);
        viewerControl.ViewExtent = new GeoKernelExtent(-180.0, -58.0, 180.0, 82.0);
        statusText.Text = "Rule based renderer applied: POP_CLASS";
    }

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

        if (!viewerControl.SetLayerRuleBasedRenderer(layerIndex, rules, DefaultCityStyle()))
        {
            System.Windows.MessageBox.Show(
                this,
                $"Could not apply rule based renderer from {PopClassFieldName} field.",
                "RuleBasedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        UpdateLegend(layerIndex);
        viewerControl.RefreshLayers();
    }

    private void UpdateLegend(int layerIndex)
    {
        var rendererJson = viewerControl.GetLayerSymbolRendererJson(layerIndex);
        using var document = JsonDocument.Parse(rendererJson);

        if (!document.RootElement.TryGetProperty("rules", out var rules) ||
            rules.ValueKind != JsonValueKind.Array)
        {
            legendListBox.ItemsSource = Array.Empty<LegendItem>();
            return;
        }

        var items = new List<LegendItem>();
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

            items.Add(new LegendItem(
                label,
                new System.Windows.Media.SolidColorBrush(ToMediaColor(style.PointColor)),
                new System.Windows.Media.SolidColorBrush(ToMediaColor(style.LineColor)),
                Math.Clamp(style.PointSize, 6.0, 20.0)));
        }

        legendListBox.ItemsSource = items;
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

    private static string WithAlpha(string rgb, byte alpha)
    {
        var hex = rgb.Trim().TrimStart('#');
        if (hex.Length != 6)
            return rgb;

        return $"#{alpha:X2}{hex.ToUpperInvariant()}";
    }

    private static System.Windows.Media.Color ToMediaColor(Color color)
    {
        return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
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

    private sealed record LegendItem(
        string Label,
        System.Windows.Media.Brush FillBrush,
        System.Windows.Media.Brush LineBrush,
        double SymbolSize);

    private readonly record struct LegendStyle(Color PointColor, Color LineColor, double PointSize)
    {
        public static LegendStyle Default { get; } = new(Color.FromArgb(165, 203, 213, 225), Color.FromArgb(220, 100, 116, 139), 4.0);

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
