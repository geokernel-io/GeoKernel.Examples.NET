using GeoKernel.NET.WinForms;

namespace GeoKernel.TabLoad.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        var path = await SampleData.EnsureFileAsync("paris_tab.zip", "paris_tab", "paris.tab",
            "Paris TAB", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(path))
        {
            statusLabel.Text = "MapInfo TAB sample data could not be prepared.";
            return;
        }

        try
        {
            if (!viewerControl.AddLayerFile(path))
                throw new InvalidOperationException($"MapInfo TAB could not be opened: {path}");
            viewerControl.SetLayerStyle(0, TabStyle());
            PopulateDetails(path);
            viewerControl.FullExtent();
        }
        catch (Exception ex)
        {
            statusLabel.Text = "MapInfo TAB could not be opened.";
            MessageBox.Show(this, $"MapInfo TAB could not be opened:{Environment.NewLine}{ex.Message}",
                "TabLoad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void PopulateDetails(string path)
    {
        const int index = 0;
        var layer = viewerControl.GetLayerInfo(index)
            ?? throw new InvalidOperationException("Loaded layer metadata is unavailable.");
        var definitions = viewerControl.GetLayerAttributeDefinitions(index);
        var stem = Path.Combine(Path.GetDirectoryName(path)!, Path.GetFileNameWithoutExtension(path));
        detailsTextBox.Text = string.Join(Environment.NewLine,
            "TabLoad sample", "", "API", "GisLayerTAB(path)", "layer.open()",
            "layer.attributeDefinitions()", "layer.attributesForRow(row)", "",
            "Loaded MapInfo TAB", path, "", "Layer", $"Name: {layer.Name}",
            $"Shape type: {layer.ShapeType}", $"Memory shape count: {layer.FeatureCount}",
            "Provider-backed TAB files can render features while memory shape count remains 0.",
            $"Field count: {definitions.Count}", $"Extent: {layer.ProjectedExtent}", "",
            "Sidecars", SidecarLine(".tab", path), SidecarLine(".dat", stem + ".dat"),
            SidecarLine(".map", stem + ".map"), SidecarLine(".id", stem + ".id"),
            SidecarLine(".dbf", stem + ".dbf"));

        schemaGrid.Rows.Clear();
        foreach (var definition in definitions)
            schemaGrid.Rows.Add(definition.Name, definition.Type, definition.Length, definition.DecimalCount);

        attributesGrid.Columns.Clear();
        attributesGrid.Columns.Add("row", "#");
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
        statusLabel.Text = $"GisLayerTAB opened {layer.FeatureCount} memory features and {definitions.Count} fields.";
    }

    private static string SidecarLine(string label, string path)
    {
        var info = new FileInfo(path);
        return $"{label}: {(info.Exists ? info.Length : 0)} bytes ({(info.Exists ? "exists" : "missing")})";
    }

    private static GeoKernelLayerStyle TabStyle() => new()
    {
        FillColor = "#D7E5DF", FillOpacity = 184, LineColor = "#6D8C86", LineWidth = 1.1,
        PointColor = "#D95D39", PointSize = 7.0
    };

    private IProgress<SampleDataProgress> CreateSampleProgress() =>
        new ControlProgress<SampleDataProgress>(this, value =>
        {
            statusLabel.Text = value.Message;
            downloadProgressBar.Visible = true;
            downloadProgressBar.Style = value.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
            if (value.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(value.Percentage.Value, 0, 100);
        });
}
