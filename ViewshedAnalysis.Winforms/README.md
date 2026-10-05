# ViewshedAnalysis — Windows Forms

Downloads and opens the Sagrada Família DEM and orthophoto automatically. Select an observer on terrain, adjust eye/target height, radius, ray spacing and grid resolution, then calculate visibility.

Results appear both in a north-up grid and as a 55%-opacity overlay on the 3D terrain. Includes pause/resume, clear, overlay visibility, imagery visibility and vertical exaggeration. Changing analysis settings invalidates the previous result.

Uses the Qt example's existing native `planViewshed` / `advanceViewshed` algorithm and triangle-clipped terrain overlay. Calculation advances in batches of 256 work steps; overlay preparation advances through 1024 terrain triangles per tick. Visibility is sampled on the 512-sample terrain mesh; buildings, vegetation and refraction are excluded. The vertical datum is unverified. Display exaggeration does not change the analysis.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.





```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "ViewshedAnalysis.Winforms\ViewshedAnalysis.Winforms.csproj" -c Release && "ViewshedAnalysis.Winforms\bin\Release\net10.0-windows\ViewshedAnalysis.Winforms.exe"
```

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.