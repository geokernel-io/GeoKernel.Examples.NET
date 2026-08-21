using System.Text;
using System.Text.Json;
using GeoKernel.Examples.Common;
using GeoKernel.NET.WinForms;

namespace GeoKernel.AnalysisGeoParquetFilter.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    private readonly ComboBox classBox = new() { DropDownStyle = ComboBoxStyle.DropDownList, Dock = DockStyle.Fill };
    private readonly NumericUpDown limitBox = new() { Minimum = 1, Maximum = 100000, Value = 25000, Dock = DockStyle.Fill };
    private readonly Button runButton = new() { Text = "Run automatic analysis", Dock = DockStyle.Fill };
    private readonly Button cancelButton = new() { Text = "Cancel", Enabled = false, Dock = DockStyle.Fill };
    private readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100, Dock = DockStyle.Fill };
    private readonly Label stage = new() { Text = "Ready.", AutoSize = true };
    private readonly TextBox diagnostics = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill };
    private readonly ToolStripStatusLabel status = new();
    private readonly System.Windows.Forms.Timer poll = new() { Interval = 40 };
    private readonly GeoKernelAnalysis analysis = new();
    private AnalysisJob? job;
    private AnalysisLayer? layer;
    private string parquetPath = string.Empty;
    private bool closing;

    public MainForm()
    {
        Text = "AnalysisGeoParquetFilter"; Width = 1220; Height = 790;
        Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "resources", "GeoKernelAppIcon.ico"));
        classBox.Items.AddRange(["apartments", "house", "commercial", "industrial"]); classBox.SelectedIndex = 0;

        var right = new TableLayoutPanel { Dock = DockStyle.Right, Width = 340, Padding = new Padding(10), ColumnCount = 2, RowCount = 9 };
        right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50)); right.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (var i = 0; i < 8; i++) right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        var title = new Label { Text = "Backend-neutral analysis", AutoSize = true, Font = new Font(Font, FontStyle.Bold) };
        right.Controls.Add(title, 0, 0); right.SetColumnSpan(title, 2);
        right.Controls.Add(new Label { Text = "Building class", AutoSize = true }, 0, 1); right.Controls.Add(classBox, 1, 1);
        right.Controls.Add(new Label { Text = "Maximum results", AutoSize = true }, 0, 2); right.Controls.Add(limitBox, 1, 2);
        right.Controls.Add(new Label { Text = "BBOX", AutoSize = true }, 0, 3); right.Controls.Add(new Label { Text = "18.04, 59.30, 18.10, 59.35", AutoSize = true }, 1, 3);
        right.Controls.Add(runButton, 0, 4); right.Controls.Add(cancelButton, 1, 4);
        right.Controls.Add(progress, 0, 5); right.SetColumnSpan(progress, 2);
        right.Controls.Add(stage, 0, 6); right.SetColumnSpan(stage, 2);
        right.Controls.Add(diagnostics, 0, 8); right.SetColumnSpan(diagnostics, 2);

        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        tools.Items.Add("Zoom In", null, (_, _) => viewer.ZoomIn()); tools.Items.Add("Zoom Out", null, (_, _) => viewer.ZoomOut());
        tools.Items.Add("Full Extent", null, (_, _) => viewer.FullExtent()); tools.Items.Add("Pan", null, (_, _) => viewer.ActiveTool = GeoKernelViewerTool.Pan);
        var statusBar = new StatusStrip(); statusBar.Items.Add(status);
        Controls.Add(viewer); Controls.Add(right); Controls.Add(tools); Controls.Add(statusBar);
        viewer.ActiveTool = GeoKernelViewerTool.Pan; viewer.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        runButton.Enabled = false; status.Text = "Loading sample data...";
        runButton.Click += (_, _) => BeginAnalysis(); cancelButton.Click += (_, _) => job?.Cancel(); poll.Tick += (_, _) => PollJob();
        Shown += (_, _) => PrepareData(); FormClosing += (_, _) => { closing = true; if (job is { IsFinished: false }) job.Cancel(); };
    }

    private void PrepareData()
    {
        try
        {
            parquetPath = SampleData.EnsureSampleFile(new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/stockholm_data.zip"), "stockholm_data.zip", ".", Path.Combine("stockholm_data", "stockholm_buildings.parquet"), this);
            if (string.IsNullOrWhiteSpace(parquetPath)) { stage.Text = "Sample data is unavailable."; return; }
            runButton.Enabled = true; BeginAnalysis();
        }
        catch (Exception ex) { status.Text = "Sample data could not be loaded."; MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    private void BeginAnalysis()
    {
        if (string.IsNullOrWhiteSpace(parquetPath) || job is { IsFinished: false }) return;
        job?.Dispose(); layer?.Dispose(); layer = null;
        var request = new AnalysisRequest
        {
            Operation = AnalysisOperation.SpatialFilter, Backend = AnalysisBackend.Auto, InputKind = AnalysisDataKind.GeoParquet,
            Source = parquetPath, HasAttributeFilter = true, HasSpatialFilter = true, ProjectionRequired = true,
            Options = new Dictionary<string, object?>
            {
                ["columns"] = new[] { "id", "class", "geometry" }, ["predicateSql"] = "class = ?",
                ["predicateParameters"] = new[] { classBox.Text }, ["extent"] = new[] { 18.04, 59.30, 18.10, 59.35 }, ["limit"] = (long)limitBox.Value
            }
        };
        runButton.Enabled = false; cancelButton.Enabled = true; progress.Value = 0; diagnostics.Clear(); stage.Text = "Queuing analysis..."; status.Text = "Analysis queued...";
        job = analysis.ExecuteAsync(request); poll.Start();
    }

    private void PollJob()
    {
        if (closing || job is null) return;
        var state = job.Progress;
        if (state.TryGetProperty("percent", out var percent)) progress.Value = Math.Clamp(percent.GetInt32(), 0, 100);
        stage.Text = ProgressText(state); status.Text = state.TryGetProperty("message", out var message) ? message.GetString() ?? "" : "";
        if (!job.IsFinished) return;
        poll.Stop(); runButton.Enabled = true; cancelButton.Enabled = false;
        using var result = job.Wait(); var value = result.Value;
        if (value.GetProperty("cancelled").GetBoolean()) { stage.Text = "Analysis cancelled."; return; }
        diagnostics.Text = AttemptsText(value);
        if (!value.GetProperty("succeeded").GetBoolean()) { stage.Text = value.GetProperty("message").GetString() ?? "Analysis failed."; return; }
        layer = result.Materialize(new { name = $"Filtered {classBox.Text} buildings", skipInvalidGeometries = true });
        viewer.ClearLayers(); layer.AddTo(viewer);
        viewer.SetLayerStyle(viewer.LayerCount - 1, new GeoKernelLayerStyle { FillColor = "#55B7E9", LineColor = "#116A9B", LineWidth = 0.8 });
        viewer.FullExtent(); progress.Value = 100;
        var materialized = layer.Diagnostics;
        diagnostics.AppendText($"\r\n\r\nMATERIALIZATION\r\nSource rows: {GetNumber(materialized, "sourceRowCount")}\r\nLayer features: {GetNumber(materialized, "materializedCount")}\r\nSkipped: {GetNumber(materialized, "skippedCount")}");
        stage.Text = $"{GetNumber(materialized, "materializedCount")} selected and displayed with {value.GetProperty("backend").GetString()}."; status.Text = "Analysis completed successfully.";
    }

    private static string ProgressText(JsonElement value) => $"{(value.TryGetProperty("stage", out var s) ? s.GetString() : "Running")} — {(value.TryGetProperty("message", out var m) ? m.GetString() : "")}";
    private static long GetNumber(JsonElement value, string name) => value.TryGetProperty(name, out var item) ? item.GetInt64() : 0;
    private static string AttemptsText(JsonElement value)
    {
        var plan = value.GetProperty("plan"); var text = new StringBuilder();
        text.AppendLine("ANALYSIS PLAN").AppendLine("Requested backend: Auto").AppendLine($"Selected backend: {value.GetProperty("backend").GetString()}")
            .AppendLine($"Predicate pushdown: {(plan.GetProperty("usesPredicatePushdown").GetBoolean() ? "yes" : "no")}")
            .AppendLine($"Projection pushdown: {(plan.GetProperty("usesProjectionPushdown").GetBoolean() ? "yes" : "no")}").AppendLine().AppendLine(plan.GetProperty("explanation").GetString()).AppendLine().AppendLine("EXECUTION ATTEMPTS");
        foreach (var attempt in value.GetProperty("attempts").EnumerateArray()) text.AppendLine($"{attempt.GetProperty("backend").GetString()}: {(attempt.GetProperty("succeeded").GetBoolean() ? "success" : "failed")} ({attempt.GetProperty("elapsedMilliseconds").GetInt64()} ms) — {attempt.GetProperty("message").GetString()}");
        return text.ToString();
    }
}
