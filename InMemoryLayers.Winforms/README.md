# InMemoryLayers.Winforms

InMemoryLayers demonstrates how to create and update map layers entirely in memory with GeoKernel Windows Forms.

## Overview

It downloads a world reference layer and creates in-memory point, polyline, and polygon layers for cities, routes, and regions. Toolbar actions add features, reset memory layers, and return to full extent.

## GIS Workflow

- Initialize a map viewer
- Download and load reference vector data
- Create point, line, and polygon layers in memory
- Add features dynamically at runtime
- Refresh the map after memory-layer changes

## Run

```powershell
dotnet run --project InMemoryLayers.Winforms.csproj
```

![InMemoryLayers example](Screenshot.png)
