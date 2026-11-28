using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using GeoKernel.Examples.GeoAi;
#if SEARCH_WPF
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.EmbeddingSearch;

/// <summary>One query: the seed place and year, the CRS, the extent and the years.</summary>
internal sealed record SearchRequest(JsonObject Place, int SeedYear, string Crs, JsonObject Extent, int FirstYear, int LastYear, int Maximum = 20);

/// <summary>
/// The spatial embedding search shared by the Windows Forms and WPF examples. The AlphaEarth embeddings of
/// the Netherlands (2018 / 2021 / 2024, 1 km cells in UTM zones 31N and 32N, GeoKernel.SampleData) become
/// 227 424 records of a GeoKernel.NET <c>SpatialEmbeddingIndex</c>, each with its extent, CRS and year.
/// A query takes a seed cell's embedding with a CRS, an extent and years; the index returns the most similar
/// records that pass all three filters and counts what each filter rejected.
/// </summary>
internal sealed class EmbeddingSearchSession : IDisposable
{
    const string DataUrl = "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/netherlands_alphaearth_2018_2024_1km_v1.zip";
    const string DataSha = "6d28e052793bd19c44e6c6e0ce22b52d12e392cbf83b5df931aa90652579762b";
    public static readonly int[] Years = [2018, 2021, 2024];
    public static readonly (string Value, string Label)[] CrsChoices =
    [
        ("EPSG:32631", "EPSG:32631 — UTM 31N, west of 6° E"),
        ("EPSG:32632", "EPSG:32632 — UTM 32N, east of 6° E"),
    ];
    public const string Attribution =
        "The AlphaEarth Foundations Satellite Embedding dataset is produced by Google and Google DeepMind (CC BY 4.0); " +
        "2018, 2021 and 2024 layers, Source Cooperative mirror, resampled to 1 km by GeoKernel.";
    static readonly string[] ResultLayers = ["query-extent", "matches", "seed"];

    string directory = "";
    JsonObject meta = [];
    byte[] raw = [];
    byte[] centers = [];
    int scale = 127;
    readonly string output = Directory.CreateTempSubdirectory("geokernel-embedding-search-").FullName;

    public SpatialEmbeddingIndex? Index { get; private set; }
    public JsonObject? Last { get; private set; }
    public double LastMilliseconds { get; private set; }
    public IEnumerable<JsonObject> Places => meta["places"]!.AsArray().Select(p => p!.AsObject());
    public IEnumerable<JsonObject> Extents(string crs) => meta["extents"]!.AsArray().Select(e => e!.AsObject()).Where(e => (string)e["crs"]! == crs);
    public JsonObject Place(string name) => Places.First(p => (string)p["name"]! == name);
    public JsonObject Extent(string crs, string name) => Extents(crs).First(e => (string)e["name"]! == name);

    /// <summary>Downloads the data once and builds the index (on a worker thread).</summary>
    public async Task PrepareAsync(IProgress<DownloadProgress> progress, CancellationToken cancellation)
    {
        var manifest = await ModelAssets.PrepareAsync(DataUrl, DataSha, "embeddings.json", progress, cancellation);
        directory = Path.GetDirectoryName(manifest)!;
        meta = JsonNode.Parse(await File.ReadAllTextAsync(manifest, cancellation))!.AsObject();
        raw = await File.ReadAllBytesAsync(Path.Combine(directory, "embeddings.bin"), cancellation);
        centers = await File.ReadAllBytesAsync(Path.Combine(directory, "centers.bin"), cancellation);
        scale = (int)meta["scale"]!;
        Index = await Task.Run(() => Build(progress, cancellation), cancellation);
    }

