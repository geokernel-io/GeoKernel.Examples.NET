using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GraduatedRenderer.Wpf;

public partial class MainWindow : Window
{
    private const string PopulationFieldName = "POPULATION";
    private int _countyLayerIndex = -1;
    private bool _loading;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        _loading = true;
        viewerControl.MapBackgroundColor = Color.FromArgb(247, 248, 250);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.AddOpenStreetMapLayer();

        FillColorRamps();

        var countiesPath = Path.Combine(FindRepositoryRoot(), "assets", "data", "california", "california.shp");
        if (!File.Exists(countiesPath))
        {
            System.Windows.MessageBox.Show(
                this,
                $"California shapefile could not be found:{Environment.NewLine}{countiesPath}",
                "GraduatedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (!viewerControl.AddLayerFile(countiesPath))
        {
            System.Windows.MessageBox.Show(
                this,
                $"California layer could not be loaded:{Environment.NewLine}{countiesPath}",
                "GraduatedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var countiesLayer = viewerControl.GetLayerInfo(0);
        if (countiesLayer is null)
        {
            System.Windows.MessageBox.Show(
                this,
                "Loaded California layer could not be inspected.",
                "GraduatedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        _countyLayerIndex = countiesLayer.Index;
        ApplyBaseCountyStyle(_countyLayerIndex);

        _loading = false;
        ApplyRenderer();
        viewerControl.ZoomToLayer(_countyLayerIndex);
    }

    private void FillColorRamps()
    {
        rampComboBox.Items.Clear();
        foreach (var rampName in viewerControl.GetColorRampNames())
            rampComboBox.Items.Add(rampName);

        rampComboBox.SelectedItem = rampComboBox.Items.Contains(GeoKernelColorRampNames.GreenBlue)
            ? GeoKernelColorRampNames.GreenBlue
            : rampComboBox.Items.Cast<string>().FirstOrDefault();
    }

    private void ApplyBaseCountyStyle(int layerIndex)
    {
        viewerControl.SetLayerStyle(layerIndex, new GeoKernelLayerStyle
        {
            FillColor = "#DCE8E4",
            FillOpacity = 225,
            LineColor = "#536B68",
            LineWidth = 0.8
        });
    }

    private void ApplyRenderer()
    {
        if (_loading || _countyLayerIndex < 0 || rampComboBox.SelectedItem is not string rampName)
            return;

        if (!viewerControl.ApplyLayerGraduatedRenderer(
            _countyLayerIndex,
            PopulationFieldName,
            GeoKernelClassificationMethod.NaturalBreaks,
            classCount: 5,
            colorRampName: rampName))
        {
            System.Windows.MessageBox.Show(
                this,
                $"Could not create graduated renderer from {PopulationFieldName} field.",
                "GraduatedRenderer",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        UpdateLegend(_countyLayerIndex);
        viewerControl.RefreshLayers();
        statusText.Text = $"Graduated renderer applied: {PopulationFieldName} / {rampName}";
    }

    private void UpdateLegend(int layerIndex)
    {
        var rendererJson = viewerControl.GetLayerSymbolRendererJson(layerIndex);
        using var document = JsonDocument.Parse(rendererJson);

        if (!document.RootElement.TryGetProperty("ranges", out var ranges) ||
            ranges.ValueKind != JsonValueKind.Array)
        {
            legendListBox.ItemsSource = Array.Empty<LegendItem>();
            return;
        }

        var items = new List<LegendItem>();
        foreach (var range in ranges.EnumerateArray())
        {
            if (range.TryGetProperty("enabled", out var enabled) && !enabled.GetBoolean())
                continue;

            var label = ReadString(range, "label");
            if (string.IsNullOrWhiteSpace(label))
                label = ReadRangeLabel(range);

            var style = range.TryGetProperty("style", out var styleElement)
                ? LegendStyle.FromJson(styleElement)
                : LegendStyle.Default;

            items.Add(new LegendItem(
                label,
                new System.Windows.Media.SolidColorBrush(ToMediaColor(style.FillColor, style.FillOpacity)),
                new System.Windows.Media.SolidColorBrush(ToMediaColor(style.LineColor, 255))));
        }

        legendListBox.ItemsSource = items;
    }

    private void rampComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ApplyRenderer();
    }

    private static string ReadRangeLabel(JsonElement range)
    {
        var lower = ReadDouble(range, "lower");
        var upper = ReadDouble(range, "upper");
        return $"{lower:N0} - {upper:N0}";
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

    private static double ReadDouble(JsonElement element, string propertyName)
    {
        return element.TryGetProperty(propertyName, out var property) && property.TryGetDouble(out var value)
            ? value
            : 0.0;
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
        public static LegendStyle Default { get; } = new(Color.FromArgb(220, 232, 228), 225, Color.FromArgb(83, 107, 104));

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
