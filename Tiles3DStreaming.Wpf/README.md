# Tiles3DStreaming — WPF

Automatically downloads Sagrada Família terrain, orthophoto, 3D Tiles and the
EGM08D595 geoid. Uses the same native camera-driven streamer as Qt and WinForms.
Includes geometry budget, start/stop, visibility, focus and live diagnostics.

Stop retains current tiles. Hiding clears them; showing restarts streaming.
Cache counters are not total RAM/VRAM. Terrain uses the regional geoid correction;
alignment is approximate. Vertical exaggeration stays at 1×.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.

```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "Tiles3DStreaming.Wpf\Tiles3DStreaming.Wpf.csproj" -c Release && "Tiles3DStreaming.Wpf\bin\Release\net10.0-windows\Tiles3DStreaming.Wpf.exe"
```

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.