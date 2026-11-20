using System.Globalization;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
#if GEOAI_WPF
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.DsmToDtm;

/// <summary>Command-line options of the DsmToDtmAI windows.</summary>
public sealed record AiOptions(string? DataDirectory, string? SampleArchive, string? SampleUrl, string? ModelArchive, string? ModelUrl)
{
    public static AiOptions Parse(string[] args)
    {
        string? data = null, sampleArchive = null, sampleUrl = null, modelArchive = null, modelUrl = null;
        for (int i = 0; i < args.Length; i++)
        {
            string Next() => i + 1 < args.Length ? args[++i] : throw new ArgumentException($"Missing value for {args[i]}.");
            switch (args[i])
            {
                case "--data-dir": data = Next(); break;
                case "--sample-archive": sampleArchive = Next(); break;
                case "--sample-url": sampleUrl = Next(); break;
                case "--model-archive": modelArchive = Next(); break;
                case "--model-url": modelUrl = Next(); break;
                default: throw new ArgumentException($"Unknown option {args[i]}. Use --data-dir, --sample-archive, --sample-url, --model-archive or --model-url.");
            }
        }
        return new(data, sampleArchive, sampleUrl, modelArchive, modelUrl);
    }

    public DsmOptions Sample => new(SampleArchive, SampleUrl, DataDirectory);
    public DsmOptions Model(string? archive) => new(archive ?? ModelArchive, ModelUrl, DataDirectory);
}

/// <summary>pix_grad output for one source: blended raw heights and the final DSM-bounded DTM.</summary>
public sealed record AiHeight(float[] Raw, float[] Dtm, JsonObject Provenance, int Tiles, int ContextFilled, int Clamped, bool Padded);

/// <summary>One completed comparison: both estimates, display maps, scores, files and the report.</summary>
public sealed record AiComparison(TerrainResult Classical, AiHeight Ai, int Overlap, int Threads, string Model, JsonObject ModelManifest, string Folder)
{
    public string[][] Maps { get; init; } = [new string[3], new string[3]];
    public TerrainScore[] Full { get; init; } = [new(), new()];
    public TerrainScore[] Crop { get; init; } = [new(), new()];
    public Dictionary<string, string> Files { get; init; } = new();
    public JsonObject Report { get; init; } = new();
}

/// <summary>
/// Live AI / classical comparison. The experimental pix_grad model runs through GeoKernel.AI's raster
/// inference C ABI (<see cref="GeoKernelAI"/>); the classical estimate is <see cref="Terrain.Estimate"/>.
/// Both receive the same full DSM only: the reference is evaluation-only.
/// </summary>
public static class Ai
{
    public const string ModelId = "davpergan-dtm-dsm-gan-pix-grad";
    public const string ModelSha256 = "e400e27f6afc75bcf4e1a61432ee9557529683903996dd953d3e8e3fcbe90eb6";
    public const string ModelHint = "Use the exact model ZIP or retry its public URL.";
    const int MaxSupportCells = 262144;

    public static SampleAssets ModelAssets() => SampleAssets.FromJson(SampleAssets.Resource("ai-model-assets.json"));
    public static JsonObject TerrainAssets() => SampleAssets.Resource("ai-terrain-assets.json");

    // ----- acquisition -------------------------------------------------------------------------------
    /// <summary>Download (when needed) and verify the full 1 km DSM and reference by size and SHA-256.</summary>
    public static string PrepareTerrain(JsonObject assets, string root, CancellationToken token, Action<int, string> progress)
    {
        string folder = Path.Combine(root, (string)assets["cache"]!);
        Directory.CreateDirectory(folder);
        foreach (string name in new[] { "dsm", "reference" })
        {
            var asset = assets["sources"]![name]!;
            string path = Path.Combine(folder, (string)asset["file"]!), sha = (string)asset["sha256"]!;
            if (SampleCache.HashFile(path) != sha)
                SampleCache.DownloadAsync(new Uri((string)asset["url"]!), path, new Progress<(int, string)>(p => progress(p.Item1, p.Item2)), token,
                    $"Full 1 km {name}: downloading...", "Retry when the swisstopo download is reachable.").GetAwaiter().GetResult();
            token.ThrowIfCancellationRequested();
            if (new FileInfo(path).Length != (long)asset["bytes"]! || SampleCache.HashFile(path) != sha)
                throw new InvalidDataException("Full terrain source failed size/SHA-256 verification.");
        }
        return folder;
    }

