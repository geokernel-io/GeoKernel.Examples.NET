# TerrainAnalysis — WinForms

Automatically downloads and opens the Sagrada Família DEM. Uses the same SDK terrain display modes as the Qt TerrainAnalysis example: terrain relief, slope in degrees and downhill aspect. Matching colour legends, 256/512/1024 mesh resolutions, vertical exaggeration, cancellation and camera reset are included.

Analysis describes the loaded mesh faces before vertical exaggeration, not an exported analysis raster. Resolution affects results. Aspect is clockwise from north in the local ENU frame, with eight 45-degree sectors and a flat class below 0.01 degrees. Heights are treated as metres with an unverified vertical reference.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.

From CMD:

```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "TerrainAnalysis.Winforms\TerrainAnalysis.Winforms.csproj" -c Release && "TerrainAnalysis.Winforms\bin\Release\net10.0-windows\TerrainAnalysis.Winforms.exe"
```

Data is cached in the application's data directory. No orthophoto or vector data is loaded.
