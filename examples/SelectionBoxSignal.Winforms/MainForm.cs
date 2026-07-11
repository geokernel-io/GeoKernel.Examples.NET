using GeoKernel.NET.WinForms;

namespace GeoKernel.SelectionBoxSignal.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        ConfigureGrids();
        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Select;
        geoKernelViewerControl.MapSelectionBoxFinished += geoKernelViewerControl_MapSelectionBoxFinished;

        if (!LoadSampleLayers())
            return;

        ShowEmptyHits();
        SetSampleExtent();
        UpdateStatus("Drag a box with Select to emit MapSelectionBoxFinished(rect, extent, modifiers).");
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
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", "SelectionBoxSignal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = style }))
        {
            MessageBox.Show(this, $"Layer could not be loaded:{Environment.NewLine}{path}", "SelectionBoxSignal", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(geoKernelViewerControl.LayerCount - 1);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, displayName);

        return true;
    }

    private void selectButton_Click(object? sender, EventArgs e)
    {
        selectButton.Checked = true;
        panButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Select;
        UpdateStatus("Drag a box to emit MapSelectionBoxFinished.");
    }

    private void panButton_Click(object? sender, EventArgs e)
    {
        panButton.Checked = true;
        selectButton.Checked = false;
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        UpdateStatus("Pan mode.");
    }

    private void clearSelectionButton_Click(object? sender, EventArgs e)
    {
        geoKernelViewerControl.ClearSelectedFeatures();
        ShowEmptyHits();
        UpdateStatus("Selection cleared.");
    }

    private void fullExtentButton_Click(object? sender, EventArgs e)
    {
        SetSampleExtent();
    }

    private void geoKernelViewerControl_MapSelectionBoxFinished(object? sender, GeoKernelSelectionBoxFinishedEventArgs e)
    {
        var hits = geoKernelViewerControl.HitTestFeaturesInScreenRectangle(e.ScreenRectangle)
            .Where(hit => hit.IsValid)
            .ToList();

        AppendSignalLog(e, hits.Count);
        ShowHits(hits);
        UpdateStatus($"MapSelectionBoxFinished: rect={RectText(e.ScreenRectangle)} extent={ExtentText(e.WorldExtent)} modifiers={ModifiersText(e.Modifiers)} hits={hits.Count}.");
    }

    private void ConfigureGrids()
    {
        signalGrid.Columns.Clear();
        signalGrid.Columns.Add("Time", "Time");
        signalGrid.Columns.Add("Rect", "Screen rect");
        signalGrid.Columns.Add("Extent", "World extent");
        signalGrid.Columns.Add("Modifiers", "Modifiers");
        signalGrid.Columns.Add("HitCount", "Hits");

        hitsGrid.Columns.Clear();
        hitsGrid.Columns.Add("Number", "#");
        hitsGrid.Columns.Add("Layer", "Layer");
        hitsGrid.Columns.Add("ShapeId", "Shape id");
        hitsGrid.Columns.Add("FeatureId", "Feature id");
        hitsGrid.Columns.Add("ShapeType", "Type");
    }

    private void AppendSignalLog(GeoKernelSelectionBoxFinishedEventArgs e, int hitCount)
    {
        signalGrid.Rows.Add(
            DateTime.Now.ToString("HH:mm:ss.fff"),
            RectText(e.ScreenRectangle),
            ExtentText(e.WorldExtent),
            ModifiersText(e.Modifiers),
            hitCount);

        if (signalGrid.Rows.Count > 0)
            signalGrid.FirstDisplayedScrollingRowIndex = signalGrid.Rows.Count - 1;
    }

    private void ShowEmptyHits()
    {
        hitsGrid.Rows.Clear();
        hitsGrid.Rows.Add("-", "Drag a selection box to list matching features.", "-", "-", "-");
    }

    private void ShowHits(IReadOnlyList<GeoKernelFeatureHitTestResult> hits)
    {
        hitsGrid.Rows.Clear();
        for (var i = 0; i < hits.Count; i++)
        {
            var hit = hits[i];
            hitsGrid.Rows.Add(i + 1, hit.LayerName, hit.ShapeId, hit.FeatureId, hit.ShapeType);
        }

        if (hits.Count == 0)
            hitsGrid.Rows.Add("-", "No hits", "-", "-", "-");
    }

    private void SetSampleExtent()
    {
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-130.0, 22.0, -65.0, 55.0);
    }

    private void UpdateStatus(string text)
    {
        statusLabel.Text = text;
    }

    private static string RectText(GeoKernelScreenRectangle rect)
    {
        return $"left={rect.Left} top={rect.Top} width={rect.Width} height={rect.Height}";
    }

    private static string ExtentText(GeoKernelExtent extent)
    {
        return $"({extent.XMin:F6}, {extent.YMin:F6}) - ({extent.XMax:F6}, {extent.YMax:F6})";
    }

    private static string ModifiersText(int modifiers)
    {
        if (modifiers == 0)
            return "-";

        var parts = new List<string>();
        if ((modifiers & 0x02000000) != 0)
            parts.Add("Shift");
        if ((modifiers & 0x04000000) != 0)
            parts.Add("Ctrl");
        if ((modifiers & 0x08000000) != 0)
            parts.Add("Alt");
        if ((modifiers & 0x10000000) != 0)
            parts.Add("Meta");
        return parts.Count == 0 ? modifiers.ToString() : string.Join("+", parts);
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
        FillOpacity = 155,
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