    /// <summary>Verified pinned model package folder (archive and every package file hash-checked).</summary>
    public static string PrepareModel(AiOptions options, string? archive, CancellationToken token, Action<int, string> progress) =>
        PrepareModel(ModelAssets(), options.Model(archive), ModelId, ModelSha256, token, (p, m) => progress(p, "Model: " + m));

    /// <summary>A verified pinned model package with the expected manifest id and ONNX hash.</summary>
    public static string PrepareModel(SampleAssets assets, DsmOptions source, string id, string sha256, CancellationToken token, Action<int, string> progress)
    {
        string folder = SampleCache.PrepareAsync(assets, source, new Progress<(int Percent, string Message)>(p => progress(p.Percent, p.Message)),
            token, ModelHint).GetAwaiter().GetResult();
        token.ThrowIfCancellationRequested();
        var inspection = GeoKernelAI.InspectModelPackage(folder);
        if (!inspection.GetProperty("valid").GetBoolean() || inspection.GetProperty("manifest").GetProperty("id").GetString() != id
            || inspection.GetProperty("modelSha256").GetString() != sha256)
            throw new InvalidDataException("Unexpected model package. " + string.Join(" ", inspection.GetProperty("validationErrors").EnumerateArray().Select(e => e.GetString())));
        return folder;
    }

    // ----- AI inference -------------------------------------------------------------------------------
    static TerrainRaster Resized(TerrainRaster d, int width, int height, float[] z) => new()
    {
        Path = d.Path, Projection = d.Projection, Sha256 = d.Sha256, Width = width, Height = height, Pixel = d.Pixel,
        Transform = d.Transform, Z = z, Valid = d.Valid,
    };

