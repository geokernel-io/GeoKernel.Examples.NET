# GlobeHello — WinForms

Embeds the Globe control from the published **GeoKernel.NET 1.5.35** NuGet package.
Like the Qt GlobeHello example, it starts with a blank globe. Use the toolbar to
enable atmosphere, Esri satellite imagery, online DEM and sunlight independently.

Requires Windows x64, .NET 10 SDK and a Vulkan-capable graphics driver.

```powershell
dotnet build -c Release
dotnet run -c Release --no-build
```

Build restores the SDK and native runtime automatically from NuGet. Imagery and
elevation tiles are requested from Esri as you navigate; internet access is
required for those layers. No synthetic data, separate data preparation, local
SDK checkout or application command-line options are used.

Left drag pans; right drag or wheel zooms; middle drag / Ctrl orbits.
The status bar shows attribution and service errors. The application icon is
shared with the other examples.
