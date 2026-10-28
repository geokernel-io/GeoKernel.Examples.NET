using System.IO;
using System.Text.Json;

namespace GeoKernel.Examples.GeoAi;

internal sealed record GeoAiProfile(string TaskKind, string Title, string ModelUrl, string ModelHash,
    string DataUrl, string DataHash, string RasterName, string DisplayName, string Legend,
    string Attribution, string Description, bool BuildingFootprints = false, bool ArchivedData = true)
{
    public static readonly GeoAiProfile DynamicWorld = new(
        "dynamic-world", "Google Dynamic World — land cover",
        "https://pub-00d214f25f754884b22349c19ac37bbf.r2.dev/geokernel-models/google-dynamicworld-geokernel-onnx-v1.zip",
        "326bb2899cdea6e620a02f5e16fd330ec848e0a5b76f72f576038d7bc1e0aa63",
        "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/spain_sentinel2_l1c.zip",
        "07811e9600882c526185268c5b280134e5715c95042754e414d6736fa5647839",
        "dynamicworld_spain_sentinel2_l1c.tif", "dynamicworld_spain_rgb.tif",
        "Water · Trees · Grass · Flooded vegetation · Crops · Shrub and scrub · Built · Bare · Snow and ice",
        "Contains modified Copernicus Sentinel data 2019.\nModel: Google / National Geographic / WRI.\nApache-2.0; see downloaded notices.",
        "Spain / Alicante · Sentinel-2 L1C · 512 × 512 · 10 m\nNo predictions are loaded at startup.");

    public static readonly GeoAiProfile Hotosm = new(
        "hotosm-buildings", "HOTOSM DINOv3-S — building footprints",
        "https://pub-00d214f25f754884b22349c19ac37bbf.r2.dev/geokernel-models/hotosm-dinov3s-buildings-geokernel-onnx-v1.zip",
        "3a6cd11e013f81b46b3db700feddef2d98319780285b8320e1200573684b24bb",
        "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/buildings.zip",
        "f7cb0554b54c8e8bf31d576c38f988a8902382ad2ec3db3de6de05f5dbfb6f2b",
        "buildings.tif", "buildings.tif", "Building footprints",
        "Model: HOTOSM / Meta DINOv3.\nCC-BY-4.0 and separate DINOv3 License; retain package notices.\nImagery source/license not verified.",
        "Buildings RGB sample · original raster resolution\nNo predictions are loaded at startup.", true);

    public static readonly GeoAiProfile GeoAiUsa = new(
        "geoai-usa-buildings", "GeoAI USA — Mask R-CNN building footprints",
        "https://pub-00d214f25f754884b22349c19ac37bbf.r2.dev/geokernel-models/giswqs-geoai-usa-maskrcnn-resnet50-fpn-geokernel-onnx-v1.zip",
        "055097781e125083201421c37c70d36336f2f968b69b1aa09350578d2c87ca32",
        "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/buildings.zip",
        "f7cb0554b54c8e8bf31d576c38f988a8902382ad2ec3db3de6de05f5dbfb6f2b",
        "buildings.tif", "buildings.tif", "Building footprints",
        "Model: giswqs/geoai — Mask R-CNN ResNet50 FPN.\nMIT model declaration; retain package licenses/notices.\nImagery source/license not verified.",
        "USA-trained aerial RGB model · original raster resolution\nNo predictions are loaded at startup.", true);

    public static readonly GeoAiProfile Prithvi = new(
        "prithvi-flood", "IBM/NASA Prithvi EO 2.0 — flood analysis",
        "https://pub-00d214f25f754884b22349c19ac37bbf.r2.dev/geokernel-models/ibm-nasa-prithvi-eo2-300m-tl-sen1floods11-geokernel-onnx-v1.zip",
        "34711ae55d48fe265b19c18551aef4b4aa84045a07bffb407161570b19429c13",
        "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/flood_analysis.zip",
        "059e9a84bed08ea5da76941432e6e8409bd1352661ff72f750e47df5f740c38f",
        "India_900498_S2Hand.tif", "", "Non-water · Water / flood",
        "Model: IBM / NASA Prithvi EO 2.0; Apache-2.0.\nImagery: Copernicus / Sen1Floods11 (Cloud to Street).\nRetain package notices; processed-chip redistribution terms need confirmation.",
        "India · Sentinel-2 L1C · 512 × 512\nRGB display uses B4/B3/B2; inference uses six spectral bands.\nNo predictions are loaded at startup.");

    public static readonly GeoAiProfile Prue = new(
        "prue-fields", "Fields of The World / PRUE — field boundaries",
        "https://pub-00d214f25f754884b22349c19ac37bbf.r2.dev/geokernel-models/fieldsoftheworld-prue-efficientnet-b5-ccby-geokernel-onnx-v2.zip",
        "0035f723ae5753eb861e056bbb38b60605617485c9700cc16d3317856374da44",
        "https://raw.githubusercontent.com/fieldsoftheworld/ftw-baselines/779b809813b301de7fb9f00f0c5db07086b23227/tests/data-files/inference-img.tif",
        "da482e7987b36898bd9f8381e7c7fe95c05dfc5ccbba19a58eb65e544d1af750",
        "inference-img.tif", "", "Background · Field interior · Field boundary",
        "Model: Fields of The World / PRUE EfficientNet-B5 CC-BY.\nWeights: CC-BY-4.0; upstream code: MIT.\nImagery: Sentinel-2 / FTW test sample; retain source notices.",
        "FTW Sentinel-2 sample · 578 × 379 · two periods, eight bands\nRGB display uses the first period; inference uses all eight bands.\nNo predictions are loaded at startup.", ArchivedData: false);

    public static readonly GeoAiProfile Sen2sr = new(
        "sen2sr-super-resolution", "ESAOpenSR / SEN2SRLite — 4× super-resolution",
        "https://pub-00d214f25f754884b22349c19ac37bbf.r2.dev/geokernel-models/esaopensr-sen2srlite-rgbn-x4-geokernel-onnx-v1.zip",
        "bcad3673c73429fddc6f6277fff823b01bdc77a268aee8b09540566d510d1017",
        "https://raw.githubusercontent.com/fieldsoftheworld/ftw-baselines/779b809813b301de7fb9f00f0c5db07086b23227/tests/data-files/inference-img.tif",
        "da482e7987b36898bd9f8381e7c7fe95c05dfc5ccbba19a58eb65e544d1af750",
        "inference-img.tif", "", "RGB comparison · output GeoTIFF retains RGB + NIR",
        "Model: ESAOpenSR / SEN2SRLite.\nCC0-1.0 + inherited Apache-2.0 notices.\nImagery: Sentinel-2 / FTW test sample.",
        "FTW Sentinel-2 sample · first-period RGBN\n4× finer grid; estimated detail, not new sensor measurements.\nNo predictions are loaded at startup.", ArchivedData: false);

    public bool SuperResolution => TaskKind == "sen2sr-super-resolution";

    public JsonElement CreateRequest(string manifest, string raster, string provider, double threshold)
    {
        var values = new Dictionary<string, object?>
        {
            ["task"] = TaskKind, ["modelPackagePath"] = Path.GetDirectoryName(manifest),
            ["rasterPath"] = raster, ["provider"] = provider.ToLowerInvariant(),
            ["intraOpThreads"] = 4, ["maximumMilliseconds"] = 600000
        };
        if (BuildingFootprints) values["threshold"] = threshold;
        else if (TaskKind == "dynamic-world")
            values["validityMaskPath"] = Path.Combine(Path.GetDirectoryName(raster)!, "dynamicworld_spain_valid_mask.tif");
        return JsonSerializer.SerializeToElement(values);
    }
}
