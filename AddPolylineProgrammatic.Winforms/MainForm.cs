using GeoKernel.NET.WinForms;

namespace GeoKernel.AddPolylineProgrammatic.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string PolylineLayerName = "Programmatic Polylines";

    private int _polylineLayerIndex = -1;
    private int _polylineCursor;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        _worldSamplePath = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(_worldSamplePath)) return;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePolylineLayer();
        BeginPolylineEditing();
        SetSampleExtent();
        UpdateStatus("Click Add Polyline to call addPolylineToEditLayer(index, worldPoints).");
    }

    private bool LoadLayer()
    {
        var path = _worldSamplePath;
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"World shapefile could not be found:{Environment.NewLine}{path}",
                Text,
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
                Text,
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

        if (_polylineLayerIndex < 0)
            MessageBox.Show(this, "Programmatic polyline layer could not be created.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void BeginPolylineEditing()
    {
        if (_polylineLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_polylineLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_polylineLayerIndex);

        geoKernelViewerControl.SetActiveEditLayerIndex(_polylineLayerIndex);
        UpdatePolylineCount();
    }

    private void addPolylineButton_Click(object? sender, EventArgs e)
    {
        if (_polylineLayerIndex < 0)
            return;

        BeginPolylineEditing();

        var points = SamplePolylineAt(_polylineCursor);
        if (!geoKernelViewerControl.AddPolylineToEditLayer(_polylineLayerIndex, points))
        {
            UpdateStatus("Polyline could not be added.");
            return;
        }

        _polylineCursor++;
        RefreshMap();
        UpdatePolylineCount();
        UpdateStatus($"addPolylineToEditLayer({_polylineLayerIndex}, {points.Count} vertices)");
    }

    private void clearLinesButton_Click(object? sender, EventArgs e)
    {
        if (_polylineLayerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_polylineLayerIndex);
        _polylineCursor = 0;
        BeginPolylineEditing();
        RefreshMap();
        UpdateStatus("Programmatic polylines cleared.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex == _polylineLayerIndex)
            UpdatePolylineCount();
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 20.0, -65.0, 52.0);
    }

    private void RefreshMap()
    {
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
    }

    private void UpdatePolylineCount()
    {
        var count = _polylineLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_polylineLayerIndex) : 0;
        polylineCountLabel.Text = $"Polyline count: {count}";
    }

    private void UpdateStatus(string text)
    {
        statusLabel.Text = text;
    }

    private static IReadOnlyList<GeoKernelPoint> SamplePolylineAt(int index)
    {
        const double startX = -124.0;
        const double startY = 29.0;
        const double xStep = 7.0;
        const double yStep = 3.0;
        const int columns = 7;

        var column = index % columns;
        var row = index / columns;
        var x = startX + column * xStep;
        var y = startY + row * yStep;

        return
        [
            new GeoKernelPoint(x, y),
            new GeoKernelPoint(x + 2.2, y + 1.4),
            new GeoKernelPoint(x + 4.8, y + 0.4),
            new GeoKernelPoint(x + 6.4, y + 2.2)
        ];
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

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, p =>
    { statusLabel.Text = p.Message; downloadProgressBar.Visible = true; downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee; if (p.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(p.Percentage.Value, 0, 100); });

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
