using System.Diagnostics;
using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerLoadOptions.Winforms;

public sealed partial class MainForm : Form
{
    private enum LoadedIndexMode
    {
        None,
        NoIndex,
        RTree
    }

    private LoadedIndexMode _currentMode;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        SetProgress(0);
        SetStatus("Ready. Load one mode first.");
    }

    private void loadNoIndexButton_Click(object sender, EventArgs e)
    {
        LoadVectorLayer(useSpatialIndex: false);
    }

    private void loadRTreeButton_Click(object sender, EventArgs e)
    {
        LoadVectorLayer(useSpatialIndex: true);
    }

    private void runQueryTestButton_Click(object sender, EventArgs e)
    {
        RunQueryBenchmark();
    }

    private void clearButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        _currentMode = LoadedIndexMode.None;
        SetProgress(0);
        noIndexResultLabel.Text = "No Index: -";
        rtreeResultLabel.Text = "RTree: -";
        SetStatus("Layers cleared.");
    }

    private void LoadVectorLayer(bool useSpatialIndex)
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "usa_states_3857.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "LayerLoadOptions",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        UseWaitCursor = true;
        try
        {
            geoKernelViewerControl.ClearLayers();
            SetProgress(0);
            SetStatus(useSpatialIndex ? "Loading USA states with RTree..." : "Loading USA states without spatial index...");

            var stopwatch = Stopwatch.StartNew();
            var loaded = geoKernelViewerControl.AddLayerFile(
                path,
                CreateLoadOptions(useSpatialIndex),
                new Progress<GeoKernelLayerLoadProgress>(progress =>
                {
                    if (progress.Progress.HasValue)
                        SetProgress(progress.Progress.Value);
                    if (!string.IsNullOrWhiteSpace(progress.Status))
                        SetStatus(progress.Status);
                }),
                spatialIndexState: new Progress<GeoKernelSpatialIndexPreparationState>(state =>
                    SetStatus(SpatialIndexStateText(state))));
            stopwatch.Stop();

            if (!loaded)
            {
                SetStatus("Load failed.");
                return;
            }

            var layer = geoKernelViewerControl.GetLayerInfo(0);
            if (layer is not null)
                geoKernelViewerControl.SetLayerName(layer.Index, useSpatialIndex ? "USA States - RTree" : "USA States - No Index");

            geoKernelViewerControl.ViewExtent = new GeoKernelExtent
            {
                XMin = -16831516,
                YMin = 1856556,
                XMax = -4631023,
                YMax = 7472472
            };

            _currentMode = useSpatialIndex ? LoadedIndexMode.RTree : LoadedIndexMode.NoIndex;
            SetProgress(100);
            SetStatus(useSpatialIndex
                ? $"RTree layer loaded. Load time: {stopwatch.ElapsedMilliseconds} ms."
                : $"No-index layer loaded. Load time: {stopwatch.ElapsedMilliseconds} ms.");
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private GeoKernelLayerLoadOptions CreateLoadOptions(bool useSpatialIndex)
    {
        return new GeoKernelLayerLoadOptions
        {
            UseSpatialIndex = useSpatialIndex,
            SpatialIndexType = GeoKernelSpatialIndexType.RTree,
            BuildFeatureSource = true,
            ApplyDefaultStyle = true,
            DefaultStyle = new GeoKernelLayerStyle
            {
                FillColor = "#D8E5E1",
                FillOpacity = 210,
                LineColor = "#607D78",
                LineWidth = 0.9
            }
        };
    }

    private void RunQueryBenchmark()
    {
        if (_currentMode == LoadedIndexMode.None || geoKernelViewerControl.LayerCount == 0)
        {
            SetStatus("Load the shapefile first.");
            return;
        }

        UseWaitCursor = true;
        try
        {
            var extent = geoKernelViewerControl.GetLayerProjectedExtent(0);
            if (extent.XMax <= extent.XMin || extent.YMax <= extent.YMin)
            {
                SetStatus("Layer extent is empty.");
                return;
            }

            const int rows = 5;
            const int columns = 8;
            const int passes = 6;
            var stepX = (extent.XMax - extent.XMin) / columns;
            var stepY = (extent.YMax - extent.YMin) / rows;
            var queryWidth = stepX * 0.65;
            var queryHeight = stepY * 0.65;
            var totalQueries = rows * columns * passes;
            var completed = 0;
            var totalHits = 0;

            SetProgress(0);
            SetStatus("Running query benchmark...");

            var stopwatch = Stopwatch.StartNew();
            for (var pass = 0; pass < passes; ++pass)
            {
                for (var row = 0; row < rows; ++row)
                {
                    for (var column = 0; column < columns; ++column)
                    {
                        var xMin = extent.XMin + column * stepX;
                        var yMin = extent.YMin + row * stepY;
                        totalHits += geoKernelViewerControl.HitTestFeatureCountInExtent(new GeoKernelExtent
                        {
                            XMin = xMin,
                            YMin = yMin,
                            XMax = xMin + queryWidth,
                            YMax = yMin + queryHeight
                        });

                        ++completed;
                        if (completed % 16 == 0)
                            SetProgress(completed * 100 / totalQueries);
                    }
                }
            }
            stopwatch.Stop();

            SetProgress(100);
            var resultText = $"{ModeText(_currentMode)}: query {stopwatch.ElapsedMilliseconds} ms, {totalQueries} queries, {totalHits} hits";
            if (_currentMode == LoadedIndexMode.RTree)
                rtreeResultLabel.Text = resultText;
            else
                noIndexResultLabel.Text = resultText;

            SetStatus($"Query test finished: {totalQueries} queries, {totalHits} hits, {stopwatch.ElapsedMilliseconds} ms.");
        }
        finally
        {
            UseWaitCursor = false;
        }
    }

    private void SetProgress(int value)
    {
        progressBar.Value = Math.Clamp(value, 0, 100);
        Application.DoEvents();
    }

    private void SetStatus(string text)
    {
        statusLabel.Text = text;
        Application.DoEvents();
    }

    private static string ModeText(LoadedIndexMode mode)
    {
        return mode == LoadedIndexMode.RTree ? "RTree" : "No Index";
    }

    private static string SpatialIndexStateText(GeoKernelSpatialIndexPreparationState state)
    {
        return state switch
        {
            GeoKernelSpatialIndexPreparationState.Loading => "Spatial index is loading...",
            GeoKernelSpatialIndexPreparationState.BuildingLocator => "Spatial locator is preparing...",
            GeoKernelSpatialIndexPreparationState.BuildingIndex => "Spatial index is building...",
            GeoKernelSpatialIndexPreparationState.Ready => "Spatial index is ready.",
            GeoKernelSpatialIndexPreparationState.Cancelled => "Spatial index cancelled.",
            GeoKernelSpatialIndexPreparationState.Failed => "Spatial index failed.",
            _ => "Spatial index idle."
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
