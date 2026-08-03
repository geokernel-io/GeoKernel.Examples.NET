# Project.Winforms

Project demonstrates how to load a complete GeoKernel project file with its saved layers, styles, view extent, and rendering settings in a Windows Forms application.

## Overview

The example downloads and prepares the required Andalucia sample project data, opens `andalucia.geokernel`, reports download, extraction, and project loading progress, and then presents the restored map in an interactive viewer.

## GIS Workflow

- Download and prepare project sample data
- Open a saved `.geokernel` project
- Restore layers and symbology
- Track project loading progress
- Present the restored map in an interactive viewer

## Run

```powershell
dotnet run --project Project.Winforms.csproj
```

![Project example](Screenshot.png)
