# GlobeBuildings (WinForms)

Real Sagrada Familia buildings over Esri satellite imagery, with atmosphere and
the same oblique starting camera as the Qt example.

```powershell
cd D:\projects\GeoKernel.Examples.NET\GlobeBuildings.Winforms
dotnet run -c Release
```

Requires Windows x64, .NET 10 SDK, a Vulkan-capable graphics driver and a valid
GeoKernel license. NuGet restores the published **GeoKernel.NET 1.5.35** SDK and
native runtime automatically. First launch downloads
[Sagrada Familia buildings](https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/sagrada_familia_buildings.zip)
into `data/sagrada_familia_buildings` beside the executable. Later launches reuse
the files. Internet is required for the first download and Esri imagery.

Buildings initially appear as flat footprints. Click **Extrude** to build 3D
geometry; click again to restore footprints. **Display height (m)** defaults to
18 metres. This source has no measured building heights: the chosen value is a
display setting, not survey data. Turn extrusion off before changing the height.

Matches Qt: uniform beige buildings, no DEM, and bases on the reference sphere.
Polygons rejected by SDK fill validation are counted as outline-only in the
footprint view and excluded from extrusion. Zoom to buildings restores the
starting camera; Atmosphere and Esri toggle those features.

Loading and extrusion use asynchronous SDK jobs. Controls are disabled during
these operations, and closing cancels pending work. Includes an application icon,
fixed 24-pixel status bar, no console window, and no test modes or application
command-line parameters.
