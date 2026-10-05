# Tiles3DStreaming — WinForms

Downloads Sagrada Família DEM, orthophoto, 3D Tiles and the regional EGM08D595
geoid. Uses the same native camera-driven streamer as the Qt example. Controls
include a 64–128 MiB geometry budget, start/stop, visibility and focus. Counters
report tile selection, cache and GPU upload state; they are not total RAM/VRAM.

DEM heights receive the same geoid correction as Qt. Alignment is approximate,
not independently surveyed. Vertical exaggeration remains 1×. Hiding clears the
model; showing restarts streaming. Stop keeps the current tiles.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.



```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "Tiles3DStreaming.Winforms\Tiles3DStreaming.Winforms.csproj" -c Release && "Tiles3DStreaming.Winforms\bin\Release\net10.0-windows\Tiles3DStreaming.Winforms.exe"
```

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.