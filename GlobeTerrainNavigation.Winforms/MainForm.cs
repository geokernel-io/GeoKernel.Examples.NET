using GeoKernel.NET.WinForms;

namespace GeoKernel.GlobeTerrainNavigation.Winforms;

public sealed class MainForm : Form
{
    private static readonly GlobeCamera Home = new(2.1744, 41.4036, 40, 2500, 0.65 * 180 / Math.PI, 0);
    private readonly GeoKernelGlobeControl _globe = new() { Dock = DockStyle.Fill };
    private readonly ToolStripButton _atmosphere = new("Atmosphere") { CheckOnClick = true };
    private readonly ToolStripButton _esri = new("Esri Satellite") { CheckOnClick = true };
    private readonly ToolStripButton _dem = new("DEM") { CheckOnClick = true };
    private readonly ToolStripButton _sunlight = new("Sunlight") { CheckOnClick = true };
    private readonly ToolStripStatusLabel _status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly System.Windows.Forms.Timer _statusTimer = new() { Interval = 250 };
    private int _esriId = -1;

    public MainForm()
    {
        Text = "GeoKernel — Terrain & Navigation";
        ClientSize = new Size(1200, 800);
        MinimumSize = new Size(640, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        // Match the Qt example's oblique Sagrada Familia view and real terrain.
        _globe.StartupCamera = Home;
        _globe.StartupOptions = new GlobeOptions
        {
            NetworkEnabled = true,
            TerrainEnabled = true,
            AtmosphereEnabled = true,
            SunlightEnabled = true,
            TerrainMaxZoom = 14,
            TerrainLerc = true
        };
        var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Enabled = false };
        toolbar.Items.AddRange([_atmosphere, _esri, _dem, _sunlight]);
        var navigation = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Enabled = false };
        (string Text, GlobeNavigation Action)[] actions =
        [
            ("Zoom in", GlobeNavigation.ZoomIn),
            ("Zoom out", GlobeNavigation.ZoomOut),
            ("Rotate left", GlobeNavigation.RotateLeft),
            ("Rotate right", GlobeNavigation.RotateRight),
            ("Tilt up", GlobeNavigation.TiltUp),
            ("Tilt down", GlobeNavigation.TiltDown),
            ("Top view", GlobeNavigation.Top),
            ("North", GlobeNavigation.North),
            ("Overview", GlobeNavigation.Home)
        ];
        foreach (var (text, action) in actions)
        {
            var button = new ToolStripButton(text);
            button.Click += (_, _) => _globe.Globe.Navigate(action);
            navigation.Items.Add(button);
        }
        _esri.ToolTipText = "Esri World Imagery; internet required";
        _dem.ToolTipText = "Esri Terrain3D elevation, 1x; internet required";
        var statusBar = new StatusStrip { AutoSize = false, Height = 24, SizingGrip = false };
        statusBar.Items.Add(_status);
        _status.Text = "Preparing globe…";
        Controls.Add(_globe);
        Controls.Add(navigation);
        Controls.Add(toolbar);
        Controls.Add(statusBar);

        _globe.GlobeReady += (_, _) =>
        {
            _esriId = _globe.Globe.GetBasemaps().FirstOrDefault(map => map.Name == "Esri Satellite")?.Id ?? -1;
            _globe.Globe.Options = _globe.Globe.Options with { Basemap = _esriId };
            _globe.Globe.SetHomeCamera(Home);
            toolbar.Enabled = true;
            navigation.Enabled = true;
            _esri.Enabled = _esriId >= 0;
            UpdateStatus();
            _statusTimer.Start();
        };
        _atmosphere.Click += ApplyOptions;
        _esri.Click += ApplyOptions;
        _dem.Click += ApplyOptions;
        _sunlight.Click += ApplyOptions;
        _statusTimer.Tick += (_, _) => UpdateStatus();
    }

    private void ApplyOptions(object? sender, EventArgs e)
    {
        _globe.Globe.Options = _globe.Globe.Options with
        {
            AtmosphereEnabled = _atmosphere.Checked,
            Basemap = _esri.Checked ? _esriId : -1,
            TerrainEnabled = _dem.Checked,
            SunlightEnabled = _sunlight.Checked
        };
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var options = _globe.Globe.Options;
        _atmosphere.Checked = options.AtmosphereEnabled;
        _esri.Checked = _esriId >= 0 && options.Basemap == _esriId;
        _dem.Checked = options.TerrainEnabled;
        _sunlight.Checked = options.SunlightEnabled;

        var messages = new List<string>();
        if (options.Basemap >= 0) messages.Add(_globe.Globe.GetAttribution());
        if (options.TerrainEnabled) messages.Add("Terrain: Esri / source contributors");
        messages.AddRange(_globe.Globe.GetState().Errors.Values.Where(error => !string.IsNullOrWhiteSpace(error)));
        _status.Text = messages.Count > 0
            ? string.Join(" | ", messages)
            : "Left: pan | Right / wheel: zoom | Middle / Ctrl: orbit";
        _status.ToolTipText = _status.Text;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _statusTimer.Dispose();
        base.Dispose(disposing);
    }
}
