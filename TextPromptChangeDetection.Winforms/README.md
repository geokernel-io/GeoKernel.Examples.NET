# TextPromptChangeDetection — Windows Forms

Type what to look for ("building", "greenhouse", "solar panel") and see where it appeared and
disappeared between dates, year by year, with a review queue before anything is saved. SAM 3
segments the concept from text on every date and GeoKernel tracks the objects from date to
date. Windows Forms port of the Qt `TextPromptChangeDetection` example; the workflow (sample
data, SAM 3 package, native task, timeline frames, review) is shared with the
[WPF version](../TextPromptChangeDetection.Wpf/README.md) in
[`ChangeSession.cs`](../Common/TextPromptChange/ChangeSession.cs), on top of GeoKernel.NET's
`ChangeTask` (C ABI `GeoKernelChange_*`).

## Requirements and build

Requires Windows x64, .NET SDK 10 and a valid GeoKernel license. The change task API ships after
GeoKernel.NET **1.5.40**; until the next release, build against the GeoKernel.NET sources and
put a native GeoKernel runtime with the change API on `PATH`:

```powershell
$env:Path = "D:\projects\GeoKernel\outputs\build\Release;" + $env:Path
dotnet run -c Release --project TextPromptChangeDetection.Winforms -p:GeoKernelNetSource=D:/projects/GeoKernel.NET
```

After the release, use the NuGet package instead: `-p:GeoKernelSdkVersion=<version>`. Without
either property the build stops with an explanation.

At startup the example downloads the 9-date Almere / Oosterwold series (56 MB, SHA-256 checked);
the first **Find changes** downloads the SAM 3 text package (3.1 GB). Set
`GEOKERNEL_SAM3_TEXT_PACKAGE` to an installed package folder to skip that. Downloads are cached in
`%LOCALAPPDATA%\GeoKernel\Examples\GeoAI` and shared with the other GeoAI examples.
`TextPromptChangeDetection.Winforms.exe --smoke` runs the demo area, prints the result and exits.

## Workflow

1. Enter what to look for and a score threshold; choose **Two dates** (2016 → 2025) or the
   **Series** (9 dates; the provider has no 2021 layer), the **Demo area** (320 m) or the full
   1 km² dataset, and a provider. **Find changes** runs as a native background task.
2. Move the **timeline** or **Play** it: objects present on the date in yellow, new on that date
   in red, gone since the previous date as blue outlines.
3. The grid lists the changes of the interval ending at the shown date. Changes scoring 0.75 or
   more are accepted automatically, 0.5–0.75 wait for review. Select a row to zoom to it; enter
   your name and **Approve**, **Reject** or **Approve all pending** (recorded in a hash-chained
   audit trail).
4. **Save accepted changes** writes the interval's auto-accepted and approved changes (GeoPackage
   or GeoJSON, EPSG:25831); **Save series GeoPackage** writes every object's lifespan and every
   interval's changes.

Method, measured accuracy and limits are described in the Qt example's README. Results are
predictions, not ground truth.

Imagery: Beeldmateriaal Nederland / PDOK, 2016–2025, CC BY 4.0. Model: Meta SAM 3 (SAM License).
