using System.Diagnostics;
using System.IO;
using System.Windows;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.RasterOverview.Wpf;

public sealed partial class MainWindow
{
    private string _sourceRasterPath = string.Empty;
    private string _mode = "Reset";
    private long _elapsedMilliseconds;
    private BenchmarkResult? _withoutOverviewBenchmark;
    private BenchmarkResult? _withOverviewBenchmark;
    private string _lastBenchmarkText = string.Empty;
    private static string WorkingDirectory => Path.Combine(AppContext.BaseDirectory, "RasterOverviewData");
    private static string WorkingRasterPath => Path.Combine(WorkingDirectory, "world_8km_overview_test.tif");
    private static string OverviewPath => WorkingRasterPath + ".ovr";

    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        _sourceRasterPath = SampleData.EnsureKnownWpfSampleFile("world_8km.tif", this);
        if (!string.IsNullOrWhiteSpace(_sourceRasterPath)) ResetWorkingCopy();
    }

    private void Reset_Click(object sender, RoutedEventArgs e) => ResetWorkingCopy();
    private void LoadWithout_Click(object sender, RoutedEventArgs e) => LoadRaster(false);
    private void LoadWith_Click(object sender, RoutedEventArgs e) => LoadRaster(true);
    private void Benchmark_Click(object sender, RoutedEventArgs e) => ShowBenchmarkComparison();
    private void FullExtent_Click(object sender, RoutedEventArgs e) => viewerControl.FullExtent();

    private bool ResetWorkingCopy()
    {
        try
        {
            viewerControl.ClearLayers();
            Directory.CreateDirectory(WorkingDirectory);
            File.Copy(_sourceRasterPath, WorkingRasterPath, true);
            DeleteIfExists(OverviewPath);
            DeleteIfExists(WorkingRasterPath + ".aux.xml");
            _mode = "Reset";
            _elapsedMilliseconds = 0;
            _withoutOverviewBenchmark = null;
            _withOverviewBenchmark = null;
            _lastBenchmarkText = string.Empty;
            progressBar.Value = 0;
            statusText.Text = "Working copy reset. Overview file removed.";
            UpdateDetails();
            return true;
        }
        catch (Exception ex)
        {
            statusText.Text = "Reset failed.";
            MessageBox.Show(this, ex.Message, "RasterOverview", MessageBoxButton.OK, MessageBoxImage.Error);
            return false;
        }
    }

    private void LoadRaster(bool prepareOverview)
    {
        if (!File.Exists(WorkingRasterPath) && !ResetWorkingCopy()) return;
        var mode = prepareOverview ? "Load With Overview" : "Load Without Overview";
        try
        {
            viewerControl.ClearLayers();
            progressBar.Value = 0;
            statusText.Text = mode;
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            var timer = Stopwatch.StartNew();
            var options = new GeoKernelLayerLoadOptions
            {
                PrepareRasterOverviews = prepareOverview,
                RasterOverviewMinimumPixels = prepareOverview ? 0 : long.MaxValue,
                RasterOverviewResampling = "AVERAGE"
            };
            var progress = new Progress<GeoKernelLayerLoadProgress>(p =>
            {
                if (p.Progress.HasValue) progressBar.Value = Math.Clamp(p.Progress.Value, 0, 100);
                if (!string.IsNullOrWhiteSpace(p.Status)) statusText.Text = p.Status;
            });
            if (!viewerControl.AddLayerFile(WorkingRasterPath, options, progress))
                throw new InvalidOperationException($"Raster could not be loaded:{Environment.NewLine}{WorkingRasterPath}");
            timer.Stop();
            _mode = mode;
            _elapsedMilliseconds = timer.ElapsedMilliseconds;
            _lastBenchmarkText = string.Empty;
            viewerControl.SetLayerName(0, prepareOverview ? "GeoTIFF - Overview" : "GeoTIFF - No Overview");
            viewerControl.FullExtent();
            progressBar.Value = 100;
            statusText.Text = $"{mode} finished in {_elapsedMilliseconds} ms.";
            UpdateDetails();
        }
        catch (Exception ex)
        {
            statusText.Text = "Load failed.";
            MessageBox.Show(this, ex.Message, "RasterOverview", MessageBoxButton.OK, MessageBoxImage.Error);
            UpdateDetails();
        }
        finally { System.Windows.Input.Mouse.OverrideCursor = null; }
    }

    private void ShowBenchmarkComparison()
    {
        if (viewerControl.LayerCount == 0)
        {
            statusText.Text = "Load a raster first.";
            return;
        }

        try
        {
            System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
            statusText.Text = "Running downsample benchmark...";
            var result = RunCurrentBenchmark();
            _lastBenchmarkText = BenchmarkText(_mode, result);
            if (_mode == "Load Without Overview")
                _withoutOverviewBenchmark = result;
            else if (_mode == "Load With Overview")
                _withOverviewBenchmark = result;

            statusText.Text = _withoutOverviewBenchmark is not null && _withOverviewBenchmark is not null
                ? ComparisonText(_withoutOverviewBenchmark, _withOverviewBenchmark)
                : _lastBenchmarkText;
            UpdateDetails();
        }
        catch (Exception ex)
        {
            statusText.Text = "Benchmark failed.";
            MessageBox.Show(this, ex.Message, "RasterOverview", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally { System.Windows.Input.Mouse.OverrideCursor = null; }
    }

    private BenchmarkResult RunCurrentBenchmark()
    {
        var diagnostics = viewerControl.GetRasterOverviewDiagnostics(0, true);
        var benchmark = diagnostics?.Benchmark;
        if (benchmark is null || !benchmark.Valid)
            throw new InvalidOperationException(benchmark?.ErrorMessage ?? "Raster overview benchmark returned no result.");
        return new BenchmarkResult(
            benchmark.Passes,
            benchmark.TargetWidth,
            benchmark.TargetHeight,
            benchmark.SelectedOverview,
            benchmark.ElapsedMs);
    }

    private static string ComparisonText(BenchmarkResult withoutOverview, BenchmarkResult withOverview)
    {
        var saved = withoutOverview.ElapsedMs - withOverview.ElapsedMs;
        return saved > 0
            ? $"Comparison: overview saved {saved} ms ({(saved * 100.0 / withoutOverview.ElapsedMs):0.0}% faster) for this zoomed-out read ({withoutOverview.ElapsedMs} ms without, {withOverview.ElapsedMs} ms with)."
            : $"Comparison: overview did not win on this run ({withoutOverview.ElapsedMs} ms without, {withOverview.ElapsedMs} ms with). This can happen on small rasters or warm OS cache.";
    }

    private void UpdateDetails()
    {
        var raster = new FileInfo(WorkingRasterPath);
        var overview = new FileInfo(OverviewPath);
        var lines = new List<string>
        {
            "RasterOverview sample", "", $"Mode: {_mode}", $"Load elapsed: {_elapsedMilliseconds} ms",
            $"Working raster: {WorkingRasterPath}", $"Raster file exists: {(raster.Exists ? "yes" : "no")}",
            $"Raster file size: {(raster.Exists ? raster.Length : 0)} bytes", $"Overview file: {OverviewPath}",
            $"Overview file exists: {(overview.Exists ? "yes" : "no")}", $"Overview file size: {(overview.Exists ? overview.Length : 0)} bytes", "",
            "Layer load options", "PrepareRasterOverviews = true/false", "RasterOverviewMinimumPixels = threshold",
            "RasterOverviewResampling = AVERAGE", "", "Workflow",
            "1. Reset Working Copy removes the generated .ovr file.",
            "2. Load Without Overview skips pyramid creation.",
            "3. Load With Overview forces pyramid creation.",
            "4. Run Downsample Benchmark performs 40 zoomed-out 128x64 reads in each mode.",
            "5. This sample raster is small; real gains become clearer on large rasters."
        };
        lines.Insert(15, "Benchmark");
        lines.Insert(16, string.IsNullOrEmpty(_lastBenchmarkText) ? "Run Downsample Benchmark after loading a raster." : _lastBenchmarkText);
        if (_withoutOverviewBenchmark is not null)
            lines.Add(BenchmarkText("Without overview", _withoutOverviewBenchmark));
        if (_withOverviewBenchmark is not null)
            lines.Add(BenchmarkText("With overview", _withOverviewBenchmark));
        if (_withoutOverviewBenchmark is not null && _withOverviewBenchmark is not null)
            lines.Add(ComparisonText(_withoutOverviewBenchmark, _withOverviewBenchmark));
        if (viewerControl.LayerCount > 0)
        {
            var info = viewerControl.GetLayerInfo(0);
            lines.InsertRange(10, new[] { $"Layer name: {info?.Name}", $"Layer path: {info?.Path}", "" });
        }
        detailsTextBox.Text = string.Join(Environment.NewLine, lines);
    }

    private static string BenchmarkText(string mode, BenchmarkResult result) =>
        $"{mode}: {result.Passes} reads to {result.TargetWidth}x{result.TargetHeight}, selected overview={result.SelectedOverview}, elapsed={result.ElapsedMs} ms";

    private sealed record BenchmarkResult(int Passes, int TargetWidth, int TargetHeight, int SelectedOverview, long ElapsedMs);

    private static void DeleteIfExists(string path) { if (File.Exists(path)) File.Delete(path); }
}
