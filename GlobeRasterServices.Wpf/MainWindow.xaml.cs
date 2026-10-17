using System.Windows;
using System.Windows.Threading;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GlobeRasterServices.Wpf;

public partial class MainWindow : Window
{
    private readonly CancellationTokenSource _closing = new();
    private readonly DispatcherTimer _errors = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private bool _loaded;

    public MainWindow()
    {
        InitializeComponent();
        GlobeView.StartupCamera = new GlobeCamera(2.1744, 41.4036, 0, 2500, 1.32 * 180 / Math.PI, .6 * 180 / Math.PI);
        GlobeView.StartupOptions = new GlobeOptions { NetworkEnabled = true, AtmosphereEnabled = true };
        _errors.Tick += (_, _) => CheckErrors();
    }

    private async void GlobeReady(object? sender, EventArgs e) => await LoadRastersAsync();

    private async Task LoadRastersAsync()
    {
        try
        {
            var globe = GlobeView.Globe;
            int esri = globe.GetBasemaps().FirstOrDefault(map => map.Name == "Esri Satellite")?.Id ?? -1;
            globe.SetBasemap(esri);
            var progress = new Progress<string>(Report);
            string ortho = await SampleData.EnsureRasterAsync("sagrada_familia_ortophoto",
                "43a09656ca07490e91a48b413428e8c093d392d3a63549e997c9b4b20ffa9f40", progress, _closing.Token);
            string dem = await SampleData.EnsureRasterAsync("sagrada_familia_terrain",
                "69ed86c686b2500a70921920387afcb8993f3fe042e2a682932f86b7c6d5127c", progress, _closing.Token);
            _closing.Token.ThrowIfCancellationRequested();
            Report("Loading Sagrada orthophoto…");
            await WaitAsync(globe.LoadRaster(new GlobeRasterRequest("ortho", ortho)));
            Report("Loading Sagrada DEM…");
            await WaitAsync(globe.LoadDem(new GlobeDemRequest("dem", dem)));
            // Keep DEM elevation active without coloring over the orthophoto.
            var demStats = globe.GetLayerState("dem").Statistics;
            globe.SetDemStyle(false, demStats.GetProperty("minimum").GetDouble(), demStats.GetProperty("maximum").GetDouble());
            globe.Options = globe.Options with { TerrainEnabled = true, TerrainExaggeration = 1 };
            globe.FocusLayer("ortho");
            var camera = globe.Camera with
            {
                DistanceMetres = globe.Camera.DistanceMetres * .6,
                PitchDegrees = 1.32 * 180 / Math.PI,
                HeadingDegrees = .6 * 180 / Math.PI
            };
            globe.Camera = camera;
            globe.SetHomeCamera(camera);
            _loaded = true;
            CheckErrors();
            _errors.Start();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
    }

    private async Task WaitAsync(ulong job)
    {
        var globe = GlobeView.Globe;
        try { await globe.WaitForJobAsync(job, _closing.Token); }
        finally { if (!globe.IsDisposed) globe.ForgetJob(job); }
    }

    private void CheckErrors()
    {
        if (!_loaded || _closing.IsCancellationRequested) return;
        try
        {
            var globe = GlobeView.Globe;
            string? error = globe.GetState().Errors.Values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            Report(error ?? $"Sagrada Familia orthophoto + DEM | Elevation 1x | {globe.GetAttribution()}");
        }
        catch (Exception error) { Report(error.Message); }
    }

    private void Report(string message)
    {
        if (!_closing.IsCancellationRequested) Status.Text = message;
    }

    protected override void OnClosed(EventArgs e)
    {
        _closing.Cancel();
        _errors.Stop();
        GlobeView.Dispose();
        base.OnClosed(e);
    }
}