    // One record per cell with data: its 1 km extent in the zone's CRS and the year as time range.
    SpatialEmbeddingIndex Build(IProgress<DownloadProgress> progress, CancellationToken cancellation)
    {
        var index = new SpatialEmbeddingIndex(64, new JsonObject { ["method"] = "hnsw", ["efConstruction"] = 64 });
        long total = (long)meta["records"]!, added = 0;
        var timer = System.Diagnostics.Stopwatch.StartNew();
        foreach (var grid in meta["grids"]!.AsArray().Select(g => g!.AsObject()))
        {
            int year = (int)grid["year"]!, width = (int)grid["width"]!, height = (int)grid["height"]!;
            string crs = (string)grid["crs"]!, zone = crs[^2..];
            var period = YearsRange(year, year);
            for (var row = 0; row < height; row++)
            {
                if (cancellation.IsCancellationRequested)
                {
                    index.Dispose();
                    cancellation.ThrowIfCancellationRequested();
                }
                for (var column = 0; column < width; column++)
                {
                    var embedding = Embedding(grid, column, row);
                    if (embedding is null)
                        continue;
                    index.Add(new JsonObject
                    {
                        ["id"] = $"{year}-{zone}-{column}-{row}", ["embedding"] = embedding, ["crs"] = crs,
                        ["extent"] = CellExtent(grid, column, row), ["time"] = period.DeepClone(),
                        ["metadata"] = new JsonObject { ["year"] = year, ["column"] = column, ["row"] = row },
                    });
                    added++;
                }
                if (row % 32 == 0)
                    progress.Report(new((int)(100 * added / total), $"Building the index: {added:N0} of {total:N0} records…"));
            }
        }
        progress.Report(new(100, $"Index built: {added:N0} records in {timer.Elapsed.TotalSeconds:F1} s."));
        return index;
    }

    static JsonObject YearsRange(int first, int last) =>
        new() { ["startUtc"] = $"{first}-01-01T00:00:00Z", ["endUtc"] = $"{last + 1}-01-01T00:00:00Z" };

    JsonObject Grid(int year, string crs) =>
        meta["grids"]!.AsArray().Select(g => g!.AsObject()).First(g => (int)g["year"]! == year && (string)g["crs"]! == crs);

    JsonArray? Embedding(JsonObject grid, int column, int row)
    {
        var start = (long)grid["offset"]! + ((long)row * (int)grid["width"]! + column) * 64;
        var values = new JsonArray();
        var any = false;
        for (var i = 0; i < 64; i++)
        {
            var value = unchecked((sbyte)raw[start + i]);
            any |= value != -128;
            values.Add(value / (double)scale);
        }
        return any ? values : null;
    }

    static JsonArray CellExtent(JsonObject grid, int column, int row)
    {
        double size = (double)grid["cellSize"]!, x0 = (double)grid["originX"]! + column * size, y1 = (double)grid["originY"]! - row * size;
        return [x0, y1 - size, x0 + size, y1];
    }

    (double Lon, double Lat) Center(string crs, int column, int row)
    {
        var entry = meta["centers"]!.AsArray().Select(c => c!.AsObject()).First(c => (string)c["crs"]! == crs);
        var offset = (int)(long)entry["offset"]! + (row * (int)entry["width"]! + column) * 8;
        return (BitConverter.ToSingle(centers, offset), BitConverter.ToSingle(centers, offset + 4));
    }

    /// <summary>Adds the three false-colour previews (RD New, the map's CRS) at the bottom and shows 2024.</summary>
    public void AttachBackgrounds(GeoKernelViewerControl viewer)
    {
        foreach (var year in Years)
        {
            if (!viewer.AddLayerFile(Path.Combine(directory, $"preview_{year}.tif")))
                throw new InvalidOperationException($"Cannot open preview_{year}.tif");
            viewer.SetLayerOpacity(0, 0.75);
        }
        ShowBackground(viewer, 2024);
        viewer.ViewExtent = new GeoKernelExtent(0, 300000, 300000, 625000);
    }

    // The previews are the bottom three layers: 2018 at the very bottom, then 2021, then 2024.
    static void ShowBackground(GeoKernelViewerControl viewer, int year)
    {
        var count = viewer.LayerCount;
        for (var i = 0; i < Years.Length; i++)
            viewer.SetLayerVisible(count - 1 - i, Years[i] == year);
    }

