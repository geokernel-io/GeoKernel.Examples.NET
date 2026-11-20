# DsmToDtmProcessing — WinForms

Live classical processing: **run a classical DSM-to-DTM conversion on the input DSM**,
change parameters, inspect the newly generated terrain and export numeric GeoTIFFs.
Three linked native GeoKernel **2D maps** show the original DSM, the generated DTM and a
selectable diagnostic map. WinForms port of the Qt `DsmToDtmProcessing` example; the
algorithm is shared in [`TerrainSupport.cs`](../Common/DsmToDtm/TerrainSupport.cs).

Requires Windows x64, .NET SDK 10 and a valid GeoKernel license. The published
GeoKernel.NET **1.5.40** NuGet package, its native/GDAL runtime and the pinned sample
are restored/downloaded automatically.

```powershell
dotnet run --project DsmToDtmProcessing.Winforms -c Release
```

Options: `--sample-archive <zip>`, `--sample-url <https-url>`, `--data-dir <folder>`
(default: `data` beside the executable). All hash pins still apply.

## Workflow

1. Startup reuses the verified Zurich v2 sample and shows its original DSM. Nothing is
   converted until **Run DSM → DTM** is pressed.
2. Choose **Balanced**, **Remove more objects** or **Preserve more relief**, or adjust the
   six parameters (working grid, max. window, base threshold, slope allowance, relief
   allowance, keep tolerance), then **Run**. Processing runs on a worker thread with phase
   progress; **Cancel** keeps the last completed source/result.
3. The middle map shows the new DTM with the same hillshade as the original; the views are
   linked. With Pan active, click a pixel to read DSM, DTM, removed height, QA bits and
   the optional reference/error; a marker shows the cell on every map.
4. The right map shows **removed height** (0–20 m), **support / edge quality** or **signed
   reference error** (−2 to +2 m). Colours never clip the numeric results or scores.
5. Changing parameters keeps the last result on screen, marked as needing a new run; the
   summary and export keep the parameters actually used.
6. **Open DSM GeoTIFF** loads another source; **Open reference (optional)** attaches an
   exactly aligned evaluation raster and rescores without recomputing.
7. **Save result GeoTIFFs** writes `<name>.tif` (Float32 DTM), `.removed.tif`, `.qa.tif`
   (Byte QA bits 1/2/4/8/16) and `.json` (parameters, statistics, scores, provenance), each
   atomically. Source/reference paths are rejected; existing companions need confirmation.

## Method and results

Method `native-multiscale-morphological-tin-v1`: block-minimum lower envelope keeping each
minimum's real source pixel, reflected-boundary openings at doubling windows with a capped
base/slope allowance plus a DSM-derived relief allowance reduced in rough areas, GDAL linear
TIN gridding at the source pixel centres (through the package's `gdal.dll`), a grid-support
fallback outside the hull, source retention within the keep tolerance and a DSM upper bound.
The reference is never an input to processing.

Float arithmetic follows the C++ code, so on the Zurich sample the result is **identical to
the Qt build** (DTM, removed heights and QA on all 250,000 cells). Balanced: 10485 supports,
2582 hull-fallback pixels; RMSE 0.863 / MAE 0.324 / bias 0.200 m over all cells and
0.532 / 0.220 / 0.098 m outside the 32 m edge band.

Inputs: single-band north-up GeoTIFF, square pixels, projected metre CRS, metre heights,
8 × 8 to 4 million cells with at least 5% valid; working grid 3 × 3 to 262,144 cells.

## Differences from the Qt example

GeoKernel.NET 1.5.40 wraps no overlay point, so the probe is an owned point shape redrawn
on all maps. Views are linked by extent. Each map adopts its first layer's CRS.

See [the shared code](../Common/DsmToDtm/README.md) for sample and data details.
Data: Federal Office of Topography swisstopo. Fixed development crop, not a held-out benchmark.
