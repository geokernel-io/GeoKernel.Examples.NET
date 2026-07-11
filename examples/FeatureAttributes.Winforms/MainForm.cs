using GeoKernel.NET.WinForms;

namespace GeoKernel.FeatureAttributes.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        ConfigureDetailsGrid();
        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!LoadSampleLayers())
            return;

        ShowEmptyHit();
        SetSampleExtent();
        UpdateStatus("Click a feature to show FeatureHitTestResult.Attributes with all field values.");
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
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "FeatureAttributes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "FeatureAttributes", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void hitTestButton_Click(object? sender, EventArgs e)
    {
        hitTestButton.Checked = true;
        panButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;
        toolStateLabel.Text = "API: FeatureHitTestResult.Attributes";
        UpdateStatus("Click a feature to read all attributes.");
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        panButton.Checked = true;
        hitTestButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        toolStateLabel.Text = "Tool: Pan";
        UpdateStatus("Pan mode.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_MouseClick(object? sender, MouseEventArgs e)
    {
        if (geoKernelViewerControl.ActiveTool != GeoKernelViewerTool.Info)
            return;

        var hit = geoKernelViewerControl.HitTestTopFeatureAt(e.X, e.Y, 8);
        if (hit is null || !hit.IsValid)
        {
            geoKernelViewerControl.ClearSelectedFeatures();
            ShowEmptyHit();
            UpdateStatus("No feature hit.");
            return;
        }

        ShowHit(hit);
        UpdateStatus($"Attributes loaded: {hit.LayerName} feature {hit.ShapeId}, fields={hit.Attributes.Count}.");
    }

    private void ConfigureDetailsGrid()
    {
        detailsGrid.Columns.Clear();
        detailsGrid.Columns.Add("Property", "Property / Field");
        detailsGrid.Columns.Add("Value", "Value");
    }

    private void ShowEmptyHit()
    {
        detailsGrid.Rows.Clear();
        detailsGrid.Rows.Add("Hit", "Click a feature to list every attribute field.");
    }

    private void ShowHit(GeoKernelFeatureHitTestResult hit)
    {
        detailsGrid.Rows.Clear();
        detailsGrid.Rows.Add("Layer", hit.LayerName);
        detailsGrid.Rows.Add("Layer index", hit.LayerIndex);
        detailsGrid.Rows.Add("Shape id", hit.ShapeId);
        detailsGrid.Rows.Add("Feature id", hit.FeatureId);
        detailsGrid.Rows.Add("Shape type", hit.ShapeType);
        detailsGrid.Rows.Add("Extent", ExtentText(hit.Extent));
        detailsGrid.Rows.Add("Attribute count", hit.Attributes.Count);

        foreach (var pair in hit.Attributes.OrderBy(pair => pair.Key, StringComparer.OrdinalIgnoreCase))
            detailsGrid.Rows.Add(pair.Key, pair.Value?.ToString() ?? "<null>");
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
        statusLabel.Text = text;
    }

    private static string ExtentText(GeoKernelExtent extent)
    {
        return $"({extent.XMin:F6}, {extent.YMin:F6}) - ({extent.XMax:F6}, {extent.YMax:F6})";
    }

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 210,
            LineColor = "#708984",
            LineWidth = 0.6,
            SelectedLineColor = "#F59E0B",
            SelectedLineWidth = 3.0
        };
    }

    private static GeoKernelLayerStyle StateStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#C7DEE7",
            FillOpacity = 160,
            LineColor = "#2D6F8E",
            LineWidth = 1.0,
            SelectedLineColor = "#F59E0B",
            SelectedLineWidth = 4.0
        };
    }

    private static GeoKernelLayerStyle CityStyle()
    {
        return new GeoKernelLayerStyle
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
}

