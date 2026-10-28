using System.Text.Json;
#if GEOAI_WPF
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.GeoAi;

internal static class VectorOverlay
{
    public static void ApplyOpacity(GeoKernelViewerControl viewer, int index, double percent,
        string fillColor = "#ef5350", int fillAlpha = 110, string lineRgb = "ffdd55", double lineWidth = 1.0)
    {
        double opacity = Math.Clamp(percent / 100.0, 0, 1);
        // Apply alpha in the actual vector paint, including native backends that
        // currently honor layer opacity for raster images but not vector brushes.
        string style = JsonSerializer.Serialize(new
        {
            fillColor,
            fillOpacity = (int)Math.Round(fillAlpha * opacity),
            lineColor = $"#{(int)Math.Round(255 * opacity):X2}{lineRgb}",
            lineWidth
        });
        if (!viewer.SetLayerStyleJson(index, style) || !viewer.SetLayerOpacity(index, 1) ||
            !viewer.SetLayerVisible(index, opacity > 0))
            throw new InvalidOperationException("Cannot update the result polygon style.");
        viewer.RefreshLayers();
    }
}
