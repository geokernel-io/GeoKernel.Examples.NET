using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GeoKernel.Examples.DsmToDtm;

/// <summary>Selected-area reference errors of one method.</summary>
public sealed record AccuracyMetric(int Count, double Rmse, double Mae, double Bias, double P95, int Above, int[] Bins)
{
    public JsonObject Json() => new()
    {
        ["pixels"] = Count,
        ["rmse_m"] = Rmse,
        ["mae_m"] = Mae,
        ["bias_m"] = Bias,
        ["p95_absolute_m"] = P95,
        ["above_threshold_pixels"] = Above,
        ["above_threshold_percent"] = Count > 0 ? 100.0 * Above / Count : 0.0,
        ["absolute_error_histogram"] = new JsonArray(Bins.Select(b => (JsonNode)b).ToArray()),
    };
}

/// <summary>Native source cells of an evaluation area: column, row, width and height.</summary>
public readonly record struct PixelWindow(int Left, int Top, int Width, int Height)
{
    public bool Contains(int x, int y) => x >= Left && x < Left + Width && y >= Top && y < Top + Height;
}

/// <summary>Reference-accuracy helpers shared by the DsmToDtmAccuracy examples.</summary>
public static class Accuracy
{
    public static readonly double[] DisplayLimits = [1, 2, 5, 10];
    public static readonly double[] BinEdges = [.25, .5, 1, 2, 5];
    public static readonly string[] Presets = ["Full sample", "Central urban block", "Northern block", "Southern block", "Custom selection"];
    public static readonly string[] MethodColors = ["#087e8b", "#8857bd"];

    static (int R, int G, int B) Rgb(string hex)
    {
        int v = Convert.ToInt32(hex.TrimStart('#'), 16);
        return (v >> 16, (v >> 8) & 0xff, v & 0xff);
    }
    static readonly (int R, int G, int B) Neutral = Rgb("#fafafa"), Below = Rgb("#2464b5"), Above = Rgb("#c93645"), Orange = Rgb("#f4a340"), Red = Rgb("#b22f35");

    static int Round(double v) => (int)Math.Round(v, MidpointRounding.AwayFromZero);
    static (int R, int G, int B) Mix((int R, int G, int B) a, (int R, int G, int B) b, double t) =>
        (Round(a.R + (b.R - a.R) * t), Round(a.G + (b.G - a.G) * t), Round(a.B + (b.B - a.B) * t));

    /// <summary>Shared display scale: signed blue-white-red or absolute white-orange-red, saturating at the limit.</summary>
    public static (int R, int G, int B) ErrorColor(double error, bool absolute, double limit)
    {
        double t = Math.Clamp(Math.Abs(error) / limit, 0, 1);
        if (!absolute) return Mix(Neutral, error < 0 ? Below : Above, t);
        return t < .5 ? Mix(Neutral, Orange, t * 2) : Mix(Orange, Red, (t - .5) * 2);
    }

    /// <summary>R, G, B bytes of one error display map (estimate − reference).</summary>
    public static (byte[] R, byte[] G, byte[] B) ErrorBands(Grids grids, int method, bool absolute, double limit, CancellationToken token)
    {
        float[] estimate = grids.Z[method], reference = grids.Z[3];
        var (r, g, b) = (new byte[reference.Length], new byte[reference.Length], new byte[reference.Length]);
        for (int y = 0; y < grids.Height; y++)
        {
            token.ThrowIfCancellationRequested();
            for (int i = y * grids.Width; i < (y + 1) * grids.Width; i++)
            {
                var c = ErrorColor((double)estimate[i] - reference[i], absolute, limit);
                (r[i], g[i], b[i]) = ((byte)c.R, (byte)c.G, (byte)c.B);
            }
        }
        return (r, g, b);
    }

    /// <summary>Native cells whose centres lie in the area, clipped to the grid.</summary>
    public static PixelWindow Window(Grids grids, (double XMin, double YMin, double XMax, double YMax) area)
    {
        if (!double.IsFinite(area.XMin) || !double.IsFinite(area.YMin) || !double.IsFinite(area.XMax) || !double.IsFinite(area.YMax) || area.XMax <= area.XMin || area.YMax <= area.YMin)
            throw new ArgumentException("Choose a nonempty finite study area.");
        var t = grids.Transform;
        double dx = t[1], dy = -t[5], x0 = t[0], y0 = t[3];
        static int Clip(double v, int max) => (int)Math.Clamp(v, 0, max);
        int left = Clip(Math.Ceiling((area.XMin - x0) / dx - .5), grids.Width);
        int right = Clip(Math.Floor((area.XMax - x0) / dx - .5) + 1, grids.Width);
        int top = Clip(Math.Ceiling((y0 - area.YMax) / dy - .5), grids.Height);
        int bottom = Clip(Math.Floor((y0 - area.YMin) / dy - .5) + 1, grids.Height);
        if (right <= left || bottom <= top) throw new ArgumentException("The selected area contains no source pixel centres.");
        return new PixelWindow(left, top, right - left, bottom - top);
    }

