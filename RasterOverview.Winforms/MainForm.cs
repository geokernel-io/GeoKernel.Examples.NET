using System.Diagnostics;
using GeoKernel.NET.WinForms;

namespace GeoKernel.RasterOverview.Winforms;

public sealed partial class MainForm : Form
{
    private string _sourceRasterPath = string.Empty;
    private string _mode = "Reset";
    private long _elapsedMilliseconds;
    private GeoKernelRasterOverviewBenchmark? _withoutOverviewBenchmark;
    private GeoKernelRasterOverviewBenchmark? _withOverviewBenchmark;
    private string _lastBenchmarkText = string.Empty;
    private static string WorkingDirectory => Path.Combine(AppContext.BaseDirectory, "RasterOverviewData");
    private static string WorkingRasterPath => Path.Combine(WorkingDirectory, "world_8km_overview_test.tif");
    private static string OverviewPath => WorkingRasterPath + ".ovr";

    public MainForm() => InitializeComponent();

    private async void MainForm_Shown(object? sender, EventArgs e)
    {
        EnsureDetailsPanelWidth();
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        statusLabel.Text = "Preparing sample data...";
        progressBar.Style = ProgressBarStyle.Blocks;
        progressBar.Value = 0;
        _sourceRasterPath = await SampleData.EnsureFileAsync(
            "world_8km_tif.zip", "world_8km_tif", "world_8km.tif", "World GeoTIFF", this);
        if (!string.IsNullOrWhiteSpace(_sourceRasterPath)) ResetWorkingCopy();
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        if (IsHandleCreated) EnsureDetailsPanelWidth();
    }

    private void EnsureDetailsPanelWidth()
    {
        const int detailsWidth = 420;
        const int minimumMapWidth = 300;
        if (splitContainer.Width <= detailsWidth + minimumMapWidth + splitContainer.SplitterWidth)
            return;
        splitContainer.Panel2Collapsed = false;
        splitContainer.SplitterDistance = splitContainer.Width - detailsWidth - splitContainer.SplitterWidth;
    }

    private void resetButton_Click(object? sender, EventArgs e) => ResetWorkingCopy();
    private void loadWithoutButton_Click(object? sender, EventArgs e) => LoadRaster(false);
    private void loadWithButton_Click(object? sender, EventArgs e) => LoadRaster(true);
    private void benchmarkButton_Click(object? sender, EventArgs e) => ShowBenchmarkComparison();
    private void fullExtentButton_Click(object? sender, EventArgs e) => viewerControl.FullExtent();

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
            statusLabel.Text = "Working copy reset. Overview file removed.";
            UpdateDetails();
            return true;
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Reset failed.";
            MessageBox.Show(this, ex.Message, "RasterOverview", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            statusLabel.Text = mode;
            UseWaitCursor = true;
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
                if (!string.IsNullOrWhiteSpace(p.Status)) statusLabel.Text = p.Status;
                Application.DoEvents();
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
            statusLabel.Text = $"{mode} finished in {_elapsedMilliseconds} ms.";
            UpdateDetails();
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Load failed.";
            MessageBox.Show(this, ex.Message, "RasterOverview", MessageBoxButtons.OK, MessageBoxIcon.Error);
            UpdateDetails();
        }
        finally { UseWaitCursor = false; }
    }

    private void ShowBenchmarkComparison()
    {
        if (viewerControl.LayerCount == 0)
        {
            statusLabel.Text = "Load a raster first.";
            return;
        }

        try
        {
            UseWaitCursor = true;
            statusLabel.Text = "Running downsample benchmark...";
            Application.DoEvents();
            var result = RunCurrentBenchmark();
            _lastBenchmarkText = BenchmarkText(_mode, result);
            if (_mode == "Load Without Overview")
                _withoutOverviewBenchmark = result;
            else if (_mode == "Load With Overview")
                _withOverviewBenchmark = result;

            statusLabel.Text = _withoutOverviewBenchmark is not null && _withOverviewBenchmark is not null
                ? ComparisonText(_withoutOverviewBenchmark, _withOverviewBenchmark)
                : _lastBenchmarkText;
            UpdateDetails();
        }
        catch (Exception ex)
        {
            statusLabel.Text = "Benchmark failed.";
            MessageBox.Show(this, ex.Message, "RasterOverview", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally { UseWaitCursor = false; }
    }

    private GeoKernelRasterOverviewBenchmark RunCurrentBenchmark()
    {
        var diagnostics = viewerControl.GetRasterOverviewDiagnostics(0, true);
        var benchmark = diagnostics?.Benchmark;
        if (benchmark is null || !benchmark.Valid)
            throw new InvalidOperationException(benchmark?.ErrorMessage ?? "Raster overview benchmark returned no result.");
        return benchmark;
    }

    private static string ComparisonText(GeoKernelRasterOverviewBenchmark withoutOverview, GeoKernelRasterOverviewBenchmark withOverview)
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

    private static string BenchmarkText(string mode, GeoKernelRasterOverviewBenchmark result) =>
        $"{mode}: {result.Passes} reads to {result.TargetWidth}x{result.TargetHeight}, selected overview={result.SelectedOverview}, elapsed={result.ElapsedMs} ms";

    private static void DeleteIfExists(string path) { if (File.Exists(path)) File.Delete(path); }
}
