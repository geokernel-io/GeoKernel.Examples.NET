# ViewshedAnalysis — WPF

Automatically downloads and opens the Sagrada Família DEM and orthophoto. Select an observer on terrain, set eye/target height, radius, ray spacing and grid size, then calculate visibility. Includes pause/resume, clear, orthophoto visibility and vertical exaggeration.

Results appear in a north-up grid and as a 55%-opacity overlay on the 3D terrain. Uses the same native calculation and triangle-clipped overlay as the Qt and WinForms examples. Changing analysis settings invalidates the previous result.

Analysis samples the loaded 512-sample terrain mesh; buildings, vegetation and atmospheric refraction are excluded. Small obstacles between samples can be missed. Vertical datum is unverified; exaggeration changes only the view.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.

```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "ViewshedAnalysis.Wpf\ViewshedAnalysis.Wpf.csproj" -c Release && "ViewshedAnalysis.Wpf\bin\Release\net10.0-windows\ViewshedAnalysis.Wpf.exe"
```

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.