using System.Diagnostics;
using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerLoadOptions.Winforms;

public sealed partial class MainForm : Form
{
    private enum LoadedIndexMode { None, NoIndex, RTree }
    private LoadedIndexMode _currentMode;

    public MainForm() => InitializeComponent();

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        SetProgress(0);
        SetStatus("Ready. Load one mode first.");
    }

    private async void loadNoIndexButton_Click(object sender, EventArgs e) => await LoadVectorLayerAsync(false);
    private async void loadRTreeButton_Click(object sender, EventArgs e) => await LoadVectorLayerAsync(true);
    private void runQueryTestButton_Click(object sender, EventArgs e) => RunQueryBenchmark();

    private void clearButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.InvalidateRenderCache(false, true);
        _currentMode = LoadedIndexMode.None;
        SetProgress(0);
        noIndexResultLabel.Text = "No Index: -";
        rtreeResultLabel.Text = "RTree: -";
        SetStatus("Layers cleared.");
    }

    private async Task LoadVectorLayerAsync(bool useSpatialIndex)
    {
        SetUiEnabled(false);
        UseWaitCursor = true;
        try
        {
            var dataProgress = new ControlProgress<SampleDataProgress>(this, p =>
            {
                SetStatus(p.Message);
                SetProgress(p.Percentage);
            });
            var path = await SampleData.EnsureStatesAsync(this, dataProgress);
            if (string.IsNullOrEmpty(path)) return;

            geoKernelViewerControl.ClearLayers();
            SetProgress(0);
            SetStatus(useSpatialIndex ? "Loading USA states with RTree..." : "Loading USA states without spatial index...");
            var layerProgress = new ControlProgress<GeoKernelLayerLoadProgress>(this, p =>
            {
                if (p.Progress.HasValue) SetProgress(p.Progress.Value);
                if (!string.IsNullOrWhiteSpace(p.Status)) SetStatus(p.Status);
            });
            var indexProgress = new ControlProgress<GeoKernelSpatialIndexPreparationState>(this, state => SetStatus(SpatialIndexStateText(state)));

            var stopwatch = Stopwatch.StartNew();
            var loaded = geoKernelViewerControl.AddLayerFile(
                path,
                CreateLoadOptions(useSpatialIndex),
                layerProgress,
                spatialIndexState: indexProgress);
            stopwatch.Stop();
            if (!loaded) { SetStatus("Load failed."); return; }

            var layer = geoKernelViewerControl.GetLayerInfo(0);
            if (layer is not null) geoKernelViewerControl.SetLayerName(layer.Index, useSpatialIndex ? "USA States - RTree" : "USA States - No Index");
            geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-16831516, 1856556, -4631023, 7472472);
            _currentMode = useSpatialIndex ? LoadedIndexMode.RTree : LoadedIndexMode.NoIndex;
            SetProgress(100);
            SetStatus($"{(useSpatialIndex ? "RTree" : "No-index")} layer loaded. Load time: {stopwatch.ElapsedMilliseconds} ms.");
        }
        finally { UseWaitCursor = false; SetUiEnabled(true); }
    }

    private static GeoKernelLayerLoadOptions CreateLoadOptions(bool useSpatialIndex) => new()
    {
        UseSpatialIndex = useSpatialIndex,
        SpatialIndexType = GeoKernelSpatialIndexType.RTree,
        BuildFeatureSource = true,
        ApplyDefaultStyle = true,
        DefaultStyle = new GeoKernelLayerStyle { FillColor = "#D8E5E1", FillOpacity = 210, LineColor = "#607D78", LineWidth = 0.9 }
    };

    private void RunQueryBenchmark()
    {
        if (_currentMode == LoadedIndexMode.None || geoKernelViewerControl.LayerCount == 0) { SetStatus("Load the shapefile first."); return; }
        UseWaitCursor = true;
        SetUiEnabled(false);
        try
        {
            var extent = geoKernelViewerControl.GetLayerProjectedExtent(0);
            if (extent.XMax <= extent.XMin || extent.YMax <= extent.YMin) { SetStatus("Layer extent is empty."); return; }
            const int rows = 5, columns = 8, passes = 6;
            var stepX = (extent.XMax - extent.XMin) / columns;
            var stepY = (extent.YMax - extent.YMin) / rows;
            var totalQueries = rows * columns * passes;
            var completed = 0;
            var totalHits = 0;
            SetProgress(0);
            SetStatus("Running query benchmark...");
            var stopwatch = Stopwatch.StartNew();
            for (var pass = 0; pass < passes; pass++)
            for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
            {
                var xMin = extent.XMin + column * stepX;
                var yMin = extent.YMin + row * stepY;
                totalHits += geoKernelViewerControl.HitTestFeatureCountInExtent(new GeoKernelExtent(
                    xMin, yMin, xMin + stepX * 0.65, yMin + stepY * 0.65));
                completed++;
                if (completed % 16 == 0) SetProgress(completed * 100 / totalQueries);
            }
            stopwatch.Stop();
            SetProgress(100);
            var mode = _currentMode == LoadedIndexMode.RTree ? "RTree" : "No Index";
            var result = $"{mode}: query {stopwatch.ElapsedMilliseconds} ms, {totalQueries} queries, {totalHits} hits";
            if (_currentMode == LoadedIndexMode.RTree) rtreeResultLabel.Text = result; else noIndexResultLabel.Text = result;
            SetStatus($"Query test finished: {totalQueries} queries, {totalHits} hits, {stopwatch.ElapsedMilliseconds} ms.");
        }
        finally { UseWaitCursor = false; SetUiEnabled(true); }
    }

    private void SetProgress(int? value)
    {
        progressBar.Style = value.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (value.HasValue) progressBar.Value = Math.Clamp(value.Value, 0, 100);
    }

    private void SetStatus(string text) => statusLabel.Text = text;
    private void SetUiEnabled(bool enabled) { foreach (Control control in toolbarPanel.Controls) if (control is Button) control.Enabled = enabled; }

    private static string SpatialIndexStateText(GeoKernelSpatialIndexPreparationState state) => state switch
    {
        GeoKernelSpatialIndexPreparationState.Loading => "Spatial index is loading...",
        GeoKernelSpatialIndexPreparationState.BuildingLocator => "Spatial locator is preparing...",
        GeoKernelSpatialIndexPreparationState.BuildingIndex => "Spatial index is building...",
        GeoKernelSpatialIndexPreparationState.Ready => "Spatial index is ready.",
        GeoKernelSpatialIndexPreparationState.Cancelled => "Spatial index cancelled.",
        GeoKernelSpatialIndexPreparationState.Failed => "Spatial index failed.",
        _ => "Spatial index idle."
    };

    private sealed class ControlProgress<T>(Control control, Action<T> callback) : IProgress<T>
    {
        public void Report(T value) { if (control.IsDisposed) return; if (control.InvokeRequired) control.Invoke(() => callback(value)); else callback(value); }
    }
}
