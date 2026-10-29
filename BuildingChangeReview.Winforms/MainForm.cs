using System.Drawing;
using System.Text.Json.Nodes;
using GeoKernel.Examples.BuildingChange;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.BuildingChangeReview.Winforms;

public sealed class MainForm : Form
{
    readonly ChangeSession session = new();
    readonly ComboBox area = Choice("Demo area (fixed)", "Current view", "Full dataset");
    readonly ComboBox filter = Choice("Conservative", "Balanced");
    readonly ComboBox provider = Choice("CPU", "CUDA");
    readonly NumericUpDown threshold = new() { Minimum = .05m, Maximum = .95m, DecimalPlaces = 4, Increment = .025m, Value = .4371m };
    readonly Button run = new() { Text = "Run analysis" }, cancel = new() { Text = "Cancel" }, save = new() { Text = "Export GeoJSON" }, locate = new() { Text = "Locate" };
    readonly Label summary = new() { Text = "Preparing imagery…", AutoSize = true };
    readonly ToolStripStatusLabel status = new() { Spring = true, TextAlign = ContentAlignment.MiddleLeft };
    readonly ProgressBar progress = new() { Maximum = 100 };
    readonly TextBox actor = new() { Text = "demo-reviewer", MaxLength = 100 }, note = new() { MaxLength = 512 };
    readonly DataGridView table = new()
    {
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AllowUserToResizeRows = false,
        RowHeadersVisible = false,
        MultiSelect = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoGenerateColumns = false,
        BackgroundColor = SystemColors.Window,
        BorderStyle = BorderStyle.FixedSingle,
        GridColor = Color.FromArgb(225, 228, 232),
        EnableHeadersVisualStyles = false,
        MinimumSize = new Size(0, 100),
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
    };
    readonly TableLayoutPanel side = new() { Dock = DockStyle.Fill, ColumnCount = 1, Padding = new Padding(10), AutoScroll = true, Margin = Padding.Empty };
    readonly TableLayoutPanel runRow = EqualRow(2);
    readonly List<Button> decisions = [];
    readonly System.Windows.Forms.Timer timer = new() { Interval = 100 };
    int settingsRow, progressRow, summaryRow;
    bool changing, closing, closeAllowed;

    public MainForm()
    {
        Text = "Building change review — WinForms";
        AutoScaleMode = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96, 96);
        ClientSize = new Size(1440, 820);
        MinimumSize = new Size(1060, 680);
        ExampleIcon.Apply(this);
        side.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        BuildSidebar();

