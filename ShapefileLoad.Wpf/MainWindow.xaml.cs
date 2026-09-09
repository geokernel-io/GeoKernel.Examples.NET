using System.Data;
using System.IO;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ShapefileLoad.Wpf;

public sealed partial class MainWindow
{
    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        var path = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
        if (string.IsNullOrEmpty(path))
        {
            statusText.Text = "Shapefile sample data could not be prepared.";
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
                "ShapefileLoad", MessageBoxButton.OK, MessageBoxImage.Error);
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
            "ShapefileLoad sample", "", "API", "GisLayerSHP(path)", "layer.open()",
            "layer.attributeDefinitions()", "layer.attributesForRow(row)", "",
            "Loaded shapefile", path, "", "Layer", $"Name: {layer.Name}",
            $"Shape type: {layer.ShapeType}", $"Memory shape count: {layer.FeatureCount}",
            "Provider-backed shapefiles can render features while memory shape count remains 0.",
            $"Field count: {definitions.Count}", $"Extent: {layer.ProjectedExtent}", "",
            "Sidecars", $".shp: {FileSize(path)} bytes", $".shx: {FileSize(stem + ".shx")} bytes",
            $".dbf: {FileSize(stem + ".dbf")} bytes");
        schemaGrid.ItemsSource = definitions;

        var table = new DataTable();
        table.Columns.Add("#");
        foreach (var definition in definitions) table.Columns.Add(definition.Name);
        for (var row = 0; row < Math.Min(12, layer.FeatureCount); ++row)
        {
            var attributes = viewerControl.GetLayerFeatureAttributes(index, row);
            var values = new object?[definitions.Count + 1];
            values[0] = row;
            for (var column = 0; column < definitions.Count; ++column)
                values[column + 1] = attributes.TryGetValue(definitions[column].Name, out var value)
                    ? value?.ToString() : null;
            table.Rows.Add(values);
        }
        if (table.Rows.Count == 0) table.Rows.Add("No attribute rows returned.");
        attributesGrid.ItemsSource = table.DefaultView;
        statusText.Text = $"GisLayerSHP opened {layer.FeatureCount} features and {definitions.Count} fields.";
    }

    private static long FileSize(string path) => File.Exists(path) ? new FileInfo(path).Length : 0;
    private static GeoKernelLayerStyle WorldStyle() => new()
        { FillColor = "#D7E5DF", LineColor = "#6D8C86", LineWidth = 1.0 };
}
