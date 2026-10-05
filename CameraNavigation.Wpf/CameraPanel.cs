using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.CameraNavigation.Wpf;

public sealed class CameraPanel : StackPanel, IDisposable
{
    public sealed record SavedView(string Name, float[] Camera)
    {
        public override string ToString() => Name;
    }
    private readonly GeoKernelViewer3DControl _viewer;
    private readonly ComboBox _presets = new();
    private readonly CheckBox _smooth = new() { Content = "Smooth transition (1.2 seconds)", IsChecked = true, Margin = new Thickness(0, 8, 0, 8) };
    private readonly ComboBox _views = new();
    private readonly TextBox _name = new() { ToolTip = "Saved view name" };
    private readonly TextBlock _status = new() { TextWrapping = TextWrapping.Wrap };
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private readonly string _file;
    private float[]? _home;
    public bool Ready => _home != null;

    public CameraPanel(GeoKernelViewer3DControl viewer, string? savedViewsPath = null)
    {
        _viewer = viewer;
        _file = savedViewsPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GeoKernel", "CameraNavigation.Wpf", "sagrada-camera-views-v1.json");
        foreach (var name in new[] { "Overview", "Top view", "East view", "West view", "Close oblique" }) _presets.Items.Add(name);
        _presets.SelectedIndex = 0;
        Children.Add(_presets);
        AddButton("Go to preset", () => GoPreset(_presets.SelectedIndex));
        Children.Add(_smooth);
        _smooth.Checked += (_, _) => _viewer.StopCamera();
        _smooth.Unchecked += (_, _) => _viewer.StopCamera();
        AddButton("Stop transition", _viewer.StopCamera);
        AddButton("Zoom in", () => Zoom(.7f));
        AddButton("Zoom out", () => Zoom(1 / .7f));
        AddButton("North up", () => { var c = _viewer.GetCamera(); c[0] = 0; MoveTo(c); });
        Children.Add(new TextBlock { Text = "Saved views" });
        Children.Add(_views);
        Children.Add(new TextBlock { Text = "Name" });
        Children.Add(_name);
        AddButton("Save current view", () => SaveView(_name.Text));
        AddButton("Restore saved view", RestoreView);
        AddButton("Delete saved view", DeleteView);
        Children.Add(_status);
        Children.Add(new TextBlock { Text = "Mouse or keyboard input stops transitions.\nViews belong to this sample dataset.", TextWrapping = TextWrapping.Wrap });
        try
        {
            if (File.Exists(_file))
                foreach (var view in JsonSerializer.Deserialize<List<SavedView>>(File.ReadAllText(_file)) ?? [])
                    if (view != null && _views.Items.Count < 20 && !string.IsNullOrWhiteSpace(view.Name) && Valid(view.Camera)) _views.Items.Add(view);
            if (_views.Items.Count > 0) _views.SelectedIndex = 0;
        }
        catch (Exception ex) when (ex is IOException or JsonException or UnauthorizedAccessException)
        { _status.Text = "Saved views could not be read."; }
        _timer.Tick += UpdateTelemetry;
        _timer.Start();
    }

    private void UpdateTelemetry(object? sender, EventArgs e)
    {
        if (!Ready || !IsEnabled) return;
        var c = _viewer.GetCamera();
        _status.Text = $"Heading: {(c[0] % 360 + 360) % 360:F1} degrees\nTilt: {c[1]:F1} degrees";
    }
    private void AddButton(string text, Action action)
    {
        var button = new Button { Content = text, Margin = new Thickness(0, 4, 0, 4), Padding = new Thickness(6) };
        button.Click += (_, _) => {
            try { action(); }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Camera navigation", MessageBoxButton.OK, MessageBoxImage.Error); }
        };
        Children.Add(button);
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
    private void MoveTo(float[] c) => _viewer.MoveCamera(c, _smooth.IsChecked == true ? 1200 : 0);
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
    public void Dispose() { _timer.Stop(); _timer.Tick -= UpdateTelemetry; }
}
