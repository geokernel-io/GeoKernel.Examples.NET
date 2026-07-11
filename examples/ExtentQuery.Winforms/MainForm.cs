using GeoKernel.NET.WinForms;

namespace GeoKernel.ExtentQuery.Winforms;

public sealed partial class MainForm : Form
{
    private readonly List<GeoKernelFeatureHitTestResult> _hits = [];
    private bool _updatingHits;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        ConfigureGrids();

        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadSampleLayers())
            return;

        ShowEmptyHits();
        SetSampleExtent();
        UpdateStatus($"Ready. Query extent: {ExtentText(QueryExtent())}.");
    }

    private bool LoadSampleLayers()
    {
        return AddLayer("world_4326.shp", "World", WorldStyle())
            && AddLayer("usa_states_4326.shp", "USA States", StateStyle())
            && AddLayer("cities_4326.shp", "Cities", CityStyle());
    }

    private bool AddLayer(string fileName, string displayName, GeoKernelLayerStyle style)
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", fileName);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "ExtentQuery", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "ExtentQuery", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void selectButton_Click(object? sender, EventArgs e)
    {
        RunQuery();
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        panButton.Checked = true;
        selectButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan mode.");
    }

    private void clearSelectionButton_Click(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ClearSelectedFeatures();
        ShowEmptyHits();
        UpdateStatus("Result list cleared.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void RunQuery()
    {
        var extent = QueryExtent();
        var hits = geoKernelViewerControl.HitTestFeaturesInExtent(extent)
            .Where(hit => hit.IsValid)
            .ToList();

        ShowHits(hits);
        if (hits.Count == 0)
        {
            ShowEmptyAttributes("No features intersect the query extent.");
            UpdateStatus($"No features in extent {ExtentText(extent)}.");
            return;
        }

        hitsGrid.Rows[0].Selected = true;
        ShowAttributes(hits[0]);
        UpdateStatus($"{hits.Count} feature hit(s), extent {ExtentText(extent)}.");
    }

    private void hitsGrid_SelectionChanged(object? sender, EventArgs e)
    {
        if (_updatingHits || hitsGrid.CurrentRow is null)
            return;

        var index = hitsGrid.CurrentRow.Index;
        if (index < 0 || index >= _hits.Count)
            return;

        ShowAttributes(_hits[index]);
        UpdateStatus($"Selected row {index + 1}/{_hits.Count}: {_hits[index].LayerName} feature {_hits[index].ShapeId}.");
    }

    private void ConfigureGrids()
    {
        hitsGrid.Columns.Clear();
        hitsGrid.Columns.Add("Number", "#");
        hitsGrid.Columns.Add("Layer", "Layer");
        hitsGrid.Columns.Add("ShapeId", "Shape id");
        hitsGrid.Columns.Add("FeatureId", "Feature id");
        hitsGrid.Columns.Add("ShapeType", "Type");

        attributesGrid.Columns.Clear();
        attributesGrid.Columns.Add("Property", "Property / Field");
        attributesGrid.Columns.Add("Value", "Value");
    }

    private void ShowEmptyHits()
    {
        _hits.Clear();
        hitsGrid.Rows.Clear();
        hitsGrid.Rows.Add("-", "No hits", "-", "-", "-");
        ShowEmptyAttributes("Click Run Extent Query to list features intersecting the query extent.");
    }

    private void ShowHits(IReadOnlyList<GeoKernelFeatureHitTestResult> hits)
    {
        _updatingHits = true;
        try
        {
            _hits.Clear();
            _hits.AddRange(hits);
            hitsGrid.Rows.Clear();

            for (var i = 0; i < _hits.Count; i++)
            {
                var hit = _hits[i];
                hitsGrid.Rows.Add(i + 1, hit.LayerName, hit.ShapeId, hit.FeatureId, hit.ShapeType);
            }

            if (_hits.Count == 0)
                hitsGrid.Rows.Add("-", "No hits", "-", "-", "-");
        }
        finally
        {
            _updatingHits = false;
        }
    }

    private void ShowEmptyAttributes(string text)
    {
        attributesGrid.Rows.Clear();
        attributesGrid.Rows.Add("Hit", text);
    }

    private void ShowAttributes(GeoKernelFeatureHitTestResult hit)
    {
        attributesGrid.Rows.Clear();
        attributesGrid.Rows.Add("Layer", hit.LayerName);
        attributesGrid.Rows.Add("Layer index", hit.LayerIndex);
        attributesGrid.Rows.Add("Shape id", hit.ShapeId);
        attributesGrid.Rows.Add("Feature id", hit.FeatureId);
        attributesGrid.Rows.Add("Shape type", hit.ShapeType);
        attributesGrid.Rows.Add("Extent", ExtentText(hit.Extent));

        foreach (var pair in hit.Attributes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            attributesGrid.Rows.Add(pair.Key, pair.Value?.ToString() ?? "<null>");
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
        statusLabel.Text = text;
    }

    private static GeoKernelExtent QueryExtent() => new(-125.0, 32.0, -113.0, 42.5);

    private static string ExtentText(GeoKernelExtent extent)
    {
        return $"({extent.XMin:F6}, {extent.YMin:F6}) - ({extent.XMax:F6}, {extent.YMax:F6})";
    }

    private static GeoKernelLayerStyle WorldStyle() => new()
    {
        FillColor = "#D8E5E1",
        FillOpacity = 210,
        LineColor = "#708984",
        LineWidth = 0.6,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 3.0
    };

    private static GeoKernelLayerStyle StateStyle() => new()
    {
        FillColor = "#C7DEE7",
        FillOpacity = 155,
        LineColor = "#2D6F8E",
        LineWidth = 1.0,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 4.0
    };

    private static GeoKernelLayerStyle CityStyle() => new()
    {
        PointColor = "#D95D39",
        LineColor = "#8C321D",
        PointSize = 8.0,
        LineWidth = 1.0,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 4.0,
        ShowLabels = true,
        LabelField = "NAME",
        LabelFontSize = 9.0,
        LabelColor = "#263238",
        LabelHaloEnabled = true,
        LabelHaloColor = "#FFFFFF",
        LabelHaloWidth = 2.0
    };

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

