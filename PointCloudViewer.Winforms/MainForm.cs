using GeoKernel.NET.WinForms;

namespace GeoKernel.PointCloudViewer.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewer3DControl _viewer = new() { PointCloudPreview = true, Dock = DockStyle.Fill };
    private readonly ComboBox _colors = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
    private readonly ComboBox _samples = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
    private readonly Button _reload = new() { Text = "Load / apply sample limit", Width = 280 };
    private readonly Button _cancel = new() { Text = "Cancel loading", Width = 280 };
    private readonly Label _info = Caption("No points loaded.");
    private readonly Label _errors = Caption("");
    private readonly ToolStripStatusLabel _status = new() { Text = "Preparing point cloud…" };
    private readonly ToolStripProgressBar _progress = new() { Style = ProgressBarStyle.Marquee };
    private readonly System.Windows.Forms.Timer _poll = new() { Interval = 1000 };
    private CancellationTokenSource? _load;
    private bool _ready;

    public MainForm()
    {
        Text = "PointCloudViewer — GeoKernel";
        ClientSize = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        _colors.Items.AddRange(["RGB", "Height"]);
        _colors.SelectedIndex = 0;
        _samples.Items.AddRange([50000, 100000, 250000]);
        _samples.SelectedIndex = 2;
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 310, Padding = new Padding(12),
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
        };
        var reset = new Button { Text = "Reset view", Width = 280 };
        panel.Controls.AddRange([
            Caption("EuroSDR P4 — 40 × 40 m crop"), Caption("Colour"), _colors,
            Caption("Sample limit"), _samples, _reload, _cancel, _info, _errors, reset,
            Caption("Local normalized preview. Source coordinates remain EPSG:32630; heights are provisional. Display is sampled up to 250,000 points. Source points are preserved. Point size: 1 pixel (SDK fixed)."),
            Caption("Left drag: pan\nWheel/right drag: zoom\nMiddle/Ctrl+left drag: orbit\nShift+left drag: look")
        ]);
        var status = new StatusStrip();
        status.Items.AddRange([_status, _progress]);
        Controls.Add(_viewer);
        Controls.Add(panel);
        Controls.Add(status);
        SetBusy(false);
        _reload.Click += async (_, _) => await LoadSampleAsync();
        _cancel.Click += (_, _) => { _load?.Cancel(); _cancel.Enabled = false; };
        _colors.SelectedIndexChanged += (_, _) =>
        {
            if (_ready) Perform(() => _viewer.SetPointCloudStyle(_colors.SelectedIndex == 1));
        };
        reset.Click += (_, _) => Perform(() => _viewer.ResetCamera());
        _poll.Tick += (_, _) => Perform(RefreshState);
        Shown += async (_, _) => await LoadSampleAsync();
        FormClosing += (_, _) => { _poll.Stop(); _load?.Cancel(); };
    }

    private static Label Caption(string text) => new()
    {
        Text = text, AutoSize = true, MaximumSize = new Size(280, 0), Margin = new Padding(0, 8, 0, 6)
    };

    private void Perform(Action action)
    {
        try { action(); }
        catch (Exception ex) { _errors.Text = ex.Message; }
    }

    private void SetBusy(bool busy)
    {
        _reload.Enabled = _samples.Enabled = !busy;
        _cancel.Enabled = busy;
        _colors.Enabled = _ready && !busy;
        _progress.Visible = busy;
        if (_ready && !busy) _poll.Start();
        else _poll.Stop();
    }

    private void RefreshState()
    {
        var state = _viewer.GetPointCloudState();
        _info.Text = $"Source: {state.Total:N0} points\nDisplay sample: {state.Sample:N0} points\n" +
            $"Source Z: {state.MinimumZ:F2} – {state.MaximumZ:F2} m\nRGB available: {(state.HasRgb ? "Yes" : "No")}";
        _errors.Text = state.Error.Trim();
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        SetBusy(true);
        try
        {
            var progress = new Progress<string>(text => { if (!IsDisposed) _status.Text = text; });
            string path = await SampleData.EnsureArchiveAsync("eurosdr_p4_pointcloud",
                "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/eurosdr_p4_pointcloud.zip",
                "eurosdr_p4_pointcloud.laz", progress, cancellation.Token);
            if (IsDisposed) return;
            _status.Text = "Reading and sampling LAZ…";
            await _viewer.LoadPointCloudAsync(path, (int)_samples.SelectedItem!, cancellation.Token);
            if (IsDisposed) return;
            _ready = true;
            _viewer.SetPointCloudStyle(_colors.SelectedIndex == 1);
            RefreshState();
            _status.Text = "Point cloud loaded.";
        }
        catch (OperationCanceledException)
        {
            if (!IsDisposed) _status.Text = _ready ? "Cancelled; previous cloud retained." : "Loading cancelled.";
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _status.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "PointCloudViewer", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _load = null;
            if (!IsDisposed) SetBusy(false);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _poll.Dispose();
        base.Dispose(disposing);
    }
}