    /// <summary>
    /// pix_grad over the full DSM with the package tiling; the reference is not involved. Missing cells
    /// are filled for input context only (four-neighbour DSM support) and inputs smaller than one tile are
    /// edge-padded right/bottom, the package's padding mode, before the raster reaches the SDK. The source
    /// mask is restored and the final DTM bounded above by the DSM; the blended raw output is retained.
    /// </summary>
    public static AiHeight Infer(TerrainRaster dsm, string model, int overlap, int threads, string folder, CancellationToken token, Action<int, string> progress)
    {
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(model, "geokernel-model.json")))!.AsObject();
        if ((string?)manifest["modelSha256"] != ModelSha256) throw new InvalidDataException("This example requires the pinned pix_grad adaptation.");
        int tileW = (int)manifest["tiling"]!["width"]!, tileH = (int)manifest["tiling"]!["height"]!;
        int w = dsm.Width, h = dsm.Height, pw = Math.Max(w, tileW), ph = Math.Max(h, tileH);
        int filled = dsm.Valid.Count(v => v == 0);
        bool padded = pw != w || ph != h;
        string source = dsm.Path;
        if (filled > 0 || padded)
        {
            float[] z = dsm.Z;
            if (filled > 0)
            {
                progress(38, "Filling missing DSM context from DSM support only...");
                int[] nearest = Terrain.SupportMap(dsm.Valid, w, h, token);
                z = new float[w * h];
                for (int i = 0; i < z.Length; i++) z[i] = dsm.Valid[i] != 0 ? dsm.Z[i] : dsm.Z[nearest[i]];
            }
            if (padded)
            {
                var grown = new float[pw * ph];
                for (int y = 0; y < ph; y++)
                {
                    int row = Math.Min(y, h - 1) * w;
                    for (int x = 0; x < pw; x++) grown[y * pw + x] = z[row + Math.Min(x, w - 1)];
                }
                z = grown;
            }
            source = Terrain.Write(Resized(dsm, pw, ph, z), Path.Combine(folder, "dsm-context.tif"), z);
        }
        token.ThrowIfCancellationRequested();
        string output = Path.Combine(folder, "ai-inference.tif");
        // The C ABI call has no cancellation hook; cancellation is checked when it returns.
        var result = GeoKernelAI.RunRasterInference(new AIRasterInferenceRequest
        {
            ModelPackagePath = model, RasterPath = source, OutputPath = output, OutputMode = AIRasterOutputMode.FloatBands, Bands = [1],
            Provider = AIExecutionProvider.CPU, IntraOpThreads = threads, InterOpThreads = 1, Tiling = new AITileOptions(tileW, tileH, overlap, overlap),
        }, new SyncProgress(p => progress(40 + (int)(p.Percent * .4), $"AI tiles {p.CompletedTiles} / {p.TotalTiles} · {p.Message}")));
        token.ThrowIfCancellationRequested();
        if (result.GetProperty("cancelled").GetBoolean() || result.GetProperty("width").GetInt32() != pw || result.GetProperty("height").GetInt32() != ph
            || result.GetProperty("bandCount").GetInt32() != 1 || result.GetProperty("dataType").GetString() != "Float32")
            throw new InvalidDataException("AI result dimensions or numeric type do not match the source.");
        double[] transform = [.. result.GetProperty("geoTransform").EnumerateArray().Select(v => v.GetDouble())];
        if (!transform.SequenceEqual(dsm.Transform) || !Terrain.SameProjection(dsm.Projection, result.GetProperty("projectionWkt").GetString() ?? ""))
            throw new InvalidDataException("AI inference changed the source grid or CRS.");
        float[] values = Gdal.Read(output, 1).Bands[0];
        var raw = new float[w * h];
        for (int y = 0; y < h; y++) Array.Copy(values, y * pw, raw, y * w, w);
        var dtm = (float[])raw.Clone();
        int clamped = 0;
        for (int i = 0; i < raw.Length; i++)
        {
            if (dsm.Valid[i] == 0) { raw[i] = dtm[i] = Terrain.NoData; continue; }
            if (!float.IsFinite(raw[i])) throw new InvalidDataException("AI returned a nonfinite valid-source height.");
            if (dtm[i] > dsm.Z[i]) { dtm[i] = dsm.Z[i]; clamped++; }
        }
        var provenance = JsonNode.Parse(result.GetProperty("provenance").GetRawText())!.AsObject();
        return new AiHeight(raw, dtm, provenance, result.GetProperty("processedTiles").GetInt32(), filled, clamped, padded);
    }

    /// <summary>Reports on the calling (SDK worker) thread instead of posting to a synchronization context.</summary>
    sealed class SyncProgress(Action<AIProgress> report) : IProgress<AIProgress>
    {
        public void Report(AIProgress value) => report(value);
    }

    // ----- comparison ---------------------------------------------------------------------------------
    /// <summary>Keep the selected settings but coarsen the working grid until it fits the support-cell cap.</summary>
    public static TerrainParameters EffectiveParameters(TerrainRaster dsm, TerrainParameters p)
    {
        double working = p.Working;
        long Cells(int size) => (long)Math.Ceiling(size / Math.Max(1, Math.Round(working / dsm.Pixel, MidpointRounding.AwayFromZero)));
        while (Cells(dsm.Width) * Cells(dsm.Height) > MaxSupportCells) working += dsm.Pixel;
        return p with { Working = working };
    }

    /// <summary>Column, row, width, height: the shared Zurich crop, or the central 60% of a custom DSM.</summary>
    public static (int X, int Y, int W, int H) Window(TerrainScene scene)
    {
        var d = scene.Dsm;
        if ((bool?)scene.Manifest["standard_full_tile"] == true) return (200, 800, 500, 500);
        int left = d.Width / 5, top = d.Height / 5;
        return (left, top, d.Width - 2 * left, d.Height - 2 * top);
    }

    public static (double XMin, double YMin, double XMax, double YMax) CropExtent(TerrainScene scene)
    {
        var (x, y, w, h) = Window(scene);
        double p = scene.Dsm.Pixel, left = scene.Dsm.Transform[0] + x * p, top = scene.Dsm.Transform[3] - y * p;
        return (left, top - h * p, left + w * p, top);
    }

    public static TerrainScore AreaScore(TerrainRaster dsm, float[] z, TerrainRaster? reference, (int X, int Y, int W, int H) r)
    {
        if (reference == null) return new TerrainScore();
        long count = 0;
        double squared = 0, absolute = 0, signed = 0;
        for (int y = r.Y; y < r.Y + r.H; y++)
            for (int x = r.X; x < r.X + r.W; x++)
            {
                int i = y * dsm.Width + x;
                if (dsm.Valid[i] == 0 || reference.Valid[i] == 0) continue;
                double e = (double)z[i] - reference.Z[i];
                count++;
                squared += e * e;
                absolute += Math.Abs(e);
                signed += e;
            }
        return count == 0 ? new TerrainScore() : new TerrainScore(count, Math.Sqrt(squared / count), absolute / count, signed / count);
    }

    public static float[] Cut(float[] values, int width, (int X, int Y, int W, int H) r)
    {
        var output = new float[r.W * r.H];
        for (int y = 0; y < r.H; y++) Array.Copy(values, (r.Y + y) * width + r.X, output, y * r.W, r.W);
        return output;
    }

    public static TerrainRaster CutGrid(TerrainRaster d, (int X, int Y, int W, int H) r) => new()
    {
        Path = d.Path, Projection = d.Projection, Sha256 = d.Sha256, Width = r.W, Height = r.H, Pixel = d.Pixel,
        Transform = [d.Transform[0] + r.X * d.Pixel, d.Transform[1], d.Transform[2], d.Transform[3] - r.Y * d.Pixel, d.Transform[4], d.Transform[5]],
        Z = Cut(d.Z, d.Width, r), Valid = [],
    };

    public static TerrainResult Display(TerrainRaster d, float[] dtm, float[] removed) => new()
    {
        Parameters = new(), Working = d.Pixel, Seconds = 0, Supports = 0, Dtm = dtm, Removed = removed, Quality = [], Fallback = 0, Summary = new(),
    };

    static string Rgb(TerrainRaster d, string path, (byte[] R, byte[] G, byte[] B) bands) =>
        Gdal.WriteRgb(path, d.Width, d.Height, d.Transform, d.Projection, bands.R, bands.G, bands.B);

    public static AiComparison Evaluate(TerrainScene scene, AiComparison result, JsonObject modelAssets)
    {
        var d = scene.Dsm;
        var rect = Window(scene);
        TerrainScore[] full = [Terrain.Score(d, result.Classical.Dtm, scene.Reference), Terrain.Score(d, result.Ai.Dtm, scene.Reference)];
        TerrainScore[] crop = [AreaScore(d, result.Classical.Dtm, scene.Reference, rect), AreaScore(d, result.Ai.Dtm, scene.Reference, rect)];
        var files = new JsonObject();
        foreach (string name in result.Files.Keys) files[name] = name;
        var report = new JsonObject
        {
            ["schema_version"] = 1,
            ["method"] = "pix_grad_live_and_independent_native_classical",
            ["source_dsm"] = d.Path,
            ["source_sha256"] = d.Sha256,
            ["reference"] = scene.Reference?.Path,
            ["reference_sha256"] = scene.Reference?.Sha256,
            ["reference_used_for_processing"] = false,
            ["classical_uses_ai"] = false,
            ["ai_uses_classical"] = false,
            ["model_archive_sha256"] = modelAssets["sha256"]?.DeepClone(),
            ["model_sha256"] = result.ModelManifest["modelSha256"]?.DeepClone(),
            ["model_manifest"] = result.ModelManifest.DeepClone(),
            ["sdk_inference"] = result.Ai.Provenance.DeepClone(),
            ["ai"] = new JsonObject
            {
                ["overlap_pixels"] = result.Overlap, ["cpu_threads"] = result.Threads, ["processed_tiles"] = result.Ai.Tiles,
                ["context_filled_source_pixels"] = result.Ai.ContextFilled, ["context_fill"] = "four-neighbour DSM-only support propagation",
                ["small_input_edge_padding"] = result.Ai.Padded, ["upper_bound_clamped_pixels"] = result.Ai.Clamped, ["raw_output_retained"] = true,
            },
            ["classical"] = new JsonObject { ["method"] = Terrain.Method, ["parameters"] = result.Classical.Parameters.Json(), ["summary"] = result.Classical.Summary.DeepClone() },
            ["grid"] = new JsonObject
            {
                ["width"] = d.Width, ["height"] = d.Height, ["transform"] = new JsonArray([.. d.Transform.Select(v => (JsonNode)v)]), ["crs_wkt"] = d.Projection,
            },
            ["evaluation"] = new JsonObject
            {
                ["full"] = new JsonObject { ["classical"] = full[0].Json(), ["ai"] = full[1].Json() },
                ["comparison_crop"] = new JsonObject
                {
                    ["pixel_window"] = new JsonObject { ["column"] = rect.X, ["row"] = rect.Y, ["width"] = rect.W, ["height"] = rect.H },
                    ["classical"] = crop[0].Json(), ["ai"] = crop[1].Json(),
                },
            },
            ["source_manifest"] = scene.Manifest.DeepClone(),
            ["output_files"] = files,
            ["attribution"] = "Terrain sample: Federal Office of Topography swisstopo. Model weights: David Perez, Zenodo 7973800, CC BY 4.0; deterministic ONNX adaptation.",
            ["limits"] = "Experimental regional model; fixed development scene, not held-out benchmark. Metre heights, no vertical datum conversion. Source filename years differ.",
        };
        return result with { Full = full, Crop = crop, Report = report };
    }

    /// <summary>Classical first, then an independent AI inference from the same full source DSM.</summary>
    public static AiComparison Compare(TerrainScene scene, string model, TerrainParameters parameters, int overlap, int threads, string folder,
        JsonObject modelAssets, CancellationToken token, Action<int, string> progress)
    {
        var d = scene.Dsm;
        var classical = Terrain.Estimate(d, parameters, token, (p, text) => progress((int)(p * .35), "Classical: " + text));
        var ai = Infer(d, model, overlap, threads, folder, token, progress);
        var manifest = JsonNode.Parse(File.ReadAllText(Path.Combine(model, "geokernel-model.json")))!.AsObject();
        var result = new AiComparison(classical, ai, overlap, threads, model, manifest, folder);
        void Numeric(string name, float[]? heights, byte[]? quality = null)
        {
            token.ThrowIfCancellationRequested();
            result.Files[name] = Terrain.Write(d, Path.Combine(folder, name), heights, quality);
        }
        progress(82, "Writing native-resolution numeric results...");
        Numeric("classical-dtm.tif", classical.Dtm);
        Numeric("ai-dtm.tif", ai.Dtm);
        Numeric("ai-raw.tif", ai.Raw);
        Numeric("classical-removed.tif", classical.Removed);
        var removed = new float[ai.Dtm.Length];
        for (int i = 0; i < removed.Length; i++) removed[i] = d.Valid[i] != 0 ? Math.Max(0f, d.Z[i] - ai.Dtm[i]) : Terrain.NoData;
        Numeric("ai-removed.tif", removed);
        Numeric("classical-qa.tif", null, classical.Quality);
        TerrainResult[] terrains = [classical, Display(d, ai.Dtm, removed)];
        for (int method = 0; method < 2; method++)
            for (int mode = 0; mode < 3; mode++)
            {
                progress(86 + method * 4 + mode, $"Preparing {(method == 1 ? "AI" : "classical")} display...");
                var bands = mode == 0 ? Terrain.Relief(d, terrains[method].Dtm, token) : Terrain.Diagnostic(d, terrains[method], mode == 1 ? 0 : 2, scene.Reference, token);
                result.Maps[method][mode] = Rgb(d, Path.Combine(folder, $"view-{method}-{mode}.tif"), bands);
            }
        progress(96, "Preparing the shared comparison crop and scores...");
        var rect = Window(scene);
        var cropped = CutGrid(d, rect);
        void Crop(string name, float[] values) => result.Files[name] = Terrain.Write(cropped, Path.Combine(folder, name), Cut(values, d.Width, rect));
        Crop("comparison-dsm.tif", d.Z);
        Crop("comparison-classical.tif", classical.Dtm);
        Crop("comparison-ai.tif", ai.Dtm);
        if (scene.Reference != null) Crop("comparison-reference.tif", scene.Reference.Z);
        var scored = Evaluate(scene, result, modelAssets);
        token.ThrowIfCancellationRequested();
        return scored;
    }

    /// <summary>New error maps, crop file and scores for a new evaluation reference; heights unchanged.</summary>
    public static AiComparison AttachReference(TerrainScene scene, AiComparison result, string folder, JsonObject modelAssets, CancellationToken token, Action<int, string> progress)
    {
        var d = scene.Dsm;
        string[][] maps = [(string[])result.Maps[0].Clone(), (string[])result.Maps[1].Clone()];
        float[][] heights = [result.Classical.Dtm, result.Ai.Dtm];
        for (int method = 0; method < 2; method++)
        {
            progress(40 + method * 20, "Preparing reference error maps...");
            maps[method][2] = Rgb(d, Path.Combine(folder, $"reference-{method}.tif"), Terrain.Diagnostic(d, Display(d, heights[method], []), 2, scene.Reference, token));
        }
        var rect = Window(scene);
        var files = new Dictionary<string, string>(result.Files)
        {
            ["comparison-reference.tif"] = Terrain.Write(CutGrid(d, rect), Path.Combine(folder, "comparison-reference.tif"), Cut(scene.Reference!.Z, d.Width, rect)),
        };
        return Evaluate(scene, result with { Maps = maps, Files = files }, modelAssets);
    }

    /// <summary>Stage every file plus comparison.json beside the target, then rename into a new unique folder.</summary>
    public static string Export(AiComparison result, string parent, CancellationToken token, Action<int, string> progress) =>
        Export(result.Files, result.Report, parent, "dsm-dtm-ai", ".dsm-dtm-export-", token, progress);

    /// <summary>Stage the files plus comparison.json beside the target, then rename into a new <c>name-time-id</c> folder.</summary>
    public static string Export(IReadOnlyDictionary<string, string> outputs, JsonObject finalReport, string parent, string name, string stagingPrefix,
        CancellationToken token, Action<int, string> progress)
    {
        string target = Path.Combine(parent, $"{name}-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString("N")[..6]}");
        string staging = Directory.CreateDirectory(Path.Combine(parent, stagingPrefix + Guid.NewGuid().ToString("N")[..6])).FullName;
        try
        {
            int index = 0;
            foreach (var (file, source) in outputs)
            {
                token.ThrowIfCancellationRequested();
                progress(++index * 90 / outputs.Count, $"Saving {file}");
                Terrain.AtomicCopy(source, Path.Combine(staging, file), token);
            }
            var report = (JsonObject)finalReport.DeepClone();
            var files = new JsonObject();
            foreach (string file in outputs.Keys) files[file] = file;
            report["output_files"] = files;
            Terrain.WriteJson(Path.Combine(staging, "comparison.json"), report);
            token.ThrowIfCancellationRequested();
            if (Directory.Exists(target)) throw new IOException("Cannot commit result directory: it already exists.");
            Directory.Move(staging, target);
        }
        catch
        {
            if (Directory.Exists(staging)) Directory.Delete(staging, true);
            throw;
        }
        return target;
    }
}

