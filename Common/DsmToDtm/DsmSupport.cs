using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text.Json.Nodes;

namespace GeoKernel.Examples.DsmToDtm;

/// <summary>Command-line options shared by the DSM-to-DTM examples.</summary>
public sealed record DsmOptions(string? SampleArchive, string? SampleUrl, string? DataDirectory)
{
    public static DsmOptions Parse(string[] args)
    {
        string? archive = null, url = null, data = null;
        for (int i = 0; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"Missing value for {args[i]}.");
            switch (args[i])
            {
                case "--sample-archive": archive = Next(); break;
                case "--sample-url": url = Next(); break;
                case "--data-dir": data = Next(); break;
                default: throw new ArgumentException($"Unknown option {args[i]}. Use --sample-archive, --sample-url or --data-dir.");
            }
        }
        return new(archive, url, data);
    }

    public string DataRoot => DataDirectory != null ? Path.GetFullPath(DataDirectory) : Path.Combine(AppContext.BaseDirectory, "data");
}

public sealed class SampleAssets
{
    public required string Url { get; init; }
    public required string Archive { get; init; }
    public required string Cache { get; init; }
    public required string Sha256 { get; init; }
    public required IReadOnlyDictionary<string, string> Files { get; init; }

    /// <summary>The pinned Zurich v2 sample, compiled into the example as a resource.</summary>
    public static SampleAssets ZurichV2() => FromJson(Resource("sample-assets.json"));

    /// <summary>A JSON manifest embedded from Common/DsmToDtm.</summary>
    public static JsonObject Resource(string name)
    {
        using var stream = typeof(SampleAssets).Assembly.GetManifestResourceStream("GeoKernel.Examples.DsmToDtm." + name)
            ?? throw new InvalidOperationException($"Embedded {name} is missing.");
        return JsonNode.Parse(stream)!.AsObject();
    }

    /// <summary>A pinned archive manifest: URL, archive name, cache folder, archive and per-file SHA-256.</summary>
    public static SampleAssets FromJson(JsonObject json) => new()
    {
        Url = (string)json["url"]!,
        Archive = (string)json["archive"]!,
        Cache = (string)json["cache"]!,
        Sha256 = (string)json["sha256"]!,
        Files = json["files"]!.AsObject().ToDictionary(p => p.Key, p => (string)p.Value!),
    };
}

/// <summary>Pinned sample acquisition: archive SHA-256 plus every extracted file.</summary>
public static class SampleCache
{
    public static string HashFile(string path)
    {
        try
        {
            using var stream = File.OpenRead(path);
            return Convert.ToHexStringLower(SHA256.HashData(stream));
        }
        catch (IOException) { return ""; }
        catch (UnauthorizedAccessException) { return ""; }
    }

    public static bool Valid(string folder, SampleAssets assets) =>
        assets.Files.Count > 0 && assets.Files.All(f => HashFile(Path.Combine(folder, f.Key)) == f.Value);

    public const string SampleHint = "The sample ZIP must be uploaded to the SampleData v1 release. Use Retry after upload.";

