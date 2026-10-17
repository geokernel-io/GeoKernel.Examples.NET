using GeoKernel.NET.WinForms;

namespace GeoKernel.GlobeHello.Winforms;

public sealed class MainForm : Form
{
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
        Text = "GeoKernel — Globe Hello";
        ClientSize = new Size(1100, 760);
        MinimumSize = new Size(640, 480);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

        // Match GlobeHello.Qt: a blank globe with independently enabled features.
        _globe.StartupOptions = new GlobeOptions { NetworkEnabled = true };
        var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Enabled = false };
        toolbar.Items.AddRange([_atmosphere, _esri, _dem, _sunlight]);
        _esri.ToolTipText = "Esri World Imagery; internet required";
        _dem.ToolTipText = "Esri Terrain3D elevation, 1x; internet required";
        var statusBar = new StatusStrip { AutoSize = false, Height = 24, SizingGrip = false };
        statusBar.Items.Add(_status);
        _status.Text = "Preparing globe…";
        Controls.Add(_globe);
        Controls.Add(toolbar);
        Controls.Add(statusBar);

        _globe.GlobeReady += (_, _) =>
        {
            _esriId = _globe.Globe.GetBasemaps().FirstOrDefault(map => map.Name == "Esri Satellite")?.Id ?? -1;
            toolbar.Enabled = true;
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
