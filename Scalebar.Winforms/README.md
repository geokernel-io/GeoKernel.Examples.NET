# Scalebar.Winforms

Scalebar demonstrates how to add a dynamic map scale indicator to a GeoKernel Windows Forms viewer.

## Overview

It prepares the required sample data, loads a world boundaries shapefile, enables the scale bar, and positions it in the bottom-right corner. The scale bar updates as the user zooms or pans.

## GIS Workflow

- Initialize a map viewer
- Download and prepare vector sample data
- Load world boundaries
- Add a map overlay control
- Display an interactive scale reference

## Run

```powershell
dotnet run --project Scalebar.Winforms.csproj
```

![Scalebar example](Screenshot.png)
