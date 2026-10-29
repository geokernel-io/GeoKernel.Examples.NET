using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using GeoKernel.Examples.GeoAi;
#if GEOAI_WPF
using GeoKernel.NET.Wpf;
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.BuildingChange;

internal sealed record Policy(double Area, double Coverage, double Tolerance, double Own, double Other)
{
    internal static Policy Conservative = new(50, .10, 2, .75, .20), Balanced = new(20, .20, 1, .55, .35);
}
internal sealed record Inputs(string Before, string After, string Model);
internal sealed record Prediction(JsonElement Native, List<Polygon> Polygons, int[] Labels, float[] Scores, Grid Grid);

internal static class ChangeCore
{
    internal const string DataUrl = "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/almere_oosterwold_change_2016_2025_v1.zip";
    internal const string DataHash = "ecc35ef2c6bbdd586adc6ecc812ce1c74aee62fbf8e404de1d63b75bf7137a88";
    internal const string BeforeHash = "db723783db4c8738a716fe8b898de2e3099bab9394e335bd489db2412ae0c5c3", AfterHash = "7b1b12d8b27cd0714f140415004daf9fd0000495eda31366c7cd4aab218caa82";
    internal const string ModelUrl = "https://pub-00d214f25f754884b22349c19ac37bbf.r2.dev/geokernel-models/hotosm-dinov3s-buildings-geokernel-onnx-v1.zip", ModelHash = "3a6cd11e013f81b46b3db700feddef2d98319780285b8320e1200573684b24bb";
    internal const string Attribution = "Beeldmateriaal Nederland / PDOK, 2016 & 2025, CC BY 4.0. WMS crop/reprojection rendered at 0.5 m. Model: HOTOSM / Meta DINOv3; retain package licenses.";
    internal static readonly double[] Bounds = [657533, 5800382, 658557, 5801406], Demo = [657885, 5800734, 658205, 5801054];
    internal static string Hash(JsonNode node) => Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(node.ToJsonString())));
    internal static string FileHash(string path)
    {
        using var f = File.OpenRead(path);
        return Convert.ToHexStringLower(SHA256.HashData(f));
    }
    internal static async Task<Inputs> Prepare(IProgress<DownloadProgress> progress, CancellationToken token)
    {
        string before = await ModelAssets.PrepareAsync(DataUrl, DataHash, "oosterwold_2016-epsg25831-50cm.tif", progress, token), after = Path.Combine(Path.GetDirectoryName(before)!, "oosterwold_2025-epsg25831-50cm.tif");
        if (FileHash(before) != BeforeHash || FileHash(after) != AfterHash)
            throw new InvalidDataException("Imagery SHA-256 mismatch.");
        string model = await ModelAssets.PrepareAsync(ModelUrl, ModelHash, "geokernel-model.json", progress, token);
        return new(before, after, Path.GetDirectoryName(model)!);
    }
    internal static async Task<JsonObject> Analyze(Inputs input, double[] bounds, Policy policy, double threshold, string provider, IProgress<DownloadProgress> progress, CancellationToken token)
    {
        D.GDALAllRegister();
        var timer = Stopwatch.StartNew();
        var predictions = new List<Prediction>();
        string work = Path.Combine(Path.GetTempPath(), "geokernel-change-" + Guid.NewGuid());
        Directory.CreateDirectory(work);
        try
        {
            for (int date = 0; date < 2; date++)
            {
                token.ThrowIfCancellationRequested();
                int year = date == 0 ? 2016 : 2025;
                string crop = Path.Combine(work, $"{year}.tif");
                D.Crop(date == 0 ? input.Before : input.After, bounds, crop);
                await using var task = AITask.Start(JsonSerializer.SerializeToElement(new
                {
                    task = "hotosm-buildings",
                    modelPackagePath = input.Model,
                    rasterPath = crop,
                    threshold,
                    provider,
                    intraOpThreads = 2
                }));
                bool cancelled = false;
                while (true)
                {
                    if (token.IsCancellationRequested && !cancelled)
                    {
                        task.Cancel();
                        cancelled = true;
                    }
                    var status = task.Status();
                    progress.Report(new(date * 45 + (int)(status.GetProperty("percent").GetInt32() * .3), $"{year}: {status.GetProperty("message").GetString()}"));
                    string state = status.GetProperty("state").GetString()!;
                    if (state == "failed")
                        throw new InvalidOperationException(status.GetProperty("error").GetString());
                    if (state == "cancelled")
                        throw new OperationCanceledException();
                    if (state == "completed")
                        break;
                    await Task.Delay(80);
                }
                token.ThrowIfCancellationRequested();
                var native = task.Result();
                Grid grid;
                using (var ds = new D.Ds(D.Open(crop)))
                    grid = D.Metadata(ds.Value);
                var (polygons, labels) = D.Polygons(native.GetProperty("vectorPath").GetString()!, grid);
                string raw = Path.Combine(work, $"{year}-scores.tif");
                progress.Report(new(date * 45 + 31, $"{year}: matching scores…"));
                // The public raw-score call has no mid-pass cancellation. Close waits
                // for this pass and cancellation is checked before publication.
                await GeoKernelAI.RunRasterInferenceAsync(new AIRasterInferenceRequest { ModelPackagePath = input.Model, RasterPath = crop, OutputPath = raw, OutputMode = AIRasterOutputMode.FloatBands, Bands = [1, 2, 3], Provider = provider == "cuda" ? AIExecutionProvider.CUDA : AIExecutionProvider.CPU, IntraOpThreads = 2, InterOpThreads = 1 });
                token.ThrowIfCancellationRequested();
                float[] scores;
                using (var ds = new D.Ds(D.Open(raw)))
                {
                    if (!D.Metadata(ds.Value).Same(grid))
                        throw new InvalidDataException("Score and instance grids differ.");
                    scores = D.Read<float>(ds.Value, 1, 6);
                }
                for (int i = 0; i < scores.Length; i++)
                {
                    if (!float.IsFinite(scores[i]))
                        throw new InvalidDataException("Nonfinite model logits.");
                    scores[i] = (float)(1 / (1 + Math.Exp(-Math.Clamp(scores[i], -80, 80))));
                }
                predictions.Add(new(native, polygons, labels, scores, grid));
            }
            if (!predictions[0].Grid.Same(predictions[1].Grid))
                throw new InvalidDataException("Date grids differ.");
            JsonArray features = [];
            int edgeCount = 0, matched = 0;
            foreach (int date in new[] { 1, 0 })
            {
                token.ThrowIfCancellationRequested();
                var own = predictions[date];
                var other = predictions[1 - date];
                var grid = own.Grid;
                int n = own.Labels.Max() + 1;
                var counts = new int[n];
                var covered = new int[n];
                var ownSum = new double[n];
                var otherSum = new double[n];
                var edge = new HashSet<int>();
                var foreground = Dilate(other.Scores, grid.Width, grid.Height, (int)Math.Ceiling(policy.Tolerance / .5));
                for (int y = 0; y < grid.Height; y++)
                {
                    token.ThrowIfCancellationRequested();
                    for (int x = 0; x < grid.Width; x++)
                    {
                        int pixel = y * grid.Width + x, id = own.Labels[pixel];
                        counts[id]++;
                        covered[id] += foreground[pixel] ? 1 : 0;
                        ownSum[id] += own.Scores[pixel];
                        otherSum[id] += other.Scores[pixel];
                        if (id > 0 && (x == 0 || y == 0 || x == grid.Width - 1 || y == grid.Height - 1))
                            edge.Add(id);
                    }
                }
                edgeCount += edge.Count;
                var seen = new HashSet<int>();
                foreach (var polygon in own.Polygons)
                {
                    int id = polygon.Id;
                    if (edge.Contains(id) || counts[id] * .25 < policy.Area || polygon.Area < policy.Area)
                        continue;
                    double mean = ownSum[id] / counts[id], opposite = otherSum[id] / counts[id], overlap = (double)covered[id] / counts[id];
                    if (mean < policy.Own || opposite > policy.Other || overlap >= policy.Coverage)
                    {
                        if (seen.Add(id))
                            matched++;
                        continue;
                    }
                    var (geo, _) = D.Geometry(polygon.Geometry, 25831, 4326);
                    string kind = date == 1 ? "new" : "removed", identifier = $"{kind}-{features.Count + 1:0000}";
                    features.Add(new JsonObject { { "type", "Feature" }, { "id", identifier }, { "geometry", geo }, { "properties", new JsonObject { { "candidate_id", identifier }, { "change_type", kind }, { "review_status", "pending" }, { "review_note", "" }, { "reviewed_by", "" }, { "area_m2", polygon.Area }, { "source_instance_id", id }, { "building_score", mean }, { "other_date_score", opposite }, { "other_date_coverage", overlap }, { "before_year", 2016 }, { "after_year", 2025 } } } });
                }
            }
            var provenance = new JsonObject { { "dataset_url", DataUrl }, { "dataset_archive_sha256", DataHash }, { "before_sha256", BeforeHash }, { "after_sha256", AfterHash }, { "before_year", 2016 }, { "after_year", 2025 }, { "source_crs", "EPSG:25831" }, { "model_id", "hotosm-dinov3s-buildings" }, { "model_archive_sha256", ModelHash }, { "model_sha256", FileHash(Path.Combine(input.Model, "model.onnx")) }, { "threshold", threshold }, { "filters", JsonSerializer.SerializeToNode(policy, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }) }, { "weak_building_threshold", .25 }, { "provider", predictions[0].Native.GetProperty("provider").GetString() }, { "analysis_geotransform", JsonSerializer.SerializeToNode(predictions[0].Grid.Transform) }, { "analysis_width", predictions[0].Grid.Width }, { "analysis_height", predictions[0].Grid.Height }, { "elapsed_seconds", timer.Elapsed.TotalSeconds }, { "attribution", Attribution }, { "imagery_license", "https://creativecommons.org/licenses/by/4.0/" }, { "method", "Native HOTOSM instances plus second FloatBands score pass; sigmoid after logit blending. Same filter policy as Qt; not bit-identical to per-tile decoded scoring. Predictions require review." } };
            progress.Report(new(100, "Complete"));
            return new JsonObject { { "type", "FeatureCollection" }, { "features", features }, { "geokernel_provenance", provenance }, { "stats", new JsonObject { { "before", predictions[0].Native.GetProperty("instances").GetInt32() }, { "after", predictions[1].Native.GetProperty("instances").GetInt32() }, { "edge", edgeCount }, { "matched", matched }, { "elapsed", timer.Elapsed.TotalSeconds } } } };
        }
        finally { Directory.Delete(work, true); }
    }
    internal static bool[] Dilate(float[] scores, int w, int h, int radius)
    {
        var a = new bool[scores.Length];
        var b = new bool[scores.Length];
        for (int y = 0; y < h; y++)
        {
            int count = 0;
            for (int x = 0; x < Math.Min(w, radius + 1); x++)
                count += scores[y * w + x] >= .25 ? 1 : 0;
            for (int x = 0; x < w; x++)
            {
                a[y * w + x] = count > 0;
                if (x - radius >= 0)
                    count -= scores[y * w + x - radius] >= .25 ? 1 : 0;
                if (x + radius + 1 < w)
                    count += scores[y * w + x + radius + 1] >= .25 ? 1 : 0;
            }
        }
        for (int x = 0; x < w; x++)
        {
            int count = 0;
            for (int y = 0; y < Math.Min(h, radius + 1); y++)
                count += a[y * w + x] ? 1 : 0;
            for (int y = 0; y < h; y++)
            {
                b[y * w + x] = count > 0;
                if (y - radius >= 0)
                    count -= a[(y - radius) * w + x] ? 1 : 0;
                if (y + radius + 1 < h)
                    count += a[(y + radius + 1) * w + x] ? 1 : 0;
            }
        }
        return b;
    }
    internal static bool Update(JsonObject feature, JsonObject geometry)
    {
        if (Hash(feature["geometry"]!) == Hash(geometry))
            return false;
        var (_, area) = D.Geometry(geometry);
        feature["geometry"] = geometry;
        var p = feature["properties"]!.AsObject();
        p["area_m2"] = area;
        p["review_status"] = "pending";
        p["reviewed_by"] = "";
        p["review_note"] = "Geometry edited; review again.";
        feature.Remove("approved_geometry_sha256");
        return true;
    }
    internal static void Review(JsonObject feature, string status, string actor, string note)
    {
        if (string.IsNullOrWhiteSpace(actor) || !new[] { "approved", "rejected", "pending" }.Contains(status))
            throw new ArgumentException("Enter a reviewer and a valid decision.");
        if (status == "approved")
        {
            var (_, area) = D.Geometry(feature["geometry"]!);
            feature["properties"]!["area_m2"] = area;
            feature["approved_geometry_sha256"] = Hash(feature["geometry"]!);
        }
        else
            feature.Remove("approved_geometry_sha256");
        feature["properties"]!["review_status"] = status;
        feature["properties"]!["reviewed_by"] = actor.Trim();
        feature["properties"]!["review_note"] = note.Trim();
    }
    internal static void Export(JsonObject result, string path)
    {
        JsonArray approved = [];
        foreach (var node in result["features"]!.AsArray())
        {
            var f = node!.AsObject();
            if (f["properties"]!["review_status"]!.GetValue<string>() != "approved")
                continue;
            if (f["approved_geometry_sha256"]?.GetValue<string>() != Hash(f["geometry"]!))
                throw new InvalidOperationException("Geometry changed; approve again.");
            D.Geometry(f["geometry"]!);
            var copy = f.DeepClone().AsObject();
            copy.Remove("approved_geometry_sha256");
            approved.Add(copy);
        }
        if (approved.Count == 0)
            throw new InvalidOperationException("Approve at least one candidate.");
        var output = new JsonObject { { "type", "FeatureCollection" }, { "features", approved }, { "geokernel_provenance", result["geokernel_provenance"]!.DeepClone() } };
        string temporary = path + ".part";
        File.WriteAllText(temporary, output.ToJsonString(new()
        {
            WriteIndented = true
        }));
        File.Move(temporary, path, true);
    }
    internal static JsonObject Rectangle(double[] b) => JsonNode.Parse(JsonSerializer.Serialize(new { type = "Polygon", coordinates = new[] { new[] { new[] { b[0], b[1] }, new[] { b[2], b[1] }, new[] { b[2], b[3] }, new[] { b[0], b[3] }, new[] { b[0], b[1] } } } }))!.AsObject();
    internal static double[] Envelope(JsonNode geometry)
    {
        var points = new List<double[]>();
        void Walk(JsonNode n)
        {
            var a = n.AsArray();
            if (a[0] is JsonValue)
                points.Add([a[0]!.GetValue<double>(), a[1]!.GetValue<double>()]);
            else
                foreach (var child in a)
                    Walk(child!);
        }
        Walk(geometry["coordinates"]!);
        return [points.Min(p => p[0]), points.Min(p => p[1]), points.Max(p => p[0]), points.Max(p => p[1])];
    }
}
