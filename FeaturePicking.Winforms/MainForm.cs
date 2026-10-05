using GeoKernel.NET.WinForms;

namespace GeoKernel.FeaturePicking.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewer3DControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly Button _reload = new() { Text = "Reload sample", Width = 280 };
    private readonly Button _cancel = new() { Text = "Cancel loading", Width = 280, Enabled = false };
    private readonly CheckBox _imagery = new() { Text = "Show orthophoto", Checked = true, Width = 280 };
    private readonly CheckBox _buildings = new() { Text = "Show buildings", Checked = true, Width = 280 };
    private readonly CheckBox _roads = new() { Text = "Show roads", Checked = true, Width = 280 };
    private readonly CheckBox _multiple = new() { Text = "Multiple selection (click to add/remove)", AutoSize = true };
    private readonly ComboBox _selected = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
    private readonly Label _count = new() { Text = "No selection", AutoSize = true };
    private readonly Button _focus = new() { Text = "Zoom to selection", Width = 280, Enabled = false };
    private readonly Button _clear = new() { Text = "Clear selection (Esc)", Width = 280, Enabled = false };
    private readonly NumericUpDown _height = new()
    {
        Minimum = .25m, Maximum = 10, DecimalPlaces = 2, Increment = .25m, Value = 1, Width = 280
    };
    private readonly DataGridView _attributes = new()
    {
        Width = 280, Height = 240, ReadOnly = true, AllowUserToAddRows = false,
        AllowUserToDeleteRows = false, RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect
    };
    private readonly ToolStripStatusLabel _status = new() { Text = "Preparing sample…" };
    private readonly ToolStripProgressBar _progress = new() { Style = ProgressBarStyle.Marquee };
    private readonly System.Windows.Forms.Timer _selectionTimer = new() { Interval = 150 };
    private FeatureSelection3D? _snapshot;
    private CancellationTokenSource? _load;
    private bool _ready;

    public MainForm()
    {
        Text = "FeaturePicking — GeoKernel";
        ClientSize = new Size(1200, 850);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        KeyPreview = true;
        _attributes.Columns.Add("Property", "Property");
        _attributes.Columns.Add("Value", "Value");
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 312, Padding = new Padding(12),
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
        };
        var reset = new Button { Text = "Reset view", Width = 280 };
        panel.Controls.AddRange([
            Caption("Sagrada Família — feature picking"),
            Caption("Click a building or road to inspect its source attributes. Drag to navigate."),
            _reload, _cancel, _imagery, _buildings, _roads, _multiple,
            _count, _selected, _attributes, _focus, _clear,
            Caption("Vertical exaggeration"), _height, reset,
            Caption("Building heights are illustrative (9 m), not measured. DEM heights are metres; the vertical datum is unverified."),
            Caption("Left drag: pan | Wheel/right drag: zoom | Middle/Ctrl+left drag: orbit")
        ]);
        var status = new StatusStrip();
        status.Items.AddRange([_status, _progress]);
        Controls.Add(_viewer);
        Controls.Add(panel);
        Controls.Add(status);
        SetSceneControls(false);

        _reload.Click += async (_, _) => await LoadSampleAsync();
        _cancel.Click += (_, _) => _load?.Cancel();
        _imagery.CheckedChanged += (_, _) => { if (_ready) _viewer.ImageryVisible = _imagery.Checked; };
        _buildings.CheckedChanged += (_, _) => ApplyVisibility();
        _roads.CheckedChanged += (_, _) => ApplyVisibility();
        _multiple.CheckedChanged += (_, _) => { if (_ready) _viewer.SetMultipleSelectionEnabled(_multiple.Checked); };
        _height.ValueChanged += (_, _) => _viewer.HeightScale = (float)_height.Value;
        reset.Click += (_, _) => _viewer.ResetCamera();
        _clear.Click += (_, _) => { _viewer.ClearSelection(); RefreshSelection(); };
        _focus.Click += (_, _) => _viewer.FocusSelection();
        _selected.SelectionChangeCommitted += (_, _) =>
        {
            if (_snapshot != null && _selected.SelectedItem is SelectedFeature3D feature)
            {
                _viewer.ActivateSelection(_snapshot.Revision, feature.Index);
                RefreshSelection();
            }
        };
        KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Escape && _ready)
            {
                _viewer.ClearSelection();
                RefreshSelection();
                e.Handled = e.SuppressKeyPress = true;
            }
        };
        _selectionTimer.Tick += (_, _) =>
        {
            try { RefreshSelection(); }
            catch (Exception ex) { _selectionTimer.Stop(); _status.Text = ex.Message; }
        };
        Shown += async (_, _) => await LoadSampleAsync();
        FormClosing += (_, _) => { _selectionTimer.Stop(); _load?.Cancel(); };
    }

    private static Label Caption(string text) => new()
    {
        Text = text, AutoSize = true, MaximumSize = new Size(280, 0), Margin = new Padding(0, 6, 0, 6)
    };

    private void SetSceneControls(bool enabled)
    {
        _imagery.Enabled = _roads.Enabled = _buildings.Enabled = _multiple.Enabled = enabled;
        _selected.Enabled = _focus.Enabled = _clear.Enabled = enabled && (_snapshot?.Entries.Count ?? 0) > 0;
    }

    private void ApplyVisibility()
    {
        if (!_ready) return;
        _viewer.SetBuildingStyle(_buildings.Checked, 1, Color.FromArgb(217, 196, 165));
        _viewer.SetRoadStyle(_roads.Checked, 1, Color.FromArgb(240, 224, 160));
        RefreshSelection();
    }

    private void RefreshSelection()
    {
        if (!_ready || IsDisposed) return;
        var snapshot = _viewer.GetFeatureSelection();
        if (_snapshot?.Revision == snapshot.Revision) return;
        _snapshot = snapshot;
        _selected.Items.Clear();
        _attributes.Rows.Clear();
        foreach (var feature in snapshot.Entries) _selected.Items.Add(feature);
        bool hasSelection = snapshot.Entries.Count > 0;
        _count.Text = hasSelection ? $"{snapshot.Entries.Count} selected" : "No selection";
        _selected.Enabled = _focus.Enabled = _clear.Enabled = hasSelection;
        if (!hasSelection) return;
        var active = snapshot.Entries.FirstOrDefault(f => f.Index == snapshot.ActiveIndex) ?? snapshot.Entries[0];
        _selected.SelectedItem = active;
        _attributes.Rows.Add("Layer", active.Layer);
        _attributes.Rows.Add("Type", active.Type);
        _attributes.Rows.Add("Scene feature ID", active.FeatureId);
        if (active.SourceFid != null) _attributes.Rows.Add("Source FID", active.SourceFid);
        if (active.DisplayHeight.HasValue) _attributes.Rows.Add("Display height (estimated)", $"{active.DisplayHeight:0.##} m");
        foreach (var pair in active.Attributes.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
            _attributes.Rows.Add(pair.Key, pair.Value ?? "(null)");
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        _selectionTimer.Stop();
        _reload.Enabled = false;
        _cancel.Enabled = _progress.Visible = true;
        SetSceneControls(false);
        try
        {
            var progress = new Progress<string>(message => { if (!IsDisposed) _status.Text = message; });
            var dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            var imagery = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            var roads = await SampleData.EnsureRoadsAsync(progress, cancellation.Token);
            var buildings = await SampleData.EnsureBuildingsAsync(progress, cancellation.Token);
            _status.Text = "Loading buildings and roads…";
            await _viewer.LoadFeaturesOnTerrainAsync(dem, imagery, roads, buildings, cancellationToken: cancellation.Token);
            if (IsDisposed) return;
            _ready = true;
            _snapshot = null;
            _viewer.ImageryVisible = _imagery.Checked;
            _viewer.SetMultipleSelectionEnabled(_multiple.Checked);
            ApplyVisibility();
            _status.Text = "Ready — click a building or road.";
        }
        catch (OperationCanceledException) { if (!IsDisposed) _status.Text = "Loading cancelled."; }
        catch (Exception ex)
        {
            if (!IsDisposed)
            {
                _status.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "Unable to load sample", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _load = null;
            if (!IsDisposed)
            {
                _reload.Enabled = true;
                _cancel.Enabled = _progress.Visible = false;
                SetSceneControls(_ready);
                if (_ready) _selectionTimer.Start();
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _selectionTimer.Dispose(); _load?.Cancel(); }
        base.Dispose(disposing);
    }
}