    /// <summary>The query: the seed cell's embedding with the CRS, extent and time filters. True when every match passes them.</summary>
    public bool Run(GeoKernelViewerControl viewer, SearchRequest r)
    {
        int column = (int)r.Place["column"]!, row = (int)r.Place["row"]!;
        var query = new JsonObject
        {
            ["embedding"] = Embedding(Grid(r.SeedYear, (string)r.Place["crs"]!), column, row), ["extent"] = r.Extent["extent"]!.DeepClone(),
            ["crs"] = r.Crs, ["time"] = YearsRange(r.FirstYear, r.LastYear), ["maximumResults"] = r.Maximum,
        };
        var timer = System.Diagnostics.Stopwatch.StartNew();
        Last = Index!.Query(query);
        LastMilliseconds = timer.Elapsed.TotalMilliseconds;
        var matches = Last["matches"]!.AsArray().Select(m => m!.AsObject()).ToList();
        ShowResults(viewer, r, matches);
        ShowBackground(viewer, r.LastYear);
        // The guarantee: nothing comes from outside the filters.
        var box = r.Extent["extent"]!.AsArray().Select(v => (double)v!).ToArray();
        return matches.Count > 0 && matches.All(m =>
        {
            var e = m["extent"]!.AsArray().Select(v => (double)v!).ToArray();
            var year = (int)m["metadata"]!["year"]!;
            return (string)m["crs"]! == r.Crs && year >= r.FirstYear && year <= r.LastYear
                   && e[0] <= box[2] && e[2] >= box[0] && e[1] <= box[3] && e[3] >= box[1];
        });
    }

    /// <summary>What the filters did, in the words of the result.</summary>
    public string Describe(SearchRequest r)
    {
        var l = Last!;
        var budget = (bool)l["candidateBudgetReached"]! ? " (evaluation budget reached)" : "";
        var cross = (string)r.Place["crs"]! != r.Crs
            ? $"The seed is a cell in {r.Place["crs"]}; its embedding is compared with records in {r.Crs}, never its extent."
            : "Every match shares the query's CRS, intersects its extent and lies in its years.";
        return $"{l["matches"]!.AsArray().Count} matches in {LastMilliseconds:F1} ms among {Index!.Count:N0} records.{Environment.NewLine}" +
               $"Similarity evaluated: {(long)l["candidatesExamined"]!:N0}{budget}{Environment.NewLine}" +
               $"Rejected by the CRS filter (not {r.Crs}): {(long)l["crsRejected"]!:N0}{Environment.NewLine}" +
               $"Rejected by the extent filter (outside {r.Extent["name"]}): {(long)l["spatialRejected"]!:N0}{Environment.NewLine}" +
               $"Rejected by the time filter (not {r.FirstYear}–{r.LastYear}): {(long)l["temporalRejected"]!:N0}{Environment.NewLine}{cross}";
    }

    public IEnumerable<string> MatchLines() =>
        Last!["matches"]!.AsArray().Select((m, i) => $"#{i + 1}  {m!["metadata"]!["year"]}  similarity {(double)m["similarity"]!:F3}  ({m["id"]})");

