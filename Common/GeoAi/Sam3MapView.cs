#if GEOAI_WPF
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.GeoAi;

internal sealed class Sam3MapView
{
    private const string ResultName = "SAM3 result";
    private readonly GeoKernelViewerControl viewer;
    private readonly List<string> promptLayers = [];
    private int resultIndex = -1;
    public Sam3Prompts Prompts { get; } = new();
    public Sam3MapTool Tool { get; private set; }
    public Func<bool> CanSelect { get; set; } = () => false;
    public event Action? SelectionChanged;
    public string SelectionText { get; private set; } = "0 points · no box";

    public Sam3MapView(GeoKernelViewerControl viewer)
    {
        this.viewer = viewer;
        viewer.MapMouseDown += (_, e) =>
        {
            if (!CanSelect() || Tool == Sam3MapTool.Pan || e.ButtonOrButtons != 1) return;
            try
            {
                Prompts.Add(Tool, e.WorldPoint.X, e.WorldPoint.Y);
                DrawPrompts();
                SelectionText = Prompts.Description;
            }
            catch (ArgumentException error) { SelectionText = error.Message; }
            SelectionChanged?.Invoke();
        };
    }

    public void SetTool(Sam3MapTool tool)
    {
        Tool = tool;
        viewer.ActiveTool = tool == Sam3MapTool.Pan ? GeoKernelViewerTool.Pan : GeoKernelViewerTool.Info;
    }

    public void ShowSource(string path)
    {
        viewer.ClearLayers();
        promptLayers.Clear();
        resultIndex = -1;
        viewer.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.Wgs84);
        if (!viewer.AddLayerFile(path)) throw new InvalidOperationException("Cannot open the SAM3 RGB source.");
        viewer.SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.Wgs84);
        viewer.SetLayerName(0, "Buildings RGB source");
        viewer.FullExtent();
    }

    public void ShowResult(string path)
    {
        if (!viewer.AddLayerFile(path)) throw new InvalidOperationException("Cannot display the SAM3 draft polygons.");
        resultIndex = 0; // Native viewer prepends every newly loaded layer.
        viewer.SetLayerName(resultIndex, ResultName);
        viewer.SetLayerStyleJson(resultIndex, "{\"fillColor\":\"#ff963f\",\"fillOpacity\":120,\"lineColor\":\"#ffd65a\",\"lineWidth\":1.1}");
        ApplyOpacity(60);
    }

    public void ClearResult()
    {
        if (resultIndex >= 0) viewer.RemoveLayerByName(ResultName);
        resultIndex = -1;
    }

    public void ApplyOpacity(double percent)
    {
        if (resultIndex < 0) return;
        VectorOverlay.ApplyOpacity(viewer, resultIndex, percent, "#ff963f", 120, "ffd65a", 1.1);
    }

    public void ClearPrompts()
    {
        Prompts.Clear();
        DrawPrompts();
        SelectionText = Prompts.Description;
        SelectionChanged?.Invoke();
    }

    private void DrawPrompts()
    {
        foreach (string name in promptLayers) viewer.RemoveLayerByName(name);
        promptLayers.Clear();
        void Point(Sam3MapPoint p, string color)
        {
            string name = "SAM3 prompt " + promptLayers.Count;
            int index = viewer.AddPointLayer(name, [new GeoKernelPoint(p.X, p.Y)],
                new GeoKernelLayerStyle { PointColor = color, PointSize = 10, LineColor = "#ffffff", LineWidth = 1 });
            if (index < 0) throw new ArgumentException("Cannot display the selected point.");
            viewer.SetLayerCoordinateSystemPreset(index, GeoKernelCoordinateSystemPreset.Wgs84);
            promptLayers.Add(name);
        }
        foreach (Sam3MapPoint p in Prompts.Points) Point(p, p.Positive ? "#20df80" : "#ff4848");
        if (Prompts.FirstCorner is Sam3MapPoint first) Point(first, "#ffe45a");
        if (Prompts.Box is Sam3MapBox box)
        {
            string name = "SAM3 prompt box";
            int index = viewer.AddPolygonLayer(name,
                [new GeoKernelPoint(box.MinimumX, box.MinimumY), new GeoKernelPoint(box.MaximumX, box.MinimumY),
                 new GeoKernelPoint(box.MaximumX, box.MaximumY), new GeoKernelPoint(box.MinimumX, box.MaximumY),
                 new GeoKernelPoint(box.MinimumX, box.MinimumY)],
                new GeoKernelLayerStyle { FillColor = "#ffe45a", FillOpacity = 24, LineColor = "#ffe45a", LineWidth = 2 });
            if (index < 0) throw new ArgumentException("Cannot display the selected box.");
            viewer.SetLayerCoordinateSystemPreset(index, GeoKernelCoordinateSystemPreset.Wgs84);
            promptLayers.Add(name);
        }
        viewer.RefreshLayers();
    }
}
