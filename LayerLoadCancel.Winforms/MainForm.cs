using System.Diagnostics;
using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerLoadCancel.Winforms;

public sealed partial class MainForm : Form
{
    private bool _cancelRequested;
    private bool _isLoading;
    private bool _isPumpingMessages;

    public MainForm() => InitializeComponent();

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        SetProgress(0);
    }

    private async void loadButton_Click(object sender, EventArgs e)
    {
        loadButton.Enabled = false;
        clearButton.Enabled = false;
        UseWaitCursor = true;
        try
        {
            var dataProgress = new ControlProgress<SampleDataProgress>(this, p =>
            {
                SetStatus(p.Message);
                SetProgress(p.Percentage);
            });
            var path = await SampleData.EnsureLargeLayerAsync(this, dataProgress);
            if (string.IsNullOrEmpty(path))
            {
                SetProgress(0);
                SetStatus("Sample data could not be prepared.");
                return;
            }
            LoadLargeLayer(path);
        }
        finally
        {
            if (!_isLoading)
            {
                UseWaitCursor = false;
                loadButton.Enabled = true;
                clearButton.Enabled = true;
            }
        }
    }

    private void cancelButton_Click(object sender, EventArgs e)
    {
        if (!_isLoading) return;
        _cancelRequested = true;
        cancelButton.Enabled = false;
        SetStatus("Cancel requested...");
    }

    private void clearButton_Click(object sender, EventArgs e)
    {
        if (_isLoading) return;
        _cancelRequested = false;
        geoKernelViewerControl.ClearLayers();
        geoKernelViewerControl.InvalidateRenderCache(false, true);
        SetProgress(0);
        loadButton.Enabled = true;
        cancelButton.Enabled = false;
        SetStatus("Layers cleared.");
    }

    private void LoadLargeLayer(string path)
    {
        _cancelRequested = false;
        _isLoading = true;
        loadButton.Enabled = false;
        cancelButton.Enabled = true;
        clearButton.Enabled = false;
        SetProgress(0);
        SetStatus("Layer load started...");
        var layerProgress = new ControlProgress<GeoKernelLayerLoadProgress>(this, p =>
        {
            if (p.Progress.HasValue) SetProgress(p.Progress.Value);
            if (!string.IsNullOrWhiteSpace(p.Status)) SetStatus(p.Status);
        });
        var indexProgress = new ControlProgress<GeoKernelSpatialIndexPreparationState>(this, state => SetStatus(SpatialIndexStateText(state)));
        var stopwatch = Stopwatch.StartNew();
        try
        {
            geoKernelViewerControl.ClearLayers();
            var loaded = geoKernelViewerControl.AddLayerFile(
                path,
                CreateLoadOptions(),
                layerProgress,
                isCancellationRequested: () =>
                {
                    PumpMessagesForCancel();
                    return _cancelRequested;
                },
                spatialIndexState: indexProgress);
            stopwatch.Stop();

            if (_cancelRequested)
            {
                geoKernelViewerControl.ClearLayers();
                SetProgress(0);
                SetStatus($"Layer load cancelled after {stopwatch.ElapsedMilliseconds} ms.");
                return;
            }
            if (!loaded)
            {
                SetProgress(0);
                SetStatus("Layer load failed.");
                MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "LayerLoadCancel", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var layer = geoKernelViewerControl.GetLayerInfo(0);
            if (layer is not null) geoKernelViewerControl.SetLayerName(layer.Index, "One Million Points");
            geoKernelViewerControl.FullExtent();
            SetProgress(100);
            SetStatus($"Layer loaded in {stopwatch.ElapsedMilliseconds} ms.");
        }
        finally
        {
            _isLoading = false;
            UseWaitCursor = false;
            loadButton.Enabled = true;
            cancelButton.Enabled = false;
            clearButton.Enabled = true;
        }
    }

    private static GeoKernelLayerLoadOptions CreateLoadOptions() => new()
    {
        UseSpatialIndex = true,
        SpatialIndexType = GeoKernelSpatialIndexType.RTree,
        BuildFeatureSource = true,
        ApplyDefaultStyle = true,
        DefaultStyle = new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1", FillOpacity = 210, LineColor = "#607D78", LineWidth = 0.9,
            PointColor = "#2D82B7", PointSize = 3.5
        }
    };

    private void SetProgress(int? value)
    {
        progressBar.Style = value.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
        if (value.HasValue) progressBar.Value = Math.Clamp(value.Value, 0, 100);
    }

    private void SetStatus(string text) => statusLabel.Text = text;

    private void PumpMessagesForCancel()
    {
        if (_isPumpingMessages) return;
        _isPumpingMessages = true;
        try { Application.DoEvents(); }
        finally { _isPumpingMessages = false; }
    }

    private static string SpatialIndexStateText(GeoKernelSpatialIndexPreparationState state) => state switch
    {
        GeoKernelSpatialIndexPreparationState.Loading => "Spatial index is loading...",
        GeoKernelSpatialIndexPreparationState.BuildingLocator => "Feature locators are preparing...",
        GeoKernelSpatialIndexPreparationState.BuildingIndex => "Spatial index is building...",
        GeoKernelSpatialIndexPreparationState.Ready => "Spatial index is ready.",
        GeoKernelSpatialIndexPreparationState.Cancelled => "Load cancelled while preparing spatial index.",
        GeoKernelSpatialIndexPreparationState.Failed => "Spatial index failed.",
        _ => "Spatial index idle."
    };

    private sealed class ControlProgress<T>(Control control, Action<T> callback) : IProgress<T>
    {
        public void Report(T value) { if (control.IsDisposed) return; if (control.InvokeRequired) control.Invoke(() => callback(value)); else callback(value); }
    }
}
