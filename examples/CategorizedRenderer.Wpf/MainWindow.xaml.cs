using System.IO;
using System.Text.Json;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.CategorizedRenderer.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.AddOpenStreetMapLayer();

        var statesPath = Path.Combine(FindRepositoryRoot(), "assets", "data", "usa_states_3857.shp");
        if (!File.Exists(statesPath))
        {
            System.Windows.MessageBox.Show(
                this,
                $"States shapefile could not be found:{Environment.NewLine}{statesPath}",
                "CategorizedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (!viewerControl.AddLayerFile(statesPath))
        {
            System.Windows.MessageBox.Show(
                this,
                $"States layer could not be loaded:{Environment.NewLine}{statesPath}",
                "CategorizedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var statesLayer = viewerControl.GetLayerInfo(0);
        if (statesLayer is null)
        {
            System.Windows.MessageBox.Show(
                this,
                "Loaded states layer could not be inspected.",
                "CategorizedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var statesIndex = statesLayer.Index;
        ApplyBaseStateStyle(statesIndex);

        if (!viewerControl.ApplyLayerCategorizedRenderer(
            statesIndex,
            "STATE",
            GeoKernelColorRampNames.Unique,
            categoryLimit: 64))
        {
            System.Windows.MessageBox.Show(
                this,
                "Could not create categorized renderer from STATE field.",
                "CategorizedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        UpdateLegend(statesIndex);
        viewerControl.ViewExtent = new GeoKernelExtent(-16831516.0, 1856556.0, -4631023.0, 7472472.0);
        statusText.Text = "Categorized renderer applied: STATE";
    }

    private void ApplyBaseStateStyle(int layerIndex)
    {
        viewerControl.SetLayerStyle(layerIndex, new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 220,
            LineColor = "#536B68",
            LineWidth = 0.9
        });
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
                new System.Windows.Media.SolidColorBrush(ToMediaColor(style.FillColor, style.FillOpacity)),
                new System.Windows.Media.SolidColorBrush(ToMediaColor(style.LineColor, 255))));
        }

        legendListBox.ItemsSource = items;
    }

    private static System.Windows.Media.Color ToMediaColor(Color color, int alpha)
    {
        return System.Windows.Media.Color.FromArgb((byte)Math.Clamp(alpha, 0, 255), color.R, color.G, color.B);
    }

    private static string ReadString(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString() ?? string.Empty
            : string.Empty;
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

    private sealed record LegendItem(string Label, System.Windows.Media.Brush FillBrush, System.Windows.Media.Brush LineBrush);

    private readonly record struct LegendStyle(Color FillColor, int FillOpacity, Color LineColor)
    {
        public static LegendStyle Default { get; } = new(Color.FromArgb(216, 229, 225), 220, Color.FromArgb(83, 107, 104));

        public static LegendStyle FromJson(JsonElement style)
        {
            var fillColor = ReadColor(style, "fillColor", Default.FillColor);
            var fillOpacity = ReadInt(style, "fillOpacity", Default.FillOpacity);
            var lineColor = ReadColor(style, "lineColor", Default.LineColor);
            return new LegendStyle(fillColor, Math.Clamp(fillOpacity, 0, 255), lineColor);
        }

        private static Color ReadColor(JsonElement element, string propertyName, Color fallback)
        {
            var value = ReadString(element, propertyName);

            if (string.IsNullOrWhiteSpace(value))
                return fallback;

            try
            {
                return ColorTranslator.FromHtml(value);
            }
            catch (Exception)
            {
                return fallback;
            }
        }

        private static int ReadInt(JsonElement element, string propertyName, int fallback)
        {
            return element.TryGetProperty(propertyName, out var property) && property.TryGetInt32(out var value)
                ? value
                : fallback;
        }
    }
}
