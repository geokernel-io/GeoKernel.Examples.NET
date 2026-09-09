using GeoKernel.NET.WinForms;

namespace GeoKernel.GeoPackageLoad.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        var path = await SampleData.EnsureFileAsync("europe_detailed.zip", "europe_detailed_gpkg",
            "europe_detailed.gpkg", "Europe GeoPackage", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(path))
        {
            statusLabel.Text = "GeoPackage sample data could not be prepared.";
            return;
        }
        try
        {
            if (!viewerControl.AddLayerFile(path))
                throw new InvalidOperationException($"GeoPackage could not be opened: {path}");
            viewerControl.SetLayerStyle(0, GeoPackageStyle());
            PopulateDetails(path); viewerControl.FullExtent();
        }
        catch (Exception ex)
        {
            statusLabel.Text = "GeoPackage could not be opened.";
            MessageBox.Show(this, $"GeoPackage could not be opened:{Environment.NewLine}{ex.Message}",
                "GeoPackageLoad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PopulateDetails(string path)
    {
        const int index = 0;
        var layer = viewerControl.GetLayerInfo(index)
            ?? throw new InvalidOperationException("Loaded layer metadata is unavailable.");
        var definitions = viewerControl.GetLayerAttributeDefinitions(index);
        detailsTextBox.Text = string.Join(Environment.NewLine,
            "GeoPackageLoad sample", "", "API", "GisLayerGPKG(path)", "layer.open()",
            "layer.attributeDefinitions()", "layer.attributesForRow(row)", "",
            "Loaded GeoPackage", path, "", "Layer", $"Name: {layer.Name}",
            $"Shape type: {layer.ShapeType}", $"Memory shape count: {layer.FeatureCount}",
            "Provider-backed GeoPackage files can render features while memory shape count remains 0.",
            $"Field count: {definitions.Count}", $"Extent: {layer.ProjectedExtent}", "",
            "File", FileLine(".gpkg", path));
        schemaGrid.Rows.Clear();
        foreach (var definition in definitions)
            schemaGrid.Rows.Add(definition.Name, definition.Type, definition.Length, definition.DecimalCount);
        attributesGrid.Columns.Clear(); attributesGrid.Columns.Add("row", "#");
        foreach (var definition in definitions) attributesGrid.Columns.Add(definition.Name, definition.Name);
        attributesGrid.Rows.Clear();
        var rowCount = Math.Min(12, layer.FeatureCount);
        for (var row = 0; row < rowCount; ++row)
        {
            var attributes = viewerControl.GetLayerFeatureAttributes(index, row);
            attributesGrid.Rows.Add(new object[] { row }.Concat(definitions.Select(definition =>
                attributes.TryGetValue(definition.Name, out var value)
                    ? value?.ToString() ?? string.Empty : string.Empty)).ToArray());
        }
        if (rowCount == 0) attributesGrid.Rows.Add("No attribute rows returned.");
        statusLabel.Text = $"GisLayerGPKG opened {layer.FeatureCount} memory features and {definitions.Count} fields.";
    }

    private static string FileLine(string label, string path)
    {
        var info = new FileInfo(path);
        return $"{label}: {(info.Exists ? info.Length : 0)} bytes ({(info.Exists ? "exists" : "missing")})";
    }
    private static GeoKernelLayerStyle GeoPackageStyle() => new()
    {
        FillColor = "#D7E5DF", FillOpacity = 184, LineColor = "#6D8C86", LineWidth = 1.1,
        PointColor = "#D95D39", PointSize = 7.0
    };
    private IProgress<SampleDataProgress> CreateSampleProgress() =>
        new ControlProgress<SampleDataProgress>(this, value =>
        {
            statusLabel.Text = value.Message; downloadProgressBar.Visible = true;
            downloadProgressBar.Style = value.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
            if (value.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(value.Percentage.Value, 0, 100);
        });
}