    // GeoJSON layers in WGS84 (cell centres from centers.bin); the viewer projects them onto the map.
    void ShowResults(GeoKernelViewerControl viewer, SearchRequest r, List<JsonObject> matches)
    {
        static JsonObject Point(double lon, double lat, JsonObject properties) => new()
        {
            ["type"] = "Feature", ["properties"] = properties,
            ["geometry"] = new JsonObject { ["type"] = "Point", ["coordinates"] = new JsonArray(lon, lat) },
        };
        static JsonObject Collection(IEnumerable<JsonNode> features) => new() { ["type"] = "FeatureCollection", ["features"] = new JsonArray(features.ToArray()) };
        var layers = new Dictionary<string, JsonObject>
        {
            ["query-extent"] = Collection([new JsonObject
            {
                ["type"] = "Feature", ["properties"] = new JsonObject { ["name"] = (string)r.Extent["name"]! },
                ["geometry"] = new JsonObject { ["type"] = "Polygon", ["coordinates"] = new JsonArray(r.Extent["outline"]!.DeepClone()) },
            }]),
            ["matches"] = Collection(matches.Select((m, i) =>
            {
                var (lon, lat) = Center((string)m["crs"]!, (int)m["metadata"]!["column"]!, (int)m["metadata"]!["row"]!);
                return (JsonNode)Point(lon, lat, new JsonObject { ["rank"] = i + 1, ["similarity"] = (double)m["similarity"]!, ["year"] = (int)m["metadata"]!["year"]! });
            })),
            ["seed"] = Collection([Point((double)r.Place["lon"]!, (double)r.Place["lat"]!, new JsonObject { ["name"] = (string)r.Place["name"]! })]),
        };
        var styles = new Dictionary<string, string>
        {
            ["query-extent"] = """{"lineColor":"#FFD400","lineWidth":2.0,"fillOpacity":0}""",
            ["matches"] = """{"pointColor":"#E31A1C","fillColor":"#E31A1C","lineColor":"#7F0000","pointSize":9.0,"lineWidth":1.0}""",
            ["seed"] = """{"pointColor":"#1F78B4","fillColor":"#1F78B4","lineColor":"#FFFFFF","pointSize":12.0,"lineWidth":1.5}""",
        };
        foreach (var name in ResultLayers)
        {
            viewer.RemoveLayerByName(name);
            var path = Path.Combine(output, name + ".geojson");
            File.WriteAllText(path, layers[name].ToJsonString());
            if (viewer.AddLayerFile(path))
                viewer.SetLayerStyleJson(0, styles[name]);
        }
        viewer.RefreshLayers();
    }

    /// <summary>The map extent (RD New) around match i.</summary>
    public GeoKernelExtent? MatchView(int i)
    {
        var matches = Last?["matches"]?.AsArray();
        if (matches is null || i < 0 || i >= matches.Count)
            return null;
        var e = matches[i]!["extent"]!.AsArray().Select(v => (double)v!).ToArray();
        var (x, y) = GeoKernelCoordinateSystemFactory.TransformPoint(int.Parse(((string)matches[i]!["crs"]!)[5..]), 28992, (e[0] + e[2]) / 2, (e[1] + e[3]) / 2);
        return new GeoKernelExtent(x - 8000, y - 8000, x + 8000, y + 8000);
    }

    /// <summary>Three searches that each show one filter (the --smoke run).</summary>
    public IEnumerable<(string Label, SearchRequest Request)> Checks() =>
    [
        ("time: solar park 2024, zone 31, 2018 only", new(Place("Solar park"), 2024, "EPSG:32631", Extent("EPSG:32631", "Whole zone 31 (west of 6° E)"), 2018, 2018)),
        ("extent: greenhouses 2024, zone 31, Randstad, all years", new(Place("Greenhouses"), 2024, "EPSG:32631", Extent("EPSG:32631", "Randstad"), 2018, 2024)),
        ("crs: solar park 2024 (zone 31) searched in zone 32, Groningen and Drenthe, 2024", new(Place("Solar park"), 2024, "EPSG:32632", Extent("EPSG:32632", "Groningen and Drenthe"), 2024, 2024)),
    ];

    public string CheckLine(string label, bool ok)
    {
        var l = Last!;
        var matches = l["matches"]!.AsArray();
        var best = matches.Count > 0 ? (double)matches[0]!["similarity"]! : 0;
        return $"smoke | {label}: {(ok ? "ok" : "FAILED")} — {matches.Count} matches, best {best:F3}, rejected crs {l["crsRejected"]} / extent {l["spatialRejected"]} / time {l["temporalRejected"]}";
    }

    public void Dispose()
    {
        Index?.Dispose();
        Index = null;
    }
}
