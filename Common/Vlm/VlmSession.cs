using System.Text.Json.Nodes;
using GeoKernel.Examples.GeoAi;
#if VLM_WPF
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif

namespace GeoKernel.Examples.Vlm;

/// <summary>An example area in EPSG:25831 (the CRS of the photo and of the map), 512 x 512 pixels.</summary>
internal sealed record Area(string Name, string Note, double MinX, double MinY, double MaxX, double MaxY);

/// <summary>
/// The "What is in this area?" workflow shared by the Windows Forms and WPF examples: the Oosterwold 2025
/// aerial photo (GeoKernel.SampleData), the Florence-2 package, the native assistant (GeoKernel.NET
/// <c>VlmAssistant</c>) and the example areas. Questions use a remote vision model only when
/// GEOKERNEL_LLM_PROVIDER is set; the area's pixels are then sent to that provider.
/// </summary>
internal sealed class VlmSession : IDisposable
{
    const string PhotoUrl = "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/almere_oosterwold_series_2016_2025_v1.zip";
    const string PhotoSha = "080742d758d9c471ee23cebd379821f61c2e5b28a28947f20ac6e20f5cc44f4d";
    const string Photo = "oosterwold_2025-epsg25831-50cm.tif";
    const string ModelUrl = "https://pub-00d214f25f754884b22349c19ac37bbf.r2.dev/geokernel-models/microsoft-florence-2-base-ft-fp32-geokernel-onnx-v1.zip";
    const string ModelSha = "d848b2a7cbb2b1b2a68c6387abd6c5d0c79575d5b89a93f3373053d950b9e20a";
    const string Model = "florence2.geokernel.json";

    public static readonly Area[] Areas =
    [
        new("Houses", "Detached houses with gardens", 657883, 5800850, 658139, 5801106),
        new("Road crossing", "Houses along a road crossing", 657683, 5800550, 657939, 5800806),
        new("Round garden", "A circular building and garden plots", 658233, 5800450, 658489, 5800706),
    ];

    public const string Credit = "Aerial photo: Beeldmateriaal Nederland / PDOK (CC BY 4.0). Florence-2: Microsoft (MIT).";
    public const string Help =
        "Pick an example or turn on \"Pick an area on the map\" and drag a box. Describe and Mark run Florence-2 on this computer; " +
        "nothing leaves it. Questions need a remote vision model: set GEOKERNEL_LLM_PROVIDER (anthropic, openai or ollama), " +
        "GEOKERNEL_LLM_MODEL and the provider's API key before starting; the area's pixels are then sent to that provider.";

    int revision;

    public VlmAssistant? Assistant { get; private set; }
    public string? PhotoPath { get; private set; }
    public string? ModelPath { get; private set; }
    public bool HasRemote { get; private set; }

    public async Task PrepareAsync(IProgress<DownloadProgress> progress, CancellationToken cancellation)
    {
        PhotoPath = await ModelAssets.PrepareAsync(PhotoUrl, PhotoSha, Photo, progress, cancellation);
        // GEOKERNEL_FLORENCE2_PACKAGE: an extracted package (florence2.geokernel.json) instead of the 630 MB download.
        var local = Environment.GetEnvironmentVariable("GEOKERNEL_FLORENCE2_PACKAGE");
        ModelPath = !string.IsNullOrEmpty(local) ? local : await ModelAssets.PrepareAsync(ModelUrl, ModelSha, Model, progress, cancellation);
    }

    /// <summary>Shows the photo and opens the assistant on the viewer. Returns the ready message.</summary>
    public string Attach(GeoKernelViewerControl viewer)
    {
        if (!viewer.AddLayerFile(PhotoPath!))
            throw new InvalidOperationException("Cannot open the aerial photo " + PhotoPath);
        var options = new JsonObject { ["localModel"] = ModelPath };
        HasRemote = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("GEOKERNEL_LLM_PROVIDER"));
        if (HasRemote)
            options["remote"] = true;
        Assistant = new VlmAssistant(viewer, PhotoPath!, options);
        viewer.ViewExtent = new GeoKernelExtent(657533, 5800382, 658557, 5801406);
        return HasRemote ? "Ready: Florence-2 on this computer, questions go to the remote model."
                         : "Ready: Florence-2 on this computer. Set GEOKERNEL_LLM_PROVIDER to ask questions.";
    }

    public void Pick(int index)
    {
        var a = Areas[index];
        Assistant!.SetArea(a.MinX, a.MinY, a.MaxX, a.MaxY);
        Assistant.ZoomToArea();
    }

    /// <summary>The result of a run finished since the last call (also one started on the map), or null.</summary>
    public JsonObject? NewResult()
    {
        if (Assistant is null || Assistant.Revision == revision)
            return null;
        revision = Assistant.Revision;
        return Assistant.Result();
    }

    public static string Status(JsonObject result) =>
        result["error"] is JsonNode error ? error.GetValue<string>()
            : $"{result["kind"]}: {result["model"]} ({((bool?)result["remote"] == true ? "remote" : "local")}), {(double)result["elapsedMs"]! / 1000:0.0} s";

    public static string Answer(JsonObject result)
    {
        if (result["error"] is not null)
            return "";
        if ((string?)result["kind"] == "find")
            return $"{result["objects"]!.AsArray().Count} objects marked as \"{result["prompt"]}\" (red boxes). The model reports no confidence: review them.";
        var question = (string?)result["question"];
        return (string.IsNullOrEmpty(question) ? "" : question + "\n\n") + (string?)result["text"];
    }

    public void Dispose()
    {
        Assistant?.Wait(TimeSpan.FromMinutes(10));
        Assistant?.Dispose();
    }
}
