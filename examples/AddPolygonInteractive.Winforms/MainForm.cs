using GeoKernel.NET.WinForms;

namespace GeoKernel.AddPolygonInteractive.Winforms;

public sealed partial class MainForm : Form
{
    private const string PolygonLayerName = "Drawn Polygons";

    private int _polygonLayerIndex = -1;
    private bool _addPolygonMode = true;
    private int _displayPolygonCount;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPolygon;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePolygonLayer();
        BeginPolygonEditing();
        SetSampleExtent();
        UpdateStatus("Add Polygon active. Click vertices, then double-click or press Enter to finish.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "AddPolygonInteractive",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = WorldStyle()
                }))
        {
            MessageBox.Show(
                this,
                $"World layer could not be loaded:{Environment.NewLine}{path}",
                "AddPolygonInteractive",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var worldLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            geoKernelViewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreatePolygonLayer()
    {
        _polygonLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(
            PolygonLayerName,
            GeoKernelShapeType.Polygon,
            PolygonStyle());

        _polygonLayerIndex = geoKernelViewerControl.GetLayerInfoByName(PolygonLayerName)?.Index ?? _polygonLayerIndex;
        _displayPolygonCount = FeatureCount();
    }

    private void BeginPolygonEditing()
    {
        if (_polygonLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_polygonLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_polygonLayerIndex);

        geoKernelViewerControl.SetActiveEditLayerIndex(_polygonLayerIndex);
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _polygonLayerIndex)
            return;

        _displayPolygonCount = FeatureCount();
        RefreshMap();
        UpdateStatus(_addPolygonMode
            ? "Polygon layer updated. Click vertices, then double-click or press Enter to finish."
            : "Polygon layer updated.");
    }

    private void addPolygonButton_Click(object sender, EventArgs e)
    {
        _addPolygonMode = true;
        SetToolbarMode(addPolygonActive: true);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPolygon;
        BeginPolygonEditing();
        UpdateStatus("Add Polygon active. Click vertices, then double-click or press Enter to finish.");
    }

    private void panButton_Click(object sender, EventArgs e)
    {
        _addPolygonMode = false;
        SetToolbarMode(addPolygonActive: false);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan active.");
    }

    private void clearPolygonsButton_Click(object sender, EventArgs e)
    {
        if (_polygonLayerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_polygonLayerIndex);
        BeginPolygonEditing();
        _displayPolygonCount = FeatureCount();
        RefreshMap();
        UpdateStatus("Drawn polygons cleared.");
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void RefreshMap()
    {
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 20.0, -65.0, 52.0);
    }

    private void UpdateStatus(string message)
    {
        polygonCountLabel.Text = $"Polygon count: {_displayPolygonCount}";
        statusLabel.Text = message;
    }

    private void SetToolbarMode(bool addPolygonActive)
    {
        addPolygonButton.BackColor = addPolygonActive ? Color.FromArgb(210, 232, 255) : SystemColors.Control;
        addPolygonButton.FlatAppearance.BorderSize = addPolygonActive ? 1 : 0;
        panButton.BackColor = addPolygonActive ? SystemColors.Control : Color.FromArgb(210, 232, 255);
        panButton.FlatAppearance.BorderSize = addPolygonActive ? 0 : 1;
    }

    private int FeatureCount()
    {
        return _polygonLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_polygonLayerIndex) : 0;
    }

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 210,
            LineColor = "#6F8883",
            LineWidth = 0.7
        };
    }

    private static GeoKernelLayerStyle PolygonStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#F2D27A",
            FillOpacity = 160,
            LineColor = "#D95D39",
            LineWidth = 2.0
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
