# BuildingChangeReview — WinForms

Two synchronized maps compare Oosterwold in 2016 and 2025. Run building change
analysis, review/edit candidates and export approved GeoJSON. The sidebar keeps
settings collapsed and gives the candidate table the remaining space.

Requires Windows x64, .NET SDK 10, a graphics driver supported by GeoKernel and
a valid GeoKernel license. The published GeoKernel.NET **1.5.39** NuGet package,
native runtime, sample imagery and model are restored/downloaded automatically.

From this directory:

```powershell
dotnet run -c Release
```

From the repository root:

```powershell
dotnet run --project BuildingChangeReview.Winforms -c Release
```

See [workflow, filtering, data licenses and implementation differences from Qt](../Common/BuildingChange/README.md).
The WinForms and WPF examples compile the same analysis/session sources.
