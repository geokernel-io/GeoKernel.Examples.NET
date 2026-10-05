# ElevationProfile — Windows Forms

Downloads and opens the Sagrada Família DEM and orthophoto automatically. Click terrain to draw a route and display its elevation profile. Finish, undo or clear the route; adjust sampling from 1 to 100 metres.

Uses the same native `planTerrainProfile` and `advanceTerrainProfile` functions as the Qt example, processing 32 samples per timer tick. Routes have up to 64 points and profiles up to 4096 samples. Results describe the loaded 512-sample terrain mesh, with unverified source vertical datum. NoData gaps are not connected. Display exaggeration does not affect measurements.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.

```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "ElevationProfile.Winforms\ElevationProfile.Winforms.csproj" -c Release && "ElevationProfile.Winforms\bin\Release\net10.0-windows\ElevationProfile.Winforms.exe"
```

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.