using GeoKernel.NET.WinForms;

namespace GeoKernel.ShapeClone.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelPoint[] SourceRing =
    [
        new(-4.0, -1.8),
        new(-0.4, -1.8),
        new(0.6, 0.0),
        new(-0.4, 1.8),
        new(-4.0, 1.8),
        new(-4.0, -1.8)
    ];

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        RenderScene();
        SetSampleExtent();
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void RenderScene()
    {
        // GeoKernel's managed polygon representation is an independent point collection.
        // Materializing it creates the deep clone used by the .NET API surface.
        var clone = SourceRing.ToArray();
        for (var index = 0; index < clone.Length; index++)
            clone[index] = new GeoKernelPoint(clone[index].X + 5.0, clone[index].Y + 0.7);

        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.AddPolygonLayer("Source shape", SourceRing, SourceStyle());
        geoKernelViewerControl.AddPolygonLayer("Independent clone", clone, CloneStyle());

        var sourceUnchanged = SourceRing[0].X == -4.0 && SourceRing[0].Y == -1.8;
        detailsTextBox.Text =
            $"Shape clone (.NET managed geometry){Environment.NewLine}{Environment.NewLine}" +
            $"Source vertices: {SourceRing.Length}{Environment.NewLine}" +
            $"Clone vertices: {clone.Length}{Environment.NewLine}" +
            $"ReferenceEquals(source, clone): {ReferenceEquals(SourceRing, clone).ToString().ToLowerInvariant()}{Environment.NewLine}" +
            $"Source remains unchanged after moving clone: {sourceUnchanged.ToString().ToLowerInvariant()}{Environment.NewLine}{Environment.NewLine}" +
            $"Blue: original polygon{Environment.NewLine}" +
            $"Orange: independently cloned and translated polygon";

        statusLabel.Text = "Independent shape clone rendered.";
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-5.2, -3.0, 6.2, 3.6);
    }

    private static GeoKernelLayerStyle SourceStyle() => new()
    {
        FillColor = "#BFD7EA",
        FillOpacity = 120,
        LineColor = "#1F6F8B",
        LineWidth = 2.2
    };

    private static GeoKernelLayerStyle CloneStyle() => new()
    {
        FillColor = "#F6D6AD",
        FillOpacity = 120,
        LineColor = "#D95D39",
        LineWidth = 2.2
    };
}
