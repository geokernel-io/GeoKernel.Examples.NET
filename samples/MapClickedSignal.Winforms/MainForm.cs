using GeoKernel.NET.WinForms;

namespace GeoKernel.MapClickedSignal.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        ConfigureLogGrid();
        geoKernelViewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(244, 246, 245);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;

        if (!LoadSampleLayers())
            return;

        SetSampleExtent();
        UpdateStatus("Click the map to log mapClicked(tool, screenPoint, worldPoint, modifiers).");
    }

    private bool LoadSampleLayers()
    {
        return AddLayer("world_4326.shp", "World", WorldStyle())
            && AddLayer("usa_states_4326.shp", "USA States", StateStyle())
            && AddLayer("cities_4326.shp", "Cities", CityStyle());
    }

    private bool AddLayer(string fileName, string displayName, GeoKernelLayerStyle style)
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", fileName);
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "MapClickedSignal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "MapClickedSignal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(geoKernelViewerControl.LayerCount - 1);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void infoButton_Click(object? sender, EventArgs e)
    {
        infoButton.Checked = true;
        panButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Info;
        UpdateStatus("Info tool active. Click to emit/log mapClicked.");
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        panButton.Checked = true;
        infoButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan mode. Mouse clicks still report the active tool when released.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_MouseClick(object? sender, MouseEventArgs e)
    {
        var worldPoint = geoKernelViewerControl.ScreenToWorld(e.X, e.Y);
        var hit = geoKernelViewerControl.HitTestTopFeatureAt(e.X, e.Y, 8);

        if (hit is not null && hit.IsValid)
        {
            geoKernelViewerControl.ClearSelectedFeatures();
            geoKernelViewerControl.AddTopFeatureToSelectionAt(e.X, e.Y, 8);
        }
        else
        {
            geoKernelViewerControl.ClearSelectedFeatures();
        }

        AppendClickLog(e, worldPoint, hit);
        UpdateStatus($"mapClicked: tool={geoKernelViewerControl.ActiveTool} screen={PointText(e.X, e.Y)} world={PointText(worldPoint.X, worldPoint.Y)} modifiers={ModifiersText()}");
    }

    private void ConfigureLogGrid()
    {
        logGrid.Columns.Clear();
        logGrid.Columns.Add("Time", "Time");
        logGrid.Columns.Add("Tool", "Tool");
        logGrid.Columns.Add("ScreenPoint", "Screen point");
        logGrid.Columns.Add("WorldPoint", "World point");
        logGrid.Columns.Add("Modifiers", "Modifiers");
        logGrid.Columns.Add("HitLayer", "Hit layer");
        logGrid.Columns.Add("FeatureId", "Feature ID");
        logGrid.Columns.Add("ShapeType", "Shape type");
    }

    private void AppendClickLog(MouseEventArgs e, GeoKernelPoint worldPoint, GeoKernelFeatureHitTestResult? hit)
    {
        logGrid.Rows.Add(
            DateTime.Now.ToString("HH:mm:ss.fff"),
            geoKernelViewerControl.ActiveTool,
            PointText(e.X, e.Y),
            PointText(worldPoint.X, worldPoint.Y),
            ModifiersText(),
            hit is { IsValid: true } ? hit.LayerName : "-",
            hit is { IsValid: true } ? hit.FeatureId.ToString() : "-",
            hit is { IsValid: true } ? hit.ShapeType.ToString() : "-");

        if (logGrid.Rows.Count > 0)
            logGrid.FirstDisplayedScrollingRowIndex = logGrid.Rows.Count - 1;
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
        statusLabel.Text = text;
    }

    private static string PointText(double x, double y)
    {
        return $"({x:F6}, {y:F6})";
    }

    private static string ModifiersText()
    {
        var modifiers = Control.ModifierKeys;
        var parts = new List<string>();
        if (modifiers.HasFlag(Keys.Shift))
            parts.Add("Shift");
        if (modifiers.HasFlag(Keys.Control))
            parts.Add("Ctrl");
        if (modifiers.HasFlag(Keys.Alt))
            parts.Add("Alt");
        return parts.Count == 0 ? "-" : string.Join("+", parts);
    }

    private static GeoKernelLayerStyle WorldStyle() => new()
    {
        FillColor = "#D8E5E1",
        FillOpacity = 210,
        LineColor = "#708984",
        LineWidth = 0.6,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 3.0
    };

    private static GeoKernelLayerStyle StateStyle() => new()
    {
        FillColor = "#C7DEE7",
        FillOpacity = 160,
        LineColor = "#2D6F8E",
        LineWidth = 1.0,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 4.0
    };

    private static GeoKernelLayerStyle CityStyle() => new()
    {
        PointColor = "#D95D39",
        LineColor = "#8C321D",
        PointSize = 8.0,
        LineWidth = 1.0,
        SelectedLineColor = "#F59E0B",
        SelectedLineWidth = 4.0,
        ShowLabels = true,
        LabelField = "NAME",
        LabelFontSize = 9.0,
        LabelColor = "#263238",
        LabelHaloEnabled = true,
        LabelHaloColor = "#FFFFFF",
        LabelHaloWidth = 2.0
    };

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "assets", "data")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
