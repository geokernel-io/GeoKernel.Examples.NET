using GeoKernel.NET.WinForms;
using GeoKernel.Examples.Common;

namespace GeoKernel.Examples.Routing.WinForms;

public class RoutingSampleForm : Form
{
    private readonly GeoKernelViewerControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly ListBox _routes = new() { Dock = DockStyle.Fill };
    private readonly ListBox _directions = new() { Dock = DockStyle.Fill };
    private readonly Label _summary = new() { Dock = DockStyle.Top, Height = 72 };
    private readonly Label _status = new() { Dock = DockStyle.Bottom, Height = 22 };
    private readonly NumericUpDown _vehicles = new() { Minimum = 1, Maximum = 20, Value = 3, Width = 55 };
    private readonly Button _calculate = new() { Text = "Calculate route", AutoSize = true };
    private readonly Button _reset = new() { Text = "New route", AutoSize = true };
    private readonly List<GeoKernelPoint> _points = [];
    private readonly List<List<string>> _vehicleDirections = [];
    private readonly string _sample = System.Reflection.Assembly.GetEntryAssembly()!.GetName().Name!;
    private int _roadLayerIndex;

    public RoutingSampleForm()
    {
        Text = _sample; Width = 1200; Height = 800; StartPosition = FormStartPosition.CenterScreen;
        var iconPath = Path.Combine(AppContext.BaseDirectory, "resources", "GeoKernelAppIcon.ico");
        if (File.Exists(iconPath)) Icon = new Icon(iconPath);
        if (_sample.Contains("AlternativeRoutes", StringComparison.OrdinalIgnoreCase))
        {
            _reset.Text = "Select route points";
            _calculate.Visible = false;
        }
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 34, WrapContents = false };
        toolbar.Controls.AddRange([_reset, _calculate]);
        if (_sample.Contains("Optimization", StringComparison.OrdinalIgnoreCase))
        {
            toolbar.Controls.Add(new Label { Text = "Service vehicles:", AutoSize = true, Padding = new Padding(8, 7, 0, 0) });
            toolbar.Controls.Add(_vehicles);
        }
        toolbar.Controls.Add(new Label { Text = "  ● Start   ● Stop   ● Finish", AutoSize = true, Padding = new Padding(4, 7, 0, 0) });
        var panel = new TableLayoutPanel { Dock = DockStyle.Right, Width = 310, RowCount = 5 };
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 80)); panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 35)); panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 65));
        panel.Controls.Add(_summary, 0, 0); panel.Controls.Add(new Label { Text = "Routes / legs", Dock = DockStyle.Fill }, 0, 1);
        panel.Controls.Add(_routes, 0, 2); panel.Controls.Add(new Label { Text = "Road directions", Dock = DockStyle.Fill }, 0, 3);
        panel.Controls.Add(_directions, 0, 4);
        Controls.Add(_viewer); Controls.Add(panel); Controls.Add(toolbar); Controls.Add(_status);
        Shown += OnShown; _viewer.MapMouseUp += OnMapClicked; _reset.Click += (_, _) => BeginRouteSelection();
        _calculate.Click += (_, _) => Calculate(); _routes.SelectedIndexChanged += (_, _) => SelectRoute();
    }

    private void OnShown(object? sender, EventArgs e)
    {
        try
        {
            _viewer.ActiveTool = GeoKernelViewerTool.Pan;
            var path = SampleData.EnsureSampleFile(new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/stockholm.zip"),
                "stockholm.zip", "stockholm", "stockholm.shp", this);
            if (string.IsNullOrWhiteSpace(path) || !_viewer.AddLayerFile(path)) throw new InvalidOperationException("Stockholm road layer could not be loaded.");
            _roadLayerIndex = 0;
            _viewer.SetLayerCoordinateSystemPreset(0, GeoKernelCoordinateSystemPreset.Wgs84);
            _viewer.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.WebMercator);
            _viewer.SetLayerStyle(0, new GeoKernelLayerStyle { LineColor = "#718684", LineWidth = 1.0 });
            if (!_viewer.BuildRoutingGraphForLayer(0, 0.000001, true, "maxspeed", "name", "oneway", 50.0))
                throw new InvalidOperationException("Routing graph could not be built.");
            var stockholmExtent = _viewer.GetLayerInfo(0)?.ProjectedExtent;
            if (stockholmExtent is not null) _viewer.ViewExtent = stockholmExtent.Value;
            else _viewer.FullExtent();
            BeginRouteSelection();
            _status.Text = $"Stockholm graph ready: {_viewer.RoutingGraphNodeCount} nodes, {_viewer.RoutingGraphEdgeCount} directed edges";
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, _sample, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void ResetRoute()
    {
        while (_viewer.LayerCount > 1) _viewer.RemoveLayer(0);
        _roadLayerIndex = _viewer.LayerCount - 1; _points.Clear(); _routes.Items.Clear(); _directions.Items.Clear(); _vehicleDirections.Clear();
        _summary.Text = _sample.Contains("Optimization") ? "Click once for the depot, then add visit points." :
            _sample.Contains("MultiStop") ? "Click the start, intermediate stops, and finish." : "Click the start and finish points.";
        _calculate.Enabled = _sample.Contains("MultiStop") || _sample.Contains("Optimization");
    }

    private void BeginRouteSelection()
    {
        ResetRoute();
        _viewer.ActiveTool = GeoKernelViewerTool.Route;
        _status.Text = "Click the map to choose the start point.";
    }

    private void OnMapClicked(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if ((e.ButtonOrButtons & 1) == 0 || e.Tool != GeoKernelViewerTool.Route) return;
        if (!_sample.Contains("MultiStop") && !_sample.Contains("Optimization") && _points.Count == 2) ResetRoute();
        _points.Add(e.WorldPoint);
        _status.Text = _points.Count == 1 ? "Start selected. Click the finish point." : $"Point {_points.Count} selected.";
        if (!_calculate.Enabled && _points.Count == 2) Calculate();
    }

    private void Calculate()
    {
        if (_points.Count < 2) { _status.Text = "Select at least two points."; return; }
        while (_viewer.LayerCount > 1) _viewer.RemoveLayer(0);
        _routes.Items.Clear(); _directions.Items.Clear(); _vehicleDirections.Clear();
        if (_sample.Contains("Optimization")) CalculateVehicles(); else CalculateLegs(_points, 0, "Route");
    }

    private void CalculateVehicles()
    {
        var count = Math.Min((int)_vehicles.Value, _points.Count - 1);
        for (var vehicle = 0; vehicle < count; vehicle++)
        {
            var assigned = new List<GeoKernelPoint> { _points[0] };
            for (var visit = 1 + vehicle; visit < _points.Count; visit += count) assigned.Add(_points[visit]);
            assigned.Add(_points[0]);
            var before = _routes.Items.Count; CalculateLegs(assigned, vehicle, $"Vehicle {vehicle + 1}");
            var texts = Enumerable.Range(before, _routes.Items.Count - before).Select(i => _routes.Items[i]?.ToString() ?? "").ToList();
            _vehicleDirections.Add(texts);
        }
        _summary.Text = $"{count} vehicles • {_points.Count - 1} visits";
        if (_routes.Items.Count > 0) _routes.SelectedIndex = 0;
    }

    private void CalculateLegs(IReadOnlyList<GeoKernelPoint> points, int group, string name)
    {
        double distance = 0, seconds = 0;
        var succeeded = 0;
        for (var i = 1; i < points.Count; i++)
        {
            var result = _viewer.AddShortestRouteLayerBetweenPoints(points[i - 1], points[i], GeoKernelRoutingCostMetric.TravelTime,
                double.PositiveInfinity, 50, $"{name} {i}", false);
            if (!result.Succeeded) { _routes.Items.Add($"{name}: no connected route"); continue; }
            succeeded++;
            distance += result.TotalDistance; seconds += result.TotalTime;
            _routes.Items.Add($"{name} {i}: {result.TotalDistance / 1000:0.00} km • {result.TotalTime / 60:0.0} min");
            _directions.Items.Add($"{i}. Route segment • {result.TotalDistance:0} m");
            _viewer.SetLayerStyle(0, new GeoKernelLayerStyle { LineColor = RouteColor(group), LineWidth = 4.0 });
        }
        if (succeeded == 0)
        {
            _summary.Text = "No connected route was found.";
            _status.Text = "No connected route was found. Select a new start point.";
            return;
        }
        _summary.Text = $"{name}\r\n{distance / 1000:0.00} km • {seconds / 60:0.0} min";
        _status.Text = "Route calculated successfully.";
    }

    private void SelectRoute()
    {
        if (!_sample.Contains("Optimization") || _routes.SelectedIndex < 0) return;
        var text = _routes.SelectedItem?.ToString() ?? "";
        var separator = text.IndexOf(' '); var colon = text.IndexOf(':');
        if (separator < 0 || colon < 0 || !int.TryParse(text[(separator + 1)..colon].Split(' ')[0], out var vehicle)) return;
        _directions.Items.Clear(); foreach (var line in _vehicleDirections.ElementAtOrDefault(vehicle - 1) ?? []) _directions.Items.Add(line);
    }

    private static string RouteColor(int index) => new[] { "#2563EB", "#E53935", "#16A34A", "#F59E0B", "#7C3AED", "#0891B2" }[index % 6];
}
