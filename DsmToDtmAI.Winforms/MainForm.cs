using GeoKernel.Examples.DsmToDtm;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.DsmToDtmAI.Winforms;

/// <summary>
/// Live AI versus classical: run the experimental pix_grad model through GeoKernel.AI and the native
/// classical filter independently on the same DSM, and compare the generated heights on three linked
/// native 2D maps (original DSM, classical, AI). The reference is evaluation-only.
/// </summary>
internal sealed class MainForm : Form
{
    const string Title = "DSM to DTM | Live AI versus classical — GeoKernel";
    static readonly Color Page = ColorTranslator.FromHtml("#f2f6f8"), Ink = ColorTranslator.FromHtml("#233e50");

    readonly AiOptions options;
    readonly SampleAssets sampleAssets = SampleAssets.ZurichV2();
    readonly System.Text.Json.Nodes.JsonObject modelAssets = SampleAssets.Resource("ai-model-assets.json"), terrainAssets = Ai.TerrainAssets();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-ai-").FullName;
    readonly GeoKernelViewerControl[] maps = [new(), new(), new()];
    readonly Label[] headings = new Label[3];
    readonly NumericUpDown overlap = new() { Minimum = 0, Maximum = 128, Increment = 32, Value = 64, Width = 110 },
        threads = new() { Minimum = 1, Maximum = 8, Value = 4, Width = 110 };
    readonly ComboBox preset = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 },
        mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 272 },
        scope = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 272 };
    readonly Label source = MakeLabel("Preparing source data...", true), modelInfo = MakeLabel("Model not prepared.", true),
        state = MakeLabel("Load source and model, then Run."), probe = MakeLabel("Click a source pixel to inspect both estimates.", true),
        legend = new() { AutoSize = true, Margin = new Padding(0, 6, 0, 4), Text = "Same light and contrast for both methods. No conversion has run yet." },
        summary = new() { AutoSize = true, Margin = new Padding(0, 4, 0, 2), Text = "No prepared terrain result is loaded." }, status = new() { AutoSize = true };
    readonly DataGridView scores = new()
    {
        Dock = DockStyle.Fill, Height = 92, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
        RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
    };
    readonly Button sampleButton = MakeButton("Load full Zurich 1 km tile"), openButton = MakeButton("Open DSM GeoTIFF..."),
        referenceButton = MakeButton("Open reference (optional)..."), modelButton = MakeButton("Download / verify model"), zipButton = MakeButton("Use model ZIP..."),
        runButton = MakeButton("Run AI + classical"), cancelButton = MakeButton("Cancel"), saveButton = MakeButton("Save comparison folder...");
    readonly List<ToolStripItem> navigation = [];
    readonly ProgressBar progress = new() { Width = 230, Height = 16, Visible = false };
    readonly (double X, double Y)?[] presses = new (double X, double Y)?[3];
    readonly GeoKernelExtent?[] applied = new GeoKernelExtent?[3];
    CancellationTokenSource cts = new();
    TerrainScene? scene;
    AiComparison? comparison;
    string? model;
    (double X, double Y)? probePoint;
    GeoKernelViewerTool tool = GeoKernelViewerTool.Pan;
    bool busy, closeQueued, closed, syncing, setupModel;
    int generation;

    static Label MakeLabel(string text, bool card = false)
    {
        var label = new Label { Text = text, AutoSize = true, MaximumSize = new Size(272, 0), Margin = new Padding(0, 0, 0, 8) };
        if (card) { label.BackColor = Color.White; label.Padding = new Padding(8); label.BorderStyle = BorderStyle.FixedSingle; label.MinimumSize = new Size(272, 0); }
        return label;
    }

    static Button MakeButton(string text) => new() { Text = text, Width = 272, Height = 32, Margin = new Padding(0, 0, 0, 7) };

    public MainForm(AiOptions options)
    {
        this.options = options;
        Text = Title;
        ClientSize = new Size(1760, 980);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Page;
        ForeColor = Ink;
        Font = new Font("Segoe UI", 9);
        ExampleIcon.Apply(this);
        BuildLayout();
        SetBusy(false);
        Shown += (_, _) => LoadSample();
    }

    void BuildLayout()
    {
        var sidebar = new FlowLayoutPanel { FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true, Dock = DockStyle.Left, Width = 312, Padding = new Padding(12) };
        preset.Items.AddRange(Terrain.PresetNames);
        preset.SelectedIndex = 0;
        mode.Items.AddRange(AiText.Modes);
        mode.SelectedIndex = 0;
        scope.Items.AddRange(AiText.Scopes);
        scope.SelectedIndex = 0;
        var title = MakeLabel("AI / Classical\nRun both methods");
        title.Font = new Font("Segoe UI", 15, FontStyle.Bold);
        title.ForeColor = ColorTranslator.FromHtml("#145e72");
        var form = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };
        (string, Control)[] rows = [("Tile overlap (px)", overlap), ("CPU threads", threads), ("Classical preset", preset)];
        for (int i = 0; i < rows.Length; i++)
        {
            form.Controls.Add(new Label { Text = rows[i].Item1, AutoSize = true, Margin = new Padding(0, 6, 8, 0) }, 0, i);
            form.Controls.Add(rows[i].Item2, 1, i);
        }
        sidebar.Controls.AddRange([
            title, MakeLabel("One DSM · two independent estimates.\npix_grad · experimental DSM-only model."), sampleButton, openButton, referenceButton, source,
            modelButton, zipButton, modelInfo, form, runButton, cancelButton, state, MakeLabel("Compare maps"), mode, scope, MakeLabel(AiText.ScopeNote),
            saveButton, probe, MakeLabel(AiText.Attribution),
        ]);

        var views = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 2 };
        for (int i = 0; i < 3; i++) views.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33f));
        views.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        views.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        string[] titles = ["ORIGINAL DSM · shared input", "CLASSICAL DTM · Run to generate", "AI DTM · Run to generate"];
        for (int i = 0; i < 3; i++)
        {
            int side = i;
            headings[i] = new Label
            {
                Text = titles[i], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0),
                BackColor = ColorTranslator.FromHtml("#e4eef2"), ForeColor = ColorTranslator.FromHtml("#125a70"), Font = new Font(Font, FontStyle.Bold),
            };
            maps[i].Dock = DockStyle.Fill;
            maps[i].MinimumSize = new Size(250, 270);
            maps[i].VisibleExtentChanged += (_, _) => ExtentChanged(side);
            maps[i].MapMouseDown += (_, e) => { if (e.ButtonOrButtons == 1) presses[side] = (e.ScreenPoint.X, e.ScreenPoint.Y); };
            maps[i].MapMouseUp += (_, e) => MapClick(side, e);
            views.Controls.Add(headings[i], i, 0);
            views.Controls.Add(maps[i], i, 1);
        }

        foreach (string column in AiText.Columns) scores.Columns.Add(column.Replace(" ", ""), column);
        scores.Rows.Add(2);
        var workspace = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(0, 12, 12, 6) };
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 92));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.Controls.Add(views, 0, 0);
        workspace.Controls.Add(legend, 0, 1);
        workspace.Controls.Add(scores, 0, 2);
        workspace.Controls.Add(summary, 0, 3);
        workspace.Controls.Add(new Label { Text = AiText.Footer, AutoSize = true, Margin = new Padding(0, 2, 0, 0) }, 0, 4);

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
        Tool("Fit comparison", "FullExtent.png", Fit);

        var statusStrip = new StatusStrip { SizingGrip = false };
        statusStrip.Items.Add(new ToolStripControlHost(status));
        statusStrip.Items.Add(new ToolStripStatusLabel { Spring = true });
        statusStrip.Items.Add(new ToolStripControlHost(progress));

        Controls.Add(workspace);
        Controls.Add(sidebar);
        Controls.Add(toolbar);
        Controls.Add(statusStrip);

        void Changed() { if (comparison != null) state.Text = AiText.ControlsChanged; }
        overlap.ValueChanged += (_, _) => Changed();
        threads.ValueChanged += (_, _) => Changed();
        preset.SelectedIndexChanged += (_, _) => Changed();
        mode.SelectedIndexChanged += (_, _) => Display();
        scope.SelectedIndexChanged += (_, _) => { UpdateScores(); Fit(); };
        sampleButton.Click += (_, _) => LoadSample();
        openButton.Click += (_, _) => OpenDsm();
        referenceButton.Click += (_, _) => OpenReference();
        modelButton.Click += (_, _) => PrepareModel(null);
        zipButton.Click += (_, _) => LocalModel();
        runButton.Click += (_, _) => Process();
        cancelButton.Click += (_, _) => RequestCancel();
        saveButton.Click += (_, _) => ExportResults();
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
        foreach (Control c in (Control[])[sampleButton, openButton, modelButton, zipButton, overlap, threads, preset, scope]) c.Enabled = !value;
        foreach (var item in navigation) item.Enabled = !value && scene != null;
        runButton.Enabled = !value && scene != null && model != null;
        referenceButton.Enabled = !value && scene != null;
        saveButton.Enabled = mode.Enabled = !value && comparison != null;
        cancelButton.Enabled = value;
        progress.Visible = value;
    }

    void RequestCancel()
    {
        cts.Cancel();
        cancelButton.Enabled = false;
        status.Text = "Cancelling; completed comparison retained...";
    }

    string NextFolder() => Directory.CreateDirectory(Path.Combine(temporary, (++generation).ToString())).FullName;

    /// <summary>Run a worker job; on success apply its result, keeping the last completed state on failure.</summary>
    async void RunJob<T>(Func<CancellationToken, Action<int, string>, T> job, Action<T> apply)
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
            else if (setupModel && scene != null && model == null) { setupModel = false; BeginInvoke(() => PrepareModel(null)); }
        }
        catch (Exception e)
        {
            if (closed) return;
            SetBusy(false);
            bool cancelled = e is OperationCanceledException || token.IsCancellationRequested;
            state.Text = status.Text = cancelled ? "Cancelled; completed comparison retained." : e.Message;
            if (closeQueued) BeginInvoke(Close);
            else if (!cancelled) MessageBox.Show(this, e.Message, "Unable to complete operation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ----- jobs ----------------------------------------------------------------------------
    void PrepareModel(string? archive)
    {
        if (busy) return;
        RunJob((token, report) => Ai.PrepareModel(options, archive, token, report), folder =>
        {
            model = folder;
            modelInfo.Text = AiText.ModelReady;
            state.Text = status.Text = scene != null ? "Source and model ready. Press Run AI + classical." : "Model ready; load a DSM.";
        });
    }

    void LocalModel()
    {
        if (busy) return;
        using var dialog = new OpenFileDialog { Title = "Choose the pinned pix_grad model ZIP", Filter = "ZIP (*.zip)|*.zip" };
        if (dialog.ShowDialog(this) == DialogResult.OK) PrepareModel(dialog.FileName);
    }

    void LoadSample()
    {
        if (busy) return;
        string folder = NextFolder();
        setupModel = model == null;
        RunJob((token, report) =>
        {
            string support = SampleCache.PrepareAsync(sampleAssets, options.Sample, new Progress<(int Percent, string Message)>(p => report(p.Percent, "Projection/sample support: " + p.Message)), token)
                .GetAwaiter().GetResult();
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(support);
            string terrain = Ai.PrepareTerrain(terrainAssets, options.Sample.DataRoot, token, report);
            var manifest = (System.Text.Json.Nodes.JsonObject)terrainAssets.DeepClone();
            manifest["standard_full_tile"] = true;
            var loaded = TerrainJobs.Load(Path.Combine(terrain, "dsm.tif"), Path.Combine(terrain, "reference.tif"), manifest, folder, token, report);
            var (x0, y0, x1, y1) = loaded.Dsm.Extent;
            if (loaded.Dsm.Width != 2000 || loaded.Dsm.Height != 2000 || loaded.Dsm.Pixel != .5 || (x0, y0, x1, y1) != (2677000, 1251000, 2678000, 1252000))
                throw new InvalidDataException("Unexpected full Zurich grid.");
            return loaded;
        }, loaded => Publish(loaded, null, "Source ready. Prepare model and Run."));
    }

    void OpenDsm()
    {
        if (busy) return;
        using var dialog = new OpenFileDialog { Title = "Open metre-height DSM", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        string path = dialog.FileName, folder = NextFolder();
        RunJob((token, report) => TerrainJobs.Load(path, null, new System.Text.Json.Nodes.JsonObject(), folder, token, report),
            loaded => Publish(loaded, null, "Source ready. Prepare model and Run."));
    }

    void OpenReference()
    {
        if (busy || scene is not { } current) return;
        using var dialog = new OpenFileDialog { Title = "Open aligned evaluation-only reference", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        string path = dialog.FileName, folder = NextFolder();
        var completed = comparison;
        RunJob((token, report) =>
        {
            report(5, "Reading evaluation-only reference...");
            var reference = Terrain.Read(path, token);
            Terrain.ValidateReference(current.Dsm, reference);
            var next = current with { Reference = reference };
            var updated = completed == null ? null : Ai.AttachReference(next, completed, folder, modelAssets, token, report);
            token.ThrowIfCancellationRequested();
            return (next, updated);
        }, result => Publish(result.next, result.updated, "Reference updated. Both generated height arrays are unchanged."));
    }

    void Process()
    {
        if (busy || scene is not { } current || model is not { } package) return;
        var parameters = Ai.EffectiveParameters(current.Dsm, Terrain.Presets[preset.SelectedIndex]);
        int tileOverlap = (int)overlap.Value, cpu = (int)threads.Value;
        string folder = NextFolder();
        RunJob((token, report) => Ai.Compare(current, package, parameters, tileOverlap, cpu, folder, modelAssets, token, report),
            result => Publish(current, result, "Both methods completed from the same full DSM."));
    }

    void ExportResults()
    {
        if (busy || comparison is not { } completed) return;
        using var dialog = new FolderBrowserDialog { Description = "Choose parent folder for a new result bundle", UseDescriptionForTitle = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        string parent = dialog.SelectedPath;
        RunJob((token, report) => Ai.Export(completed, parent, token, report), target => { state.Text = status.Text = "Saved complete comparison bundle: " + target; });
    }

    void Publish(TerrainScene loaded, AiComparison? result, string message)
    {
        bool changed = scene == null || !ReferenceEquals(loaded.Dsm, scene.Dsm);
        scene = loaded;
        comparison = result;
        if (changed)
        {
            Replace(0, loaded.Original);
            for (int pane = 1; pane < 3; pane++) { maps[pane].ClearLayers(); maps[pane].RefreshLayers(); }
            probePoint = null;
            probe.Text = "Click a source pixel to inspect both estimates.";
            Array.Clear(applied);
            BeginInvoke(Fit);
        }
        source.Text = AiText.Source(loaded.Dsm, loaded.Reference != null);
        if (comparison != null)
        {
            Display();
            UpdateScores();
        }
        else
        {
            foreach (DataGridViewRow row in scores.Rows) foreach (DataGridViewCell cell in row.Cells) cell.Value = "";
            summary.Text = "No terrain conversion has run for this input.";
            headings[1].Text = "CLASSICAL DTM · Run to generate";
            headings[2].Text = "AI DTM · Run to generate";
        }
        state.Text = status.Text = message;
    }

    // ----- maps ----------------------------------------------------------------------------
    /// <summary>Swap a map's raster but keep its view; a map's first layer joins the linked view.</summary>
    void Replace(int pane, string path)
    {
        var map = maps[pane];
        GeoKernelExtent? extent = map.LayerCount > 0 ? map.ViewExtent : pane > 0 && maps[0].LayerCount > 0 ? maps[0].ViewExtent : null;
        syncing = true;
        try
        {
            map.ClearLayers();
            if (!map.AddLayerFile(path)) throw new InvalidOperationException($"The display layer could not be added: {path}");
            map.SetTool(tool);
            if (extent is { } e) { map.ViewExtent = e; applied[pane] = map.ViewExtent; }
            map.RefreshLayers();
        }
        finally { syncing = false; }
    }

    void Fit()
    {
        if (scene == null) return;
        bool crop = scope.SelectedIndex == 1;
        var (x0, y0, x1, y1) = crop ? Ai.CropExtent(scene) : scene.Dsm.Extent;
        double dx = (x1 - x0) * .025, dy = (y1 - y0) * .025;
        DrawShapes();
        syncing = true;
        try { maps[0].ViewExtent = new GeoKernelExtent(x0 - dx, y0 - dy, x1 + dx, y1 + dy); }
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
                if (other != side && maps[other].LayerCount > 0) { maps[other].ViewExtent = applied[side]!.Value; applied[other] = maps[other].ViewExtent; }
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
            if (!syncing && !closed && scene != null && maps[side].LayerCount > 0 && !Same(applied[side], maps[side].ViewExtent)) SyncFrom(side);
        });
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

    void Display()
    {
        if (comparison == null || scene == null) return;
        int selected = mode.SelectedIndex;
        if (selected == 2 && scene.Reference == null)
        {
            mode.SelectedIndex = 0; // re-enters through SelectedIndexChanged
            status.Text = "Attach an aligned reference to compare errors.";
            return;
        }
        for (int method = 0; method < 2; method++) Replace(method + 1, comparison.Maps[method][selected]);
        headings[1].Text = "CLASSICAL " + AiText.Suffix[selected];
        headings[2].Text = "AI " + AiText.Suffix[selected];
        legend.Text = AiText.Legends[selected];
        DrawShapes();
    }

    void UpdateScores()
    {
        if (comparison == null || scene == null) return;
        bool crop = scope.SelectedIndex == 1;
        var values = crop ? comparison.Crop : comparison.Full;
        for (int method = 0; method < 2; method++)
        {
            string[] cells = AiText.Row(method, values[method]);
            for (int column = 0; column < cells.Length; column++)
            {
                var cell = scores.Rows[method].Cells[column];
                cell.Value = cells[column];
                cell.Style.ForeColor = ColorTranslator.FromHtml(method == 1 ? "#8857bd" : "#087e8b");
            }
        }
        scores.ClearSelection();
        summary.Text = AiText.Summary(comparison, scene, crop);
    }

    /// <summary>
    /// GeoKernel.NET 1.5.40 wraps no overlay point, so the probe is an owned point shape; owned shapes also dim
    /// everything outside the shared comparison crop (four translucent bands) and outline it.
    /// </summary>
    void DrawShapes()
    {
        if (scene == null) return;
        var (x0, y0, x1, y1) = Ai.CropExtent(scene);
        var (ox0, oy0, ox1, oy1) = scene.Dsm.Extent;
        double margin = Math.Max(ox1 - ox0, oy1 - oy0);
        (ox0, oy0, ox1, oy1) = (ox0 - margin, oy0 - margin, ox1 + margin, oy1 + margin);
        (double, double, double, double)[] bands = [(ox0, y1, ox1, oy1), (ox0, oy0, ox1, y0), (ox0, y0, x0, y1), (x1, y0, ox1, y1)];
        var dim = new GeoKernelLayerStyle { FillColor = "#ffffff", FillOpacity = 155, LineWidth = 0, LineColor = "#ffffff" };
        foreach (var map in maps)
        {
            map.ClearShapes();
            if (scope.SelectedIndex == 1)
            {
                foreach (var (a, b, c, d) in bands)
                    map.AddPolygonShape([new(a, b), new(c, b), new(c, d), new(a, d), new(a, b)], dim);
                map.AddPolylineShape([new(x0, y0), new(x1, y0), new(x1, y1), new(x0, y1), new(x0, y0)], new GeoKernelLayerStyle { LineColor = "#098b97", LineWidth = 2 });
            }
            if (probePoint is { } p)
                map.AddPointShape(new GeoKernelPoint(p.X, p.Y), new GeoKernelLayerStyle { PointColor = "#d73573", PointSize = 11, LineColor = "#ffffff", LineWidth = 2 });
            map.RefreshLayers(); // owned shapes are drawn on the next full map render
        }
    }

    void Inspect(double x, double y)
    {
        if (scene == null || busy) return;
        if (AiText.Probe(scene, comparison, x, y) is not { } hit) return;
        probe.Text = hit.Text;
        probePoint = (hit.X, hit.Y);
        DrawShapes();
    }

    // ----- shutdown ------------------------------------------------------------------------
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // The SDK inference call has no cancellation hook; a queued close completes after the current AI pass.
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
