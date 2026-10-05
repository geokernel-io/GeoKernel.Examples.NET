using GeoKernel.NET.WinForms;

namespace GeoKernel.Buildings3D.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewer3DControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly ComboBox _quality = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 230 };
    private readonly CheckBox _imagery = new() { Text = "Show orthophoto", Checked = true, Enabled = false, Width = 230 };
    private readonly NumericUpDown _height = new()
    {
        Minimum = .25m, Maximum = 10, Increment = .25m, DecimalPlaces = 2, Value = 1, Width = 230
    };
    private readonly Button _reload = new() { Text = "Apply height / Reload sample", Width = 230 };
    private readonly Button _cancel = new() { Text = "Cancel loading", Width = 230, Enabled = false };
    private readonly ToolStripStatusLabel _status = new() { Text = "Preparing terrain…" };
    private readonly ToolStripProgressBar _progress = new() { Style = ProgressBarStyle.Marquee };
    private readonly CheckBox _buildings = new() { Text = "Show buildings", Checked = true, Enabled = false, Width = 230 };
    private readonly NumericUpDown _opacity = new() { Minimum = 0, Maximum = 100, Value = 100, Width = 230, Enabled = false };
    private readonly Button _buildingColor = new() { Text = "Building color", Width = 230, Enabled = false };
    private Color _color = Color.FromArgb(217, 196, 165);
    private readonly NumericUpDown _buildingHeight = new()
    {
        Minimum = 1, Maximum = 300, Value = 9, DecimalPlaces = 1, Width = 230
    };
    private CancellationTokenSource? _load;

    public MainForm()
    {
        Text = "Buildings3D — GeoKernel";
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
            Caption("Sagrada Família — buildings on terrain"),
            Caption("Terrain mesh"),
            _quality,
            Caption("Default building height (m)"), _buildingHeight,
            Caption("Illustrative height: this dataset has no measured building heights. Apply height / Reload sample after changing it."),
            _reload, _cancel,
            _imagery, _buildings, _buildingColor, Caption("Building opacity (%)"), _opacity,
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

        _buildings.CheckedChanged += (_, _) => ApplyBuildingStyle();
        _opacity.ValueChanged += (_, _) => ApplyBuildingStyle();
        _buildingColor.Click += (_, _) => {
            using var dialog = new ColorDialog { Color = _color };
            if (dialog.ShowDialog(this) == DialogResult.OK) { _color = dialog.Color; ApplyBuildingStyle(); }
        };
        _imagery.CheckedChanged += (_, _) => _viewer.ImageryVisible = _imagery.Checked;
        _height.ValueChanged += (_, _) => _viewer.HeightScale = (float)_height.Value;
        _reload.Click += async (_, _) => await LoadSampleAsync();
        _cancel.Click += (_, _) => _load?.Cancel();
        Shown += async (_, _) => await LoadSampleAsync();
        FormClosing += (_, _) => _load?.Cancel();
    }

    private void ApplyBuildingStyle()
    {
        _viewer.SetBuildingStyle(_buildings.Checked, (float)_opacity.Value / 100, _color);
        _buildingColor.Text = $"Building color: #{_color.R:X2}{_color.G:X2}{_color.B:X2}";
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
        _reload.Enabled = _quality.Enabled = _buildingHeight.Enabled = false;
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
            string buildings = await SampleData.EnsureBuildingsAsync(progress, cancellation.Token);
            if (IsDisposed) return;
            _status.Text = "Loading terrain…";
            await _viewer.LoadBuildingsOnTerrainAsync(path, imagery, buildings, (float)_buildingHeight.Value, 256 << _quality.SelectedIndex, cancellation.Token);
            if (IsDisposed) return;
            _buildings.Enabled = _opacity.Enabled = _buildingColor.Enabled = true;
            ApplyBuildingStyle();
            _imagery.Enabled = true;
            _viewer.ImageryVisible = _imagery.Checked;
            _status.Text = $"Terrain, orthophoto and buildings loaded - {_viewer.GetBuildingVertexCount():N0} building vertices";
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
                MessageBox.Show(this, ex.Message, "Buildings3D", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _load = null;
            if (!IsDisposed)
            {
                _reload.Enabled = _quality.Enabled = _buildingHeight.Enabled = true;
                _cancel.Enabled = false;
                _progress.Visible = false;
            }
        }
    }
}


