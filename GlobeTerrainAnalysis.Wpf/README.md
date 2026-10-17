# GlobeTerrainAnalysis — WPF

```powershell
cd D:\projects\GeoKernel.Examples.NET\GlobeTerrainAnalysis.Wpf
dotnet run -c Release
```

Uses published GeoKernel.NET 1.5.35. Downloads the real Sagrada Familia terrain
GeoTIFF automatically, verifies SHA-256 and caches it under the output `data`
folder. Opens over Barcelona with Esri imagery, atmosphere, local DEM at 1x and
DEM color shading disabled. Requires Windows x64, .NET 10, a Vulkan-capable GPU,
a valid license and internet for downloads/imagery.

Choose a tool, then click the terrain:

- Line of sight: observer and target, 1.7 m above terrain, within 10 km. The map
  line marks the ground path, not the elevated sight ray.
- Viewshed: one observer, radius 350 m and sample spacing 15 m.
- Slope / aspect: one point; displays degrees, percent and downhill direction
  clockwise from north. Flat terrain has no defined aspect.
- Measure: start and end points; displays ground distance.

Further clicks start a new selection. Clear or Esc cancels; Barcelona restores
the camera. Analysis is terrain-only and does not include buildings. If terrain
residency changes during calculation, the old result is marked stale: wait for
loading to settle and select again. Missing terrain samples are reported rather
than assumed visible. The fixed 170-DIP scrollable result panel and 24-DIP
status bar do not grow with result text. Includes application icon and native
cleanup; no console window, application CLI parameters or smoke-test modes.

