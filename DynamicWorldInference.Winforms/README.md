# DynamicWorldInference — Winforms

Google Dynamic World nine-class land-cover classification on the Spain Sentinel-2 L1C sample.

## SDK and launch

Uses the published **GeoKernel.NET 1.5.38** NuGet package, including the managed controls and native Windows x64 runtime. No sibling SDK source checkout, native build or preparation script is required.

From this example directory:

```powershell
dotnet run -c Release
```

NuGet restores the package and copies its native dependencies to the application output. Models and sample data download automatically in the application. `GEOKERNEL_NATIVE_DIR` is only an explicit override for a separately deployed compatible runtime (for example, GPU support); it is not set automatically.


## Workflow

1. The source raster downloads automatically on startup. No predictions are loaded.
2. Click **Run land-cover classification**. The fixed model downloads if needed and inference runs on a native worker.
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