/// <summary>User-facing text shared by the DsmToDtmAI windows.</summary>
public static class AiText
{
    public static readonly string[] Modes = ["Terrain relief", "Removed height: DSM − DTM", "Signed reference errors"];
    public static readonly string[] Scopes = ["Full input", "Shared comparison crop"];
    public static readonly string[] Suffix = ["DTM · terrain relief", "REMOVED HEIGHT · DSM − DTM", "ERROR · DTM − reference"];
    public static readonly string[] Legends =
    [
        "Identical relief light and contrast. Left: original DSM. Both estimates are independently computed from this full input.",
        "One shared removed-height scale: white 0 m → red ≥20 m. Display saturation only; numeric heights are not clipped.",
        "One shared signed-error scale: blue ≤−2 m · white 0 · red ≥+2 m. Scores use all common valid pixels in the selected scope.",
    ];
    public static readonly string[] Columns = ["Method", "Valid pixels", "RMSE (m)", "MAE (m)", "Bias (m)"];
    public const string ControlsChanged = "Controls changed. Run to update; displayed maps and scores use the last completed settings.";
    public const string ModelReady = "pix_grad ready · 256 × 256 tiles\nVerified ONNX · CPU · experimental";
    public const string ScopeNote = "Source DSM remains on the left. Display and evaluation scope never change the full input used for processing.";
    public const string Attribution = "Terrain: Federal Office of Topography swisstopo.\nWeights: David Perez · CC BY 4.0 · Zenodo 7973800.\nReference is evaluation-only. No vertical datum conversion.";
    public const string Footer = "Both methods process the complete supplied DSM. The shared crop changes only the view and scores. Experimental model; this development scene is not a held-out benchmark.";

