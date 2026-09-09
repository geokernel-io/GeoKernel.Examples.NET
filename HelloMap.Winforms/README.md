# HelloMap

HelloMap is the first GeoKernel example and demonstrates the basic workflow for displaying a map layer with Windows Forms.

## Overview

It prepares the required sample data, loads a world boundaries shapefile, and zooms the view to the full map extent.

## GIS Workflow

This example shows the minimum GIS workflow needed to start working with GeoKernel:

- Initialize a map viewer
- Load vector data
- Zoom to the full map extent
- Present the result as an interactive world map

## Requirements

- Windows x64
- .NET 10 SDK

Open `HelloMap.Winforms.slnx` in Visual Studio or run:

```powershell
dotnet run --project HelloMap.Winforms.csproj
```
