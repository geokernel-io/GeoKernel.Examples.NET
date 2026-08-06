using GeoKernel.Examples.Common;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LayerLoadCancel.Wpf;

public sealed partial class MainWindow : Window
{
    private bool _cancelRequested;
    private bool _isLoading;
    private bool _isPumpingMessages;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        SetProgress(0);
    }

    private void Load_Click(object sender, RoutedEventArgs e)
    {
        LoadLargeLayer();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        if (!_isLoading)
            return;

        _cancelRequested = true;
        cancelButton.IsEnabled = false;
        statusText.Text = "Cancel requested...";
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        if (_isLoading)
            return;

        _cancelRequested = false;
        viewerControl.ClearLayers();
        SetProgress(0);
        loadButton.IsEnabled = true;
        cancelButton.IsEnabled = false;
        SetStatus("Layers cleared.");
    }

    private void LoadLargeLayer()
    {
        var path = SampleData.EnsureKnownWpfSampleFile("output_1m_points.shp", this);
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "LayerLoadCancel",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        _cancelRequested = false;
        _isLoading = true;
        loadButton.IsEnabled = false;
        cancelButton.IsEnabled = true;
        clearButton.IsEnabled = false;
        SetProgress(0);
        SetStatus("Layer load started...");

        Mouse.OverrideCursor = Cursors.Wait;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            viewerControl.ClearLayers();
            var loaded = viewerControl.AddLayerFile(
                path,
                CreateLoadOptions(),
                new Progress<GeoKernelLayerLoadProgress>(progress =>
                {
                    if (progress.Progress.HasValue)
                        SetProgress(progress.Progress.Value);
                    if (!string.IsNullOrWhiteSpace(progress.Status))
                        SetStatus(progress.Status);
                }),
                isCancellationRequested: () =>
                {
                    PumpMessagesForCancel();
                    return _cancelRequested;
                },
                spatialIndexState: new Progress<GeoKernelSpatialIndexPreparationState>(state =>
                    SetStatus(SpatialIndexStateText(state))));

            stopwatch.Stop();

            if (_cancelRequested)
            {
                SetProgress(0);
                SetStatus($"Layer load cancelled after {stopwatch.ElapsedMilliseconds} ms.");
                return;
            }

            if (!loaded)
            {
                SetProgress(0);
                SetStatus("Layer load failed.");
                MessageBox.Show(
                    this,
                    $"Layer could not be loaded:{Environment.NewLine}{path}",
                    "LayerLoadCancel",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return;
            }

            var layer = viewerControl.GetLayerInfo(0);
            if (layer is not null)
                viewerControl.SetLayerName(layer.Index, "One Million Points");

            viewerControl.FullExtent();
            SetProgress(100);
            SetStatus($"Layer loaded in {stopwatch.ElapsedMilliseconds} ms.");
        }
        finally
        {
            _isLoading = false;
            Mouse.OverrideCursor = null;
            loadButton.IsEnabled = true;
            cancelButton.IsEnabled = false;
            clearButton.IsEnabled = true;
        }
    }

    private static GeoKernelLayerLoadOptions CreateLoadOptions()
    {
        return new GeoKernelLayerLoadOptions
        {
            UseSpatialIndex = true,
            SpatialIndexType = GeoKernelSpatialIndexType.RTree,
            BuildFeatureSource = true,
            ApplyDefaultStyle = true,
            DefaultStyle = new GeoKernelLayerStyle
            {
                FillColor = "#D8E5E1",
                FillOpacity = 210,
                LineColor = "#607D78",
                LineWidth = 0.9,
                PointColor = "#2D82B7",
                PointSize = 3.5
            }
        };
    }

    private void SetProgress(int value)
    {
        progressBar.Value = Math.Clamp(value, 0, 100);
    }

    private void SetStatus(string text)
    {
        statusText.Text = text;
    }

    private void PumpMessagesForCancel()
    {
        if (_isPumpingMessages)
            return;

        _isPumpingMessages = true;
        try
        {
            Dispatcher.Invoke(static () => { }, DispatcherPriority.Background);
        }
        finally
        {
            _isPumpingMessages = false;
        }
    }

    private static string SpatialIndexStateText(GeoKernelSpatialIndexPreparationState state)
    {
        return state switch
        {
            GeoKernelSpatialIndexPreparationState.Loading => "Spatial index is loading...",
            GeoKernelSpatialIndexPreparationState.BuildingLocator => "Feature locators are preparing...",
            GeoKernelSpatialIndexPreparationState.BuildingIndex => "Spatial index is building...",
            GeoKernelSpatialIndexPreparationState.Ready => "Spatial index is ready.",
            GeoKernelSpatialIndexPreparationState.Cancelled => "Load cancelled while preparing spatial index.",
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
