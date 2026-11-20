# DsmToDtmProfile — WinForms

Interactive DSM / DTM elevation profile: draw your own **A → B transect** on two
linked native GeoKernel **2D maps** and compare the original DSM, classical DTM, AI
DTM and provider reference heights on one chart. **No inference runs here.**
WinForms port of the Qt `DsmToDtmProfile` example.

Requires Windows x64, .NET SDK 10 and a valid GeoKernel license. The published
GeoKernel.NET **1.5.40** NuGet package, its native/GDAL runtime and the pinned sample
are restored/downloaded automatically.

From this directory:

```powershell
dotnet run -c Release
```

From the repository root:

```powershell
dotnet run --project DsmToDtmProfile.Winforms -c Release
```

Options: `--sample-archive <zip>` uses an exact local copy of the pinned ZIP,
`--sample-url <https-url>` selects a mirror and `--data-dir <folder>` moves the
sample cache (default: `data` beside the executable). All hash pins still apply.

## Workflow

1. Startup reuses the verified Zurich v2 sample of `DsmToDtm` (archive and every
   file SHA-256 checked) and validates the four numeric height rasters.
2. The left map is always the original DSM; choose **Classical / AI / Reference**
   for the right map. Both maps share position and scale.
3. The fixed transect loads initially. Choose **Draw transect**, click **A**, then
   **B** on either map. Dragging still pans; toggling Draw off cancels an unfinished
   line. Clicks outside the valid pixel centres are rejected; endpoints must be at
   least 0.5 m apart.
4. The chart shows gray DSM, teal classical, purple AI and dashed orange reference;
   **Compare errors against reference** plots both estimates minus the reference.
5. Hovering over the chart marks the sample on both maps and lists its distance,
   coordinates and four heights. **Sample spacing** (0.5–10 m) resamples the line.
6. **Export profile CSV** writes the numeric samples and errors, then
   `<name>.csv.metadata.json`; each file atomically.
7. Cancel stops preparation; closing during loading cancels first.

Profiles sample the Float32 arrays bilinearly between pixel centres (no
extrapolation, no datum conversion); RMSE/MAE describe this line only. Both maps use
the same fixed hillshade (azimuth 315°, altitude 40°), written as display-only RGB
GeoTIFFs.

## Differences from the Qt example

The maps are native windows, so the transect uses the viewer's owned shapes instead
of a painter overlay. GeoKernel.NET 1.5.40 wraps no overlay point or text shape: the
A/B markers are distinguished by colour (A teal, B orange) rather than letter tags,
and hovering redraws the shapes, which re-renders both maps. The viewer has no
EPSG:2056 preset; each map adopts the CRS of its first relief layer.

See [the shared code](../Common/DsmToDtm/README.md) for sample and data details.
Data: Federal Office of Topography swisstopo (swissSURFACE3D Raster, swissALTI3D);
AI: David Perez's pix_grad checkpoint, CC BY 4.0 (derived output only, no weights).
