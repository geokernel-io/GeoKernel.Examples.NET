using GeoKernel.NET.WinForms;

namespace GeoKernel.ShapefileSaveAs.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly string[] SidecarExtensions = [".shp", ".shx", ".dbf", ".prj", ".cpg", ".qix"];
    private string _sourcePath = string.Empty;
    private int _sourceLayerIndex = -1;

    public MainForm() => InitializeComponent();

    private string OutputDirectory => Path.Combine(AppContext.BaseDirectory, "ShapefileSaveAsData");
    private string OutputPath => Path.Combine(OutputDirectory, "world_4326_copy.shp");

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        _sourcePath = await SampleData.EnsureFileAsync(
            "world_4326.zip", "world_4326", "world_4326.shp", "World shapefile", this,
            new ControlProgress<SampleDataProgress>(this, progress =>
            {
                statusLabel.Text = progress.Message;
                saveProgressBar.Style = progress.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
                if (progress.Percentage.HasValue)
                    saveProgressBar.Value = Math.Clamp(progress.Percentage.Value, 0, 100);
            }));

        saveProgressBar.Style = ProgressBarStyle.Blocks;
        saveProgressBar.Value = 0;
        if (string.IsNullOrEmpty(_sourcePath))
            return;

        if (!viewerControl.AddLayerFile(_sourcePath))
        {
            MessageBox.Show(this, "Source shapefile could not be opened.", "ShapefileSaveAs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        _sourceLayerIndex = 0;
        viewerControl.SetLayerStyle(_sourceLayerIndex, WorldStyle());
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.FullExtent();
        PopulateDetails(savedLayerIndex: null);
        FillAttributeTable(_sourceLayerIndex);
        await RunSaveAsAsync();
    }

    private async void saveButton_Click(object? sender, EventArgs e) => await RunSaveAsAsync();

    private async Task RunSaveAsAsync()
    {
        if (_sourceLayerIndex < 0)
            return;

        saveButton.Enabled = false;
        saveProgressBar.Style = ProgressBarStyle.Marquee;
        saveProgressBar.MarqueeAnimationSpeed = 25;
        statusLabel.Text = "Saving shapefile copy...";
        try
        {
            await Task.Yield();
            Directory.CreateDirectory(OutputDirectory);
            RemoveExistingOutput();

            if (!viewerControl.SaveLayerAsShapefile(_sourceLayerIndex, OutputPath))
                throw new InvalidOperationException("SaveLayerAsShapefile returned false.");

            if (!viewerControl.AddLayerFile(OutputPath))
                throw new InvalidOperationException("Saved shapefile could not be reloaded.");

            const int savedLayerIndex = 0;
            PopulateDetails(savedLayerIndex);
            FillAttributeTable(savedLayerIndex);
            viewerControl.RemoveLayer(savedLayerIndex);
            _sourceLayerIndex = 0;
            saveProgressBar.Style = ProgressBarStyle.Blocks;
            saveProgressBar.Value = 100;
            statusLabel.Text = $"SaveAs wrote {OutputPath}";
        }
        catch (Exception ex)
        {
            PopulateDetails(savedLayerIndex: null);
            saveProgressBar.Style = ProgressBarStyle.Blocks;
            MessageBox.Show(this, $"SaveAs failed:{Environment.NewLine}{ex.Message}", "ShapefileSaveAs", MessageBoxButtons.OK, MessageBoxIcon.Error);
            statusLabel.Text = "SaveAs failed.";
        }
        finally
        {
            saveProgressBar.MarqueeAnimationSpeed = 0;
            saveButton.Enabled = true;
        }
    }

    private void PopulateDetails(int? savedLayerIndex)
    {
        var source = viewerControl.GetLayerInfo(_sourceLayerIndex);
        var sourceFields = viewerControl.GetLayerAttributeDefinitions(_sourceLayerIndex);
        var lines = new List<string>
        {
            "ShapefileSaveAs sample", "", "API",
            "viewerControl.AddLayerFile(sourcePath);",
            "viewerControl.SaveLayerAsShapefile(index, outputPath);", "",
            "Source shapefile", _sourcePath,
            $"Source fields: {sourceFields.Count}",
            $"Source feature count: {viewerControl.GetLayerFeatureCount(_sourceLayerIndex)}", "",
            "Output shapefile", OutputPath
        };

        foreach (var extension in SidecarExtensions)
        {
            var path = Path.ChangeExtension(OutputPath, extension);
            var file = new FileInfo(path);
            lines.Add($"{extension}: {(file.Exists ? $"{file.Length} bytes" : "missing")}");
        }

        if (savedLayerIndex.HasValue)
        {
            var saved = viewerControl.GetLayerInfo(savedLayerIndex.Value);
            var fields = viewerControl.GetLayerAttributeDefinitions(savedLayerIndex.Value);
            lines.AddRange(["", "Reloaded output",
                $"Layer name: {saved?.Name}",
                $"Fields: {fields.Count}",
                $"Extent: {ExtentText(saved?.ProjectedExtent ?? default)}",
                $"Feature count: {viewerControl.GetLayerFeatureCount(savedLayerIndex.Value)}"]);
        }

        detailsTextBox.Text = string.Join(Environment.NewLine, lines);
    }

    private void FillAttributeTable(int layerIndex)
    {
        var fields = viewerControl.GetLayerAttributeDefinitions(layerIndex);
        attributesGrid.Columns.Clear();
        attributesGrid.Rows.Clear();
        attributesGrid.Columns.Add("Row", "#");
        foreach (var field in fields)
            attributesGrid.Columns.Add(field.Name, field.Name);

        var count = Math.Min(12, viewerControl.GetLayerFeatureCount(layerIndex));
        for (var rowIndex = 0; rowIndex < count; rowIndex++)
        {
            var attributes = viewerControl.GetLayerFeatureAttributes(layerIndex, rowIndex);
            var values = new object[fields.Count + 1];
            values[0] = rowIndex;
            for (var column = 0; column < fields.Count; column++)
                values[column + 1] = attributes.TryGetValue(fields[column].Name, out var value) ? ValueText(value) : string.Empty;
            attributesGrid.Rows.Add(values);
        }
    }

    private void RemoveExistingOutput()
    {
        foreach (var extension in SidecarExtensions)
        {
            var path = Path.ChangeExtension(OutputPath, extension);
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    private static string ValueText(object? value) => value?.ToString() ?? string.Empty;
    private static string ExtentText(GeoKernelExtent extent) =>
        $"({extent.XMin:F2}, {extent.YMin:F2}) - ({extent.XMax:F2}, {extent.YMax:F2})";

    private static GeoKernelLayerStyle WorldStyle() => new()
    {
        FillColor = "#D7E5DF",
        LineColor = "#6D8C86",
        LineWidth = 1.0
    };
}
