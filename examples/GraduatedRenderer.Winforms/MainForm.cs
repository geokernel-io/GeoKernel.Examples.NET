using System.Text.Json;
using GeoKernel.NET.WinForms;

namespace GeoKernel.GraduatedRenderer.Winforms;

public sealed partial class MainForm : Form
{
    private const string PopulationFieldName = "POPULATION";
    private int _countyLayerIndex = -1;
    private bool _loading;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        _loading = true;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.AddOpenStreetMapLayer();

        FillColorRamps();

        var countiesPath = Path.Combine(FindRepositoryRoot(), "assets", "data", "california", "california.shp");
        if (!File.Exists(countiesPath))
        {
            MessageBox.Show(
                this,
                $"California shapefile could not be found:{Environment.NewLine}{countiesPath}",
                "GraduatedRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (!geoKernelViewerControl.AddLayerFile(countiesPath))
        {
            MessageBox.Show(
                this,
                $"California layer could not be loaded:{Environment.NewLine}{countiesPath}",
                "GraduatedRenderer",
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
                "GraduatedRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        _countyLayerIndex = countiesLayer.Index;
        ApplyBaseCountyStyle(_countyLayerIndex);

        _loading = false;
        ApplyRenderer();
        geoKernelViewerControl.ZoomToLayer(_countyLayerIndex);
    }

    private void FillColorRamps()
    {
        rampComboBox.Items.Clear();
        foreach (var rampName in geoKernelViewerControl.GetColorRampNames())
            rampComboBox.Items.Add(rampName);

        var selectedIndex = rampComboBox.Items.IndexOf(GeoKernelColorRampNames.GreenBlue);
        rampComboBox.SelectedIndex = selectedIndex >= 0 ? selectedIndex : Math.Min(0, rampComboBox.Items.Count - 1);
    }

    private void ApplyBaseCountyStyle(int layerIndex)
    {
        geoKernelViewerControl.SetLayerStyle(layerIndex, new GeoKernelLayerStyle
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

        if (!geoKernelViewerControl.ApplyLayerGraduatedRenderer(
            _countyLayerIndex,
            PopulationFieldName,
            GeoKernelClassificationMethod.NaturalBreaks,
            classCount: 5,
            colorRampName: rampName))
        {
            MessageBox.Show(
                this,
                $"Could not create graduated renderer from {PopulationFieldName} field.",
                "GraduatedRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        UpdateLegend(_countyLayerIndex);
        geoKernelViewerControl.RefreshLayers();
        statusLabel.Text = $"Graduated renderer applied: {PopulationFieldName} / {rampName}";
    }

    private void UpdateLegend(int layerIndex)
    {
        legendListView.BeginUpdate();
        legendListView.Items.Clear();
        legendImageList.Images.Clear();

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

    private static string ReadRangeLabel(JsonElement range)
    {
        var lower = ReadDouble(range, "lower");
        var upper = ReadDouble(range, "upper");
        return $"{lower:N0} - {upper:N0}";
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

    private void rampComboBox_SelectedIndexChanged(object sender, EventArgs e)
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
