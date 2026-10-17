# GlobeHello — WPF

Embeds the Globe control from the published **GeoKernel.NET 1.5.35** NuGet package.
Matches Qt and WinForms GlobeHello: a blank globe with independent atmosphere,
Esri satellite imagery, online DEM and sunlight switches.

Requires Windows x64, .NET 10 SDK and a Vulkan-capable graphics driver.

```powershell
dotnet build -c Release
dotnet run -c Release --no-build
```

Build automatically restores the SDK and native runtime from NuGet. Esri imagery
and elevation tiles load as you navigate and require internet access. There is
no synthetic data, preparation script, local SDK dependency or application
command-line parameter.

Left drag pans; right drag or wheel zooms; middle drag / Ctrl orbits.
The fixed-height status bar displays attribution and service errors. The icon
is shared with the other examples. Closing the window stops the status timer
and disposes the embedded native Globe control.
