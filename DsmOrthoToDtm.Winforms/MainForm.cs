using System.Text.Json.Nodes;
using GeoKernel.Examples.DsmToDtm;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.DsmOrthoToDtm.Winforms;

/// <summary>
/// DSM + orthophoto to DTM: three independent terrain estimates (classical, DSM-only pix_grad and DSM+RGB
/// rgb_grad_fm) from the same full DSM on four linked native 2D maps, with the input context on the left.
/// The reference is evaluation-only; the methods do not feed each other.
/// </summary>
internal sealed class MainForm : Form
{
    const string Title = "DSM + Orthophoto to DTM | Three independent methods — GeoKernel";
    static readonly Color Page = ColorTranslator.FromHtml("#f2f6f8"), Ink = ColorTranslator.FromHtml("#233e50");

    readonly OrthoOptions options;
    readonly SampleAssets sampleAssets = Ortho.SampleAssets();
    readonly string[] archiveSha256 = Ortho.ArchiveSha256();
    readonly string temporary = Directory.CreateTempSubdirectory("geokernel-dsm-ortho-").FullName;
    readonly GeoKernelViewerControl[] maps = [new(), new(), new(), new()];
    readonly Label[] headings = new Label[4];
    readonly NumericUpDown overlap = new() { Minimum = 0, Maximum = 128, Increment = 32, Value = 64, Width = 110 },
        threads = new() { Minimum = 1, Maximum = 8, Value = 4, Width = 110 };
    readonly ComboBox preset = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 },
        input = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 272 },
        mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 272 },
        scope = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 272 };
    readonly Label source = MakeLabel("Preparing sample...", true), modelInfo = MakeLabel("Models not prepared.", true),
        state = MakeLabel("Load data and models, then Run."), probe = MakeLabel("Click a source pixel to inspect RGB and all terrain estimates.", true),
        legend = new() { AutoSize = true, Margin = new Padding(0, 6, 0, 4), Text = "Inputs ready first; Run generates all three terrain estimates." },
        summary = new() { AutoSize = true, Margin = new Padding(0, 4, 0, 2), Text = "No prepared prediction is loaded." }, status = new() { AutoSize = true };
    readonly DataGridView scores = new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false, AllowUserToResizeRows = false,
        RowHeadersVisible = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, BackgroundColor = Color.White, BorderStyle = BorderStyle.FixedSingle,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
    };
    readonly Button sampleButton = MakeButton("Load Zurich DSM + orthophoto"), openButton = MakeButton("Open aligned DSM + RGB..."),
        referenceButton = MakeButton("Open reference (optional)..."), modelsButton = MakeButton("Download / verify both models"),
        pixButton = MakeButton("Use pix_grad model ZIP..."), rgbButton = MakeButton("Use RGB model ZIP..."),
        runButton = MakeButton("Run all three methods"), cancelButton = MakeButton("Cancel"), saveButton = MakeButton("Save comparison folder...");
    readonly List<ToolStripItem> navigation = [];
    readonly ProgressBar progress = new() { Width = 230, Height = 16, Visible = false };
    readonly (double X, double Y)?[] presses = new (double X, double Y)?[4];
    readonly GeoKernelExtent?[] applied = new GeoKernelExtent?[4];
    CancellationTokenSource cts = new();
    OrthoScene? scene;
    OrthoComparison? comparison;
    string[]? models;
    (double X, double Y)? probePoint;
    GeoKernelViewerTool tool = GeoKernelViewerTool.Pan;
    bool busy, closeQueued, closed, syncing, setupModels;
    int generation;

    static Label MakeLabel(string text, bool card = false)
    {
        var label = new Label { Text = text, AutoSize = true, MaximumSize = new Size(272, 0), Margin = new Padding(0, 0, 0, 8) };
        if (card) { label.BackColor = Color.White; label.Padding = new Padding(8); label.BorderStyle = BorderStyle.FixedSingle; label.MinimumSize = new Size(272, 0); }
        return label;
    }

    static Button MakeButton(string text) => new() { Text = text, Width = 272, Height = 32, Margin = new Padding(0, 0, 0, 7) };

    public MainForm(OrthoOptions options)
    {
        this.options = options;
        Text = Title;
        ClientSize = new Size(1920, 980);
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
        input.Items.AddRange(OrthoText.Inputs);
        input.SelectedIndex = 1;
        mode.Items.AddRange(OrthoText.Modes);
        mode.SelectedIndex = 0;
        scope.Items.AddRange(OrthoText.Scopes);
        scope.SelectedIndex = 1;
        var title = MakeLabel("DSM + RGB → DTM\nCompare three methods");
        title.Font = new Font("Segoe UI", 15, FontStyle.Bold);
        title.ForeColor = ColorTranslator.FromHtml("#145e72");
        var form = new TableLayoutPanel { ColumnCount = 2, AutoSize = true, Margin = new Padding(0, 0, 0, 8) };
        (string, Control)[] rows = [("AI overlap (px)", overlap), ("CPU threads", threads), ("Classical preset", preset)];
        for (int i = 0; i < rows.Length; i++)
        {
            form.Controls.Add(new Label { Text = rows[i].Item1, AutoSize = true, Margin = new Padding(0, 6, 8, 0) }, 0, i);
            form.Controls.Add(rows[i].Item2, 1, i);
        }
        sidebar.Controls.AddRange([
            title, MakeLabel("Classical · DSM-only AI · DSM+RGB AI.\nEach starts from the same full DSM."), sampleButton, openButton, referenceButton, source,
            modelsButton, pixButton, rgbButton, modelInfo, form, runButton, cancelButton, state, MakeLabel("Input context"), input,
            MakeLabel("Compare estimates"), mode, scope, saveButton, probe, MakeLabel(OrthoText.Attribution),
        ]);

        var views = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 4, RowCount = 2 };
        for (int i = 0; i < 4; i++) views.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25f));
        views.RowStyles.Add(new RowStyle(SizeType.Absolute, 36));
        views.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (int i = 0; i < 4; i++)
        {
            int side = i;
            headings[i] = new Label
            {
                Text = OrthoText.Titles[i], Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(8, 0, 0, 0),
                BackColor = ColorTranslator.FromHtml("#e4eef2"), ForeColor = ColorTranslator.FromHtml("#125a70"), Font = new Font(Font, FontStyle.Bold),
            };
            maps[i].Dock = DockStyle.Fill;
            maps[i].MinimumSize = new Size(230, 260);
            maps[i].VisibleExtentChanged += (_, _) => ExtentChanged(side);
            maps[i].MapMouseDown += (_, e) => { if (e.ButtonOrButtons == 1) presses[side] = (e.ScreenPoint.X, e.ScreenPoint.Y); };
            maps[i].MapMouseUp += (_, e) => MapClick(side, e);
            views.Controls.Add(headings[i], i, 0);
            views.Controls.Add(maps[i], i, 1);
        }

        foreach (string column in AiText.Columns) scores.Columns.Add(column.Replace(" ", ""), column);
        scores.Rows.Add(3);
        var workspace = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(0, 12, 12, 6) };
        workspace.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.RowStyles.Add(new RowStyle(SizeType.Absolute, 116));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        workspace.Controls.Add(views, 0, 0);
        workspace.Controls.Add(legend, 0, 1);
        workspace.Controls.Add(scores, 0, 2);
        workspace.Controls.Add(summary, 0, 3);
        workspace.Controls.Add(new Label { Text = OrthoText.Footer, AutoSize = true, Margin = new Padding(0, 2, 0, 0) }, 0, 4);

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

        void Changed() { if (comparison != null) state.Text = OrthoText.ControlsChanged; }
        overlap.ValueChanged += (_, _) => Changed();
        threads.ValueChanged += (_, _) => Changed();
        preset.SelectedIndexChanged += (_, _) => Changed();
        input.SelectedIndexChanged += (_, _) => ShowInput();
        mode.SelectedIndexChanged += (_, _) => Display();
        scope.SelectedIndexChanged += (_, _) => { UpdateScores(); Fit(); };
        sampleButton.Click += (_, _) => LoadSample();
        openButton.Click += (_, _) => OpenInputs();
        referenceButton.Click += (_, _) => OpenReference();
        modelsButton.Click += (_, _) => PrepareModels(-1, null);
        pixButton.Click += (_, _) => ChooseModel(0);
        rgbButton.Click += (_, _) => ChooseModel(1);
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
        foreach (Control c in (Control[])[sampleButton, openButton, modelsButton, pixButton, rgbButton, overlap, threads, preset, input, scope]) c.Enabled = !value;
        foreach (var item in navigation) item.Enabled = !value && scene != null;
        runButton.Enabled = !value && scene != null && models != null;
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
            else if (setupModels && scene != null && models == null) { setupModels = false; BeginInvoke(() => PrepareModels(-1, null)); }
        }
        catch (Exception e)
        {
            if (closed) return;
            SetBusy(false);
            bool cancelled = e is OperationCanceledException || token.IsCancellationRequested;
            state.Text = status.Text = cancelled ? "Cancelled; last completed comparison retained." : e.Message;
            if (closeQueued) BeginInvoke(Close);
            else if (!cancelled) MessageBox.Show(this, e.Message, "Unable to complete operation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    // ----- jobs ----------------------------------------------------------------------------
    void ChooseModel(int index)
    {
        if (busy) return;
        using var dialog = new OpenFileDialog { Title = $"Choose the pinned {Ortho.Methods[index + 1]} model ZIP", Filter = "ZIP (*.zip)|*.zip" };
        if (dialog.ShowDialog(this) == DialogResult.OK) PrepareModels(index, dialog.FileName);
    }

    void PrepareModels(int selected, string? local)
    {
        if (busy) return;
        RunJob((token, report) =>
        {
            try { return new[] { Ortho.PrepareModel(options, 0, selected == 0 ? local : null, token, report), Ortho.PrepareModel(options, 1, selected == 1 ? local : null, token, report) }; }
            catch (Exception e) when (e is not OperationCanceledException) { throw new InvalidOperationException(e.Message + "\nUse the pinned model ZIPs or retry the published URLs.", e); }
        }, folders =>
        {
            models = folders;
            modelInfo.Text = OrthoText.ModelsReady;
            state.Text = status.Text = scene != null ? "Inputs and both models ready. Press Run." : "Models ready; load aligned DSM and RGB.";
        });
    }

    OrthoScene LoadScene(string dsmPath, string rgbPath, string? referencePath, JsonObject manifest, string folder, CancellationToken token, Action<int, string> report)
    {
        report(5, "Reading source DSM...");
        var dsm = Terrain.Read(dsmPath, token);
        report(25, "Checking RGB channels, grid and coverage...");
        var rgb = Ortho.ReadOrtho(dsm, rgbPath, token);
        TerrainRaster? reference = null;
        if (referencePath != null)
        {
            report(50, "Reading evaluation-only reference...");
            reference = Terrain.Read(referencePath, token);
            Terrain.ValidateReference(dsm, reference);
        }
        if ((bool?)manifest["standard_full_tile"] == true)
        {
            var (x0, y0, x1, y1) = dsm.Extent;
            if (!Gdal.SameCrs(dsm.Projection, 2056) || dsm.Width != 2000 || dsm.Height != 2000 || dsm.Pixel != .5 || (x0, y0, x1, y1) != (2677000, 1251000, 2678000, 1252000))
                throw new InvalidDataException("Unexpected Zurich sample grid.");
        }
        report(70, "Preparing original DSM relief...");
        var (r, g, b) = Terrain.Relief(dsm, dsm.Z, token);
        string original = Gdal.WriteRgb(Path.Combine(folder, "original.tif"), dsm.Width, dsm.Height, dsm.Transform, dsm.Projection, r, g, b);
        token.ThrowIfCancellationRequested();
        return new OrthoScene(dsm, rgb, reference, original, manifest);
    }

    void LoadSample()
    {
        if (busy) return;
        string folder = NextFolder();
        setupModels = models == null;
        RunJob((token, report) =>
        {
            string sample = SampleCache.PrepareAsync(sampleAssets, options.Sample, new Progress<(int Percent, string Message)>(p => report(p.Percent, p.Message)), token)
                .GetAwaiter().GetResult();
            token.ThrowIfCancellationRequested();
            Gdal.ConfigureRuntime(sample);
            var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(sample, "source-manifest.json")))!.AsObject();
            return LoadScene(Path.Combine(sample, "dsm.tif"), Path.Combine(sample, "ortho_rgb.tif"), Path.Combine(sample, "reference.tif"), manifest, folder, token, report);
        }, loaded => Publish(loaded, null, "Aligned inputs ready. Prepare both models and Run."));
    }

    void OpenInputs()
    {
        if (busy) return;
        using var dsmDialog = new OpenFileDialog { Title = "Open metre-height DSM", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff" };
        if (dsmDialog.ShowDialog(this) != DialogResult.OK) return;
        using var rgbDialog = new OpenFileDialog { Title = "Open exactly aligned RGB Byte orthophoto", Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff", InitialDirectory = Path.GetDirectoryName(dsmDialog.FileName) };
        if (rgbDialog.ShowDialog(this) != DialogResult.OK) return;
        string dsm = dsmDialog.FileName, rgb = rgbDialog.FileName, folder = NextFolder();
        RunJob((token, report) => LoadScene(dsm, rgb, null, new JsonObject(), folder, token, report),
            loaded => Publish(loaded, null, "Aligned inputs ready. Prepare both models and Run."));
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
            var updated = completed == null ? null : Ortho.AttachReference(next, completed, folder, archiveSha256, token, report);
            token.ThrowIfCancellationRequested();
            return (next, updated);
        }, result => Publish(result.next, result.updated, "Reference updated; all generated height arrays are unchanged."));
    }

    void Process()
    {
        if (busy || scene is not { } current || models is not { } packages) return;
        var parameters = Ai.EffectiveParameters(current.Dsm, Terrain.Presets[preset.SelectedIndex]);
        int tileOverlap = (int)overlap.Value, cpu = (int)threads.Value;
        string folder = NextFolder();
        RunJob((token, report) => Ortho.Compare(current, packages, parameters, tileOverlap, cpu, folder, archiveSha256, token, report),
            result => Publish(current, result, "All three methods completed from the same full source grid."));
    }

    void ExportResults()
    {
        if (busy || comparison is not { } completed) return;
        using var dialog = new FolderBrowserDialog { Description = "Choose parent folder for a new complete result bundle", UseDescriptionForTitle = true };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        string parent = dialog.SelectedPath;
        RunJob((token, report) => Ortho.Export(completed, parent, token, report), target => { state.Text = status.Text = "Saved complete comparison bundle: " + target; });
    }

    void Publish(OrthoScene loaded, OrthoComparison? result, string message)
    {
        bool changed = scene == null || !ReferenceEquals(loaded.Dsm, scene.Dsm);
        scene = loaded;
        comparison = result;
        if (changed)
        {
            ShowInput();
            for (int pane = 1; pane < 4; pane++) { maps[pane].ClearLayers(); maps[pane].RefreshLayers(); }
            probePoint = null;
            probe.Text = "Click a source pixel to inspect RGB and all terrain estimates.";
            Array.Clear(applied);
            BeginInvoke(Fit);
        }
        source.Text = OrthoText.Source(loaded);
        if (comparison != null)
        {
            Display();
            UpdateScores();
        }
        else
        {
            foreach (DataGridViewRow row in scores.Rows) foreach (DataGridViewCell cell in row.Cells) cell.Value = "";
            summary.Text = "Aligned inputs loaded. Run to create all three terrain estimates.";
            for (int pane = 1; pane < 4; pane++) headings[pane].Text = OrthoText.Pending[pane - 1];
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

    void ShowInput()
    {
        if (scene == null) return;
        bool rgb = input.SelectedIndex == 1;
        Replace(0, rgb ? scene.Rgb.Path : scene.Original);
        headings[0].Text = rgb ? "RGB ORTHOPHOTO · source input" : "ORIGINAL DSM · source input";
        DrawShapes();
    }

    void Fit()
    {
        if (scene == null) return;
        bool crop = scope.SelectedIndex == 1;
        var (x0, y0, x1, y1) = crop ? Ai.CropExtent(scene.Terrain) : scene.Dsm.Extent;
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
            for (int other = 0; other < 4; other++)
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
            status.Text = "Attach an aligned reference to show errors.";
            return;
        }
        for (int method = 0; method < 3; method++)
        {
            Replace(method + 1, comparison.Maps[method][selected]);
            headings[method + 1].Text = $"{Ortho.Methods[method].ToUpperInvariant()} {OrthoText.Suffix[selected]}";
        }
        legend.Text = OrthoText.Legends[selected];
        DrawShapes();
    }

    void UpdateScores()
    {
        if (comparison == null || scene == null) return;
        bool crop = scope.SelectedIndex == 1;
        var values = crop ? comparison.Crop : comparison.Full;
        for (int method = 0; method < 3; method++)
        {
            string[] cells = OrthoText.Row(method, values[method]);
            for (int column = 0; column < cells.Length; column++)
            {
                var cell = scores.Rows[method].Cells[column];
                cell.Value = cells[column];
                cell.Style.ForeColor = ColorTranslator.FromHtml(OrthoText.Colors[method]);
            }
        }
        scores.ClearSelection();
        summary.Text = OrthoText.Summary(comparison, scene, crop);
    }

    /// <summary>
    /// GeoKernel.NET 1.5.40 wraps no overlay point, so the probe is an owned point shape; owned shapes also dim
    /// everything outside the shared comparison crop (four translucent bands) and outline it.
    /// </summary>
    void DrawShapes()
    {
        if (scene == null) return;
        var (x0, y0, x1, y1) = Ai.CropExtent(scene.Terrain);
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
        if (OrthoText.Probe(scene, comparison, x, y) is not { } hit) return;
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
