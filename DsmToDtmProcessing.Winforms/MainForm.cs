using System.Text.Json.Nodes;
using GeoKernel.Examples.DsmToDtm;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.DsmToDtmProcessing.Winforms;

/// <summary>
/// Live classical DSM-to-DTM processing: run the multiscale morphological filter and TIN on the
/// input DSM, change parameters and inspect the new terrain on three linked native 2D maps
/// (original DSM, generated DTM, diagnostic). The reference is evaluation-only.
/// </summary>
internal sealed class MainForm : Form
{
    const string Title = "DSM to DTM | Live classical processing — GeoKernel";
    static readonly Color Page = ColorTranslator.FromHtml("#f2f6f8"), Ink = ColorTranslator.FromHtml("#233e50");

    readonly DsmOptions options;
    readonly SampleAssets assets = SampleAssets.ZurichV2();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-processing-").FullName;
    readonly GeoKernelViewerControl[] maps = [new(), new(), new()];
    readonly Label[] headings = new Label[3];
    readonly ComboBox presets = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 262 };
    readonly ComboBox mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 262 };
    readonly NumericUpDown working = Spin(.25m, 20, 2, .5m, 2), window = Spin(4, 200, 64, 4, 1), threshold = Spin(.05m, 3, .4m, .05m, 2),
        slope = Spin(0, .5m, .12m, .02m, 2), relief = Spin(0, 2, .5m, .1m, 2), retain = Spin(0, 1, .35m, .05m, 2);
    readonly NumericUpDown[] parameters;
    readonly Label source = MakeLabel("No source loaded.", true), state = MakeLabel("Load a DSM to begin."),
        metrics = MakeLabel("Reference errors appear after a completed run.", true), probe = MakeLabel("Click any map to inspect source heights.", true),
        legend = new() { AutoSize = true, Margin = new Padding(0, 6, 0, 2), Text = "Original DSM remains visible. Both relief maps use identical lighting and contrast." },
        summary = new() { AutoSize = true, Margin = new Padding(0, 2, 0, 2), Text = "No processing has run yet." }, status = new() { AutoSize = true };
    readonly Button sampleButton = MakeButton("Load Zurich sample"), openButton = MakeButton("Open DSM GeoTIFF..."),
        referenceButton = MakeButton("Open reference (optional)..."), runButton = MakeButton("Run DSM → DTM"), cancelButton = MakeButton("Cancel"),
        saveButton = MakeButton("Save result GeoTIFFs...");
    readonly List<ToolStripItem> navigation = [];
    readonly ProgressBar progress = new() { Width = 230, Height = 16, Visible = false };
    readonly (double X, double Y)?[] presses = new (double X, double Y)?[3];
    readonly GeoKernelExtent?[] applied = new GeoKernelExtent?[3];
    CancellationTokenSource cts = new();
    TerrainScene? scene;
    TerrainRun? run;
    (double X, double Y)? probePoint;
    GeoKernelViewerTool tool = GeoKernelViewerTool.Pan;
    bool busy, closeQueued, closed, syncing;
    int generation;

    static NumericUpDown Spin(decimal minimum, decimal maximum, decimal value, decimal step, int decimals) =>
        new() { Minimum = minimum, Maximum = maximum, Value = value, Increment = step, DecimalPlaces = decimals, Width = 110 };

    static Label MakeLabel(string text, bool card = false)
    {
        var label = new Label { Text = text, AutoSize = true, MaximumSize = new Size(262, 0), Margin = new Padding(0, 0, 0, 10) };
        if (card) { label.BackColor = Color.White; label.Padding = new Padding(8); label.BorderStyle = BorderStyle.FixedSingle; label.MinimumSize = new Size(262, 0); }
        return label;
    }

    static Button MakeButton(string text) => new() { Text = text, Width = 262, Height = 32, Margin = new Padding(0, 0, 0, 8) };

    public MainForm(DsmOptions options)
    {
        this.options = options;
        parameters = [working, window, threshold, slope, relief, retain];
        Text = Title;
        ClientSize = new Size(1720, 960);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Page;
        ForeColor = Ink;
        Font = new Font("Segoe UI", 9);
        ExampleIcon.Apply(this);
        BuildLayout();
        SetBusy(false);
        Shown += (_, _) => _ = LoadSampleAsync();
    }

    void BuildLayout()
    {
        var sidebar = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Dock = DockStyle.Left, Width = 300, Padding = new Padding(12) };
        presets.Items.AddRange(Terrain.PresetNames);
        presets.SelectedIndex = 0;
        mode.Items.AddRange(TerrainText.DiagnosticNames);
        mode.SelectedIndex = 0;
        var title = MakeLabel("DSM → DTM\nRun the conversion");
        title.Font = new Font("Segoe UI", 15, FontStyle.Bold);
        title.ForeColor = ColorTranslator.FromHtml("#145e72");
        var form = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };
        string[] names = ["Working grid (m)", "Max. window (m)", "Base threshold (m)", "Slope allowance", "Relief allowance", "Keep tolerance (m)"];
        for (int i = 0; i < parameters.Length; i++)
        {
            form.Controls.Add(new Label { Text = names[i], AutoSize = true, Margin = new Padding(0, 6, 8, 0) }, 0, i);
            form.Controls.Add(parameters[i], 1, i);
            parameters[i].ValueChanged += (_, _) => { if (run != null) state.Text = TerrainText.ControlsChanged; };
        }
        sidebar.Controls.AddRange([
            title, MakeLabel("Live multiscale filtering + TIN.\nChange parameters, then Run."), sampleButton, openButton, referenceButton, source,
            presets, form, runButton, cancelButton, state, MakeLabel("Right map"), mode, metrics, saveButton, probe,
            MakeLabel("Sample: Federal Office of Topography swisstopo.\nReference is evaluation-only. Heights are metres; no vertical datum conversion."),
        ]);

        var views = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2 };
        for (int i = 0; i < 3; i++) views.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        views.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        views.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        string[] titles = ["ORIGINAL DSM · input", "PROCESSED DTM · Run to generate", "REMOVED HEIGHT · Run to generate"];
        for (int i = 0; i < 3; i++)
        {
            int side = i;
            headings[i] = new Label
            {
                Text = titles[i], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0),
                BackColor = ColorTranslator.FromHtml("#e4eef2"), ForeColor = ColorTranslator.FromHtml("#125a70"), Font = new Font(Font, FontStyle.Bold),
            };
            maps[i].Dock = DockStyle.Fill;
            maps[i].MinimumSize = new Size(240, 260);
            maps[i].VisibleExtentChanged += (_, _) => ExtentChanged(side);
            maps[i].MapMouseDown += (_, e) => { if (e.ButtonOrButtons == 1) presses[side] = (e.ScreenPoint.X, e.ScreenPoint.Y); };
            maps[i].MapMouseUp += (_, e) => MapClick(side, e);
            views.Controls.Add(headings[i], i, 0);
            views.Controls.Add(maps[i], i, 1);
        }

        var workspace = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(0, 12, 12, 6) };
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (int i = 0; i < 3; i++) workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.Controls.Add(views, 0, 0);
        workspace.Controls.Add(legend, 0, 1);
        workspace.Controls.Add(summary, 0, 2);
        workspace.Controls.Add(new Label { Text = TerrainText.EdgeNote, AutoSize = true, Margin = new Padding(0, 2, 0, 0) }, 0, 3);

        var toolbar = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, BackColor = Color.White, Padding = new Padding(6), ImageScalingSize = new Size(20, 20) };
        void Tool(string text, string? image, Action action)
        {
            var button = new ToolStripButton(text, image == null ? null : LoadImage(image)) { DisplayStyle = image == null ? ToolStripItemDisplayStyle.Text : ToolStripItemDisplayStyle.ImageAndText };
            button.Click += (_, _) => action();
            toolbar.Items.Add(button);
            navigation.Add(button);
        }
        Tool("Pan", null, () => UseTool(GeoKernelViewerTool.Pan));
        Tool("Zoom box", null, () => UseTool(GeoKernelViewerTool.ZoomBox));
        Tool("Zoom in", "ZoomIn.png", () => maps[0].ZoomIn(1.5));
        Tool("Zoom out", "ZoomOut.png", () => maps[0].ZoomOut(1.5));
        Tool("Fit scene", "FullExtent.png", Fit);

        var statusStrip = new StatusStrip { SizingGrip = false };
        statusStrip.Items.Add(new ToolStripControlHost(status));
        statusStrip.Items.Add(new ToolStripStatusLabel { Spring = true });
        statusStrip.Items.Add(new ToolStripControlHost(progress));

        Controls.Add(workspace);
        Controls.Add(sidebar);
        Controls.Add(toolbar);
        Controls.Add(statusStrip);

        presets.SelectedIndexChanged += (_, _) => ApplyPreset(presets.SelectedIndex);
        mode.SelectedIndexChanged += (_, _) => ShowDiagnostic();
        sampleButton.Click += (_, _) => _ = LoadSampleAsync();
        openButton.Click += (_, _) => OpenDsm();
        referenceButton.Click += (_, _) => OpenReference();
        runButton.Click += (_, _) => Process();
        cancelButton.Click += (_, _) => RequestCancel();
        saveButton.Click += (_, _) => SaveResult();
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
        foreach (Control c in (Control[])[sampleButton, openButton, presets, .. parameters]) c.Enabled = !value;
        foreach (var item in navigation) item.Enabled = !value && scene != null;
        runButton.Enabled = referenceButton.Enabled = !value && scene != null;
        saveButton.Enabled = mode.Enabled = !value && run != null;
        cancelButton.Enabled = value;
        progress.Visible = value;
    }

    void RequestCancel()
    {
        cts.Cancel();
        cancelButton.Enabled = false;
        status.Text = "Cancelling; completed result will be retained...";
    }

    void ApplyPreset(int index)
    {
        var p = Terrain.Presets[index];
        decimal[] values = [(decimal)p.Working, (decimal)p.Window, (decimal)p.Threshold, (decimal)p.Slope, (decimal)p.Terrain, (decimal)p.Retain];
        for (int i = 0; i < parameters.Length; i++) parameters[i].Value = values[i];
    }

    string NextFolder() => Directory.CreateDirectory(Path.Combine(temporary, (++generation).ToString())).FullName;

    /// <summary>Run a worker job; on success apply its result, keeping the last completed state on failure.</summary>
    async Task RunJob<T>(Func<CancellationToken, Action<int, string>, T> job, Action<T> apply)
    {
        if (busy) return;
        cts = new CancellationTokenSource();
        var token = cts.Token;
        SetBusy(true);
        var report = new Progress<(int Percent, string Message)>(p => { if (!closed) { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; } });
        IProgress<(int, string)> sink = report;
        try
        {
            T result = await Task.Run(() => job(token, (p, m) => sink.Report((p, m))), token);
            token.ThrowIfCancellationRequested();
            if (closed) return;
            apply(result);
            SetBusy(false);
            if (closeQueued) BeginInvoke(Close);
        }
        catch (Exception e)
        {
            if (closed) return;
            SetBusy(false);
            bool cancelled = e is OperationCanceledException || token.IsCancellationRequested;
            status.Text = cancelled ? "Cancelled. Last completed source/result retained." : e.Message;
            if (closeQueued) BeginInvoke(Close);
            else if (!cancelled) MessageBox.Show(this, e.Message, "Operation failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ----- jobs ----------------------------------------------------------------------------
    async Task LoadSampleAsync()
    {
        if (busy) return;
        string folder = NextFolder();
        await RunJob((token, report) =>
        {
            report(0, "Checking cached sample files...");
            string sample = SampleCache.PrepareAsync(assets, options, new Progress<(int Percent, string Message)>(p => report(p.Percent, p.Message)), token).GetAwaiter().GetResult();
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(sample);
            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(sample, "source-manifest.json")))!.AsObject();
            return TerrainJobs.Load(Path.Combine(sample, "dsm.tif"), Path.Combine(sample, "dtm_reference.tif"), manifest, folder, token, report);
        }, loaded => Publish(loaded, null, "Source ready. Choose parameters and Run."));
    }

    void OpenDsm()
    {
        if (busy) return;
        using var dialog = new OpenFileDialog { Title = "Open DSM: metre heights in projected metre CRS", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        string path = dialog.FileName, folder = NextFolder();
        _ = RunJob((token, report) => TerrainJobs.Load(path, null, new JsonObject(), folder, token, report),
            loaded => Publish(loaded, null, "Source ready. Choose parameters and Run."));
    }

    void OpenReference()
    {
        if (busy || scene is not { } current) return;
        using var dialog = new OpenFileDialog { Title = "Open aligned reference (evaluation only)", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        string path = dialog.FileName, folder = NextFolder();
        var completed = run;
        _ = RunJob((token, report) => TerrainJobs.AttachReference(current, completed, path, folder, token, report),
            result => Publish(result.Scene, result.Run, "Reference attached; processing heights unchanged."));
    }

    void Process()
    {
        if (busy || scene is not { } current) return;
        var p = new TerrainParameters((double)working.Value, (double)window.Value, (double)threshold.Value, (double)slope.Value, (double)relief.Value, (double)retain.Value);
        string folder = NextFolder();
        _ = RunJob((token, report) => TerrainJobs.Process(current, p, folder, token, report),
            result => Publish(current, result, "Processing complete. Change parameters to run again, or save the result."));
    }

    void SaveResult()
    {
        if (busy || scene is not { } current || run is not { } completed) return;
        using var dialog = new SaveFileDialog { Title = "Save DTM, removed heights, QA and JSON", FileName = "terrain-result.tif", Filter = "GeoTIFF (*.tif)|*.tif", OverwritePrompt = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        string[] destinations = TerrainJobs.Destinations(dialog.FileName);
        if (TerrainJobs.UsesSource(current, destinations))
        {
            MessageBox.Show(this, "Choose output names different from the source and reference.", "Cannot export", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (destinations.Skip(1).Any(File.Exists) && MessageBox.Show(this, "Removed-height, QA or JSON companion files already exist. Replace the result set?",
                "Replace existing result companions?", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        _ = RunJob((token, report) => { TerrainJobs.Save(completed, destinations, token, report); return true; },
            _ => { state.Text = status.Text = "Saved DTM, removed heights, QA flags and provenance JSON."; });
    }

    void Publish(TerrainScene loaded, TerrainRun? result, string message)
    {
        bool changed = scene == null || !ReferenceEquals(loaded.Dsm, scene.Dsm);
        scene = loaded;
        run = result;
        if (changed)
        {
            Replace(0, loaded.Original);
            for (int pane = 1; pane < 3; pane++) { maps[pane].ClearLayers(); maps[pane].RefreshLayers(); }
            probePoint = null;
            DrawProbe();
            probe.Text = "Click a map to inspect source heights.";
            Array.Clear(applied);
            BeginInvoke(Fit);
        }
        if (run != null)
        {
            Replace(1, run.Display);
            ShowDiagnostic();
            summary.Text = TerrainText.Summary(run.Terrain);
            metrics.Text = TerrainText.Metrics(run.All, run.Interior, loaded.Reference != null);
        }
        else
        {
            headings[1].Text = "PROCESSED DTM · Run to generate";
            headings[2].Text = "REMOVED HEIGHT · Run to generate";
            metrics.Text = "No processing has run yet.";
            summary.Text = "Source loaded. Run to generate a new terrain estimate.";
        }
        source.Text = TerrainText.Source(loaded.Dsm, loaded.Reference != null);
        state.Text = status.Text = message;
    }

    // ----- maps ----------------------------------------------------------------------------
    /// <summary>Swap a map's raster but keep its view; the empty map adopts the raster's CRS.</summary>
    void Replace(int pane, string path)
    {
        var map = maps[pane];
        GeoKernelExtent? extent = map.LayerCount > 0 ? map.ViewExtent : null;
        syncing = true;
        try
        {
            map.ClearLayers();
            if (!map.AddLayerFile(path)) throw new InvalidOperationException($"The display layer could not be added: {path}");
            map.SetTool(tool);
            if (extent is { } e) map.ViewExtent = e;
            map.RefreshLayers();
        }
        finally { syncing = false; }
    }

    void Fit()
    {
        if (scene == null) return;
        var (x0, y0, x1, y1) = scene.Dsm.Extent;
        double pad = scene.Dsm.Pixel * 10;
        syncing = true;
        try { maps[0].ViewExtent = new GeoKernelExtent(x0 - pad, y0 - pad, x1 + pad, y1 + pad); }
        finally { syncing = false; }
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
        BeginInvoke(() => { if (!syncing && !closed && scene != null && !Same(applied[side], maps[side].ViewExtent)) SyncFrom(side); });
    }

    void MapClick(int side, GeoKernelMapMouseEventArgs e)
    {
        if (e.ButtonOrButtons != 1) return;
        var press = presses[side];
        presses[side] = null;
        if (press is { } p && tool == GeoKernelViewerTool.Pan && Math.Abs(p.X - e.ScreenPoint.X) + Math.Abs(p.Y - e.ScreenPoint.Y) <= 3)
        {
            double x = e.WorldPoint.X, y = e.WorldPoint.Y;
            BeginInvoke(() => Inspect(x, y));
        }
    }

    void UseTool(GeoKernelViewerTool value)
    {
        tool = value;
        foreach (var map in maps) map.SetTool(value);
    }

    void ShowDiagnostic()
    {
        if (run == null || scene == null) return;
        int selected = mode.SelectedIndex;
        if (selected == 2 && scene.Reference == null)
        {
            mode.SelectedIndex = 0; // re-enters through SelectedIndexChanged
            status.Text = "Attach an aligned reference to show errors.";
            return;
        }
        Replace(2, run.Diagnostics[selected]);
        headings[1].Text = "PROCESSED DTM · live native result";
        headings[2].Text = TerrainText.DiagnosticTitles[selected];
        legend.Text = TerrainText.Legends[selected];
        DrawProbe();
    }

    /// <summary>GeoKernel.NET 1.5.40 wraps no overlay point, so the probe is an owned point shape.</summary>
    void DrawProbe()
    {
        foreach (var map in maps)
        {
            map.ClearShapes();
            if (probePoint is { } p)
                map.AddPointShape(new GeoKernelPoint(p.X, p.Y), new GeoKernelLayerStyle { PointColor = "#d73573", PointSize = 11, LineColor = "#ffffff", LineWidth = 2 });
            map.RefreshLayers(); // owned shapes are drawn on the next full map render
        }
    }

    void Inspect(double x, double y)
    {
        if (scene == null || busy) return;
        if (TerrainText.Probe(scene.Dsm, run?.Terrain, scene.Reference, x, y) is not { } hit) return;
        probe.Text = hit.Text;
        probePoint = (hit.X, hit.Y);
        DrawProbe();
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
}
