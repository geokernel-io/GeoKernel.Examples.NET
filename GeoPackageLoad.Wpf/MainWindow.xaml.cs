using System.Data;
using System.IO;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GeoPackageLoad.Wpf;

public sealed partial class MainWindow
{
    public MainWindow() => InitializeComponent();
    private void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        var path = SampleData.EnsureKnownWpfSampleFile("europe_detailed.gpkg", this);
        if (string.IsNullOrEmpty(path))
        {
            statusText.Text = "GeoPackage sample data could not be prepared."; return;
        }
        try
        {
            if (!viewerControl.AddLayerFile(path))
                throw new InvalidOperationException($"GeoPackage could not be opened: {path}");
            viewerControl.SetLayerStyle(0, GeoPackageStyle()); PopulateDetails(path); viewerControl.FullExtent();
        }
        catch (Exception ex)
        {
            statusText.Text = "GeoPackage could not be opened.";
            MessageBox.Show(this, $"GeoPackage could not be opened:{Environment.NewLine}{ex.Message}",
                "GeoPackageLoad", MessageBoxButton.OK, MessageBoxImage.Error);
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
        schemaGrid.ItemsSource = definitions;
        var table = new DataTable(); table.Columns.Add("#");
        foreach (var definition in definitions) table.Columns.Add(definition.Name);
        for (var row = 0; row < Math.Min(12, layer.FeatureCount); ++row)
        {
            var attributes = viewerControl.GetLayerFeatureAttributes(index, row);
            var values = new object?[definitions.Count + 1]; values[0] = row;
            for (var column = 0; column < definitions.Count; ++column)
                values[column + 1] = attributes.TryGetValue(definitions[column].Name, out var value)
                    ? value?.ToString() : null;
            table.Rows.Add(values);
        }
        if (table.Rows.Count == 0) table.Rows.Add("No attribute rows returned.");
        attributesGrid.ItemsSource = table.DefaultView;
        statusText.Text = $"GisLayerGPKG opened {layer.FeatureCount} memory features and {definitions.Count} fields.";
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
}