    /// <summary>World bounds of the union of the window's cells.</summary>
    public static (double XMin, double YMin, double XMax, double YMax) Extent(Grids grids, PixelWindow w)
    {
        var t = grids.Transform;
        return (t[0] + w.Left * t[1], t[3] + (w.Top + w.Height) * t[5], t[0] + (w.Left + w.Width) * t[1], t[3] + w.Top * t[5]);
    }

    /// <summary>Geographic windows (not land-cover classes): full, central, northern, southern.</summary>
    public static (double XMin, double YMin, double XMax, double YMax) PresetArea(Grids grids, int index)
    {
        double x = grids.Transform[0], y = grids.Transform[3], size = grids.Width * .5;
        return index switch
        {
            0 => (x, y - size, x + size, y),
            1 => (x + size * .2, y - size * .8, x + size * .8, y - size * .2),
            2 => (x, y - size * .35, x + size, y),
            _ => (x, y - size, x + size, y - size * .65),
        };
    }

    /// <summary>RMSE, MAE, bias, nearest-rank P95 of |error|, count above threshold and six-bin histogram.</summary>
    public static AccuracyMetric[] Measure(Grids grids, PixelWindow w, double threshold)
    {
        if (!double.IsFinite(threshold) || threshold <= 0) throw new ArgumentException("Error threshold must be positive.");
        if (w.Width <= 0 || w.Height <= 0 || w.Left < 0 || w.Top < 0 || w.Left + w.Width > grids.Width || w.Top + w.Height > grids.Height)
            throw new ArgumentException("Invalid source pixel window.");
        var result = new AccuracyMetric[2];
        float[] reference = grids.Z[3];
        for (int method = 0; method < 2; method++)
        {
            float[] estimate = grids.Z[method + 1];
            double squared = 0, absolute = 0, signed = 0;
            int above = 0;
            var bins = new int[6];
            var errors = new double[w.Width * w.Height];
            int n = 0;
            for (int y = w.Top; y < w.Top + w.Height; y++)
                for (int i = y * grids.Width + w.Left; i < y * grids.Width + w.Left + w.Width; i++)
                {
                    double error = (double)estimate[i] - reference[i], magnitude = Math.Abs(error);
                    squared += error * error;
                    absolute += magnitude;
                    signed += error;
                    if (magnitude > threshold) above++;
                    int bin = 0;
                    while (bin < BinEdges.Length && magnitude > BinEdges[bin]) bin++;
                    bins[bin]++;
                    errors[n++] = magnitude;
                }
            Array.Sort(errors);
            result[method] = new AccuracyMetric(n, Math.Sqrt(squared / n), absolute / n, signed / n, errors[(int)Math.Ceiling(.95 * n) - 1], above, bins);
        }
        return result;
    }

    /// <summary>Selected-area metrics and provenance as JSON, written atomically.</summary>
    public static void ExportReport(string path, Grids grids, PixelWindow w, string areaName, double threshold, AccuracyMetric[] metrics, string archiveSha256)
    {
        var e = Extent(grids, w);
        var report = new JsonObject
        {
            ["schema_version"] = 1,
            ["sample_archive_sha256"] = archiveSha256,
            ["source_manifest"] = grids.Manifest.DeepClone(),
            ["area_name"] = areaName,
            ["source_pixel_window"] = new JsonObject { ["column"] = w.Left, ["row"] = w.Top, ["width"] = w.Width, ["height"] = w.Height },
            ["evaluated_cell_bounds_epsg2056"] = new JsonArray(e.XMin, e.YMin, e.XMax, e.YMax),
            ["pixel_footprint_m2"] = metrics[0].Count * .25,
            ["large_error_threshold_m"] = threshold,
            ["p95_definition"] = "Nearest-rank empirical 95th percentile of absolute reference errors",
            ["histogram_bin_edges_m"] = new JsonArray(BinEdges.Select(v => (JsonNode)v).ToArray()),
            ["classical"] = metrics[0].Json(),
            ["ai_pix_grad"] = metrics[1].Json(),
            ["reference_used_for_inference"] = false,
            ["metric_scope"] = "Shared native pixel centres in this selected area; fixed development scene; not held-out generalization accuracy",
            ["attribution"] = "Federal Office of Topography swisstopo; AI: David Perez, pix_grad adaptation",
        };
        string temporary = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, $".{Path.GetFileName(path)}.{Environment.ProcessId}.tmp");
        try
        {
            File.WriteAllText(temporary, report.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), Encoding.UTF8);
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    /// <summary>Nineteen shared-scale Byte RGB maps: DSM relief; per method 4 signed, 4 absolute, 1 relief.</summary>
    public static (string Original, string[][] Methods) PrepareDisplays(Grids g, string folder, IProgress<int> progress, CancellationToken token)
    {
        int done = 0;
        string Save(byte[] r, byte[] gr, byte[] b, string name)
        {
            token.ThrowIfCancellationRequested();
            string path = Gdal.WriteRgb(Path.Combine(folder, name), g.Width, g.Height, g.Transform, g.Projection, r, gr, b);
            progress.Report(++done * 100 / 19);
            return path;
        }
        string Relief(int band, string name) { var gray = Profiles.Relief(g, band, token); return Save(gray, gray, gray, name); }
        string original = Relief(0, "original.tif");
        var methods = new string[2][];
        for (int m = 0; m < 2; m++)
        {
            methods[m] = new string[9];
            for (int k = 0; k < 4; k++)
            {
                var s = ErrorBands(g, m + 1, false, DisplayLimits[k], token);
                methods[m][k] = Save(s.R, s.G, s.B, $"signed-{m}-{k}.tif");
                var a = ErrorBands(g, m + 1, true, DisplayLimits[k], token);
                methods[m][4 + k] = Save(a.R, a.G, a.B, $"absolute-{m}-{k}.tif");
            }
            methods[m][8] = Relief(m + 1, $"terrain-{m}.tif");
        }
        return (original, methods);
    }
}

