# LayerAddRemove.Winforms

LayerAddRemove demonstrates how to add and remove map layers at runtime in a GeoKernel Windows Forms viewer.

## Overview

It lets the user add world boundaries, USA states, and USA city point layers from toolbar actions, with required sample data downloaded automatically on first use. Layers can be removed individually or cleared all at once.

## GIS Workflow

- Initialize a map viewer
- Download and load vector sample data
- Add layers from user actions
- Remove individual layers
- Clear all map layers

## Run

```powershell
dotnet run --project LayerAddRemove.Winforms.csproj
```

![LayerAddRemove example](Screenshot.png)
