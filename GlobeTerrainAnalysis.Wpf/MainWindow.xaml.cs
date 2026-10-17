using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GlobeTerrainAnalysis.Wpf;

public partial class MainWindow : Window
{
    private readonly CancellationTokenSource _closing = new();
    private readonly DispatcherTimer _timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    private readonly List<GlobePoint> _points = [];
    private readonly ToggleButton[] _tools;
    private readonly GlobeCamera _home = new(2.1744, 41.4036, 40, 2500, .85 * 180 / Math.PI, .6 * 180 / Math.PI);
    private int _mode = -1;
    private bool _loaded;
    private bool _analyzing;

    public MainWindow()
    {
        InitializeComponent();
        GlobeView.StartupCamera = _home;
        GlobeView.StartupOptions = new GlobeOptions { NetworkEnabled = true, AtmosphereEnabled = true };
        _tools = [LineOfSight, Viewshed, Slope, MeasureTool];
        _timer.Tick += (_, _) => { if (_analyzing) ShowAnalysis(); };
    }

    private async void GlobeReady(object? sender, EventArgs e) => await LoadTerrainAsync();
    private void SelectTool(object sender, RoutedEventArgs e) => SelectMode(int.Parse((string)((ToggleButton)sender).Tag));
    private void ClearTool(object sender, RoutedEventArgs e) => ClearSelection();
    private void GoToBarcelona(object sender, RoutedEventArgs e) => GlobeView.Globe.Camera = _home;

    private async Task LoadTerrainAsync()
    {
        try
        {
            var globe = GlobeView.Globe;
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
            Toolbar.IsEnabled = true;
            _timer.Start();
            Report($"Barcelona | Real DEM | Terrain only | {globe.GetAttribution()}");
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); Result.Text = error.Message; }
    }

    private void ClearSelection()
    {
        _analyzing = false;
        _points.Clear();
        _mode = -1;
        var globe = GlobeView.Globe;
        globe.CancelTerrainAnalysis();
        globe.ClearTerrainAnalysis();
        globe.ClearMeasurement();
        globe.SetInteractionMode(GlobeInteraction.Navigate);
        foreach (var tool in _tools) tool.IsChecked = false;
        Result.Text = "Selection cleared. Choose an analysis tool to select new points.";
    }

    private void SelectMode(int mode)
    {
        ClearSelection();
        _mode = mode;
        _tools[mode].IsChecked = true;
        GlobeView.Globe.SetInteractionMode(GlobeInteraction.GroundSelection);
        Result.Text = mode switch
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
                Result.Text = "DEM not loaded here. Wait for terrain or return to Barcelona and try again.";
                return;
            }
            AddPoint(new GlobePoint(e.Data.GetProperty("longitude").GetDouble(), e.Data.GetProperty("latitude").GetDouble()));
        }
        catch (Exception error) { Result.Text = error.Message; }
    }

    private void AddPoint(GlobePoint point)
    {
        var globe = GlobeView.Globe;
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
                Result.Text = distance < 1 ? "Choose a point at least 1 metre away." : "Choose a target within 10 km.";
                return;
            }
        }
        _points.Add(point);
        globe.SetMeasurementPoints(new GlobeMeasurementRequest(_points.ToArray()));
        if (_points.Count < required)
        {
            Result.Text = $"Start/observer: {point.Latitude:F6}, {point.Longitude:F6}. Now click the end/target point. Esc cancels.";
            return;
        }
        if (_mode == 3)
        {
            Result.Text = $"Ground distance: {globe.GetMeasurement().DistanceMetres:F2} m.\r\nClick to start a new measurement.";
            return;
        }
        RunAnalysis();
    }

    private void RunAnalysis()
    {
        try
        {
            var mode = _mode switch { 0 => GlobeAnalysisMode.LineOfSight, 1 => GlobeAnalysisMode.Viewshed, _ => GlobeAnalysisMode.Slope };
            GlobeView.Globe.StartTerrainAnalysis(new GlobeAnalysisRequest(mode, _points.ToArray())
            {
                SpacingMetres = 15, RadiusMetres = 350, ObserverHeightMetres = 1.7, TargetHeightMetres = 1.7
            });
            _analyzing = true;
            ShowAnalysis();
        }
        catch (Exception error) { _analyzing = false; Result.Text = error.Message; }
    }

    private void ShowAnalysis()
    {
        try
        {
            var analysis = GlobeView.Globe.GetTerrainAnalysis();
            _analyzing = analysis.Status == "calculating";
            if (analysis.Stale)
            {
                Result.Text = "Terrain changed while calculating. Wait for terrain loading to settle, then click to start a new selection. Previous results are no longer current.";
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
            Result.Text = text + "\r\nTerrain only. Click to start a new selection; Esc cancels.";
        }
        catch (Exception error) { Result.Text = error.Message; }
    }

    private async Task WaitAsync(ulong job)
    {
        var globe = GlobeView.Globe;
        try { await globe.WaitForJobAsync(job, _closing.Token); }
        finally { if (!globe.IsDisposed) globe.ForgetJob(job); }
    }

    private void Report(string message)
    {
        if (!_closing.IsCancellationRequested) Status.Text = message;
    }

    protected override void OnClosed(EventArgs e)
    {
        _closing.Cancel();
        _timer.Stop();
        GlobeView.Dispose();
        base.OnClosed(e);
    }
}

