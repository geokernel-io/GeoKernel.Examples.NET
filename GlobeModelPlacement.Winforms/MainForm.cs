using GeoKernel.NET.WinForms;

namespace GeoKernel.GlobeModelPlacement.Winforms;

public sealed class MainForm : Form
{
    private const string LayerId = "sagrada-familia";
    private readonly GeoKernelGlobeControl _globe = new() { Dock = DockStyle.Fill };
    private readonly ToolStrip _toolbar = new() { GripStyle = ToolStripGripStyle.Hidden, Enabled = false };
    private readonly NumericUpDown _scale = new() { Minimum = .01m, Maximum = 10, DecimalPlaces = 2, Increment = .1m, Value = 1, Width = 65 };
    private readonly NumericUpDown _height = new() { Minimum = -100, Maximum = 10000, DecimalPlaces = 3, Value = 14.089m, Width = 85 };
    private readonly ToolStripButton _rotate = new("Rotate 30°");
    private readonly ToolStripButton _remove = new("Remove");
    private readonly ToolStripButton _restore = new("Restore") { Enabled = false };
    private readonly ToolStripButton _atmosphere = new("Atmosphere") { CheckOnClick = true, Checked = true };
    private readonly ToolStripButton _esri = new("Esri") { CheckOnClick = true, Checked = true };
    private readonly ToolStripStatusLabel _status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly CancellationTokenSource _closing = new();
    private NET.WinForms.GlobeModelPlacement _placement = new(LayerId, 2.1744, 41.4036) { Name = "Sagrada Familia", Height = 14.089 };
    private string? _modelPath;
    private int _esriId = -1;
    private bool _present;

    public MainForm()
    {
        Text = "GeoKernel — Sagrada Familia Model";
        ClientSize = new Size(1200, 800);
        MinimumSize = new Size(950, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        _globe.StartupCamera = ModelCamera();
        _globe.StartupOptions = new GlobeOptions { NetworkEnabled = true, AtmosphereEnabled = true };
        var zoom = new ToolStripButton("Zoom to model");
        _toolbar.Items.AddRange([zoom, _rotate, new ToolStripLabel("Scale:"), new ToolStripControlHost(_scale),
            new ToolStripLabel("Anchor height (m):"), new ToolStripControlHost(_height),
            _remove, _restore, _atmosphere, _esri]);
        var statusBar = new StatusStrip { AutoSize = false, Height = 24, SizingGrip = false };
        statusBar.Items.Add(_status);
        Controls.Add(_globe);
        Controls.Add(_toolbar);
        Controls.Add(statusBar);
        Report("Preparing globe…");
        _globe.GlobeReady += async (_, _) => await LoadModelAsync();
        zoom.Click += (_, _) => FocusModel();
        _rotate.Click += async (_, _) => await PlaceModelAsync(_placement with { Heading = (_placement.Heading + 30) % 360 });
        _scale.ValueChanged += async (_, _) => await PlaceModelAsync(_placement with { Scale = (double)_scale.Value });
        _height.ValueChanged += async (_, _) => await PlaceModelAsync(_placement with { Height = (double)_height.Value });
        _remove.Click += (_, _) =>
        {
            _globe.Globe.RemoveLayer(LayerId);
            _present = false;
            UpdateControls();
            ShowSummary();
        };
        _restore.Click += async (_, _) => await LoadModelAsync();
        _atmosphere.Click += (_, _) =>
            _globe.Globe.Options = _globe.Globe.Options with { AtmosphereEnabled = _atmosphere.Checked };
        _esri.Click += (_, _) =>
        {
            _globe.Globe.SetBasemap(_esri.Checked ? _esriId : -1);
            ShowSummary();
        };
    }

    private GlobeCamera ModelCamera() => new(_placement.Longitude, _placement.Latitude,
        _placement.Height + 65 * _placement.Scale, 550 * _placement.Scale, 1.15 * 180 / Math.PI, .6 * 180 / Math.PI);

    private void FocusModel()
    {
        _globe.Globe.Camera = ModelCamera();
        _globe.Globe.SetHomeCamera(ModelCamera());
    }

    private async Task LoadModelAsync()
    {
        _toolbar.Enabled = false;
        try
        {
            var globe = _globe.Globe;
            if (_modelPath is null)
            {
                _esriId = globe.GetBasemaps().FirstOrDefault(map => map.Name == "Esri Satellite")?.Id ?? -1;
                globe.SetBasemap(_esriId);
                _modelPath = await SampleData.EnsureModelAsync(new Progress<string>(Report), _closing.Token);
            }
            _closing.Token.ThrowIfCancellationRequested();
            Report("Loading Sagrada Familia model…");
            await WaitAsync(globe.LoadModel(new GlobeModelRequest(LayerId, _modelPath, _placement.Longitude, _placement.Latitude)
            {
                Name = _placement.Name, Height = _placement.Height, Scale = _placement.Scale, Heading = _placement.Heading
            }));
            _present = true;
            FocusModel();
            await WaitForUploadAsync();
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
        finally { if (!_closing.IsCancellationRequested) UpdateControls(); }
    }

    private async Task PlaceModelAsync(NET.WinForms.GlobeModelPlacement next)
    {
        if (!_present || !_toolbar.Enabled) return;
        _toolbar.Enabled = false;
        try
        {
            Report("Updating model placement…");
            await WaitAsync(_globe.Globe.SetModelPlacement(next));
            _placement = next;
            FocusModel();
            await WaitForUploadAsync();
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
        finally
        {
            if (!_closing.IsCancellationRequested)
            {
                _scale.Value = (decimal)_placement.Scale;
                _height.Value = (decimal)_placement.Height;
                UpdateControls();
            }
        }
    }

    private async Task WaitAsync(ulong job)
    {
        var globe = _globe.Globe;
        try { await globe.WaitForJobAsync(job, _closing.Token); }
        finally { if (!globe.IsDisposed) globe.ForgetJob(job); }
    }

    private async Task WaitForUploadAsync()
    {
        Report("Uploading model to the GPU…");
        while (true)
        {
            _closing.Token.ThrowIfCancellationRequested();
            var state = _globe.Globe.GetState();
            if (state.Errors.TryGetValue("scene", out string? error) && !string.IsNullOrWhiteSpace(error))
                throw new InvalidOperationException(error);
            if (!state.SceneUploadPending && state.SceneVerticesDrawn > 0) return;
            await Task.Delay(50, _closing.Token);
        }
    }

    private void UpdateControls()
    {
        _toolbar.Enabled = true;
        _rotate.Enabled = _scale.Enabled = _height.Enabled = _remove.Enabled = _present;
        _restore.Enabled = !_present;
        _esri.Enabled = _esriId >= 0;
    }

    private void ShowSummary() => Report($"Sagrada Familia | {(_present ? "Approximate placement" : "Model removed — click Restore")} | Model: PeeJaa | {_globe.Globe.GetAttribution()}");

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

