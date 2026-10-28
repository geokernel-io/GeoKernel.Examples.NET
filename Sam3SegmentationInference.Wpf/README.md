# SAM3 segmentation — Wpf

Text, positive/negative point and box prompting on the fixed buildings RGB raster, with draft review before saving.

## SDK and launch

Uses the published **GeoKernel.NET 1.5.39** NuGet package, including the managed controls and native Windows x64 runtime. No sibling SDK source checkout, native build or preparation script is required.

From this example directory:

```powershell
dotnet run -c Release
```

NuGet restores the package and copies its native dependencies to the application output. Models and sample data download automatically in the application. `GEOKERNEL_NATIVE_DIR` is only an explicit override for a separately deployed compatible runtime (for example, GPU support); it is not set automatically.


## Use

1. Startup downloads/caches the original RGB source only. No predictions are restored.
2. Select Text segmentation, type a concept such as `building`, and click **Generate draft**.
3. Alternatively choose Point / box segmentation, then choose a map tool:
   - Positive point: click inside one object (green marker).
   - Negative point: exclude an area (red marker); a positive point or box is also required.
   - Box: click two opposite corners around one object. The first corner and completed yellow box are displayed.
   - Pan map: move around; toolbar buttons zoom or restore full extent.
4. Review the orange draft polygons. **Accept draft** enables saving. **Discard result** removes the result and enables another run.
5. Save accepted Shapefile for polygons or accepted GeoTIFF for source-grid instance IDs using the file dialog.
   Choose a new Shapefile destination; the native task exporter does not replace an existing dataset.

Changing prompts/task is disabled while a result is retained. Discard before another run. Clear prompts removes all point/box selections.
The score control applies to text mode. A box selects an object mask; use text mode to find all matching objects in the raster.
The output transparency slider becomes more opaque toward the right.
Results are temporary until explicit export; closing detaches the displayed result before background job cleanup.

## Downloads and runtime

Only the selected model profile downloads when Generate is clicked. Text and tracker are separate multi-GB bundles;
allow space for both the ZIP cache and extracted files. Archive SHA-256 is verified, and extraction uses a staging directory.
Inputs are cached at `%LOCALAPPDATA%\GeoKernel\Examples\GeoAI`, overridable with `GEOKERNEL_EXAMPLES_DATA_DIR`.
Native model/session validation checks the installed files. Predictions are never cached as startup input.

Downloads and model work run off the UI thread. Cancel is cooperative; model loading or an active kernel may finish first.
The native deadline is 30 minutes; tracker input is limited to 16 million source pixels.
Auto currently maps to CPU. CUDA needs a compatible GPU ONNX Runtime and matching CUDA/cuDNN DLLs in the selected native deployment;
selecting CUDA cannot add GPU support to a CPU-only SDK. No GPU installation or silent CUDA-to-CPU fallback is performed.

See [data-sources.md](data-sources.md) for model/imagery hashes, attribution and licensing.
No license keys or command-line test modes are embedded in the examples.

The viewer uses a temporary WGS84 GeoJSON display preview. The authoritative result and exported polygons keep the source CRS and attributes. Moving the opacity slider changes both polygon fill and outline; the preview is cleaned up with the job.
