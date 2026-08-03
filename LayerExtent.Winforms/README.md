# LayerExtent.Winforms

LayerExtent demonstrates how to read and visualize a layer's geographic extent in a GeoKernel Windows Forms viewer.

## Overview

It prepares the California sample data, loads the boundary layer, calculates its extent, and draws a polygon rectangle around that extent. This demonstrates how layer bounds can create helper geometry or visual diagnostics.

## GIS Workflow

- Initialize a map viewer
- Download and load vector sample data
- Read a layer extent
- Create geometry from extent coordinates
- Add an extent rectangle overlay

## Run

```powershell
dotnet run --project LayerExtent.Winforms.csproj
```

![LayerExtent example](Screenshot.png)
