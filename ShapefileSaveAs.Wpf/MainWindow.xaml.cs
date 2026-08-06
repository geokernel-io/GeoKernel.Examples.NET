using System.Data;
using System.IO;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ShapefileSaveAs.Wpf;

public partial class MainWindow
{
    private static readonly string[] SidecarExtensions = [".shp", ".shx", ".dbf", ".prj", ".cpg", ".qix"];
    private string _sourcePath = string.Empty;
    private int _sourceLayerIndex = -1;
    private string OutputDirectory => Path.Combine(AppContext.BaseDirectory, "ShapefileSaveAsData");
    private string OutputPath => Path.Combine(OutputDirectory, "world_4326_copy.shp");

    public MainWindow() => InitializeComponent();

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            statusText.Text = "Preparing world shapefile...";
            saveProgressBar.IsIndeterminate = true;
            _sourcePath = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
            if (string.IsNullOrEmpty(_sourcePath) || !File.Exists(_sourcePath)) return;
            if (!viewerControl.AddLayerFile(_sourcePath))
                throw new InvalidOperationException("Source shapefile could not be opened.");

            _sourceLayerIndex = 0;
            viewerControl.SetLayerStyle(_sourceLayerIndex, WorldStyle());
            viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
            viewerControl.FullExtent();
            PopulateDetails(null);
            FillAttributeTable(_sourceLayerIndex);
            await RunSaveAsAsync();
        }
        catch (Exception ex)
        {
            saveProgressBar.IsIndeterminate = false;
            statusText.Text = "ShapefileSaveAs failed.";
            MessageBox.Show(this, ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void Save_Click(object sender, RoutedEventArgs e) => await RunSaveAsAsync();

    private async Task RunSaveAsAsync()
    {
        if (_sourceLayerIndex < 0) return;
        saveButton.IsEnabled = false;
        saveProgressBar.IsIndeterminate = true;
        statusText.Text = "Saving shapefile copy...";
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
            saveProgressBar.IsIndeterminate = false;
            saveProgressBar.Value = 100;
            statusText.Text = $"SaveAs wrote {OutputPath}";
        }
        catch (Exception ex)
        {
            PopulateDetails(null);
            saveProgressBar.IsIndeterminate = false;
            statusText.Text = "SaveAs failed.";
            MessageBox.Show(this, $"SaveAs failed:{Environment.NewLine}{ex.Message}", Title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { saveButton.IsEnabled = true; }
    }

    private void PopulateDetails(int? savedLayerIndex)
    {
        var sourceFields = viewerControl.GetLayerAttributeDefinitions(_sourceLayerIndex);
        var lines = new List<string>
        {
            "ShapefileSaveAs sample", "", "API", "viewerControl.AddLayerFile(sourcePath);",
            "viewerControl.SaveLayerAsShapefile(index, outputPath);", "", "Source shapefile", _sourcePath,
            $"Source fields: {sourceFields.Count}", $"Source feature count: {viewerControl.GetLayerFeatureCount(_sourceLayerIndex)}", "", "Output shapefile", OutputPath
        };
        foreach (var extension in SidecarExtensions)
        {
            var file = new FileInfo(Path.ChangeExtension(OutputPath, extension));
            lines.Add($"{extension}: {(file.Exists ? $"{file.Length} bytes" : "missing")}");
        }
        if (savedLayerIndex.HasValue)
        {
            var saved = viewerControl.GetLayerInfo(savedLayerIndex.Value);
            var fields = viewerControl.GetLayerAttributeDefinitions(savedLayerIndex.Value);
            lines.AddRange(["", "Reloaded output", $"Layer name: {saved?.Name}", $"Fields: {fields.Count}",
                $"Extent: {ExtentText(saved?.ProjectedExtent ?? default)}", $"Feature count: {viewerControl.GetLayerFeatureCount(savedLayerIndex.Value)}"]);
        }
        detailsTextBox.Text = string.Join(Environment.NewLine, lines);
    }

    private void FillAttributeTable(int layerIndex)
    {
        var fields = viewerControl.GetLayerAttributeDefinitions(layerIndex);
        var table = new DataTable();
        table.Columns.Add("#");
        foreach (var field in fields) table.Columns.Add(field.Name);
        var count = Math.Min(12, viewerControl.GetLayerFeatureCount(layerIndex));
        for (var rowIndex = 0; rowIndex < count; rowIndex++)
        {
            var attributes = viewerControl.GetLayerFeatureAttributes(layerIndex, rowIndex);
            var values = new object[fields.Count + 1]; values[0] = rowIndex;
            for (var column = 0; column < fields.Count; column++)
                values[column + 1] = attributes.TryGetValue(fields[column].Name, out var value) ? value?.ToString() ?? "" : "";
            table.Rows.Add(values);
        }
        attributesGrid.ItemsSource = table.DefaultView;
    }

    private void RemoveExistingOutput()
    {
        foreach (var extension in SidecarExtensions)
        {
            var path = Path.ChangeExtension(OutputPath, extension);
            if (File.Exists(path)) File.Delete(path);
        }
    }

    private static string ExtentText(GeoKernelExtent e) => $"({e.XMin:F2}, {e.YMin:F2}) - ({e.XMax:F2}, {e.YMax:F2})";
    private static GeoKernelLayerStyle WorldStyle() => new() { FillColor="#D7E5DF", LineColor="#6D8C86", LineWidth=1.0 };
}
