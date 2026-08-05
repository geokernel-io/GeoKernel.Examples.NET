using GeoKernel.NET.WinForms;

namespace GeoKernel.DxfLoad.Winforms;

public sealed partial class MainForm : Form
{
    private string _samplePath = string.Empty;
    private static readonly string SampleName = "DxfLoad";
    private static readonly string SampleKind = "file";
    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        _samplePath = await SampleData.EnsureFileAsync("geog_25000_dxf.zip", "geog_25000_dxf", "geog_25000.dxf", "DXF sample", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(_samplePath))
            return;
        LoadSample();
    }

    private void primaryButton_Click(object? sender, EventArgs e) => LoadSample();
    private void secondaryButton_Click(object? sender, EventArgs e) => viewerControl.FullExtent();

    private void LoadSample()
    {
        viewerControl.ClearLayers();
        var details = new List<string> { "DxfLoad sample", "", "API", ApiText(), "" };
        try
        {
            RunSample(details);
            details.Add("");
            details.Add("Layers");
            foreach (var layer in viewerControl.GetLayersInfo())
                details.Add($"#{layer.Index}: {layer.Name} | features: {layer.FeatureCount} | type: {layer.ShapeType}");
            detailsTextBox.Text = string.Join(Environment.NewLine, details);
            statusLabel.Text = "DxfLoad loaded.";
        }
        catch (Exception ex)
        {
            details.Add(ex.Message);
            detailsTextBox.Text = string.Join(Environment.NewLine, details);
            statusLabel.Text = "DxfLoad failed.";
            MessageBox.Show(this, ex.Message, "DxfLoad", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void RunSample(List<string> details)
    {
        switch (SampleKind)
        {
            case "osm":
                viewerControl.AddOpenStreetMapLayer();
                viewerControl.ViewExtent = EuropeExtent3857();
                details.Add("viewer.AddOpenStreetMapLayer()");
                break;
            case "xyz":
                AddXyz(details);
                break;
            case "rasterOverview":
                AddRaster(details, new GeoKernelLayerLoadOptions { PrepareRasterOverviews = true, RasterOverviewMinimumPixels = 0 });
                break;
            case "rasterTileCache":
                AddRaster(details, new GeoKernelLayerLoadOptions { RasterTileCacheEnabled = true, RasterTileCachePixelBudget = 64 * 1024 * 1024 });
                break;
            case "labelCollision":
                AddLabelCollision(details);
                break;
            default:
                AddFile(details, "geog_25000.dxf", null);
                break;
        }
    }

    private static string ApiText() => SampleKind switch
    {
        "osm" => "AddOpenStreetMapLayer()",
        "xyz" => "AddXyzLayer(name, urlTemplate, minZoom, maxZoom, tileSize, attribution, localCacheEnabled)",
        "rasterOverview" => "AddLayerFile(path, new GeoKernelLayerLoadOptions { PrepareRasterOverviews = true })",
        "rasterTileCache" => "AddLayerFile(path, new GeoKernelLayerLoadOptions { RasterTileCacheEnabled = true })",
        "labelCollision" => "SetLayerStyle(index, new GeoKernelLayerStyle { LabelAllowOverlap = ... })",
        _ => "AddLayerFile(path); GetLayerInfo(index); GetLayerAttributeDefinitions(index)"
    };

    private void AddXyz(List<string> details)
    {
        var index = SampleName switch
        {
            "XyzCustomUrl" => viewerControl.AddXyzLayer("Custom OSM", "https://tile.openstreetmap.org/{z}/{x}/{y}.png", attribution: "OpenStreetMap contributors"),
            "XyzLocalCache" => viewerControl.AddXyzLayer("OSM cached", "https://tile.openstreetmap.org/{z}/{x}/{y}.png", attribution: "OpenStreetMap contributors", localCacheEnabled: true, cacheDirectory: Path.Combine(AppContext.BaseDirectory, "XyzLocalCache")),
            "XyzTileSize" => viewerControl.AddXyzLayer("OSM 512 tile request", "https://tile.openstreetmap.org/{z}/{x}/{y}.png", tileSize: 512, attribution: "OpenStreetMap contributors"),
            "XyzMinMaxZoom" => viewerControl.AddXyzLayer("OSM min/max zoom", "https://tile.openstreetmap.org/{z}/{x}/{y}.png", minZoom: 2, maxZoom: 12, attribution: "OpenStreetMap contributors"),
            "XyzAttribution" => viewerControl.AddXyzLayer("OSM attribution", "https://tile.openstreetmap.org/{z}/{x}/{y}.png", attribution: "OpenStreetMap contributors"),
            "XyzDiagnostics" => viewerControl.AddXyzLayer("OSM diagnostics", "https://tile.openstreetmap.org/{z}/{x}/{y}.png", attribution: "OpenStreetMap contributors"),
            _ => viewerControl.AddXyzLayer("OSM preset", "https://tile.openstreetmap.org/{z}/{x}/{y}.png", attribution: "OpenStreetMap contributors")
        };
        if (index < 0)
            throw new InvalidOperationException("XYZ layer could not be added.");
        viewerControl.ViewExtent = EuropeExtent3857();
        details.Add("XYZ layer index: " + index);
        if (SampleName == "XyzDiagnostics")
        {
            details.Add("");
            details.Add("Render backend diagnostics");
            details.Add(viewerControl.RenderBackendDiagnostics);
        }
    }

    private void AddRaster(List<string> details, GeoKernelLayerLoadOptions options)
    {
        AddFile(details, "world_8km.tif", options);
        details.Add("");
        details.Add("Raster options");
        details.Add($"PrepareRasterOverviews: {options.PrepareRasterOverviews}");
        details.Add($"RasterTileCacheEnabled: {options.RasterTileCacheEnabled}");
        details.Add($"RasterTileCachePixelBudget: {options.RasterTileCachePixelBudget}");
    }

    private void AddLabelCollision(List<string> details)
    {
        AddFile(details, "world_4326.shp", null);
        AddFile(details, "cities_4326.shp", null, zoom: false);
        var style = new GeoKernelLayerStyle
        {
            PointColor = "#2E86AB",
            PointSize = 4,
            ShowLabels = true,
            LabelField = "CITY_NAME",
            LabelColor = "#1F2933",
            LabelFontSize = 8,
            LabelHaloEnabled = true,
            LabelHaloColor = "#FFFFFF",
            LabelHaloWidth = 1.5,
            LabelAllowOverlap = true
        };
        viewerControl.SetLayerStyle(1, style);
        viewerControl.ViewExtent = new GeoKernelExtent(-1500000, 3500000, 5200000, 8200000);
        details.Add("Cities labels use labelAllowOverlap = true.");
    }

    private void AddFile(List<string> details, string relativePath, GeoKernelLayerLoadOptions? options, bool zoom = true)
    {
        var path = DataPath(relativePath);
        if (!File.Exists(path))
            throw new FileNotFoundException("Sample data file could not be found.", path);
        var ok = options is null ? viewerControl.AddLayerFile(path) : viewerControl.AddLayerFile(path, options);
        if (!ok)
            throw new InvalidOperationException($"Layer could not be loaded: {path}");
        if (zoom)
            viewerControl.FullExtent();
        details.Add("Loaded: " + path);
    }

    private static GeoKernelExtent EuropeExtent3857() => new(-1400000.0, 4100000.0, 4200000.0, 7800000.0);
    private string DataPath(string relativePath) => relativePath.Equals(Path.GetFileName(_samplePath), StringComparison.OrdinalIgnoreCase) ? _samplePath : relativePath;
    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, value =>
    {
        statusLabel.Text = value.Message;
        downloadProgressBar.Visible = true;
        downloadProgressBar.Style = value.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (value.Percentage.HasValue)
            downloadProgressBar.Value = Math.Clamp(value.Percentage.Value, 0, 100);
    });

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
}
