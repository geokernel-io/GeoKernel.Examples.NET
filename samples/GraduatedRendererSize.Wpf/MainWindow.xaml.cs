using System.IO;
using System.Text.Json;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GraduatedRendererSize.Wpf;

public partial class MainWindow : Window
{
    private const string ClassFieldName = "POP_CLASS";

    private static readonly SizeClass[] SizeClasses =
    [
        new("Less than 50,000", 5.0),
        new("50,000 to 100,000", 8.0),
        new("100,000 to 250,000", 11.0),
        new("250,000 to 500,000", 14.0),
        new("500,000 to 1,000,000", 17.0),
        new("1,000,000 to 5,000,000", 20.0)
    ];

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = Color.FromArgb(247, 248, 250);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        var citiesPath = Path.Combine(FindRepositoryRoot(), "assets", "data", "cities_4326.shp");
        if (!File.Exists(citiesPath))
        {
            System.Windows.MessageBox.Show(
                this,
                $"Cities shapefile could not be found:{Environment.NewLine}{citiesPath}",
                "GraduatedRendererSize",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (!viewerControl.AddLayerFile(
                citiesPath,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = BaseCityStyle()
                }))
        {
            System.Windows.MessageBox.Show(
                this,
                $"Cities layer could not be loaded:{Environment.NewLine}{citiesPath}",
                "GraduatedRendererSize",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var layer = viewerControl.GetLayerInfo(0);
        if (layer is null)
        {
            System.Windows.MessageBox.Show(
                this,
                "Loaded cities layer could not be inspected.",
                "GraduatedRendererSize",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        viewerControl.SetLayerName(layer.Index, "Cities - graduated size by POP_CLASS");

        if (!viewerControl.SetLayerCategorizedRenderer(
                layer.Index,
                ClassFieldName,
                SizeClasses.Select(item => new GeoKernelCategorizedSymbolClass
                {
                    Value = item.Label,
                    Label = item.Label,
                    Style = CityStyle(item.PointSize)
                }),
                BaseCityStyle()))
        {
            System.Windows.MessageBox.Show(
                this,
                $"Could not create graduated size renderer from {ClassFieldName}.",
                "GraduatedRendererSize",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        UpdateLegend(layer.Index);
        viewerControl.FullExtent();
        statusText.Text = "Graduated size renderer applied: POP_CLASS";
    }

    private static GeoKernelLayerStyle BaseCityStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#46D95F35",
            LineColor = "#968A3A24",
            LineWidth = 1.0,
            PointSize = 5.0
        };
    }

    private static GeoKernelLayerStyle CityStyle(double pointSize)
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#46D95F35",
            LineColor = "#968A3A24",
            LineWidth = 1.0,
            PointSize = pointSize
        };
    }

    private void UpdateLegend(int layerIndex)
    {
        var rendererJson = viewerControl.GetLayerSymbolRendererJson(layerIndex);
        using var document = JsonDocument.Parse(rendererJson);

        if (!document.RootElement.TryGetProperty("categories", out var categories) ||
            categories.ValueKind != JsonValueKind.Array)
        {
            legendListBox.ItemsSource = Array.Empty<LegendItem>();
            return;
        }

        var items = new List<LegendItem>();
        foreach (var category in categories.EnumerateArray())
        {
            if (category.TryGetProperty("enabled", out var enabled) && !enabled.GetBoolean())
                continue;

            var label = ReadString(category, "label");
            if (string.IsNullOrWhiteSpace(label))
                label = "(empty)";

            var style = category.TryGetProperty("style", out var styleElement)
                ? LegendStyle.FromJson(styleElement)
                : LegendStyle.Default;

            items.Add(new LegendItem(
                label,
                Math.Clamp(style.PointSize, 4.0, 18.0),
                new System.Windows.Media.SolidColorBrush(ToMediaColor(style.PointColor)),
                new System.Windows.Media.SolidColorBrush(ToMediaColor(style.LineColor))));
        }

        legendListBox.ItemsSource = items;
    }

    private static System.Windows.Media.Color ToMediaColor(Color color)
    {
        return System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
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

    private sealed record SizeClass(string Label, double PointSize);

    private sealed record LegendItem(
        string Label,
        double Diameter,
        System.Windows.Media.Brush FillBrush,
        System.Windows.Media.Brush LineBrush);

    private readonly record struct LegendStyle(Color PointColor, Color LineColor, double PointSize)
    {
        public static LegendStyle Default { get; } = new(Color.FromArgb(70, 217, 95, 53), Color.FromArgb(150, 138, 58, 36), 5.0);

        public static LegendStyle FromJson(JsonElement style)
        {
            var pointColor = ReadColor(style, "pointColor", Default.PointColor);
            var lineColor = ReadColor(style, "lineColor", Default.LineColor);
            var pointSize = ReadDouble(style, "pointSize", Default.PointSize);
            return new LegendStyle(pointColor, lineColor, pointSize);
        }

        private static Color ReadColor(JsonElement element, string propertyName, Color fallback)
        {
            var value = ReadString(element, propertyName);
            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            if (value.Length == 9 && value[0] == '#')
            {
                try
                {
                    var argb = Convert.ToUInt32(value[1..], 16);
                    return Color.FromArgb(
                        (int)((argb >> 24) & 0xff),
                        (int)((argb >> 16) & 0xff),
                        (int)((argb >> 8) & 0xff),
                        (int)(argb & 0xff));
                }
                catch (Exception)
                {
                    return fallback;
                }
            }

            try
            {
                return ColorTranslator.FromHtml(value);
            }
            catch (Exception)
            {
                return fallback;
            }
        }
    }
}
