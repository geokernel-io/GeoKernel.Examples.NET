using GeoKernel.NET.WinForms;

namespace GeoKernel.TerrainAnalysis.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewer3DControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly ComboBox _quality = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
    private readonly ComboBox _mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
    private readonly NumericUpDown _height = new()
    {
        Minimum = .25m, Maximum = 10, Increment = .25m, DecimalPlaces = 2, Value = 1, Width = 280
    };
    private readonly Button _reload = new() { Text = "Reload sample", Width = 280 };
    private readonly Button _cancel = new() { Text = "Cancel loading", Width = 280, Enabled = false };
    private readonly FlowLayoutPanel _legend = new()
    {
        AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Width = 280
    };
    private readonly Label _info = Caption("No terrain loaded.");
    private readonly ToolStripStatusLabel _status = new() { Text = "Preparing terrain…" };
    private readonly ToolStripProgressBar _progress = new() { Style = ProgressBarStyle.Marquee, Visible = false };
    private CancellationTokenSource? _load;
    private bool _ready;

    public MainForm()
    {
        Text = "TerrainAnalysis — GeoKernel";
        ClientSize = new Size(1200, 800);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        _quality.Items.AddRange(["256 — Fast", "512 — Balanced", "1024 — Detailed"]);
        _quality.SelectedIndex = 1;
        _mode.Items.AddRange(["Terrain relief", "Slope (degrees)", "Aspect (downhill direction)"]);
        _mode.SelectedIndex = 1;
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 315, Padding = new Padding(12),
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
        };
        var reset = new Button { Text = "Reset view", Width = 280 };
        panel.Controls.AddRange([
            Caption("Sagrada Família — sample terrain"),
            Caption("Slope and aspect describe the loaded terrain mesh, not an exported analysis raster. Mesh resolution affects results. Heights have an unverified vertical reference."),
            _reload, _cancel, Caption("Mesh samples per side"), _quality,
            Caption("Display"), _mode, _legend,
            Caption("Analysis uses terrain mesh faces before vertical exaggeration. Aspect is clockwise from north in the local ENU frame."),
            Caption("Vertical exaggeration"), _height, _info, reset,
            Caption("Left drag: pan\nWheel/right drag: zoom\nMiddle/Ctrl+left drag: orbit\nShift+left drag: look")
        ]);
        var status = new StatusStrip();
        status.Items.AddRange([_status, _progress]);
        Controls.Add(_viewer);
        Controls.Add(panel);
        Controls.Add(status);
        _mode.SelectedIndexChanged += (_, _) => Perform(ApplyMode);
        _height.ValueChanged += (_, _) => Perform(() => _viewer.HeightScale = (float)_height.Value);
        reset.Click += (_, _) => Perform(() => _viewer.ResetCamera());
        _reload.Click += async (_, _) => await LoadSampleAsync();
        _cancel.Click += (_, _) => { _load?.Cancel(); _cancel.Enabled = false; };
        Shown += async (_, _) => await LoadSampleAsync();
        FormClosing += (_, _) => _load?.Cancel();
        ApplyMode();
    }

    private static Label Caption(string text) => new()
    {
        Text = text, AutoSize = true, MaximumSize = new Size(280, 0), Margin = new Padding(0, 8, 0, 6)
    };

    private void Perform(Action action)
    {
        try { action(); }
        catch (Exception ex) { _status.Text = ex.Message; }
    }

    private void ApplyMode()
    {
        if (_ready)
        {
            _viewer.ColorRamp = TerrainColorRamp.Terrain;
            _viewer.SetTerrainDisplayMode((TerrainDisplayMode)_mode.SelectedIndex);
        }
        _legend.SuspendLayout();
        try
        {
            while (_legend.Controls.Count > 0) _legend.Controls[0].Dispose();
            (string Color, string Text)[] rows = _mode.SelectedIndex switch
            {
                1 => [("#2c7bb6", "0 to <5°"), ("#abd9e9", "5 to <15°"),
                      ("#ffffbf", "15 to <30°"), ("#fdae61", "30 to <45°"), ("#d7191c", "45 to 90°")],
                2 => [("#e6194b", "N — 0°"), ("#f58230", "NE — 45°"), ("#ffe119", "E — 90°"),
                      ("#3cb44b", "SE — 135°"), ("#46f0f0", "S — 180°"), ("#0082c8", "SW — 225°"),
                      ("#911eb4", "W — 270°"), ("#f032e6", "NW — 315°"), ("#808080", "Flat (slope <0.01°)")],
                _ => [("#185b3f", "Low"), ("#68984c", "↓"), ("#cfbe7e", "↓"),
                      ("#926b4c", "↓"), ("#fafafa", "High")]
            };
            foreach (var row in rows)
            {
                var line = new FlowLayoutPanel { AutoSize = true, WrapContents = false, Margin = new Padding(0, 2, 0, 2) };
                line.Controls.Add(new Panel { BackColor = ColorTranslator.FromHtml(row.Color), Size = new Size(24, 18) });
                line.Controls.Add(new Label { Text = row.Text, AutoSize = true });
                _legend.Controls.Add(line);
            }
            if (_mode.SelectedIndex == 2) _legend.Controls.Add(Caption("Each direction spans ±22.5°."));
            if (_mode.SelectedIndex == 0) _legend.Controls.Add(Caption("Relative ENU relief, with lighting."));
        }
        finally { _legend.ResumeLayout(); }
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        _reload.Enabled = _quality.Enabled = false;
        _cancel.Enabled = true;
        _progress.Visible = true;
        int resolution = 256 << _quality.SelectedIndex;
        try
        {
            var progress = new Progress<string>(text => { if (!IsDisposed) _status.Text = text; });
            string path = await SampleData.EnsureTerrainAsync(progress, cancellation.Token);
            if (IsDisposed) return;
            _status.Text = "Loading terrain…";
            await _viewer.LoadTerrainAsync(path, resolution, cancellation.Token);
            if (IsDisposed) return;
            _ready = true;
            ApplyMode();
            _viewer.HeightScale = (float)_height.Value;
            _info.Text = $"DEM: {Path.GetFileName(path)}\nHeights: Unknown — unverified preview\nMesh: up to {resolution} samples per side";
            _status.Text = "Terrain loaded.";
        }
        catch (OperationCanceledException)
        {
            if (!IsDisposed) _status.Text = "Loading cancelled. Previous scene retained.";
        }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _status.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "TerrainAnalysis", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
