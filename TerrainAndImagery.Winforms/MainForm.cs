using GeoKernel.NET.WinForms;

namespace GeoKernel.TerrainAndImagery.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewer3DControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly ComboBox _quality = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 230 };
    private readonly CheckBox _imagery = new() { Text = "Show orthophoto", Checked = true, Enabled = false, Width = 230 };
    private readonly NumericUpDown _height = new()
    {
        Minimum = .25m, Maximum = 10, Increment = .25m, DecimalPlaces = 2, Value = 1, Width = 230
    };
    private readonly Button _reload = new() { Text = "Reload terrain", Width = 230 };
    private readonly Button _cancel = new() { Text = "Cancel loading", Width = 230, Enabled = false };
    private readonly ToolStripStatusLabel _status = new() { Text = "Preparing terrain…" };
    private readonly ToolStripProgressBar _progress = new() { Style = ProgressBarStyle.Marquee };
    private CancellationTokenSource? _load;

    public MainForm()
    {
        Text = "TerrainAndImagery — GeoKernel";
        ClientSize = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        _quality.Items.AddRange(["256 — Fast", "512 — Balanced", "1024 — Detailed"]);
        _quality.SelectedIndex = 1;

        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 260, Padding = new Padding(12),
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
        };
        panel.Controls.AddRange([
            Caption("Sagrada Familia - terrain and orthophoto"),
            Caption("Terrain mesh"),
            _quality, _reload, _cancel,
            _imagery,
            Caption("Vertical exaggeration"), _height,
            Caption("DEM heights are treated as metres; the vertical datum is unverified. No height correction is applied."),
            Caption("Left drag: pan\nWheel / right drag: zoom\nMiddle / Ctrl + left drag: orbit")
        ]);
        var reset = new Button { Text = "Reset view", Width = 230 };
        reset.Click += (_, _) => _viewer.ResetCamera();
        panel.Controls.Add(reset);
        var statusBar = new StatusStrip();
        statusBar.Items.AddRange([_status, _progress]);
        Controls.Add(_viewer);
        Controls.Add(panel);
        Controls.Add(statusBar);

        _imagery.CheckedChanged += (_, _) => _viewer.ImageryVisible = _imagery.Checked;
        _height.ValueChanged += (_, _) => _viewer.HeightScale = (float)_height.Value;
        _reload.Click += async (_, _) => await LoadSampleAsync();
        _cancel.Click += (_, _) => _load?.Cancel();
        Shown += async (_, _) => await LoadSampleAsync();
        FormClosing += (_, _) => _load?.Cancel();
    }

    private static Label Caption(string text) => new()
    {
        Text = text, AutoSize = true, MaximumSize = new Size(230, 0), Margin = new Padding(0, 8, 0, 6)
    };

    private async Task LoadSampleAsync()
    {
        if (_load != null) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        _reload.Enabled = _quality.Enabled = false;
        _cancel.Enabled = true;
        _progress.Visible = true;
        try
        {
            var progress = new Progress<string>(message =>
            {
                if (!IsDisposed) _status.Text = message;
            });
            string path = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            string imagery = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            if (IsDisposed) return;
            _status.Text = "Loading terrain…";
            await _viewer.LoadTerrainAndImageryAsync(path, imagery, 256 << _quality.SelectedIndex, cancellation.Token);
            if (IsDisposed) return;
            _imagery.Enabled = true;
            _viewer.ImageryVisible = _imagery.Checked;
            _status.Text = "Terrain and orthophoto loaded";
        }
        catch (OperationCanceledException)
        {
            if (!IsDisposed) _status.Text = "Loading cancelled.";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex);
            if (!IsDisposed)
            {
                _status.Text = "Terrain could not be loaded.";
                MessageBox.Show(this, ex.Message, "TerrainAndImagery", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _load = null;
            if (!IsDisposed)
            {
                _reload.Enabled = _quality.Enabled = true;
                _cancel.Enabled = false;
                _progress.Visible = false;
            }
        }
    }
}

