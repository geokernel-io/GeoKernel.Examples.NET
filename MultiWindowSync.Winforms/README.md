# MultiWindowSync.Winforms

MultiWindowSync demonstrates how to keep multiple GeoKernel map viewers synchronized in one Windows Forms application.

## Overview

It opens Viewer A and Viewer B, loads the same world layer into both maps, and mirrors viewport changes so panning or zooming either viewer updates the other. Synchronization can be disabled at runtime.

## GIS Workflow

- Create multiple viewer instances
- Download and load shared sample data
- Synchronize visible map extents
- Enable or disable synchronization
- Apply the same navigation tools to both viewers

## Run

```powershell
dotnet run --project MultiWindowSync.Winforms.csproj
```

![MultiWindowSync example](Screenshot.png)
