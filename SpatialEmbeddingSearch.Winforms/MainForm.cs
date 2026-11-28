using System.Text.Json.Nodes;
using GeoKernel.Examples.EmbeddingSearch;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.EmbeddingSearch.Winforms;

/// <summary>
/// Places that look alike, but only in one CRS, one area and one time span: a seed cell's AlphaEarth
/// embedding is searched in a SpatialEmbeddingIndex of the Netherlands (3 years × 2 UTM zones) with CRS,
/// extent and time filters, and the panel shows how many records each filter rejected.
/// </summary>
internal sealed class MainForm : Form
{
    const string Title = "Spatial embedding search";

    readonly bool smoke;  // --smoke: run three searches that each show one filter, print, close
    readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    readonly EmbeddingSearchSession session = new();
    readonly CancellationTokenSource cancellation = new();
    readonly ComboBox place = Combo(), seedYear = Combo(), crs = Combo(), area = Combo(), fromYear = Combo(), toYear = Combo();
    readonly NumericUpDown maximum = new() { Minimum = 1, Maximum = 200, Value = 20, Dock = DockStyle.Fill };
    readonly Button search = new() { Text = "Search", Dock = DockStyle.Fill, Height = 30 };
    readonly Label stats = new() { Dock = DockStyle.Fill, AutoSize = true, MaximumSize = new Size(400, 0), Text = "Search to see how many records each filter rejected." };
    readonly ListBox matches = new() { Dock = DockStyle.Fill };
    readonly ProgressBar progress = new() { Dock = DockStyle.Fill };
    readonly Label status = new() { Dock = DockStyle.Fill, AutoSize = true, MaximumSize = new Size(400, 0), Text = "Preparing the Netherlands AlphaEarth embeddings (14 MB on the first run)…" };

    static ComboBox Combo() => new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };

    public MainForm(bool smoke = false)
    {
        this.smoke = smoke;
        Text = Title + " — Windows Forms";
        ExampleIcon.Apply(this);
        ClientSize = new Size(1460, 920);
        StartPosition = FormStartPosition.CenterScreen;

        var form = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, AutoSize = true };
        form.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        form.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        void Row(string label, Control control)
        {
            form.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left });
            form.Controls.Add(control);
        }
        Row("Look for places like", place);
        Row("Seed year", seedYear);
        Row("Search in CRS", crs);
        Row("Extent", area);
        Row("From year", fromYear);
        Row("To year", toYear);
        Row("Matches", maximum);
        form.Controls.Add(search);
        form.SetColumnSpan(search, 2);
        var query = new GroupBox { Text = "Query", Dock = DockStyle.Fill, AutoSize = true };
        query.Controls.Add(form);
        var filters = new GroupBox { Text = "Filters", Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(8) };
        filters.Controls.Add(stats);

        var side = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(8) };
        side.Controls.Add(query);
        side.Controls.Add(filters);
        side.Controls.Add(new Label { Text = "Matches (click to zoom):", AutoSize = true });
        side.Controls.Add(matches);
        side.Controls.Add(progress);
        side.Controls.Add(status);
        side.Controls.Add(new Label { Text = EmbeddingSearchSession.Attribution, AutoSize = true, MaximumSize = new Size(400, 0), ForeColor = Color.DimGray });
        for (var i = 0; i < side.Controls.Count; i++)
            side.RowStyles.Add(side.Controls[i] == matches ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.AutoSize));

        var split = new SplitContainer { Dock = DockStyle.Fill, FixedPanel = FixedPanel.Panel1 };
        split.Panel1.Controls.Add(side);
        split.Panel2.Controls.Add(viewer);
        Controls.Add(split);
        Load += (_, _) => split.SplitterDistance = 430;
        SetControlsEnabled(false);

        place.SelectedIndexChanged += (_, _) => FillSeedYears();
        crs.SelectedIndexChanged += (_, _) => FillExtents();
        search.Click += (_, _) => Search(Current());
        matches.SelectedIndexChanged += (_, _) => { if (session.MatchView(matches.SelectedIndex) is { } view) viewer.ViewExtent = view; };
        Shown += async (_, _) => await PrepareAsync();
        FormClosing += (_, _) => cancellation.Cancel();
        FormClosed += (_, _) => session.Dispose();
    }

    void SetControlsEnabled(bool enabled)
    {
        foreach (Control control in new Control[] { place, seedYear, crs, area, fromYear, toYear, maximum, search })
            control.Enabled = enabled;
    }

    async Task PrepareAsync()
    {
        var report = new Progress<DownloadProgress>(p => { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; });
        try
        {
            await session.PrepareAsync(report, cancellation.Token);
            session.AttachBackgrounds(viewer);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            status.Text = e.Message;
            if (smoke)
            {
                Console.WriteLine("smoke | error: " + e.Message);
                Environment.ExitCode = 2;
                Close();
            }
            return;
        }
        foreach (var p in session.Places)
            place.Items.Add(new Item($"{p["name"]} — {p["note"]}", p));
        foreach (var (value, label) in EmbeddingSearchSession.CrsChoices)
            crs.Items.Add(new Item(label, value));
        foreach (var year in EmbeddingSearchSession.Years)
        {
            fromYear.Items.Add(year);
            toYear.Items.Add(year);
        }
        fromYear.SelectedIndex = toYear.SelectedIndex = 2;
        place.SelectedIndex = 0;
        crs.SelectedIndex = 0;
        SetControlsEnabled(true);
        progress.Value = 100;
        status.Text = $"Ready: {session.Index!.Count:N0} records (3 years × 2 UTM zones). Pick a place, a CRS, an extent and years, then Search.";
        if (smoke)
            BeginInvoke(RunChecks);
    }

    sealed record Item(string Label, object Value)
    {
        public override string ToString() => Label;
    }

    void FillSeedYears()
    {
        seedYear.Items.Clear();
        if (place.SelectedItem is Item { Value: JsonObject p })
            foreach (var year in p["years"]!.AsArray())
                seedYear.Items.Add((int)year!);
        seedYear.SelectedIndex = seedYear.Items.Count - 1;
    }

    void FillExtents()
    {
        area.Items.Clear();
        if (crs.SelectedItem is Item { Value: string value })
            foreach (var e in session.Extents(value))
                area.Items.Add(new Item((string)e["name"]!, e));
        if (area.Items.Count > 0)
            area.SelectedIndex = 0;
    }

    SearchRequest Current()
    {
        int first = (int)fromYear.SelectedItem!, last = (int)toYear.SelectedItem!;
        return new(((Item)place.SelectedItem!).Value as JsonObject ?? [], (int)seedYear.SelectedItem!, (string)((Item)crs.SelectedItem!).Value,
                   (JsonObject)((Item)area.SelectedItem!).Value, Math.Min(first, last), Math.Max(first, last), (int)maximum.Value);
    }

    bool Search(SearchRequest request)
    {
        var ok = session.Run(viewer, request);
        stats.Text = session.Describe(request);
        matches.Items.Clear();
        foreach (var line in session.MatchLines())
            matches.Items.Add(line);
        status.Text = $"Seed: {request.Place["name"]}, {request.SeedYear} (cell {request.Place["column"]}, {request.Place["row"]} in {request.Place["crs"]}).";
        return ok;
    }

    void RunChecks()
    {
        var all = true;
        foreach (var (label, request) in session.Checks())
        {
            var ok = Search(request);
            all &= ok;
            Console.WriteLine(session.CheckLine(label, ok));
        }
        Environment.ExitCode = all ? 0 : 2;
        BeginInvoke(Close);
    }
}
