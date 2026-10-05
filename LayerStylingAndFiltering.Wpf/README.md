# LayerStylingAndFiltering — WPF

Loads the Sagrada Família DEM, orthophoto, roads and buildings automatically.
Downloads are cached under the output directory's data folder.

Select roads or buildings to change visibility, opacity, uniform/source color,
attribute coloring, and a literal case-insensitive substring filter. Each layer
keeps its settings. Example values are limited to 200 per field; filters search
all records. Matching records can include features without drawable geometry.
Zoom is enabled only for visible, nontransparent drawable matches.

Filters and themes use the same ViewerController implementation as the Qt sample.
Changes are session-only. Reload replaces the scene; cancellation keeps the previous
scene. Building heights are illustrative (9 m); the DEM vertical datum is unverified.

## Local development

The styling bridge is new and is not included in published SDK 1.5.32.
Build GeoKernel.Viewer3D first. In a fresh CMD, run these separately:





```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "LayerStylingAndFiltering.Wpf\LayerStylingAndFiltering.Wpf.csproj" -c Release && "LayerStylingAndFiltering.Wpf\bin\Release\net10.0-windows\LayerStylingAndFiltering.Wpf.exe"
```

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.