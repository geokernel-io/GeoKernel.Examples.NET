using GeoKernel.NET.WinForms;

namespace GeoKernel.ProjectSaveLoad.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewer3DControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly FlowLayoutPanel _styles = new()
    {
        Width = 300, AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Enabled = false
    };
    private readonly ComboBox _layer = Choice();
    private readonly ComboBox _theme = Choice();
    private readonly ComboBox _field = Choice();
    private readonly ComboBox _values = Choice();
    private readonly CheckBox _visible = new() { Text = "Visible", Width = 280 };
    private readonly CheckBox _imagery = new() { Text = "Show orthophoto", Checked = true, Width = 280 };
    private readonly NumericUpDown _opacity = new() { Minimum = 0, Maximum = 100, Width = 280 };
    private readonly NumericUpDown _height = new()
    {
        Minimum = .25m, Maximum = 10, DecimalPlaces = 2, Increment = .25m, Value = 1, Width = 280
    };
    private readonly TextBox _text = new() { Width = 280, PlaceholderText = "Contains text (case-insensitive)" };
    private readonly Label _count = Caption("No layer loaded.");
    private readonly Button _color = Button("Choose uniform color");
    private readonly Button _focus = Button("Zoom to matching features");
    private readonly Button _reload = Button("Reload sample");
    private readonly Button _cancel = Button("Cancel loading");
    private readonly ToolStripStatusLabel _status = new() { Text = "Preparing sample…" };
    private readonly ToolStripProgressBar _progress = new() { Style = ProgressBarStyle.Marquee };
    private readonly System.Windows.Forms.Timer _debounce = new() { Interval = 250 };
    private List<LayerStyling3D> _layers = [];
    private string _activeId = "";
    private readonly Button _open = Button("Open project…");
    private readonly Button _save = Button("Save");
    private readonly Button _saveAs = Button("Save as…");
    private readonly Label _projectInfo = Caption("Unsaved sample scene");
    private string? _projectPath;
    private bool _syncing;
    private bool _ready;
    private CancellationTokenSource? _load;

    public MainForm()
    {
        Text = "ProjectSaveLoad — GeoKernel";
        ClientSize = new Size(1250, 850);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 328, Padding = new Padding(10),
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
        };
        var source = Button("Restore source color");
        var clear = Button("Clear this filter");
        var resetStyles = Button("Reset all styles and filters");
        var resetView = Button("Reset view");
        _styles.Controls.AddRange([
            Caption("Layer"), _layer, _visible, Caption("Opacity (%)"), _opacity, _color, source,
            Caption("Color by attribute"), _theme, Caption("Filter attribute"), _field,
            Caption("Contains"), _text, Caption("Example values (up to 200)"), _values,
            clear, _count, _focus, resetStyles
        ]);
        panel.Controls.AddRange([
            Caption("Sagrada Família — layer styling and filtering"),
            _open, _save, _saveAs, _projectInfo,
            Caption("Projects reference local files; they do not embed data. Keep the data cache. Open/reload replaces the current scene; save changes first."),
            _reload, _cancel, _imagery, _styles, Caption("Vertical exaggeration"), _height, resetView,
            Caption("Filters use literal, case-insensitive substring matching and affect drawing and selection. Attribute coloring overrides uniform color. Settings are session-only."),
            Caption("Counts include records without drawable geometry. Building heights are illustrative (9 m). DEM heights are metres; the vertical datum is unverified."),
            Caption("Left drag: pan | Wheel/right drag: zoom | Middle/Ctrl+left drag: orbit")
        ]);
        var statusBar = new StatusStrip();
        statusBar.Items.AddRange([_status, _progress]);
        Controls.Add(_viewer);
        Controls.Add(panel);
        Controls.Add(statusBar);
        _cancel.Enabled = false;
        _save.Enabled = _saveAs.Enabled = false;
        _open.Click += async (_, _) =>
        {
            if (_load != null) return;
            using var dialog = new OpenFileDialog
            {
                Filter = "GeoKernel 3D project (*.gk3d;*.json)|*.gk3d;*.json",
                CheckFileExists = true, Title = "Open project"
            };
            if (dialog.ShowDialog(this) == DialogResult.OK) await LoadSampleAsync(dialog.FileName);
        };
        _save.Click += (_, _) => SaveProject(false);
        _saveAs.Click += (_, _) => SaveProject(true);
        KeyPreview = true;
        KeyDown += (_, e) =>
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.Handled = e.SuppressKeyPress = true;
                SaveProject(e.Shift);
            }
        };
        _imagery.Enabled = false;

        _reload.Click += async (_, _) => await LoadSampleAsync();
        _cancel.Click += (_, _) => _load?.Cancel();
        _imagery.CheckedChanged += (_, _) => Safe(() => _viewer.ImageryVisible = _imagery.Checked);
        _height.ValueChanged += (_, _) => Safe(() => _viewer.HeightScale = (float)_height.Value);
        resetView.Click += (_, _) => Safe(() => _viewer.ResetCamera());
        _layer.SelectedIndexChanged += (_, _) =>
        {
            if (_syncing || !_ready) return;
            Safe(() =>
            {
                if (_debounce.Enabled) ApplyFilter();
                _activeId = (_layer.SelectedItem as LayerStyling3D)?.Id ?? "";
                RefreshLayers();
            });
        };
        _visible.CheckedChanged += (_, _) => Appearance();
        _opacity.ValueChanged += (_, _) => Appearance();
        _color.Click += (_, _) => Safe(() =>
        {
            ApplyFilter();
            var current = Current();
            using var dialog = new ColorDialog
            {
                Color = string.IsNullOrEmpty(current?.Color) ? Color.White : ColorTranslator.FromHtml(current.Color)
            };
            if (dialog.ShowDialog(this) == DialogResult.OK)
            {
                _viewer.SetLayerColor(_activeId, dialog.Color);
                RefreshLayers();
            }
        });
        source.Click += (_, _) => Safe(() => { ApplyFilter(); _viewer.SetLayerColor(_activeId, null); RefreshLayers(); });
        _theme.SelectionChangeCommitted += (_, _) => Safe(() =>
        {
            ApplyFilter();
            _viewer.SetLayerTheme(_activeId, SelectedField(_theme));
            RefreshLayers();
        });
        _field.SelectionChangeCommitted += (_, _) => Safe(() => { PopulateValues(); ApplyFilter(); });
        _text.TextChanged += (_, _) =>
        {
            if (!_syncing && _ready) { _debounce.Stop(); _debounce.Start(); }
        };
        _debounce.Tick += (_, _) => Safe(ApplyFilter);
        _values.SelectionChangeCommitted += (_, _) =>
        {
            if (_values.SelectedIndex > 0) _text.Text = _values.SelectedItem?.ToString() ?? "";
        };
        clear.Click += (_, _) => Safe(() => { _text.Clear(); ApplyFilter(); });
        _focus.Click += (_, _) => Safe(() => { ApplyFilter(); _viewer.FocusLayer(_activeId); });
        resetStyles.Click += (_, _) => Safe(() => { _debounce.Stop(); _viewer.ResetLayerStyling(); RefreshLayers(); });
        Shown += async (_, _) => await LoadSampleAsync();
        FormClosing += (_, _) => { _debounce.Stop(); _load?.Cancel(); };
    }

    private static ComboBox Choice() => new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 280 };
    private static Button Button(string text) => new() { Text = text, Width = 280, AutoSize = true };
    private static Label Caption(string text) => new()
    {
        Text = text, AutoSize = true, MaximumSize = new Size(280, 0), Margin = new Padding(0, 6, 0, 6)
    };
    private static string SelectedField(ComboBox combo) => combo.SelectedIndex > 0 ? combo.SelectedItem?.ToString() ?? "" : "";
    private LayerStyling3D? Current() => _layers.FirstOrDefault(layer => layer.Id == _activeId);

    private void Safe(Action action)
    {
        if (!_ready || _syncing || IsDisposed) return;
        try { action(); }
        catch (Exception ex) { _debounce.Stop(); _status.Text = ex.Message; }
    }

    private void Appearance() => Safe(() =>
    {
        _viewer.SetLayerAppearance(_activeId, _visible.Checked, (float)_opacity.Value / 100);
        // Keep pending filter text intact while changing appearance.
        UpdateCounts();
    });

    private void ApplyFilter()
    {
        _debounce.Stop();
        if (string.IsNullOrEmpty(_activeId)) return;
        _viewer.SetLayerFilter(_activeId, SelectedField(_field), _text.Text);
        UpdateCounts();
    }

    private void UpdateCounts()
    {
        _layers = _viewer.GetLayerStyling();
        var current = Current();
        if (current == null) return;
        _count.Text = $"Matching records: {current.MatchingRecords:N0} / {current.Records:N0}\nDrawable matching ranges: {current.DrawableRanges:N0}";
        _focus.Enabled = current.DrawableRanges > 0 && current.Visible && current.Opacity > 0;
    }

    private void RefreshLayers()
    {
        _layers = _viewer.GetLayerStyling();
        _syncing = true;
        try
        {
            _layer.Items.Clear();
            foreach (var item in _layers) _layer.Items.Add(item);
            var current = Current() ?? _layers.FirstOrDefault();
            _activeId = current?.Id ?? "";
            _styles.Enabled = current != null;
            if (current == null) return;
            _layer.SelectedItem = current;
            _visible.Checked = current.Visible;
            _opacity.Value = Math.Clamp((decimal)current.Opacity * 100, 0, 100);
            _color.Text = current.Color.Length > 0 ? $"Uniform color: {current.Color}" : "Choose uniform color";
            _field.Items.Clear();
            _theme.Items.Clear();
            _field.Items.Add("All attributes / source IDs");
            _theme.Items.Add("Uniform / source color");
            foreach (var key in current.Attributes.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
            {
                _field.Items.Add(key);
                _theme.Items.Add(key);
            }
            _field.SelectedIndex = Math.Max(0, _field.Items.IndexOf(current.Field));
            _theme.SelectedIndex = Math.Max(0, _theme.Items.IndexOf(current.Theme));
            _text.Text = current.Text;
            PopulateValues();
        }
        finally { _syncing = false; }
        UpdateCounts();
    }

    private void PopulateValues()
    {
        _values.Items.Clear();
        _values.Items.Add("Choose a value or type text");
        if (Current()?.Attributes.TryGetValue(SelectedField(_field), out var values) == true)
            foreach (var value in values) _values.Items.Add(value);
        _values.SelectedIndex = 0;
        _values.Enabled = _values.Items.Count > 1;
    }

    private void SaveProject(bool choosePath)
    {
        if (!_ready || _load != null) return;
        try
        {
            ApplyFilter();
            string? path = _projectPath;
            if (choosePath || string.IsNullOrEmpty(path))
            {
                using var dialog = new SaveFileDialog
                {
                    Filter = "GeoKernel 3D project (*.gk3d)|*.gk3d",
                    DefaultExt = "gk3d", AddExtension = true, OverwritePrompt = true,
                    FileName = path ?? "SagradaFamilia.gk3d", Title = "Save project"
                };
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                path = dialog.FileName;
            }
            _viewer.SaveProject(path);
            _projectPath = Path.GetFullPath(path);
            _projectInfo.Text = _projectPath;
            _status.Text = "Project saved. Local source files remain separate.";
        }
        catch (Exception ex)
        {
            _status.Text = ex.Message;
            MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task LoadSampleAsync(string? projectPath = null)
    {
        if (_load != null) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        if (_ready) Safe(ApplyFilter);
        _debounce.Stop();
        _styles.Enabled = _imagery.Enabled = _reload.Enabled = _open.Enabled = _save.Enabled = _saveAs.Enabled = _height.Enabled = false;
        _cancel.Enabled = _progress.Visible = true;
        try
        {
            if (projectPath == null)
            {
                var progress = new Progress<string>(message => { if (!IsDisposed) _status.Text = message; });
                var dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
                var imagery = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
                var roads = await SampleData.EnsureRoadsAsync(progress, cancellation.Token);
                var buildings = await SampleData.EnsureBuildingsAsync(progress, cancellation.Token);
                cancellation.Token.ThrowIfCancellationRequested();
                if (IsDisposed) return;
                _status.Text = "Loading terrain, roads and buildings…";
                await _viewer.LoadFeaturesOnTerrainAsync(dem, imagery, roads, buildings, cancellationToken: cancellation.Token);
                if (IsDisposed) return;
                _viewer.ImageryVisible = _imagery.Checked;
                _viewer.HeightScale = (float)_height.Value;
            }
            else
            {
                _status.Text = "Opening project…";
                await _viewer.OpenProjectAsync(projectPath, cancellation.Token);
                if (IsDisposed) return;
                _syncing = true;
                try
                {
                    _height.Value = (decimal)_viewer.HeightScale;
                    _imagery.Checked = _viewer.ImageryVisible;
                }
                finally { _syncing = false; }
            }
            _ready = true;
            _projectPath = projectPath == null ? null : Path.GetFullPath(projectPath);
            _projectInfo.Text = _projectPath ?? "Unsaved sample scene";
            RefreshLayers();
            _status.Text = "Ready — choose a layer to style or filter.";
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
                _reload.Enabled = _open.Enabled = _height.Enabled = true;
                _cancel.Enabled = _progress.Visible = false;
                _styles.Enabled = _imagery.Enabled = _save.Enabled = _saveAs.Enabled = _ready;
            }
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _debounce.Dispose(); _load?.Cancel(); }
        base.Dispose(disposing);
    }
}
