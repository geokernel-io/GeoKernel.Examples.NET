# Minimap.Winforms

Minimap demonstrates how to add an overview map control to a GeoKernel Windows Forms viewer.

## Overview

It prepares the required sample data, loads a world boundaries shapefile, and displays a minimap in the top-right corner. The minimap helps users understand the current viewport position within the larger map extent while they pan and zoom.

## GIS Workflow

- Initialize a map viewer
- Download and prepare vector sample data
- Load world boundaries
- Add an overview/minimap control
- Keep spatial context visible while navigating the map

## Run

```powershell
dotnet run --project Minimap.Winforms.csproj
```

![Minimap example](Screenshot.png)
