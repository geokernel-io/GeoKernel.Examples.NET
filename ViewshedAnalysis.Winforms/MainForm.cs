using GeoKernel.NET.WinForms;

namespace GeoKernel.ViewshedAnalysis.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewer3DControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly Button _reload = Button("Reload sample"), _cancel = Button("Cancel loading");
    private readonly Button _calculate = Button("Calculate"), _pause = Button("Pause calculation");
    private readonly Button _clear = Button("Clear observer and result");
    private readonly CheckBox _pick = Check("Select observer on terrain", true);
    private readonly CheckBox _imagery = Check("Show orthophoto", true);
    private readonly CheckBox _overlay = Check("Show result on terrain", true);
    private readonly NumericUpDown _eye = Number(.1m, 100, 1.7m);
    private readonly NumericUpDown _target = Number(0, 100, 1.7m);
    private readonly NumericUpDown _radius = Number(10, 2000, 300);
    private readonly NumericUpDown _spacing = Number(1, 50, 5);
    private readonly NumericUpDown _height = Number(.25m, 10, 1);
    private readonly ComboBox _grid = new() { Width = 330, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _location = Caption("No observer selected."), _result = Caption("Load terrain to begin.");
    private readonly ViewshedMap _map = new() { Width = 330, Height = 280 };
    private readonly ProgressBar _analysisProgress = new() { Width = 330 };
    private readonly ToolStripStatusLabel _status = new() { Text = "Preparing sample…" };
    private readonly System.Windows.Forms.Timer _poll = new() { Interval = 150 };
    private CancellationTokenSource? _load;
    private bool _ready, _updating;
    private string? _snapshot;

    public MainForm()
    {
        Text = "ViewshedAnalysis — GeoKernel";
        ClientSize = new Size(1400, 900);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        _grid.Items.AddRange(["17 × 17 — fast", "33 × 33 — balanced", "65 × 65 — detailed"]);
        _grid.SelectedIndex = 1;
        _height.Increment = .25m;
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Right, Width = 365, Padding = new Padding(12),
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
        };
        var reset = Button("Reset view");
        panel.Controls.AddRange([
            Caption("Sagrada Família — viewshed analysis"), _reload, _cancel, _imagery,
            Caption("Select observer, then click terrain. The marker shows eye height."), _pick, _location,
            Caption("Observer above terrain (m)"), _eye, Caption("Target above terrain (m)"), _target,
            Caption("Radius (m)"), _radius, Caption("Ray sample spacing (m)"), _spacing,
            Caption("Grid"), _grid, _calculate, _pause, _clear, _analysisProgress,
            _map, _overlay, Caption("Green: visible | Red: blocked\nGrey: unknown / NoData | Pale grey: pending"), _result,
            Caption("Vertical exaggeration"), _height, reset,
            Caption("Terrain-only sampled visibility; buildings, vegetation and atmospheric refraction are excluded. The DEM mesh has 512 samples per side. Small obstacles between samples may be missed. Vertical datum is unverified. Exaggeration changes only the view.")
        ]);
        var status = new StatusStrip();
        status.Items.Add(_status);
        Controls.Add(_viewer); Controls.Add(panel); Controls.Add(status);
        _reload.Click += async (_, _) => await LoadSampleAsync();
        _cancel.Click += (_, _) => _load?.Cancel();
        _pick.CheckedChanged += (_, _) => ApplyCapture();
        _imagery.CheckedChanged += (_, _) => { if (_ready) _viewer.ImageryVisible = _imagery.Checked; };
        _overlay.CheckedChanged += (_, _) => Safe(() => _viewer.SetViewshedOverlay(_overlay.Checked));
        foreach (var number in new[] { _eye, _target, _radius, _spacing })
            number.ValueChanged += (_, _) => Safe(Configure);
        _grid.SelectedIndexChanged += (_, _) => Safe(Configure);
        _calculate.Click += (_, _) => Action(ViewshedAction.Calculate);
        _pause.Click += (_, _) => Action(ViewshedAction.Pause);
        _clear.Click += (_, _) => Action(ViewshedAction.Clear);
        _height.ValueChanged += (_, _) => _viewer.HeightScale = (float)_height.Value;
        reset.Click += (_, _) => _viewer.ResetCamera();
        _poll.Tick += (_, _) =>
        {
            try { RefreshResult(); }
            catch (Exception ex) { _poll.Stop(); _status.Text = ex.Message; }
        };
        SetBusy(false);
        Shown += async (_, _) => await LoadSampleAsync();
        FormClosing += (_, _) => { _poll.Stop(); _load?.Cancel(); };
    }

    private static Button Button(string text) => new() { Text = text, Width = 330, AutoSize = true };
    private static CheckBox Check(string text, bool value) => new() { Text = text, Checked = value, Width = 330 };
    private static NumericUpDown Number(decimal min, decimal max, decimal value) => new()
    {
        Minimum = min, Maximum = max, Value = value, DecimalPlaces = 2, Width = 330
    };
    private static Label Caption(string text) => new()
    {
        Text = text, AutoSize = true, MaximumSize = new Size(330, 0), Margin = new Padding(0, 7, 0, 4)
    };
    private void Configure() => _viewer.ConfigureViewshed((double)_eye.Value, (double)_target.Value,
        (double)_radius.Value, (double)_spacing.Value, new[] { 17, 33, 65 }[_grid.SelectedIndex]);

    private void Safe(System.Action action)
    {
        if (!_ready || _load != null || _updating) return;
        try { action(); RefreshResult(); }
        catch (Exception ex) { _status.Text = ex.Message; }
    }
    private void ApplyCapture() => Safe(() => _viewer.SetViewshedCapture(_pick.Checked));
    private void Action(ViewshedAction action) => Safe(() => _viewer.RunViewshedAction(action));

    private void RefreshResult()
    {
        if (!_ready || IsDisposed) return;
        var result = _viewer.GetViewshed();
        string snapshot = System.Text.Json.JsonSerializer.Serialize(result);
        if (_snapshot == snapshot) return;
        _snapshot = snapshot;
        _updating = true;
        try { _pick.Checked = result.Capture; }
        finally { _updating = false; }
        _location.Text = result.HasObserver
            ? $"Observer: {result.Longitude:F6}° E, {result.Latitude:F6}° N" : "No observer selected.";
        _map.Result = result;
        _analysisProgress.Value = result.Cells.Count == 0 ? 0 : Math.Clamp(100 * result.Processed / result.Cells.Count, 0, 100);
        int Count(ViewshedCell cell) => result.Cells.Count(value => value == cell);
        _result.Text = $"{result.Message}\nVisible: {Count(ViewshedCell.Clear)} | Blocked: {Count(ViewshedCell.Blocked)}\n" +
            $"Unknown: {Count(ViewshedCell.Unknown)} | Pending: {Count(ViewshedCell.Pending)}\n" +
            $"Grid spacing: {result.CellSpacing:F1} m\nMaximum ray spacing: {result.MaxRaySpacing:F1} m\n{result.OverlayMessage}";
        _calculate.Text = result.Complete ? "Recalculate" : result.Cells.Count > 0 ? "Resume calculation" : "Calculate";
        _calculate.Enabled = _load == null && result.HasObserver && !result.Running;
        _pause.Enabled = _load == null && result.Running;
        _clear.Enabled = _load == null && result.HasObserver;
    }
    private void SetBusy(bool busy)
    {
        _reload.Enabled = !busy; _cancel.Enabled = busy;
        foreach (Control control in new Control[] { _pick, _imagery, _overlay, _eye, _target, _radius, _spacing, _grid })
            control.Enabled = _ready && !busy;
        _calculate.Enabled = _pause.Enabled = _clear.Enabled = false;
        _snapshot = null;
        if (_ready && !busy) { RefreshResult(); _poll.Start(); }
        else _poll.Stop();
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        SetBusy(true);
        try
        {
            if (_ready)
            {
                _viewer.SetViewshedCapture(false);
                if (_viewer.GetViewshed().Running) _viewer.RunViewshedAction(ViewshedAction.Pause);
            }
            var progress = new Progress<string>(message => { if (!IsDisposed) _status.Text = message; });
            string dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            string imagery = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            if (IsDisposed) return;
            _status.Text = "Loading terrain…";
            await _viewer.LoadTerrainAndImageryAsync(dem, imagery, 512, cancellation.Token);
            if (IsDisposed) return;
            _ready = true;
            Configure();
            _viewer.ImageryVisible = _imagery.Checked;
            _viewer.SetViewshedOverlay(_overlay.Checked);
            _viewer.SetViewshedCapture(true);
            _status.Text = "Ready — select an observer on terrain.";
        }
        catch (OperationCanceledException) { if (!IsDisposed) _status.Text = "Loading cancelled."; }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _status.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "ViewshedAnalysis", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _load = null;
            if (!IsDisposed)
            {
                try { SetBusy(false); }
                catch (Exception ex) { _poll.Stop(); _status.Text = ex.Message; }
            }
        }
    }
    protected override void Dispose(bool disposing)
    {
        if (disposing) _poll.Dispose();
        base.Dispose(disposing);
    }
}
