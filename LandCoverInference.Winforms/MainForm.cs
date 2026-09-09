using System.Text.Json;
using System.IO;
using GeoKernel.Examples.Common;
using GeoKernel.NET.WinForms;

namespace GeoKernel.LandCoverInference.Winforms;

public sealed class MainForm : Form
{
    static readonly Uri RasterUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/bilbao_s2_rgbnir_2021.zip");
    static readonly Uri ModelUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/landcover-bilbao-model.zip");
    static readonly AIClassColor[] WorldCoverColors =
    [
        new(10, 0, 100, 0), new(20, 255, 187, 34), new(30, 255, 255, 76),
        new(40, 240, 150, 255), new(50, 250, 0, 0), new(60, 180, 180, 180),
        new(70, 240, 240, 240), new(80, 0, 100, 200), new(90, 0, 150, 160),
        new(95, 0, 207, 117), new(100, 250, 230, 160)
    ];

    readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    readonly TextBox modelPath = new();
    readonly TextBox rasterPath = new();
    readonly TextBox outputPath = new();
    readonly ComboBox provider = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    readonly TrackBar predictionOpacity = new() { Minimum = 0, Maximum = 100, Value = 50, TickStyle = TickStyle.None, Dock = DockStyle.Fill };
    readonly Label predictionOpacityValue = new() { Text = "50%", AutoSize = true, Anchor = AnchorStyles.Left };
    readonly Button run = new() { Text = "Run land-cover inference", Dock = DockStyle.Fill, Margin = new Padding(0, 3, 0, 3) };
    readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100, Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) };
    readonly Label progressText = new() { AutoSize = true, Anchor = AnchorStyles.Left };
    readonly TextBox details = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill };
    readonly ToolStripStatusLabel status = new();
    int predictionLayerIndex = -1;

    public MainForm()
    {
        Text = "LandCoverInference"; Width = 1280; Height = 820;
        Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "resources", "GeoKernelAppIcon.ico"));
        provider.Items.AddRange(["Auto", "CPU", "CUDA", "DirectML"]); provider.SelectedIndex = 0;

        var right = new Panel { Dock = DockStyle.Right, Width = 420, Padding = new Padding(10) };
        var upper = new TableLayoutPanel { Dock = DockStyle.Top, Height = 326, ColumnCount = 2, RowCount = 10, Padding = new Padding(0), Margin = new Padding(0) };
        upper.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120)); upper.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        upper.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        upper.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        upper.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        upper.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        upper.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        upper.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        upper.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        upper.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        upper.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        upper.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        upper.Controls.Add(new Label { Text = "Land-cover inference", AutoSize = true, Font = new Font(Font, FontStyle.Bold) }, 0, 0); upper.SetColumnSpan(upper.GetControlFromPosition(0, 0)!, 2);
        upper.Controls.Add(new Label { Text = "Run a GeoKernel model package on a georeferenced RGBNIR raster.", AutoSize = true }, 0, 1); upper.SetColumnSpan(upper.GetControlFromPosition(0, 1)!, 2);
        AddPathRow(upper, 2, "Model package", modelPath, BrowseModel);
        AddPathRow(upper, 3, "Input raster", rasterPath, BrowseRaster);
        AddPathRow(upper, 4, "Class mask", outputPath, BrowseOutput);
        upper.Controls.Add(new Label { Text = "Execution provider", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 5); provider.Dock = DockStyle.Fill; upper.Controls.Add(provider, 1, 5);
        var opacityHolder = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) }; opacityHolder.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); opacityHolder.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 42)); opacityHolder.Controls.Add(predictionOpacity, 0, 0); opacityHolder.Controls.Add(predictionOpacityValue, 1, 0);
        upper.Controls.Add(new Label { Text = "Prediction opacity", AutoSize = true, Anchor = AnchorStyles.Left }, 0, 6); upper.Controls.Add(opacityHolder, 1, 6);
        upper.Controls.Add(run, 0, 7); upper.SetColumnSpan(run, 2);
        upper.Controls.Add(progress, 0, 8); upper.SetColumnSpan(progress, 2);
        upper.Controls.Add(progressText, 0, 9); upper.SetColumnSpan(progressText, 2);
        var legend = CreateLegend();
        right.Controls.Add(details); right.Controls.Add(new Label { Text = "Inference diagnostics", Dock = DockStyle.Top, Height = 22 }); right.Controls.Add(upper); right.Controls.Add(legend);
        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        tools.Items.Add("Zoom In", null, (_, _) => viewer.ZoomIn()); tools.Items.Add("Zoom Out", null, (_, _) => viewer.ZoomOut()); tools.Items.Add("Full Extent", null, (_, _) => viewer.FullExtent()); tools.Items.Add("Zoom Rect", null, (_, _) => viewer.ActiveTool = GeoKernelViewerTool.ZoomBox); tools.Items.Add("Pan", null, (_, _) => viewer.ActiveTool = GeoKernelViewerTool.Pan);
        var bar = new StatusStrip(); bar.Items.Add(status); Controls.Add(viewer); Controls.Add(right); Controls.Add(tools); Controls.Add(bar);
        viewer.ActiveTool = GeoKernelViewerTool.Pan;
        run.Click += async (_, _) => await RunInference(); predictionOpacity.ValueChanged += (_, _) => ApplyPredictionOpacity(); details.Text = "Preparing the Bilbao sample...";
        Shown += async (_, _) => await PrepareSamples();
    }

    static string Existing(string path) => Directory.Exists(path) || File.Exists(path) ? path : "";
    static Control CreateLegend()
    {
        var legend = new Panel { Dock = DockStyle.Bottom, Height = 88 };
        var title = new Label { Text = "ESA WorldCover classes", Dock = DockStyle.Top, Height = 22, Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold) };
        var items = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoScroll = false, WrapContents = true, Padding = new Padding(0), Margin = new Padding(0) };
        foreach (var item in new (string Name, Color Color)[]
        {
            ("Tree cover", Color.FromArgb(0, 100, 0)), ("Shrubland", Color.FromArgb(255, 187, 34)),
            ("Grassland", Color.FromArgb(255, 255, 76)), ("Cropland", Color.FromArgb(240, 150, 255)),
            ("Built-up", Color.FromArgb(250, 0, 0)), ("Bare / sparse", Color.FromArgb(180, 180, 180)),
            ("Permanent water", Color.FromArgb(0, 100, 200))
        })
        {
            var entry = new FlowLayoutPanel { AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, WrapContents = false, Margin = new Padding(0, 1, 12, 1) };
            entry.Controls.Add(new Label { BackColor = item.Color, Width = 10, Height = 10, Margin = new Padding(0, 4, 4, 0) });
            entry.Controls.Add(new Label { Text = item.Name, AutoSize = true, Margin = new Padding(0, 1, 0, 0) });
            items.Controls.Add(entry);
        }
        legend.Controls.Add(items);
        legend.Controls.Add(title);
        return legend;
    }
    async Task PrepareSamples()
    {
        run.Enabled = false;
        try
        {
            await Task.Yield();
            rasterPath.Text = SampleData.EnsureSampleFile(RasterUrl, "bilbao_s2_rgbnir_2021.zip", "bilbao_s2_rgbnir_2021", "bilbao_s2_rgbnir_2021.tif", this);
            var manifest = SampleData.EnsureSampleFile(ModelUrl, "landcover-bilbao-model.zip", "landcover-bilbao-model", "geokernel-model.json", this);
            modelPath.Text = Path.GetDirectoryName(manifest)!;
            UpdateOutputPath();
            OpenBaseRaster();
            details.Text = "Bilbao input raster is open. Run land-cover inference to add the prediction layer.";
        }
        catch (Exception ex) { details.Text = "Sample preparation failed:\r\n" + ex.Message; MessageBox.Show(this, ex.Message, Text); }
        finally { run.Enabled = true; }
    }
    void OpenBaseRaster()
    {
        predictionLayerIndex = -1;
        viewer.ClearLayers();
        if (!viewer.AddLayerFile(rasterPath.Text)) throw new InvalidOperationException("The Bilbao input raster could not be opened.");
        viewer.SetLayerName(0, "Bilbao RGBNIR raster"); viewer.RefreshLayers(); viewer.FullExtent(); status.Text = "Map ready.";
    }
    void ApplyPredictionOpacity()
    {
        predictionOpacityValue.Text = $"{predictionOpacity.Value}%";
        if (predictionLayerIndex < 0) return;
        viewer.SetLayerOpacity(predictionLayerIndex, predictionOpacity.Value / 100d); viewer.RefreshLayers();
    }
    void UpdateOutputPath() { if (File.Exists(rasterPath.Text)) outputPath.Text = Path.Combine(Path.GetDirectoryName(rasterPath.Text)!, Path.GetFileNameWithoutExtension(rasterPath.Text) + "_landcover_mask.tif"); }
    static void AddPathRow(TableLayoutPanel panel, int row, string label, TextBox editor, EventHandler browse)
    {
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, row); var holder = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) }; holder.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); holder.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 78)); holder.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); editor.Dock = DockStyle.Fill; editor.Margin = new Padding(0, 3, 4, 3); var button = new Button { Text = "Browse...", Dock = DockStyle.Fill, Margin = new Padding(0, 2, 0, 2) }; button.Click += browse; holder.Controls.Add(editor, 0, 0); holder.Controls.Add(button, 1, 0); panel.Controls.Add(holder, 1, row);
    }
    void BrowseModel(object? s, EventArgs e) { using var dialog = new FolderBrowserDialog { InitialDirectory = modelPath.Text }; if (dialog.ShowDialog(this) == DialogResult.OK) modelPath.Text = dialog.SelectedPath; }
    void BrowseRaster(object? s, EventArgs e) { using var dialog = new OpenFileDialog { Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff", FileName = rasterPath.Text }; if (dialog.ShowDialog(this) == DialogResult.OK) { rasterPath.Text = dialog.FileName; UpdateOutputPath(); OpenBaseRaster(); } }
    void BrowseOutput(object? s, EventArgs e) { using var dialog = new SaveFileDialog { Filter = "GeoTIFF (*.tif)|*.tif", FileName = outputPath.Text }; if (dialog.ShowDialog(this) == DialogResult.OK) outputPath.Text = dialog.FileName; }

    async Task RunInference()
    {
        if (!Directory.Exists(modelPath.Text) || !File.Exists(rasterPath.Text)) { MessageBox.Show(this, "Select an existing model package and input raster.", Text); return; }
        if (string.IsNullOrWhiteSpace(outputPath.Text)) { MessageBox.Show(this, "Select a class-mask output path.", Text); return; }
        run.Enabled = false; SetProgress(0, "Opening model package..."); details.Text = "Validating the model package and preparing tiled inference...";
        try
        {
            var preview = Path.Combine(Path.GetDirectoryName(outputPath.Text)!, Path.GetFileNameWithoutExtension(outputPath.Text) + "_preview.tif");
            var selected = Enum.Parse<AIExecutionProvider>(provider.Text);
            var reporter = new Progress<AIProgress>(x => SetProgress(x.Percent, x.Message));
            var result = await GeoKernelAI.RunRasterInferenceAsync(new AIRasterInferenceRequest { ModelPackagePath = modelPath.Text, RasterPath = rasterPath.Text, OutputPath = outputPath.Text, PreviewOutputPath = preview, ApplyManifestClassCodes = true, ClassPalette = WorldCoverColors, Bands = [1, 2, 3, 4], Provider = selected, OutputMode = AIRasterOutputMode.ClassMask }, reporter);
            SetProgress(95, "Opening color preview..."); viewer.RemoveLayerByName("Land-cover prediction"); if (!viewer.AddLayerFile(preview)) throw new InvalidOperationException("The color preview could not be opened."); predictionLayerIndex = 0; viewer.SetLayerName(predictionLayerIndex, "Land-cover prediction"); viewer.SetLayerOpacity(predictionLayerIndex, predictionOpacity.Value / 100d); viewer.RefreshLayers();
            details.Text = Diagnostics(result, outputPath.Text, preview); SetProgress(100, "Inference complete"); status.Text = $"Land-cover mask created in {result.GetProperty("elapsedMilliseconds").GetInt64()} ms.";
        }
        catch (Exception ex) { progress.Value = 0; progressText.Text = "Inference failed"; details.Text = "Inference failed:\r\n" + ex.Message; MessageBox.Show(this, ex.Message, Text); }
        finally { run.Enabled = true; }
    }
    void SetProgress(int value, string text) { progress.Value = Math.Clamp(value, 0, 100); progressText.Text = $"{progress.Value}% — {text}"; status.Text = text; }
    static string Diagnostics(JsonElement r, string mask, string preview) => $"GeoKernel AI land-cover inference\r\n\r\nProvider: {r.GetProperty("provider").GetString()}\r\nRaster: {r.GetProperty("width").GetInt32()} x {r.GetProperty("height").GetInt32()}\r\nTiles: {r.GetProperty("processedTiles").GetInt32()}\r\nElapsed: {r.GetProperty("elapsedMilliseconds").GetInt64()} ms\r\n\r\nClass mask:\r\n{mask}\r\n\r\nColor preview:\r\n{preview}";
}
