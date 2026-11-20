using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json.Nodes;
#if GEOAI_WPF
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.DsmToDtm;

/// <summary>Command-line options of the DsmOrthoToDtm windows.</summary>
public sealed record OrthoOptions(string? DataDirectory, string? SampleArchive, string? SampleUrl, string?[] ModelArchives, string?[] ModelUrls)
{
    public static OrthoOptions Parse(string[] args)
    {
        string? data = null, archive = null, url = null;
        string?[] archives = new string?[2], urls = new string?[2];
        for (int i = 0; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"Missing value for {args[i]}.");
            switch (args[i])
            {
                case "--data-dir": data = Next(); break;
                case "--sample-archive": archive = Next(); break;
                case "--sample-url": url = Next(); break;
                case "--pix-model-archive": archives[0] = Next(); break;
                case "--rgb-model-archive": archives[1] = Next(); break;
                case "--pix-model-url": urls[0] = Next(); break;
                case "--rgb-model-url": urls[1] = Next(); break;
                default:
                    throw new ArgumentException($"Unknown option {args[i]}. Use --data-dir, --sample-archive, --sample-url, --pix-model-archive, --rgb-model-archive, --pix-model-url or --rgb-model-url.");
            }
        }
        return new(data, archive, url, archives, urls);
    }

    public DsmOptions Sample => new(SampleArchive, SampleUrl, DataDirectory);
    public DsmOptions Model(int index, string? archive) => new(archive ?? ModelArchives[index], ModelUrls[index], DataDirectory);
}

/// <summary>An aligned RGB orthophoto: source bytes and the same bands with RGB-only context fill.</summary>
public sealed record OrthoImage(string Path, string Sha256, byte[][] Bands, byte[][] Context, int Filled);

/// <summary>A loaded source: DSM, orthophoto, optional evaluation-only reference and the DSM relief.</summary>
public sealed record OrthoScene(TerrainRaster Dsm, OrthoImage Rgb, TerrainRaster? Reference, string Original, JsonObject Manifest)
{
    /// <summary>The DSM-only view the shared comparison helpers take.</summary>
    public TerrainScene Terrain => new(Dsm, Reference, Original, Manifest);
}

/// <summary>One completed comparison of classical, pix_grad and rgb_grad_fm estimates.</summary>
public sealed record OrthoComparison(TerrainResult Classical, AiHeight[] Ai, int Overlap, int Threads, JsonObject[] ModelManifests)
{
    public string[][] Maps { get; init; } = [new string[3], new string[3], new string[3]];
    public TerrainScore[] Full { get; init; } = [new(), new(), new()];
    public TerrainScore[] Crop { get; init; } = [new(), new(), new()];
    public Dictionary<string, string> Files { get; init; } = new();
    public JsonObject Report { get; init; } = new();
    public float[] Heights(int method) => method == 0 ? Classical.Dtm : Ai[method - 1].Dtm;
}

/// <summary>
/// DSM + orthophoto to DTM: three independent estimates from the same full DSM: the classical filter
/// (DSM only), pix_grad (DSM only) and rgb_grad_fm (DSM, R, G, B), both models through GeoKernel.AI's
/// raster inference C ABI. No method sees another's result or the evaluation reference.
/// </summary>
public static class Ortho
{
    public static readonly string[] Methods = ["classical", "pix_grad", "rgb_grad_fm"];
    static readonly string[] ModelIds = ["davpergan-dtm-dsm-gan-pix-grad", "davpergan-dtm-dsm-gan-rgb-grad-fm"];
    static readonly string[] ModelSha256 = ["e400e27f6afc75bcf4e1a61432ee9557529683903996dd953d3e8e3fcbe90eb6", "5042d5abac64d6b1764265924c9910807e3a5efee7e1b9517fc9d86bf4d0e3a3"];

    public static SampleAssets SampleAssets() => DsmToDtm.SampleAssets.FromJson(DsmToDtm.SampleAssets.Resource("ortho-sample-assets.json"));
    public static JsonObject ModelAssets(int index) => DsmToDtm.SampleAssets.Resource(index == 0 ? "ortho-pix-model-assets.json" : "ortho-rgb-model-assets.json");
    public static string[] ArchiveSha256() => [(string)ModelAssets(0)["sha256"]!, (string)ModelAssets(1)["sha256"]!];

