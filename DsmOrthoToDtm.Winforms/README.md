# DsmOrthoToDtm — WinForms

DSM + orthophoto to DTM: compare three newly computed terrain estimates on **the same full
DSM**: the native classical filter (DSM only), the experimental **pix_grad** model (DSM only)
and the experimental **rgb_grad_fm** model (DSM plus an aligned RGB orthophoto). Four linked
native GeoKernel **2D maps** keep the input context on the left, switchable between the real
orthophoto and the DSM relief. The reference is evaluation-only; the methods do not feed each
other. WinForms port of the Qt `DsmOrthoToDtm` example; inference, comparison and export are
shared in [`OrthoSupport.cs`](../Common/DsmToDtm/OrthoSupport.cs) (with pieces of
[`AiSupport.cs`](../Common/DsmToDtm/AiSupport.cs)), the classical filter in
[`TerrainSupport.cs`](../Common/DsmToDtm/TerrainSupport.cs).

Requires Windows x64, .NET SDK 10 and a valid GeoKernel license. The published GeoKernel.NET
**1.5.40** package and its native/GDAL/ONNX runtime are restored automatically; the input ZIP
and both model packages are downloaded and verified on first start.

```powershell
dotnet run --project DsmOrthoToDtm.Winforms -c Release
```

Options: `--data-dir <folder>` (default `data` beside the executable), `--sample-archive <zip>` /
`--sample-url <https-url>` (the pinned DSM + orthophoto ZIP), `--pix-model-archive`,
`--rgb-model-archive`, `--pix-model-url`, `--rgb-model-url` (the pinned model ZIPs). All archive
and file hashes stay enforced.

## Workflow

1. Startup verifies/downloads `zurich_dsm_ortho_to_dtm_v1.zip` (GeoKernel.SampleData v1;
   [pins](../Common/DsmToDtm/ortho-sample-assets.json)): full 1 km `dsm.tif`, aligned
   SWISSIMAGE `ortho_rgb.tif`, evaluation `reference.tif`, manifest, notices and PROJ database
   (2000 × 2000, 0.5 m, EPSG:2056). Then both model packages are verified
   ([pix](../Common/DsmToDtm/ortho-pix-model-assets.json),
   [rgb](../Common/DsmToDtm/ortho-rgb-model-assets.json)); the model buttons supply the same
   pinned ZIPs locally.
2. Choose AI overlap (64 px, 0–128), CPU threads (4) and the classical preset, then **Run all
   three methods**: classical, then pix_grad (DSM), then rgb_grad_fm (DSM, R, G, B).
3. The estimates share one hillshade and switch together to removed heights (0–20 m) or signed
   reference errors (−2 to +2 m). **Input context** shows the orthophoto or the DSM relief.
4. The default **Shared comparison crop** (column 200 / row 800, 500 × 500 cells) dims the rest
   of each map; **Full input** scores the complete valid overlap. Both are saved.
5. Pan/zoom is linked; click a cell to read RGB bytes, DSM, all three estimates and reference.
6. **Open aligned DSM + RGB** loads custom sources (crop = central 60%); **Open reference**
   attaches an aligned evaluation surface without regenerating any heights.
7. **Save comparison folder** stages the three `<method>-dtm.tif` / `-removed.tif`, both
   `<AI>-raw.tif`, `classical-qa.tif`, the `comparison-*.tif` crops (heights and RGB) and
   `comparison.json`, then renames the folder to `dsm-ortho-dtm-<UTC time>-<id>`.

Custom RGB must be three RGB-ordered Byte bands on the DSM's exact grid covering every valid
DSM cell; RGB gaps outside valid DSM are context-filled from RGB only, DSM gaps from DSM only.

## Results

On the Zurich sample every output raster (heights, raw AI, QA, nine display maps, crops) is
**identical to the Qt build**, and so are the scores. Shared crop RMSE / MAE / bias: classical
1.018 / 0.355 / 0.253 m, pix_grad 0.659 / 0.407 / −0.277 m, rgb_grad_fm 0.663 / 0.399 /
−0.249 m; full input: 1.300 / 0.517 / 0.400, 1.749 / 0.981 / −0.668 and 1.615 / 0.877 /
−0.561 m. A small custom input with DSM and RGB gaps also matched exactly. Different
checkpoints and objectives mean this does not isolate the causal contribution of RGB.

## Differences from the Qt example

- Inference uses `GeoKernelAI.RunRasterInference` (the SDK's JSON raster inference C ABI)
  instead of the C++ `requestFromPackage`. It takes one raster and a band list, so the example
  stacks the channels (context-filled DSM, then R, G, B) into one Float32 raster in the model's
  order, edge-pads inputs smaller than one tile and crops the result, and re-applies the source
  mask. The SDK reads every band as Float32, so inputs and results are the same as Qt's. The
  request type has no model revision field, so the revision in the SDK provenance is empty.
- The inference call has no cancellation hook: **Cancel** and closing the window take effect
  when the running AI pass ends.
- GeoKernel.NET 1.5.40 wraps no overlay point, so the probe marker, the crop dimming and its
  solid outline are owned shapes redrawn on all maps. Views are linked by extent.
- Models are prepared on a worker thread, not the GUI thread.

Data: Federal Office of Topography swisstopo. Model weights: David Perez, Zenodo 7973800 /
7973886, CC BY 4.0 (deterministic ONNX adaptations). Fixed development scene, not a held-out
benchmark.
