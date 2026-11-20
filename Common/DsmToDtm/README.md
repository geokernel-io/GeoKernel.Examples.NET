# DSM to DTM — shared code

Shared sample acquisition, numeric raster IO and presentation text for the
[WinForms](../../DsmToDtm.Winforms/README.md) and [WPF](../../DsmToDtm.Wpf/README.md)
ports of the Qt `DsmToDtm` example. Both projects import `DsmExample.props`, which
links these files, embeds the pinned [sample-assets.json](sample-assets.json) and
references the published GeoKernel.NET **1.5.40** package.

- `SampleCache` downloads the fixed Zurich v2 ZIP from the `GeoKernel.SampleData` v1
  release and checks the archive and every extracted file against the SHA-256 pins.
  A complete valid cache is reused offline.
- `Gdal` reads and writes Float32 GeoTIFFs through the GDAL runtime deployed by the
  NuGet package, checks CRS equivalence and configures process-local
  `GDAL_DATA` / `PROJ_DATA` (packaged `proj-data` first, then the sample database).
- `Grids` validates the four pinned 500 × 500, 0.5 m EPSG:2056 height rasters and
  produces the interpolated display surface `(1-t)*DSM+t*DTM`.

- `Profiles` (in `ProfileSupport.cs`, used by DsmToDtmProfile) computes the shared
  hillshade, bilinear transect samples, line RMSE/MAE and the atomic CSV/metadata export;
  `ProfileText` and `ChartModel` hold the window text and chart scale.
- `Accuracy` (in `AccuracySupport.cs`, used by DsmToDtmAccuracy) builds the shared error
  colours and display maps, evaluation windows, metrics and the JSON report; `AccuracyText`
  holds the window text.
- `Terrain` (in `TerrainSupport.cs`, used by DsmToDtmProcessing) is the classical
  DSM-to-DTM filter (validated raster IO, estimate, GDAL TIN gridding, displays, scores,
  report, atomic export); `TerrainJobs` runs the load/process/reference/save steps and
  `TerrainText` holds the window text.
- `Ai` (in `AiSupport.cs`, used by DsmToDtmAI) acquires the pinned model package and full
  1 km terrain ([`ai-model-assets.json`](ai-model-assets.json),
  [`ai-terrain-assets.json`](ai-terrain-assets.json)), runs pix_grad through
  `GeoKernelAI.RunRasterInference`, compares it with `Terrain.Estimate` on the same DSM and
  stages the comparison-folder export; `AiText` holds the window text.
- `Ortho` (in `OrthoSupport.cs`, used by DsmOrthoToDtm) reads and checks the aligned RGB
  orthophoto, runs pix_grad (DSM) and rgb_grad_fm (stacked DSM, R, G, B) through
  `GeoKernelAI.RunRasterInference`, compares both with `Terrain.Estimate` and stages the export
  (pins in the `ortho-*.json` manifests); `OrthoText` holds the window text.

System GDAL, Python and a sibling SDK checkout are not required.
