using GeoKernel.NET.WinForms;

namespace GeoKernel.XyzCustomUrl.Winforms;

public sealed partial class MainForm : Form
{
    private const string DefaultUrl = "https://tile.openstreetmap.org/{z}/{x}/{y}.png";
    private static readonly GeoKernelExtent DefaultExtent = new(-1400000, 4100000, 4200000, 7800000);

    public MainForm() => InitializeComponent();

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        urlTextBox.Text = DefaultUrl;
        minZoomNumeric.Value = 0;
        maxZoomNumeric.Value = 19;
        localCacheCheckBox.Checked = true;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        ApplyCustomUrl();
    }

    private void zoomInButton_Click(object? s, EventArgs e) => viewerControl.ZoomIn();
    private void zoomOutButton_Click(object? s, EventArgs e) => viewerControl.ZoomOut();
    private void zoomRectButton_Click(object? s, EventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.ZoomBox;
    private void panButton_Click(object? s, EventArgs e) => viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
    private void fullExtentButton_Click(object? s, EventArgs e) => viewerControl.ViewExtent = DefaultExtent;
    private void applyButton_Click(object? s, EventArgs e) => ApplyCustomUrl();
    private void urlTextBox_KeyDown(object? s, KeyEventArgs e)
    {
        if (e.KeyCode != Keys.Enter) return;
        e.SuppressKeyPress = true;
        ApplyCustomUrl();
    }

    private void ApplyCustomUrl()
    {
        var url = urlTextBox.Text.Trim();
        if (!IsSupportedTileTemplate(url))
        {
            MessageBox.Show(this, "Tile URL template must include {z}, {x}, and {y}, or Bing-style {q}.", "XyzCustomUrl", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (minZoomNumeric.Value > maxZoomNumeric.Value)
        {
            MessageBox.Show(this, "Minimum zoom cannot be greater than maximum zoom.", "XyzCustomUrl", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            viewerControl.ClearLayers();
            var index = viewerControl.AddXyzLayer("Custom XYZ", url, minZoom: (int)minZoomNumeric.Value,
                maxZoom: (int)maxZoomNumeric.Value, tileSize: 256, localCacheEnabled: localCacheCheckBox.Checked);
            if (index < 0) throw new InvalidOperationException("Custom XYZ layer could not be loaded.");
            viewerControl.ViewExtent = DefaultExtent;
            detailsTextBox.Text = LayerDetails(url);
            statusLabel.Text = "Custom XYZ URL applied.";
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Custom XYZ layer failed.";
            MessageBox.Show(this, $"Custom XYZ layer could not be loaded:\n{ex.Message}", "XyzCustomUrl", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static bool IsSupportedTileTemplate(string url) =>
        (url.Contains("{z}") && url.Contains("{x}") && url.Contains("{y}")) || url.Contains("{q}");

    private string LayerDetails(string url) => string.Join(Environment.NewLine,
        "Custom XYZ URL sample", "", "Active URL template:", url, "",
        $"Min zoom: {(int)minZoomNumeric.Value}", $"Max zoom: {(int)maxZoomNumeric.Value}", "Tile size: 256",
        $"Local cache: {(localCacheCheckBox.Checked ? "enabled" : "disabled")}", "", "SDK flow:",
        "viewerControl.AddXyzLayer(name, urlTemplate, minZoom, maxZoom, tileSize, localCacheEnabled)", "",
        "Template requirements:", "- XYZ: {z}, {x}, {y}", "- or Bing style: {q}");
}
