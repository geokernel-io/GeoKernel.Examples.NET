using GeoKernel.NET.WinForms;

namespace GeoKernel.GlobeVectorStyling.Winforms;

public sealed class MainForm : Form
{
    private const string LayerId = "buildings";
    private readonly GeoKernelGlobeControl _globe = new() { Dock = DockStyle.Fill };
    private readonly ToolStrip _toolbar = new() { GripStyle = ToolStripGripStyle.Hidden, Enabled = false };
    private readonly ToolStripButton _fill = new("Fill") { CheckOnClick = true, Checked = true };
    private readonly ToolStripButton _labels = new("Labels") { CheckOnClick = true };
    private readonly ToolStripComboBox _category = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
    private readonly ToolStripComboBox _labelField = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 100 };
    private readonly TrackBar _opacity = new() { Minimum = 15, Maximum = 100, Value = 60, TickStyle = TickStyle.None, Width = 100, Height = 24 };
    private readonly ToolStripButton _esri = new("Esri") { CheckOnClick = true, Checked = true };
    private readonly ToolStripStatusLabel _status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly CancellationTokenSource _closing = new();
    private GlobeCamera? _home;
    private Color _baseColor = Color.FromArgb(239, 184, 76);
    private int _esriId = -1;
    private bool _loaded;

    public MainForm()
    {
        Text = "GeoKernel — Sagrada Familia Building Styles";
        ClientSize = new Size(1200, 800);
        MinimumSize = new Size(900, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        _globe.StartupCamera = new GlobeCamera(2.1744, 41.4036, 0, 2500, 75.63, 34.38);
        _globe.StartupOptions = new GlobeOptions { NetworkEnabled = true, AtmosphereEnabled = true };

        var color = new ToolStripButton("Base color…");
        var zoom = new ToolStripButton("Zoom to buildings");
        _toolbar.Items.AddRange([_fill, _labels, new ToolStripLabel("Color by:"), _category,
            new ToolStripLabel("Label:"), _labelField, new ToolStripLabel("Opacity:"),
            new ToolStripControlHost(_opacity) { AutoSize = false, Size = new Size(100, 24) }, color, zoom, _esri]);
        var statusBar = new StatusStrip { AutoSize = false, Height = 24, SizingGrip = false };
        statusBar.Items.Add(_status);
        Controls.Add(_globe);
        Controls.Add(_toolbar);
        Controls.Add(statusBar);
        Report("Preparing globe…");

        _globe.GlobeReady += async (_, _) => await LoadBuildingsAsync();
        _fill.Click += async (_, _) => await ApplyStyleAsync();
        _labels.Click += async (_, _) => await ApplyStyleAsync();
        _category.SelectedIndexChanged += async (_, _) => await ApplyStyleAsync();
        _labelField.SelectedIndexChanged += async (_, _) => await ApplyStyleAsync();
        _opacity.ValueChanged += (_, _) =>
        {
            if (_loaded) _globe.Globe.SetLayerOpacity(LayerId, _opacity.Value / 100.0);
        };
        color.Click += async (_, _) =>
        {
            using var dialog = new ColorDialog { Color = _baseColor, FullOpen = true };
            if (dialog.ShowDialog(this) != DialogResult.OK) return;
            _baseColor = dialog.Color;
            await ApplyStyleAsync();
        };
        zoom.Click += (_, _) => { if (_home is not null) _globe.Globe.Camera = _home; };
        _esri.Click += (_, _) =>
        {
            _globe.Globe.SetBasemap(_esri.Checked ? _esriId : -1);
            ShowSummary();
        };
    }

    private GlobeVectorStyle CurrentStyle() => new()
    {
        Fill = _fill.Checked,
        Labels = _labels.Checked,
        CategoryField = _category.SelectedIndex > 0 ? _category.Text : "",
        LabelField = _labelField.Text,
        BaseColor = ColorTranslator.ToHtml(_baseColor),
        LabelSize = 12
    };

    private async Task LoadBuildingsAsync()
    {
        try
        {
            var globe = _globe.Globe;
            _esriId = globe.GetBasemaps().FirstOrDefault(map => map.Name == "Esri Satellite")?.Id ?? -1;
            globe.SetBasemap(_esriId);
            globe.EventReceived += (_, e) =>
            {
                if (e.Type != "featurePicked" || !e.Data.GetProperty("hit").GetBoolean()) return;
                string id = e.Data.GetProperty("featureId").ToString();
                var info = globe.GetFeatureInfo(LayerId, id);
                Report($"Building {id}: " + string.Join(" | ", info.Attributes.Select(pair => $"{pair.Key}: {pair.Value}")));
            };
            string path = await SampleData.EnsureBuildingsAsync(new Progress<string>(Report), _closing.Token);
            _closing.Token.ThrowIfCancellationRequested();
            Report("Loading Sagrada Familia buildings…");
            await WaitAsync(globe.LoadVector(new GlobeVectorRequest(LayerId, path)
            {
                SourceCrs = "EPSG:25831",
                Style = new GlobeVectorStyle { CategoryField = "type", LabelField = "name", BaseColor = "#efb84c", LabelSize = 12 }
            }));
            var stats = globe.GetLayerState(LayerId).Statistics;
            var fields = stats.GetProperty("fields").EnumerateArray().Select(field => field.GetString()!).ToArray();
            if (!fields.Contains("type") || !fields.Contains("name"))
                throw new InvalidDataException("Expected building fields 'type' and 'name' are missing.");
            _category.Items.Add("Uniform");
            _category.Items.AddRange(fields);
            _labelField.Items.AddRange(fields);
            _category.SelectedItem = "type";
            _labelField.SelectedItem = "name";
            globe.SetLayerOpacity(LayerId, _opacity.Value / 100.0);
            globe.FocusLayer(LayerId);
            _home = globe.Camera with
            {
                DistanceMetres = globe.Camera.DistanceMetres * 0.28,
                PitchDegrees = 1.32 * 180 / Math.PI,
                HeadingDegrees = 0.60 * 180 / Math.PI
            };
            globe.Camera = _home;
            globe.SetHomeCamera(_home);
            globe.SetInteractionMode(GlobeInteraction.Identify);
            _loaded = true;
            _toolbar.Enabled = true;
            _esri.Enabled = _esriId >= 0;
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
    }

    private async Task ApplyStyleAsync()
    {
        if (!_loaded || !_toolbar.Enabled) return;
        _toolbar.Enabled = false;
        try
        {
            Report("Applying building style…");
            await WaitAsync(_globe.Globe.SetVectorStyle(LayerId, CurrentStyle()));
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
        finally { if (!_closing.IsCancellationRequested) _toolbar.Enabled = true; }
    }

    private async Task WaitAsync(ulong job)
    {
        var globe = _globe.Globe;
        try { await globe.WaitForJobAsync(job, _closing.Token); }
        finally { if (!globe.IsDisposed) globe.ForgetJob(job); }
    }

    private void ShowSummary()
    {
        var stats = _globe.Globe.GetLayerState(LayerId).Statistics;
        Report($"{stats.GetProperty("features")} buildings | {stats.GetProperty("outlineOnly")} outline-only | Click to identify | {_globe.Globe.GetAttribution()}");
    }

    private void Report(string message)
    {
        if (IsDisposed || _closing.IsCancellationRequested) return;
        _status.Text = message;
        _status.ToolTipText = message;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _closing.Cancel();
        base.Dispose(disposing);
    }
}
