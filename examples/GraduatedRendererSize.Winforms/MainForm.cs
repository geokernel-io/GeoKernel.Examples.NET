using System.Text.Json;
using GeoKernel.NET.WinForms;

namespace GeoKernel.GraduatedRendererSize.Winforms;

public sealed partial class MainForm : Form
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

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        var citiesPath = Path.Combine(FindRepositoryRoot(), "assets", "data", "cities_4326.shp");
        if (!File.Exists(citiesPath))
        {
            MessageBox.Show(
                this,
                $"Cities shapefile could not be found:{Environment.NewLine}{citiesPath}",
                "GraduatedRendererSize",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                citiesPath,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = BaseCityStyle()
                }))
        {
            MessageBox.Show(
                this,
                $"Cities layer could not be loaded:{Environment.NewLine}{citiesPath}",
                "GraduatedRendererSize",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is null)
        {
            MessageBox.Show(
                this,
                "Loaded cities layer could not be inspected.",
                "GraduatedRendererSize",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        geoKernelViewerControl.SetLayerName(layer.Index, "Cities - graduated size by POP_CLASS");

        if (!geoKernelViewerControl.SetLayerCategorizedRenderer(
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
            MessageBox.Show(
                this,
                $"Could not create graduated size renderer from {ClassFieldName}.",
                "GraduatedRendererSize",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        UpdateLegend(layer.Index);
        geoKernelViewerControl.FullExtent();
        statusLabel.Text = "Graduated size renderer applied: POP_CLASS";
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

        UpdateLegendColumnWidth();
        legendListView.EndUpdate();
    }

    private static Bitmap CreateLegendBitmap(LegendStyle style)
    {
        var bitmap = new Bitmap(48, 26);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var brush = new SolidBrush(style.PointColor);
        using var pen = new Pen(style.LineColor, 1.0f);

        var radius = Math.Clamp(style.PointSize, 4.0, 18.0) / 2.0;
        graphics.FillEllipse(
            brush,
            (float)(24.0 - radius),
            (float)(13.0 - radius),
            (float)(radius * 2.0),
            (float)(radius * 2.0));
        graphics.DrawEllipse(
            pen,
            (float)(24.0 - radius),
            (float)(13.0 - radius),
            (float)(radius * 2.0),
            (float)(radius * 2.0));

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

    private readonly record struct LegendStyle(Color PointColor, Color LineColor, double PointSize)
    {
        public static LegendStyle Default { get; } = new(
            Color.FromArgb(70, 217, 95, 53),
            Color.FromArgb(150, 138, 58, 36),
            5.0);

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
