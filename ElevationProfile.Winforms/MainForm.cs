using GeoKernel.NET.WinForms;

namespace GeoKernel.ElevationProfile.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewer3DControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly Button _reload = new() { Text = "Reload sample", Width = 300 };
    private readonly Button _cancel = new() { Text = "Cancel loading", Width = 300, Enabled = false };
    private readonly NumericUpDown _spacing = new() { Minimum = 1, Maximum = 100, Value = 10, Width = 300 };
    private readonly CheckBox _capture = new() { Text = "Add points on click", Checked = true, Width = 300 };
    private readonly CheckBox _imagery = new() { Text = "Show orthophoto", Checked = true, Width = 300 };
    private readonly NumericUpDown _height = new()
    {
        Minimum = .25m, Maximum = 10, DecimalPlaces = 2, Increment = .25m, Value = 1, Width = 300
    };
    private readonly Button _finish = new() { Text = "Finish route", Width = 300 };
    private readonly Button _undo = new() { Text = "Undo last point (Backspace)", Width = 300 };
    private readonly Button _clear = new() { Text = "Clear route (Esc)", Width = 300 };
    private readonly Label _result = Caption("Load terrain to draw a profile.");
    private readonly ProfileChart _chart = new() { Dock = DockStyle.Bottom, Height = 240 };
    private readonly ToolStripStatusLabel _status = new() { Text = "Preparing sample…" };
    private readonly ToolStripProgressBar _progress = new() { Style = ProgressBarStyle.Marquee };
    private readonly System.Windows.Forms.Timer _poll = new() { Interval = 150 };
    private CancellationTokenSource? _load;
    private bool _ready;
    private string? _lastSnapshot;

    public MainForm()
    {
        Text = "ElevationProfile — GeoKernel";
        ClientSize = new Size(1300, 850);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        KeyPreview = true;
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 335, Padding = new Padding(12),
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
        };
        var reset = new Button { Text = "Reset view", Width = 300 };
        panel.Controls.AddRange([
            Caption("Sagrada Família — elevation profile"), _reload, _cancel, _imagery,
            Caption("Enable Add points, then click two or more terrain positions. Dragging still navigates."),
            Caption("Sample spacing (m)"), _spacing, _capture, _finish, _undo, _clear, _result,
            Caption("Vertical exaggeration"), _height, reset,
            Caption("Approximate terrain mesh profile; maximum 64 route points and 4096 samples. Gaps are not joined. Finer spacing cannot add DEM detail. Exaggeration does not change results. DEM datum is unverified."),
            Caption("Left drag: pan | Wheel/right drag: zoom | Middle/Ctrl+left drag: orbit")
        ]);
        var status = new StatusStrip();
        status.Items.AddRange([_status, _progress]);
        Controls.Add(_viewer);
        Controls.Add(_chart);
        Controls.Add(panel);
        Controls.Add(status);
        SetBusy(false);
        _reload.Click += async (_, _) => await LoadSampleAsync();
        _cancel.Click += (_, _) => _load?.Cancel();
        _imagery.CheckedChanged += (_, _) => { if (_ready) _viewer.ImageryVisible = _imagery.Checked; };
        _height.ValueChanged += (_, _) => _viewer.HeightScale = (float)_height.Value;
        reset.Click += (_, _) => _viewer.ResetCamera();
        _spacing.ValueChanged += (_, _) => ApplyMode();
        _capture.CheckedChanged += (_, _) => ApplyMode();
        _finish.Click += (_, _) => _capture.Checked = false;
        _undo.Click += (_, _) => EditPoints(true);
        _clear.Click += (_, _) => EditPoints(false);
        KeyDown += (_, e) =>
        {
            if (_ready && _load == null && (e.KeyCode == Keys.Escape || e.KeyCode == Keys.Back))
            {
                EditPoints(e.KeyCode == Keys.Back);
                e.Handled = e.SuppressKeyPress = true;
            }
        };
        _poll.Tick += (_, _) =>
        {
            try { RefreshProfile(); }
            catch (Exception ex) { _poll.Stop(); _status.Text = ex.Message; }
        };
        Shown += async (_, _) => await LoadSampleAsync();
        FormClosing += (_, _) => { _poll.Stop(); _load?.Cancel(); };
    }

    private static Label Caption(string text) => new()
    {
        Text = text, AutoSize = true, MaximumSize = new Size(300, 0), Margin = new Padding(0, 8, 0, 6)
    };

    private void ApplyMode()
    {
        if (!_ready || _load != null) return;
        _viewer.SetProfileMode(_capture.Checked, (double)_spacing.Value);
        RefreshProfile();
    }

    private void EditPoints(bool undo)
    {
        if (!_ready || _load != null) return;
        _viewer.EditProfile(undo);
        RefreshProfile();
    }

    private void RefreshProfile()
    {
        if (!_ready) return;
        var measurement = _viewer.GetProfile();
        string snapshot = System.Text.Json.JsonSerializer.Serialize(measurement);
        if (snapshot == _lastSnapshot) return;
        _lastSnapshot = snapshot;
        _chart.Profile = measurement;
        int count = measurement.PointCount;
        _result.Text = $"{count} route points\nSamples: {measurement.Processed}/{measurement.Samples.Count}\n" +
            $"Horizontal: {measurement.Horizontal:F1} m\nAscent: {measurement.Ascent:F1} m\n" +
            $"Descent: {measurement.Descent:F1} m\nMaximum interval: {measurement.MaxInterval:F1} m\n" +
            $"NoData: {measurement.Missing}\n{measurement.Message}";
        _undo.Enabled = _clear.Enabled = _load == null && count > 0;
        _finish.Enabled = _load == null && measurement.Capture && count >= 2;
    }
    private void SetBusy(bool busy)
    {
        _reload.Enabled = !busy;
        _cancel.Enabled = busy;
        _progress.Visible = busy;
        _spacing.Enabled = _capture.Enabled = _imagery.Enabled = _ready && !busy;
        _finish.Enabled = _undo.Enabled = _clear.Enabled = false;
        _lastSnapshot = null;
        if (_ready && !busy) { RefreshProfile(); _poll.Start(); }
        else _poll.Stop();
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        SetBusy(true);
        if (_ready) _viewer.SetProfileMode(false, (double)_spacing.Value);
        try
        {
            var progress = new Progress<string>(message => { if (!IsDisposed) _status.Text = message; });
            string dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            string imagery = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            if (IsDisposed) return;
            _status.Text = "Loading terrain…";
            await _viewer.LoadTerrainAndImageryAsync(dem, imagery, 512, cancellation.Token);
            if (IsDisposed) return;
            _ready = true;
            _viewer.ImageryVisible = _imagery.Checked;
            _status.Text = "Ready — click terrain to draw a profile.";
        }
        catch (OperationCanceledException) { if (!IsDisposed) _status.Text = "Loading cancelled."; }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _status.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "ElevationProfile", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _load = null;
            if (!IsDisposed)
            {
                // Preserve existing points if a replacement load failed or was cancelled.
                if (_ready) ApplyMode();
                SetBusy(false);
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _poll.Dispose();
        base.Dispose(disposing);
    }
}