/// <summary>User-facing text shared by the WinForms and WPF accuracy windows.</summary>
public static class AccuracyText
{
    public const string Title = "DSM to DTM | Where is each method more accurate? — GeoKernel";
    public const string Ready = "Ready · select an evaluation area or click any source pixel.";
    public const string Idle = "Select an area, choose a preset, or click a pixel to inspect errors.";
    public const string Selecting = "Drag a box on any map. The same source pixels will be evaluated for both methods.";
    public const string Note = "One shared reference and source grid. Geographic presets are not land-cover classes. Dimmed map pixels are outside the evaluated area.";
    public const string Footnote = "Prepared independent results; reference is evaluation-only. Source heights: LN02 metres, no vertical datum conversion. Source filename years: DSM 2018 / reference 2019. Fixed development crop; these are not held-out benchmark results.";
    public static readonly string[] Titles = ["ORIGINAL DSM · context", "CLASSICAL DTM · reference error", "AI DTM · reference error"];
    public static readonly string[][] Headings = [["CLASSICAL DTM · signed error", "AI DTM · signed error"], ["CLASSICAL DTM · absolute error", "AI DTM · absolute error"], ["CLASSICAL DTM · terrain estimate", "AI DTM · experimental pix_grad"]];
    public static readonly string[] BinLabels = ["≤0.25", "0.25–0.5", "0.5–1", "1–2", "2–5", ">5 m"];

    public static string Winners(AccuracyMetric[] m)
    {
        static string Rank(double c, double a) => Math.Abs(c - a) <= 1e-9 ? "Tie" : c < a ? "Classical" : "AI · pix_grad";
        return $"THIS AREA ONLY\n\nLower RMSE: {Rank(m[0].Rmse, m[1].Rmse)}\nLower MAE: {Rank(m[0].Mae, m[1].Mae)}\n\nDifferent metrics can rank methods differently. No overall winner is implied.";
    }

    public static string Scope(string area, AccuracyMetric[] m) =>
        $"{area} · {m[0].Count} common valid reference pixels · evaluated cell footprint {m[0].Count * .25:F2} m² · reference coverage 100% · bias is estimate − reference";

    public static string[] Row(int method, AccuracyMetric m) =>
        [method == 1 ? "AI · pix_grad" : "Classical", $"{m.Rmse:F3}", $"{m.Mae:F3}", $"{m.Bias:F3}", $"{m.P95:F3}", $"{100.0 * m.Above / m.Count:F2}"];

    public static string Probe(Grids g, int x, int y, PixelWindow? area)
    {
        int i = y * g.Width + x;
        string outside = area is { } a && a.Contains(x, y) ? "" : "\nOutside evaluation area";
        var z = g.Z;
        return $"SOURCE PIXEL · column {x} / row {y}{outside}\n\nDSM: {z[0][i]:F2} m\nReference: {z[3][i]:F2} m\n\n" +
               $"Classical: {z[1][i]:F2} m · error {(double)z[1][i] - z[3][i]:F3} m\nAI: {z[2][i]:F2} m · error {(double)z[2][i] - z[3][i]:F3} m";
    }

    public static string LegendTitle(int mode) => mode == 0
        ? "Estimated DTM − reference: blue below · white zero · red above (metres)"
        : "Absolute reference error: white zero · orange/red larger errors (metres)";
    public const string LegendRelief = "Neutral relief: same light and contrast for both terrain estimates. Switch to an error view to compare reference differences.";
    public const string LegendNote = "One shared scale · display saturation only; numeric errors are unchanged";
}
