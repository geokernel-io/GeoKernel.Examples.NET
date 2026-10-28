# SEN2SR super-resolution — Wpf

ESAOpenSR SEN2SRLite RGBN 4× inference on the first Sentinel-2 period of the fixed FTW raster.

## SDK and launch

Uses the published **GeoKernel.NET 1.5.39** NuGet package, including the managed controls and native Windows x64 runtime. No sibling SDK source checkout, native build or preparation script is required.

From this example directory:

```powershell
dotnet run -c Release
```

NuGet restores the package and copies its native dependencies to the application output. Models and sample data download automatically in the application. `GEOKERNEL_NATIVE_DIR` is only an explicit override for a separately deployed compatible runtime (for example, GPU support); it is not set automatically.


## Workflow

- Startup prepares only the source RGB display. No predictions are restored.
- Click **Run 4× super-resolution**. The fixed model downloads if needed; inference runs on a native worker.
- After inference, the enhanced image is shown fully opaque by default. The slider blends the improved RGB over the source. Left shows the source; right makes the improved image fully opaque.
  Zoom in to compare edge detail; the full-extent view downsamples the enhanced image to the same display area. The slider is enabled after a result exists.
  Both result comparison previews use the same source-derived stretch. The current map extent is retained when previews change.
- **Cancel** cooperatively stops preparation/inference. Re-running removes the earlier result before releasing its temporary workspace.
- **Save GeoTIFF** opens the file dialog and exports the scientific four-band Float32 result. It does not export the RGB display preview.

Only input/model files and derived source-display imagery are cached. Predictions/comparison files remain temporary until explicit export.
Closing detaches both displayed job previews before cleanup. No Shapefile/instance-label controls or command-line test modes are included.

## Raster contract

The original eight-band FTW TIFF is preserved. Inference uses its first four bands, B4/B3/B2/B8 (RGB + NIR),
with L2A reflectance scaled by 10000 and the required BOA offset already corrected. Model normalization is embedded.
The source is 578 × 379; output is 2312 × 1516, using 28 reflected/halo-cropped tiles.

The output retains four Float32 surface-reflectance bands without clipping. Band four is **NIR, not alpha**.
CRS, origin and source footprint remain the same; affine pixel/rotation coefficients are divided by four.
AI provenance is stored in the GeoTIFF. A finer estimated grid does not constitute a new high-resolution sensor acquisition.
NoData, nonfinite or negative input samples must be handled before super-resolution; the native adapter rejects them.

## Cache and provider

Inputs use a SHA-256 keyed cache at `%LOCALAPPDATA%\GeoKernel\Examples\GeoAI`, overridable with `GEOKERNEL_EXAMPLES_DATA_DIR`.
The pinned raw TIFF and model ZIP are hash checked; ZIP extraction is staged. The sample cache is shared with PRUE.
Auto currently selects CPU. Explicit CUDA needs matching GPU ONNX Runtime and CUDA/cuDNN dependencies in the selected native deployment;
these examples do not install GPU dependencies or silently fall back from CUDA.

See [data-sources.md](data-sources.md) and downloaded notices for model and imagery terms. SDK licensing is separate.
No license keys are embedded in these examples.
