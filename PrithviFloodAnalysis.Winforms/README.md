# PrithviFloodAnalysis — Winforms

IBM/NASA Prithvi EO 2.0 300M-TL Sen1Floods11 water/flood mapping on the India Sentinel-2 sample.

## SDK and launch

Uses the published **GeoKernel.NET 1.5.39** NuGet package, including the managed controls and native Windows x64 runtime. No sibling SDK source checkout, native build or preparation script is required.

From this example directory:

```powershell
dotnet run -c Release
```

NuGet restores the package and copies its native dependencies to the application output. Models and sample data download automatically in the application. `GEOKERNEL_NATIVE_DIR` is only an explicit override for a separately deployed compatible runtime (for example, GPU support); it is not set automatically.


## Workflow

1. The source raster downloads automatically on startup. No predictions are loaded.
2. Click **Run flood analysis**. The fixed model downloads if needed and inference runs on a native worker.
3. Use **Cancel** during preparation or inference. Cancellation waits for the current native processing boundary.
4. Move the output transparency slider right for a more opaque result.
5. Use **Save Shapefile** and choose a new destination to retain the polygons. Existing Shapefile datasets are not overwritten by the task API.

Only model and imagery inputs are cached. Results remain temporary until explicit export;
closing or running again releases the previous job after removing its displayed result layer.
Save/export and native job cleanup run off the UI thread. An export already in progress finishes before the window closes.

Cache: `%LOCALAPPDATA%\GeoKernel\Examples\GeoAI`, keyed by archive SHA-256.
Override with `GEOKERNEL_EXAMPLES_DATA_DIR`. ZIP downloads are SHA-256 checked and extracted through staging directories;
interrupted downloads are discarded. Provider Auto currently means CPU. CUDA requires a compatible GPU ONNX Runtime,
CUDA and cuDNN deployment; the examples do not install GPU dependencies or silently fall back from an explicitly selected CUDA provider.

The SDK's separate license/evaluation rules apply. Model and imagery credits and terms are in [data-sources.md](data-sources.md)
and in the downloaded archives. No license keys are embedded in these examples.


The source display is prepared on a worker using the SDK-shipped GDAL, without changing the original 13-band TIFF. This derived RGB input preview is cached; predictions remain session-only. Model loading uses a multi-GB working set and cancellation may wait for the current model-loading operation or tile.

The CPU reference produces 57 connected water/flood polygons, 9 tiles, and class counts 139744 non-water / 122400 water pixels. Only water/flood polygons are saved by Save Shapefile; the two-class raster preview remains on screen. This is a reproducibility reference, not an accuracy score.
