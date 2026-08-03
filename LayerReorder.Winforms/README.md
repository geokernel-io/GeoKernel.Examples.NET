# LayerReorder.Winforms

LayerReorder demonstrates how to change the drawing order of layers in a GeoKernel Windows Forms viewer.

## Overview

It prepares the required sample data, loads world boundaries, USA states, and USA city point layers, and displays the current layer stack in a side panel. The selected layer can be moved up or down to control drawing order.

## GIS Workflow

- Initialize a map viewer
- Download and load multiple vector layers
- Display the current layer order
- Move layers up or down
- Refresh the map after layer order changes

## Run

```powershell
dotnet run --project LayerReorder.Winforms.csproj
```

![LayerReorder example](Screenshot.png)
