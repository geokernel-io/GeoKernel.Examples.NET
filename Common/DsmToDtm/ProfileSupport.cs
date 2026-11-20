using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GeoKernel.Examples.DsmToDtm;

/// <summary>One bilinear sample of all four rasters on a transect.</summary>
public readonly record struct ProfileSample(double X, double Y, double Distance, double[] Heights);

/// <summary>An equal-spaced transect with line-only RMSE/MAE of both estimates against the reference.</summary>
public sealed record TransectProfile(
    (double X, double Y) Start, (double X, double Y) End, double Length, double Spacing,
    IReadOnlyList<ProfileSample> Samples, double[] Rmse, double[] Mae);

/// <summary>Interactive-profile helpers shared by the DsmToDtmProfile examples.</summary>
public static class Profiles
{
    public static readonly string[] SurfaceNames = ["Original DSM", "Classical DTM", "AI DTM", "Reference DTM"];

    /// <summary>Hillshade with one fixed light (azimuth 315°, altitude 40°) and contrast for every map.
    /// No per-map normalization: identical heights give identical gray values in every relief.</summary>
    public static byte[] Relief(Grids grids, int band, CancellationToken token)
    {
        int w = grids.Width, h = grids.Height;
        float[] z = grids.Z[band];
        double azimuth = 315 * Math.PI / 180, altitude = 40 * Math.PI / 180;
        double lx = Math.Sin(azimuth) * Math.Cos(altitude), ly = Math.Cos(azimuth) * Math.Cos(altitude), lz = Math.Sin(altitude);
        var output = new byte[w * h];
        for (int y = 0; y < h; y++)
        {
            token.ThrowIfCancellationRequested();
            int top = Math.Max(0, y - 1), bottom = Math.Min(h - 1, y + 1);
            for (int x = 0; x < w; x++)
            {
                int left = Math.Max(0, x - 1), right = Math.Min(w - 1, x + 1);
                double dx = (z[y * w + right] - z[y * w + left]) / ((right - left) * .5);
                double dy = (z[top * w + x] - z[bottom * w + x]) / ((bottom - top) * .5);
                double light = (-dx * lx - dy * ly + lz) / Math.Sqrt(dx * dx + dy * dy + 1);
                output[y * w + x] = (byte)Math.Clamp((int)Math.Round(255 * (.25 + .75 * Math.Max(0, light)), MidpointRounding.AwayFromZero), 0, 255);
            }
        }
        return output;
    }

    /// <summary>Bilinear height between pixel centres; null outside them (no extrapolation).</summary>
    public static double? Height(Grids grids, int band, double x, double y)
    {
        var t = grids.Transform;
        double col = (x - t[0]) / t[1] - .5, row = (y - t[3]) / t[5] - .5;
        if (!double.IsFinite(col) || !double.IsFinite(row) || col < -1e-7 || row < -1e-7 || col > grids.Width - 1 + 1e-7 || row > grids.Height - 1 + 1e-7)
            return null;
        double cx = Math.Clamp(col, 0, grids.Width - 1), cy = Math.Clamp(row, 0, grids.Height - 1);
        int ix = Math.Min((int)cx, grids.Width - 2), iy = Math.Min((int)cy, grids.Height - 2), w = grids.Width;
        double u = cx - ix, v = cy - iy;
        float[] z = grids.Z[band];
        int i = iy * w + ix;
        return (1 - v) * ((1 - u) * z[i] + u * z[i + 1]) + v * ((1 - u) * z[i + w] + u * z[i + w + 1]);
    }

    /// <summary>ceil(length / spacing) + 1 equal-spaced samples (at most 4096).</summary>
    public static TransectProfile Make(Grids grids, (double X, double Y) a, (double X, double Y) b, double requested)
    {
        if (Height(grids, 0, a.X, a.Y) == null || Height(grids, 0, b.X, b.Y) == null)
            throw new ArgumentException("Choose both endpoints inside the raster pixel centres.");
        if (!double.IsFinite(requested) || requested < .5 || requested > 10)
            throw new ArgumentException("Profile spacing must be between 0.5 and 10 metres.");
        double length = Math.Sqrt((b.X - a.X) * (b.X - a.X) + (b.Y - a.Y) * (b.Y - a.Y));
        if (length < .5) throw new ArgumentException("Choose endpoints at least 0.5 m apart.");
        int intervals = Math.Clamp((int)Math.Ceiling(length / requested), 1, 4095);
        var samples = new List<ProfileSample>(intervals + 1);
        double[] squared = new double[2], absolute = new double[2];
        for (int i = 0; i <= intervals; i++)
        {
            double t = (double)i / intervals, x = a.X * (1 - t) + b.X * t, y = a.Y * (1 - t) + b.Y * t;
            var heights = new double[4];
            for (int band = 0; band < 4; band++)
                heights[band] = Height(grids, band, x, y) ?? throw new ArgumentException("The transect crosses unavailable raster data.");
            for (int method = 0; method < 2; method++)
            {
                double error = heights[method + 1] - heights[3];
                squared[method] += error * error;
                absolute[method] += Math.Abs(error);
            }
            samples.Add(new ProfileSample(x, y, length * t, heights));
        }
        int n = samples.Count;
        return new TransectProfile(a, b, length, length / intervals, samples,
            [Math.Sqrt(squared[0] / n), Math.Sqrt(squared[1] / n)], [absolute[0] / n, absolute[1] / n]);
    }

