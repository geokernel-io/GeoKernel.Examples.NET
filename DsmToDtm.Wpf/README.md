# DsmToDtm — WPF

Ground beneath buildings: an original DSM stays visible beside a prepared terrain
estimate in two linked native GeoKernel 3D viewers. Choose the classical
morphological/TIN or experimental DSM-only pix_grad estimate and move the surface
slider; heights are interpolated numerically between both endpoints. **No inference
runs here.** WPF port of the Qt `DsmToDtm` example.

Requires Windows x64, .NET SDK 10, Vulkan and a valid GeoKernel license. The
published GeoKernel.NET **1.5.40** NuGet package, its native/GDAL runtime and the
pinned sample are restored/downloaded automatically.

From this directory:

```powershell
dotnet run -c Release
```

From the repository root:

```powershell
dotnet run --project DsmToDtm.Wpf -c Release
```

Options: `--sample-archive <zip>` uses an exact local copy of the pinned ZIP,
`--sample-url <https-url>` selects a mirror and `--data-dir <folder>` moves the
sample cache (default: `data` beside the executable). All hash pins still apply.

## Workflow

1. Startup downloads and verifies the Zurich v2 sample (archive and every file).
2. The left view always shows the original DSM; the right starts with the
   classical DTM at 100%. Pan, orbit or zoom either view; the other follows.
3. **0% is exactly the DSM, 100% exactly the selected DTM.** Intermediate values
   write a temporary Float32 GeoTIFF and rebuild the right-hand terrain.
4. The profile chart samples all four rasters on the fixed building transect:
   gray DSM, teal classical, purple AI, dashed orange reference.
5. Vertical exaggeration applies to both views. Cancel keeps the previous complete
   views and restores the last committed selection. Closing during preparation
   cancels first, then releases both viewers.

The reference DTM is used only for the profile and stored crop scores.

## Differences from the Qt example

The published Viewer3D C API has no exact ENU camera mapping and no analysis-line
overlay. Render-space cameras are copied directly (both scenes share one grid and
grid-centre origin, so they correspond to within 1e-5), the transect is shown only
in the profile chart, and native terrain preparation shows an indeterminate
progress bar.

See [the shared code](../Common/DsmToDtm/README.md) for sample and data details.
Data: Federal Office of Topography swisstopo (swissSURFACE3D Raster, swissALTI3D);
AI: David Perez's pix_grad checkpoint, CC BY 4.0 (derived output only, no weights).
