using System.Text;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AnalysisGeoParquetFilter.Wpf;

public sealed class MainWindow : Window
{
    private readonly GeoKernelViewerControl viewer = new();
    private readonly ComboBox classBox = new() { ItemsSource = new[] { "apartments", "house", "commercial", "industrial" }, SelectedIndex = 0 };
    private readonly TextBox limitBox = new() { Text = "25000" };
    private readonly Button runButton = new() { Content = "Run automatic analysis" };
    private readonly Button cancelButton = new() { Content = "Cancel", IsEnabled = false };
    private readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100, Height = 18 };
    private readonly TextBlock stage = new() { Text = "Ready.", TextWrapping = TextWrapping.Wrap };
    private readonly TextBox diagnostics = new() { IsReadOnly = true, AcceptsReturn = true, AcceptsTab = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, HorizontalScrollBarVisibility = ScrollBarVisibility.Auto };
    private readonly TextBlock status = new();
    private readonly DispatcherTimer poll = new() { Interval = TimeSpan.FromMilliseconds(40) };
    private readonly GeoKernelAnalysis analysis = new();
    private AnalysisJob? job;
    private AnalysisLayer? layer;
    private string parquetPath = string.Empty;
    private bool closing;

    public MainWindow()
    {
        Title = "AnalysisGeoParquetFilter"; Width = 1220; Height = 790;
        Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Images/GeoKernelAppIcon.ico"));
        var root = new DockPanel();
        var toolbar = new StackPanel { Orientation = Orientation.Horizontal, Height = 34, Background = Brushes.WhiteSmoke };
        foreach (var item in new (string, Action)[] { ("Zoom In", () => viewer.ZoomIn()), ("Zoom Out", () => viewer.ZoomOut()), ("Full Extent", viewer.FullExtent), ("Pan", () => viewer.ActiveTool = GeoKernelViewerTool.Pan) })
        { var button = new Button { Content = item.Item1, Margin = new Thickness(2) }; button.Click += (_, _) => item.Item2(); toolbar.Children.Add(button); }
        DockPanel.SetDock(toolbar, Dock.Top); root.Children.Add(toolbar);
        var footer = new Border { Child = status, Padding = new Thickness(4), Background = Brushes.WhiteSmoke }; DockPanel.SetDock(footer, Dock.Bottom); root.Children.Add(footer);

        var right = new Grid { Width = 340, Margin = new Thickness(10) };
        for (var i = 0; i < 8; i++) right.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        right.RowDefinitions.Add(new RowDefinition());
        Add(right, new TextBlock { Text = "Backend-neutral analysis", FontWeight = FontWeights.Bold, FontSize = 14 }, 0);
        var form = new Grid(); form.ColumnDefinitions.Add(new ColumnDefinition()); form.ColumnDefinitions.Add(new ColumnDefinition());
        for (var i = 0; i < 3; i++) form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        AddCell(form, new TextBlock { Text = "Building class" }, 0, 0); AddCell(form, classBox, 0, 1);
        AddCell(form, new TextBlock { Text = "Maximum results" }, 1, 0); AddCell(form, limitBox, 1, 1);
        AddCell(form, new TextBlock { Text = "BBOX" }, 2, 0); AddCell(form, new TextBlock { Text = "18.04, 59.30, 18.10, 59.35" }, 2, 1); Add(right, form, 1);
        var buttons = new Grid(); buttons.ColumnDefinitions.Add(new ColumnDefinition()); buttons.ColumnDefinitions.Add(new ColumnDefinition()); AddCell(buttons, runButton, 0, 0); AddCell(buttons, cancelButton, 0, 1); Add(right, buttons, 2);
        Add(right, progress, 3); Add(right, stage, 4); Grid.SetRow(diagnostics, 8); right.Children.Add(diagnostics);
        DockPanel.SetDock(right, Dock.Right); root.Children.Add(right); root.Children.Add(viewer); Content = root;

        viewer.ActiveTool = GeoKernelViewerTool.Pan; viewer.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        runButton.IsEnabled = false; status.Text = "Loading sample data...";
        runButton.Click += (_, _) => BeginAnalysis(); cancelButton.Click += (_, _) => job?.Cancel(); poll.Tick += (_, _) => PollJob();
        Loaded += (_, _) => PrepareData(); Closing += (_, _) => { closing = true; if (job is { IsFinished: false }) job.Cancel(); layer?.Dispose(); job?.Dispose(); };
    }

    private static void Add(Grid grid, UIElement child, int row) { Grid.SetRow(child, row); grid.Children.Add(child); }
    private static void AddCell(Grid grid, UIElement child, int row, int column) { Grid.SetRow(child, row); Grid.SetColumn(child, column); grid.Children.Add(child); }

    private void PrepareData()
    {
        try
        {
            parquetPath = SampleData.EnsureWpfSampleFile(new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/stockholm_data.zip"), "stockholm_data.zip", ".", System.IO.Path.Combine("stockholm_data", "stockholm_buildings.parquet"), this);
            if (string.IsNullOrWhiteSpace(parquetPath)) { stage.Text = "Sample data is unavailable."; return; }
            runButton.IsEnabled = true; BeginAnalysis();
        }
        catch (Exception ex) { status.Text = "Sample data could not be loaded."; MessageBox.Show(this, ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void BeginAnalysis()
    {
        if (string.IsNullOrWhiteSpace(parquetPath) || job is { IsFinished: false }) return;
        job?.Dispose(); layer?.Dispose(); layer = null;
        var limit = long.TryParse(limitBox.Text, out var parsed) ? Math.Clamp(parsed, 1, 100000) : 25000;
        var request = new AnalysisRequest
        {
            Operation = AnalysisOperation.SpatialFilter, Backend = AnalysisBackend.Auto, InputKind = AnalysisDataKind.GeoParquet,
            Source = parquetPath, HasAttributeFilter = true, HasSpatialFilter = true, ProjectionRequired = true,
            Options = new Dictionary<string, object?>
            {
                ["columns"] = new[] { "id", "class", "geometry" }, ["predicateSql"] = "class = ?",
                ["predicateParameters"] = new[] { classBox.Text }, ["extent"] = new[] { 18.04, 59.30, 18.10, 59.35 }, ["limit"] = limit
            }
        };
        runButton.IsEnabled = false; cancelButton.IsEnabled = true; progress.Value = 0; diagnostics.Clear(); stage.Text = "Queuing analysis..."; status.Text = "Analysis queued...";
        job = analysis.ExecuteAsync(request); poll.Start();
    }

    private void PollJob()
    {
        if (closing || job is null) return;
        var state = job.Progress;
        if (state.TryGetProperty("percent", out var percent)) progress.Value = Math.Clamp(percent.GetInt32(), 0, 100);
        stage.Text = $"{(state.TryGetProperty("stage", out var s) ? s.GetString() : "Running")} — {(state.TryGetProperty("message", out var m) ? m.GetString() : "")}";
        status.Text = state.TryGetProperty("message", out var message) ? message.GetString() ?? "" : "";
        if (!job.IsFinished) return;
        poll.Stop(); runButton.IsEnabled = true; cancelButton.IsEnabled = false;
        using var result = job.Wait(); var value = result.Value;
        if (value.GetProperty("cancelled").GetBoolean()) { stage.Text = "Analysis cancelled."; return; }
        diagnostics.Text = AttemptsText(value);
        if (!value.GetProperty("succeeded").GetBoolean()) { stage.Text = value.GetProperty("message").GetString() ?? "Analysis failed."; return; }
        layer = result.Materialize(new { name = $"Filtered {classBox.Text} buildings", skipInvalidGeometries = true });
        viewer.ClearLayers(); layer.AddTo(viewer);
        viewer.SetLayerStyle(viewer.LayerCount - 1, new GeoKernelLayerStyle { FillColor = "#55B7E9", LineColor = "#116A9B", LineWidth = 0.8 });
        viewer.FullExtent(); progress.Value = 100;
        var materialized = layer.Diagnostics;
        diagnostics.AppendText($"\n\nMATERIALIZATION\nSource rows: {GetNumber(materialized, "sourceRowCount")}\nLayer features: {GetNumber(materialized, "materializedCount")}\nSkipped: {GetNumber(materialized, "skippedCount")}");
        stage.Text = $"{GetNumber(materialized, "materializedCount")} selected and displayed with {value.GetProperty("backend").GetString()}."; status.Text = "Analysis completed successfully.";
    }

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
