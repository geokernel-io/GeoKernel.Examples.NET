using GeoKernel.NET.WinForms;

namespace GeoKernel.AddShapeToEditLayer.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string PolygonLayerName = "Programmatic Polygons";

    private int _polygonLayerIndex = -1;
    private int _polygonCursor;

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

        CreatePolygonLayer();
        BeginPolygonEditing();
        SetSampleExtent();
        UpdateStatus("Click Add Shape to add polygon geometry to the active edit layer.");
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

    private void CreatePolygonLayer()
    {
        _polygonLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(
            PolygonLayerName,
            GeoKernelShapeType.Polygon,
            PolygonStyle());

        _polygonLayerIndex = geoKernelViewerControl.GetLayerInfoByName(PolygonLayerName)?.Index ?? _polygonLayerIndex;

        if (_polygonLayerIndex < 0)
            MessageBox.Show(this, "Programmatic polygon layer could not be created.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void BeginPolygonEditing()
    {
        if (_polygonLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_polygonLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_polygonLayerIndex);

        geoKernelViewerControl.SetActiveEditLayerIndex(_polygonLayerIndex);
        UpdatePolygonCount();
    }

    private void addPolygonButton_Click(object? sender, EventArgs e)
    {
        if (_polygonLayerIndex < 0)
            return;

        BeginPolygonEditing();

        var points = SamplePolygonAt(_polygonCursor);
        if (!geoKernelViewerControl.AddPolygonToEditLayer(_polygonLayerIndex, points))
        {
            UpdateStatus("Polygon could not be added.");
            return;
        }

        _polygonCursor++;
        RefreshMap();
        UpdatePolygonCount();
        UpdateStatus($"Shape added with AddPolygonToEditLayer({_polygonLayerIndex}, {points.Count} vertices).");
    }

    private void clearPolygonsButton_Click(object? sender, EventArgs e)
    {
        if (_polygonLayerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_polygonLayerIndex);
        _polygonCursor = 0;
        BeginPolygonEditing();
        RefreshMap();
        UpdateStatus("Programmatic polygons cleared.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex == _polygonLayerIndex)
            UpdatePolygonCount();
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

    private void UpdatePolygonCount()
    {
        var count = _polygonLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_polygonLayerIndex) : 0;
        polygonCountLabel.Text = $"Polygon count: {count}";
    }

    private void UpdateStatus(string text)
    {
        statusLabel.Text = text;
    }

    private static IReadOnlyList<GeoKernelPoint> SamplePolygonAt(int index)
    {
        const double startX = -124.0;
        const double startY = 27.0;
        const double xStep = 7.5;
        const double yStep = 4.2;
        const int columns = 7;

        var column = index % columns;
        var row = index / columns;
        var x = startX + column * xStep;
        var y = startY + row * yStep;

        return
        [
            new GeoKernelPoint(x, y),
            new GeoKernelPoint(x + 4.4, y + 0.2),
            new GeoKernelPoint(x + 5.6, y + 2.4),
            new GeoKernelPoint(x + 2.3, y + 3.4),
            new GeoKernelPoint(x - 0.4, y + 2.0),
            new GeoKernelPoint(x, y)
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
