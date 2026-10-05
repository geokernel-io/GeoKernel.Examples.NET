# ProjectSaveLoad — WinForms

Automatically downloads and loads Sagrada Família terrain, orthophoto, roads and
buildings. Includes the layer styling/filtering controls, Open, Save and Save as.
Ctrl+S saves; Ctrl+Shift+S opens Save as.

Uses the SDK's existing readProject/writeProject implementation and .gk3d format,
shared with the Qt sample. Persists source paths, terrain resolution, building
height, scene/layer IDs, camera, vertical exaggeration, visibility, opacity,
uniform colors, attribute themes and filters. Pending filter text is applied
before saving.

Projects reference external local files; they do not embed the datasets.
Keep source files/cache accessible. Open/reload replaces the scene, so save
changes first. Cancelled or failed preparation retains the previous scene.
Advanced model, point-cloud, 3D Tiles, globe, analysis, time-series and orthometric
geoid-conversion projects are outside this example's scope and are rejected.

Building heights are illustrative (9 m). DEM vertical datum is unverified.

Uses the published GeoKernel 1.5.32 NuGet package.

```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "ProjectSaveLoad.Winforms\ProjectSaveLoad.Winforms.csproj" -c Release && "ProjectSaveLoad.Winforms\bin\Release\net10.0-windows\ProjectSaveLoad.Winforms.exe"
```
