# GlobeTerrainNavigation — WinForms

Uses the published **GeoKernel.NET 1.5.35** package and matches the Qt
GlobeTerrainNavigation example. Opens above Sagrada Familia in Barcelona with
Esri satellite imagery, streamed DEM, atmosphere and sunlight enabled.

Requires Windows x64, .NET 10 SDK, a Vulkan-capable graphics driver and a valid
GeoKernel license. Internet access is required for imagery and elevation.

```powershell
dotnet build -c Release
dotnet run -c Release --no-build
```

NuGet restores the SDK and native runtime automatically. No local SDK build,
manual data download or synthetic terrain is needed.

The navigation toolbar provides zoom in/out, rotation, tilt, top view and north
alignment. **Overview** restores the starting camera: longitude 2.1744, latitude
41.4036, target height 40 m, distance 2500 m, pitch approximately 37.24 degrees.
The four layer buttons independently toggle DEM, imagery, atmosphere and light.

Left drag pans; right drag or wheel zooms; middle drag / Ctrl + left drag orbits.
The status bar stays 24 pixels high and displays attribution and service errors.
The application uses the shared icon and opens without a console. No application
command-line parameters, diagnostics or test modes are included.
