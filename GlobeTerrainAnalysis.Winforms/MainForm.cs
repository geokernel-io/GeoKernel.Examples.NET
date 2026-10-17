using GeoKernel.NET.WinForms;

namespace GeoKernel.GlobeTerrainAnalysis.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelGlobeControl _globe = new() { Dock = DockStyle.Fill };
    private readonly ToolStrip _toolbar = new() { GripStyle = ToolStripGripStyle.Hidden, Enabled = false };
    private readonly TextBox _result = new() { Dock = DockStyle.Bottom, Height = 170, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, WordWrap = true };
    private readonly ToolStripStatusLabel _status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    private readonly CancellationTokenSource _closing = new();
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 100 };
    private readonly List<GlobePoint> _points = [];
    private readonly ToolStripButton[] _tools;
    private readonly GlobeCamera _home = new(2.1744, 41.4036, 40, 2500, .85 * 180 / Math.PI, .6 * 180 / Math.PI);
    private int _mode = -1;
    private bool _loaded;
    private bool _analyzing;

    public MainForm()
    {
        Text = "GeoKernel — Barcelona Terrain Analysis";
        ClientSize = new Size(1200, 800);
        MinimumSize = new Size(850, 550);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        _globe.StartupCamera = _home;
        _globe.StartupOptions = new GlobeOptions { NetworkEnabled = true, AtmosphereEnabled = true };
        _tools = [new("Line of sight"), new("Viewshed"), new("Slope / aspect"), new("Measure")];
        for (int i = 0; i < _tools.Length; i++)
        {
            int mode = i;
            _tools[i].Click += (_, _) => SelectMode(mode);
            _toolbar.Items.Add(_tools[i]);
        }
        _toolbar.Items.Add("Clear", null, (_, _) => ClearSelection());
        _toolbar.Items.Add("Barcelona", null, (_, _) => _globe.Globe.Camera = _home);
        var statusBar = new StatusStrip { AutoSize = false, Height = 24, SizingGrip = false };
        statusBar.Items.Add(_status);
        Controls.Add(_globe);
        Controls.Add(_result);
        Controls.Add(_toolbar);
        Controls.Add(statusBar);
        _result.Text = "Choose an analysis tool, then click the terrain. Drag to navigate; Esc cancels selection.";
        Report("Loading Barcelona terrain…");
        _globe.GlobeReady += async (_, _) => await LoadTerrainAsync();
        _globe.GlobeEventReceived += OnGlobeEvent;
        _timer.Tick += (_, _) => { if (_analyzing) ShowAnalysis(); };
    }

    private async Task LoadTerrainAsync()
    {
        try
        {
            var globe = _globe.Globe;
            int esri = globe.GetBasemaps().FirstOrDefault(map => map.Name == "Esri Satellite")?.Id ?? -1;
            globe.SetBasemap(esri);
            string dem = await SampleData.EnsureRasterAsync("sagrada_familia_terrain",
                "69ed86c686b2500a70921920387afcb8993f3fe042e2a682932f86b7c6d5127c", new Progress<string>(Report), _closing.Token);
            _closing.Token.ThrowIfCancellationRequested();
            await WaitAsync(globe.LoadDem(new GlobeDemRequest("dem", dem)));
            var stats = globe.GetLayerState("dem").Statistics;
            globe.SetDemStyle(false, stats.GetProperty("minimum").GetDouble(), stats.GetProperty("maximum").GetDouble());
            globe.Options = globe.Options with { TerrainEnabled = true, TerrainExaggeration = 1 };
            globe.Camera = _home;
            globe.SetHomeCamera(_home);
            _loaded = true;
            _toolbar.Enabled = true;
            _timer.Start();
            Report($"Barcelona | Real DEM | Terrain only | {globe.GetAttribution()}");
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); _result.Text = error.Message; }
    }

    private void ClearSelection()
    {
        _analyzing = false;
        _points.Clear();
        _mode = -1;
        var globe = _globe.Globe;
        globe.CancelTerrainAnalysis();
        globe.ClearTerrainAnalysis();
        globe.ClearMeasurement();
        globe.SetInteractionMode(GlobeInteraction.Navigate);
        foreach (var tool in _tools) tool.Checked = false;
        _result.Text = "Selection cleared. Choose an analysis tool to select new points.";
    }

    private void SelectMode(int mode)
    {
        ClearSelection();
        _mode = mode;
        _tools[mode].Checked = true;
        _globe.Globe.SetInteractionMode(GlobeInteraction.GroundSelection);
        _result.Text = mode switch
        {
            0 => "Line of sight: click the observer, then the target within 10 km. Both are 1.7 m above ground. Terrain only; buildings excluded.",
            1 => "Viewshed: click an observer. Radius: 350 m; sample spacing: 15 m. Terrain only; buildings excluded.",
            2 => "Slope / aspect: click a terrain point. Slope and downhill direction will appear here.",
            _ => "Measure: click a start point, then an end point."
        };
    }

    private void OnGlobeEvent(object? sender, GlobeEvent e)
    {
        if (!_loaded || _closing.IsCancellationRequested) return;
        try
        {
            if (e.Type == "selectionFinished") { ClearSelection(); return; }
            if (e.Type != "groundClicked" || _mode < 0) return;
            if (!e.Data.GetProperty("heightAvailable").GetBoolean())
            {
                _result.Text = "DEM not loaded here. Wait for terrain or return to Barcelona and try again.";
                return;
            }
            AddPoint(new GlobePoint(e.Data.GetProperty("longitude").GetDouble(), e.Data.GetProperty("latitude").GetDouble()));
        }
        catch (Exception error) { _result.Text = error.Message; }
    }

    private void AddPoint(GlobePoint point)
    {
        var globe = _globe.Globe;
        int required = _mode is 0 or 3 ? 2 : 1;
        if (_points.Count == required)
        {
                _analyzing = false;
            globe.CancelTerrainAnalysis();
            globe.ClearTerrainAnalysis();
            globe.ClearMeasurement();
            _points.Clear();
        }
        if (_points.Count > 0)
        {
            globe.SetMeasurementPoints(new GlobeMeasurementRequest([_points[0], point]));
            double distance = globe.GetMeasurement().DistanceMetres;
            if (distance < 1 || (_mode == 0 && distance > 10000))
            {
                globe.SetMeasurementPoints(new GlobeMeasurementRequest(_points.ToArray()));
                _result.Text = distance < 1 ? "Choose a point at least 1 metre away." : "Choose a target within 10 km.";
                return;
            }
        }
        _points.Add(point);
        globe.SetMeasurementPoints(new GlobeMeasurementRequest(_points.ToArray()));
        if (_points.Count < required)
        {
            _result.Text = $"Start/observer: {point.Latitude:F6}, {point.Longitude:F6}. Now click the end/target point. Esc cancels.";
            return;
        }
        if (_mode == 3)
        {
            _result.Text = $"Ground distance: {globe.GetMeasurement().DistanceMetres:F2} m.\r\nClick to start a new measurement.";
            return;
        }
        RunAnalysis();
    }

    private void RunAnalysis()
    {
        try
        {
            var mode = _mode switch { 0 => GlobeAnalysisMode.LineOfSight, 1 => GlobeAnalysisMode.Viewshed, _ => GlobeAnalysisMode.Slope };
            _globe.Globe.StartTerrainAnalysis(new GlobeAnalysisRequest(mode, _points.ToArray())
            {
                SpacingMetres = 15, RadiusMetres = 350, ObserverHeightMetres = 1.7, TargetHeightMetres = 1.7
            });
            _analyzing = true;
            ShowAnalysis();
        }
        catch (Exception error) { _analyzing = false; _result.Text = error.Message; }
    }

    private void ShowAnalysis()
    {
        try
        {
            var analysis = _globe.Globe.GetTerrainAnalysis();
            _analyzing = analysis.Status == "calculating";
            if (analysis.Stale)
            {
                _result.Text = "Terrain changed while calculating. Wait for terrain loading to settle, then click to start a new selection. Previous results are no longer current.";
                return;
            }
            string text = $"Status: {analysis.Status}";
            if (analysis.Slope is { Available: true } slope)
                text += $"\r\nSlope: {slope.Degrees:F2}° ({slope.Percent:F2}%)\r\nAspect: {(slope.AspectDegrees is double aspect ? $"{aspect:F1}° clockwise from north (downhill)" : "Undefined: flat terrain")}";
            else if (analysis.LineOfSight is { } los)
                text += $"\r\nLine of sight: {los.Outcome} | Distance: {los.LengthMetres:F1} m\r\nSamples: {los.Processed}/{los.Total}; missing: {los.MissingSamples}\r\nThe map line marks the ground path, not the elevated sight ray.";
            else if (analysis.Viewshed is { } viewshed)
                text += $"\r\nVisible: {viewshed.Clear}; blocked: {viewshed.Blocked}; unknown: {viewshed.Unknown}\r\nProcessed: {viewshed.Processed}/{viewshed.Total} | Radius: 350 m";
            else text += "\r\nResult unavailable; neighbouring DEM samples may be missing.";
            if (!string.IsNullOrWhiteSpace(analysis.Error)) text += "\r\n" + analysis.Error;
            _result.Text = text + "\r\nTerrain only. Click to start a new selection; Esc cancels.";
        }
        catch (Exception error) { _result.Text = error.Message; }
    }

    private async Task WaitAsync(ulong job)
    {
        var globe = _globe.Globe;
        try { await globe.WaitForJobAsync(job, _closing.Token); }
        finally { if (!globe.IsDisposed) globe.ForgetJob(job); }
    }

    private void Report(string message)
    {
        if (IsDisposed || _closing.IsCancellationRequested) return;
        _status.Text = message;
        _status.ToolTipText = message;
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) { _closing.Cancel(); _timer.Dispose(); }
        base.Dispose(disposing);
    }
}


