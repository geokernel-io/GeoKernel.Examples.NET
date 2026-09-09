using GeoKernel.Examples.Common;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.BusyCallback.Wpf;

public partial class MainWindow
{
    private bool _isLoading;

    public MainWindow()
    {
        InitializeComponent();
        ConnectBusyEvents();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        SetProgress(0);
        SetStatus("Ready. Click Load Large Layer to see busy/progress callbacks.");
        AppendLog("Sample ready. API: BusyChanged + AddLayerFile progress callbacks.");
    }

    private void ConnectBusyEvents()
    {
        viewerControl.BusyChanged += (_, e) =>
        {
            busyStateText.Text = e.Busy ? "Busy: ON" : "Busy: OFF";
            AppendLog($"Event: BusyChanged({e.Busy.ToString().ToLowerInvariant()})");
        };
        viewerControl.LayerAdded += (_, e) => AppendLog($"Event: LayerAdded(index={e.LayerIndex}, name={e.LayerName})");
        viewerControl.LayersChanged += (_, _) => AppendLog($"Event: LayersChanged(count={viewerControl.LayerCount})");
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        LoadLargeLayer();
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
            return;

        viewerControl.ClearLayers();
        SetProgress(0);
        SetStatus("Layers cleared.");
        AppendLog("Action: ClearLayers()");
    }

    private void LoadLargeLayer()
    {
        var path = SampleData.EnsureKnownWpfSampleFile("output_1m_points.shp", this);
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "BusyCallback",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        _isLoading = true;
        loadButton.IsEnabled = false;
        clearButton.IsEnabled = false;
        Mouse.OverrideCursor = Cursors.Wait;
        SetProgress(0);
        SetStatus("Loading output_1m_points.shp...");
        AppendLog("Action: AddLayerFile(output_1m_points.shp)");

        var stopwatch = Stopwatch.StartNew();
        try
        {
            viewerControl.ClearLayers();
            var loaded = viewerControl.AddLayerFile(
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

            var layer = viewerControl.GetLayerInfo(0);
            if (layer is not null)
                viewerControl.SetLayerName(layer.Index, "One Million Points");

            viewerControl.FullExtent();
            SetProgress(100);
            SetStatus($"Layer loaded in {stopwatch.ElapsedMilliseconds} ms.");
            AppendLog($"Result: loaded in {stopwatch.ElapsedMilliseconds} ms");
        }
        finally
        {
            _isLoading = false;
            Mouse.OverrideCursor = null;
            loadButton.IsEnabled = true;
            clearButton.IsEnabled = true;
            busyStateText.Text = "Busy: OFF";
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
        PumpUi();
    }

    private void SetStatus(string text)
    {
        statusText.Text = text;
        PumpUi();
    }

    private void AppendLog(string text)
    {
        eventLogTextBox.AppendText($"{DateTime.Now:HH:mm:ss.fff}  {text}{Environment.NewLine}");
        eventLogTextBox.ScrollToEnd();
    }

    private void PumpUi()
    {
        Dispatcher.Invoke(static () => { }, DispatcherPriority.Render);
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
