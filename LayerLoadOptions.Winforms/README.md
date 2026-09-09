# LayerLoadOptions.Winforms

LayerLoadOptions demonstrates how to control vector layer loading behavior with GeoKernel layer load options.

## Overview

It prepares the USA states sample data and lets the user load it without a spatial index or with an RTree index. The example reports load and index preparation progress and includes a feature query benchmark.

## GIS Workflow

- Initialize a map viewer
- Download and load vector sample data
- Configure layer loading options
- Enable or disable spatial indexing
- Benchmark spatial query performance

## Run

```powershell
dotnet run --project LayerLoadOptions.Winforms.csproj
```

![LayerLoadOptions example](Screenshot.png)
