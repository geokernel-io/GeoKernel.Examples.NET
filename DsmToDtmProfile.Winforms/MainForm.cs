using System.Drawing.Drawing2D;
using GeoKernel.Examples.DsmToDtm;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.DsmToDtmProfile.Winforms;

/// <summary>
/// Interactive DSM / DTM elevation profile: draw an A → B transect on two linked native 2D
/// maps and compare the original DSM, classical DTM, AI DTM and provider reference heights.
/// Profiles sample the numeric rasters; the maps show matching display reliefs. No inference.
/// </summary>
internal sealed class MainForm : Form
{
    static readonly Color Page = ColorTranslator.FromHtml("#f2f6f8"), Ink = ColorTranslator.FromHtml("#233e50");
    static readonly string[] Colors = DsmText.Colors;

    readonly DsmOptions options;
    readonly SampleAssets assets = SampleAssets.ZurichV2();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-profile-").FullName;
    readonly GeoKernelViewerControl[] maps = [new(), new()];
    readonly Label[] headings = new Label[2];
    readonly ComboBox surface = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 260 };
    readonly ComboBox mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Top };
    readonly NumericUpDown spacing = new() { Minimum = .5m, Maximum = 10, Increment = .5m, DecimalPlaces = 2, Value = .5m, Width = 80 };
    readonly Label instructions = MakeLabel("Preparing sample data...", true), stats = MakeLabel("Select a transect to compare errors.", true),
        cursor = MakeLabel(ProfileText.NoCursor, true), status = new() { AutoSize = true },
        legend = new() { Text = ProfileText.Legends[0], AutoSize = true, Dock = DockStyle.Bottom, Padding = new Padding(0, 4, 0, 0) };
    readonly Button export = new() { Text = "Export profile CSV", Width = 260, Height = 32 }, reload = new() { Text = "Retry / reload sample", Width = 260, Height = 32 },
        cancel = new() { Text = "Cancel loading", Width = 260, Height = 32 };
    readonly ToolStripButton draw = new("Draw transect · A → B") { CheckOnClick = true };
    readonly List<ToolStripItem> navigation = [];
    readonly ProfileChart chart;
    readonly ProgressBar progress = new() { Width = 200, Height = 16, Visible = false };
    readonly (double X, double Y)?[] presses = new (double X, double Y)?[2];
    readonly GeoKernelExtent?[] applied = new GeoKernelExtent?[2];
    CancellationTokenSource cts = new();
    Grids? grids;
    (double X, double Y)? draft;
    bool busy, closeQueued, closed, syncing;
    int generation;

    static Label MakeLabel(string text, bool card = false)
    {
        var label = new Label { Text = text, AutoSize = true, MaximumSize = new Size(260, 0), Margin = new Padding(0, 0, 0, 10) };
        if (card) { label.BackColor = Color.White; label.Padding = new Padding(10); label.BorderStyle = BorderStyle.FixedSingle; label.MinimumSize = new Size(260, 0); }
        return label;
    }

    public MainForm(DsmOptions options)
    {
        this.options = options;
        chart = new ProfileChart(Hover) { Dock = DockStyle.Fill };
        Text = ProfileText.Title;
        ClientSize = new Size(1460, 940);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Page;
        ForeColor = Ink;
        Font = new Font("Segoe UI", 9);
        ExampleIcon.Apply(this);
        BuildLayout();
        SetBusy(false);
        Shown += (_, _) => _ = StartAsync();
    }

    void BuildLayout()
    {
        var sidebar = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Dock = DockStyle.Left, Width = 295, Padding = new Padding(12) };
        surface.Items.AddRange(["Classical DTM", "AI DTM · pix_grad", "Reference DTM · evaluation only"]);
        surface.SelectedIndex = 0;
        var spacingRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 0, 0, 10) };
        spacingRow.Controls.Add(new Label { Text = "Sample spacing (m)", AutoSize = true, Margin = new Padding(0, 6, 8, 0) });
        spacingRow.Controls.Add(spacing);
        var title = MakeLabel("Compare along\nyour own transect");
        title.Font = new Font("Segoe UI", 15, FontStyle.Bold);
        title.ForeColor = ColorTranslator.FromHtml("#145e72");
        sidebar.Controls.AddRange([
            title, MakeLabel("Zürich · 250 × 250 m\n0.5 m height pixels · EPSG:2056"), instructions,
            MakeLabel("Right-hand map"), surface, spacingRow, stats, cursor, MakeLabel(ProfileText.Numeric),
            export, reload, cancel,
            MakeLabel("Source: Federal Office of Topography swisstopo\nAI: David Perez · experimental pix_grad"),
        ]);

        var views = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        views.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        views.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        views.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        views.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (int i = 0; i < 2; i++)
        {
            int side = i;
            headings[i] = new Label
            {
                Text = i == 0 ? "ORIGINAL DSM · roofs + trees + ground" : ProfileText.Headings[0],
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0),
                BackColor = ColorTranslator.FromHtml("#e4eef2"), ForeColor = ColorTranslator.FromHtml("#125a70"), Font = new Font(Font, FontStyle.Bold),
            };
            maps[i].Dock = DockStyle.Fill;
            maps[i].MinimumSize = new Size(300, 280);
            // Native events are already posted to the UI thread after the viewer's callback returns.
            maps[i].VisibleExtentChanged += (_, _) => ExtentChanged(side);
            maps[i].MapMouseDown += (_, e) => { if (e.ButtonOrButtons == 1) presses[side] = (e.ScreenPoint.X, e.ScreenPoint.Y); };
            maps[i].MapMouseUp += (_, e) => MapClick(side, e);
            views.Controls.Add(headings[i], i, 0);
            views.Controls.Add(maps[i], i, 1);
        }

        mode.Items.AddRange(["Compare elevations", "Compare errors against reference"]);
        mode.SelectedIndex = 0;
        var profilePane = new Panel { Dock = DockStyle.Fill };
        profilePane.Controls.Add(chart);
        profilePane.Controls.Add(mode);
        profilePane.Controls.Add(legend);
        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Horizontal, SplitterWidth = 6, BackColor = Page };
        split.Panel1.Controls.Add(views);
        split.Panel2.Controls.Add(profilePane);
        var workspace = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(0, 12, 12, 6) };
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.Controls.Add(split, 0, 0);
        workspace.Controls.Add(new Label { Text = ProfileText.Footnote, AutoSize = true, Margin = new Padding(0, 6, 0, 0) }, 0, 1);

        var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, BackColor = Color.White, Padding = new Padding(6), ImageScalingSize = new Size(20, 20) };
        ToolStripButton Tool(string text, string? image, Action action)
        {
            var button = new ToolStripButton(text, image == null ? null : LoadImage(image)) { DisplayStyle = image == null ? ToolStripItemDisplayStyle.Text : ToolStripItemDisplayStyle.ImageAndText };
            button.Click += (_, _) => action();
            toolbar.Items.Add(button);
            navigation.Add(button);
            return button;
        }
        toolbar.Items.Add(draw);
        Tool("Example transect", null, ExampleProfile);
        Tool("Clear transect", null, ClearProfile);
        toolbar.Items.Add(new ToolStripSeparator());
        Tool("Pan", null, () => UseTool(GeoKernelViewerTool.Pan));
        Tool("Zoom box", null, () => UseTool(GeoKernelViewerTool.ZoomBox));
        Tool("Zoom in", "ZoomIn.png", () => maps[0].ZoomIn(1.5));
        Tool("Zoom out", "ZoomOut.png", () => maps[0].ZoomOut(1.5));
        Tool("Full extent", "FullExtent.png", FullExtent);

        var statusStrip = new StatusStrip { SizingGrip = false };
        statusStrip.Items.Add(new ToolStripControlHost(status));
        statusStrip.Items.Add(new ToolStripStatusLabel { Spring = true });
        statusStrip.Items.Add(new ToolStripControlHost(progress));

        Controls.Add(workspace);
        Controls.Add(sidebar);
        Controls.Add(toolbar);
        Controls.Add(statusStrip);
        Load += (_, _) => split.SplitterDistance = Math.Max(300, (int)(split.Height * .66));

        draw.CheckedChanged += (_, _) => ToggleDraw(draw.Checked);
        surface.SelectedIndexChanged += (_, _) => SelectSurface();
        spacing.ValueChanged += (_, _) => Resample();
        mode.SelectedIndexChanged += (_, _) => { chart.Errors = mode.SelectedIndex == 1; legend.Text = ProfileText.Legends[mode.SelectedIndex]; chart.Invalidate(); };
        export.Click += (_, _) => ExportCsv();
        reload.Click += (_, _) => _ = StartAsync();
        cancel.Click += (_, _) => RequestCancel();
    }

    static Image LoadImage(string name)
    {
        using var stream = typeof(MainForm).Assembly.GetManifestResourceStream("GeoKernel.Examples." + name)
            ?? throw new InvalidOperationException($"Embedded image {name} is missing.");
        return Image.FromStream(stream);
    }

    // ----- loading -------------------------------------------------------------------------
    void SetBusy(bool value)
    {
        busy = value;
        bool ready = !value && grids != null;
        reload.Enabled = !value;
        cancel.Enabled = value;
        progress.Visible = value;
        draw.Enabled = surface.Enabled = spacing.Enabled = ready;
        foreach (var item in navigation) item.Enabled = ready;
        export.Enabled = !value && chart.Profile != null;
    }

    void Report(int percent, string message)
    {
        progress.Value = Math.Clamp(percent, 0, 100);
        status.Text = message;
    }

    void RequestCancel()
    {
        cts.Cancel();
        cancel.Enabled = false;
        status.Text = "Cancelling loading...";
    }

    async Task StartAsync()
    {
        if (busy) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        SetBusy(true);
        var report = new Progress<(int Percent, string Message)>(p => Report(p.Percent, p.Message));
        string folder = Path.Combine(temporary, (++generation).ToString());
        try
        {
            string sample = await SampleCache.PrepareAsync(assets, options, report, token);
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(sample);
            Directory.CreateDirectory(folder);
            var (loaded, reliefs) = await Task.Run(() =>
            {
                const string message = "Reading numeric heights and preparing matching relief maps...";
                ((IProgress<(int, string)>)report).Report((0, message));
                var g = Grids.Read(sample, token);
                var paths = new string[4];
                for (int band = 0; band < 4; band++)
                {
                    paths[band] = Gdal.WriteRgb(Path.Combine(folder, $"relief-{band}.tif"), g.Width, g.Height, g.Transform, g.Projection, Profiles.Relief(g, band, token));
                    ((IProgress<(int, string)>)report).Report(((band + 1) * 25, message));
                }
                return (g, paths);
            }, token);
            token.ThrowIfCancellationRequested();
            Publish(loaded, reliefs);
            SetBusy(false);
            ExampleProfile();
            status.Text = ProfileText.Ready;
            if (closeQueued) BeginInvoke(Close);
        }
        catch (Exception e)
        {
            if (closed) return;
            SetBusy(false);
            bool cancelled = e is OperationCanceledException || cts.IsCancellationRequested;
            status.Text = cancelled ? "Loading cancelled." : e.Message;
            if (closeQueued) BeginInvoke(Close);
            else if (!cancelled) MessageBox.Show(this, e.Message, "Unable to load sample", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    /// <summary>DSM relief left; all three estimate reliefs right with one visible.</summary>
    void Publish(Grids loaded, string[] reliefs)
    {
        grids = loaded;
        for (int pane = 0; pane < 2; pane++)
        {
            var map = maps[pane];
            map.ClearShapes();
            map.ClearLayers();
            // The empty map adopts the first layer's CRS (EPSG:2056); new layers go to index 0.
            foreach (int band in pane == 0 ? new[] { 0 } : [1, 2, 3])
            {
                if (!map.AddLayerFile(reliefs[band])) throw new InvalidOperationException($"The relief layer could not be added: {reliefs[band]}");
                map.SetLayerName(0, Profiles.SurfaceNames[band]);
            }
            map.SetTool(GeoKernelViewerTool.Pan);
        }
        applied[0] = applied[1] = null;
        FullExtent();
        SelectSurface();
    }

    // ----- maps ----------------------------------------------------------------------------
    static int RightIndex(int band) => 3 - band; // bands 1, 2, 3 were prepended in that order

    void FullExtent()
    {
        var e = ProfileText.FullExtent;
        maps[0].ViewExtent = new GeoKernelExtent(e.XMin, e.YMin, e.XMax, e.YMax);
        SyncFrom(0);
    }

    void SyncFrom(int side)
    {
        int other = 1 - side;
        syncing = true;
        try
        {
            applied[side] = maps[side].ViewExtent;
            maps[other].ViewExtent = applied[side]!.Value;
            applied[other] = maps[other].ViewExtent;
        }
        finally { syncing = false; }
    }

    static bool Same(GeoKernelExtent? a, GeoKernelExtent b) =>
        a is GeoKernelExtent x && Math.Abs(x.XMin - b.XMin) < 1e-9 && Math.Abs(x.YMin - b.YMin) < 1e-9 && Math.Abs(x.XMax - b.XMax) < 1e-9 && Math.Abs(x.YMax - b.YMax) < 1e-9;

    void ExtentChanged(int side)
    {
        // The event can arrive synchronously inside SetViewExtent or while the map renders:
        // ignore our own updates and read the new extent after the native call returns.
        if (syncing || closed) return;
        BeginInvoke(() =>
        {
            if (!syncing && !closed && grids != null && !Same(applied[side], maps[side].ViewExtent)) SyncFrom(side);
        });
    }

    void MapClick(int side, GeoKernelMapMouseEventArgs e)
    {
        if (e.ButtonOrButtons != 1) return;
        var press = presses[side];
        presses[side] = null;
        // A click (not a pan drag) picks a point; dragging still pans.
        if (press is { } p && Math.Abs(p.X - e.ScreenPoint.X) + Math.Abs(p.Y - e.ScreenPoint.Y) <= 3)
            Pick(e.WorldPoint.X, e.WorldPoint.Y);
    }

    void UseTool(GeoKernelViewerTool tool)
    {
        draw.Checked = false;
        foreach (var map in maps) map.SetTool(tool);
    }

    void SelectSurface()
    {
        if (grids == null) return;
        int choice = surface.SelectedIndex + 1;
        for (int band = 1; band <= 3; band++) maps[1].SetLayerVisible(RightIndex(band), band == choice);
        maps[1].RefreshLayers();
        headings[1].Text = ProfileText.Headings[choice - 1];
    }

    static GeoKernelLayerStyle Marker(string color, double size) => new() { PointColor = color, PointSize = size, LineColor = "#ffffff", LineWidth = 2 };

    /// <summary>Owned shapes on both maps: white halo + teal line, A teal, B orange and the hovered sample.
    /// GeoKernel.NET 1.5.40 wraps no overlay point, so hovering redraws these shapes.</summary>
    void DrawOverlays()
    {
        var profile = chart.Profile;
        foreach (var map in maps)
        {
            map.ClearShapes();
            if (draft is { } a) map.AddPointShape(new GeoKernelPoint(a.X, a.Y), Marker(Colors[1], 16));
            else if (profile != null)
            {
                GeoKernelPoint[] line = [new(profile.Start.X, profile.Start.Y), new(profile.End.X, profile.End.Y)];
                map.AddPolylineShape(line, new GeoKernelLayerStyle { LineColor = "#ffffff", LineWidth = 5 });
                map.AddPolylineShape(line, new GeoKernelLayerStyle { LineColor = Colors[1], LineWidth = 2.5 });
                map.AddPointShape(line[0], Marker(Colors[1], 16));
                map.AddPointShape(line[1], Marker(Colors[3], 16));
                if (chart.Hovered >= 0 && chart.Hovered < profile.Samples.Count)
                {
                    var s = profile.Samples[chart.Hovered];
                    map.AddPointShape(new GeoKernelPoint(s.X, s.Y), Marker("#dd3470", 11));
                }
            }
            map.RefreshLayers(); // owned shapes are drawn on the next full map render
        }
    }

    // ----- transect ------------------------------------------------------------------------
    void ToggleDraw(bool on)
    {
        draft = null;
        if (on)
        {
            foreach (var map in maps) map.SetTool(GeoKernelViewerTool.Pan);
            instructions.Text = "Click A on either map. Dragging still pans; wheel zooms.";
        }
        else instructions.Text = "Draw a new A → B transect, or inspect the current profile.";
        DrawOverlays();
    }

    void Pick(double x, double y)
    {
        if (grids == null || busy || !draw.Checked) return;
        if (Profiles.Height(grids, 0, x, y) == null) { status.Text = "Click inside the valid raster pixel centres."; return; }
        if (draft is not { } a)
        {
            draft = (x, y);
            instructions.Text = "A selected. Click B on either map. Toggle Draw off to cancel.";
            DrawOverlays();
            return;
        }
        try { CommitProfile(a, (x, y)); }
        catch (ArgumentException e) { status.Text = e.Message; return; }
        draft = null;
        draw.Checked = false;
        instructions.Text = "Your transect is ready. Hover over the chart to locate matching heights on both maps.";
    }

    void ExampleProfile()
    {
        if (grids == null) return;
        draw.Checked = false;
        draft = null;
        var (a, b) = Profiles.Example(grids);
        CommitProfile(a, b);
        instructions.Text = "Example transect loaded. Select Draw transect, then click A and B to compare your own line.";
    }

    void ClearProfile()
    {
        draw.Checked = false;
        draft = null;
        chart.Profile = null;
        chart.Hovered = -1;
        chart.Invalidate();
        RefreshStats();
        Hover(-1);
    }

    void Resample()
    {
        if (chart.Profile is not { } p) return;
        try { CommitProfile(p.Start, p.End); }
        catch (ArgumentException e) { status.Text = e.Message; }
    }

    void CommitProfile((double X, double Y) a, (double X, double Y) b)
    {
        chart.Profile = Profiles.Make(grids!, a, b, (double)spacing.Value);
        chart.Hovered = -1;
        chart.Invalidate();
        RefreshStats();
        Hover(-1);
    }

    void RefreshStats()
    {
        export.Enabled = !busy && chart.Profile != null;
        stats.Text = ProfileText.Stats(chart.Profile);
    }

    void Hover(int index)
    {
        var p = chart.Profile;
        cursor.Text = p != null && index >= 0 && index < p.Samples.Count ? ProfileText.Cursor(p.Samples[index]) : ProfileText.NoCursor;
        DrawOverlays();
    }

    void ExportCsv()
    {
        if (chart.Profile is not { } profile || grids == null) return;
        using var dialog = new SaveFileDialog { Title = "Export numeric profile", FileName = "profile.csv", Filter = "CSV (*.csv)|*.csv" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { Profiles.Export(dialog.FileName, profile, grids, assets.Sha256); }
        catch (Exception e) { MessageBox.Show(this, e.Message, "Export profile", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        status.Text = "Saved numeric profile CSV and metadata sidecar.";
    }

    // ----- shutdown ------------------------------------------------------------------------
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (busy) { closeQueued = true; RequestCancel(); e.Cancel = true; return; }
        closed = true;
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        foreach (var map in maps) map.Dispose();
        try { Directory.Delete(temporary, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    sealed class ProfileChart(Action<int> hoverChanged) : Control
    {
        public TransectProfile? Profile;
        public bool Errors;
        public int Hovered = -1;

        protected override void OnCreateControl()
        {
            base.OnCreateControl();
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.White;
            MinimumSize = new Size(0, 200);
        }

        RectangleF Plot => new(65, 30, Width - 90, Height - 72);

        void SetHovered(int index)
        {
            if (index == Hovered) return;
            Hovered = index;
            hoverChanged(index);
            Invalidate();
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            var plot = Plot;
            int count = Profile?.Samples.Count ?? 0;
            SetHovered(count == 0 || !plot.Contains(e.Location) ? -1
                : Math.Clamp((int)Math.Round((e.X - plot.Left) / plot.Width * (count - 1)), 0, count - 1));
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            SetHovered(-1);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            var ink = ColorTranslator.FromHtml("#476174");
            using var text = new SolidBrush(ink);
            var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            if (Profile is not { } profile)
            {
                g.DrawString("Draw a transect: choose A, then B on either map.", Font, text, ClientRectangle, center);
                return;
            }
            var model = ChartModel.From(profile, Errors);
            double low = model.Low, high = model.High;
            var plot = Plot;
            float Y(double v) => (float)(plot.Bottom - (v - low) / (high - low) * plot.Height);
            g.DrawString(Errors ? "Estimate minus reference (m) · zero is closest" : "Source elevation (LN02 metres) · all four rasters on one scale", Font, text, 8, 6);
            using var grid = new Pen(ColorTranslator.FromHtml("#e2e9ee"));
            var right = new StringFormat { Alignment = StringAlignment.Far };
            var middle = new StringFormat { Alignment = StringAlignment.Center };
            for (int i = 0; i <= 4; i++)
            {
                double t = i / 4.0;
                float y = (float)(plot.Bottom - t * plot.Height);
                g.DrawLine(grid, plot.Left, y, plot.Right, y);
                g.DrawString($"{low + t * (high - low):F1}", Font, text, new RectangleF(0, y - 8, 58, 18), right);
                g.DrawString($"{t * profile.Length:F1}", Font, text, new RectangleF(plot.Left + (float)t * plot.Width - 28, plot.Bottom + 5, 56, 18), middle);
            }
            g.DrawString("Distance along A → B (projected metres)", Font, text, new RectangleF(0, Height - 22, Width, 20), middle);
            if (Errors)
            {
                using var zero = new Pen(ColorTranslator.FromHtml("#b4bfca")) { DashStyle = DashStyle.Dash };
                g.DrawLine(zero, plot.Left, Y(0), plot.Right, Y(0));
            }
            foreach (int band in new[] { 0, 3, 1, 2 })
            {
                if (!model.Bands.Contains(band)) continue;
                var points = profile.Samples.Select(s => new PointF(plot.Left + (float)(s.Distance / profile.Length) * plot.Width, Y(ChartModel.Value(s, band, Errors)))).ToArray();
                using var pen = new Pen(ColorTranslator.FromHtml(Colors[band]), band == 0 ? 1.5f : 2f);
                if (band == 3) pen.DashStyle = DashStyle.Dash;
                g.DrawLines(pen, points);
            }
            if (Hovered >= 0 && Hovered < profile.Samples.Count)
            {
                float x = plot.Left + (float)(profile.Samples[Hovered].Distance / profile.Length) * plot.Width;
                using var marker = new Pen(ColorTranslator.FromHtml("#dd3470")) { DashStyle = DashStyle.Dash };
                g.DrawLine(marker, x, plot.Top, x, plot.Bottom);
            }
        }
    }
}
