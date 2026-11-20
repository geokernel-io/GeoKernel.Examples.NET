using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace GeoKernel.Examples.DsmToDtm;

/// <summary>A single-band north-up metre height raster with an explicit validity mask.</summary>
public sealed class TerrainRaster
{
    public required string Path { get; init; }
    public required string Projection { get; init; }
    public required string Sha256 { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required double Pixel { get; init; }
    public required double[] Transform { get; init; }
    public required float[] Z { get; init; }
    public required byte[] Valid { get; init; }

    public (double XMin, double YMin, double XMax, double YMax) Extent =>
        (Transform[0], Transform[3] - Height * Pixel, Transform[0] + Width * Pixel, Transform[3]);
}

/// <summary>The six user parameters of the classical filter.</summary>
public sealed record TerrainParameters(double Working = 2, double Window = 64, double Threshold = .4, double Slope = .12, double Terrain = .5, double Retain = .35)
{
    public JsonObject Json() => new()
    {
        ["requested_working_m"] = Working,
        ["maximum_window_m"] = Window,
        ["base_threshold_m"] = Threshold,
        ["slope_allowance"] = Slope,
        ["terrain_slope_factor"] = Terrain,
        ["retain_tolerance_m"] = Retain,
        ["roughness_scale_m"] = .6,
        ["maximum_base_threshold_m"] = 3.0,
    };
}

/// <summary>One completed estimate: Float32 DTM and removed heights, QA bits and statistics.</summary>
public sealed class TerrainResult
{
    public required TerrainParameters Parameters { get; init; }
    public required double Working { get; init; }
    public required double Seconds { get; init; }
    public required int Supports { get; init; }
    public required float[] Dtm { get; init; }
    public required float[] Removed { get; init; }
    public required byte[] Quality { get; init; }
    public required long Fallback { get; init; }
    public required JsonObject Summary { get; init; }
}

/// <summary>RMSE, MAE and signed bias of DTM − reference over shared valid cells.</summary>
public sealed record TerrainScore(long Count = 0, double Rmse = 0, double Mae = 0, double Bias = 0)
{
    public JsonObject Json() => new()
    {
        ["pixels"] = Count,
        ["rmse_m"] = Count > 0 ? Rmse : null,
        ["mae_m"] = Count > 0 ? Mae : null,
        ["bias_m"] = Count > 0 ? Bias : null,
    };
}

/// <summary>
/// Live classical DSM-to-DTM processing (method native-multiscale-morphological-tin-v1), a direct
/// port of the Qt DsmToDtmProcessing filter: block lower envelope keeping each minimum's real source
/// pixel, reflected-boundary morphological openings at increasing scales with a slope/relief/roughness
/// allowance, GDAL linear TIN gridding at the native pixel centres, retained source samples, a DSM upper
/// bound and QA bits. Float arithmetic follows the C++ code, so results are identical to the Qt build.
/// The reference is never an input to <see cref="Estimate"/>. All members may run on a worker thread.
/// </summary>
public static class Terrain
{
    public const float NoData = -9999f;
    public const string Method = "native-multiscale-morphological-tin-v1";
    public static readonly TerrainParameters[] Presets = [new(), new(1, 80, .25, .08, .25, .2), new(2, 48, .6, .16, .8, .4)];
    public static readonly string[] PresetNames = ["Balanced", "Remove more objects", "Preserve more relief"];