    /// <summary>Verified pix_grad (0) or rgb_grad_fm (1) package folder.</summary>
    public static string PrepareModel(OrthoOptions options, int index, string? archive, CancellationToken token, Action<int, string> progress) =>
        Ai.PrepareModel(DsmToDtm.SampleAssets.FromJson(ModelAssets(index)), options.Model(index, archive), ModelIds[index], ModelSha256[index], token,
            (p, m) => progress(p, $"{Methods[index + 1]}: {m}"));

    // ----- orthophoto -----------------------------------------------------------------------------------
    const string Lib = "gdal.dll";
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int access);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern void GDALClose(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetRasterXSize(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetRasterYSize(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetRasterCount(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGetRasterBand(IntPtr ds, int band);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGetMaskBand(IntPtr band);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetRasterDataType(IntPtr band);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetRasterColorInterpretation(IntPtr band);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern double GDALGetRasterScale(IntPtr band, out int present);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern double GDALGetRasterOffset(IntPtr band, out int present);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALGetGeoTransform(IntPtr ds, [Out] double[] gt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALGetProjectionRef(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern int GDALRasterIO(IntPtr band, int rw, int x, int y, int w, int h, byte[] data, int bw, int bh, int type, int ps, int ls);

    static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidDataException(message);
    }

    /// <summary>Three RGB-ordered Byte bands on the DSM's exact grid that cover every valid DSM cell.</summary>
    public static OrthoImage ReadOrtho(TerrainRaster dsm, string path, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        IntPtr ds = GDALOpen(path, 0);
        Require(ds != IntPtr.Zero, "Cannot open RGB orthophoto.");
        int n = dsm.Width * dsm.Height;
        var bands = new byte[3][];
        var valid = Enumerable.Repeat((byte)1, n).ToArray();
        try
        {
            Require(GDALGetRasterCount(ds) == 3, "Orthophoto must contain exactly three RGB Byte bands.");
            var t = new double[6];
            bool hasTransform = GDALGetGeoTransform(ds, t) == 0;
            string projection = Marshal.PtrToStringUTF8(GDALGetProjectionRef(ds)) ?? "";
            Require(DsmToDtm.Terrain.SameProjection(dsm.Projection, projection) && hasTransform && GDALGetRasterXSize(ds) == dsm.Width
                    && GDALGetRasterYSize(ds) == dsm.Height && t.SequenceEqual(dsm.Transform),
                "RGB and DSM must have the same dimensions, exact affine grid and equivalent CRS. Align them before loading.");
            var mask = new byte[n];
            for (int channel = 0; channel < 3; channel++)
            {
                token.ThrowIfCancellationRequested();
                IntPtr band = GDALGetRasterBand(ds, channel + 1);
                Require(GDALGetRasterDataType(band) == 1 && GDALGetRasterColorInterpretation(band) == channel + 3,
                    "Use RGB-ordered Byte bands; no implicit spectral or color conversion is performed.");
                double scale = GDALGetRasterScale(band, out int present);
                Require(present == 0 || Math.Abs(scale - 1) < 1e-12, "Apply RGB scale before loading.");
                double offset = GDALGetRasterOffset(band, out present);
                Require(present == 0 || Math.Abs(offset) < 1e-12, "Apply RGB offset before loading.");
                bands[channel] = new byte[n];
                Require(GDALRasterIO(band, 0, 0, 0, dsm.Width, dsm.Height, bands[channel], dsm.Width, dsm.Height, 1, 0, 0) == 0
                        && GDALRasterIO(GDALGetMaskBand(band), 0, 0, 0, dsm.Width, dsm.Height, mask, dsm.Width, dsm.Height, 1, 0, 0) == 0,
                    "Cannot read RGB pixels or validity.");
                for (int i = 0; i < n; i++) if (mask[i] == 0) valid[i] = 0;
            }
        }
        finally { GDALClose(ds); }
        for (int i = 0; i < n; i++)
            Require(dsm.Valid[i] == 0 || valid[i] != 0, "RGB does not cover every valid DSM pixel. Crop or prepare common source coverage explicitly.");
        int filled = valid.Count(v => v == 0);
        var context = bands;
        if (filled > 0)
        {
            int[] nearest = DsmToDtm.Terrain.SupportMap(valid, dsm.Width, dsm.Height, token);
            context = [.. bands.Select(b => Enumerable.Range(0, n).Select(i => valid[i] != 0 ? b[i] : b[nearest[i]]).ToArray())];
        }
        token.ThrowIfCancellationRequested();
        return new OrthoImage(Path.GetFullPath(path), SampleCache.HashFile(path), bands, context, filled);
    }

    // ----- inference -------------------------------------------------------------------------------------
    /// <summary>Reports on the calling (SDK worker) thread instead of posting to a synchronization context.</summary>
    sealed class SyncProgress(Action<AIProgress> report) : IProgress<AIProgress>
    {
        public void Report(AIProgress value) => report(value);
    }

    /// <summary>
    /// pix_grad (DSM) or rgb_grad_fm (DSM, R, G, B) over the full grid; the reference is not involved. Missing DSM
    /// cells are filled from DSM support only (RGB was filled from RGB support only). The channels are stacked in the
    /// model's order into one Float32 raster, edge-padded right/bottom when smaller than one tile; the source mask is
    /// restored and the final DTM bounded above by the DSM.
    /// </summary>
    public static AiHeight Infer(TerrainRaster dsm, string model, OrthoImage? rgb, int overlap, int threads, string folder, CancellationToken token, Action<int, string> progress)
    {
        int index = rgb == null ? 0 : 1;
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(model, "geokernel-model.json")))!.AsObject();
        Require((string?)manifest["modelSha256"] == ModelSha256[index], "Unexpected AI checkpoint for the declared input profile.");
        int tileW = (int)manifest["tiling"]!["width"]!, tileH = (int)manifest["tiling"]!["height"]!;
        int w = dsm.Width, h = dsm.Height, pw = Math.Max(w, tileW), ph = Math.Max(h, tileH);
        int filled = dsm.Valid.Count(v => v == 0);
        bool padded = pw != w || ph != h;
        float[] z = dsm.Z;
        if (filled > 0)
        {
            int[] nearest = DsmToDtm.Terrain.SupportMap(dsm.Valid, w, h, token);
            z = new float[w * h];
            for (int i = 0; i < z.Length; i++) z[i] = dsm.Valid[i] != 0 ? dsm.Z[i] : dsm.Z[nearest[i]];
        }
        var channels = new List<float[]> { z };
        if (rgb != null) channels.AddRange(rgb.Context.Select(b => b.Select(v => (float)v).ToArray()));
        if (padded)
            channels = [.. channels.Select(c =>
            {
                var grown = new float[pw * ph];
                for (int y = 0; y < ph; y++)
                {
                    int row = Math.Min(y, h - 1) * w;
                    for (int x = 0; x < pw; x++) grown[y * pw + x] = c[row + Math.Min(x, w - 1)];
                }
                return grown;
            })];
        string source = dsm.Path;
        if (filled > 0 || padded || rgb != null)
            source = Gdal.WriteFloat(Path.Combine(folder, $"{Methods[index + 1]}-input.tif"), pw, ph, dsm.Transform, dsm.Projection, channels, null);
        token.ThrowIfCancellationRequested();
        string output = Path.Combine(folder, $"{Methods[index + 1]}-inference.tif");
        // The C ABI call has no cancellation hook; cancellation is checked when it returns.
        var result = GeoKernelAI.RunRasterInference(new AIRasterInferenceRequest
        {
            ModelPackagePath = model, RasterPath = source, OutputPath = output, OutputMode = AIRasterOutputMode.FloatBands,
            Bands = [.. Enumerable.Range(1, channels.Count)], Provider = AIExecutionProvider.CPU, IntraOpThreads = threads, InterOpThreads = 1,
            Tiling = new AITileOptions(tileW, tileH, overlap, overlap),
        }, new SyncProgress(p => progress(p.Percent, $"tiles {p.CompletedTiles} / {p.TotalTiles} · {p.Message}")));
        token.ThrowIfCancellationRequested();
        Require(!result.GetProperty("cancelled").GetBoolean() && result.GetProperty("width").GetInt32() == pw && result.GetProperty("height").GetInt32() == ph
                && result.GetProperty("bandCount").GetInt32() == 1 && result.GetProperty("dataType").GetString() == "Float32",
            "AI result does not match the source grid/type.");
        double[] transform = [.. result.GetProperty("geoTransform").EnumerateArray().Select(v => v.GetDouble())];
        Require(transform.SequenceEqual(dsm.Transform) && DsmToDtm.Terrain.SameProjection(dsm.Projection, result.GetProperty("projectionWkt").GetString() ?? ""),
            "AI result changed the CRS or affine grid.");
        float[] values = Gdal.Read(output, 1).Bands[0];
        var raw = new float[w * h];
        for (int y = 0; y < h; y++) Array.Copy(values, y * pw, raw, y * w, w);
        var dtm = (float[])raw.Clone();
        int clamped = 0;
        for (int i = 0; i < raw.Length; i++)
        {
            if (dsm.Valid[i] == 0) { raw[i] = dtm[i] = DsmToDtm.Terrain.NoData; continue; }
            Require(float.IsFinite(raw[i]) && raw[i] != DsmToDtm.Terrain.NoData, "AI returned an invalid height on a valid DSM pixel.");
            if (dtm[i] > dsm.Z[i]) { dtm[i] = dsm.Z[i]; clamped++; }
        }
        return new AiHeight(raw, dtm, JsonNode.Parse(result.GetProperty("provenance").GetRawText())!.AsObject(), result.GetProperty("processedTiles").GetInt32(),
            filled, clamped, padded);
    }

    // ----- comparison ------------------------------------------------------------------------------------
    static float[] Removed(TerrainRaster d, float[] dtm)
    {
        var removed = new float[dtm.Length];
        for (int i = 0; i < removed.Length; i++) removed[i] = d.Valid[i] != 0 ? Math.Max(0f, d.Z[i] - dtm[i]) : DsmToDtm.Terrain.NoData;
        return removed;
    }

    static string Rgb(TerrainRaster d, string path, (byte[] R, byte[] G, byte[] B) bands) =>
        Gdal.WriteRgb(path, d.Width, d.Height, d.Transform, d.Projection, bands.R, bands.G, bands.B);

    public static OrthoComparison Evaluate(OrthoScene scene, OrthoComparison result, string[] archiveSha256)
    {
        var d = scene.Dsm;
        var rect = Ai.Window(scene.Terrain);
        TerrainScore[] full = new TerrainScore[3], crop = new TerrainScore[3];
        JsonObject fullJson = new(), cropJson = new(), models = new();
        for (int method = 0; method < 3; method++)
        {
            full[method] = DsmToDtm.Terrain.Score(d, result.Heights(method), scene.Reference);
            crop[method] = Ai.AreaScore(d, result.Heights(method), scene.Reference, rect);
            fullJson[Methods[method]] = full[method].Json();
            cropJson[Methods[method]] = crop[method].Json();
        }
        cropJson["pixel_window"] = new JsonObject { ["column"] = rect.X, ["row"] = rect.Y, ["width"] = rect.W, ["height"] = rect.H };
        for (int model = 0; model < 2; model++)
        {
            var a = result.Ai[model];
            models[Methods[model + 1]] = new JsonObject
            {
                ["archive_sha256"] = archiveSha256[model], ["manifest"] = result.ModelManifests[model].DeepClone(), ["sdk_inference"] = a.Provenance.DeepClone(),
                ["context_filled_dsm_pixels"] = a.ContextFilled, ["small_input_edge_padding"] = a.Padded, ["upper_bound_clamped_pixels"] = a.Clamped,
            };
        }
        var files = new JsonObject();
        foreach (string name in result.Files.Keys) files[name] = name;
        var report = new JsonObject
        {
            ["schema_version"] = 1,
            ["experiment"] = "independent classical / DSM-only pix_grad / DSM+RGB rgb_grad_fm",
            ["source_dsm"] = d.Path,
            ["source_dsm_sha256"] = d.Sha256,
            ["source_rgb"] = scene.Rgb.Path,
            ["source_rgb_sha256"] = scene.Rgb.Sha256,
            ["rgb_context_filled_pixels"] = scene.Rgb.Filled,
            ["reference"] = scene.Reference?.Path,
            ["reference_sha256"] = scene.Reference?.Sha256,
            ["reference_used_for_processing"] = false,
            ["methods_feed_each_other"] = false,
            ["model_input_order"] = "pix_grad: DSM; rgb_grad_fm: DSM, R, G, B",
            ["ai_options"] = new JsonObject { ["overlap_pixels"] = result.Overlap, ["cpu_threads"] = result.Threads, ["tile_pixels"] = 256 },
            ["models"] = models,
            ["classical"] = new JsonObject { ["method"] = DsmToDtm.Terrain.Method, ["parameters"] = result.Classical.Parameters.Json(), ["summary"] = result.Classical.Summary.DeepClone() },
            ["grid"] = new JsonObject { ["width"] = d.Width, ["height"] = d.Height, ["transform"] = new JsonArray([.. d.Transform.Select(v => (JsonNode)v)]), ["crs_wkt"] = d.Projection },
            ["evaluation"] = new JsonObject { ["full"] = fullJson, ["comparison_crop"] = cropJson },
            ["source_manifest"] = scene.Manifest.DeepClone(),
            ["output_files"] = files,
            ["attribution"] = "Federal Office of Topography swisstopo. AI weights: David Perez, Zenodo 7973800 / 7973886, CC BY 4.0; independent deterministic ONNX adaptations.",
            ["interpretation"] = "Different pretrained checkpoints/training objectives; observed differences do not isolate the causal contribution of RGB. Fixed development scene, not a held-out benchmark. Survey years and training GSD differ or remain unverified.",
        };
        return result with { Full = full, Crop = crop, Report = report };
    }

    /// <summary>Classical, then pix_grad, then rgb_grad_fm, each from the same full source grid.</summary>
    public static OrthoComparison Compare(OrthoScene scene, string[] models, TerrainParameters parameters, int overlap, int threads, string folder,
        string[] archiveSha256, CancellationToken token, Action<int, string> progress)
    {
        var d = scene.Dsm;
        JsonObject[] manifests = [.. models.Select(m => JsonNode.Parse(File.ReadAllText(Path.Combine(m, "geokernel-model.json")))!.AsObject())];
        var classical = DsmToDtm.Terrain.Estimate(d, parameters, token, (p, text) => progress((int)(p * .24), "Classical: " + text));
        var ai = new AiHeight[2];
        for (int model = 0; model < 2; model++)
        {
            int m = model;
            ai[model] = Infer(d, models[model], model == 1 ? scene.Rgb : null, overlap, threads, folder, token,
                (p, text) => progress(25 + m * 29 + (int)(p * .28), $"{Methods[m + 1]}: {text}"));
        }
        var result = new OrthoComparison(classical, ai, overlap, threads, manifests);
        void Numeric(string name, float[]? heights, byte[]? quality = null)
        {
            token.ThrowIfCancellationRequested();
            result.Files[name] = DsmToDtm.Terrain.Write(d, Path.Combine(folder, name), heights, quality);
        }
        for (int method = 0; method < 3; method++)
        {
            progress(84 + method * 3, $"Preparing {Methods[method]} outputs...");
            var display = Ai.Display(d, result.Heights(method), Removed(d, result.Heights(method)));
            Numeric($"{Methods[method]}-dtm.tif", display.Dtm);
            Numeric($"{Methods[method]}-removed.tif", display.Removed);
            if (method > 0) Numeric($"{Methods[method]}-raw.tif", ai[method - 1].Raw);
            for (int mode = 0; mode < 3; mode++)
            {
                var bands = mode == 0 ? DsmToDtm.Terrain.Relief(d, display.Dtm, token) : DsmToDtm.Terrain.Diagnostic(d, display, mode == 1 ? 0 : 2, scene.Reference, token);
                result.Maps[method][mode] = Rgb(d, Path.Combine(folder, $"view-{method}-{mode}.tif"), bands);
            }
        }
        Numeric("classical-qa.tif", null, classical.Quality);
        progress(96, "Writing identical source-pixel comparison crops...");
        var rect = Ai.Window(scene.Terrain);
        var cropped = Ai.CutGrid(d, rect);
        void Crop(string name, float[] values) => result.Files[name] = DsmToDtm.Terrain.Write(cropped, Path.Combine(folder, name), Ai.Cut(values, d.Width, rect));
        Crop("comparison-dsm.tif", d.Z);
        for (int method = 0; method < 3; method++) Crop($"comparison-{Methods[method]}.tif", result.Heights(method));
        if (scene.Reference != null) Crop("comparison-reference.tif", scene.Reference.Z);
        byte[][] rgbCrop = [.. scene.Rgb.Bands.Select(b =>
        {
            var c = new byte[rect.W * rect.H];
            for (int y = 0; y < rect.H; y++) Array.Copy(b, (rect.Y + y) * d.Width + rect.X, c, y * rect.W, rect.W);
            return c;
        })];
        result.Files["comparison-rgb.tif"] = Gdal.WriteRgb(Path.Combine(folder, "comparison-rgb.tif"), rect.W, rect.H, cropped.Transform, d.Projection, rgbCrop[0], rgbCrop[1], rgbCrop[2]);
        var scored = Evaluate(scene, result, archiveSha256);
        token.ThrowIfCancellationRequested();
        return scored;
    }

    /// <summary>New error maps, crop file and scores for a new evaluation reference; heights unchanged.</summary>
    public static OrthoComparison AttachReference(OrthoScene scene, OrthoComparison result, string folder, string[] archiveSha256, CancellationToken token, Action<int, string> progress)
    {
        var d = scene.Dsm;
        string[][] maps = [.. result.Maps.Select(m => (string[])m.Clone())];
        for (int method = 0; method < 3; method++)
        {
            progress(30 + method * 20, "Preparing reference error maps...");
            maps[method][2] = Rgb(d, Path.Combine(folder, $"reference-{method}.tif"), DsmToDtm.Terrain.Diagnostic(d, Ai.Display(d, result.Heights(method), []), 2, scene.Reference, token));
        }
        var rect = Ai.Window(scene.Terrain);
        var files = new Dictionary<string, string>(result.Files)
        {
            ["comparison-reference.tif"] = DsmToDtm.Terrain.Write(Ai.CutGrid(d, rect), Path.Combine(folder, "comparison-reference.tif"), Ai.Cut(scene.Reference!.Z, d.Width, rect)),
        };
        return Evaluate(scene, result with { Maps = maps, Files = files }, archiveSha256);
    }

    public static string Export(OrthoComparison result, string parent, CancellationToken token, Action<int, string> progress) =>
        Ai.Export(result.Files, result.Report, parent, "dsm-ortho-dtm", ".dsm-ortho-export-", token, progress);
}

/// <summary>User-facing text shared by the DsmOrthoToDtm windows.</summary>
public static class OrthoText
{
    public static readonly string[] Inputs = ["Original DSM relief", "RGB orthophoto"];
    public static readonly string[] Modes = ["Terrain relief", "Removed height: DSM − DTM", "Signed reference errors"];
    public static readonly string[] Scopes = ["Full input", "Shared comparison crop"];
    public static readonly string[] Suffix = ["DTM · relief", "REMOVED HEIGHT", "ERROR · DTM − reference"];
    public static readonly string[] Titles = ["RGB ORTHOPHOTO · input context", "CLASSICAL · DSM only", "PIX_GRAD · DSM only", "RGB_GRAD_FM · DSM + RGB"];
    public static readonly string[] Pending = ["CLASSICAL · Run to generate", "PIX_GRAD · Run to generate", "RGB_GRAD_FM · Run to generate"];
    public static readonly string[] Names = ["Classical · DSM", "pix_grad · DSM", "rgb_grad_fm · DSM + RGB"];
    public static readonly string[] Colors = ["#087e8b", "#8857bd", "#ba7020"];
    public static readonly string[] Legends =
    [
        "Same relief light/contrast for all methods; source orthophoto or DSM remains on the left.",
        "Shared removed-height scale: white 0 m → red ≥20 m. Display saturation only; numeric heights are unchanged.",
        "Shared signed-error scale: blue ≤−2 m · white 0 · red ≥+2 m. Scores use identical valid reference cells in the selected scope.",
    ];
    public const string ControlsChanged = "Controls changed. Run to update; maps and scores retain the last completed settings.";
    public const string ModelsReady = "pix_grad + rgb_grad_fm ready\nVerified 256 × 256 tiles · CPU";
    public const string Attribution = "Data: Federal Office of Topography swisstopo.\nAI weights: David Perez · CC BY 4.0.\nReference is evaluation-only; no datum conversion.";
    public const string Footer = "Different pretrained checkpoints and objectives: differences do not isolate the causal benefit of RGB. Process full input, then evaluate the same crop. Experimental development scene.";

    static string G(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);

    public static string Source(OrthoScene scene) =>
        $"{Path.GetFileName(scene.Dsm.Path)} + {Path.GetFileName(scene.Rgb.Path)}\n{scene.Dsm.Width} × {scene.Dsm.Height} · {G(scene.Dsm.Pixel)} m cells\n" +
        (scene.Reference != null ? "Aligned reference attached" : "No reference");

    public static string[] Row(int method, TerrainScore s)
    {
        string F(double v) => s.Count > 0 ? v.ToString("0.000", CultureInfo.InvariantCulture) : "—";
        return [Names[method], s.Count.ToString(CultureInfo.InvariantCulture), F(s.Rmse), F(s.Mae), F(s.Bias)];
    }

    public static string Summary(OrthoComparison r, OrthoScene scene, bool crop)
    {
        var (_, _, w, h) = Ai.Window(scene.Terrain);
        double p = scene.Dsm.Pixel;
        return $"Last run · pix_grad {r.Ai[0].Tiles} tiles · rgb_grad_fm {r.Ai[1].Tiles} tiles · overlap {r.Overlap} px · CPU threads {r.Threads} · classical working {G(r.Classical.Working)} m / window {G(r.Classical.Parameters.Window)} m\n" +
               $"Evaluation: {(crop ? "shared crop" : "full input")} · common crop {w} × {h} cells ({G(w * p)} × {G(h * p)} m). All methods processed the full input; outside-crop map areas are dimmed.";
    }

    /// <summary>Probe text of the source cell containing a world point, or null outside the raster.</summary>
    public static (string Text, double X, double Y)? Probe(OrthoScene scene, OrthoComparison? result, double x, double y)
    {
        var d = scene.Dsm;
        double cx = (x - d.Transform[0]) / d.Pixel, cy = (d.Transform[3] - y) / d.Pixel;
        if (!double.IsFinite(cx) || !double.IsFinite(cy) || cx < 0 || cy < 0 || cx >= d.Width || cy >= d.Height) return null;
        int col = (int)cx, row = (int)cy, i = row * d.Width + col;
        var inv = CultureInfo.InvariantCulture;
        var text = new StringBuilder($"Source pixel {col} / {row}\nRGB {scene.Rgb.Bands[0][i]} / {scene.Rgb.Bands[1][i]} / {scene.Rgb.Bands[2][i]}\n");
        if (d.Valid[i] == 0) text.Append("DSM NoData");
        else
        {
            text.Append(inv, $"DSM {d.Z[i]:0.00} m");
            if (result != null)
                for (int method = 0; method < 3; method++) text.Append(inv, $"\n{Ortho.Methods[method]} {result.Heights(method)[i]:0.00} m");
            if (scene.Reference is { } reference && reference.Valid[i] != 0) text.Append(inv, $"\nReference {reference.Z[i]:0.00} m");
        }
        return (text.ToString(), d.Transform[0] + (col + .5) * d.Pixel, d.Transform[3] - (row + .5) * d.Pixel);
    }
}
