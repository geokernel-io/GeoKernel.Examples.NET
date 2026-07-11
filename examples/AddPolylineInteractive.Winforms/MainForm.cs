using GeoKernel.NET.WinForms;

namespace GeoKernel.AddPolylineInteractive.Winforms;

public sealed partial class MainForm : Form
{
    private const string PolylineLayerName = "Drawn Polylines";

    private int _polylineLayerIndex = -1;
    private bool _addPolylineMode = true;
    private int _displayPolylineCount;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePolylineLayer();
        BeginPolylineEditing();
        SetSampleExtent();
        UpdateStatus("Add Polyline active. Click vertices, then double-click or press Enter to finish.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                "AddPolylineInteractive",
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
                "AddPolylineInteractive",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var worldLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            geoKernelViewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreatePolylineLayer()
    {
        _polylineLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(
            PolylineLayerName,
            GeoKernelShapeType.Polyline,
            PolylineStyle());

        _polylineLayerIndex = geoKernelViewerControl.GetLayerInfoByName(PolylineLayerName)?.Index ?? _polylineLayerIndex;
        _displayPolylineCount = FeatureCount();
    }

    private void BeginPolylineEditing()
    {
        if (_polylineLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_polylineLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_polylineLayerIndex);

        geoKernelViewerControl.SetActiveEditLayerIndex(_polylineLayerIndex);
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex != _polylineLayerIndex)
            return;

        _displayPolylineCount = FeatureCount();
        RefreshMap();
        UpdateStatus(_addPolylineMode
            ? "Polyline layer updated. Click vertices, then double-click or press Enter to finish."
            : "Polyline layer updated.");
    }

    private void addPolylineButton_Click(object sender, EventArgs e)
    {
        _addPolylineMode = true;
        SetToolbarMode(addPolylineActive: true);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPolyline;
        BeginPolylineEditing();
        UpdateStatus("Add Polyline active. Click vertices, then double-click or press Enter to finish.");
    }

    private void panButton_Click(object sender, EventArgs e)
    {
        _addPolylineMode = false;
        SetToolbarMode(addPolylineActive: false);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan active.");
    }

    private void clearLinesButton_Click(object sender, EventArgs e)
    {
        if (_polylineLayerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_polylineLayerIndex);
        BeginPolylineEditing();
        _displayPolylineCount = FeatureCount();
        RefreshMap();
        UpdateStatus("Drawn polylines cleared.");
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
        polylineCountLabel.Text = $"Polyline count: {_displayPolylineCount}";
        statusLabel.Text = message;
    }

    private void SetToolbarMode(bool addPolylineActive)
    {
        addPolylineButton.BackColor = addPolylineActive ? System.Drawing.Color.FromArgb(210, 232, 247) : System.Drawing.SystemColors.Control;
        addPolylineButton.FlatAppearance.BorderSize = addPolylineActive ? 1 : 0;
        panButton.BackColor = addPolylineActive ? System.Drawing.SystemColors.Control : System.Drawing.Color.FromArgb(210, 232, 247);
        panButton.FlatAppearance.BorderSize = addPolylineActive ? 0 : 1;
    }

    private int FeatureCount()
    {
        return _polylineLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_polylineLayerIndex) : 0;
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

    private static GeoKernelLayerStyle PolylineStyle()
    {
        return new GeoKernelLayerStyle
        {
            LineColor = "#D95D39",
            LineWidth = 2.6
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
