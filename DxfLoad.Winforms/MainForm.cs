using GeoKernel.NET.WinForms;

namespace GeoKernel.DxfLoad.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm() => InitializeComponent();
    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        var path = await SampleData.EnsureFileAsync("geog_25000_dxf.zip", "geog_25000_dxf",
            "geog_25000.dxf", "DXF sample", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(path)) { statusLabel.Text = "DXF sample data could not be prepared."; return; }
        try
        {
            if (!viewerControl.AddLayerFile(path)) throw new InvalidOperationException($"DXF could not be opened: {path}");
            viewerControl.SetLayerStyle(0, DxfStyle()); PopulateDetails(path); viewerControl.FullExtent();
        }
        catch (Exception ex)
        {
            statusLabel.Text = "DXF could not be opened.";
            MessageBox.Show(this, $"DXF could not be opened:{Environment.NewLine}{ex.Message}",
                "DxfLoad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PopulateDetails(string path)
    {
        const int index = 0;
        var layer = viewerControl.GetLayerInfo(index) ?? throw new InvalidOperationException("Loaded layer metadata is unavailable.");
        var definitions = viewerControl.GetLayerAttributeDefinitions(index);
        detailsTextBox.Text = string.Join(Environment.NewLine,
            "DxfLoad sample", "", "API", "GisLayerDXF(path)", "layer.open()",
            "layer.attributeDefinitions()", "layer.attributesForRow(row)", "",
            "Loaded CAD DXF", path, "", "Layer", $"Name: {layer.Name}",
            $"Shape type: {layer.ShapeType}", $"Memory shape count: {layer.FeatureCount}",
            "GisLayerDXF parses supported DXF entities into an in-memory vector layer.",
            "Supported entities include POINT, TEXT, MTEXT, LINE, LWPOLYLINE, POLYLINE, CIRCLE and ARC.",
            $"Field count: {definitions.Count}", $"Extent: {layer.ProjectedExtent}", "", "File", FileLine(".dxf", path));
        schemaGrid.Rows.Clear();
        foreach (var definition in definitions) schemaGrid.Rows.Add(definition.Name, definition.Type, definition.Length, definition.DecimalCount);
        attributesGrid.Columns.Clear(); attributesGrid.Columns.Add("row", "#");
        foreach (var definition in definitions) attributesGrid.Columns.Add(definition.Name, definition.Name);
        attributesGrid.Rows.Clear();
        var rowCount = Math.Min(12, layer.FeatureCount);
        for (var row = 0; row < rowCount; ++row)
        {
            var attributes = viewerControl.GetLayerFeatureAttributes(index, row);
            attributesGrid.Rows.Add(new object[] { row }.Concat(definitions.Select(definition =>
                attributes.TryGetValue(definition.Name, out var value) ? value?.ToString() ?? string.Empty : string.Empty)).ToArray());
        }
        if (rowCount == 0) attributesGrid.Rows.Add("No attribute rows returned.");
        statusLabel.Text = $"GisLayerDXF opened {layer.FeatureCount} features and {definitions.Count} fields.";
    }
    private static string FileLine(string label, string path)
    { var info = new FileInfo(path); return $"{label}: {(info.Exists ? info.Length : 0)} bytes ({(info.Exists ? "exists" : "missing")})"; }
    private static GeoKernelLayerStyle DxfStyle() => new()
    { FillColor = "#D7E5DF", FillOpacity = 89, LineColor = "#2E6F91", LineWidth = 1.25, PointColor = "#D95D39", PointSize = 6.0 };
    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, value =>
    {
        statusLabel.Text = value.Message; downloadProgressBar.Visible = true;
        downloadProgressBar.Style = value.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (value.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(value.Percentage.Value, 0, 100);
    });
}
