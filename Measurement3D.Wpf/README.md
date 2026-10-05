# Measurement3D — WPF

Downloads and opens the Sagrada Família DEM and orthophoto automatically. Click
terrain to measure distance or horizontal polygon area. Measurements appear on
the map and in a coordinate table. Finish stops adding points; Backspace undoes
the last point and Esc clears it. Buttons provide the same actions.

Uses the same native measurement API as WinForms and the Qt measurement logic.
Points are physical ENU metres, independent of display exaggeration. 3D length
uses straight segments, not terrain-following length. Area is plan area, not
terrain surface area. The DEM datum is unverified; maximum 64 points.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.

```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "Measurement3D.Wpf\Measurement3D.Wpf.csproj" -c Release && "Measurement3D.Wpf\bin\Release\net10.0-windows\Measurement3D.Wpf.exe"
```

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.