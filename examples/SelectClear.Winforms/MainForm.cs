using GeoKernel.NET.WinForms;

namespace GeoKernel.SelectClear.Winforms;

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

        RefreshSelectedFeatures("No selected features.");
        SetSampleExtent();
        UpdateStatus("Click a feature to add it to selection.");
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
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "SelectClear", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "SelectClear", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void addButton_Click(object? sender, EventArgs e)
    {
        addButton.Checked = true;
        panButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;
        toolStateLabel.Text = "Click = addSelectedFeature";
        UpdateStatus("Click a feature to add it to selection.");
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        addButton.Checked = false;
        panButton.Checked = true;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        toolStateLabel.Text = "Tool: Pan";
        UpdateStatus("Pan mode.");
    }

    private void clearButton_Click(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ClearSelectedFeatures();
        RefreshSelectedFeatures("Selection cleared.");
        UpdateStatus($"clearSelectedFeatures applied. selectedFeatureCount={geoKernelViewerControl.SelectedFeatureCount}.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_MouseClick(object? sender, MouseEventArgs e)
    {
        if (geoKernelViewerControl.ActiveTool != GeoKernelViewerTool.Info)
            return;

        var ok = geoKernelViewerControl.AddTopFeatureToSelectionAt(e.X, e.Y, 8);

        if (!ok)
        {
            UpdateStatus("No feature hit.");
            return;
        }

        RefreshSelectedFeatures();
        UpdateStatus($"addSelectedFeature applied. selectedFeatureCount={geoKernelViewerControl.SelectedFeatureCount}.");
    }

    private void ConfigureDetailsGrid()
    {
        detailsGrid.Columns.Clear();
        detailsGrid.Columns.Add("Number", "#");
        detailsGrid.Columns.Add("Layer", "Layer");
        detailsGrid.Columns.Add("ShapeId", "Shape id");
        detailsGrid.Columns.Add("FeatureId", "Feature id");
        detailsGrid.Columns.Add("ShapeType", "Type");
    }

    private void RefreshSelectedFeatures(string? emptyMessage = null)
    {
        var selected = geoKernelViewerControl.GetSelectedFeatures();
        detailsGrid.Rows.Clear();

        for (var i = 0; i < selected.Count; i++)
        {
            var hit = selected[i];
            detailsGrid.Rows.Add(i + 1, hit.LayerName, hit.ShapeId, hit.FeatureId, hit.ShapeType);
        }

        if (selected.Count == 0)
            detailsGrid.Rows.Add("-", emptyMessage ?? "No selected features.", "-", "-", "-");
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
        statusLabel.Text = text;
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
        FillOpacity = 160,
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

