# VlmImageQA — WPF

Pick an area of an aerial photo. Florence-2 describes it and marks objects named in plain language
on this computer; a remote vision model answers questions about it. This is a WPF port of the
Qt `VlmImageQA` example. The workflow (sample data, model, example areas, results) is shared with
the [Windows Forms version](../VlmImageQA.Winforms/README.md) in
[`VlmSession.cs`](../Common/Vlm/VlmSession.cs), on top of GeoKernel.NET's `VlmAssistant`
(C ABI `GeoKernelVlmAssistant_*`).

## Requirements and build

Requires Windows x64, .NET SDK 10 and a valid GeoKernel license. The VLM assistant API ships
after GeoKernel.NET **1.5.40**. Until the next release, build against the GeoKernel.NET sources
and put a native GeoKernel runtime with the API on `PATH`:

```powershell
$env:Path = "D:\projects\GeoKernel\outputs\build\Release;" + $env:Path
dotnet run -c Release --project VlmImageQA.Wpf -p:GeoKernelNetSource=D:/projects/GeoKernel.NET
```

After the release, use the NuGet package instead: `-p:GeoKernelSdkVersion=<version>`.

**Data and model.** At startup the example downloads
`almere_oosterwold_series_2016_2025_v1.zip` from GeoKernel.SampleData v1 (56 MB; the 2025 photo,
0.5 m, EPSG:25831) and `microsoft-florence-2-base-ft-fp32-geokernel-onnx-v1.zip` (630 MB, Florence-2
package). Both are SHA-256 checked and cached. `GEOKERNEL_FLORENCE2_PACKAGE` (the
`florence2.geokernel.json` of an extracted package) skips the model download.

**Questions** need a remote vision model. Before starting, set `GEOKERNEL_LLM_PROVIDER`
(`anthropic`, `openai` or `ollama`), `GEOKERNEL_LLM_MODEL` and the provider's key
(`ANTHROPIC_API_KEY` / `OPENAI_API_KEY`). The picked area's pixels are then sent to that
provider. Without these settings Ask is off; Describe and Mark always run locally.

## Workflow

1. **Example areas**: **Houses**, **Road crossing** or **Round garden** (512 × 512 pixels of
   Almere Oosterwold, 2025). Or turn on **Pick an area on the map** and drag a box (a click picks
   256 × 256 pixels around it). The area is outlined in yellow.
2. **Describe** (one sentence) or **In detail** (a paragraph): Florence-2-base-ft on this
   computer, 1–2 s per area on CPU.
3. **Mark** objects named in plain language, for example `car`. Florence-2 scans 256-pixel tiles
   and draws the objects as red boxes in the photo's CRS. It finds vehicles reliably but misses
   many of them, and gives no confidence.
4. **Ask** a question in any language, for example "How many houses are in this area?". A remote
   vision model answers; it has no local alternative.

`--smoke` describes the houses, marks cars, asks (when a remote model is set), prints the answers
and exits. Example run (gpt-5.5 as the remote model):

```
describe: An aerial view of a city with lots of buildings. (1.5 s)
find car: 3 objects (21 s)
ask: About 30 houses. (23 s; remote answers vary between runs, 25–40 here)
```

## Credits

Aerial photo: Beeldmateriaal Nederland / PDOK (CC BY 4.0). Florence-2: Microsoft (MIT).