    // ----- GDAL bridge (the SDK-deployed gdal.dll) ---------------------------------------------------
    const string Lib = "gdal.dll";
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] delegate int GridProgress(double fraction, IntPtr message, IntPtr user);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern void GDALAllRegister();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALVersionInfo([MarshalAs(UnmanagedType.LPUTF8Str)] string request);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int access);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern void GDALClose(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetRasterXSize(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetRasterYSize(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetRasterCount(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGetRasterBand(IntPtr ds, int band);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGetMaskBand(IntPtr band);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern double GDALGetRasterScale(IntPtr band, out int present);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern double GDALGetRasterOffset(IntPtr band, out int present);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGetRasterUnitType(IntPtr band);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALSetRasterUnitType(IntPtr band, [MarshalAs(UnmanagedType.LPUTF8Str)] string units);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern double GDALGetRasterNoDataValue(IntPtr band, out int has);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALSetRasterNoDataValue(IntPtr band, double value);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALSetMetadataItem(IntPtr ds, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, [MarshalAs(UnmanagedType.LPUTF8Str)] string value, IntPtr domain);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALRasterIO(IntPtr band, int rw, int x, int y, int w, int h, float[] data, int bw, int bh, int type, int ps, int ls);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GDALRasterIO")] static extern int GDALRasterIOBytes(IntPtr band, int rw, int x, int y, int w, int h, byte[] data, int bw, int bh, int type, int ps, int ls);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetGeoTransform(IntPtr ds, [Out] double[] gt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALSetGeoTransform(IntPtr ds, double[] gt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGetProjectionRef(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALSetProjection(IntPtr ds, [MarshalAs(UnmanagedType.LPUTF8Str)] string wkt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGetDriverByName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALCreate(IntPtr driver, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, int w, int h, int bands, int type, IntPtr[] options);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr CPLGetLastErrorMsg();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr OSRNewSpatialReference(IntPtr wkt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern void OSRDestroySpatialReference(IntPtr srs);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int OSRSetFromUserInput(IntPtr srs, [MarshalAs(UnmanagedType.LPUTF8Str)] string definition);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int OSRIsSame(IntPtr a, IntPtr b);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int OSRIsProjected(IntPtr srs);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern double OSRGetLinearUnits(IntPtr srs, out IntPtr name);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGridContextCreate(int algorithm, ref LinearOptions options, uint count, double[] x, double[] y, double[] z, int keepArrays);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGridContextProcess(IntPtr context, double xMin, double xMax, double yMin, double yMax, uint width, uint height, int type, float[] data, GridProgress progress, IntPtr user);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern void GDALGridContextFree(IntPtr context);

    /// <summary>GDALGridLinearOptions, versioned by size since GDAL 3.6.</summary>
    [StructLayout(LayoutKind.Sequential)]
    struct LinearOptions { public nuint Size; public double Radius; public double NoData; }

    static readonly Lazy<bool> Registered = new(() =>
    {
        GDALAllRegister();
        if (!int.TryParse(Marshal.PtrToStringUTF8(GDALVersionInfo("VERSION_NUM")), out int version) || version < 3060000)
            throw new InvalidOperationException("The gridding bridge requires GDAL 3.6 or newer.");
        return true;
    });

    static Exception Error(string message) =>
        new InvalidOperationException($"{message} {Marshal.PtrToStringUTF8(CPLGetLastErrorMsg())}".Trim());

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }

    static bool MetreProjected(string projection)
    {
        IntPtr srs = OSRNewSpatialReference(IntPtr.Zero);
        try
        {
            return OSRSetFromUserInput(srs, projection) == 0 && OSRIsProjected(srs) != 0 && Math.Abs(OSRGetLinearUnits(srs, out _) - 1) < 1e-9;
        }
        finally { OSRDestroySpatialReference(srs); }
    }

    public static TerrainRaster Read(string path, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        _ = Registered.Value;
        IntPtr ds = GDALOpen(path, 0);
        if (ds == IntPtr.Zero) throw Error("Cannot open height raster.");
        int width, height;
        var t = new double[6];
        string projection;
        float[] z;
        byte[] mask;
        double noData;
        int hasNoData;
        try
        {
            Require(GDALGetRasterCount(ds) == 1, "Use a single-band height raster.");
            IntPtr band = GDALGetRasterBand(ds, 1);
            double scale = GDALGetRasterScale(band, out int present);
            Require(present == 0 || Math.Abs(scale - 1) < 1e-12, "Apply raster scale to metre heights before opening.");
            double offset = GDALGetRasterOffset(band, out present);
            Require(present == 0 || Math.Abs(offset) < 1e-12, "Apply raster offset before opening.");
            string units = (Marshal.PtrToStringUTF8(GDALGetRasterUnitType(band)) ?? "").ToLowerInvariant();
            Require(units is "" or "m" or "metre" or "meter" or "metres" or "meters", "Height units must be metres. Unspecified units are assumed to be metres.");
            width = GDALGetRasterXSize(ds);
            height = GDALGetRasterYSize(ds);
            bool hasTransform = GDALGetGeoTransform(ds, t) == 0;
            projection = Marshal.PtrToStringUTF8(GDALGetProjectionRef(ds)) ?? "";
            Require(MetreProjected(projection), "Use a projected CRS with metre XY units.");
            Require(hasTransform && t[2] == 0 && t[4] == 0 && t[1] > 0 && Math.Abs(t[1] + t[5]) < 1e-9, "Use a north-up raster with square pixels.");
            Require(width >= 8 && height >= 8 && (long)width * height <= 4_000_000, "This interactive example supports 8 x 8 to 4 million source cells.");
            z = new float[width * height];
            if (GDALRasterIO(band, 0, 0, 0, width, height, z, width, height, 6, 0, 0) != 0) throw Error("Cannot read complete heights.");
            noData = GDALGetRasterNoDataValue(band, out hasNoData);
            mask = new byte[width * height];
            if (GDALRasterIOBytes(GDALGetMaskBand(band), 0, 0, 0, width, height, mask, width, height, 1, 0, 0) != 0) throw Error("Cannot read source validity pixels.");
        }
        finally { GDALClose(ds); }
        var valid = new byte[z.Length];
        int count = 0;
        for (int i = 0; i < z.Length; i++)
        {
            bool ok = mask[i] != 0 && float.IsFinite(z[i]) && (hasNoData == 0 || z[i] != noData);
            valid[i] = ok ? (byte)1 : (byte)0;
            if (ok) count++;
        }
        Require(count >= 4 && count >= z.Length * .05, "Insufficient valid source coverage.");
        token.ThrowIfCancellationRequested();
        return new TerrainRaster
        {
            Path = System.IO.Path.GetFullPath(path), Projection = projection, Sha256 = SampleCache.HashFile(path), Width = width, Height = height,
            Pixel = t[1], Transform = [t[0], t[1], 0, t[3], 0, t[5]], Z = z, Valid = valid,
        };
    }

    /// <summary>True when two CRS definitions are equivalent (GDAL OSRIsSame).</summary>
    public static bool SameProjection(string first, string second)
    {
        IntPtr a = OSRNewSpatialReference(IntPtr.Zero), b = OSRNewSpatialReference(IntPtr.Zero);
        try { return OSRSetFromUserInput(a, first) == 0 && OSRSetFromUserInput(b, second) == 0 && OSRIsSame(a, b) != 0; }
        finally { OSRDestroySpatialReference(a); OSRDestroySpatialReference(b); }
    }

    public static void ValidateReference(TerrainRaster dsm, TerrainRaster reference)
    {
        bool same = SameProjection(dsm.Projection, reference.Projection);
        Require(same && dsm.Width == reference.Width && dsm.Height == reference.Height && dsm.Transform.SequenceEqual(reference.Transform),
            "Reference must share the DSM's exact dimensions, grid and equivalent CRS; no implicit resampling.");
    }

    /// <summary>Float32 heights (NoData −9999, metre units) or Byte QA (NoData 1) on the source grid.</summary>
    public static string Write(TerrainRaster raster, string path, float[]? heights, byte[]? quality = null)
    {
        _ = Registered.Value;
        int type = heights != null ? 6 : 1;
        IntPtr[] options = [Marshal.StringToCoTaskMemUTF8("COMPRESS=DEFLATE"), Marshal.StringToCoTaskMemUTF8("TILED=YES"), IntPtr.Zero];
        try
        {
            IntPtr ds = GDALCreate(GDALGetDriverByName("GTiff"), path, raster.Width, raster.Height, 1, type, options);
            if (ds == IntPtr.Zero) throw Error($"Cannot create GeoTIFF: {path}");
            try
            {
                if (GDALSetGeoTransform(ds, raster.Transform) != 0 || GDALSetProjection(ds, raster.Projection) != 0) throw Error("Cannot write raster coordinates.");
                GDALSetMetadataItem(ds, "GEOKERNEL_TERRAIN_METHOD", Method, IntPtr.Zero);
                IntPtr band = GDALGetRasterBand(ds, 1);
                GDALSetRasterNoDataValue(band, type == 1 ? 1 : NoData);
                if (type == 6) GDALSetRasterUnitType(band, "metre");
                int code = heights != null
                    ? GDALRasterIO(band, 1, 0, 0, raster.Width, raster.Height, heights, raster.Width, raster.Height, 6, 0, 0)
                    : GDALRasterIOBytes(band, 1, 0, 0, raster.Width, raster.Height, quality!, raster.Width, raster.Height, 1, 0, 0);
                if (code != 0) throw Error("Cannot write raster pixels.");
            }
            finally { GDALClose(ds); }
        }
        finally
        {
            Marshal.FreeCoTaskMem(options[0]);
            Marshal.FreeCoTaskMem(options[1]);
        }
        return path;
    }

    // ----- filter building blocks --------------------------------------------------------------------
    static int Round(double value) => (int)Math.Round(value, MidpointRounding.AwayFromZero);

    static int Reflect(int index, int count)
    {
        while (index < 0 || index >= count)
            index = index < 0 ? -index - 1 : 2 * count - index - 1;
        return index;
    }

    /// <summary>Separable sliding minimum/maximum over (2r+1)² with reflected boundaries (monotonic deque).</summary>
    static float[] Extremes(float[] input, int width, int height, int radius, bool minimum, CancellationToken token)
    {
        var horizontal = new float[input.Length];
        var output = new float[input.Length];
        int capacity = Math.Max(width, height) + 2 * radius + 1;
        var keys = new int[capacity];
        var values = new float[capacity];
        void Pass(float[] source, float[] target, bool vertical)
        {
            int lines = vertical ? width : height, length = vertical ? height : width;
            for (int line = 0; line < lines; line++)
            {
                token.ThrowIfCancellationRequested();
                int head = 0, tail = 0;
                for (int k = -radius; k < length + radius; k++)
                {
                    int at = Reflect(k, length);
                    float value = source[vertical ? at * width + line : line * width + at];
                    while (tail > head && (minimum ? values[tail - 1] >= value : values[tail - 1] <= value)) tail--;
                    keys[tail] = k;
                    values[tail++] = value;
                    while (keys[head] < k - radius * 2) head++;
                    int destination = k - radius;
                    if (destination >= 0) target[vertical ? destination * width + line : line * width + destination] = values[head];
                }
            }
        }
        Pass(input, horizontal, false);
        Pass(horizontal, output, true);
        return output;
    }

    static float[] Gaussian(float[] input, int width, int height, double sigma, CancellationToken token)
    {
        int radius = (int)Math.Ceiling(3 * sigma);
        var kernel = new double[radius * 2 + 1];
        double sum = 0;
        for (int k = -radius; k <= radius; k++)
            sum += kernel[k + radius] = Math.Exp(-.5 * k * k / (sigma * sigma));
        for (int k = 0; k < kernel.Length; k++) kernel[k] /= sum;
        var temporary = new float[input.Length];
        var output = new float[input.Length];
        for (int y = 0; y < height; y++)
        {
            token.ThrowIfCancellationRequested();
            for (int x = 0; x < width; x++)
            {
                double value = 0;
                for (int k = -radius; k <= radius; k++) value += input[y * width + Reflect(x + k, width)] * kernel[k + radius];
                temporary[y * width + x] = (float)value;
            }
        }
        for (int y = 0; y < height; y++)
        {
            token.ThrowIfCancellationRequested();
            for (int x = 0; x < width; x++)
            {
                double value = 0;
                for (int k = -radius; k <= radius; k++) value += temporary[Reflect(y + k, height) * width + x] * kernel[k + radius];
                output[y * width + x] = (float)value;
            }
        }
        return output;
    }

    static float[] Gradient(float[] z, int width, int height, double pixel)
    {
        var result = new float[z.Length];
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int l = Math.Max(0, x - 1), r = Math.Min(width - 1, x + 1), t = Math.Max(0, y - 1), b = Math.Min(height - 1, y + 1);
                float dx = z[y * width + r] - z[y * width + l], dy = z[b * width + x] - z[t * width + x];
                result[y * width + x] = (float)double.Hypot(dx / ((r - l) * pixel), dy / ((b - t) * pixel));
            }
        return result;
    }

    /// <summary>Four-neighbour propagation of the nearest support index (fills holes and hull fallbacks).</summary>
    public static int[] SupportMap(byte[] valid, int width, int height, CancellationToken token)
    {
        var nearest = new int[valid.Length];
        Array.Fill(nearest, -1);
        var queue = new List<int>(valid.Length);
        for (int i = 0; i < valid.Length; i++)
            if (valid[i] != 0) { nearest[i] = i; queue.Add(i); }
        Require(queue.Count > 0, "No support cells.");
        for (int head = 0; head < queue.Count; head++)
        {
            if (head % 4096 == 0) token.ThrowIfCancellationRequested();
            int i = queue[head], x = i % width, y = i / width;
            foreach (int n in (ReadOnlySpan<int>)[x > 0 ? i - 1 : -1, x + 1 < width ? i + 1 : -1, y > 0 ? i - width : -1, y + 1 < height ? i + width : -1])
                if (n >= 0 && nearest[n] < 0) { nearest[n] = nearest[i]; queue.Add(n); }
        }
        return nearest;
    }

    static float[] Triangulate(TerrainRaster raster, double[] x, double[] y, double[] z, CancellationToken token, Action<int, string> progress)
    {
        _ = Registered.Value;
        var options = new LinearOptions { Size = (nuint)Marshal.SizeOf<LinearOptions>(), Radius = 0, NoData = NoData };
        token.ThrowIfCancellationRequested();
        progress(62, $"Triangulating {z.Length} ground supports...");
        IntPtr context = GDALGridContextCreate(10, ref options, (uint)z.Length, x, y, z, 1);
        token.ThrowIfCancellationRequested();
        if (context == IntPtr.Zero) throw new InvalidOperationException("GDAL could not triangulate ground supports; check candidate distribution and SDK Qhull support.");
        var result = new float[raster.Width * raster.Height];
        Array.Fill(result, NoData);
        int status;
        GridProgress tick = (fraction, _, _) =>
        {
            progress(65 + (int)(fraction * 16), "Interpolating native source pixel centres...");
            return token.IsCancellationRequested ? 0 : 1;
        };
        try
        {
            // Local x = column metres, y = row metres; output GeoTIFFs keep the real CRS.
            status = GDALGridContextProcess(context, 0, raster.Width * raster.Pixel, 0, raster.Height * raster.Pixel, (uint)raster.Width, (uint)raster.Height, 6, result, tick, IntPtr.Zero);
            GC.KeepAlive(tick);
            GC.KeepAlive(x); GC.KeepAlive(y); GC.KeepAlive(z);
        }
        finally { GDALGridContextFree(context); }
        token.ThrowIfCancellationRequested();
        if (status != 0) throw new InvalidOperationException("TIN interpolation failed.");
        return result;
    }

    public static TerrainResult Estimate(TerrainRaster dsm, TerrainParameters p, CancellationToken token, Action<int, string> progress)
    {
        var elapsed = Stopwatch.StartNew();
        int factor = Math.Max(1, Round(p.Working / dsm.Pixel));
        double working = factor * dsm.Pixel;
        Require(p.Window >= 2 * working, "Maximum window must span at least two working cells.");
        int width = (dsm.Width + factor - 1) / factor, height = (dsm.Height + factor - 1) / factor;
        Require(width >= 3 && height >= 3 && (long)width * height <= 262144, "Working grid must have at least 3 x 3 and at most 262144 cells. Adjust working resolution.");
        int count = width * height;
        var low = new float[count];
        Array.Fill(low, float.PositiveInfinity);
        var minima = new int[count];
        Array.Fill(minima, -1);
        var valid = new byte[count];
        for (int y = 0; y < dsm.Height; y++)
        {
            token.ThrowIfCancellationRequested();
            for (int x = 0; x < dsm.Width; x++)
            {
                int source = y * dsm.Width + x, cell = (y / factor) * width + x / factor;
                if (dsm.Valid[source] != 0 && dsm.Z[source] < low[cell])
                {
                    low[cell] = dsm.Z[source];
                    minima[cell] = source;
                    valid[cell] = 1;
                }
            }
        }
        int[] fill = SupportMap(valid, width, height, token);
        for (int i = 0; i < count; i++)
            if (valid[i] == 0) low[i] = low[fill[i]];
        byte[] candidates = (byte[])valid.Clone();
        int RadiusFor(double window) => Math.Max(1, Round(window / working) / 2);
        float[] Opening(float[] z, int radius) => Extremes(Extremes(z, width, height, radius, true, token), width, height, radius, false, token);

        progress(15, "Estimating DSM-derived slope and roughness...");
        float[] envelope = Opening(low, RadiusFor(p.Window));
        float[] terrainSlope = Gaussian(Gradient(envelope, width, height, working), width, height, 1, token);
        float[] smoothed = Gaussian(low, width, height, 1, token);
        float[] localSlope = Extremes(Gradient(smoothed, width, height, working), width, height, 2, false, token);
        var highPass = new float[count];
        for (int i = 0; i < count; i++) highPass[i] = (low[i] - smoothed[i]) * (low[i] - smoothed[i]);
        float[] variance = Gaussian(highPass, width, height, 2, token);
        var windows = new List<double>();
        for (double window = Math.Max(working * 2, 4); window < p.Window; window *= 2) windows.Add(window);
        windows.Add(p.Window);
        for (int step = 0; step < windows.Count; step++)
        {
            double window = windows[step];
            progress(20 + (int)(35.0 * step / windows.Count), $"Ground candidates: {window:0.##} m window");
            float[] opened = Opening(low, RadiusFor(window));
            float[] slope = Extremes(terrainSlope, width, height, RadiusFor(window), false, token);
            for (int i = 0; i < count; i++)
            {
                double weight = 1 / (1 + Math.Pow(MathF.Sqrt(Math.Max(0f, variance[i])) / .6, 4));
                double threshold = Math.Min(3, p.Threshold + p.Slope * window) + p.Terrain * window * Math.Min(slope[i], localSlope[i]) * weight;
                candidates[i] = candidates[i] != 0 && low[i] - opened[i] <= threshold ? (byte)1 : (byte)0;
            }
        }
        var xs = new List<double>();
        var ys = new List<double>();
        var zs = new List<double>();
        for (int i = 0; i < count; i++)
            if (candidates[i] != 0)
            {
                int source = minima[i];
                xs.Add((source % dsm.Width + .5) * dsm.Pixel);
                ys.Add((source / dsm.Width + .5) * dsm.Pixel);
                zs.Add(dsm.Z[source]);
            }
        Require(zs.Count >= 4, "Not enough ground supports; relax parameters.");
        int[] nearest = SupportMap(candidates, width, height, token);
        float[] dtm = Triangulate(dsm, [.. xs], [.. ys], [.. zs], token, progress);
        var removed = new float[dsm.Z.Length];
        var quality = new byte[dsm.Z.Length];
        long retained = 0, fallback = 0, clamped = 0, borderCount = 0;
        int border = (int)Math.Ceiling(p.Window * .5 / dsm.Pixel);
        for (int row = 0; row < dsm.Height; row++)
        {
            token.ThrowIfCancellationRequested();
            for (int col = 0; col < dsm.Width; col++)
            {
                int i = row * dsm.Width + col, cell = (row / factor) * width + col / factor;
                byte qa = 0;
                if (dsm.Valid[i] == 0)
                {
                    dtm[i] = removed[i] = NoData;
                    quality[i] = 1;
                    continue;
                }
                if (dtm[i] == NoData || !float.IsFinite(dtm[i]))
                {
                    dtm[i] = low[nearest[cell]];
                    qa |= 4;
                    fallback++;
                }
                if (candidates[cell] != 0 && Math.Abs(dsm.Z[i] - dtm[i]) <= p.Retain)
                {
                    dtm[i] = dsm.Z[i];
                    retained++;
                }
                else qa |= 2;
                if (dtm[i] > dsm.Z[i])
                {
                    dtm[i] = dsm.Z[i];
                    qa |= 16;
                    clamped++;
                }
                if (row < border || col < border || row >= dsm.Height - border || col >= dsm.Width - border)
                {
                    qa |= 8;
                    borderCount++;
                }
                removed[i] = Math.Max(0f, dsm.Z[i] - dtm[i]);
                quality[i] = qa;
            }
        }
        double seconds = elapsed.Elapsed.TotalSeconds;
        var summary = new JsonObject
        {
            ["method"] = Method,
            ["effective_working_m"] = working,
            ["support_points"] = zs.Count,
            ["retained_source_pixels"] = retained,
            ["outside_hull_fallback_pixels"] = fallback,
            ["upper_bound_clamped_pixels"] = clamped,
            ["edge_sensitive_pixels"] = borderCount,
            ["edge_band_m"] = p.Window * .5,
            ["compute_seconds"] = seconds,
        };
        progress(84, "Preparing numeric GeoTIFFs and display maps...");
        return new TerrainResult
        {
            Parameters = p, Working = working, Seconds = seconds, Supports = zs.Count, Dtm = dtm, Removed = removed, Quality = quality,
            Fallback = fallback, Summary = summary,
        };
    }

    // ----- displays, scores and report ---------------------------------------------------------------
    static readonly (byte R, byte G, byte B) NoDataGray = (0x75, 0x81, 0x8C);

    static (byte R, byte G, byte B) Rgb(string hex)
    {
        int v = Convert.ToInt32(hex.TrimStart('#'), 16);
        return ((byte)(v >> 16), (byte)((v >> 8) & 0xff), (byte)(v & 0xff));
    }
    static readonly (byte R, byte G, byte B) White = Rgb("#fafafa"), RemovedRed = Rgb("#bc3e31"), Below = Rgb("#2464b5"), Above = Rgb("#c93645"),
        Fallback = Rgb("#a743a6"), Edge = Rgb("#e8ad49"), Interpolated = Rgb("#edf3f5"), Retained = Rgb("#15969b");

    static (byte R, byte G, byte B) Blend((byte R, byte G, byte B) a, (byte R, byte G, byte B) b, double t)
    {
        t = Math.Clamp(t, 0, 1);
        return ((byte)Round(a.R + (b.R - a.R) * t), (byte)Round(a.G + (b.G - a.G) * t), (byte)Round(a.B + (b.B - a.B) * t));
    }

    /// <summary>Hillshade (azimuth 315°, altitude 40°) on true pixel spacing; NoData gray.</summary>
    public static (byte[] R, byte[] G, byte[] B) Relief(TerrainRaster raster, float[] z, CancellationToken token)
    {
        int w = raster.Width, h = raster.Height;
        var r = new byte[w * h];
        var g = new byte[w * h];
        var b = new byte[w * h];
        double az = 315 * 3.141592653589793 / 180, alt = 40 * 3.141592653589793 / 180;
        for (int y = 0; y < h; y++)
        {
            token.ThrowIfCancellationRequested();
            for (int x = 0; x < w; x++)
            {
                int i = y * w + x;
                if (raster.Valid[i] == 0) { (r[i], g[i], b[i]) = NoDataGray; continue; }
                float here = z[i];
                float At(int xx, int yy) { int k = yy * w + xx; return raster.Valid[k] != 0 ? z[k] : here; }
                int l = Math.Max(0, x - 1), rr = Math.Min(w - 1, x + 1), t = Math.Max(0, y - 1), bt = Math.Min(h - 1, y + 1);
                double dx = (At(rr, y) - At(l, y)) / ((rr - l) * raster.Pixel), dy = (At(x, t) - At(x, bt)) / ((bt - t) * raster.Pixel);
                double light = (-dx * Math.Sin(az) * Math.Cos(alt) - dy * Math.Cos(az) * Math.Cos(alt) + Math.Sin(alt)) / Math.Sqrt(dx * dx + dy * dy + 1);
                byte gray = (byte)Math.Clamp(Round(255 * (.25 + .75 * Math.Max(0, light))), 0, 255);
                r[i] = g[i] = b[i] = gray;
            }
        }
        return (r, g, b);
    }

    /// <summary>Mode 0 removed height (0–20 m), 1 support/edge quality, 2 signed reference error (−2..+2 m).</summary>
    public static (byte[] R, byte[] G, byte[] B) Diagnostic(TerrainRaster dsm, TerrainResult result, int mode, TerrainRaster? reference, CancellationToken token)
    {
        int n = dsm.Width * dsm.Height;
        var r = new byte[n];
        var g = new byte[n];
        var b = new byte[n];
        for (int y = 0; y < dsm.Height; y++)
        {
            token.ThrowIfCancellationRequested();
            for (int i = y * dsm.Width; i < (y + 1) * dsm.Width; i++)
            {
                var color = NoDataGray;
                if (dsm.Valid[i] != 0)
                {
                    if (mode == 0) color = Blend(White, RemovedRed, result.Removed[i] / 20.0);
                    else if (mode == 1)
                    {
                        byte qa = result.Quality[i];
                        color = (qa & 4) != 0 ? Fallback : (qa & 8) != 0 ? Edge : (qa & 2) != 0 ? Interpolated : Retained;
                    }
                    else if (reference != null && reference.Valid[i] != 0)
                    {
                        double error = (double)result.Dtm[i] - reference.Z[i];
                        color = error < 0 ? Blend(White, Below, -error / 2) : Blend(White, Above, error / 2);
                    }
                }
                (r[i], g[i], b[i]) = color;
            }
        }
        return (r, g, b);
    }

    public static TerrainScore Score(TerrainRaster dsm, float[] result, TerrainRaster? reference, int border = 0)
    {
        if (reference == null) return new TerrainScore();
        long count = 0;
        double squared = 0, absolute = 0, signed = 0;
        for (int y = border; y < dsm.Height - border; y++)
            for (int x = border; x < dsm.Width - border; x++)
            {
                int i = y * dsm.Width + x;
                if (dsm.Valid[i] == 0 || reference.Valid[i] == 0) continue;
                double e = (double)result[i] - reference.Z[i];
                count++;
                squared += e * e;
                absolute += Math.Abs(e);
                signed += e;
            }
        return count == 0 ? new TerrainScore() : new TerrainScore(count, Math.Sqrt(squared / count), absolute / count, signed / count);
    }

    /// <summary>Scores over all shared valid cells and outside the edge band (half the maximum window).</summary>
    public static (TerrainScore All, TerrainScore Interior) Evaluate(TerrainRaster dsm, TerrainResult result, TerrainRaster? reference) =>
        (Score(dsm, result.Dtm, reference), Score(dsm, result.Dtm, reference, (int)Math.Ceiling(result.Parameters.Window * .5 / dsm.Pixel)));

    public static JsonObject Report(TerrainRaster dsm, TerrainRaster? reference, JsonObject manifest, TerrainResult result, TerrainScore all, TerrainScore interior) => new()
    {
        ["schema_version"] = 1,
        ["method"] = Method,
        ["parameters"] = result.Parameters.Json(),
        ["summary"] = result.Summary.DeepClone(),
        ["source_dsm"] = dsm.Path,
        ["source_sha256"] = dsm.Sha256,
        ["reference"] = reference?.Path,
        ["reference_sha256"] = reference?.Sha256,
        ["reference_used_for_processing"] = false,
        ["grid"] = new JsonObject
        {
            ["width"] = dsm.Width,
            ["height"] = dsm.Height,
            ["geotransform"] = new JsonArray(dsm.Transform.Select(v => (JsonNode)v).ToArray()),
            ["crs_wkt"] = dsm.Projection,
        },
        ["source_manifest"] = manifest.DeepClone(),
        ["all_valid_reference_error"] = all.Json(),
        ["interior_reference_error"] = interior.Json(),
        ["edge_band_definition"] = "Half the requested maximum window, rounded up to source cells",
        ["quality_bits"] = new JsonObject
        {
            ["1"] = "Source NoData",
            ["2"] = "Interpolated (not explicitly retained source sample)",
            ["4"] = "Outside TIN hull: grid-support fallback",
            ["8"] = "Edge-sensitive band",
            ["16"] = "Prediction clamped to DSM",
        },
        ["height_units"] = "metre; unspecified source units assumed metres; no vertical datum conversion",
        ["processing_scope"] = "This input raster only. Supply surrounding DSM context and crop afterward to reduce boundary effects.",
    };

    /// <summary>Copy through a temporary sibling, flushed to disk, then replace the target.</summary>
    public static void AtomicCopy(string source, string target, CancellationToken token)
    {
        string temporary = System.IO.Path.Combine(System.IO.Path.GetDirectoryName(System.IO.Path.GetFullPath(target))!, $".{System.IO.Path.GetFileName(target)}.{Environment.ProcessId}.tmp");
        try
        {
            using (var reader = File.OpenRead(source))
            using (var writer = new FileStream(temporary, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                var block = new byte[1 << 20];
                int read;
                while ((read = reader.Read(block, 0, block.Length)) > 0)
                {
                    token.ThrowIfCancellationRequested();
                    writer.Write(block, 0, read);
                }
                writer.Flush(true);
            }
            token.ThrowIfCancellationRequested();
            File.Move(temporary, target, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    public static void WriteJson(string path, JsonObject value)
    {
        string temporary = path + $".{Environment.ProcessId}.tmp";
        try
        {
            File.WriteAllText(temporary, value.ToJsonString(new JsonSerializerOptions { WriteIndented = true }), new UTF8Encoding(false));
            File.Move(temporary, path, true);
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    public static bool SameFile(string a, string b) =>
        string.Equals(System.IO.Path.GetFullPath(a), System.IO.Path.GetFullPath(b), StringComparison.OrdinalIgnoreCase);
}

/// <summary>User-facing text shared by the DsmToDtmProcessing windows.</summary>
public static class TerrainText
{
    public static readonly string[] DiagnosticNames = ["Removed height: DSM − DTM", "Support / edge quality", "Signed reference error"];
    public static readonly string[] DiagnosticTitles = ["REMOVED HEIGHT · DSM − DTM", "PROCESSING QUALITY · supports / edges", "REFERENCE ERROR · DTM − reference"];
    public static readonly string[] Legends =
    [
        "Removed height: white 0 m → dark red ≥20 m. Display saturation only. NoData gray.",
        "Quality: teal retained source · pale interpolated · orange edge-sensitive · purple hull fallback · gray NoData. Full flag combinations are saved in QA GeoTIFF.",
        "Signed error: blue ≤−2 m · white 0 · red ≥+2 m. Shared fixed scale; numeric scores are not clipped.",
    ];
    public const string ControlsChanged = "Controls changed. Run to update; displayed result and scores still use the last completed run.";
    public const string EdgeNote = "Edge-sensitive band = half the maximum filter window. Use a larger DSM and crop afterward for better context. This prototype is independent of ArcGIS Terrain Filter.";

    static string G(double v) => v.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);

    public static string Summary(TerrainResult r)
    {
        var p = r.Parameters;
        return $"Last completed run · {r.Supports} supports · {r.Seconds.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} s · working {G(r.Working)} m · " +
               $"max window {G(p.Window)} m · base {G(p.Threshold)} m · slope {G(p.Slope)} · relief {G(p.Terrain)} · keep {G(p.Retain)} m\n" +
               $"Edge band {G(p.Window * .5)} m · hull fallback {r.Fallback} pixels · reference is evaluation-only.";
    }

    public static string Metrics(TerrainScore all, TerrainScore interior, bool hasReference)
    {
        if (!hasReference) return "No reference attached. Scores are unavailable; processing does not require a reference.";
        static string Line(string name, TerrainScore s) => s.Count > 0
            ? string.Create(System.Globalization.CultureInfo.InvariantCulture, $"{name} ({s.Count} px)\nRMSE {s.Rmse:0.000} m · MAE {s.Mae:0.000} m · bias {s.Bias:0.000} m")
            : $"{name}: no common valid reference pixels";
        return Line("All valid reference cells", all) + "\n\n" + Line("Interior outside edge band", interior);
    }

    public static string Source(TerrainRaster dsm, bool hasReference) =>
        $"{Path.GetFileName(dsm.Path)}\n{dsm.Width} × {dsm.Height} · {G(dsm.Pixel)} m cells\n" +
        (hasReference ? "Aligned reference attached" : "No reference: heights can still be processed");

    /// <summary>Probe text of the source cell containing a world point, or null outside the raster.</summary>
    public static (string Text, double X, double Y)? Probe(TerrainRaster dsm, TerrainResult? result, TerrainRaster? reference, double x, double y)
    {
        double cx = (x - dsm.Transform[0]) / dsm.Pixel, cy = (dsm.Transform[3] - y) / dsm.Pixel;
        if (cx < 0 || cy < 0 || cx >= dsm.Width || cy >= dsm.Height) return null;
        int col = (int)cx, row = (int)cy, i = row * dsm.Width + col;
        var inv = System.Globalization.CultureInfo.InvariantCulture;
        var text = new StringBuilder($"Source pixel · {col} / {row}\n");
        if (dsm.Valid[i] == 0) text.Append("NoData");
        else
        {
            text.Append(inv, $"DSM {dsm.Z[i]:0.00} m");
            if (result != null) text.Append(inv, $"\nDTM {result.Dtm[i]:0.00} m · removed {result.Removed[i]:0.00} m\nQA bits: {result.Quality[i]}");
            if (reference != null && reference.Valid[i] != 0)
            {
                text.Append(inv, $"\nReference {reference.Z[i]:0.00} m");
                if (result != null) text.Append(inv, $" · error {(double)result.Dtm[i] - reference.Z[i]:0.000} m");
            }
        }
        return (text.ToString(), dsm.Transform[0] + (col + .5) * dsm.Pixel, dsm.Transform[3] - (row + .5) * dsm.Pixel);
    }
}

/// <summary>A loaded source: DSM, optional evaluation-only reference and its relief display.</summary>
public sealed record TerrainScene(TerrainRaster Dsm, TerrainRaster? Reference, string Original, JsonObject Manifest);

/// <summary>One completed run: numeric GeoTIFFs, display maps, scores and the provenance report.</summary>
public sealed record TerrainRun(TerrainResult Terrain, string Dtm, string Removed, string Quality, string Display, string[] Diagnostics,
    TerrainScore All, TerrainScore Interior, JsonObject Report);

/// <summary>Worker-thread jobs of the DsmToDtmProcessing windows; each writes into its own session folder.</summary>
public static class TerrainJobs
{
    static string Rgb(TerrainRaster d, string path, (byte[] R, byte[] G, byte[] B) bands) =>
        Gdal.WriteRgb(path, d.Width, d.Height, d.Transform, d.Projection, bands.R, bands.G, bands.B);

    public static TerrainScene Load(string dsmPath, string? referencePath, JsonObject manifest, string folder, CancellationToken token, Action<int, string> progress)
    {
        progress(5, "Reading source DSM...");
        var dsm = Terrain.Read(dsmPath, token);
        TerrainRaster? reference = null;
        if (referencePath != null)
        {
            progress(30, "Reading evaluation-only reference...");
            reference = Terrain.Read(referencePath, token);
            Terrain.ValidateReference(dsm, reference);
        }
        progress(60, "Preparing original DSM relief...");
        string original = Rgb(dsm, Path.Combine(folder, "original.tif"), Terrain.Relief(dsm, dsm.Z, token));
        token.ThrowIfCancellationRequested();
        return new TerrainScene(dsm, reference, original, manifest);
    }

    static TerrainRun Scored(TerrainScene scene, TerrainRun run)
    {
        var (all, interior) = Terrain.Evaluate(scene.Dsm, run.Terrain, scene.Reference);
        return run with { All = all, Interior = interior, Report = Terrain.Report(scene.Dsm, scene.Reference, scene.Manifest, run.Terrain, all, interior) };
    }

    /// <summary>Attach an aligned reference; the generated DTM is not recomputed.</summary>
    public static (TerrainScene Scene, TerrainRun? Run) AttachReference(TerrainScene scene, TerrainRun? run, string path, string folder, CancellationToken token, Action<int, string> progress)
    {
        progress(10, "Reading evaluation-only reference...");
        var reference = Terrain.Read(path, token);
        Terrain.ValidateReference(scene.Dsm, reference);
        var next = scene with { Reference = reference };
        if (run == null) return (next, null);
        string[] diagnostics = (string[])run.Diagnostics.Clone();
        diagnostics[2] = Rgb(scene.Dsm, Path.Combine(folder, "reference-error.tif"), Terrain.Diagnostic(scene.Dsm, run.Terrain, 2, reference, token));
        return (next, Scored(next, run with { Diagnostics = diagnostics }));
    }

    public static TerrainRun Process(TerrainScene scene, TerrainParameters parameters, string folder, CancellationToken token, Action<int, string> progress)
    {
        var d = scene.Dsm;
        var terrain = Terrain.Estimate(d, parameters, token, progress);
        string dtm = Terrain.Write(d, Path.Combine(folder, "dtm.tif"), terrain.Dtm);
        token.ThrowIfCancellationRequested();
        string removed = Terrain.Write(d, Path.Combine(folder, "removed.tif"), terrain.Removed);
        token.ThrowIfCancellationRequested();
        string quality = Terrain.Write(d, Path.Combine(folder, "quality.tif"), null, terrain.Quality);
        string display = Rgb(d, Path.Combine(folder, "terrain-relief.tif"), Terrain.Relief(d, terrain.Dtm, token));
        string[] diagnostics = [.. Enumerable.Range(0, 3).Select(m => Rgb(d, Path.Combine(folder, $"diagnostic-{m}.tif"), Terrain.Diagnostic(d, terrain, m, scene.Reference, token)))];
        progress(96, "Scoring reference after processing...");
        var run = Scored(scene, new TerrainRun(terrain, dtm, removed, quality, display, diagnostics, new(), new(), new JsonObject()));
        token.ThrowIfCancellationRequested();
        return run;
    }

    /// <summary>DTM, removed heights, QA and JSON names for a chosen <c>.tif</c> result.</summary>
    public static string[] Destinations(string target)
    {
        if (!target.EndsWith(".tif", StringComparison.OrdinalIgnoreCase)) target += ".tif";
        string stem = target[..^4];
        return [target, stem + ".removed.tif", stem + ".qa.tif", stem + ".json"];
    }

    public static bool UsesSource(TerrainScene scene, string[] destinations) =>
        destinations.Any(d => Terrain.SameFile(d, scene.Dsm.Path) || (scene.Reference != null && Terrain.SameFile(d, scene.Reference.Path)));

    /// <summary>Each file is replaced atomically; a failure names the files already committed.</summary>
    public static void Save(TerrainRun run, string[] destinations, CancellationToken token, Action<int, string> progress)
    {
        var saved = new List<string>();
        try
        {
            string[] sources = [run.Dtm, run.Removed, run.Quality];
            for (int i = 0; i < 3; i++)
            {
                progress(i * 30, $"Saving {Path.GetFileName(destinations[i])}");
                Terrain.AtomicCopy(sources[i], destinations[i], token);
                saved.Add(destinations[i]);
            }
            token.ThrowIfCancellationRequested();
            Terrain.WriteJson(destinations[3], run.Report);
        }
        catch (Exception e) when (saved.Count > 0 && e is not OperationCanceledException)
        {
            throw new IOException($"{e.Message}\nFiles already committed:\n{string.Join("\n", saved)}", e);
        }
    }
}
