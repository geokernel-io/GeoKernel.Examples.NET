# TerrainAnalysis — WPF

Automatically downloads and opens the Sagrada Família DEM. Uses the same native terrain relief, slope and aspect display modes as the Qt and WinForms examples. Includes matching colour legends, 256/512/1024 mesh resolution, vertical exaggeration, cancellation and camera reset.

Analysis describes loaded mesh faces before exaggeration, not an exported analysis raster. Mesh resolution affects results. Aspect is clockwise from north in the local ENU frame; each compass sector spans ±22.5°. Slopes below 0.01° use the flat class. Heights are metres with an unverified vertical reference.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.

From CMD:

```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "TerrainAnalysis.Wpf\TerrainAnalysis.Wpf.csproj" -c Release && "TerrainAnalysis.Wpf\bin\Release\net10.0-windows\TerrainAnalysis.Wpf.exe"
```

Data is cached under the application's data directory. The downloader uses a single request without automatic retries. No imagery or vector data is loaded.
