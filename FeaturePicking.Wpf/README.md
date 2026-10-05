# FeaturePicking WPF

Downloads the Sagrada Familia DEM, orthophoto, road and building shapefiles and
loads them together. Click a building or road to highlight it and inspect its
source attributes. Multiple selection, active-feature switching, zoom to
selection, Escape/clear, layer visibility and vertical exaggeration are included.
Buildings use an illustrative 9 m height. This is not a surveyed elevation model.

Picking uses the existing ViewerController implementation used by the Qt example.
Selection indices are scene-local and guarded by a snapshot revision. Attribute
values are display strings (null remains null); 64-bit IDs are preserved.

## SDK availability

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.

In a fresh CMD window, build the updated native target:



Then build and launch the example:

```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "FeaturePicking.Wpf\FeaturePicking.Wpf.csproj" -c Release && "FeaturePicking.Wpf\bin\Release\net10.0-windows\FeaturePicking.Wpf.exe"
```

After the selection API is published in 1.5.32, omit `-p:UseLocalGeoKernel=true`
to use the NuGet package alone.

## Manual validation

- The four datasets download automatically and the complete scene loads.
- Clicking a building/road highlights it and displays source attributes.
- Multiple selection adds/removes features and the list activates a selection.
- Zoom to selection and Escape/clear work without changing source data.
- Hiding a layer clears selections that are no longer visible.
- Reload, cancellation and closing during loading finish without stale selection.

This change has not been built or run by the assistant, as requested.

