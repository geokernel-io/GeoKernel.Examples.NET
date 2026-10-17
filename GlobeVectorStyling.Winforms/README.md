# GlobeVectorStyling (WinForms)

Displays real Sagrada Familia building footprints over Esri satellite imagery,
with atmosphere and the same oblique starting camera as the Qt example.

## Run

Requires Windows x64, .NET 10 SDK and a Vulkan-capable graphics driver.

```powershell
cd D:\projects\GeoKernel.Examples.NET\GlobeVectorStyling.Winforms
dotnet run -c Release
```

NuGet restores the published **GeoKernel.NET 1.5.35** SDK and runtime automatically.
On first launch, the application downloads and extracts
[Sagrada Familia buildings](https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/sagrada_familia_buildings.zip)
into `data/sagrada_familia_buildings` beside the executable. Later launches reuse
these files. Internet is required for the first download and Esri imagery.

## Controls

- **Fill / Labels:** polygon fills and attribute labels.
- **Color by:** source attribute categories, or Uniform for a single color.
- **Label:** source attribute used for labels.
- **Opacity:** building layer transparency.
- **Base color:** uniform or fallback color.
- **Zoom to buildings:** restore the initial building view.
- **Esri:** toggle satellite imagery.
- Click a building to show its attributes; hover the status text to read long values.

Polygon fills rejected by SDK validation remain outline-only. Their count appears
in the fixed-height status bar. Data loading and styling use asynchronous SDK jobs;
closing the window cancels pending work. The example has an application icon,
no console window and no test modes or command-line arguments.
