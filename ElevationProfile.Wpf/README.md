# ElevationProfile — WPF

Automatically downloads and opens the Sagrada Família DEM and orthophoto. Click terrain to draw a route; its elevation profile appears below the map. Finish, undo or clear the route, and adjust sample spacing from 1 to 100 metres.

Uses the same native profile calculation as the Qt and WinForms examples: 32 samples per timer tick, up to 64 route points and 4096 samples. Heights describe the loaded terrain mesh in its unverified source vertical datum. Missing samples remain gaps; display exaggeration does not affect measurements.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.

```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "ElevationProfile.Wpf\ElevationProfile.Wpf.csproj" -c Release && "ElevationProfile.Wpf\bin\Release\net10.0-windows\ElevationProfile.Wpf.exe"
```

The package-only configuration targets planned version 1.5.32, which must be published before using that configuration.
