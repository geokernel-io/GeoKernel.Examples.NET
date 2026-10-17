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
