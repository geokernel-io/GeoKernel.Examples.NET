using System.Drawing.Drawing2D;
using GeoKernel.Examples.Common;
using GeoKernel.NET.WinForms;

namespace GeoKernel.ShortestRoute.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewerControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly ListBox _directions = new() { Dock = DockStyle.Fill };
    private readonly Label _summary = new() { Dock = DockStyle.Fill, Text = "Select a start and finish point." };
    private readonly ToolStripStatusLabel _status = new();
    private readonly Button _selectButton = new() { Text = "Select route points", AutoSize = true, Enabled = false };
    private readonly RouteOverlayWindow _overlay;
    private ShortestRoutingEngine? _engine;
    private RoutePoint? _startPoint, _finishPoint;
    private int _startNode = -1;
    private ShortestRouteResult? _route;

    public MainForm()
    {
        Text = "ShortestRoute"; Width = 1200; Height = 760; StartPosition = FormStartPosition.CenterScreen;
        var icon = Path.Combine(AppContext.BaseDirectory, "resources", "GeoKernelAppIcon.ico"); if (File.Exists(icon)) Icon = new Icon(icon);
        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 34, WrapContents = false };
        toolbar.Controls.Add(_selectButton); AddLegend(toolbar, "Start", Color.FromArgb(34, 197, 94)); AddLegend(toolbar, "Finish", Color.FromArgb(239, 68, 68));
        var side = new TableLayoutPanel { Dock = DockStyle.Right, Width = 300, Padding = new Padding(10), RowCount = 4 };
        side.RowStyles.Add(new RowStyle(SizeType.Absolute, 22)); side.RowStyles.Add(new RowStyle(SizeType.Absolute, 65)); side.RowStyles.Add(new RowStyle(SizeType.Absolute, 26)); side.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        side.Controls.Add(new Label { Text = "Shortest route", Font = new Font(Font, FontStyle.Bold), Dock = DockStyle.Fill }, 0, 0); side.Controls.Add(_summary, 0, 1);
        side.Controls.Add(new Label { Text = "Road directions", Font = new Font(Font, FontStyle.Bold), Dock = DockStyle.Fill }, 0, 2); side.Controls.Add(_directions, 0, 3);
        var statusBar = new StatusStrip(); statusBar.Items.Add(_status); Controls.Add(_viewer); Controls.Add(side); Controls.Add(toolbar); Controls.Add(statusBar);
        _overlay = new RouteOverlayWindow(_viewer);
        Shown += LoadSample; Move += (_, _) => _overlay.SyncBounds(); Resize += (_, _) => _overlay.SyncBounds(); FormClosed += (_, _) => _overlay.Close();
        _viewer.VisibleExtentChanged += (_, _) => _overlay.Invalidate(); _viewer.MapMouseUp += MapClicked; _selectButton.Click += (_, _) => BeginSelection();
    }

    private static void AddLegend(Control parent, string text, Color color)
    { parent.Controls.Add(new Label { AutoSize = true, Padding = new Padding(8, 7, 0, 0), Text = "●", ForeColor = color }); parent.Controls.Add(new Label { AutoSize = true, Padding = new Padding(0, 7, 0, 0), Text = text }); }

    private async void LoadSample(object? sender, EventArgs e)
    {
        UseWaitCursor = true; _status.Text = "Loading Stockholm road network..."; await Task.Yield();
        try
        {
            _viewer.ActiveTool = GeoKernelViewerTool.Pan;
            var path = SampleData.EnsureSampleFile(new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/stockholm.zip"), "stockholm.zip", "stockholm", "stockholm.shp", this);
            if (string.IsNullOrEmpty(path) || !_viewer.AddLayerFile(path)) return;
            _viewer.SetLayerCoordinateSystemPreset(0, GeoKernelCoordinateSystemPreset.Wgs84); _viewer.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.WebMercator);
            _viewer.SetLayerStyle(0, new GeoKernelLayerStyle { LineColor = "#718684", LineWidth = 1 });
            if (!_viewer.BuildRoutingGraphForLayer(0, 1e-6, true, "maxspeed", "name", "oneway", 50)) throw new InvalidOperationException("Routing graph could not be built.");
            _status.Text = "Preparing routing graph...";
            var snapshot = await Task.Run(() => _viewer.GetRoutingGraphSnapshot()) ?? throw new InvalidOperationException("Routing graph is unavailable.");
            _engine = await Task.Run(() => new ShortestRoutingEngine(
                snapshot.Nodes.Select(node => new RouteNode(node.Id, new RoutePoint(node.Position.X, node.Position.Y))),
                snapshot.Edges.Select(edge => new RouteEdge(edge.Id, edge.FromId, edge.ToId, edge.Distance, edge.SpeedKmh,
                    edge.Geometry.Select(point => new RoutePoint(point.X, point.Y)).ToArray(), edge.Attributes.TryGetValue("name", out var name) ? name?.ToString() ?? "" : ""))));
            var extent = _viewer.GetLayerInfo(0)?.ProjectedExtent; if (extent is not null) _viewer.ViewExtent = extent.Value;
            _selectButton.Enabled = true; _overlay.Show(this); BeginSelection();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { UseWaitCursor = false; }
    }

    private void BeginSelection()
    { _startPoint = _finishPoint = null; _startNode = -1; _route = null; _directions.Items.Clear(); _summary.Text = "Select a start and finish point."; _overlay.SetState(null, null, null); _viewer.ActiveTool = GeoKernelViewerTool.Route; _status.Text = "Click the map to choose the start point."; }

    private void MapClicked(object? sender, GeoKernelMapMouseEventArgs e)
    {
        if (_engine is null || e.Tool != GeoKernelViewerTool.Route || (e.ButtonOrButtons & 1) == 0) return;
        var source = ShortestRoutingEngine.ToWgs84(new RoutePoint(e.WorldPoint.X, e.WorldPoint.Y)); var snapped = _engine.NearestNode(source, 2000);
        if (snapped is null) { MessageBox.Show(this, "No road node was found near the selected point.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        var world = ShortestRoutingEngine.ToWebMercator(snapped.Position);
        if (!_startPoint.HasValue || _finishPoint.HasValue)
        { _startPoint = world; _finishPoint = null; _startNode = snapped.Id; _route = null; _directions.Items.Clear(); _summary.Text = "Select the finish point."; _overlay.SetState(_startPoint, null, null); _status.Text = "Start selected. Click the map to choose the finish point."; return; }
        _finishPoint = world; _route = _engine.FindRoute(_startNode, snapped.Id);
        if (_route is null) { _overlay.SetState(_startPoint, _finishPoint, null); MessageBox.Show(this, "No connected route was found.", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning); _status.Text = "No connected route found. Click once to choose a new start."; return; }
        _summary.Text = $"{_route.Distance / 1000:0.00} km  •  {_route.Time / 60:0.0} min"; _directions.Items.Clear(); var steps = _engine.RoadSteps(_route);
        for (var i = 0; i < steps.Count; i++) _directions.Items.Add($"{i + 1}. {steps[i].Name}\r\n    {(steps[i].Distance >= 1000 ? $"{steps[i].Distance / 1000:0.0} km" : $"{steps[i].Distance:0} m")}");
        _overlay.SetState(_startPoint, _finishPoint, _route); _status.Text = $"Route: {_route.Distance / 1000:0.00} km, {_route.Time / 60:0.0} min";
    }

    private sealed class RouteOverlayWindow : Form
    {
        private readonly GeoKernelViewerControl _viewer; private RoutePoint? _start, _finish; private ShortestRouteResult? _route;
        public RouteOverlayWindow(GeoKernelViewerControl viewer) { _viewer = viewer; FormBorderStyle = FormBorderStyle.None; ShowInTaskbar = false; BackColor = Color.Fuchsia; TransparencyKey = Color.Fuchsia; StartPosition = FormStartPosition.Manual; }
        protected override bool ShowWithoutActivation => true;
        protected override CreateParams CreateParams { get { var value = base.CreateParams; value.ExStyle |= 0x08000020 | 0x80; return value; } }
        public void SyncBounds() { if (!Visible || !_viewer.IsHandleCreated) return; Bounds = new Rectangle(_viewer.PointToScreen(Point.Empty), _viewer.ClientSize); Invalidate(); }
        public void SetState(RoutePoint? start, RoutePoint? finish, ShortestRouteResult? route) { _start = start; _finish = finish; _route = route; SyncBounds(); Invalidate(); }
        protected override void OnShown(EventArgs e) { base.OnShown(e); SyncBounds(); }
        protected override void OnPaint(PaintEventArgs e) { e.Graphics.SmoothingMode = SmoothingMode.AntiAlias; if (_route is not null && _route.WorldGeometry.Count > 1) { using var pen = new Pen(Color.FromArgb(239, 68, 68), 4) { StartCap = LineCap.Round, EndCap = LineCap.Round, LineJoin = LineJoin.Round }; e.Graphics.DrawLines(pen, _route.WorldGeometry.Select(Screen).ToArray()); } DrawMarker(e.Graphics, _start, Color.FromArgb(34, 197, 94), Color.FromArgb(20, 83, 45)); DrawMarker(e.Graphics, _finish, Color.FromArgb(239, 68, 68), Color.FromArgb(127, 29, 29)); }
        private void DrawMarker(Graphics graphics, RoutePoint? point, Color fill, Color outline) { if (!point.HasValue) return; var p = Screen(point.Value); using var brush = new SolidBrush(fill); using var pen = new Pen(outline, 2); graphics.FillEllipse(brush, p.X - 8, p.Y - 8, 16, 16); graphics.DrawEllipse(pen, p.X - 8, p.Y - 8, 16, 16); }
        private PointF Screen(RoutePoint point) { var p = _viewer.WorldToScreen(point.X, point.Y); return new PointF((float)p.X, (float)p.Y); }
    }
}
