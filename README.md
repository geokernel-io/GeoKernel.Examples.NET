# GeoKernel .NET Examples

Official C# examples for the GeoKernel native GIS SDK, including WPF and Windows Forms applications.

## Requirements

- Windows 10 or later
- A supported .NET SDK
- Visual Studio
- A valid GeoKernel license

## Getting Started

Choose a WPF or Windows Forms example directory, open its project file, restore dependencies and run the application.

## Resources

- [GeoKernel Website](https://geokernel.io)
- [Documentation](https://docs.geokernel.io)
- [Sample Data](https://github.com/geokernel-io/GeoKernel.SampleData)

## Support

For questions, contact [admin@geokernel.io](mailto:admin@geokernel.io).

### LayerStylingAndFiltering (WinForms)

[LayerStylingAndFiltering.Winforms](LayerStylingAndFiltering.Winforms/README.md)
automatically loads terrain, orthophoto, roads and buildings. Supports per-layer
visibility, opacity, uniform/source colors, attribute coloring, substring filtering,
matching counts and zoom to matches. Requires the new local Viewer3D styling bridge
until the next package release. Build/run commands are in the example README.

### LayerStylingAndFiltering (WPF)

[LayerStylingAndFiltering.Wpf](LayerStylingAndFiltering.Wpf/README.md) provides the
same per-layer styling, attribute coloring and substring filtering as the WinForms
and Qt examples, with automatic sample downloads. Uses the local Viewer3D styling
bridge until the next SDK package release. See the example README for commands.

### ProjectSaveLoad (WinForms)

[ProjectSaveLoad.Winforms](ProjectSaveLoad.Winforms/README.md) loads the sample
automatically and saves/opens local .gk3d projects using the same format as Qt.
Persists camera, source references, height scale, layer styling and filters.
Requires the new local Viewer3D project bridge until the next package release.

### ProjectSaveLoad (WPF)

[ProjectSaveLoad.Wpf](ProjectSaveLoad.Wpf/README.md) loads the sample automatically
and opens/saves the same .gk3d format as Qt and WinForms. Includes layer styling,
filters, camera and height-scale restoration. Use the local project bridge until
the next SDK package release; build/run commands are in the example README.
