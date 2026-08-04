using System.Text.Json;
using GeoKernel.NET.WinForms;

namespace GeoKernel.ClassificationMethods.Winforms;

public sealed partial class MainForm : Form
{
    private const string PopulationFieldName = "POPULATION";
    private int _countyLayerIndex = -1;
    private bool _loading;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        _loading = true;        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.AddOpenStreetMapLayer();

        FillMethods();

        var countiesPath = await SampleData.EnsureFileAsync("california.zip", "california", "california.shp", "California", this, CreateSampleProgress());
        if (!File.Exists(countiesPath))
        {
            MessageBox.Show(
                this,
                $"California shapefile could not be found:{Environment.NewLine}{countiesPath}",
                "ClassificationMethods",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (!geoKernelViewerControl.AddLayerFile(countiesPath, new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = BaseCountyStyle()
            }))
        {
            MessageBox.Show(
                this,
                $"California layer could not be loaded:{Environment.NewLine}{countiesPath}",
                "ClassificationMethods",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        var countiesLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (countiesLayer is null)
        {
            MessageBox.Show(
                this,
                "Loaded California layer could not be inspected.",
                "ClassificationMethods",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        _countyLayerIndex = countiesLayer.Index;
        geoKernelViewerControl.SetLayerName(_countyLayerIndex, "California counties - classification methods");

        _loading = false;
        ApplyRenderer();
        geoKernelViewerControl.ZoomToLayer(_countyLayerIndex);
        downloadProgressBar.Visible = false;
    }

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, p =>
    { statusLabel.Text = p.Message; downloadProgressBar.Visible = true; downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee; if (p.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(p.Percentage.Value, 0, 100); });

    private void FillMethods()
    {
        methodComboBox.Items.Clear();
        methodComboBox.Items.Add(new MethodOption("Equal Interval", GeoKernelClassificationMethod.EqualInterval));
        methodComboBox.Items.Add(new MethodOption("Quantile", GeoKernelClassificationMethod.Quantile));
        methodComboBox.Items.Add(new MethodOption("Standard Deviation", GeoKernelClassificationMethod.StandardDeviation));
        methodComboBox.SelectedIndex = 0;
    }

    private void ApplyRenderer()
    {
        if (_loading || _countyLayerIndex < 0 || methodComboBox.SelectedItem is not MethodOption method)
            return;

        if (!geoKernelViewerControl.ApplyLayerGraduatedRenderer(
            _countyLayerIndex,
            PopulationFieldName,
            method.Method,
            classCount: 5,
            colorRampName: GeoKernelColorRampNames.GreenBlue,
            interval: method.Method == GeoKernelClassificationMethod.StandardDeviation ? 1.0 : 0.0))
        {
            MessageBox.Show(
                this,
                $"Could not create graduated renderer from {PopulationFieldName} field.",
                "ClassificationMethods",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        UpdateLegend(_countyLayerIndex, method.Name);
        geoKernelViewerControl.RefreshLayers();
        statusLabel.Text = $"Classification method applied: {PopulationFieldName} / {method.Name}";
    }

    private void UpdateLegend(int layerIndex, string methodName)
    {
        legendListView.BeginUpdate();
        legendListView.Items.Clear();
        legendImageList.Images.Clear();
        classColumn.Text = $"{PopulationFieldName} - {methodName}";

        var rendererJson = geoKernelViewerControl.GetLayerSymbolRendererJson(layerIndex);
        using var document = JsonDocument.Parse(rendererJson);

        if (!document.RootElement.TryGetProperty("ranges", out var ranges) ||
            ranges.ValueKind != JsonValueKind.Array)
        {
            legendListView.EndUpdate();
            return;
        }

        var imageIndex = 0;
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

            legendImageList.Images.Add(CreateLegendBitmap(style));
            legendListView.Items.Add(new ListViewItem(label, imageIndex));
            imageIndex++;
        }

        UpdateLegendColumnWidth();
        legendListView.EndUpdate();
    }

    private static GeoKernelLayerStyle BaseCountyStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#DCE8E4",
            FillOpacity = 225,
            LineColor = "#536B68",
            LineWidth = 0.8
        };
    }

    private static string ReadRangeLabel(JsonElement range)
    {
        var lower = ReadDouble(range, "lower");
        var upper = ReadDouble(range, "upper");
        return $"{lower:N0} - {upper:N0}";
    }

    private static Bitmap CreateLegendBitmap(LegendStyle style)
    {
        var bitmap = new Bitmap(42, 24);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        using var brush = new SolidBrush(Color.FromArgb(style.FillOpacity, style.FillColor));
        using var pen = new Pen(style.LineColor, 1.5f);
        graphics.FillRectangle(brush, 5, 4, 32, 16);
        graphics.DrawRectangle(pen, 5, 4, 32, 16);
        return bitmap;
    }

    private void methodComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        ApplyRenderer();
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

    private sealed record MethodOption(string Name, GeoKernelClassificationMethod Method)
    {
        public override string ToString() => Name;
    }

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
