using GeoKernel.NET.WinForms;

namespace GeoKernel.GlobeBuildings.Winforms;

public sealed class MainForm : Form
{
    private const string LayerId = "buildings";
    private const string BuildingColor = "#e8d6b4";
    private readonly GeoKernelGlobeControl _globe = new() { Dock = DockStyle.Fill };
    private readonly ToolStrip _toolbar = new() { GripStyle = ToolStripGripStyle.Hidden, Enabled = false };
    private readonly ToolStripButton _extrude = new("Extrude") { CheckOnClick = true };
    private readonly NumericUpDown _height = new() { Minimum = 1, Maximum = 300, Value = 18, Width = 65 };
    private readonly ToolStripButton _atmosphere = new("Atmosphere") { CheckOnClick = true, Checked = true };
    private readonly ToolStripButton _esri = new("Esri") { CheckOnClick = true, Checked = true };
    private readonly ToolStripStatusLabel _status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly CancellationTokenSource _closing = new();
    private GlobeCamera? _home;
    private int _esriId = -1;
    private bool _loaded;

    public MainForm()
    {
        Text = "GeoKernel — Sagrada Familia Buildings";
        ClientSize = new Size(1200, 800);
        MinimumSize = new Size(720, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        _globe.StartupCamera = new GlobeCamera(2.1744, 41.4036, 0, 2500, 75.63, 34.38);
        _globe.StartupOptions = new GlobeOptions { NetworkEnabled = true, AtmosphereEnabled = true };

        var zoom = new ToolStripButton("Zoom to buildings");
        var heightHost = new ToolStripControlHost(_height)
        {
            ToolTipText = "Display height in metres; this source has no measured building heights."
        };
        _toolbar.Items.AddRange([_extrude, new ToolStripLabel("Display height (m):"), heightHost,
            zoom, _atmosphere, _esri]);
        var statusBar = new StatusStrip { AutoSize = false, Height = 24, SizingGrip = false };
        statusBar.Items.Add(_status);
        Controls.Add(_globe);
        Controls.Add(_toolbar);
        Controls.Add(statusBar);
        Report("Preparing globe…");

        _globe.GlobeReady += async (_, _) => await LoadBuildingsAsync();
        _extrude.Click += async (_, _) => await ToggleExtrusionAsync();
        zoom.Click += (_, _) => { if (_home is not null) _globe.Globe.Camera = _home; };
        _atmosphere.Click += (_, _) =>
            _globe.Globe.Options = _globe.Globe.Options with { AtmosphereEnabled = _atmosphere.Checked };
        _esri.Click += (_, _) =>
        {
            _globe.Globe.SetBasemap(_esri.Checked ? _esriId : -1);
            ShowSummary();
        };
    }

    private async Task LoadBuildingsAsync()
    {
        try
        {
            var globe = _globe.Globe;
            _esriId = globe.GetBasemaps().FirstOrDefault(map => map.Name == "Esri Satellite")?.Id ?? -1;
            globe.SetBasemap(_esriId);
            string path = await SampleData.EnsureBuildingsAsync(new Progress<string>(Report), _closing.Token);
            _closing.Token.ThrowIfCancellationRequested();
            Report("Loading Sagrada Familia buildings…");
            await WaitAsync(globe.LoadVector(new GlobeVectorRequest(LayerId, path)
            {
                SourceCrs = "EPSG:25831",
                Style = new GlobeVectorStyle { Fill = true, Labels = false, BaseColor = BuildingColor }
            }));
            globe.SetLayerOpacity(LayerId, 0.65);
            globe.FocusLayer(LayerId);
            _home = globe.Camera with
            {
                DistanceMetres = globe.Camera.DistanceMetres * 0.28,
                PitchDegrees = 1.32 * 180 / Math.PI,
                HeadingDegrees = 0.60 * 180 / Math.PI
            };
            globe.Camera = _home;
            globe.SetHomeCamera(_home);
            _loaded = true;
            _toolbar.Enabled = true;
            _esri.Enabled = _esriId >= 0;
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
    }

    private async Task ToggleExtrusionAsync()
    {
        if (!_loaded || !_toolbar.Enabled) return;
        _toolbar.Enabled = false;
        try
        {
            Report(_extrude.Checked ? "Extruding buildings…" : "Restoring footprints…");
            // Match Qt: no DEM in this example, so bases lie on the reference sphere.
            await WaitAsync(_globe.Globe.ConfigureBuildings(new GlobeBuildingsRequest(LayerId, GlobeBuildingBase.ReferenceSphere)
            {
                Enabled = _extrude.Checked,
                DefaultHeight = (double)_height.Value,
                BuildingColors = new GlobeBuildingColors { Mode = "uniform", Fallback = BuildingColor }
            }));
            if (_extrude.Checked)
            {
                Report("Uploading 3D buildings to the GPU…");
                // A completed geometry job does not mean the incremental GPU upload is finished.
                while (true)
                {
                    _closing.Token.ThrowIfCancellationRequested();
                    var render = _globe.Globe.GetState();
                    if (render.Errors.TryGetValue("scene", out string? error) && !string.IsNullOrWhiteSpace(error))
                        throw new InvalidOperationException(error);
                    if (!render.SceneUploadPending && render.SceneVerticesDrawn > 0) break;
                    await Task.Delay(50, _closing.Token);
                }
            }
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
        finally
        {
            if (!_closing.IsCancellationRequested)
            {
                _extrude.Checked = _globe.Globe.GetLayerState(LayerId).Extruded;
                _height.Enabled = !_extrude.Checked;
                _toolbar.Enabled = true;
            }
        }
    }

    private async Task WaitAsync(ulong job)
    {
        var globe = _globe.Globe;
        try { await globe.WaitForJobAsync(job, _closing.Token); }
        finally { if (!globe.IsDisposed) globe.ForgetJob(job); }
    }

    private void ShowSummary()
    {
        var layer = _globe.Globe.GetLayerState(LayerId);
        var stats = layer.Statistics;
        string message = layer.Extruded
            ? $"{stats.GetProperty("extrudedPolygons")} extruded polygons | Display height {_height.Value} m (not measured) | Click Extrude for footprints"
            : $"{stats.GetProperty("features")} buildings | {stats.GetProperty("outlineOnly")} outline-only | Click Extrude for 3D";
        Report($"{message} | {_globe.Globe.GetAttribution()}");
    }

    private void Report(string message)
    {
        if (IsDisposed || _closing.IsCancellationRequested) return;
        _status.Text = message;
        _status.ToolTipText = message;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _closing.Cancel();
        base.Dispose(disposing);
    }
}
