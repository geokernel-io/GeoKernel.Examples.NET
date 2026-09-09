# LayerLoadCancel.Winforms

LayerLoadCancel demonstrates how to cancel a long-running layer load operation in a GeoKernel Windows Forms viewer.

## Overview

It prepares a one-million-point dataset, loads it with RTree spatial-index preparation enabled, and lets the user request cancellation while processing is still active. Progress and status messages report the current loading stage.

## GIS Workflow

- Initialize a map viewer
- Download and load a large vector dataset
- Configure cancellable layer loading
- Report loading and spatial-index progress
- Cancel an in-progress layer load

## Run

```powershell
dotnet run --project LayerLoadCancel.Winforms.csproj
```

![LayerLoadCancel example](Screenshot.png)
