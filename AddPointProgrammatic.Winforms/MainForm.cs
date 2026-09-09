using GeoKernel.NET.WinForms;

namespace GeoKernel.AddPointProgrammatic.Winforms;

public sealed partial class MainForm : Form
{
    private string _worldSamplePath = string.Empty;
    private const string PointLayerName = "Programmatic Points";
    private int _pointLayerIndex = -1;
    private int _pointCursor;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        _worldSamplePath = await SampleData.EnsureFileAsync("world_4326.zip", "world_4326", "world_4326.shp", "World", this, CreateSampleProgress());
        downloadProgressBar.Visible = false;
        if (string.IsNullOrEmpty(_worldSamplePath)) return;
        geoKernelViewerControl.LayerEditStateChanged += geoKernelViewerControl_LayerEditStateChanged;

        if (!LoadLayer())
            return;

        CreatePointLayer();
        BeginPointEditing();
        SetSampleExtent();
        UpdateStatus("Click Add Point to call addPointToEditLayer(index, worldPoint).");
    }

    private bool LoadLayer()
    {
        var path = _worldSamplePath;

        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:\n{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            MessageBox.Show(this, $"World layer could not be opened:\n{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var worldLayer = geoKernelViewerControl.GetLayerInfo(0);
        if (worldLayer is not null)
            geoKernelViewerControl.SetLayerName(worldLayer.Index, "World");

        return true;
    }

    private void CreatePointLayer()
    {
        _pointLayerIndex = geoKernelViewerControl.AddEmptyVectorLayer(
            PointLayerName,
            GeoKernelShapeType.Point,
            PointStyle());

        _pointLayerIndex = geoKernelViewerControl.GetLayerInfoByName(PointLayerName)?.Index ?? _pointLayerIndex;

        if (_pointLayerIndex < 0)
            MessageBox.Show(this, "Programmatic point layer could not be created.", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
    }

    private void BeginPointEditing()
    {
        if (_pointLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.IsLayerEditing(_pointLayerIndex))
            geoKernelViewerControl.BeginEditLayer(_pointLayerIndex);

        geoKernelViewerControl.SetActiveEditLayerIndex(_pointLayerIndex);
        UpdatePointCount();
    }

    private void addPointButton_Click(object? sender, EventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        BeginPointEditing();

        var point = SamplePointAt(_pointCursor);
        if (!geoKernelViewerControl.AddPointToEditLayer(_pointLayerIndex, point.X, point.Y))
        {
            UpdateStatus("Point could not be added.");
            return;
        }

        _pointCursor++;
        RefreshMap();
        UpdatePointCount();
        UpdateStatus($"addPointToEditLayer({_pointLayerIndex}, {point.X:F4}, {point.Y:F4})");
    }

    private void clearPointsButton_Click(object? sender, EventArgs e)
    {
        if (_pointLayerIndex < 0)
            return;

        geoKernelViewerControl.RollbackEditLayer(_pointLayerIndex);
        _pointCursor = 0;
        BeginPointEditing();
        RefreshMap();
        UpdateStatus("Programmatic points cleared.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_LayerEditStateChanged(object? sender, GeoKernelLayerEventArgs e)
    {
        if (e.LayerIndex == _pointLayerIndex)
            UpdatePointCount();
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

    private void UpdatePointCount()
    {
        var count = _pointLayerIndex >= 0 ? geoKernelViewerControl.GetLayerFeatureCount(_pointLayerIndex) : 0;
        pointCountLabel.Text = $"Point count: {count}";
    }

    private void UpdateStatus(string text)
    {
        statusLabel.Text = text;
    }

    private static GeoKernelPoint SamplePointAt(int index)
    {
        const double xMin = -124.0;
        const double yMin = 26.0;
        const double xStep = 1.9;
        const double yStep = 1.8;
        const int columns = 29;
        const int rows = 13;

        var cell = index % (columns * rows);
        var column = (cell * 7) % columns;
        var row = ((cell / columns) + (cell * 11)) % rows;

        return new GeoKernelPoint(xMin + column * xStep, yMin + row * yStep);
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
            PointSize = 9.5,
            LineWidth = 1.2
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
