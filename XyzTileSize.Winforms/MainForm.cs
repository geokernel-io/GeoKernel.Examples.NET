using GeoKernel.NET.WinForms;

namespace GeoKernel.XyzTileSize.Winforms;

public sealed partial class MainForm : Form
{
    private const string UrlTemplate = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
    private static readonly GeoKernelExtent DefaultExtent = new(-1400000.0, 4100000.0, 4200000.0, 7800000.0);

    public MainForm() => InitializeComponent();

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        try
        {
            EqualizeViewerWidths();
            AddTileLayer(leftViewerControl, 256);
            AddTileLayer(rightViewerControl, 512);
            SetExtentForBoth();
            SetToolForBoth(GeoKernelViewerTool.Pan);
            detailsTextBox.Text = DetailsText();
            statusLabel.Text = "Compare AddXyzLayer tileSize 256 and 512.";
        }
        catch (Exception ex)
        {
            statusLabel.Text = "XyzTileSize failed.";
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void MainForm_Resize(object? sender, EventArgs e) => EqualizeViewerWidths();

    private void EqualizeViewerWidths()
    {
        if (viewerSplit.Width > viewerSplit.SplitterWidth)
            viewerSplit.SplitterDistance = (viewerSplit.Width - viewerSplit.SplitterWidth) / 2;
    }

    private static void AddTileLayer(GeoKernelViewerControl viewer, int tileSize)
    {
        viewer.ClearLayers();
        var cacheDirectory = Path.Combine(AppContext.BaseDirectory, "XyzTileSizeCache", tileSize.ToString());
        var index = viewer.AddXyzLayer($"OSM tileSize {tileSize}", UrlTemplate, 0, 19, tileSize,
            "OpenStreetMap contributors", true, cacheDirectory);
        if (index < 0)
            throw new InvalidOperationException($"XYZ layer with tileSize {tileSize} could not be added.");
    }

    private void zoomInButton_Click(object? sender, EventArgs e) { leftViewerControl.ZoomIn(); rightViewerControl.ZoomIn(); }
    private void zoomOutButton_Click(object? sender, EventArgs e) { leftViewerControl.ZoomOut(); rightViewerControl.ZoomOut(); }
    private void fullExtentButton_Click(object? sender, EventArgs e) => SetExtentForBoth();
    private void zoomRectButton_Click(object? sender, EventArgs e) => SetToolForBoth(GeoKernelViewerTool.ZoomBox);
    private void panButton_Click(object? sender, EventArgs e) => SetToolForBoth(GeoKernelViewerTool.Pan);

    private void SetExtentForBoth()
    {
        leftViewerControl.ViewExtent = DefaultExtent;
        rightViewerControl.ViewExtent = DefaultExtent;
    }

    private void SetToolForBoth(GeoKernelViewerTool tool)
    {
        leftViewerControl.ActiveTool = tool;
        rightViewerControl.ActiveTool = tool;
    }

    private static string DetailsText() => string.Join(Environment.NewLine,
        "XYZ tile size sample", "", "Left map:", "AddXyzLayer(..., tileSize: 256)", "",
        "Right map:", "AddXyzLayer(..., tileSize: 512)", "", "URL template:", UrlTemplate, "",
        "Why this matters:", "- tileSize is the expected pixel size of one downloaded tile.",
        "- Standard OSM tiles are usually 256 px.", "- Some services expose 512 px retina/high-DPI tiles.",
        "- The cache key includes tileSize, so 256 and 512 variants stay separate.", "", "SDK flow:",
        "viewer.AddXyzLayer(name, urlTemplate, 0, 19, tileSize, attribution, true, cacheDirectory);");
}
