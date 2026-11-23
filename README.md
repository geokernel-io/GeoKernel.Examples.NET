# GeoKernel .NET Examples

Official C# examples for the GeoKernel native GIS SDK, including WPF and Windows Forms applications.

## Requirements

- Windows 10 or later
- A supported .NET SDK
- Visual Studio
- A valid GeoKernel license

## Getting Started

Choose a WPF or Windows Forms example directory, open its project file, restore dependencies and run the application.

## Globe examples

- [GlobeHello.Winforms](GlobeHello.Winforms/README.md): embedded Vulkan globe with atmosphere, Esri satellite imagery, online DEM and sunlight controls. Restores the published GeoKernel.NET 1.5.35 package automatically.
- [GlobeHello.Wpf](GlobeHello.Wpf/README.md): the same GlobeHello controls in WPF, using the published 1.5.35 SDK and a fixed-height status bar.

## Agent workflows (GeoKernel.NET newer than 1.5.40)

- [AgentMapAssistant.Winforms](AgentMapAssistant.Winforms/README.md) and [AgentMapAssistant.Wpf](AgentMapAssistant.Wpf/README.md): ask for map operations in plain language; an LLM (or the offline demo's prepared plans) proposes a workflow DSL v2 plan that GeoKernel validates, previews, runs after confirmation when it changes data, undoes and records with hash-chained provenance. Builds against the GeoKernel.NET sources until the agent API is published.
- [TextPromptChangeDetection.Winforms](TextPromptChangeDetection.Winforms/README.md) and [TextPromptChangeDetection.Wpf](TextPromptChangeDetection.Wpf/README.md): type what to look for ("building", "greenhouse") and see where it appeared and disappeared between two dates or over a 9-date series, with a timeline, a QA review queue and GeoPackage export (SAM 3, objects tracked from date to date). Builds against the GeoKernel.NET sources until the change task API is published.

## DSM to DTM examples (published GeoKernel.NET 1.5.40)

- [DsmToDtm.Winforms](DsmToDtm.Winforms/README.md) and [DsmToDtm.Wpf](DsmToDtm.Wpf/README.md): ground beneath buildings; two linked native 3D views of an original DSM and prepared classical/AI terrain, numeric height transition, fixed transect profile and reference metrics.
- [DsmToDtmProfile.Winforms](DsmToDtmProfile.Winforms/README.md) and [DsmToDtmProfile.Wpf](DsmToDtmProfile.Wpf/README.md): draw your own A → B transect on two linked native 2D relief maps; DSM, classical, AI and reference heights on one chart, error mode, hover inspection, line RMSE/MAE and CSV export.
- [DsmToDtmAccuracy.Winforms](DsmToDtmAccuracy.Winforms/README.md) and [DsmToDtmAccuracy.Wpf](DsmToDtmAccuracy.Wpf/README.md): three linked 2D maps of the DSM and shared-scale classical/AI reference errors; area selection and presets, RMSE/MAE/bias/P95, histogram, pixel probe and JSON report.
- [DsmToDtmProcessing.Winforms](DsmToDtmProcessing.Winforms/README.md) and [DsmToDtmProcessing.Wpf](DsmToDtmProcessing.Wpf/README.md): run the classical multiscale-morphological + TIN conversion live with presets and six parameters; DSM, new DTM and removed-height/quality/reference-error maps, scores, pixel probe and GeoTIFF/QA/JSON export.
- [DsmToDtmAI.Winforms](DsmToDtmAI.Winforms/README.md) and [DsmToDtmAI.Wpf](DsmToDtmAI.Wpf/README.md): run the experimental pix_grad model through GeoKernel.AI and the classical filter on the same full 1 km Zurich DSM; relief/removed-height/error maps, full-input and shared-crop scores, pixel probe and a staged comparison-folder export.
- [DsmOrthoToDtm.Winforms](DsmOrthoToDtm.Winforms/README.md) and [DsmOrthoToDtm.Wpf](DsmOrthoToDtm.Wpf/README.md): compare classical, DSM-only pix_grad and DSM+RGB rgb_grad_fm terrain estimates on the same full Zurich DSM; orthophoto/DSM input context, four linked 2D maps, full-input and shared-crop scores, pixel probe and a staged comparison-folder export.

## GeoAI examples (published GeoKernel.NET 1.5.39)

- [BuildingChangeReview.Winforms](BuildingChangeReview.Winforms/README.md) and [BuildingChangeReview.Wpf](BuildingChangeReview.Wpf/README.md): Oosterwold 2016/2025 change candidates, synchronized maps, vertex editing, review decisions and approved-only GeoJSON export.
- [DynamicWorldInference.Winforms](DynamicWorldInference.Winforms/README.md) and [DynamicWorldInference.Wpf](DynamicWorldInference.Wpf/README.md): Sentinel-2 land cover, output opacity and Shapefile export.
- [HotosmBuildingFootprints.Winforms](HotosmBuildingFootprints.Winforms/README.md) and [HotosmBuildingFootprints.Wpf](HotosmBuildingFootprints.Wpf/README.md): one RGB building raster, threshold 0.50, session-only footprints and Shapefile export.
- [GeoAiUsaBuildingFootprints.Winforms](GeoAiUsaBuildingFootprints.Winforms/README.md) and [GeoAiUsaBuildingFootprints.Wpf](GeoAiUsaBuildingFootprints.Wpf/README.md): Mask R-CNN on the same RGB building raster, confidence 0.50 and explicit Shapefile export.
- [PrithviFloodAnalysis.Winforms](PrithviFloodAnalysis.Winforms/README.md) and [PrithviFloodAnalysis.Wpf](PrithviFloodAnalysis.Wpf/README.md): Sentinel-2 water/flood mapping, a separate RGB source display, output opacity and water-only Shapefile export.
- [PrueFieldBoundaries.Winforms](PrueFieldBoundaries.Winforms/README.md) and [PrueFieldBoundaries.Wpf](PrueFieldBoundaries.Wpf/README.md): two-period Sentinel-2 field segmentation, three-class overlay and field-interior Shapefile export.
- [Sam3SegmentationInference.Winforms](Sam3SegmentationInference.Winforms/README.md) and [Sam3SegmentationInference.Wpf](Sam3SegmentationInference.Wpf/README.md): text/point/box prompting, visible map selections, draft Accept/Discard and accepted Shapefile/GeoTIFF exports.
- [Sen2srSuperResolution.Winforms](Sen2srSuperResolution.Winforms/README.md) and [Sen2srSuperResolution.Wpf](Sen2srSuperResolution.Wpf/README.md): 4× RGBN super-resolution, source/result blending and scientific Float32 GeoTIFF export.

These examples use `Common/GeoAi` and the published GeoKernel.NET 1.5.39 NuGet package with its native runtime. Run `dotnet run -c Release` in an example directory; no sibling SDK checkout or native build is needed.

## Resources

- [GeoKernel Website](https://geokernel.io)
- [Documentation](https://docs.geokernel.io)
- [Sample Data](https://github.com/geokernel-io/GeoKernel.SampleData)

## Support

For questions, contact [admin@geokernel.io](mailto:admin@geokernel.io).

### LayerStylingAndFiltering (WinForms)

[LayerStylingAndFiltering.Winforms](LayerStylingAndFiltering.Winforms/README.md)
automatically loads terrain, orthophoto, roads and buildings. Supports per-layer
visibility, opacity, uniform/source colors, attribute coloring, substring filtering,
matching counts and zoom to matches. Requires the new local Viewer3D styling bridge
until the next package release. Build/run commands are in the example README.

### LayerStylingAndFiltering (WPF)

[LayerStylingAndFiltering.Wpf](LayerStylingAndFiltering.Wpf/README.md) provides the
same per-layer styling, attribute coloring and substring filtering as the WinForms
and Qt examples, with automatic sample downloads. Uses the local Viewer3D styling
bridge until the next SDK package release. See the example README for commands.

### ProjectSaveLoad (WinForms)

[ProjectSaveLoad.Winforms](ProjectSaveLoad.Winforms/README.md) loads the sample
automatically and saves/opens local .gk3d projects using the same format as Qt.
Persists camera, source references, height scale, layer styling and filters.
Requires the new local Viewer3D project bridge until the next package release.

### ProjectSaveLoad (WPF)

[ProjectSaveLoad.Wpf](ProjectSaveLoad.Wpf/README.md) loads the sample automatically
and opens/saves the same .gk3d format as Qt and WinForms. Includes layer styling,
filters, camera and height-scale restoration. Use the local project bridge until
the next SDK package release; build/run commands are in the example README.

### GlobeTerrainNavigation (WinForms)

[GlobeTerrainNavigation.Winforms](GlobeTerrainNavigation.Winforms/README.md) opens
above Barcelona with real Esri imagery, DEM, atmosphere and sunlight. Includes
zoom, rotate, tilt, top view, north and overview controls. Uses the published
GeoKernel.NET 1.5.35 package, restored automatically by NuGet.

### GlobeTerrainNavigation (WPF)

[GlobeTerrainNavigation.Wpf](GlobeTerrainNavigation.Wpf/README.md) provides the
same Barcelona terrain view and navigation controls as Qt and WinForms. Uses
GeoKernel.NET 1.5.35 from NuGet, with the application icon and a fixed-height
status row. Imagery and elevation stream automatically from Esri.

### GlobeVectorStyling (WinForms)

[GlobeVectorStyling.Winforms](GlobeVectorStyling.Winforms/README.md) downloads real
Sagrada Familia buildings and displays them over Esri imagery. Includes fills,
attribute categories, labels, opacity, base color, camera reset and identification.
Uses the published GeoKernel.NET 1.5.35 package with automatic NuGet restore.

### GlobeVectorStyling (WPF)

[GlobeVectorStyling.Wpf](GlobeVectorStyling.Wpf/README.md) provides the same real
Sagrada Familia buildings, attribute styling, labels and identification as the
WinForms example. SDK 1.5.35 and sample data download automatically. Includes an
application icon and a fixed-height status row.

### GlobeBuildings (WinForms)

[GlobeBuildings.Winforms](GlobeBuildings.Winforms/README.md) opens real Sagrada
Familia footprints over Esri imagery. Extrude toggles 3D buildings using a chosen
display height; the source has no measured heights. Matches the Qt example and
automatically downloads SDK 1.5.35 and sample data.

### GlobeBuildings (WPF)

[GlobeBuildings.Wpf](GlobeBuildings.Wpf/README.md) matches the WinForms building
example: real Sagrada Familia footprints, optional extrusion, display height,
Esri and atmosphere. Downloads SDK 1.5.35 and sample data automatically, and
waits for GPU upload before reporting extrusion complete.

## GlobeModelPlacement — WinForms

[GlobeModelPlacement.Winforms](GlobeModelPlacement.Winforms/README.md) automatically
downloads the real Sagrada Familia model and opens it over Esri imagery with
atmosphere. Includes rotation, scale, anchor height, remove/restore and camera
controls, using published SDK 1.5.35. See the example README for the known model
precision limitation affecting some placements.

```powershell
cd GlobeModelPlacement.Winforms
dotnet run -c Release
```

## GlobeModelPlacement WPF

See [GlobeModelPlacement.Wpf](GlobeModelPlacement.Wpf/README.md) for automatic model download, placement controls and the SDK precision limitation. Run: dotnet run -c Release --project GlobeModelPlacement.Wpf.

## GlobeRasterServices — WinForms

[GlobeRasterServices.Winforms](GlobeRasterServices.Winforms/README.md) opens with
real Sagrada Familia orthophoto and DEM, Esri imagery and atmosphere. Uses SDK
1.5.35 and automatically downloads the sample data; no service-selection toolbar.

```powershell
cd GlobeRasterServices.Winforms
dotnet run -c Release
```

## GlobeRasterServices — WPF

[GlobeRasterServices.Wpf](GlobeRasterServices.Wpf/README.md) opens with real Sagrada
Familia orthophoto and local DEM, Esri imagery and atmosphere. Data downloads
automatically; the status panel has a fixed height and there is no service toolbar.

```powershell
cd GlobeRasterServices.Wpf
dotnet run -c Release
```

## GlobeTerrainAnalysis — WinForms

[GlobeTerrainAnalysis.Winforms](GlobeTerrainAnalysis.Winforms/README.md) provides
interactive terrain-only line of sight, viewshed, slope/aspect and measurement
with real Barcelona DEM, Esri imagery and atmosphere. Results use a fixed-height
panel. Run `dotnet run -c Release --project GlobeTerrainAnalysis.Winforms`.

## GlobeTerrainAnalysis — WPF

[GlobeTerrainAnalysis.Wpf](GlobeTerrainAnalysis.Wpf/README.md) provides interactive
terrain-only line of sight, viewshed, slope/aspect and measurement with real
Barcelona DEM, Esri imagery and atmosphere. Results use a fixed-height panel.

```powershell
cd GlobeTerrainAnalysis.Wpf
dotnet run -c Release
```
