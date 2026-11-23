using System.Globalization;
using System.IO;
using System.Text.Json.Nodes;
using GeoKernel.Examples.GeoAi;
#if CHANGE_WPF
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.TextPromptChange;

/// <summary>One map layer of a timeline frame: a GeoJSON file in WGS84 and its style.</summary>
internal sealed record FrameLayer(string Name, string Path, string StyleJson);

/// <summary>One row of the review table: a change of the shown interval.</summary>
internal sealed record ChangeRow(string QaId, string Change, double Score, double Area, string State, JsonNode Geometry);

/// <summary>
/// The text-prompt change workflow shared by the Windows Forms and WPF examples: sample data, the
/// SAM 3 package, the native change task (GeoKernel.NET <c>ChangeTask</c>), timeline frames and review.
/// </summary>
internal sealed class ChangeSession : IDisposable
{
    const string SeriesUrl = "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/almere_oosterwold_series_2016_2025_v1.zip";
    const string SeriesSha = "080742d758d9c471ee23cebd379821f61c2e5b28a28947f20ac6e20f5cc44f4d";
    const string Sam3Url = "https://pub-00d214f25f754884b22349c19ac37bbf.r2.dev/geokernel-models/meta-sam3-text-fp32-geokernel-onnx-v1.zip";
    const string Sam3Sha = "95d9bc3ebdec6a6147a9bfa97325712bfbb77fccbd6bb3f20be453c381820931";

    /// <summary>Beeldmateriaal Nederland / PDOK annual orthophotos, 0.5 m, EPSG:25831; no 2021 layer.</summary>
    public static readonly int[] Years = [2016, 2017, 2018, 2019, 2020, 2022, 2023, 2024, 2025];
    static readonly double[] DemoArea = [657885, 5800734, 658205, 5801054];   // EPSG:25831, 320 m
    public static readonly double[] DemoView = [5.317289, 52.334073, 5.322132, 52.337039];  // the same in WGS84

    public const string Legend = "■ yellow: present   ■ red: new on this date   □ blue: gone since the previous date";
    public const string Help =
        "Each date is segmented once with SAM 3 and objects are tracked from date to date; an object missed on one date keeps its " +
        "identity. Changes scoring 0.75 or more are accepted automatically, 0.5–0.75 wait for review.\n\n" +
        "The SAM 3 text package (3.1 GB) is downloaded and SHA-256 checked on the first run, the 9-date series (56 MB) at startup. " +
        "Set GEOKERNEL_SAM3_TEXT_PACKAGE to use an installed package.\n\n" +
        "Imagery: Beeldmateriaal Nederland / PDOK, CC BY 4.0. Model: Meta SAM 3 (SAM License).";

    static readonly (string Name, string Style)[] Styles =
    [
        ("Present", """{"fillColor":"#FFD000","fillOpacity":40,"lineColor":"#D4A000","lineWidth":1.6}"""),
        ("New on this date", """{"fillColor":"#FF2020","fillOpacity":120,"lineColor":"#FF6060","lineWidth":1.6}"""),
        ("Gone since the previous date", """{"fillColor":"#2070FF","fillOpacity":0,"lineColor":"#2070FF","lineWidth":1.6}"""),
    ];

    readonly string workspace = Directory.CreateTempSubdirectory("geokernel-change-").FullName;
    int revision;

    public string? SeriesDirectory { get; private set; }
    public ChangeTask? Job { get; private set; }
    public JsonObject? Result { get; private set; }
    public IReadOnlyList<string> Dates => Result?["dates"]!.AsArray().Select(d => (string)d!).ToList() ?? [];

    public static string RasterName(int year) => $"oosterwold_{year}-epsg25831-50cm.tif";
    public string RasterPath(int year) => Path.Combine(SeriesDirectory!, RasterName(year));

    public async Task PrepareSeriesAsync(IProgress<DownloadProgress> progress, CancellationToken cancellation)
    {
        string first = await ModelAssets.PrepareAsync(SeriesUrl, SeriesSha, RasterName(Years[0]), progress, cancellation);
        SeriesDirectory = Path.GetDirectoryName(first)!;
    }

    /// <summary>Downloads the model if needed and starts the native task; poll <see cref="Poll"/>.</summary>
    public async Task StartAsync(string prompt, double score, bool series, bool demoArea, string provider,
        IProgress<DownloadProgress> progress, CancellationToken cancellation)
    {
        string? package = Environment.GetEnvironmentVariable("GEOKERNEL_SAM3_TEXT_PACKAGE");
        if (string.IsNullOrEmpty(package))
            package = Path.GetDirectoryName(await ModelAssets.PrepareAsync(Sam3Url, Sam3Sha, "sam3-package.json", progress, cancellation))!;
        var dates = new JsonArray();
        foreach (int year in series ? Years : [Years[0], Years[^1]])
            dates.Add(new JsonObject { ["date"] = $"{year}-07-01", ["rasterPath"] = RasterPath(year) });
        var request = new JsonObject
        {
            ["prompt"] = prompt,
            ["modelPackagePath"] = package,
            ["dates"] = dates,
            ["scoreThreshold"] = score,
            ["minimumArea"] = 20,
            ["maximumGap"] = 1,
            ["provider"] = provider.ToLowerInvariant(),
            ["intraOpThreads"] = Math.Max(2, Environment.ProcessorCount / 2),
        };
        if (demoArea)
            request["extent"] = new JsonArray(DemoArea.Select(v => (JsonNode)v).ToArray());
        var task = ChangeTask.Start(request);
        Job?.Dispose();
        Job = task;
        Result = null;
    }

