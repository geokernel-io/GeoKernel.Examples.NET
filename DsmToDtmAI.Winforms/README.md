# DsmToDtmAI — WinForms

Live AI versus classical: run the experimental **pix_grad** model through **GeoKernel.AI**,
run the native classical filter independently on the same DSM, and compare the newly
generated heights. Three linked native GeoKernel **2D maps** keep the original DSM on the
left. Neither method receives the reference or the other method's terrain estimate.
WinForms port of the Qt `DsmToDtmAI` example; comparison, inference and export are shared in
[`AiSupport.cs`](../Common/DsmToDtm/AiSupport.cs), the classical filter in
[`TerrainSupport.cs`](../Common/DsmToDtm/TerrainSupport.cs).

Requires Windows x64, .NET SDK 10 and a valid GeoKernel license. The published
GeoKernel.NET **1.5.40** NuGet package and its native/GDAL/ONNX runtime are restored
automatically; terrain, projection support and the model package are downloaded and
verified on first start.

```powershell
dotnet run --project DsmToDtmAI.Winforms -c Release
```

Options: `--data-dir <folder>` (cache root for terrain, support and model files; default
`data` beside the executable), `--model-archive <zip>` / `--model-url <https-url>` (the exact
pinned model ZIP), `--sample-archive <zip>` / `--sample-url <https-url>` (the pinned Zurich v2
support ZIP, not the 1 km source). All hash pins still apply.

## Workflow

1. Startup verifies the Zurich v2 support ZIP for the matching PROJ database, then downloads
   **the original full 1 km DSM and reference** directly from swisstopo (2000 × 2000 at
   0.5 m, EPSG:2056; sizes and SHA-256 in
   [`ai-terrain-assets.json`](../Common/DsmToDtm/ai-terrain-assets.json)).
2. The model package is acquired and its archive plus all six package files are checked
   against [`ai-model-assets.json`](../Common/DsmToDtm/ai-model-assets.json). **Use model
   ZIP** supplies the same pinned package locally; it does not accept arbitrary weights.
3. Choose tile overlap (default **64 px**, 0–128), CPU threads (default 4) and a classical
   preset, then **Run AI + classical**. The classical result is computed first, then an
   independent inference from the same DSM; the status bar shows phases and AI tile counts.
4. Compare **terrain relief**, **removed heights** (one fixed 0–20 m scale) or **signed
   reference errors** (one fixed −2 to +2 m scale). Display limits never clip the scores.
5. **Full input** scores all common valid pixels; **Shared comparison crop** zooms to the
   250 × 250 m window of the earlier Zurich examples (column 200 / row 800, 500 × 500 cells)
   and dims everything outside it. Both methods still process all 4 million cells.
6. Pan/zoom is linked. With Pan active, click a cell to read original, classical, AI and
   reference heights; a marker shows it on every map. Nothing here reruns inference.
7. Changing processing settings keeps the last comparison, marked as needing a new run.
   **Cancel** keeps the completed comparison.
8. **Open DSM GeoTIFF** loads a custom source (crop = central 60% per axis) and clears the
   reference and estimates; **Open reference** attaches an exactly aligned evaluation-only
   raster without rerunning inference.
9. **Save comparison folder** stages `classical-dtm.tif`, `ai-dtm.tif`, `ai-raw.tif`,
   `classical-removed.tif`, `ai-removed.tif`, `classical-qa.tif`, the `comparison-*.tif`
   crops and `comparison.json` in the chosen parent, then renames the staging folder to a new
   `dsm-dtm-ai-<UTC time>-<id>` folder. Existing files are never overwritten.

## Results

On the full Zurich tile every output raster (all 4 million cells, the six display maps and
the crops) is **identical to the Qt build**, and so are the scores: full input classical
(Balanced) RMSE 1.300 / MAE 0.517 / bias 0.400 m, AI (overlap 64) 1.749 / 0.981 / −0.668 m;
shared crop classical 1.018 / 0.355 / 0.253 m, AI 0.659 / 0.407 / −0.277 m. 121 AI tiles,
33,253 AI upper-bound clamps. A small custom input with NoData holes also matched exactly.

Supported inputs: one north-up metre-height band, projected metre CRS, square pixels,
identity scale/offset, 8 × 8 to 4 million cells, at least 5% valid; the reference must share
the exact grid and an equivalent CRS.

## Differences from the Qt example

- Inference uses `GeoKernelAI.RunRasterInference` (the SDK's JSON raster inference C ABI)
  instead of the C++ `requestFromPackage`. It has no separate context band, source-mask or
  padding options, so the example writes the context-filled DSM itself, edge-pads inputs
  smaller than one tile right/bottom (the package's padding mode) and crops the result,
  and re-applies the source mask. Results are the same; the request type has no model
  revision field, so the revision in the SDK provenance is empty.
- The inference call has no cancellation hook: **Cancel** and closing the window take effect
  when the current AI pass ends (a few seconds for the 1 km tile).
- GeoKernel.NET 1.5.40 wraps no overlay point, so the probe marker, the crop dimming and its
  solid outline are owned shapes redrawn on all maps. Views are linked by extent.
- The model is prepared on a worker thread, not the GUI thread.

Model: David Perez, Zenodo 7973800, CC BY 4.0 (deterministic ONNX adaptation). Terrain:
Federal Office of Topography swisstopo. Fixed development scene, not a held-out benchmark;
see the Qt example and [the shared code](../Common/DsmToDtm/README.md) for details.
