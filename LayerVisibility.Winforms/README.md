# LayerVisibility.Winforms

LayerVisibility demonstrates how to control individual map layer visibility in a GeoKernel Windows Forms viewer.

## Overview

It downloads and loads world boundaries, USA states, and USA city points, displays their visibility state in a side panel, and lets the selected layer be hidden or shown at runtime.

## GIS Workflow

- Initialize a map viewer
- Download and load multiple vector layers
- Display layer visibility state
- Hide and show individual layers
- Refresh the map after visibility changes

## Run

```powershell
dotnet run --project LayerVisibility.Winforms.csproj
```

![LayerVisibility example](Screenshot.png)
