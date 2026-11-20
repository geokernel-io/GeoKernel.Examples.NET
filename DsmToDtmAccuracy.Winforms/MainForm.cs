using System.Drawing.Drawing2D;
using GeoKernel.Examples.DsmToDtm;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.DsmToDtmAccuracy.Winforms;

/// <summary>
/// Reference accuracy by area: three linked native 2D maps keep the original DSM beside
/// classical and AI error maps on one shared colour scale. Select an area to compare numeric
/// errors, their distribution and individual source pixels. No inference runs here.
/// </summary>
internal sealed class MainForm : Form
{
    static readonly Color Page = ColorTranslator.FromHtml("#f2f6f8"), Ink = ColorTranslator.FromHtml("#233e50");
    static readonly (double XMin, double YMin, double XMax, double YMax) FullView = (2677090, 1251340, 2677360, 1251610);

    readonly DsmOptions options;
    readonly SampleAssets assets = SampleAssets.ZurichV2();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-accuracy-").FullName;
    readonly GeoKernelViewerControl[] maps = [new(), new(), new()];
    readonly Label[] headings = new Label[3];
    readonly ComboBox mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 255 };
    readonly ComboBox range = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 90 };
    readonly ComboBox presets = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 255 };
    readonly NumericUpDown threshold = new() { Minimum = .1m, Maximum = 20, Increment = .5m, DecimalPlaces = 1, Value = 2, Width = 90 };
    readonly Label instructions = MakeLabel("Preparing sample data...", true), winners = MakeLabel("Compare lower errors for each metric.", true),
        probe = MakeLabel("Click a map pixel to inspect its heights and signed errors.", true), status = new() { AutoSize = true },
        scope = new() { Text = "No evaluation area loaded.", AutoSize = true, Margin = new Padding(0, 4, 0, 4) };
    readonly Button export = new() { Text = "Export area report", Width = 255, Height = 32 }, reload = new() { Text = "Retry / reload sample", Width = 255, Height = 32 },
        cancel = new() { Text = "Cancel loading", Width = 255, Height = 32 };
    readonly ToolStripButton areaButton = new("Select evaluation area") { CheckOnClick = true };
    readonly List<ToolStripItem> navigation = [];
    readonly DataGridView table = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToResizeRows = false, RowHeadersVisible = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.None,
        SelectionMode = DataGridViewSelectionMode.CellSelect, Enabled = false, ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
    };
    readonly Legend legend = new() { Dock = DockStyle.Fill };
    readonly Histogram histogram = new() { Dock = DockStyle.Fill };
    readonly ProgressBar progress = new() { Width = 210, Height = 16, Visible = false };
    readonly (double X, double Y)?[] presses = new (double X, double Y)?[3];
    readonly GeoKernelExtent?[] applied = new GeoKernelExtent?[3];
    CancellationTokenSource cts = new();
    Grids? grids;
    PixelWindow? area;
    string areaName = "";
    AccuracyMetric[]? metrics;
    (double X, double Y)? probePoint;
    bool busy, closeQueued, closed, syncing;
    int generation;

    static Label MakeLabel(string text, bool card = false)
    {
        var label = new Label { Text = text, AutoSize = true, MaximumSize = new Size(255, 0), Margin = new Padding(0, 0, 0, 10) };
        if (card) { label.BackColor = Color.White; label.Padding = new Padding(10); label.BorderStyle = BorderStyle.FixedSingle; label.MinimumSize = new Size(255, 0); }
        return label;
    }

    public MainForm(DsmOptions options)
    {
        this.options = options;
        Text = AccuracyText.Title;
        ClientSize = new Size(1720, 970);
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
        var sidebar = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Dock = DockStyle.Left, Width = 290, Padding = new Padding(12) };
        mode.Items.AddRange(["Signed reference errors", "Absolute reference errors", "Terrain estimates"]);
        mode.SelectedIndex = 0;
        range.Items.AddRange(["1 m", "2 m", "5 m", "10 m"]);
        range.SelectedIndex = 1;
        presets.Items.AddRange(Accuracy.Presets);
        presets.SelectedIndex = 0;
        FlowLayoutPanel Row(string text, Control control)
        {
            var row = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 0, 0, 8) };
            row.Controls.Add(new Label { Text = text, AutoSize = true, Width = 120, Margin = new Padding(0, 6, 8, 0) });
            row.Controls.Add(control);
            return row;
        }
        var title = MakeLabel("Where does each\nmethod work better?");
        title.Font = new Font("Segoe UI", 15, FontStyle.Bold);
        title.ForeColor = ColorTranslator.FromHtml("#145e72");
        sidebar.Controls.AddRange([
            title, MakeLabel("Zürich · same DSM and reference\n250 × 250 m · 0.5 m source cells"), instructions,
            MakeLabel("Map comparison"), mode, Row("Display limit", range), Row("Large error > (m)", threshold),
            MakeLabel("Study-area preset"), presets, winners, probe, MakeLabel(AccuracyText.Note), export, reload, cancel,
            MakeLabel("Source: Federal Office of Topography swisstopo\nAI: David Perez · experimental pix_grad"),
        ]);

        var views = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2 };
        for (int i = 0; i < 3; i++) views.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        views.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        views.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (int i = 0; i < 3; i++)
        {
            int side = i;
            headings[i] = new Label
            {
                Text = AccuracyText.Titles[i], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0),
                BackColor = ColorTranslator.FromHtml("#e4eef2"), ForeColor = ColorTranslator.FromHtml("#125a70"), Font = new Font(Font, FontStyle.Bold),
            };
            maps[i].Dock = DockStyle.Fill;
            maps[i].MinimumSize = new Size(270, 290);
            maps[i].VisibleExtentChanged += (_, _) => ExtentChanged(side);
            maps[i].MapSelectionBoxFinished += (_, e) => SelectionFinished(e.WorldExtent);
            maps[i].MapMouseDown += (_, e) => { if (e.ButtonOrButtons == 1) presses[side] = (e.ScreenPoint.X, e.ScreenPoint.Y); };
            maps[i].MapMouseUp += (_, e) => MapClick(side, e);
            views.Controls.Add(headings[i], i, 0);
            views.Controls.Add(maps[i], i, 1);
        }
        foreach (var column in new[] { "Method", "RMSE (m)", "MAE (m)", "Bias (m)", "P95 |error| (m)", "|error| > 2 m (%)" })
            table.Columns.Add(column, column);
        table.Rows.Add(2);
        table.DefaultCellStyle.SelectionBackColor = Color.White;
        table.DefaultCellStyle.SelectionForeColor = Ink;

        var workspace = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(0, 12, 12, 6) };
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 64));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 160));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.Controls.Add(views, 0, 0);
        workspace.Controls.Add(legend, 0, 1);
        workspace.Controls.Add(scope, 0, 2);
        workspace.Controls.Add(table, 0, 3);
        workspace.Controls.Add(histogram, 0, 4);
        workspace.Controls.Add(new Label { Text = AccuracyText.Footnote, AutoSize = true, Margin = new Padding(0, 6, 0, 0) }, 0, 5);

        var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, BackColor = Color.White, Padding = new Padding(6), ImageScalingSize = new Size(20, 20) };
        ToolStripButton Tool(string text, string? image, Action action)
        {
            var button = new ToolStripButton(text, image == null ? null : LoadImage(image)) { DisplayStyle = image == null ? ToolStripItemDisplayStyle.Text : ToolStripItemDisplayStyle.ImageAndText };
            button.Click += (_, _) => action();
            toolbar.Items.Add(button);
            navigation.Add(button);
            return button;
        }
        toolbar.Items.Add(areaButton);
        Tool("Evaluate current view", null, () => { var e = maps[0].ViewExtent; ChooseArea((e.XMin, e.YMin, e.XMax, e.YMax), "Current map view"); });
        Tool("Full sample", null, () => ApplyPreset(0));
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

        areaButton.CheckedChanged += (_, _) => ToggleArea(areaButton.Checked);
        mode.SelectedIndexChanged += (_, _) => SelectDisplay();
        range.SelectedIndexChanged += (_, _) => SelectDisplay();
        threshold.ValueChanged += (_, _) => { if (grids != null) UpdateMetrics(); };
        presets.SelectionChangeCommitted += (_, _) => ApplyPreset(presets.SelectedIndex);
        export.Click += (_, _) => ExportReport();
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
        mode.Enabled = threshold.Enabled = presets.Enabled = export.Enabled = areaButton.Enabled = ready;
        range.Enabled = ready && mode.SelectedIndex != 2;
        foreach (var item in navigation) item.Enabled = ready;
    }

    void RequestCancel()
    {
        cts.Cancel();
        cancel.Enabled = false;
        status.Text = "Cancelling preparation...";
    }

    async Task StartAsync()
    {
        if (busy) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        areaButton.Checked = false;
        SetBusy(true);
        var report = new Progress<(int Percent, string Message)>(p => { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; });
        string folder = Path.Combine(temporary, (++generation).ToString());
        try
        {
            string sample = await SampleCache.PrepareAsync(assets, options, report, token);
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(sample);
            Directory.CreateDirectory(folder);
            const string message = "Reading heights and preparing shared-scale Byte RGB maps...";
            var percent = new Progress<int>(p => { progress.Value = Math.Clamp(p, 0, 100); status.Text = message; });
            status.Text = message;
            var (loaded, files) = await Task.Run(() =>
            {
                var g = Grids.Read(sample, token);
                return (g, Accuracy.PrepareDisplays(g, folder, percent, token));
            }, token);
            token.ThrowIfCancellationRequested();
            Publish(loaded, files.Original, files.Methods);
            SetBusy(false);
            if (area == null) ApplyPreset(0);
            else { UpdateMetrics(); DrawOverlays(); }
            instructions.Text = AccuracyText.Idle;
            status.Text = AccuracyText.Ready;
            if (closeQueued) BeginInvoke(Close);
        }
        catch (Exception e)
        {
            if (closed) return;
            SetBusy(false);
            bool cancelled = e is OperationCanceledException || cts.IsCancellationRequested;
            status.Text = cancelled ? "Preparation cancelled; completed comparison retained." : e.Message;
            if (closeQueued) BeginInvoke(Close);
            else if (!cancelled) MessageBox.Show(this, e.Message, "Unable to load comparison", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    void Publish(Grids loaded, string original, string[][] methods)
    {
        grids = loaded;
        for (int pane = 0; pane < 3; pane++)
        {
            var map = maps[pane];
            map.ClearShapes();
            map.ClearLayers();
            // The empty map adopts the first layer's CRS (EPSG:2056); new layers go to index 0.
            string[] files = pane == 0 ? [original] : methods[pane - 1];
            for (int k = 0; k < files.Length; k++)
            {
                if (!map.AddLayerFile(files[k])) throw new InvalidOperationException($"The display layer could not be added: {files[k]}");
                map.SetLayerName(0, pane == 0 ? "Original DSM" : $"{(pane == 1 ? "Classical" : "AI")} display {k}");
            }
            map.SetTool(GeoKernelViewerTool.Pan);
        }
        Array.Clear(applied);
        FullExtent();
        SelectDisplay();
    }

    // ----- maps ----------------------------------------------------------------------------
    void FullExtent()
    {
        maps[0].ViewExtent = new GeoKernelExtent(FullView.XMin, FullView.YMin, FullView.XMax, FullView.YMax);
        SyncFrom(0);
    }

    void SyncFrom(int side)
    {
        syncing = true;
        try
        {
            applied[side] = maps[side].ViewExtent;
            for (int other = 0; other < 3; other++)
                if (other != side) { maps[other].ViewExtent = applied[side]!.Value; applied[other] = maps[other].ViewExtent; }
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
        BeginInvoke(() => { if (!syncing && !closed && grids != null && !Same(applied[side], maps[side].ViewExtent)) SyncFrom(side); });
    }

    void SelectionFinished(GeoKernelExtent e)
    {
        if (!areaButton.Checked) return;
        BeginInvoke(() =>
        {
            ChooseArea((e.XMin, e.YMin, e.XMax, e.YMax), "User-selected area");
            areaButton.Checked = false;
        });
    }

    void MapClick(int side, GeoKernelMapMouseEventArgs e)
    {
        if (e.ButtonOrButtons != 1) return;
        var press = presses[side];
        presses[side] = null;
        if (press is { } p && !areaButton.Checked && Math.Abs(p.X - e.ScreenPoint.X) + Math.Abs(p.Y - e.ScreenPoint.Y) <= 3)
            Inspect(e.WorldPoint.X, e.WorldPoint.Y);
    }

    void UseTool(GeoKernelViewerTool tool)
    {
        areaButton.Checked = false;
        foreach (var map in maps) map.SetTool(tool);
    }

    void ToggleArea(bool active)
    {
        foreach (var map in maps) map.SetTool(active ? GeoKernelViewerTool.Select : GeoKernelViewerTool.Pan);
        instructions.Text = active ? AccuracyText.Selecting : AccuracyText.Idle;
    }

    void SelectDisplay()
    {
        legend.Mode = mode.SelectedIndex;
        legend.Limit = Accuracy.DisplayLimits[range.SelectedIndex];
        legend.Invalidate();
        range.Enabled = !busy && grids != null && mode.SelectedIndex != 2;
        if (grids == null) return;
        int selected = mode.SelectedIndex == 2 ? 8 : (mode.SelectedIndex == 1 ? 4 : 0) + range.SelectedIndex;
        for (int m = 1; m <= 2; m++)
        {
            for (int k = 0; k < 9; k++) maps[m].SetLayerVisible(8 - k, k == selected); // files 0..8 were prepended in order
            maps[m].RefreshLayers();
            headings[m].Text = AccuracyText.Headings[mode.SelectedIndex][m - 1];
        }
    }

    /// <summary>Owned shapes on all maps: four translucent bands dim the outside of the evaluated
    /// cells, an outline marks them and a point shows the probed cell. GeoKernel.NET 1.5.40 wraps no
    /// overlay point, so the probe is redrawn with the area shapes.</summary>
    void DrawOverlays()
    {
        if (grids == null || area is not { } window) return;
        var (x0, y0, x1, y1) = Accuracy.Extent(grids, window);
        var t = grids.Transform;
        const double margin = 2000;
        double ox0 = t[0] - margin, oy0 = t[3] + grids.Height * t[5] - margin, ox1 = t[0] + grids.Width * t[1] + margin, oy1 = t[3] + margin;
        (double, double, double, double)[] bands = [(ox0, y1, ox1, oy1), (ox0, oy0, ox1, y0), (ox0, y0, x0, y1), (x1, y0, ox1, y1)];
        var dim = new GeoKernelLayerStyle { FillColor = "#ffffff", FillOpacity = 100, LineColor = "#ffffff", LineWidth = 0 };
        GeoKernelPoint[] outline = [new(x0, y0), new(x1, y0), new(x1, y1), new(x0, y1), new(x0, y0)];
        foreach (var map in maps)
        {
            map.ClearShapes();
            foreach (var (a, b, c, d) in bands)
                if (c > a && d > b) map.AddPolygonShape([new(a, b), new(c, b), new(c, d), new(a, d), new(a, b)], dim);
            map.AddPolylineShape(outline, new GeoKernelLayerStyle { LineColor = "#167d8d", LineWidth = 2 });
            if (probePoint is { } p)
                map.AddPointShape(new GeoKernelPoint(p.X, p.Y), new GeoKernelLayerStyle { PointColor = "#dc3672", PointSize = 11, LineColor = "#ffffff", LineWidth = 2 });
            map.RefreshLayers(); // owned shapes are drawn on the next full map render
        }
    }

    // ----- evaluation ----------------------------------------------------------------------
    void ChooseArea((double XMin, double YMin, double XMax, double YMax) extent, string name, int preset = -1)
    {
        if (grids == null) return;
        try { area = Accuracy.Window(grids, extent); }
        catch (ArgumentException e) { status.Text = e.Message; return; }
        areaName = name;
        presets.SelectedIndex = preset < 0 ? 4 : preset;
        UpdateMetrics();
        DrawOverlays();
        status.Text = "Updated both methods on exactly the same source cells.";
    }

    void ApplyPreset(int index)
    {
        if (grids == null) return;
        if (index == 4) { areaButton.Checked = true; return; }
        areaButton.Checked = false;
        ChooseArea(Accuracy.PresetArea(grids, index), Accuracy.Presets[index], index);
    }

    void UpdateMetrics()
    {
        if (grids == null || area is not { } window) return;
        double limit = (double)threshold.Value;
        metrics = Accuracy.Measure(grids, window, limit);
        histogram.Metrics = metrics;
        histogram.Invalidate();
        table.Columns[5].HeaderText = $"|error| > {limit:0.#} m (%)";
        for (int m = 0; m < 2; m++)
        {
            var values = AccuracyText.Row(m, metrics[m]);
            for (int c = 0; c < 6; c++)
            {
                table.Rows[m].Cells[c].Value = values[c];
                table.Rows[m].Cells[c].Style.ForeColor = ColorTranslator.FromHtml(Accuracy.MethodColors[m]);
            }
        }
        table.ClearSelection();
        winners.Text = AccuracyText.Winners(metrics);
        scope.Text = AccuracyText.Scope(areaName, metrics);
    }

    void Inspect(double x, double y)
    {
        if (grids == null || busy) return;
        var t = grids.Transform;
        double col = (x - t[0]) / .5, row = (t[3] - y) / .5;
        if (!(col >= 0 && row >= 0 && col < grids.Width && row < grids.Height)) { status.Text = "Click inside the source raster."; return; }
        int cx = (int)col, cy = (int)row;
        probePoint = (t[0] + (cx + .5) * .5, t[3] - (cy + .5) * .5);
        probe.Text = AccuracyText.Probe(grids, cx, cy, area);
        DrawOverlays();
    }

    void ExportReport()
    {
        if (grids == null || area is not { } window || metrics == null) return;
        using var dialog = new SaveFileDialog { Title = "Export evaluation report", FileName = "accuracy-report.json", Filter = "JSON (*.json)|*.json" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try { Accuracy.ExportReport(dialog.FileName, grids, window, areaName, (double)threshold.Value, metrics, assets.Sha256); }
        catch (Exception e) { MessageBox.Show(this, e.Message, "Export report", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
        status.Text = "Saved selected-area metrics and source provenance.";
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

    sealed class Legend : Control
    {
        public int Mode;
        public double Limit = 2;

        public Legend() { DoubleBuffered = true; ResizeRedraw = true; BackColor = Color.White; }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.Clear(Color.White);
            using var text = new SolidBrush(ColorTranslator.FromHtml("#476174"));
            var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            if (Mode == 2) { g.DrawString(AccuracyText.LegendRelief, Font, text, ClientRectangle, center); return; }
            var top = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString(AccuracyText.LegendTitle(Mode), Font, text, new RectangleF(8, 2, Width - 16, 20), top);
            var bar = new RectangleF(70, 25, Math.Max(1, Width - 140), 13);
            using (var brush = new LinearGradientBrush(bar, Color.White, Color.White, 0f))
            {
                var blend = new ColorBlend(21);
                for (int i = 0; i <= 20; i++)
                {
                    double t = i / 20.0, value = Mode == 0 ? (t * 2 - 1) * Limit : t * Limit;
                    var c = Accuracy.ErrorColor(value, Mode == 1, Limit);
                    blend.Colors[i] = Color.FromArgb(c.R, c.G, c.B);
                    blend.Positions[i] = (float)t;
                }
                brush.InterpolationColors = blend;
                g.FillRectangle(brush, bar);
            }
            g.DrawString(Mode == 0 ? $"≤ −{Limit:0.#} m" : "0 m", Font, text, 8, 42);
            var right = new StringFormat { Alignment = StringAlignment.Far };
            g.DrawString($"≥ {Limit:0.#} m", Font, text, new RectangleF(Width - 188, 42, 180, 18), right);
            g.DrawString(AccuracyText.LegendNote, Font, text, new RectangleF(200, 42, Math.Max(1, Width - 400), 18), top);
        }
    }

    sealed class Histogram : Control
    {
        public AccuracyMetric[]? Metrics;

        public Histogram() { DoubleBuffered = true; ResizeRedraw = true; BackColor = Color.White; }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            if (Metrics is not { } metrics) return;
            using var text = new SolidBrush(ColorTranslator.FromHtml("#476174"));
            var center = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString("Absolute error distribution · evaluated area · teal classical / purple AI", Font, text, new RectangleF(8, 2, Width - 16, 20), center);
            var plot = new RectangleF(50, 30, Math.Max(1, Width - 70), Math.Max(1, Height - 60));
            float group = plot.Width / 6;
            double maximum = Math.Max(10, metrics.SelectMany(m => m.Bins.Select(b => 100.0 * b / m.Count)).Max());
            maximum = Math.Ceiling(maximum / 10) * 10;
            using var grid = new Pen(ColorTranslator.FromHtml("#e2e9ee"));
            var right = new StringFormat { Alignment = StringAlignment.Far };
            for (int tick = 0; tick <= 2; tick++)
            {
                float y = plot.Bottom - tick * .5f * plot.Height;
                g.DrawLine(grid, plot.Left, y, plot.Right, y);
                g.DrawString($"{tick * .5 * maximum:F0}%", Font, text, new RectangleF(0, y - 8, 43, 18), right);
            }
            for (int b = 0; b < 6; b++)
            {
                for (int m = 0; m < 2; m++)
                {
                    float h = (float)(100.0 * metrics[m].Bins[b] / metrics[m].Count / maximum * plot.Height);
                    using var brush = new SolidBrush(ColorTranslator.FromHtml(Accuracy.MethodColors[m]));
                    g.FillRectangle(brush, plot.Left + b * group + group * (.18f + m * .32f), plot.Bottom - h, group * .28f, h);
                }
                g.DrawString(AccuracyText.BinLabels[b], Font, text, new RectangleF(plot.Left + b * group, plot.Bottom + 3, group, 20), center);
            }
        }
    }
}
