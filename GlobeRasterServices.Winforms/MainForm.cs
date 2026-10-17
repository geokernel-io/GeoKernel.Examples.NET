using GeoKernel.NET.WinForms;

namespace GeoKernel.GlobeRasterServices.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelGlobeControl _globe = new() { Dock = DockStyle.Fill };
    private readonly ToolStripStatusLabel _status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly CancellationTokenSource _closing = new();
    private readonly System.Windows.Forms.Timer _errors = new() { Interval = 500 };
    private bool _loaded;

    public MainForm()
    {
        Text = "GeoKernel — Sagrada Ortho & DEM";
        ClientSize = new Size(1200, 800);
        MinimumSize = new Size(720, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        _globe.StartupCamera = new GlobeCamera(2.1744, 41.4036, 0, 2500, 1.32 * 180 / Math.PI, .6 * 180 / Math.PI);
        _globe.StartupOptions = new GlobeOptions { NetworkEnabled = true, AtmosphereEnabled = true };
        var statusBar = new StatusStrip { AutoSize = false, Height = 24, SizingGrip = false };
        statusBar.Items.Add(_status);
        Controls.Add(_globe);
        Controls.Add(statusBar);
        Report("Preparing globe…");
        _globe.GlobeReady += async (_, _) => await LoadRastersAsync();
        _errors.Tick += (_, _) => CheckErrors();
    }

    private async Task LoadRastersAsync()
    {
        try
        {
            var globe = _globe.Globe;
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
        var globe = _globe.Globe;
        try { await globe.WaitForJobAsync(job, _closing.Token); }
        finally { if (!globe.IsDisposed) globe.ForgetJob(job); }
    }

    private void CheckErrors()
    {
        if (!_loaded || _closing.IsCancellationRequested) return;
        try
        {
            var globe = _globe.Globe;
            string? error = globe.GetState().Errors.Values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
            Report(error ?? $"Sagrada Familia orthophoto + DEM | Elevation 1x | {globe.GetAttribution()}");
        }
        catch (Exception error) { Report(error.Message); }
    }

    private void Report(string message)
    {
        if (IsDisposed || _closing.IsCancellationRequested) return;
        _status.Text = message;
        _status.ToolTipText = message;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _closing.Cancel();
            _errors.Dispose();
        }
        base.Dispose(disposing);
    }
}