    public static async Task<string> PrepareAsync(SampleAssets assets, DsmOptions options, IProgress<(int Percent, string Message)> progress, CancellationToken token,
        string hint = SampleHint)
    {
        string root = options.DataRoot;
        Directory.CreateDirectory(root);
        string folder = Path.Combine(root, assets.Cache);
        progress.Report((0, "Checking cached sample files..."));
        if (Valid(folder, assets)) return folder;

        string archive = options.SampleArchive != null ? Path.GetFullPath(options.SampleArchive) : Path.Combine(root, assets.Archive);
        if (options.SampleArchive == null && HashFile(archive) != assets.Sha256)
            await DownloadAsync(new Uri(options.SampleUrl ?? assets.Url), archive, progress, token, hint: hint);
        token.ThrowIfCancellationRequested();
        if (HashFile(archive) != assets.Sha256)
            throw new InvalidDataException("Sample ZIP SHA-256 mismatch. The existing data was retained.");

        string staging = Path.Combine(root, ".extract-" + Guid.NewGuid().ToString("N"));
        try
        {
            progress.Report((0, "Extracting verified sample ZIP..."));
            await Task.Run(() => ZipFile.ExtractToDirectory(archive, staging), token);
            token.ThrowIfCancellationRequested();
            if (!Valid(staging, assets))
                throw new InvalidDataException("Extracted sample files failed SHA-256 verification.");
            if (Directory.Exists(folder))
                Directory.Move(folder, $"{folder}.invalid-{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}");
            Directory.Move(staging, folder);
        }
        finally
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
        }
        return folder;
    }

    /// <summary>HTTPS download through a <c>.part</c> sibling that replaces the destination when complete.</summary>
    public static async Task DownloadAsync(Uri url, string destination, IProgress<(int, string)> progress, CancellationToken token,
        string label = "Downloading sample data...", string hint = SampleHint)
    {
        if (url.Scheme != Uri.UriSchemeHttps) throw new ArgumentException("Download URL must use HTTPS.");
        string partial = destination + ".part";
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
            client.DefaultRequestHeaders.UserAgent.ParseAdd("GeoKernel-DsmToDtm-NET/1.0");
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, token);
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"HTTP {(int)response.StatusCode}");
            long total = response.Content.Headers.ContentLength ?? 0, done = 0;
            await using (var source = await response.Content.ReadAsStreamAsync(token))
            await using (var file = File.Create(partial))
            {
                var buffer = new byte[256 * 1024];
                int read;
                while ((read = await source.ReadAsync(buffer, token)) > 0)
                {
                    await file.WriteAsync(buffer.AsMemory(0, read), token);
                    done += read;
                    progress.Report((total > 0 ? (int)(100 * done / total) : 0, label));
                }
            }
            File.Move(partial, destination, true);
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { File.Delete(partial); throw; }
        catch (Exception e)
        {
            File.Delete(partial);
            throw new InvalidOperationException($"Sample download failed: {e.Message}\n{url}\n{hint}", e);
        }
    }
}

