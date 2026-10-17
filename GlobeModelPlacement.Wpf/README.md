# GlobeModelPlacement — WPF

```powershell
cd D:\projects\GeoKernel.Examples.NET\GlobeModelPlacement.Wpf
dotnet run -c Release
```

Uses published NuGet GeoKernel.NET 1.5.35. First launch automatically downloads
the real Sagrada Familia glTF model, verifies the archive SHA-256 checksum and
extracts its buffers/textures into the output directory's `data` folder. Later
launches reuse the completed download. Requires Windows x64, .NET 10, a Vulkan
capable graphics driver, a valid license and internet for downloads/Esri imagery.

Matches the Qt example: Esri satellite imagery, atmosphere, no DEM, approximate
placement at longitude 2.1744 / latitude 41.4036 and ellipsoidal anchor height
14.089 m. Model credit: PeeJaa. The starting camera looks toward the model.

Controls: Zoom to model, Rotate 30 degrees, Scale (0.01–10), Anchor height
(-100–10000 m), Remove, Restore, Atmosphere and Esri. Remove removes the scene
layer; Restore reloads the cached model using its last accepted placement.
Model loading and placement are asynchronous. Controls remain disabled until
GPU upload completes. Includes the application icon, fixed 24-DIP status bar,
no console window and no application test modes or command-line parameters.
Closing cancels pending work and releases native resources.

Known SDK 1.5.35 limitation: this model at heading 30 degrees and scale 1.5 is
rejected with `Model is too small for scene precision.` Scale 2 succeeds. The
example displays the SDK error and restores the last accepted control values;
the existing model stays visible. Fixing this native precision limitation needs
a separate SDK change/release; it is not hidden by modifying the sample model.


Enter scale and height using your regional decimal separator, then click Apply. Invalid values are rejected before submitting a job.
