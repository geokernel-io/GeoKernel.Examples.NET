using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ShapeClone.Wpf;

public partial class MainWindow
{
    private static readonly GeoKernelPoint[] SourceRing =
    [
        new(-4.0, -1.8), new(-0.4, -1.8), new(0.6, 0.0),
        new(-0.4, 1.8), new(-4.0, 1.8), new(-4.0, -1.8)
    ];

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        RenderScene();
        SetSampleExtent();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e) => SetSampleExtent();

    private void RenderScene()
    {
        // Materializing the managed point collection creates an independent deep clone.
        var clone = SourceRing.ToArray();
        for (var i = 0; i < clone.Length; i++)
            clone[i] = new GeoKernelPoint(clone[i].X + 5.0, clone[i].Y + 0.7);

        viewerControl.ClearLayers();
        viewerControl.AddPolygonLayer("Source shape", SourceRing, SourceStyle());
        viewerControl.AddPolygonLayer("Independent clone", clone, CloneStyle());

        var sourceUnchanged = SourceRing[0].X == -4.0 && SourceRing[0].Y == -1.8;
        detailsTextBox.Text = string.Join(Environment.NewLine,
            "Shape clone (.NET managed geometry)", "",
            $"Source vertices: {SourceRing.Length}", $"Clone vertices: {clone.Length}",
            $"ReferenceEquals(source, clone): {ReferenceEquals(SourceRing, clone).ToString().ToLowerInvariant()}",
            $"Source remains unchanged after moving clone: {sourceUnchanged.ToString().ToLowerInvariant()}", "",
            "Blue: original polygon", "Orange: independently cloned and translated polygon");
        statusText.Text = "Independent shape clone rendered.";
    }

    private void SetSampleExtent() => viewerControl.ViewExtent = new GeoKernelExtent(-5.2, -3.0, 6.2, 3.6);
    private static GeoKernelLayerStyle SourceStyle() => new() { FillColor="#BFD7EA", FillOpacity=120, LineColor="#1F6F8B", LineWidth=2.2 };
    private static GeoKernelLayerStyle CloneStyle() => new() { FillColor="#F6D6AD", FillOpacity=120, LineColor="#D95D39", LineWidth=2.2 };
}
