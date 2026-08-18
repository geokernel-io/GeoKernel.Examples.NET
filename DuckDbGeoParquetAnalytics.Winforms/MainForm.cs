using System.Buffers.Binary;
using System.Diagnostics;
using GeoKernel.Examples.Common;
using GeoKernel.NET.WinForms;

namespace GeoKernel.DuckDbGeoParquetAnalytics.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    private readonly ComboBox classBox = new() { DropDownStyle = ComboBoxStyle.DropDown, Dock = DockStyle.Top };
    private readonly NumericUpDown limitBox = new() { Minimum = 1, Maximum = 100000, Value = 25000, Increment = 5000, Dock = DockStyle.Top };
    private readonly Button runButton = new() { Text = "Run measured comparison", Dock = DockStyle.Top, Height = 32 };
    private readonly TextBox report = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Dock = DockStyle.Fill };
    private readonly ToolStripStatusLabel status = new();
    private string parquetPath = string.Empty;

    public MainForm()
    {
        Text = "DuckDbGeoParquetAnalytics"; Width = 1220; Height = 790;
        Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "resources", "GeoKernelAppIcon.ico"));
        classBox.Items.AddRange(["apartments", "residential", "house"]); classBox.Text = "apartments";
        var right = new TableLayoutPanel { Dock = DockStyle.Right, Width = 390, Padding = new Padding(10), RowCount = 8 };
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize)); right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize)); right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize)); right.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        right.RowStyles.Add(new RowStyle(SizeType.AutoSize)); right.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        right.Controls.Add(new Label { Text = "DuckDB GeoParquet analytics", AutoSize = true, Font = new Font(Font, FontStyle.Bold) });
        right.Controls.Add(new Label { Text = "Building class", AutoSize = true }); right.Controls.Add(classBox);
        right.Controls.Add(new Label { Text = "Maximum results", AutoSize = true }); right.Controls.Add(limitBox);
        right.Controls.Add(new Label { Text = "Spatial filter: Central Stockholm BBOX", AutoSize = true });
        right.Controls.Add(runButton); right.Controls.Add(report);
        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        tools.Items.Add("Zoom In", null, (_, _) => viewer.ZoomIn()); tools.Items.Add("Zoom Out", null, (_, _) => viewer.ZoomOut());
        tools.Items.Add("Full Extent", null, (_, _) => viewer.FullExtent()); tools.Items.Add("Pan", null, (_, _) => viewer.ActiveTool = GeoKernelViewerTool.Pan);
        var statusBar = new StatusStrip(); statusBar.Items.Add(status);
        Controls.Add(viewer); Controls.Add(right); Controls.Add(tools); Controls.Add(statusBar);
        viewer.ActiveTool = GeoKernelViewerTool.Pan; viewer.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        report.Text = "Press Run measured comparison.\r\n\r\nThe baseline transfers every row and filters in the application. The optimized path pushes predicate, BBOX, projection and limit into DuckDB.";
        runButton.Enabled = false; status.Text = "Loading sample data..."; runButton.Click += async (_, _) => await RunAsync(); Shown += LoadSample;
    }

    private void LoadSample(object? sender, EventArgs e)
    {
        try
        {
            parquetPath = SampleData.EnsureSampleFile(
                new Uri("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/stockholm_data.zip"),
                "stockholm_data.zip", ".", Path.Combine("stockholm_data", "stockholm_buildings.parquet"), this);
            if (string.IsNullOrWhiteSpace(parquetPath))
            {
                status.Text = "Sample data was not loaded.";
                return;
            }
            runButton.Enabled = true; status.Text = $"Ready: {Path.GetFileName(parquetPath)}";
        }
        catch (Exception ex)
        {
            status.Text = "Sample data could not be loaded.";
            MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async Task RunAsync()
    {
        runButton.Enabled = false; report.Text = "Running full transfer and DuckDB pushdown paths..."; status.Text = "Benchmark running in background...";
        try
        {
            var buildingClass = classBox.Text.Trim();
            var limit = (long)limitBox.Value;
            var result = await Task.Run(() => AnalyticsEngine.Run(parquetPath, buildingClass, limit));
            var timer = Stopwatch.StartNew(); viewer.ClearLayers();
            var rings = new List<IReadOnlyList<GeoKernelPoint>>();
            foreach (var wkb in result.Geometries) foreach (var ring in viewer.ReadWkbPolygon(wkb, IsMultiPolygon(wkb))) rings.Add(ring);
            viewer.AddPolygonLayer("DuckDB pushdown result", rings, new GeoKernelLayerStyle { FillColor = "#65B8E8", LineColor = "#176B9C", LineWidth = 0.8 });
            viewer.FullExtent(); timer.Stop(); report.Text = AnalyticsEngine.Report(result, timer.ElapsedMilliseconds); status.Text = "Comparison completed.";
        }
        catch (Exception ex) { report.Text = $"Comparison failed:\r\n{ex.Message}"; status.Text = "Comparison failed."; MessageBox.Show(this, ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error); }
        finally { runButton.Enabled = true; }
    }

    private static bool IsMultiPolygon(byte[] wkb)
    {
        if (wkb.Length < 5) throw new InvalidDataException("WKB header is incomplete.");
        var type = wkb[0] switch
        {
            1 => BinaryPrimitives.ReadUInt32LittleEndian(wkb.AsSpan(1, 4)),
            0 => BinaryPrimitives.ReadUInt32BigEndian(wkb.AsSpan(1, 4)),
            _ => throw new InvalidDataException("WKB byte order is invalid.")
        };
        var baseType = (int)((type & 0x0FFFFFFF) % 1000);
        return baseType switch
        {
            3 => false,
            6 => true,
            _ => throw new InvalidDataException($"Unsupported building WKB type: {baseType}.")
        };
    }
}
