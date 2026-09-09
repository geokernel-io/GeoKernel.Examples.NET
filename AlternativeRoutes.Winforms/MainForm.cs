using System.Drawing.Drawing2D;
using GeoKernel.Examples.AlternativeRoutes;
using GeoKernel.Examples.Common;
using GeoKernel.NET.WinForms;

namespace GeoKernel.AlternativeRoutes.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewerControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly ListBox _alternatives = new() { Dock = DockStyle.Fill };
    private readonly ListBox _directions = new() { Dock = DockStyle.Fill };
    private readonly Label _summary = new() { Dock = DockStyle.Fill, Text = "Select a start and finish point." };
    private readonly ToolStripStatusLabel _status = new();
    private readonly Button _selectButton = new() { Text = "Select route points", AutoSize = true, Enabled = false };
    private readonly RouteOverlayWindow _overlay;
    private AlternativeRoutingEngine? _engine;
    private IReadOnlySet<int> _mainComponent = new HashSet<int>();
    private RoutePoint? _startPoint, _finishPoint;
    private int _startNode = -1;
    private IReadOnlyList<AlternativeRoute> _routes = [];

    public MainForm()
    {
        Text = "AlternativeRoutes"; Width = 1200; Height = 760; StartPosition = FormStartPosition.CenterScreen;
        var icon = Path.Combine(AppContext.BaseDirectory, "resources", "GeoKernelAppIcon.ico"); if (File.Exists(icon)) Icon = new Icon(icon);
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 34, WrapContents = false };
        toolbar.Controls.Add(_selectButton);
        toolbar.Controls.Add(new Label { AutoSize = true, Padding = new Padding(8, 7, 0, 0), Text = "●", ForeColor = Color.FromArgb(22, 163, 74) });
        toolbar.Controls.Add(new Label { AutoSize = true, Padding = new Padding(0, 7, 8, 0), Text = "Start" });
        toolbar.Controls.Add(new Label { AutoSize = true, Padding = new Padding(0, 7, 0, 0), Text = "●", ForeColor = Color.FromArgb(220, 38, 38) });
        toolbar.Controls.Add(new Label { AutoSize = true, Padding = new Padding(0, 7, 0, 0), Text = "Finish" });
        var side = new TableLayoutPanel { Dock = DockStyle.Right, Width = 300, Padding = new Padding(10), RowCount = 6 };
        side.RowStyles.Add(new RowStyle(SizeType.Absolute, 22)); side.RowStyles.Add(new RowStyle(SizeType.Absolute, 55));
        side.RowStyles.Add(new RowStyle(SizeType.Absolute, 150)); side.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));
        side.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); side.RowStyles.Add(new RowStyle(SizeType.Absolute, 0));
        side.Controls.Add(new Label { Text = "Alternative routes", Font = new Font(Font, FontStyle.Bold), Dock = DockStyle.Fill }, 0, 0);
        side.Controls.Add(_summary, 0, 1); side.Controls.Add(_alternatives, 0, 2);
        side.Controls.Add(new Label { Text = "Road directions", Font = new Font(Font, FontStyle.Bold), Dock = DockStyle.Fill }, 0, 3);
        side.Controls.Add(_directions, 0, 4);
        var statusBar = new StatusStrip(); statusBar.Items.Add(_status);
        Controls.Add(_viewer); Controls.Add(side); Controls.Add(toolbar); Controls.Add(statusBar);
        _overlay = new RouteOverlayWindow(this, _viewer);
        Shown += LoadSample; Move += (_, _) => _overlay.SyncBounds(); Resize += (_, _) => _overlay.SyncBounds();
        FormClosed += (_, _) => _overlay.Close(); _viewer.VisibleExtentChanged += (_, _) => _overlay.Invalidate();
        _viewer.MapMouseUp += MapClicked; _selectButton.Click += (_, _) => BeginSelection();
        _alternatives.SelectedIndexChanged += (_, _) => SelectAlternative(_alternatives.SelectedIndex);
    }

    private async void LoadSample(object? sender, EventArgs e)
    {
        UseWaitCursor = true;
        _selectButton.Enabled = false;
        _status.Text = "Loading Stockholm road network...";
        await Task.Yield();
        try
        {
            _viewer.ActiveTool = GeoKernelViewerTool.Pan;
            var path = SampleData.EnsureSampleFile(new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/stockholm.zip"),
                "stockholm.zip", "stockholm", "stockholm.shp", this);
            if (string.IsNullOrEmpty(path) || !_viewer.AddLayerFile(path)) return;
            _viewer.SetLayerCoordinateSystemPreset(0, GeoKernelCoordinateSystemPreset.Wgs84);
            _viewer.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.WebMercator);
            _viewer.SetLayerStyle(0, new GeoKernelLayerStyle { LineColor = "#718684", LineWidth = 1 });
            if (!_viewer.BuildRoutingGraphForLayer(0, 1e-6, true, "maxspeed", "name", "oneway", 50))
                throw new InvalidOperationException("Routing graph could not be built.");
            _status.Text = "Preparing routing graph...";
            var snapshot = await Task.Run(() => _viewer.GetRoutingGraphSnapshot())
                ?? throw new InvalidOperationException("Routing graph is unavailable.");
            var prepared = await Task.Run(() =>
            {
                var engine = new AlternativeRoutingEngine(
                    snapshot.Nodes.Select(node => new RouteNode(node.Id, new RoutePoint(node.Position.X, node.Position.Y))),
                    snapshot.Edges.Select(edge => new RouteEdge(edge.Id, edge.FromId, edge.ToId, edge.Distance, edge.SpeedKmh,
                        edge.Geometry.Select(point => new RoutePoint(point.X, point.Y)).ToArray(),
                        edge.Attributes.TryGetValue("name", out var name) ? name?.ToString() ?? "" : "")));
                return (Engine: engine, Component: engine.LargestConnectedComponent());
            });
            _engine = prepared.Engine;
            _mainComponent = prepared.Component;
            if (_mainComponent.Count == 0) throw new InvalidOperationException("The main connected road network could not be identified.");
            var extent = _viewer.GetLayerInfo(0)?.ProjectedExtent; if (extent is not null) _viewer.ViewExtent = extent.Value;
            _selectButton.Enabled = true; _overlay.Show(this); BeginSelection();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { UseWaitCursor = false; }
    }

    private void BeginSelection()
    {
        _startPoint = _finishPoint = null; _startNode = -1; _routes = [];
        _alternatives.Items.Clear(); _directions.Items.Clear(); _summary.Text = "Select a start and finish point.";
        _overlay.SetState(null, null, [], 0); _viewer.ActiveTool = GeoKernelViewerTool.Route;
        _status.Text = "Click the map to choose the start point.";
    }

    private void MapClicked(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (_engine is null || e.Tool != GeoKernelViewerTool.Route || (e.ButtonOrButtons & 1) == 0) return;
        var source = AlternativeRoutingEngine.ToWgs84(new RoutePoint(e.WorldPoint.X, e.WorldPoint.Y));
        var component = _startPoint.HasValue && !_finishPoint.HasValue ? _engine.ReachableNodes(_startNode) : _mainComponent;
        var snapped = _engine.NearestNode(component, source, 2000);
        if (snapped is null) { MessageBox.Show(this, "No road node was found near the selected point.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var world = AlternativeRoutingEngine.ToWebMercator(snapped.Position);
        if (!_startPoint.HasValue || _finishPoint.HasValue)
        {
            _startPoint = world; _finishPoint = null; _startNode = snapped.Id; _routes = [];
            _alternatives.Items.Clear(); _directions.Items.Clear(); _summary.Text = "Select the finish point.";
            _overlay.SetState(_startPoint, null, [], 0); _status.Text = "Start selected. Click the map to choose the finish point."; return;
        }
        _finishPoint = world; _routes = _engine.FindAlternatives(_startNode, snapped.Id);
        if (_routes.Count == 0) { MessageBox.Show(this, "No connected route was found.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        _alternatives.Items.Clear();
        for (var index = 0; index < _routes.Count; index++)
            _alternatives.Items.Add($"{index + 1}. {_routes[index].Distance / 1000:0.00} km  •  {_routes[index].Time / 60:0.0} min");
        _overlay.SetState(_startPoint, _finishPoint, _routes, 0); _alternatives.SelectedIndex = 0;
        _status.Text = $"{_routes.Count} alternative route(s) found.";
    }

    private void SelectAlternative(int index)
    {
        if (_engine is null || index < 0 || index >= _routes.Count) return;
        var route = _routes[index]; _summary.Text = $"Alternative {index + 1}\r\n{route.Distance / 1000:0.00} km  •  {route.Time / 60:0.0} min";
        _directions.Items.Clear(); var steps = _engine.RoadSteps(route);
        for (var step = 0; step < steps.Count; step++)
            _directions.Items.Add($"{step + 1}. {steps[step].Name}\r\n    {(steps[step].Distance >= 1000 ? $"{steps[step].Distance / 1000:0.0} km" : $"{steps[step].Distance:0} m")}");
        _overlay.SetState(_startPoint, _finishPoint, _routes, index);
    }

    private sealed class RouteOverlayWindow : Form
    {
        private readonly Form _owner; private readonly GeoKernelViewerControl _viewer;
        private RoutePoint? _start, _finish; private IReadOnlyList<AlternativeRoute> _routes = []; private int _active;
        public RouteOverlayWindow(Form owner, GeoKernelViewerControl viewer)
        {
            _owner = owner; _viewer = viewer; FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false;
            BackColor = Color.Fuchsia; TransparencyKey = Color.Fuchsia; StartPosition = FormStartPosition.Manual;
        }
        protected override bool ShowWithoutActivation => true;
        protected override CreateParams CreateParams { get { var value = base.CreateParams; value.ExStyle |= 0x08000020 | 0x80; return value; } }
        public void SyncBounds() { if (!Visible || !_viewer.IsHandleCreated) return; var origin = _viewer.PointToScreen(Point.Empty); Bounds = new Rectangle(origin, _viewer.ClientSize); Invalidate(); }
        public void SetState(RoutePoint? start, RoutePoint? finish, IReadOnlyList<AlternativeRoute> routes, int active)
        { _start = start; _finish = finish; _routes = routes; _active = active; SyncBounds(); Invalidate(); }
        protected override void OnShown(EventArgs e) { base.OnShown(e); SyncBounds(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            for (var index = 0; index < _routes.Count; index++) if (index != _active) DrawRoute(e.Graphics, _routes[index], index, false);
            if (_active >= 0 && _active < _routes.Count) DrawRoute(e.Graphics, _routes[_active], _active, true);
            DrawMarker(e.Graphics, _start, Color.FromArgb(34, 197, 94), Color.FromArgb(20, 83, 45));
            DrawMarker(e.Graphics, _finish, Color.FromArgb(239, 68, 68), Color.FromArgb(127, 29, 29));
        }
        private void DrawRoute(Graphics graphics, AlternativeRoute route, int index, bool active)
        {
            if (route.WorldGeometry.Count < 2) return; using var path = new GraphicsPath(); var first = Screen(route.WorldGeometry[0]); path.StartFigure(); path.AddLine(first, first);
            for (var i = 1; i < route.WorldGeometry.Count; i++) { var next = Screen(route.WorldGeometry[i]); path.AddLine(first, next); first = next; }
            var colors = new[] { Color.FromArgb(37, 99, 235), Color.FromArgb(249, 115, 22), Color.FromArgb(147, 51, 234) };
            using var pen = new Pen(Color.FromArgb(active ? 255 : 135, colors[index % 3]), active ? 5 : 3) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round }; graphics.DrawPath(pen, path);
        }
        private void DrawMarker(Graphics graphics, RoutePoint? point, Color fill, Color outline)
        { if (!point.HasValue) return; var screen = Screen(point.Value); using var brush = new SolidBrush(fill); using var pen = new Pen(outline, 2); graphics.FillEllipse(brush, screen.X - 8, screen.Y - 8, 16, 16); graphics.DrawEllipse(pen, screen.X - 8, screen.Y - 8, 16, 16); }
        private PointF Screen(RoutePoint point) { var value = _viewer.WorldToScreen(point.X, point.Y); return new PointF((float)value.X, (float)value.Y); }
    }
}