    /// <summary>The fixed building transect of the first example (row <c>profile.row</c>, west half).</summary>
    public static ((double X, double Y) A, (double X, double Y) B) Example(Grids grids)
    {
        var t = grids.Transform;
        double y = t[3] + (grids.ProfileRow + .5) * t[5];
        return ((t[0] + .25, y), (t[0] + (grids.Width - .5) * .5, y));
    }

    static void AtomicWrite(string path, byte[] data)
    {
        string temporary = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(path))!, $".{Path.GetFileName(path)}.{Environment.ProcessId}.tmp");
        try
        {
            using (var file = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                file.Write(data);
                file.Flush(true);
            }
            File.Move(temporary, path, true);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    /// <summary>Write the numeric CSV, then <c>&lt;name&gt;.csv.metadata.json</c>; each file atomically.</summary>
    public static void Export(string path, TransectProfile profile, Grids grids, string archiveSha256)
    {
        var csv = new StringBuilder("distance_m,easting_epsg2056_m,northing_epsg2056_m,dsm_ln02_m,classical_ln02_m,ai_ln02_m,reference_ln02_m,classical_error_m,ai_error_m\n");
        foreach (var s in profile.Samples)
        {
            var h = s.Heights;
            double[] values = [s.Distance, s.X, s.Y, h[0], h[1], h[2], h[3], h[1] - h[3], h[2] - h[3]];
            csv.AppendJoin(',', values.Select(v => v.ToString("F6", CultureInfo.InvariantCulture))).Append('\n');
        }
        AtomicWrite(path, Encoding.UTF8.GetBytes(csv.ToString()));
        var metadata = new JsonObject
        {
            ["sample_archive_sha256"] = archiveSha256,
            ["source"] = grids.Manifest.DeepClone(),
            ["sampling"] = "Bilinear numeric heights, projected metric distances",
            ["line_length_m"] = profile.Length,
            ["sample_spacing_m"] = profile.Spacing,
            ["sample_count"] = profile.Samples.Count,
            ["source_attribution"] = "Federal Office of Topography swisstopo; AI: David Perez, pix_grad adaptation",
        };
        try { AtomicWrite(path + ".metadata.json", Encoding.UTF8.GetBytes(metadata.ToJsonString(new JsonSerializerOptions { WriteIndented = true }))); }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            throw new IOException($"CSV saved; cannot save metadata sidecar: {e.Message}", e);
        }
    }
}

/// <summary>User-facing text shared by the WinForms and WPF profile windows.</summary>
public static class ProfileText
{
    public const string Title = "DSM to DTM | Interactive elevation profile — GeoKernel";
    public const string Ready = "Ready · draw A → B on either map; move over the chart to inspect heights.";
    public const string NoCursor = "Move over the chart to locate a sample on both maps.";
    public const string Footnote = "Prepared independent results; no new inference. Reference is evaluation-only. Source LN02 heights, metres; no vertical datum conversion. DSM/reference source years: 2018/2019. This is a fixed development scene.";
    public const string Numeric = "Profiles sample numeric Float32 heights with bilinear interpolation. Relief images are for display only. Finer sampling cannot add source detail.";
    public static readonly string[] Headings = ["CLASSICAL DTM · prepared estimate", "AI DTM · experimental pix_grad", "REFERENCE DTM · evaluation only"];
    public static readonly string[] Legends = ["Gray DSM · teal classical · purple AI · dashed orange reference · map: A teal, B orange",
        "Teal classical − reference · purple AI − reference · zero is closest"];
    public static readonly (double XMin, double YMin, double XMax, double YMax) FullExtent = (2677090, 1251340, 2677360, 1251610);

    public static string Stats(TransectProfile? p) => p == null ? "No transect selected." :
        $"THIS TRANSECT ONLY\n\nLength: {p.Length:F1} m · {p.Samples.Count} samples\nActual spacing: {p.Spacing:F3} m\n\n" +
        $"Classical RMSE: {p.Rmse[0]:F2} m\nClassical MAE: {p.Mae[0]:F2} m\n\nAI RMSE: {p.Rmse[1]:F2} m\nAI MAE: {p.Mae[1]:F2} m\n\n" +
        "Lower error is better. These are line-sample errors, not full-area accuracy.";

    public static string Cursor(ProfileSample s) =>
        $"Distance: {s.Distance:F1} m\nE {s.X:F2} · N {s.Y:F2}\n\nDSM: {s.Heights[0]:F2} m\nClassical: {s.Heights[1]:F2} m\nAI: {s.Heights[2]:F2} m\nReference: {s.Heights[3]:F2} m";
}

/// <summary>Toolkit-neutral chart geometry: scale, ticks and series (drawn by GDI+ or WPF).</summary>
public sealed class ChartModel
{
    public required double Low { get; init; }
    public required double High { get; init; }
    public required int[] Bands { get; init; }

    public static ChartModel From(TransectProfile profile, bool errors)
    {
        int[] bands = errors ? [1, 2] : [0, 1, 2, 3];
        double low = errors ? 0 : double.MaxValue, high = errors ? 0 : double.MinValue;
        foreach (var s in profile.Samples)
            foreach (int band in bands)
            {
                double v = Value(s, band, errors);
                low = Math.Min(low, v);
                high = Math.Max(high, v);
            }
        double pad = Math.Max(.25, (high - low) * .12);
        return new ChartModel { Low = low - pad, High = high + pad, Bands = bands };
    }

    public static double Value(ProfileSample s, int band, bool errors) => s.Heights[band] - (errors ? s.Heights[3] : 0);
}
