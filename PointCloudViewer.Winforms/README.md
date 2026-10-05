# PointCloudViewer — WinForms

Downloads and opens the EuroSDR P4 40 × 40 m LAZ sample automatically. Uses the same native LAS/LAZ reader and model-preview renderer as the Qt PointCloudViewer example.

- RGB or height colours.
- Display sample limits: 50,000, 100,000 and 250,000 points.
- Cancel loading, retain the previous cloud on cancellation/read failure, and reset the camera.
- Source count, sampled count, source Z range and RGB availability.

The preview is locally normalized. Source coordinates remain EPSG:32630 and heights are provisional. The source file is not modified. Point size is fixed at one pixel by the SDK.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.

```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "PointCloudViewer.Winforms\PointCloudViewer.Winforms.csproj" -c Release && "PointCloudViewer.Winforms\bin\Release\net10.0-windows\PointCloudViewer.Winforms.exe"
```

Local mode uses the checkout's WinForms control and Viewer3D DLL with the published 1.5.32 dependencies. The default package version is 1.5.32 for the planned release. Data is cached under the application's `data` directory.
