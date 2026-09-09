using System.Text.Json;
using GeoKernel.NET.WinForms;

namespace GeoKernel.CategorizedRenderer.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.AddOpenStreetMapLayer();

        var statesPath = await SampleData.EnsureFileAsync("usa_states_3857.zip", "usa_states_3857", "usa_states_3857.shp", "USA states", this, CreateSampleProgress());
        if (!File.Exists(statesPath))
        {
            MessageBox.Show(
                this,
                $"States shapefile could not be found:{Environment.NewLine}{statesPath}",
                "CategorizedRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (!geoKernelViewerControl.AddLayerFile(statesPath))
        {
            MessageBox.Show(
                this,
                $"States layer could not be loaded:{Environment.NewLine}{statesPath}",
                "CategorizedRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var statesLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (statesLayer is null)
        {
            MessageBox.Show(
                this,
                "Loaded states layer could not be inspected.",
                "CategorizedRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var statesIndex = statesLayer.Index;
        ApplyBaseStateStyle(statesIndex);

        if (!geoKernelViewerControl.ApplyLayerCategorizedRenderer(
            statesIndex,
            "STATE",
            GeoKernelColorRampNames.Unique,
            categoryLimit: 64))
        {
            MessageBox.Show(
                this,
                "Could not create categorized renderer from STATE field.",
                "CategorizedRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        UpdateLegend(statesIndex);
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-16831516.0, 1856556.0, -4631023.0, 7472472.0);
        statusLabel.Text = "Categorized renderer applied: STATE";
        downloadProgressBar.Visible = false;
    }

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, p =>
    { statusLabel.Text = p.Message; downloadProgressBar.Visible = true; downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee; if (p.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(p.Percentage.Value, 0, 100); });

    private void ApplyBaseStateStyle(int layerIndex)
    {
        geoKernelViewerControl.SetLayerStyle(layerIndex, new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 220,
            LineColor = "#536B68",
            LineWidth = 0.9
        });
    }

    private void UpdateLegend(int layerIndex)
    {
        legendListView.BeginUpdate();
        legendListView.Items.Clear();
        legendImageList.Images.Clear();

        var rendererJson = geoKernelViewerControl.GetLayerSymbolRendererJson(layerIndex);
        using var document = JsonDocument.Parse(rendererJson);

        if (!document.RootElement.TryGetProperty("categories", out var categories) ||
            categories.ValueKind != JsonValueKind.Array)
        {
            legendListView.EndUpdate();
            return;
        }

        var imageIndex = 0;
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

            legendImageList.Images.Add(CreateLegendBitmap(style));
            legendListView.Items.Add(new ListViewItem(label, imageIndex));
            imageIndex++;
        }

        legendListView.EndUpdate();
    }

    private static Bitmap CreateLegendBitmap(LegendStyle style)
    {
        var bitmap = new Bitmap(38, 22);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var brush = new SolidBrush(Color.FromArgb(style.FillOpacity, style.FillColor));
        using var pen = new Pen(style.LineColor, 1.5f);
        graphics.FillRectangle(brush, 5, 4, 28, 14);
        graphics.DrawRectangle(pen, 5, 4, 28, 14);
        return bitmap;
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
