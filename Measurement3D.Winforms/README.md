# Measurement3D — WinForms

Automatically downloads the Sagrada Família DEM and orthophoto. Enable Add points
and click the terrain to measure a polyline or polygon. The scene shows the same
measurement overlay as the Qt example. Finish stops adding points; Backspace
undoes the last point and Esc clears the measurement.

Results use physical local ENU metres, independent of vertical exaggeration.
3D length uses straight segments, not terrain-following length. Polygon area is
horizontal plan area, not terrain surface area. Invalid polygons report an error.
The limit is 64 points; DEM vertical datum is unverified.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.



```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "Measurement3D.Winforms\Measurement3D.Winforms.csproj" -c Release && "Measurement3D.Winforms\bin\Release\net10.0-windows\Measurement3D.Winforms.exe"
```

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.