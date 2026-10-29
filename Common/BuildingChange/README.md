# Building change review

Shared analysis and review logic for [WinForms](../../BuildingChangeReview.Winforms/README.md)
and [WPF](../../BuildingChangeReview.Wpf/README.md). Both use the published
GeoKernel.NET **1.5.39** NuGet package and its GDAL/ONNX runtime. A sibling SDK
checkout, system GDAL and a Python installation are not required.

## Demo workflow

1. Startup downloads and verifies the Oosterwold imagery and HOTOSM model; the
   two synchronized maps initially show imagery only.
2. Choose **Demo area (fixed)**, **Current view**, or **Full dataset** and run.
   The fixed area is a 320 m square; the full images cover a 1024 m square.
3. Select a candidate in the table. Green means an addition, red a removal,
   gray a rejected candidate. Orange marks the selected location on both dates;
   its appearance on the old image does not mean a building existed there.
4. Review with **Approve**, **Reject**, or **Pending**. Edit vertices in the right
   map; an edit recalculates metric area and resets the decision to pending.
   Undo/redo work within the current native edit session. Browsing the table uses
   the native selection overlay: it keeps image/candidate layers and render
   caches intact. **Edit vertices** opens the selected polygon's edit session;
   selecting another candidate finishes that session and returns the map to Pan.
5. **Export GeoJSON** writes approved candidates only. Geometry hashes prevent
   a changed geometry from retaining its old approval. Reviewer, note, source
   years, scores, parameters and data/model hashes accompany the result.

Settings are collapsed initially. CPU is the default; CUDA requires a compatible
SDK GPU runtime. Cancel retains the previous published result. Native instance
inference supports cancellation; the additional score pass finishes before a
cancel or close completes. Closing awaits the worker and map readers before
removing session files. Review decisions are session-only until export.

## Analysis

The native `hotosm-buildings` task runs separately on each date. Its instance
polygons are rasterized to the same 0.5 m grid. A second `FloatBands` inference
per date supplies building scores for the opposite-date evidence filter. The
published 1.5.39 language API does not expose the instance task's decoded scores.
These ports apply sigmoid after logit blending; the Qt example applies it before
tile blending. The filters have the same purpose and defaults, but outputs are
not bit-identical and the additional inference increases runtime.

Default building threshold: **0.4371**. The following conditions select candidates;
components touching the analysis border are excluded.

| Filter | Minimum area | Other-date coverage below | Tolerance | Own mean score at least | Other-date mean score at most |
|---|---:|---:|---:|---:|---:|
| Conservative | 50 m² | 0.10 | 2 m | 0.75 | 0.20 |
| Balanced | 20 m² | 0.20 | 1 m | 0.55 | 0.35 |

Other-date coverage uses building scores of at least 0.25, buffered by the
tolerance. Areas are measured in EPSG:25831; display and exported coordinates
are WGS84 longitude/latitude. The editable native Polygon retains its rings;
multipart geometries are not flattened to an editable single ring.

CPU validation on the fixed area produced 0/68 instances and 42 addition
candidates in these ports. This is a functional check, not an accuracy estimate;
there are no reference change labels. Check shadow, vegetation and registration
effects during review. The imagery years name annual layers, not verified local
flight dates.

## Data and model

The [sample release](https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/almere_oosterwold_change_2016_2025_v1.zip)
contains `oosterwold_2016-epsg25831-50cm.tif` and
`oosterwold_2025-epsg25831-50cm.tif`, plus README, attribution and source manifest.
Both images are 2048 × 2048 RGB Byte, EPSG:25831, 0.5 m. Bounds are
`657533,5800382,658557,5801406`; the fixed demo bounds are
`657885,5800734,658205,5801054`.

Beeldmateriaal Nederland / PDOK provides the imagery under
[CC BY 4.0](https://creativecommons.org/licenses/by/4.0/);
see the [provider attribution](https://www.beeldmateriaal.nl/dataroom).
The package contains WMS JPEG renders of the 0.25 m annual layers on the 0.5 m
grid, converted to GeoTIFF without further pixel loss. These are derived renders.

The [HOTOSM DINOv3-S package](https://pub-00d214f25f754884b22349c19ac37bbf.r2.dev/geokernel-models/hotosm-dinov3s-buildings-geokernel-onnx-v1.zip)
retains HOTOSM attribution, model/dataset cards, CC BY 4.0 and the separate Meta
DINOv3 license. Preserve those files and applicable terms when redistributing.

| Verified object | SHA-256 |
|---|---|
| Imagery ZIP | `ecc35ef2c6bbdd586adc6ecc812ce1c74aee62fbf8e404de1d63b75bf7137a88` |
| 2016 GeoTIFF | `db723783db4c8738a716fe8b898de2e3099bab9394e335bd489db2412ae0c5c3` |
| 2025 GeoTIFF | `7b1b12d8b27cd0714f140415004daf9fd0000495eda31366c7cd4aab218caa82` |
| Model ZIP | `3a6cd11e013f81b46b3db700feddef2d98319780285b8320e1200573684b24bb` |
| model.onnx | `9a28e28403accc06c70a3104aedb31eca50401ed23c8ba4314b26357c25e9043` |

Assets are cached under `%LOCALAPPDATA%\GeoKernel\Examples\GeoAI` by archive
hash. Set `GEOKERNEL_EXAMPLES_DATA_DIR` to change the cache root. Downloads occur
on first use; later runs use verified cached inputs. Source images remain unchanged.
