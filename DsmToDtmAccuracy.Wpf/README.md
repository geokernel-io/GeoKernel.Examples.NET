# DsmToDtmAccuracy — WPF

Reference accuracy by area: compare **where classical and AI terrain estimates differ
from the same reference**. Three linked native GeoKernel **2D maps** keep the original
DSM on the left, with classical and AI error maps beside it on one shared colour scale.
Select an area to compare numeric errors, their distribution and individual source
pixels. **No inference runs here.** WPF port of the Qt `DsmToDtmAccuracy` example.

Requires Windows x64, .NET SDK 10 and a valid GeoKernel license. The published
GeoKernel.NET **1.5.40** NuGet package, its native/GDAL runtime and the pinned sample
are restored/downloaded automatically.

```powershell
dotnet run --project DsmToDtmAccuracy.Wpf -c Release
```

Options: `--sample-archive <zip>`, `--sample-url <https-url>`, `--data-dir <folder>`
(default: `data` beside the executable). All hash pins still apply.

## Workflow

1. Startup reuses the verified Zurich v2 sample and prepares 19 shared-scale Byte RGB
   display maps (DSM relief; per method 4 signed, 4 absolute and 1 terrain relief).
2. **Signed reference errors** (blue below, white zero, red above), **Absolute reference
   errors** or **Terrain estimates**. **Display limit** (1, 2, 5, 10 m) only sets colour
   saturation; scores never change with colours or zoom.
3. Choose a geographic preset or **Select evaluation area** and drag a box on any map.
   Both methods are evaluated on the same source cells; cells outside are dimmed on all
   maps. **Evaluate current view** and **Full sample** set the area directly.
4. The table lists RMSE, MAE, bias, nearest-rank P95 of |error| and the share above
   **Large error >**; the sidebar names the lower RMSE and lower MAE separately (no
   overall winner). The histogram shows six absolute-error bins as percentages.
5. With Pan active, click a pixel to read DSM, reference, classical and AI heights and
   both signed errors; a marker shows the cell on every map.
6. **Export area report** writes JSON metrics, histogram counts, threshold, pixel window,
   cell bounds, manifest, archive hash and attribution, atomically.

Full sample: classical RMSE 1.006692 / MAE 0.350048 / bias 0.247517 m; AI RMSE 0.659430 /
MAE 0.406952 / bias −0.277471 m. Presets are geographic windows, not land-cover classes.

## Differences from the Qt example

The maps are native windows, so the area is drawn with the viewer's owned shapes: four
translucent rectangles dim the outside and a solid outline marks the evaluated cells.
GeoKernel.NET 1.5.40 wraps no overlay point, so the probe is a point shape redrawn with
the area. Views are linked by extent. Each map adopts its first layer's CRS (no
EPSG:2056 preset).
WPF has no numeric spin box; **Large error >** is a list of 0.1–20 m values.

See [the shared code](../Common/DsmToDtm/README.md) for sample and data details.
Data: Federal Office of Topography swisstopo; AI: David Perez's pix_grad adaptation,
CC BY 4.0. Fixed development crop, not a held-out benchmark.
