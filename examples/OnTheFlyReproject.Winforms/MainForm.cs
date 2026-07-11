using GeoKernel.NET.WinForms;

namespace GeoKernel.OnTheFlyReproject.Winforms;

public sealed partial class MainForm : Form
{
    private bool _worldLayerLoaded;

    public MainForm()
    {
        InitializeComponent();
        geoKernelViewerControl.MouseCoordinatesChanged += geoKernelViewerControl_MouseCoordinatesChanged;
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        LoadSpatialReferenceOptions();
        if (!LoadLayer())
            return;

        spatialReferenceComboBox.SelectedIndex = 1;
        _worldLayerLoaded = true;
        ApplySelectedSpatialReference();
    }

    private void LoadSpatialReferenceOptions()
    {
        spatialReferenceComboBox.DisplayMember = nameof(SpatialReferenceOption.Label);
        spatialReferenceComboBox.ValueMember = nameof(SpatialReferenceOption.Preset);
        spatialReferenceComboBox.Items.Clear();

        foreach (var option in SpatialReferenceOptions())
            spatialReferenceComboBox.Items.Add(option);
    }

    private bool LoadLayer()
    {
        var shapefilePath = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(shapefilePath))
        {
            MessageBox.Show(
                this,
                $"Shapefile could not be found:{Environment.NewLine}{shapefilePath}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                shapefilePath,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = WorldStyle()
                }))
        {
            MessageBox.Show(
                this,
                $"World layer could not be loaded:{Environment.NewLine}{shapefilePath}",
                Text,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        geoKernelViewerControl.SetLayerName(0, "World countries - source EPSG:4326");
        geoKernelViewerControl.SetLayerCoordinateSystemPreset(0, GeoKernelCoordinateSystemPreset.Wgs84);
        return true;
    }

    private void spatialReferenceComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_worldLayerLoaded)
            ApplySelectedSpatialReference();
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        if (SelectedOption() is { } option)
            geoKernelViewerControl.ViewExtent = option.Extent;
    }

    private void ApplySelectedSpatialReference()
    {
        if (SelectedOption() is not { } option)
            return;

        if (!geoKernelViewerControl.SetCoordinateSystemPreset(option.Preset))
        {
            statusLabel.Text = $"{option.ShortName} could not be applied.";
            return;
        }

        geoKernelViewerControl.ViewExtent = option.Extent;
        geoKernelViewerControl.RefreshLayers();
        statusLabel.Text = $"{option.ShortName}: world_4326.shp reprojected on the fly.";
    }

    private SpatialReferenceOption? SelectedOption()
    {
        return spatialReferenceComboBox.SelectedItem as SpatialReferenceOption;
    }

    private void geoKernelViewerControl_MouseCoordinatesChanged(object? sender, GeoKernelMouseCoordinatesChangedEventArgs e)
    {
        if (SelectedOption() is not { } option)
            return;

        statusLabel.Text =
            $"Screen: {e.ScreenPoint.X:0}, {e.ScreenPoint.Y:0} | {option.ShortName}: {e.WorldPoint.X.ToString($"F{option.CoordinateDecimals}")}, {e.WorldPoint.Y.ToString($"F{option.CoordinateDecimals}")}";
    }

    private static IReadOnlyList<SpatialReferenceOption> SpatialReferenceOptions()
    {
        const double webMercator = 20037508.342789244;
        return
        [
            new(
                "EPSG:4326 - WGS 84",
                "EPSG:4326",
                GeoKernelCoordinateSystemPreset.Wgs84,
                new GeoKernelExtent(-180.0, -85.0, 180.0, 85.0),
                6),
            new(
                "EPSG:3857 - WGS 84 / Web Mercator",
                "EPSG:3857",
                GeoKernelCoordinateSystemPreset.WebMercator,
                new GeoKernelExtent(-webMercator, -webMercator, webMercator, webMercator),
                2),
            new(
                "EPSG:3395 - WGS 84 / World Mercator",
                "EPSG:3395",
                GeoKernelCoordinateSystemPreset.WorldMercator,
                new GeoKernelExtent(-webMercator, -20000000.0, webMercator, 20000000.0),
                2),
            new(
                "World Miller Cylindrical",
                "Miller",
                GeoKernelCoordinateSystemPreset.Miller,
                new GeoKernelExtent(-webMercator, -15500000.0, webMercator, 15500000.0),
                2),
            new(
                "World Mollweide",
                "Mollweide",
                GeoKernelCoordinateSystemPreset.Mollweide,
                new GeoKernelExtent(-18500000.0, -9500000.0, 18500000.0, 9500000.0),
                2),
            new(
                "World Sinusoidal",
                "Sinusoidal",
                GeoKernelCoordinateSystemPreset.Sinusoidal,
                new GeoKernelExtent(-webMercator, -10500000.0, webMercator, 10500000.0),
                2),
            new(
                "World Eckert IV",
                "Eckert IV",
                GeoKernelCoordinateSystemPreset.EckertIV,
                new GeoKernelExtent(-18500000.0, -9500000.0, 18500000.0, 9500000.0),
                2),
            new(
                "World Eckert VI",
                "Eckert VI",
                GeoKernelCoordinateSystemPreset.EckertVI,
                new GeoKernelExtent(-18500000.0, -9500000.0, 18500000.0, 9500000.0),
                2)
        ];
    }

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 210,
            LineColor = "#6F8883",
            LineWidth = 0.75
        };
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "assets", "data")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }

    private sealed record SpatialReferenceOption(
        string Label,
        string ShortName,
        GeoKernelCoordinateSystemPreset Preset,
        GeoKernelExtent Extent,
        int CoordinateDecimals);
}
