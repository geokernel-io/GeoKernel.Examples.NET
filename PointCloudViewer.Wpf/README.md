# PointCloudViewer — WPF

Automatically downloads and opens the EuroSDR P4 40 × 40 m LAZ sample using the same native reader and preview renderer as the Qt and WinForms examples.

Choose RGB or height colours, apply a 50,000 / 100,000 / 250,000 point sample limit, cancel loading or reset the camera. The panel reports source and display counts, source Z range and RGB availability. Cancellation or read failure retains the previous cloud.

The preview is locally normalized; the source remains EPSG:32630 with provisional heights. The original data is not modified. Point size is fixed at one pixel.

Uses the published GeoKernel 1.5.32 package. No local SDK build or DLL override is required.

From CMD:

```cmd
cd /d "D:\projects\GeoKernel.Examples.NET" && dotnet build "PointCloudViewer.Wpf\PointCloudViewer.Wpf.csproj" -c Release && "PointCloudViewer.Wpf\bin\Release\net10.0-windows\PointCloudViewer.Wpf.exe"
```

Downloaded data is cached under the application's `data` directory. WPF creates the native viewer after layout, pumps it on the UI dispatcher, and cancels loading when the window closes.