        var root = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Margin = Padding.Empty, Padding = Padding.Empty };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        var body = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        body.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 370));
        body.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        body.Controls.Add(BuildMaps(), 0, 0);
        body.Controls.Add(side, 1, 0);
        var strip = new StatusStrip { SizingGrip = false, Dock = DockStyle.Fill, Margin = Padding.Empty };
        strip.Items.Add(status);
        root.Controls.Add(BuildToolbar(), 0, 0);
        root.Controls.Add(body, 0, 1);
        root.Controls.Add(strip, 0, 2);
        Controls.Add(root);
        WireEvents();
        UpdateState();
    }

    static ComboBox Choice(params string[] values)
    {
        var choice = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
        choice.Items.AddRange(values);
        choice.SelectedIndex = 0;
        return choice;
    }

    static TableLayoutPanel EqualRow(int columns)
    {
        var row = new TableLayoutPanel { ColumnCount = columns, RowCount = 1, Margin = Padding.Empty };
        row.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        for (int i = 0; i < columns; i++)
            row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / columns));
        return row;
    }

    Control BuildMaps()
    {
        // Equal percentage columns stay balanced at startup, resize and DPI changes.
        var maps = EqualRow(2);
        maps.Dock = DockStyle.Fill;
        for (int index = 0; index < 2; index++)
        {
            var pane = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, Margin = new Padding(index == 0 ? 0 : 2, 0, index == 0 ? 2 : 0, 0) };
            pane.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            pane.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
            pane.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            pane.Controls.Add(new Label { Text = index == 0 ? "2016 — BEFORE" : "2025 — AFTER", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter, Margin = Padding.Empty }, 0, 0);
            session.Views[index].Dock = DockStyle.Fill;
            session.Views[index].Margin = Padding.Empty;
            pane.Controls.Add(session.Views[index], 0, 1);
            maps.Controls.Add(pane, index, 0);
        }
        return maps;
    }

    int Add(Control control, int height)
    {
        int row = side.RowCount++;
        control.Dock = DockStyle.Fill;
        control.Margin = new Padding(0, 2, 0, 2);
        side.RowStyles.Add(height < 0 ? new RowStyle(SizeType.Percent, 100) : new RowStyle(SizeType.Absolute, height));
        side.Controls.Add(control, 0, row);
        return row;
    }

    static Control Field(string text, Control control)
    {
        var row = EqualRow(2);
        row.ColumnStyles[0] = new ColumnStyle(SizeType.Absolute, 82);
        row.ColumnStyles[1] = new ColumnStyle(SizeType.Percent, 100);
        row.Controls.Add(new Label { Text = text, AutoSize = true, Anchor = AnchorStyles.Left, Margin = Padding.Empty }, 0, 0);
        control.Dock = DockStyle.Fill;
        control.Margin = Padding.Empty;
        row.Controls.Add(control, 1, 0);
        return row;
    }

    void BuildSidebar()
    {
        Add(new Label { Text = "Oosterwold · 2016 → 2025", Font = new Font(Font, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft }, 28);
        Add(Field("Area", area), 30);
        Add(Field("Filter", filter), 30);
        var settings = new CheckBox { Text = "Settings", Appearance = Appearance.Button, TextAlign = ContentAlignment.MiddleLeft, FlatStyle = FlatStyle.System };
        Add(settings, 28);
        var advanced = new TableLayoutPanel { ColumnCount = 1, RowCount = 2, Visible = false, Margin = Padding.Empty };
        advanced.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        advanced.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        advanced.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        var execution = Field("Provider", provider);
        execution.Dock = DockStyle.Fill;
        var separation = Field("Threshold", threshold);
        separation.Dock = DockStyle.Fill;
        advanced.Controls.Add(execution, 0, 0);
        advanced.Controls.Add(separation, 0, 1);
        settingsRow = Add(advanced, 0);
        settings.CheckedChanged += (_, _) => { advanced.Visible = settings.Checked; side.RowStyles[settingsRow].Height = settings.Checked ? 64 : 0; };
        run.Dock = cancel.Dock = DockStyle.Fill;
        run.Margin = new Padding(0, 0, 3, 0);
        cancel.Margin = new Padding(3, 0, 0, 0);
        runRow.Controls.Add(run, 0, 0);
        runRow.Controls.Add(cancel, 1, 0);
        Add(runRow, 36);
        progressRow = Add(progress, 0);
        summaryRow = Add(summary, 26);
        table.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(244, 246, 248);
        table.ColumnHeadersDefaultCellStyle.Font = new Font(Font, FontStyle.Bold);
        table.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        table.RowTemplate.Height = 28;
        table.Columns.Add(new DataGridViewTextBoxColumn { Name = "Candidate", HeaderText = "Candidate", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 82 });
        table.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "Type", Width = 58 });
        table.Columns.Add(new DataGridViewTextBoxColumn { Name = "Area", HeaderText = "m²", Width = 64, DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "F1" } });
        table.Columns.Add(new DataGridViewTextBoxColumn { Name = "Review", HeaderText = "Review", Width = 82 });
        Add(table, -1);
        Add(Field("Reviewer", actor), 30);
        Add(Field("Note", note), 30);
        var review = EqualRow(3);
        foreach (var pair in new[] { ("Approve", "approved"), ("Reject", "rejected"), ("Pending", "pending") })
        {
            var button = new Button { Text = pair.Item1, Dock = DockStyle.Fill, Margin = new Padding(2, 0, 2, 0) };
            button.Click += (_, _) => Guard(() => session.Review(pair.Item2, actor.Text, note.Text));
            review.Controls.Add(button);
            decisions.Add(button);
        }
        Add(review, 36);
        var exports = EqualRow(2);
        locate.Dock = save.Dock = DockStyle.Fill;
        locate.Margin = new Padding(0, 0, 3, 0);
        save.Margin = new Padding(3, 0, 0, 0);
        exports.Controls.Add(locate, 0, 0);
        exports.Controls.Add(save, 1, 0);
        Add(exports, 36);
        var overlays = new CheckBox { Text = "Candidate overlays", Checked = true };
        overlays.CheckedChanged += (_, _) => Guard(() =>
        {
            if (overlays.Checked == session.Overlays)
                return;
            try
            {
                session.SetOverlays(overlays.Checked);
            }
            catch { overlays.Checked = session.Overlays; throw; }
        });
        Add(overlays, 26);
        var credit = new LinkLabel { Text = "Beeldmateriaal / PDOK · CC BY 4.0", TextAlign = ContentAlignment.MiddleLeft };
        credit.LinkClicked += (_, _) => MessageBox.Show(ChangeCore.Attribution + "\nhttps://www.beeldmateriaal.nl/dataroom\nhttps://creativecommons.org/licenses/by/4.0/", "Attribution");
        Add(credit, 24);
    }

    ToolStrip BuildToolbar()
    {
        var tools = new ToolStrip { Dock = DockStyle.Fill, GripStyle = ToolStripGripStyle.Hidden, Margin = Padding.Empty };
        foreach (var tool in new (string, Action)[]
        {
            ("Zoom in", () => session.Views[0].ZoomIn()), ("Zoom out", () => session.Views[0].ZoomOut()),
            ("Demo area", () => session.Focus(ChangeCore.Demo)), ("Full extent", () => session.Focus(ChangeCore.Bounds)),
            ("Pan", () => session.UseTool(GeoKernelViewerTool.Pan)),
            ("Edit vertices", () => session.UseTool(GeoKernelViewerTool.EditVertices)),
            ("Undo", () => session.Undo(false)), ("Redo", () => session.Undo(true))
        })
            tools.Items.Add(tool.Item1, null, (_, _) => Guard(tool.Item2));
        return tools;
    }

    void WireEvents()
    {
        session.Changed += UpdateState;
        run.Click += async (_, _) =>
        {
            if (!session.SourcesReady)
            {
                await (session.Operation = session.Initialize());
                return;
            }
            if (session.Result is not null && MessageBox.Show("Replace this session’s candidates and review decisions?", "Run analysis", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            await (session.Operation = session.Run(area.Text, filter.SelectedIndex == 0 ? Policy.Conservative : Policy.Balanced, (double)threshold.Value, provider.Text.ToLowerInvariant()));
        };
        cancel.Click += (_, _) => session.Cancel();
        locate.Click += (_, _) => Guard(session.Locate);
        save.Click += (_, _) => Guard(() =>
        {
            using var dialog = new SaveFileDialog { Filter = "GeoJSON (*.geojson)|*.geojson", FileName = "building_changes.geojson", OverwritePrompt = true };
            if (dialog.ShowDialog(this) == DialogResult.OK)
                session.Export(dialog.FileName);
        });
        table.CurrentCellChanged += (_, _) =>
        {
            if (changing || table.CurrentRow is null)
                return;
            Guard(() =>
            {
                int row = table.CurrentRow.Index;
                try
                {
                    session.Select(row);
                }
                catch
                {
                    changing = true;
                    try
                    {
                        if (session.Selected >= 0)
                            table.CurrentCell = table.Rows[session.Selected].Cells[0];
                    }
                    finally { changing = false; }
                    throw;
                }
                if (session.Result is not null && session.Selected >= 0)
                    note.Text = session.Result["features"]![session.Selected]!["properties"]!["review_note"]!.GetValue<string>();
            });
        };
        table.CellDoubleClick += (_, _) => Guard(session.Locate);
        timer.Tick += (_, _) => { if (!session.Busy && !closing) Guard(() => session.Capture(), false); };
        timer.Start();
        Shown += async (_, _) => await (session.Operation = session.Initialize());
        FormClosing += CloseAsync;
    }

    async void CloseAsync(object? sender, FormClosingEventArgs e)
    {
        if (closeAllowed)
            return;
        e.Cancel = true;
        if (closing)
            return;
        closing = true;
        Enabled = false;
        timer.Stop();
        await session.Close();
        session.Changed -= UpdateState;
        closeAllowed = true;
        Close();
    }

    void Guard(Action action, bool dialog = true)
    {
        try
        {
            action();
        }
        catch (Exception e)
        {
            status.Text = status.ToolTipText = e.Message;
            if (dialog)
                MessageBox.Show(e.Message, "Building change review");
        }
    }

    void UpdateState()
    {
        if (IsDisposed)
            return;
        status.Text = status.ToolTipText = session.Status;
        side.SuspendLayout();
        try
        {
            progress.Value = Math.Clamp(session.Percent, 0, 100);
            progress.Visible = session.Busy;
            side.RowStyles[progressRow].Height = session.Busy ? 24 : 0;
            run.Text = session.SourcesReady ? "Run analysis" : session.Busy ? "Preparing…" : "Retry loading";
            run.Enabled = area.Enabled = filter.Enabled = provider.Enabled = threshold.Enabled = !session.Busy;
            runRow.ColumnStyles[0].Width = session.Busy ? 70 : 100;
            runRow.ColumnStyles[1].Width = session.Busy ? 30 : 0;
            cancel.Visible = session.Busy;
            table.Enabled = !session.Busy;
            locate.Enabled = !session.Busy && session.Selected >= 0;
            foreach (var button in decisions)
                button.Enabled = !session.Busy && session.Selected >= 0;
            save.Enabled = !session.Busy && session.Result is not null && session.Result["features"]!.AsArray().Any(f => f!["properties"]!["review_status"]!.GetValue<string>() == "approved");
            if (session.Result is null)
            {
                summary.Text = session.SourcesReady ? "Run analysis to find changes." : session.Status;
                summary.ForeColor = SystemColors.GrayText;
                side.RowStyles[summaryRow].Height = 26;
                return;
            }
            var features = session.Result["features"]!.AsArray();
            summary.ForeColor = SystemColors.ControlText;
            side.RowStyles[summaryRow].Height = 44;
            summary.Text = $"{features.Count(f => f!["properties"]!["change_type"]!.GetValue<string>() == "new")} new · {features.Count(f => f!["properties"]!["change_type"]!.GetValue<string>() == "removed")} removed\n{features.Count(f => f!["properties"]!["review_status"]!.GetValue<string>() == "approved")} approved · {features.Count(f => f!["properties"]!["review_status"]!.GetValue<string>() == "pending")} pending";
            if (session.Busy)
                return;
            changing = true;
            try
            {
                while (table.Rows.Count < features.Count)
                    table.Rows.Add();
                while (table.Rows.Count > features.Count)
                    table.Rows.RemoveAt(table.Rows.Count - 1);
                for (int i = 0; i < features.Count; i++)
                {
                    var f = features[i]!;
                    object[] values = [f["id"]!.GetValue<string>(), f["properties"]!["change_type"]!.GetValue<string>(), f["properties"]!["area_m2"]!.GetValue<double>(), f["properties"]!["review_status"]!.GetValue<string>()];
                    for (int column = 0; column < values.Length; column++)
                        if (!Equals(table.Rows[i].Cells[column].Value, values[column]))
                            table.Rows[i].Cells[column].Value = values[column];
                }
                if (session.Selected >= 0 && table.CurrentRow?.Index != session.Selected)
                    table.CurrentCell = table.Rows[session.Selected].Cells[0];
            }
            finally { changing = false; }
        }
        finally { side.ResumeLayout(true); }
    }
}
