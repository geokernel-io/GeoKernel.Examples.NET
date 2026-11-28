using System.Globalization;
using System.Text.Json.Nodes;
using GeoKernel.Examples.GeoAi;
#if SIMILARITY_WPF
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.Similarity;

/// <summary>An example query place in EPSG:32631 (the CRS of the grid and of the map): a box, or a point when it has two values.</summary>
internal sealed record Preset(string Name, string Note, double[] Place);

/// <summary>
/// The find-similar workflow shared by the Windows Forms and WPF examples: the Flevoland AlphaEarth
/// embedding grid (GeoKernel.SampleData), the native search (GeoKernel.NET <c>SimilaritySearch</c>),
/// preset queries and the view that frames a result.
/// </summary>
internal sealed class SimilaritySession : IDisposable
{
    const string DataUrl = "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/flevoland_alphaearth_2024_80m_v1.zip";
    const string DataSha = "c0a9f05f8680be32f46b9db4f43fa22345f0753a9bfdd9e9ff7fad83bffc4d88";
    const string Grid = "flevoland_alphaearth_2024_80m.tif";

    public static readonly Preset[] Presets =
    [
        new("Solar park", "Zonnepark Zuyderzon, Almere (OpenStreetMap outline)", [652172, 5809772, 652868, 5810511]),
        new("Open water", "Markermeer", [652727, 5819032]),
        new("Town centre", "Almere Stad", [650788, 5804723]),
        new("Farmland", "Oostelijk Flevoland", [683392, 5816743]),
    ];

    public const string Attribution =
        "The AlphaEarth Foundations Satellite Embedding dataset is produced by Google and Google DeepMind (CC BY 4.0); " +
        "2024 layer, 80 m, Source Cooperative mirror, resampled by GeoKernel.";
    public const string Help =
        "Pick an example or turn on \"Find similar on the map\" and click a place (its cell) or drag a box (the mean of its cells). " +
        "Every 80 m cell is compared with it by cosine similarity of its 64-value AlphaEarth embedding; the heat map shows the most " +
        "similar cells and the list the best matches, kept two cells apart.";

    int revision;

    public SimilaritySearch? Search { get; private set; }
    public string? GridPath { get; private set; }

    public async Task PrepareAsync(IProgress<DownloadProgress> progress, CancellationToken cancellation) =>
        GridPath = await ModelAssets.PrepareAsync(DataUrl, DataSha, Grid, progress, cancellation);

    /// <summary>Shows the grid as muted false colour and opens the search on the viewer. Returns the ready message.</summary>
    public string Attach(GeoKernelViewerControl viewer)
    {
        if (!viewer.AddLayerFile(GridPath!))
            throw new InvalidOperationException("Cannot open the embedding grid " + GridPath);
        viewer.SetLayerOpacity(0, 0.45);  // the embeddings' first three dimensions, muted so the heat map stands out
        Search = new SimilaritySearch(viewer, GridPath!, new JsonObject { ["maximumResults"] = 20, ["threshold"] = 0.9 });
        var info = Search.Info();
        var e = info["extent"]!.AsArray();
        viewer.ViewExtent = new GeoKernelExtent((double)e[0]!, (double)e[1]!, (double)e[2]!, (double)e[3]!);
        return $"Ready: {info["width"]} × {info["height"]} cells of 80 m, {info["dimensions"]} dimensions. Pick an example or search on the map.";
    }

    /// <summary>Runs a preset query and returns the result and the extent that frames the query and its ten best matches.</summary>
    public (JsonObject Result, GeoKernelExtent View) SearchPreset(int index)
    {
        var place = Presets[index].Place;
        var result = place.Length == 4 ? Search!.SearchExtent(place[0], place[1], place[2], place[3]) : Search!.SearchPoint(place[0], place[1]);
        revision = Search.Revision;
        double x0 = place[0], y0 = place[1], x1 = place[^2], y1 = place[^1];
        foreach (var match in result["matches"]!.AsArray().Take(10))
        {
            var e = match!["extent"]!.AsArray();
            x0 = Math.Min(x0, (double)e[0]!); y0 = Math.Min(y0, (double)e[1]!);
            x1 = Math.Max(x1, (double)e[2]!); y1 = Math.Max(y1, (double)e[3]!);
        }
        double margin = Math.Max(x1 - x0, y1 - y0) * 0.15 + 500;
        return (result, new GeoKernelExtent(x0 - margin, y0 - margin, x1 + margin, y1 + margin));
    }

    /// <summary>The result of an interactive click or box since the last call, or null.</summary>
    public JsonObject? NewInteractiveResult()
    {
        if (Search is null || Search.Revision == revision)
            return null;
        revision = Search.Revision;
        return Search.Result();
    }

    public static IEnumerable<string> MatchLines(JsonObject result) =>
        result["matches"]!.AsArray().Select((m, i) => string.Format(CultureInfo.InvariantCulture, "{0}.  {1:0.000}   ({2}, {3})",
            i + 1, (double)m!["similarity"]!, (int)m["column"]!, (int)m["row"]!));

    public static string Summary(JsonObject result)
    {
        var matches = result["matches"]!.AsArray();
        double best = matches.Count > 0 ? (double)matches[0]!["similarity"]! : 0;
        return string.Format(CultureInfo.InvariantCulture, "{0} matches among {1} cells in {2:0} ms (best similarity {3:0.000}).",
            matches.Count, (double)result["validCells"]!, (double)result["elapsedMs"]!, best);
    }

    public void Dispose() => Search?.Dispose();
}
