using GeoKernel.NET.WinForms;

namespace GeoKernel.ShapefileLoad.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        var path = await SampleData.EnsureFileAsync(
            "world_4326.zip", "world_4326", "world_4326.shp",
            "World shapefile", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(path))
        {
            statusLabel.Text = "Shapefile sample data could not be prepared.";
            return;
        }

        try
        {
            if (!viewerControl.AddLayerFile(path))
                throw new InvalidOperationException($"Shapefile could not be opened: {path}");
            viewerControl.SetLayerStyle(0, WorldStyle());
            PopulateDetails(path);
            viewerControl.FullExtent();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Shapefile could not be opened:{Environment.NewLine}{ex.Message}",
                "ShapefileLoad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PopulateDetails(string path)
    {
        const int layerIndex = 0;
        var layer = viewerControl.GetLayerInfo(layerIndex)
            ?? throw new InvalidOperationException("Loaded layer metadata is unavailable.");
        var definitions = viewerControl.GetLayerAttributeDefinitions(layerIndex);
        var shp = new FileInfo(path);
        var stem = Path.Combine(shp.DirectoryName!, Path.GetFileNameWithoutExtension(path));

        detailsTextBox.Text = string.Join(Environment.NewLine,
            "ShapefileLoad sample", "", "API", "GisLayerSHP(path)", "layer.open()",
            "layer.attributeDefinitions()", "layer.attributesForRow(row)", "",
            "Loaded shapefile", path, "", "Layer", $"Name: {layer.Name}",
            $"Shape type: {layer.ShapeType}", $"Memory shape count: {layer.FeatureCount}",
            "Provider-backed shapefiles can render features while memory shape count remains 0.",
            $"Field count: {definitions.Count}", $"Extent: {layer.ProjectedExtent}", "",
            "Sidecars", $".shp: {FileSize(path)} bytes", $".shx: {FileSize(stem + ".shx")} bytes",
            $".dbf: {FileSize(stem + ".dbf")} bytes");

        schemaGrid.Rows.Clear();
        foreach (var definition in definitions)
            schemaGrid.Rows.Add(definition.Name, definition.Type, definition.Length, definition.DecimalCount);

        attributesGrid.Columns.Clear();
        attributesGrid.Columns.Add("row", "#");
        foreach (var definition in definitions)
            attributesGrid.Columns.Add(definition.Name, definition.Name);
        attributesGrid.Rows.Clear();
        var rows = Math.Min(12, layer.FeatureCount);
        for (var row = 0; row < rows; ++row)
        {
            var attributes = viewerControl.GetLayerFeatureAttributes(layerIndex, row);
            attributesGrid.Rows.Add(
                new object[] { row }.Concat(definitions.Select(
                    definition => attributes.TryGetValue(definition.Name, out var value)
                        ? value?.ToString() ?? string.Empty
                        : string.Empty)).ToArray());
        }
        if (rows == 0)
            attributesGrid.Rows.Add("No attribute rows returned.");

        statusLabel.Text = $"GisLayerSHP opened {layer.FeatureCount} features and {definitions.Count} fields.";
    }

    private static long FileSize(string path) => File.Exists(path) ? new FileInfo(path).Length : 0;
    private static GeoKernelLayerStyle WorldStyle() => new()
        { FillColor = "#D7E5DF", LineColor = "#6D8C86", LineWidth = 1.0 };

    private IProgress<SampleDataProgress> CreateSampleProgress() =>
        new ControlProgress<SampleDataProgress>(this, value =>
        {
            statusLabel.Text = value.Message;
            downloadProgressBar.Visible = true;
            downloadProgressBar.Style = value.Percentage.HasValue
                ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
            if (value.Percentage.HasValue)
                downloadProgressBar.Value = Math.Clamp(value.Percentage.Value, 0, 100);
        });
}
