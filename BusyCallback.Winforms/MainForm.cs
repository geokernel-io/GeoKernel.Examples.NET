using System.Diagnostics;
using GeoKernel.NET.WinForms;

namespace GeoKernel.BusyCallback.Winforms;

public sealed partial class MainForm : Form
{
    private bool _isLoading;
    private string _samplePath = string.Empty;

    public MainForm()
    {
        InitializeComponent();
        ConnectBusyEvents();
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        var progress = new ControlProgress<SampleDataProgress>(this, value =>
        {
            SetStatus(value.Message);
            progressBar.Style = value.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee;
            if (value.Percentage.HasValue)
                SetProgress(value.Percentage.Value);
        });
        _samplePath = await SampleData.EnsureFileAsync("output_1m_points.zip", "output_1m_points", "output_1m_points.shp", "1M points", this, progress);
        progressBar.Style = ProgressBarStyle.Blocks;
        if (string.IsNullOrEmpty(_samplePath))
            return;

        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        SetProgress(0);
        SetStatus("Ready. Click Load Large Layer to see busy/progress callbacks.");
        AppendLog("Sample ready. API: BusyChanged + AddLayerFile progress callbacks.");
    }

    private void ConnectBusyEvents()
    {
        geoKernelViewerControl.BusyChanged += (_, e) =>
        {
            busyStateLabel.Text = e.Busy ? "Busy: ON" : "Busy: OFF";
            AppendLog($"Event: BusyChanged({e.Busy.ToString().ToLowerInvariant()})");
        };
        geoKernelViewerControl.LayerAdded += (_, e) => AppendLog($"Event: LayerAdded(index={e.LayerIndex}, name={e.LayerName})");
        geoKernelViewerControl.LayersChanged += (_, _) => AppendLog($"Event: LayersChanged(count={geoKernelViewerControl.LayerCount})");
    }

    private void loadButton_Click(object sender, EventArgs e)
    {
        LoadLargeLayer();
    }

    private void clearButton_Click(object sender, EventArgs e)
    {
        if (_isLoading)
            return;

        geoKernelViewerControl.ClearLayers();
        SetProgress(0);
        SetStatus("Layers cleared.");
        AppendLog("Action: ClearLayers()");
    }

    private void LoadLargeLayer()
    {
        var path = _samplePath;
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "BusyCallback",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        _isLoading = true;
        loadButton.Enabled = false;
        clearButton.Enabled = false;
        UseWaitCursor = true;
        SetProgress(0);
        SetStatus("Loading output_1m_points.shp...");
        AppendLog("Action: AddLayerFile(output_1m_points.shp)");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            geoKernelViewerControl.ClearLayers();
            var loaded = geoKernelViewerControl.AddLayerFile(
                path,
                CreateLoadOptions(),
                new DirectProgress<GeoKernelLayerLoadProgress>(progress =>
                {
                    if (progress.Progress.HasValue)
                        SetProgress(progress.Progress.Value);
                    if (!string.IsNullOrWhiteSpace(progress.Status))
                        SetStatus(progress.Status);
                }),
                spatialIndexState: new DirectProgress<GeoKernelSpatialIndexPreparationState>(state =>
                {
                    var text = SpatialIndexStateText(state);
                    SetStatus(text);
                    AppendLog($"Callback: spatialIndexState={state}");
                }));

            stopwatch.Stop();
            if (!loaded)
            {
                SetProgress(0);
                SetStatus("Layer load failed.");
                AppendLog("Result: load failed");
                return;
            }

            var layer = geoKernelViewerControl.GetLayerInfo(0);
            if (layer is not null)
                geoKernelViewerControl.SetLayerName(layer.Index, "One Million Points");

            geoKernelViewerControl.FullExtent();
            SetProgress(100);
            SetStatus($"Layer loaded in {stopwatch.ElapsedMilliseconds} ms.");
            AppendLog($"Result: loaded in {stopwatch.ElapsedMilliseconds} ms");
        }
        finally
        {
            _isLoading = false;
            UseWaitCursor = false;
            loadButton.Enabled = true;
            clearButton.Enabled = true;
            busyStateLabel.Text = "Busy: OFF";
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
            PointColor = "#2D82B7",
            PointSize = 2.8,
            LineColor = "#1C5D87",
            LineWidth = 0.8
        }
    };

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

    private void AppendLog(string text)
    {
        eventLogTextBox.AppendText($"{DateTime.Now:HH:mm:ss.fff}  {text}{Environment.NewLine}");
    }

    private static string SpatialIndexStateText(GeoKernelSpatialIndexPreparationState state) =>
        state switch
        {
            GeoKernelSpatialIndexPreparationState.Loading => "Spatial index loading...",
            GeoKernelSpatialIndexPreparationState.BuildingLocator => "Spatial locator preparing...",
            GeoKernelSpatialIndexPreparationState.BuildingIndex => "Spatial index building...",
            GeoKernelSpatialIndexPreparationState.Ready => "Spatial index ready.",
            GeoKernelSpatialIndexPreparationState.Cancelled => "Spatial index cancelled.",
            GeoKernelSpatialIndexPreparationState.Failed => "Spatial index failed.",
            _ => "Spatial index idle."
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

    private sealed class DirectProgress<T>(Action<T> handler) : IProgress<T>
    {
        public void Report(T value) => handler(value);
    }
}
