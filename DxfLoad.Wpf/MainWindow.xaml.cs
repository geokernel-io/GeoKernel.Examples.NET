using System.Data;
using System.IO;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.DxfLoad.Wpf;

public sealed partial class MainWindow
{
    public MainWindow() => InitializeComponent();
    private void Window_Loaded(object? sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        var path = SampleData.EnsureKnownWpfSampleFile("geog_25000.dxf", this);
        if (string.IsNullOrEmpty(path)) { statusText.Text = "DXF sample data could not be prepared."; return; }
        try
        {
            if (!viewerControl.AddLayerFile(path)) throw new InvalidOperationException($"DXF could not be opened: {path}");
            viewerControl.SetLayerStyle(0, DxfStyle()); PopulateDetails(path); viewerControl.FullExtent();
        }
        catch (Exception ex)
        {
            statusText.Text = "DXF could not be opened.";
            MessageBox.Show(this, $"DXF could not be opened:{Environment.NewLine}{ex.Message}",
                "DxfLoad", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PopulateDetails(string path)
    {
        const int index = 0;
        var layer = viewerControl.GetLayerInfo(index) ?? throw new InvalidOperationException("Loaded layer metadata is unavailable.");
        var definitions = viewerControl.GetLayerAttributeDefinitions(index);
        detailsTextBox.Text = string.Join(Environment.NewLine,
            "DxfLoad sample", "", "API", "GisLayerDXF(path)", "layer.open()",
            "layer.attributeDefinitions()", "layer.attributesForRow(row)", "", "Loaded CAD DXF", path, "", "Layer",
            $"Name: {layer.Name}", $"Shape type: {layer.ShapeType}", $"Memory shape count: {layer.FeatureCount}",
            "GisLayerDXF parses supported DXF entities into an in-memory vector layer.",
            "Supported entities include POINT, TEXT, MTEXT, LINE, LWPOLYLINE, POLYLINE, CIRCLE and ARC.",
            $"Field count: {definitions.Count}", $"Extent: {layer.ProjectedExtent}", "", "File", FileLine(".dxf", path));
        schemaGrid.ItemsSource = definitions;
        var table = new DataTable(); table.Columns.Add("#");
        foreach (var definition in definitions) table.Columns.Add(definition.Name);
        for (var row = 0; row < Math.Min(12, layer.FeatureCount); ++row)
        {
            var attributes = viewerControl.GetLayerFeatureAttributes(index, row);
            var values = new object?[definitions.Count + 1]; values[0] = row;
            for (var column = 0; column < definitions.Count; ++column)
                values[column + 1] = attributes.TryGetValue(definitions[column].Name, out var value) ? value?.ToString() : null;
            table.Rows.Add(values);
        }
        if (table.Rows.Count == 0) table.Rows.Add("No attribute rows returned.");
        attributesGrid.ItemsSource = table.DefaultView;
        statusText.Text = $"GisLayerDXF opened {layer.FeatureCount} features and {definitions.Count} fields.";
    }
    private static string FileLine(string label, string path)
    { var info = new FileInfo(path); return $"{label}: {(info.Exists ? info.Length : 0)} bytes ({(info.Exists ? "exists" : "missing")})"; }
    private static GeoKernelLayerStyle DxfStyle() => new()
    { FillColor = "#D7E5DF", FillOpacity = 89, LineColor = "#2E6F91", LineWidth = 1.25, PointColor = "#D95D39", PointSize = 6.0 };
}