/// <summary>Numeric raster IO through the GDAL runtime deployed by the GeoKernel.NET package.</summary>
public static class Gdal
{
    const string Lib = "gdal.dll";
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern void GDALAllRegister();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int access);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern void GDALClose(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetRasterXSize(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetRasterYSize(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetRasterCount(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGetRasterBand(IntPtr ds, int band);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern double GDALGetRasterNoDataValue(IntPtr band, out int has);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALSetRasterNoDataValue(IntPtr band, double value);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALRasterIO(IntPtr band, int rw, int x, int y, int w, int h, float[] data, int bw, int bh, int type, int ps, int ls);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl, EntryPoint = "GDALRasterIO")] static extern int GDALRasterIOBytes(IntPtr band, int rw, int x, int y, int w, int h, byte[] data, int bw, int bh, int type, int ps, int ls);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetGeoTransform(IntPtr ds, [Out] double[] gt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALSetGeoTransform(IntPtr ds, double[] gt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGetProjectionRef(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALSetProjection(IntPtr ds, [MarshalAs(UnmanagedType.LPUTF8Str)] string wkt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGetDriverByName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALCreate(IntPtr driver, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, int w, int h, int bands, int type, IntPtr options);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr CPLGetLastErrorMsg();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr OSRNewSpatialReference(IntPtr wkt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern void OSRDestroySpatialReference(IntPtr srs);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int OSRImportFromEPSG(IntPtr srs, int epsg);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int OSRSetFromUserInput(IntPtr srs, [MarshalAs(UnmanagedType.LPUTF8Str)] string definition);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int OSRIsSame(IntPtr a, IntPtr b);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern void OSRSetPROJSearchPaths(IntPtr[] paths);

    static readonly Lazy<bool> Registered = new(() => { GDALAllRegister(); return true; });
    const int Float32 = 6;

    static Exception Error(string message)
    {
        string detail = Marshal.PtrToStringUTF8(CPLGetLastErrorMsg()) ?? "";
        return new InvalidOperationException($"{message} {detail}".Trim());
    }

    /// <summary>Prefer packaged gdal-data/proj-data, then the verified sample PROJ database. Process-local only.</summary>
    public static void ConfigureRuntime(string? sampleFolder = null)
    {
        string bin = AppContext.BaseDirectory;
        string gdalData = Path.Combine(bin, "gdal-data");
        if (Directory.Exists(gdalData)) Environment.SetEnvironmentVariable("GDAL_DATA", gdalData);
        if (sampleFolder == null) return;
        string sdkProj = Path.Combine(bin, "proj-data");
        string proj = File.Exists(Path.Combine(sdkProj, "proj.db")) ? sdkProj : Path.Combine(sampleFolder, "proj-data");
        if (!File.Exists(Path.Combine(proj, "proj.db"))) throw new FileNotFoundException("Sample projection database is missing.");
        Environment.SetEnvironmentVariable("PROJ_DATA", proj);
        Environment.SetEnvironmentVariable("PROJ_LIB", proj);
        Environment.SetEnvironmentVariable("PROJ_NETWORK", "OFF");
        _ = Registered.Value;
        IntPtr path = Marshal.StringToCoTaskMemUTF8(proj);
        try { OSRSetPROJSearchPaths([path, IntPtr.Zero]); }
        finally { Marshal.FreeCoTaskMem(path); }
    }

    public sealed record Raster(int Width, int Height, double[] Transform, string Projection, double? NoData, float[][] Bands);

    public static Raster Read(string path, params int[] bands)
    {
        _ = Registered.Value;
        IntPtr ds = GDALOpen(path, 0);
        if (ds == IntPtr.Zero) throw Error($"Cannot open raster {path}.");
        try
        {
            int width = GDALGetRasterXSize(ds), height = GDALGetRasterYSize(ds);
            if (width <= 0 || height <= 0 || (long)width * height > 64_000_000) throw new InvalidDataException($"Unsupported raster dimensions in {path}.");
            var transform = new double[6];
            if (GDALGetGeoTransform(ds, transform) != 0) throw new InvalidDataException($"Raster has no world transform: {path}.");
            string projection = Marshal.PtrToStringUTF8(GDALGetProjectionRef(ds)) ?? "";
            if (bands.Length == 0) bands = Enumerable.Range(1, GDALGetRasterCount(ds)).ToArray();
            double? noData = null;
            var values = new float[bands.Length][];
            for (int i = 0; i < bands.Length; i++)
            {
                IntPtr band = GDALGetRasterBand(ds, bands[i]);
                if (band == IntPtr.Zero) throw new InvalidDataException($"Missing band {bands[i]} in {path}.");
                values[i] = new float[width * height];
                if (GDALRasterIO(band, 0, 0, 0, width, height, values[i], width, height, Float32, 0, 0) != 0) throw Error($"Could not read numeric values from {path}.");
                double nd = GDALGetRasterNoDataValue(band, out int has);
                if (i == 0 && has != 0) noData = nd;
            }
            return new Raster(width, height, transform, projection, noData, values);
        }
        finally { GDALClose(ds); }
    }

    public static bool SameCrs(string projection, int epsg)
    {
        _ = Registered.Value;
        IntPtr a = OSRNewSpatialReference(IntPtr.Zero), b = OSRNewSpatialReference(IntPtr.Zero);
        try { return OSRSetFromUserInput(a, projection) == 0 && OSRImportFromEPSG(b, epsg) == 0 && OSRIsSame(a, b) != 0; }
        finally { OSRDestroySpatialReference(a); OSRDestroySpatialReference(b); }
    }

    public static string WriteFloat(string path, int width, int height, double[] transform, string projection, IReadOnlyList<float[]> bands, double? noData = -9999)
    {
        _ = Registered.Value;
        IntPtr ds = GDALCreate(GDALGetDriverByName("GTiff"), path, width, height, bands.Count, Float32, IntPtr.Zero);
        if (ds == IntPtr.Zero) throw Error($"Cannot create GeoTIFF {path}.");
        try
        {
            if (GDALSetGeoTransform(ds, transform) != 0 || GDALSetProjection(ds, projection) != 0) throw Error("Cannot write GeoTIFF georeferencing.");
            for (int i = 0; i < bands.Count; i++)
            {
                if (bands[i].Length != width * height) throw new ArgumentException("GeoTIFF band size mismatch.");
                IntPtr band = GDALGetRasterBand(ds, i + 1);
                if (noData is double value) GDALSetRasterNoDataValue(band, value);
                if (GDALRasterIO(band, 1, 0, 0, width, height, bands[i], width, height, Float32, 0, 0) != 0) throw Error("Cannot write GeoTIFF values.");
            }
        }
        finally { GDALClose(ds); }
        return path;
    }

    /// <summary>One 8-bit gray image written as identical R, G and B bands (display relief only).</summary>
    public static string WriteRgb(string path, int width, int height, double[] transform, string projection, byte[] gray) =>
        WriteRgb(path, width, height, transform, projection, gray, gray, gray);

    /// <summary>A Byte RGB GeoTIFF (display only). Byte RGB bypasses the viewer's statistical stretch
    /// of non-Byte imagery, so colours keep their shared, precomputed scale.</summary>
    public static string WriteRgb(string path, int width, int height, double[] transform, string projection, byte[] red, byte[] green, byte[] blue)
    {
        _ = Registered.Value;
        byte[][] bands = [red, green, blue];
        if (bands.Any(b => b.Length != width * height)) throw new ArgumentException("Display image size mismatch.");
        IntPtr ds = GDALCreate(GDALGetDriverByName("GTiff"), path, width, height, 3, 1, IntPtr.Zero);
        if (ds == IntPtr.Zero) throw Error($"Cannot create GeoTIFF {path}.");
        try
        {
            if (GDALSetGeoTransform(ds, transform) != 0 || GDALSetProjection(ds, projection) != 0) throw Error("Cannot write GeoTIFF georeferencing.");
            for (int i = 0; i < 3; i++)
                if (GDALRasterIOBytes(GDALGetRasterBand(ds, i + 1), 1, 0, 0, width, height, bands[i], width, height, 1, 0, 0) != 0) throw Error("Cannot write GeoTIFF values.");
        }
        finally { GDALClose(ds); }
        return path;
    }
}

/// <summary>The four pinned 500 x 500, 0.5 m EPSG:2056 height rasters of the sample.</summary>
public sealed class Grids
{
    public static readonly string[] Names = ["dsm.tif", "dtm_classical.tif", "dtm_ai.tif", "dtm_reference.tif"];
    public required string Folder { get; init; }
    public required int Width { get; init; }
    public required int Height { get; init; }
    public required double[] Transform { get; init; }
    public required string Projection { get; init; }
    public required JsonObject Manifest { get; init; }
    public required int ProfileRow { get; init; }
    public required float[][] Z { get; init; }

    public static Grids Read(string folder, CancellationToken token)
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(folder, "source-manifest.json")))!.AsObject();
        int row = (int?)manifest["profile"]?["row"] ?? -1;
        var z = new float[Names.Length][];
        Gdal.Raster? first = null;
        for (int i = 0; i < Names.Length; i++)
        {
            token.ThrowIfCancellationRequested();
            var raster = Gdal.Read(Path.Combine(folder, Names[i]), 1);
            if (!Gdal.SameCrs(raster.Projection, 2056)) throw new InvalidDataException("The sample height CRS is not equivalent to EPSG:2056.");
            var t = raster.Transform;
            if (raster.Width != 500 || raster.Height != 500 || t[1] != .5 || t[5] != -.5 || t[2] != 0 || t[4] != 0)
                throw new InvalidDataException($"The sample requires the pinned 500 x 500, 0.5 m grid. Found {raster.Width} x {raster.Height}; pixel {t[1]}, {t[5]}; rotation {t[4]}, {t[2]}.");
            if (first == null) first = raster;
            else if (raster.Projection != first.Projection || !raster.Transform.SequenceEqual(first.Transform))
                throw new InvalidDataException("Height rasters do not share exactly the same CRS and grid.");
            foreach (float value in raster.Bands[0])
                if (!float.IsFinite(value) || (raster.NoData is double nd && value == nd))
                    throw new InvalidDataException("This prepared comparison requires valid heights in all four rasters.");
            z[i] = raster.Bands[0];
        }
        if (row < 0 || row >= first!.Height) throw new InvalidDataException("Invalid sample profile row.");
        return new Grids { Folder = folder, Width = first.Width, Height = first.Height, Transform = first.Transform, Projection = first.Projection, Manifest = manifest, ProfileRow = row, Z = z };
    }

    /// <summary>0% is exactly the DSM, 100% exactly the selected DTM; between: (1-t)*DSM + t*DTM.</summary>
    public float[] Blended(int method, int percent)
    {
        if (method is not (1 or 2) || percent is < 0 or > 100) throw new ArgumentException("Invalid surface selection.");
        if (percent == 0) return Z[0];
        if (percent == 100) return Z[method];
        double t = percent / 100.0;
        var result = new float[Z[0].Length];
        for (int i = 0; i < result.Length; i++) result[i] = (float)((1 - t) * Z[0][i] + t * Z[method][i]);
        return result;
    }

    public string SurfaceFile(int method, int percent, string temporary) => percent switch
    {
        0 => Path.Combine(Folder, "dsm.tif"),
        100 => Path.Combine(Folder, method == 1 ? "dtm_classical.tif" : "dtm_ai.tif"),
        _ => Gdal.WriteFloat(temporary, Width, Height, Transform, Projection, [Blended(method, percent)]),
    };

    public double Metric(string method, string name) => (double?)Manifest["metrics"]?[method]?[name] ?? 0;

    /// <summary>Viewer3D maps local ENU metres to render units by 2 / horizontal span of the pixel-centre grid.</summary>
    public double RenderScale => 2 / ((Width - 1) * Transform[1]);

    public double MeanDsm => Z[0].Average(v => (double)v);

    /// <summary>Profile samples (row of each raster) plus the shared value range used by the chart.</summary>
    public (float[][] Rows, double Low, double High) Profile()
    {
        var rows = Z.Select(z => z.AsSpan(ProfileRow * Width, Width).ToArray()).ToArray();
        return (rows, rows.Min(r => r.Min()) - 2.0, rows.Max(r => r.Max()) + 2.0);
    }
}

/// <summary>Render-space camera helpers for two scenes that share one grid and origin.</summary>
public static class Cameras
{
    public static bool Same(float[]? a, float[]? b) =>
        a != null && b != null && a.Zip(b, (x, y) => Math.Abs(x - y)).Sum() < 1e-5f;
}

public static class DsmText
{
    public const string Ready = "Ready · Original DSM stays visible | Drag: pan · Wheel: zoom · Middle/Ctrl+drag: orbit";
    public const string Controls = "Left drag: pan\nWheel or right drag: zoom\nMiddle drag or Ctrl + left drag: orbit\nBoth cameras stay linked.\n\nMove the surface slider and release to update geometry. The original view stays unchanged.";
    public static readonly string[] Colors = ["#66768a", "#087e8b", "#8857bd", "#d98419"];

    public static string PercentLabel(int value) => value switch
    {
        100 => "100% · prepared DTM endpoint",
        0 => "0% · original DSM endpoint",
        _ => $"{value}% · display interpolation",
    };

    public static string Heading(int method, int percent)
    {
        string name = method == 1 ? "CLASSICAL" : "AI · pix_grad";
        return percent switch
        {
            100 => name + " DTM · prepared terrain estimate",
            0 => "ORIGINAL DSM · slider at 0%",
            _ => $"{name} · {percent}% removal transition (display only)",
        };
    }

    public static string Metrics(Grids grids) =>
        "REFERENCE CHECK · this crop only\n\n" +
        $"Classical: RMSE {grids.Metric("classical", "rmse_m"):F2} m · MAE {grids.Metric("classical", "mae_m"):F2} m\n" +
        $"AI: RMSE {grids.Metric("ai", "rmse_m"):F2} m · MAE {grids.Metric("ai", "mae_m"):F2} m\n\n" +
        "250,000 shared reference pixels\nLower error is better.\n\n" +
        "Scores describe the final DTM endpoints, not the transition slider.";
}