    /// <summary>The task's state; loads the result once it completes.</summary>
    public JsonObject Poll()
    {
        var status = Job!.Status();
        if ((string?)status["state"] == "completed" && Result is null)
            Result = Job.Result();
        return status;
    }

    /// <summary>The status message with the elapsed time (SAM 3 needs seconds per tile on CPU).</summary>
    public static string Describe(JsonObject status) => $"{(string?)status["message"]} ({(int?)status["elapsedSeconds"] ?? 0} s)";

    public string Summary()
    {
        int appeared = Result!["steps"]!.AsArray().Sum(s => (int)s!["appeared"]!);
        return $"{Result["objects"]!["features"]!.AsArray().Count} objects tracked over {Dates.Count} dates in " +
               $"{(double)Result["seconds"]!:F0} s; {appeared} appeared after {Dates[0][..4]}. Move the timeline or play it.";
    }

    JsonObject? Step(int date) => Result is not null && date > 0 ? Result["steps"]![date - 1]!.AsObject() : null;

    /// <summary>The layers to draw on a date (bottom to top) and a one-line description.</summary>
    public (IReadOnlyList<FrameLayer> Layers, string Label) Frame(int date)
    {
        string day = Dates[date][..10];
        var present = ChangeTask.PresentOn(Result!, day).ToList();
        var fresh = present.Where(f => date > 0 && ((string?)f["properties"]!["appeared"] ?? "").StartsWith(day, StringComparison.Ordinal)).ToList();
        var step = Step(date);
        var gone = step?["features"]!["features"]!.AsArray().Where(f => (string?)f!["properties"]!["change"] == "disappeared")
                       .Select(f => f!.AsObject()).ToList() ?? [];
        var groups = new[] { present.Except(fresh).ToList(), fresh, gone };
        var layers = new List<FrameLayer>();
        for (int i = 0; i < groups.Length; ++i)
        {
            if (groups[i].Count == 0) continue;
            var collection = new JsonObject { ["type"] = "FeatureCollection", ["features"] = new JsonArray(groups[i].Select(f => f.DeepClone()).ToArray()) };
            string path = Path.Combine(workspace, $"layer-{++revision}.geojson");
            File.WriteAllText(path, collection.ToJsonString());
            layers.Add(new FrameLayer(Styles[i].Name, path, Styles[i].Style));
        }
        string label = step is null
            ? $"{day[..4]} · {present.Count} present (first date)"
            : $"{day[..4]} · {present.Count} present · {step["appeared"]} new · {step["disappeared"]} gone since {Dates[date - 1][..4]}";
        return (layers, label);
    }

    public bool CanReview(int date) => Step(date) is not null;

    public IReadOnlyList<ChangeRow> Rows(int date)
    {
        var step = Step(date);
        if (step is null) return [];
        var states = step["review"]!["states"]!.AsObject();
        return step["features"]!["features"]!.AsArray().Select(f =>
        {
            var p = f!["properties"]!;
            string id = (string)p["qa_id"]!;
            var state = states[id];
            string label = (string?)state?["state"] ?? (string)p["qa_state"]!;
            if (state?["reviewer"] is JsonNode who && (string?)who is { Length: > 0 } name) label += $" ({name})";
            return new ChangeRow(id, (string)p["change"]!, (double)p["score"]!, (double)p["area_m2"]!, label, f["geometry"]!);
        }).ToList();
    }

    /// <summary>action: approve / reject (with the row's QA id) or approvePending.</summary>
    public void Review(int date, string action, string reviewer, string? qaId)
    {
        var step = Step(date) ?? throw new InvalidOperationException("The first date has no interval to review.");
        step["review"] = Job!.Review(date - 1, action, reviewer, qaId);
    }

    public string SaveAccepted(int date, string path)
    {
        File.Delete(path);  // the dialog already confirmed overwriting
        var exported = Job!.Export(path, "accepted", date - 1);
        return $"{exported["written"]} accepted changes written to {path} (EPSG:25831, with QA state and reviewer).";
    }

    public string SaveSeries(string path)
    {
        File.Delete(path);
        Job!.Export(path);
        return $"Series written to {path}: layer 'objects' (lifespans) and 'changes' (every interval).";
    }

    /// <summary>A WGS84 extent around a change geometry.</summary>
    public static double[] Around(JsonNode geometry)
    {
        var points = new List<(double X, double Y)>();
        void Walk(JsonNode? node)
        {
            if (node is JsonArray array && array.Count >= 2 && array[0] is JsonValue && array[1] is JsonValue)
                points.Add(((double)array[0]!, (double)array[1]!));
            else if (node is JsonArray children)
                foreach (var child in children) Walk(child);
        }
        Walk(geometry["coordinates"]);
        const double pad = 0.0006;
        return [points.Min(p => p.X) - pad, points.Min(p => p.Y) - pad, points.Max(p => p.X) + pad, points.Max(p => p.Y) + pad];
    }

    public static string Format(double value, string format) => value.ToString(format, CultureInfo.CurrentCulture);

    public void Dispose()
    {
        Job?.Dispose();
        Job = null;
        try { Directory.Delete(workspace, true); } catch (IOException) { }
    }
}