    static string G(double v) => v.ToString("0.###", CultureInfo.InvariantCulture);

    public static string Source(TerrainRaster dsm, bool hasReference) =>
        $"{Path.GetFileName(dsm.Path)} · {dsm.Width} × {dsm.Height}\n{G(dsm.Pixel)} m cells · {(hasReference ? "aligned reference" : "no reference")}";

    /// <summary>Score table cells for one method.</summary>
    public static string[] Row(int method, TerrainScore s)
    {
        string F(double v) => s.Count > 0 ? v.ToString("0.000", CultureInfo.InvariantCulture) : "—";
        return [method == 1 ? "AI · pix_grad" : "Classical", s.Count.ToString(CultureInfo.InvariantCulture), F(s.Rmse), F(s.Mae), F(s.Bias)];
    }

    public static string Summary(AiComparison r, TerrainScene scene, bool crop)
    {
        var (_, _, w, h) = Ai.Window(scene);
        double p = scene.Dsm.Pixel;
        return $"Last completed run · {r.Ai.Tiles} AI tiles · overlap {r.Overlap} px · CPU threads {r.Threads} · classical working {G(r.Classical.Working)} m / window {G(r.Classical.Parameters.Window)} m\n" +
               $"Scores: {(crop ? "shared crop" : "full input")} · crop {w} × {h} cells ({G(w * p)} × {G(h * p)} m). Full input was used by both methods. Reference is evaluation-only.";
    }

