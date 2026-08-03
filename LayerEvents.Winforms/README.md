# LayerEvents.Winforms

LayerEvents demonstrates how to observe and log layer-related events in a GeoKernel Windows Forms viewer.

## Overview

It downloads and loads world boundaries, USA states, and USA city points, then records layer lifecycle events. The side panel can add, remove, hide, reorder, and refresh layers while displaying a timestamped event log.

## GIS Workflow

- Initialize a map viewer
- Download and load multiple vector layers
- Connect to layer events
- Log layer add/remove/visibility/order changes
- Interact with the layer stack while observing events

## Run

```powershell
dotnet run --project LayerEvents.Winforms.csproj
```

![LayerEvents example](Screenshot.png)
