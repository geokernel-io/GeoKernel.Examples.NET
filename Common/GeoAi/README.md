# Shared GeoAI sample code

Used by all seven new GeoAI WinForms/WPF example pairs, including SAM3 and SEN2SR.

- `GeoAiProfile.cs`: fixed URLs, SHA-256 hashes, input contracts and credits.
- `ModelAssets.cs`: asynchronous SHA-256 verified ZIP/raw TIFF download, staging extraction and shared input cache.
- `GeoAiSession.cs`: source-only startup, native background inference, polling, cancellation, explicit export and job ownership. SEN2SR restores its persistent source preview before releasing a previous job's comparison previews.
- `Sam3Session.cs`: specialized text/tracker bundles, source-only startup, draft acceptance gate and temporary result ownership.
- `Sam3Prompts.cs`, `Sam3Coordinates.cs`, `Sam3MapView.cs`: point/box selection state, WGS84-to-source CRS conversion and visible map prompt/result layers.
- `RuntimeSetup.cs`: app-local NuGet 1.5.39 runtime, or an explicit runtime override; process-local GDAL/PROJ/native dependency paths.
- `GeoAiRenderFence.cs`: asynchronously waits for native render completion after result removal, before releasing result files.
- `RasterPreview.cs`: worker-based source display preparation for Prithvi and PRUE, using the SDK-shipped GDAL. Preserves the spectral input and caches only the derived RGB display input.
- `GeoAiExample.props` / `.targets`: published GeoKernel.NET 1.5.39 package reference and obsolete local-hint cleanup.

The window supplies source/result display callbacks and removes the result layer before releasing a job.
Keep all display callbacks and `Changed` subscriptions on the owning UI thread. Await preparation/run/export
before disposing the session. Unsubscribe view updates during close before destroying controls.
Native model loading, inference, export and release run off the UI thread; viewer operations stay on it.

The native viewer prepends newly loaded layers. Prediction layers are therefore at index 0;
the source moves below them. Remove/style/blend the prediction at index 0, and await renderer idle before job release.

`GEOAI_WPF` selects the WPF SDK binding; otherwise the WinForms binding is used.
The example projects contain no command-line validation or smoke-test modes.

Vector results use a job-owned WGS84 GeoJSON display preview (`VectorPreview.cs`).
This avoids an empty/default-CRS GeoPackage provider view in the current native runtime.
The authoritative GeoPackage and exported Shapefile retain the source CRS and all attributes.
Preview conversion runs on a cancellable worker; its files are removed with the job after renderer cleanup.
`VectorOverlay.cs` applies opacity to fill and outline alpha explicitly, including backends that only apply layer opacity to raster images.
