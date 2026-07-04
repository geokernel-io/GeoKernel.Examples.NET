using GeoKernel.NET.WinForms;

namespace GeoKernel.AddPointInteractive.Winforms;

public sealed partial class MainForm : Form
{
    private const string PointLayerName = "Clicked Points";

    private int _pointLayerIndex = -1;
    private bool _addPointMode = true;
    private int _displayPointCount;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePointLayer();
        BeginPointEditing();
        SetSampleExtent();
        UpdateStatus("Add Point active. Click the map to add points.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "AddPointInteractive",
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
                "AddPointInteractive",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var worldLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            geoKernelViewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreatePointLayer()
    {
        _pointLayerIndex = geoKernelViewerControl.AddPointLayer(
            PointLayerName,
            [new GeoKernelPoint(-122.4194, 37.7749)],
            PointStyle());

        _pointLayerIndex = geoKernelViewerControl.GetLayerInfoByName(PointLayerName)?.Index ?? _pointLayerIndex;
        _displayPointCount = FeatureCount();
    }

    private void BeginPointEditing()
    {
        if (_pointLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_pointLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_pointLayerIndex);
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _pointLayerIndex)
            return;

        _displayPointCount = FeatureCount();
        RefreshMap();
        UpdateStatus(_addPointMode ? "Point layer updated. Click the map to add points." : "Point layer updated.");
    }

    private void addPointButton_Click(object sender, EventArgs e)
    {
        _addPointMode = true;
        SetToolbarMode(addPointActive: true);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        BeginPointEditing();
        UpdateStatus("Add Point active. Click the map to add points.");
    }

    private void panButton_Click(object sender, EventArgs e)
    {
        _addPointMode = false;
        SetToolbarMode(addPointActive: false);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan active.");
    }

    private void clearPointsButton_Click(object sender, EventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_pointLayerIndex);
        BeginPointEditing();
        _displayPointCount = FeatureCount();
        RefreshMap();
        UpdateStatus("Clicked points cleared.");
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
        pointCountLabel.Text = $"Point count: {_displayPointCount}";
        statusLabel.Text = message;
    }

    private void SetToolbarMode(bool addPointActive)
    {
        addPointButton.BackColor = addPointActive ? Color.FromArgb(210, 232, 255) : SystemColors.Control;
        addPointButton.FlatAppearance.BorderSize = addPointActive ? 1 : 0;
        panButton.BackColor = addPointActive ? SystemColors.Control : Color.FromArgb(210, 232, 255);
        panButton.FlatAppearance.BorderSize = addPointActive ? 0 : 1;
    }

    private int FeatureCount()
    {
        return _pointLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_pointLayerIndex) : 0;
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

    private static GeoKernelLayerStyle PointStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            LineColor = "#8C321D",
            PointSize = 9.0,
            LineWidth = 1.2
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
