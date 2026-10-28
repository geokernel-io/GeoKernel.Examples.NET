# PrueFieldBoundaries — Winforms

Fields of The World / PRUE EfficientNet-B5 CC-BY field segmentation on the pinned upstream two-period Sentinel-2 test raster.

## SDK and launch

Uses the published **GeoKernel.NET 1.5.39** NuGet package, including the managed controls and native Windows x64 runtime. No sibling SDK source checkout, native build or preparation script is required.

From this example directory:

```powershell
dotnet run -c Release
```

NuGet restores the package and copies its native dependencies to the application output. Models and sample data download automatically in the application. `GEOKERNEL_NATIVE_DIR` is only an explicit override for a separately deployed compatible runtime (for example, GPU support); it is not set automatically.


## Workflow

1. The source raster downloads automatically on startup. No predictions are loaded.
2. Click **Run field boundary segmentation**. The fixed model downloads if needed and inference runs on a native worker.
3. Use **Cancel** during preparation or inference. Cancellation waits for the current native processing boundary.
4. Move the output transparency slider right for a more opaque result.
5. Use **Save Shapefile** and choose a new destination to retain the polygons. Existing Shapefile datasets are not overwritten by the task API.

Only model and imagery inputs are cached. Results remain temporary until explicit export;
closing or running again releases the previous job after removing its displayed result layer.
Save/export and native job cleanup run off the UI thread. An export already in progress finishes before the window closes.

Cache: `%LOCALAPPDATA%\GeoKernel\Examples\GeoAI`, keyed by download SHA-256.
Override with `GEOKERNEL_EXAMPLES_DATA_DIR`. ZIP downloads are SHA-256 checked and extracted through staging directories;
interrupted downloads are discarded. Provider Auto currently means CPU. CUDA requires a compatible GPU ONNX Runtime,
CUDA and cuDNN deployment; the examples do not install GPU dependencies or silently fall back from an explicitly selected CUDA provider.

The SDK's separate license/evaluation rules apply. Model and imagery credits and terms are in [data-sources.md](data-sources.md)
and in the downloaded archives. No license keys are embedded in these examples.


The input TIFF downloads directly from the pinned upstream revision and is SHA-256 checked. Model ZIPs use verified staging extraction. All eight bands remain intact: B4/B3/B2/B8 for period B, then B4/B3/B2/B8 for period A. Model normalization is embedded; do not normalize twice.

A separate cached RGBA source display uses the first-period RGB bands and a 2–98% stretch, with source CRS/affine preserved. Predictions remain temporary. The three-class overlay shows background, field interior and field boundary. Save Shapefile exports only class-1 field interiors; these are image-derived agricultural divisions, not legal parcels.

The CPU reproducibility reference is 259 field-interior polygons, two tiles, and class counts 126651 background / 71533 interior / 20878 boundary. This is not an accuracy benchmark.

PRUE model v2 uses the same CCBY checkpoint at native resolution with 384 × 384 tiles and 128-pixel overlap. It requires SDK 1.5.39. Larger context reduces the rectangular top-left omission in this sample; this is a visual regression check, not a general accuracy benchmark. Model v1 remains available under its original URL.
