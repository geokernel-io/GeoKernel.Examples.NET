using System.Text.Json;
using GeoKernel.NET.WinForms;

namespace GeoKernel.CameraNavigation.Winforms;

public sealed class CameraPanel : FlowLayoutPanel
{
    public sealed record SavedView(string Name, float[] Camera)
    {
        public override string ToString() => Name;
    }

    private readonly GeoKernelViewer3DControl _viewer;
    private readonly ComboBox _presets = new() { Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly CheckBox _smooth = new() { Text = "Smooth transition (1.2 seconds)", Checked = true, Width = 230 };
    private readonly ComboBox _views = new() { Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly TextBox _name = new() { Width = 230, PlaceholderText = "Saved view name" };
    private readonly Label _status = new() { Width = 230, Height = 42 };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 150 };
    private readonly string _file;
    private float[]? _home;
    public bool Ready => _home != null;

    public CameraPanel(GeoKernelViewer3DControl viewer, string? savedViewsPath = null)
    {
        _viewer = viewer;
        _file = savedViewsPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GeoKernel", "CameraNavigation.Winforms", "sagrada-camera-views-v1.json");
        Width = 230; AutoSize = true; FlowDirection = FlowDirection.TopDown; WrapContents = false;
        _presets.Items.AddRange(["Overview", "Top view", "East view", "West view", "Close oblique"]);
        _presets.SelectedIndex = 0;
        Controls.Add(_presets);
        AddButton("Go to preset", () => GoPreset(_presets.SelectedIndex));
        Controls.Add(_smooth);
        _smooth.CheckedChanged += (_, _) => _viewer.StopCamera();
        AddButton("Stop transition", _viewer.StopCamera);
        AddButton("Zoom in", () => Zoom(.7f));
        AddButton("Zoom out", () => Zoom(1 / .7f));
        AddButton("North up", () => { var c = _viewer.GetCamera(); c[0] = 0; MoveTo(c); });
        Controls.Add(new Label { Text = "Saved views", AutoSize = true });
        Controls.Add(_views); Controls.Add(_name);
        AddButton("Save current view", () => SaveView(_name.Text));
        AddButton("Restore saved view", RestoreView);
        AddButton("Delete saved view", DeleteView);
        Controls.Add(_status);
        Controls.Add(new Label { Text = "Mouse or keyboard input stops transitions.\nViews belong to this sample dataset.", Width = 230, Height = 48 });
        try
        {
            if (File.Exists(_file))
                foreach (var view in JsonSerializer.Deserialize<List<SavedView>>(File.ReadAllText(_file)) ?? [])
                    if (_views.Items.Count < 20 && !string.IsNullOrWhiteSpace(view.Name) && Valid(view.Camera)) _views.Items.Add(view);
            if (_views.Items.Count > 0) _views.SelectedIndex = 0;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        { _status.Text = "Saved views could not be read."; }
        _timer.Tick += (_, _) => {
            if (!Ready || !Enabled) return;
            var c = _viewer.GetCamera();
            _status.Text = $"Heading: {(c[0] % 360 + 360) % 360:F1} degrees\nTilt: {c[1]:F1} degrees";
        };
        _timer.Start();
    }

    private void AddButton(string text, Action action)
    {
        var button = new Button { Text = text, Width = 230 };
        button.Click += (_, _) => {
            try { action(); }
            catch (Exception ex) { MessageBox.Show(this, ex.Message, "Camera navigation", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        };
        Controls.Add(button);
    }

    public void SetHome() { _viewer.StopCamera(); _home = _viewer.GetCamera(); }
    public void GoPreset(int index)
    {
        if (_home == null) return;
        var c = (float[])_home.Clone();
        if (index == 1) { c[0] = 0; c[1] = 85; }
        if (index == 2) { c[0] = 90; c[1] = 35; }
        if (index == 3) { c[0] = -90; c[1] = 35; }
        if (index == 4) { c[1] = 30; c[2] = Math.Max(.005f, c[2] * .55f); }
        MoveTo(c);
    }
    private void MoveTo(float[] c) => _viewer.MoveCamera(c, _smooth.Checked ? 1200 : 0);
    private void Zoom(float factor) { var c = _viewer.GetCamera(); c[2] = Math.Clamp(c[2] * factor, .005f, 20); MoveTo(c); }
    public void SaveView(string name)
    {
        if (!Ready || string.IsNullOrWhiteSpace(name) || _views.Items.Count >= 20) return;
        _viewer.StopCamera();
        _views.Items.Add(new SavedView(name.Trim(), _viewer.GetCamera()));
        _views.SelectedIndex = _views.Items.Count - 1;
        Persist();
    }
    public void RestoreView() { if (_views.SelectedItem is SavedView view) MoveTo(view.Camera); }
    public void DeleteView()
    {
        if (_views.SelectedIndex < 0) return;
        _views.Items.RemoveAt(_views.SelectedIndex);
        if (_views.Items.Count > 0) _views.SelectedIndex = 0;
        Persist();
    }
    private void Persist()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(_file))!);
        var temporary = _file + ".tmp";
        File.WriteAllText(temporary, JsonSerializer.Serialize(_views.Items.Cast<SavedView>().ToArray()));
        File.Move(temporary, _file, true);
    }
    private static bool Valid(float[]? c) => c is { Length: 6 } && c.All(float.IsFinite) && c[1] >= -85 && c[1] <= 85 && c[2] >= .005f && c[2] <= 20;
    protected override void Dispose(bool disposing) { if (disposing) _timer.Dispose(); base.Dispose(disposing); }
}
