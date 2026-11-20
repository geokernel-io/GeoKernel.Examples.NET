using System.Diagnostics;
using System.Drawing.Drawing2D;
using GeoKernel.Examples.DsmToDtm;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.DsmToDtm.Winforms;

/// <summary>
/// Ground beneath buildings: two linked native 3D viewers show the original DSM beside a
/// prepared terrain estimate. The slider interpolates numeric heights between both endpoints.
/// No inference runs here; the reference DTM is used only for the profile and scores.
/// </summary>
internal sealed class MainForm : Form
{
    static readonly Color Page = ColorTranslator.FromHtml("#f2f6f8"), Ink = ColorTranslator.FromHtml("#233e50");

    readonly DsmOptions options;
    readonly SampleAssets assets = SampleAssets.ZurichV2();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-to-dtm-").FullName;
    readonly GeoKernelViewer3DControl[] viewers = [new(), new()];
    readonly Label[] headings = new Label[2];
    readonly ComboBox method = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 270 };
    readonly TrackBar slider = new() { Minimum = 0, Maximum = 100, Value = 100, TickStyle = TickStyle.None, Width = 270, LargeChange = 10 };
    readonly NumericUpDown exaggeration = new() { Minimum = .25m, Maximum = 4, Increment = .25m, DecimalPlaces = 2, Value = 1, Width = 80 };
    readonly Label percentLabel = MakeLabel("100% · prepared DTM endpoint"), metrics = MakeLabel("Preparing the sample...", "metrics"), status = new() { AutoSize = true };
    readonly Button retry = new() { Text = "Retry / reload sample", Width = 270, Height = 32 }, cancel = new() { Text = "Cancel loading", Width = 270, Height = 32 };
    readonly Button[] endpoints = [new() { Text = "Original · 0%", Width = 132, Height = 32 }, new() { Text = "Terrain · 100%", Width = 132, Height = 32 }];
    readonly ProfileChart chart = new() { Dock = DockStyle.Fill };
    readonly ProgressBar progress = new() { Width = 200, Height = 16, Visible = false };
    readonly System.Windows.Forms.Timer cameraPoll = new() { Interval = 30 }, debounce = new() { Interval = 200 };
    readonly float[]?[] lastCamera = new float[]?[2];
    CancellationTokenSource cts = new();
    Grids? grids;
    bool ready, busy, closeQueued, sliderDown;
    int committedMethod = 1, committedPercent = 100, serial;
    float heightScale = 1;

    static Label MakeLabel(string text, string? style = null)
    {
        var label = new Label { Text = text, AutoSize = true, MaximumSize = new Size(270, 0), Margin = new Padding(0, 0, 0, 10) };
        switch (style)
        {
            case "title": label.Font = new Font("Segoe UI", 16, FontStyle.Bold); label.ForeColor = ColorTranslator.FromHtml("#145e72"); break;
            case "notice": label.BackColor = ColorTranslator.FromHtml("#fff5db"); label.ForeColor = ColorTranslator.FromHtml("#705528"); label.Padding = new Padding(8); break;
            case "metrics": label.BackColor = Color.White; label.Padding = new Padding(10); label.BorderStyle = BorderStyle.FixedSingle; break;
        }
        return label;
    }

    public MainForm(DsmOptions options)
    {
        this.options = options;
        Text = "DSM to DTM | Ground beneath buildings — GeoKernel";
        ClientSize = new Size(1520, 960);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Page;
        ForeColor = Ink;
        Font = new Font("Segoe UI", 9);
        ExampleIcon.Apply(this);
        BuildLayout();
        cameraPoll.Tick += (_, _) => PollCameras();
        debounce.Tick += (_, _) => { debounce.Stop(); if (!sliderDown) _ = UpdateSurfaceAsync(); };
        SetBusy(false);
        Shown += (_, _) => _ = StartAsync();
    }

    void BuildLayout()
    {
        var sidebar = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Dock = DockStyle.Left, Width = 300, Padding = new Padding(12) };
        method.Items.AddRange(["Classical · morphological / TIN", "AI · DSM-only pix_grad"]);
        method.SelectedIndex = 0;
        var endpointRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 0, 0, 10) };
        endpointRow.Controls.AddRange(endpoints);
        var exaggerationRow = new FlowLayoutPanel { AutoSize = true, Margin = new Padding(0, 0, 0, 10) };
        exaggerationRow.Controls.Add(new Label { Text = "Vertical exaggeration (×)", AutoSize = true, Margin = new Padding(0, 6, 8, 0) });
        exaggerationRow.Controls.Add(exaggeration);
        sidebar.Controls.AddRange([
            MakeLabel("Ground beneath buildings", "title"),
            MakeLabel("Zürich · 250 × 250 m\n0.5 m height pixels · EPSG:2056"),
            MakeLabel("1  Inspect the original roofs and trees.\n2  Choose a prepared terrain estimate.\n3  Move the slider to reveal the ground."),
            MakeLabel("Terrain estimate"), method, percentLabel, slider, endpointRow,
            MakeLabel("Intermediate positions interpolate heights for display. They are not new terrain estimates."),
            exaggerationRow, metrics,
            MakeLabel("Prepared results · no inference runs here. Both methods start from the original DSM independently.", "notice"),
            retry, cancel,
            MakeLabel("Source: Federal Office of Topography swisstopo\nAI: David Perez · experimental pix_grad"),
        ]);

        var views = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 2 };
        views.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        views.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        views.RowStyles.Add(new RowStyle(SizeType.Absolute, 38));
        views.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (int i = 0; i < 2; i++)
        {
            headings[i] = new Label
            {
                Text = i == 0 ? "ORIGINAL DSM · roofs + trees + ground" : "CLASSICAL DTM · prepared terrain estimate",
                Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0),
                BackColor = ColorTranslator.FromHtml("#e4eef2"), ForeColor = ColorTranslator.FromHtml("#125a70"), Font = new Font(Font, FontStyle.Bold),
            };
            viewers[i].Dock = DockStyle.Fill;
            viewers[i].MinimumSize = new Size(300, 320);
            views.Controls.Add(headings[i], i, 0);
            views.Controls.Add(viewers[i], i, 1);
        }

        var workspace = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(0, 12, 12, 6) };
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 200));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Label Wide(string text) => new() { Text = text, AutoSize = true, Margin = new Padding(0, 4, 0, 4) };
        workspace.Controls.Add(Wide("One observed surface. Two independent ways to estimate terrain."), 0, 0);
        workspace.Controls.Add(views, 0, 1);
        workspace.Controls.Add(chart, 0, 2);
        workspace.Controls.Add(Wide("Profile: gray DSM · teal classical · purple AI · dashed orange reference. The line is fixed across the same buildings in both views."), 0, 3);
        workspace.Controls.Add(Wide("Local 3D preview of source LN02 heights; no geoid conversion. DSM/reference source years: 2018/2019. Fixed development crop; scores are not a held-out accuracy benchmark."), 0, 4);

        var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, BackColor = Color.White, Padding = new Padding(6), ImageScalingSize = new Size(20, 20) };
        ToolStripButton Tool(string text, string? image, Action action)
        {
            var button = new ToolStripButton(text, image == null ? null : LoadImage(image)) { DisplayStyle = image == null ? ToolStripItemDisplayStyle.Text : ToolStripItemDisplayStyle.Image, ToolTipText = text };
            button.Click += (_, _) => action();
            toolbar.Items.Add(button);
            return button;
        }
        Tool("Reset view", "FullExtent.png", ResetView);
        Tool("Top view", null, TopView);
        Tool("Zoom in", "ZoomIn.png", () => Zoom(.8f));
        Tool("Zoom out", "ZoomOut.png", () => Zoom(1.25f));
        var profile = Tool("Show profile", null, () => { });
        profile.CheckOnClick = true;
        profile.Checked = true;
        profile.CheckedChanged += (_, _) => chart.Visible = profile.Checked;
        Tool("Data folder", null, () => { Directory.CreateDirectory(options.DataRoot); Process.Start(new ProcessStartInfo(options.DataRoot) { UseShellExecute = true }); });
        Tool("Controls", null, () => MessageBox.Show(this, DsmText.Controls, "Camera controls"));

        var statusStrip = new StatusStrip { SizingGrip = false };
        statusStrip.Items.Add(new ToolStripControlHost(status));
        statusStrip.Items.Add(new ToolStripStatusLabel { Spring = true });
        statusStrip.Items.Add(new ToolStripControlHost(progress));

        Controls.Add(workspace);
        Controls.Add(sidebar);
        Controls.Add(toolbar);
        Controls.Add(statusStrip);

        retry.Click += (_, _) => _ = StartAsync();
        cancel.Click += (_, _) => RequestCancel();
        slider.ValueChanged += (_, _) => { percentLabel.Text = DsmText.PercentLabel(slider.Value); debounce.Stop(); debounce.Start(); };
        slider.MouseDown += (_, _) => sliderDown = true;
        slider.MouseUp += (_, _) => { sliderDown = false; debounce.Stop(); _ = UpdateSurfaceAsync(); };
        endpoints[0].Click += (_, _) => slider.Value = 0;
        endpoints[1].Click += (_, _) => slider.Value = 100;
        method.SelectedIndexChanged += (_, _) => { debounce.Stop(); _ = UpdateSurfaceAsync(); };
        exaggeration.ValueChanged += (_, _) => SetExaggeration((float)exaggeration.Value);
    }

    static Image LoadImage(string name)
    {
        using var stream = typeof(MainForm).Assembly.GetManifestResourceStream("GeoKernel.Examples." + name)
            ?? throw new InvalidOperationException($"Embedded image {name} is missing.");
        return Image.FromStream(stream);
    }

    // ----- state ---------------------------------------------------------------------------
    void SetBusy(bool value)
    {
        busy = value;
        retry.Enabled = !value;
        cancel.Enabled = value;
        foreach (Control control in new Control[] { method, slider, endpoints[0], endpoints[1] })
            control.Enabled = !value && ready;
        progress.Visible = value;
    }

    void Report(int percent, string message)
    {
        progress.Style = ProgressBarStyle.Continuous;
        progress.Value = Math.Clamp(percent, 0, 100);
        status.Text = message;
    }

    void Failure(string message)
    {
        SetBusy(false);
        status.Text = message;
        if (closeQueued) { BeginInvoke(Close); return; }
        if (!cts.IsCancellationRequested) MessageBox.Show(this, message, "Sample unavailable", MessageBoxButtons.OK, MessageBoxIcon.Warning);
    }

    void RequestCancel()
    {
        cts.Cancel();
        cancel.Enabled = false;
        status.Text = "Cancelling... Previous complete views are retained.";
    }

    async Task StartAsync()
    {
        if (busy) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        SetBusy(true);
        int m = method.SelectedIndex + 1, percent = slider.Value;
        string surface = Path.Combine(temporary, $"surface-{++serial}.tif");
        try
        {
            string folder = await SampleCache.PrepareAsync(assets, options, new Progress<(int Percent, string Message)>(p => Report(p.Percent, p.Message)), token);
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(folder);
            Report(0, "Reading numeric heights and preparing both native 3D scenes...");
            var loaded = await Task.Run(() => Grids.Read(folder, token), token);
            string path = await Task.Run(() => loaded.SurfaceFile(m, percent, surface), token);
            await LoadViewersAsync(loaded, Path.Combine(folder, "dsm.tif"), path, token);
            Commit(loaded, m, percent);
        }
        catch (Exception e) { Abort(e); }
    }

    async Task UpdateSurfaceAsync()
    {
        if (!ready || busy || grids == null) return;
        int m = method.SelectedIndex + 1, percent = slider.Value;
        if (m == committedMethod && percent == committedPercent) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        SetBusy(true);
        Report(0, "Preparing the interpolated display surface...");
        var current = grids;
        string surface = Path.Combine(temporary, $"surface-{++serial}.tif");
        try
        {
            string path = await Task.Run(() => current.SurfaceFile(m, percent, surface), token);
            await LoadViewersAsync(current, null, path, token);
            Commit(current, m, percent);
        }
        catch (Exception e) { Abort(e); }
    }

    async Task LoadViewersAsync(Grids source, string? original, string result, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        // Viewer3D prepares geometry on its own worker; the C API reports no percentage.
        progress.Style = ProgressBarStyle.Marquee;
        status.Text = "Preparing native 3D terrain...";
        string neutral = Path.Combine(source.Folder, "neutral_surface.tif");
        var loads = new List<Task>();
        if (original != null) loads.Add(viewers[0].LoadTerrainAndImageryAsync(original, neutral, 500, token));
        loads.Add(viewers[1].LoadTerrainAndImageryAsync(result, neutral, 500, token));
        await Task.WhenAll(loads);
    }

    void Abort(Exception e)
    {
        if (IsDisposed) return;
        method.SelectedIndex = committedMethod - 1;
        slider.Value = committedPercent;
        debounce.Stop();
        percentLabel.Text = DsmText.PercentLabel(committedPercent);
        Failure(e is OperationCanceledException || cts.IsCancellationRequested ? "Loading cancelled. Previous complete views retained." : e.Message);
    }

    void Commit(Grids loaded, int m, int percent)
    {
        bool first = !ready;
        grids = loaded;
        ready = true;
        committedMethod = m;
        committedPercent = percent;
        foreach (var viewer in viewers) viewer.HeightScale = heightScale;
        chart.Grids = loaded;
        chart.Selected = m;
        chart.Invalidate();
        if (first) { ResetView(); cameraPoll.Start(); }
        else SyncFrom(0);
        headings[1].Text = DsmText.Heading(m, percent);
        metrics.Text = DsmText.Metrics(loaded);
        percentLabel.Text = DsmText.PercentLabel(slider.Value);
        SetBusy(false);
        status.Text = DsmText.Ready;
        if (closeQueued) BeginInvoke(Close);
    }

    // ----- cameras -------------------------------------------------------------------------
    void ResetView()
    {
        if (!ready || grids == null) return;
        viewers[0].ResetCamera();
        // Both scenes share the grid-centre origin at zero height: the crop centre at the
        // mean DSM height is (0, mean * scale * exaggeration, 0) in render units.
        viewers[0].MoveCamera([20, 48, 3.25f, 0, (float)(grids.MeanDsm * grids.RenderScale * heightScale), 0], 0);
        SyncFrom(0);
    }

    void TopView()
    {
        if (!ready) return;
        var camera = viewers[0].GetCamera();
        camera[0] = 0;
        camera[1] = 85;
        viewers[0].MoveCamera(camera, 0);
        SyncFrom(0);
    }

    void Zoom(float factor)
    {
        if (!ready) return;
        var camera = viewers[0].GetCamera();
        camera[2] = Math.Clamp(camera[2] * factor, .05f, 20f);
        viewers[0].MoveCamera(camera, 0);
        SyncFrom(0);
    }

    void SyncFrom(int source)
    {
        // Same grid, same origin and (to within 1e-5) the same render scale: render-space
        // cameras transfer directly between the two native scenes.
        viewers[1 - source].MoveCamera(viewers[source].GetCamera(), 0);
        for (int i = 0; i < 2; i++) lastCamera[i] = viewers[i].GetCamera();
    }

    void PollCameras()
    {
        // Pause while a scene is being replaced: its interim camera must not propagate.
        if (!ready || busy || IsDisposed) return;
        for (int i = 0; i < 2; i++)
            if (!Cameras.Same(lastCamera[i], viewers[i].GetCamera())) { SyncFrom(i); break; }
    }

    void SetExaggeration(float value)
    {
        float old = heightScale;
        heightScale = value;
        foreach (var viewer in viewers)
        {
            float[]? camera = ready ? viewer.GetCamera() : null;
            viewer.HeightScale = value;
            if (camera != null) { camera[4] *= value / old; viewer.MoveCamera(camera, 0); }
        }
        if (ready) SyncFrom(0);
    }

    // ----- shutdown ------------------------------------------------------------------------
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (busy) { closeQueued = true; RequestCancel(); e.Cancel = true; return; }
        cameraPoll.Stop();
        debounce.Stop();
        base.OnFormClosing(e);
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        base.OnFormClosed(e);
        foreach (var viewer in viewers) viewer.Dispose();
        try { Directory.Delete(temporary, true); } catch (IOException) { } catch (UnauthorizedAccessException) { }
    }

    sealed class ProfileChart : Control
    {
        public Grids? Grids;
        public int Selected = 1;

        public ProfileChart()
        {
            DoubleBuffered = true;
            BackColor = Color.White;
            ResizeRedraw = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.Clear(Color.White);
            if (Grids == null) return;
            var (rows, low, high) = Grids.Profile();
            var plot = new RectangleF(64, 30, Width - 88, Height - 64);
            var ink = ColorTranslator.FromHtml("#476174");
            using var text = new SolidBrush(ink);
            using var grid = new Pen(ColorTranslator.FromHtml("#e2e9ee"));
            g.DrawString("Fixed building transect · original pixel heights (LN02 metres)", Font, text, 8, 6);
            var right = new StringFormat { Alignment = StringAlignment.Far };
            var center = new StringFormat { Alignment = StringAlignment.Center };
            for (int i = 0; i <= 4; i++)
            {
                float t = i / 4f, y = plot.Bottom - t * plot.Height;
                g.DrawLine(grid, plot.Left, y, plot.Right, y);
                g.DrawString($"{low + t * (high - low):F0}", Font, text, new RectangleF(0, y - 8, 56, 18), right);
                g.DrawString($"{t * 249.5:F0} m", Font, text, new RectangleF(plot.Left + t * plot.Width - 25, plot.Bottom + 5, 50, 18), center);
            }
            foreach (int index in new[] { 0, 3, 1, 2 })
            {
                var points = rows[index].Select((v, x) => new PointF(plot.Left + (float)x / (Grids.Width - 1) * plot.Width,
                    (float)(plot.Bottom - (v - low) / (high - low) * plot.Height))).ToArray();
                using var pen = new Pen(ColorTranslator.FromHtml(DsmText.Colors[index]), index == Selected ? 2.5f : 1.4f);
                if (index == 3) pen.DashStyle = DashStyle.Dash;
                g.DrawLines(pen, points);
            }
        }
    }
}
