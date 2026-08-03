# LayerZoomTo.Winforms

LayerZoomTo demonstrates how to zoom the map view to a selected layer in a GeoKernel Windows Forms viewer.

## Overview

It prepares the California sample data, loads multiple city/county boundary layers, and lists them in a combo box. Selecting a layer zooms directly to its extent, while the default option returns to full extent.

## GIS Workflow

- Initialize a map viewer
- Download and load multiple vector layers
- Populate a layer selection control
- Find a layer by name
- Zoom to a specific layer extent

## Run

```powershell
dotnet run --project LayerZoomTo.Winforms.csproj
```

![LayerZoomTo example](Screenshot.png)
