using GeoKernel.NET.WinForms;

namespace GeoKernel.LabelCollisionOff.Winforms;

public sealed partial class MainForm : Form
{
    private static readonly GeoKernelExtent ContinentalUsExtent = new(-127.0, 23.0, -66.0, 50.0);
    public MainForm() => InitializeComponent();

    private void comparisonSplit_Resize(object? sender, EventArgs e)
    {
        var availableWidth = comparisonSplit.ClientSize.Width - comparisonSplit.SplitterWidth;
        if (availableWidth > 0)
            comparisonSplit.SplitterDistance = availableWidth / 2;
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        comparisonSplit_Resize(comparisonSplit, EventArgs.Empty);
        var progress = new ControlProgress<SampleDataProgress>(this, SetProgress);
        var worldPath = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, progress);
        var citiesPath = await SampleData.EnsureFileAsync("world_cities_4326.zip", "world_cities_4326", "world_cities_4326.shp", "World cities", this, progress);
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(worldPath) || string.IsNullOrEmpty(citiesPath)) return;

        if (!LoadComparisonLayers(collisionOnViewer, worldPath, citiesPath, false) || !LoadComparisonLayers(collisionOffViewer, worldPath, citiesPath, true))
        { MessageBox.Show(this, "Comparison layers could not be loaded.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error); return; }

        collisionOnViewer.ViewExtent = ContinentalUsExtent;
        collisionOffViewer.ViewExtent = ContinentalUsExtent;
        statusLabel.Text = "Left: collision filtering. Right: label overlap allowed.";
    }

    private static bool LoadComparisonLayers(GeoKernelViewerControl viewer, string worldPath, string citiesPath, bool allowOverlap)
    {
        viewer.ActiveTool = GeoKernelViewerTool.Pan;
        if (!viewer.AddLayerFile(worldPath)) return false;
        if (!viewer.AddLayerFile(citiesPath)) return false;
        var cities = viewer.GetLayerInfo(0); var world = viewer.GetLayerInfo(1);
        if (cities is null || world is null) return false;
        viewer.SetLayerName(world.Index, "World");
        viewer.SetLayerName(cities.Index, allowOverlap ? "Cities - labelAllowOverlap true" : "Cities - labelAllowOverlap false");
        viewer.SetLayerStyle(world.Index, WorldStyle());
        viewer.SetLayerStyle(cities.Index, CityStyle(allowOverlap));
        return true;
    }

    private static GeoKernelLayerStyle WorldStyle() => new() { FillColor = "#D8E5E1", FillOpacity = 215, LineColor = "#6F8380", LineWidth = 0.8 };
    private static GeoKernelLayerStyle CityStyle(bool allowOverlap) => new()
    {
        PointColor = "#D56037", LineColor = "#A23D23", PointSize = 5.5, LineWidth = 0.8,
        ShowLabels = true, LabelField = "CITY_NAME", LabelFontSize = 8.0, LabelColor = "#1F2933",
        LabelHaloEnabled = true, LabelHaloColor = "#FFFFFF", LabelHaloWidth = 1.5,
        LabelAllowOverlap = allowOverlap, LabelOffsetX = 7.0, LabelOffsetY = -7.0
    };

    private void SetProgress(SampleDataProgress p) { statusLabel.Text = p.Message; downloadProgressBar.Visible = true; downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee; if (p.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(p.Percentage.Value, 0, 100); }
}
