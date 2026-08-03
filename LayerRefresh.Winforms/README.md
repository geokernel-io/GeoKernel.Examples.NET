# LayerRefresh.Winforms

LayerRefresh demonstrates how to refresh a map layer after changing its style at runtime in a GeoKernel Windows Forms viewer.

## Overview

It prepares the California sample data, loads the boundary layer, and provides toolbar actions to change fill color, outline color, and fill opacity. The refresh action applies the pending style and redraws the viewer.

## GIS Workflow

- Initialize a map viewer
- Download and load vector sample data
- Change layer style properties
- Refresh the map after style changes
- Inspect visual updates interactively

## Run

```powershell
dotnet run --project LayerRefresh.Winforms.csproj
```

![LayerRefresh example](Screenshot.png)
