using GeoKernel.NET.WinForms;

namespace GeoKernel.Tiles3DStreaming.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewer3DControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly Button _reload = new() { Text = "Reload sample", Width = 290 };
    private readonly Button _cancel = new() { Text = "Cancel loading", Width = 290, Enabled = false };
    private readonly Button _start = new() { Text = "Start / apply budget", Width = 290 };
    private readonly Button _stop = new() { Text = "Stop streaming (keep current tiles)", Width = 290 };
    private readonly Button _focus = new() { Text = "Focus loaded tiles", Width = 290 };
    private readonly CheckBox _visible = new() { Text = "Show buildings", Checked = true, Width = 290 };
    private readonly CheckBox _imagery = new() { Text = "Show orthophoto", Checked = true, Width = 290 };
    private readonly NumericUpDown _budget = new() { Minimum = 64, Maximum = 128, Increment = 32, Value = 96, Width = 290 };
    private readonly Label _details = Caption("No terrain loaded.");
    private readonly Label _errors = Caption("");
    private readonly ToolStripStatusLabel _status = new() { Text = "Preparing sample…" };
    private readonly ToolStripProgressBar _progress = new() { Style = ProgressBarStyle.Marquee };
    private readonly System.Windows.Forms.Timer _poll = new() { Interval = 500 };
    private CancellationTokenSource? _load;
    private string? _source;
    private bool _ready, _updating;

    public MainForm()
    {
        Text = "Tiles3DStreaming — GeoKernel";
        ClientSize = new Size(1400, 850);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 325, Padding = new Padding(12),
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
        };
        var reset = new Button { Text = "Reset view", Width = 290 };
        panel.Controls.AddRange([
            Caption("Sagrada Família — terrain and 3D Tiles"), _reload, _cancel, _imagery,
            Caption("Camera movement selects tile detail automatically. Data streams from the downloaded local archive."),
            Caption("CPU geometry batch budget (MiB)"), _budget, _start, _stop, _visible, _focus,
            _details, _errors, reset,
            Caption("Vertical exaggeration: 1×. DEM uses the regional EGM08D595 correction. Building/terrain alignment is approximate and not independently surveyed."),
            Caption("Hiding clears the loaded model; showing restarts streaming. Stop may wait for in-flight work. Cache counters are not total RAM/VRAM usage."),
            Caption("Left drag: pan | Wheel/right drag: zoom | Middle/Ctrl+left drag: orbit")
        ]);
        var status = new StatusStrip();
        status.Items.AddRange([_status, _progress]);
        Controls.Add(_viewer); Controls.Add(panel); Controls.Add(status);
        SetBusy(false);
        _reload.Click += async (_, _) => await LoadSampleAsync();
        _cancel.Click += (_, _) => _load?.Cancel();
        _start.Click += (_, _) => Perform(StartStreaming);
        _stop.Click += (_, _) => Perform(() => _viewer.StopTiles());
        _focus.Click += (_, _) => Perform(() => _viewer.FocusTiles());
        _visible.CheckedChanged += (_, _) =>
        {
            if (_updating || !_ready || _load != null) return;
            Perform(() => { if (_visible.Checked) StartStreaming(); else _viewer.StopTiles(clear: true); });
        };
        _imagery.CheckedChanged += (_, _) => { if (_ready) Perform(() => _viewer.ImageryVisible = _imagery.Checked); };
        reset.Click += (_, _) => Perform(() => _viewer.ResetCamera());
        _poll.Tick += (_, _) =>
        {
            try { RefreshState(); }
            catch (Exception ex) { _poll.Stop(); _errors.Text = ex.Message; }
        };
        Shown += async (_, _) => await LoadSampleAsync();
        FormClosing += (_, _) => { _poll.Stop(); _load?.Cancel(); };
    }

    private static Label Caption(string text) => new()
    {
        Text = text, AutoSize = true, MaximumSize = new Size(290, 0), Margin = new Padding(0, 8, 0, 6)
    };

    private void Perform(Action action)
    {
        try { action(); _errors.Text = ""; RefreshState(); }
        catch (Exception ex) { _errors.Text = ex.Message; }
    }

    private void StartStreaming()
    {
        if (!_ready || _source == null) return;
        _viewer.StartTiles(_source, (int)_budget.Value);
        _updating = true;
        try { _visible.Checked = true; }
        finally { _updating = false; }
    }

    private void RefreshState()
    {
        if (!_ready || _load != null) return;
        var state = _viewer.GetTilesState();
        _stop.Enabled = state.Active;
        _focus.Enabled = state.Attached && _visible.Checked;
        _details.Text = $"State: {(state.Active ? (state.Loading ? "Updating (previous tiles visible)" : "Active") : "Stopped")}\n" +
            $"Loaded tiles: {state.Tiles}\nDeferred tiles: {state.DeferredTiles}\n" +
            $"Detail budget limited: {(state.SelectionLimited ? "Yes" : "No")}\n" +
            $"CPU cache: {state.CacheBytes / (1024.0 * 1024):F1} MiB\nCache hits: {state.CacheHits}\n" +
            $"Drawn vertices: {state.DrawnVertices}\nGPU upload pending: {(state.UploadPending ? "Yes" : "No")}";
        _errors.Text = state.Error.Trim();
    }

    private void SetBusy(bool busy)
    {
        _reload.Enabled = !busy;
        _cancel.Enabled = busy;
        _progress.Visible = busy;
        _start.Enabled = _budget.Enabled = _visible.Enabled = _imagery.Enabled = _ready && !busy;
        _stop.Enabled = _focus.Enabled = false;
        if (_ready && !busy) { _poll.Start(); RefreshState(); }
        else _poll.Stop();
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        SetBusy(true);
        bool resume = false;
        try
        {
            if (_ready) { resume = _viewer.GetTilesState().Active; _viewer.StopTiles(); }
            var progress = new Progress<string>(text => { if (!IsDisposed) _status.Text = text; });
            string dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            string image = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            string tiles = await SampleData.EnsureArchiveAsync("sagrada_familia_3d_tiles",
                "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/sagrada_familia_3d_tiles.zip",
                "tileset.json", progress, cancellation.Token);
            string geoid = await SampleData.EnsureArchiveAsync("egm08d595",
                "https://icgc-web-pro.s3.eu-central-1.amazonaws.com/produccio/s3fs-public/EGM08D595_19839.zip",
                "cat80000.gr", progress, cancellation.Token);
            if (IsDisposed) return;
            _status.Text = "Loading corrected terrain…";
            await _viewer.LoadTilesTerrainAsync(dem, image, geoid, tiles, cancellation.Token);
            if (IsDisposed) return;
            _source = tiles;
            _ready = true;
            _viewer.ImageryVisible = _imagery.Checked;
            resume = _visible.Checked;
            _status.Text = "Terrain loaded — camera movement controls tile detail.";
        }
        catch (OperationCanceledException) { if (!IsDisposed) _status.Text = "Loading cancelled."; }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _status.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "Tiles3DStreaming", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _load = null;
            if (!IsDisposed)
            {
                try { if (resume && _ready) StartStreaming(); SetBusy(false); }
                catch (Exception ex) { _poll.Stop(); _errors.Text = ex.Message; _reload.Enabled = true; _cancel.Enabled = false; _progress.Visible = false; }
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _poll.Dispose();
        base.Dispose(disposing);
    }
}