    /// <summary>Probe text of the source cell containing a world point, or null outside the raster.</summary>
    public static (string Text, double X, double Y)? Probe(TerrainScene scene, AiComparison? result, double x, double y)
    {
        var d = scene.Dsm;
        double cx = (x - d.Transform[0]) / d.Pixel, cy = (d.Transform[3] - y) / d.Pixel;
        if (!double.IsFinite(cx) || !double.IsFinite(cy) || cx < 0 || cy < 0 || cx >= d.Width || cy >= d.Height) return null;
        int col = (int)cx, row = (int)cy, i = row * d.Width + col;
        var inv = CultureInfo.InvariantCulture;
        var text = new StringBuilder($"Source pixel {col} / {row}\n");
        if (d.Valid[i] == 0) text.Append("NoData");
        else
        {
            text.Append(inv, $"DSM {d.Z[i]:0.00} m");
            if (result != null) text.Append(inv, $"\nClassical {result.Classical.Dtm[i]:0.00} m\nAI {result.Ai.Dtm[i]:0.00} m");
            if (scene.Reference is { } reference && reference.Valid[i] != 0) text.Append(inv, $"\nReference {reference.Z[i]:0.00} m");
        }
        return (text.ToString(), d.Transform[0] + (col + .5) * d.Pixel, d.Transform[3] - (row + .5) * d.Pixel);
    }
}
