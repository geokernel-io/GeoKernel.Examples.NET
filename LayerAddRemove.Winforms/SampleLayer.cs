using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerAddRemove.Winforms;

internal sealed record SampleLayer(string Name, string ArchiveName, string FolderName, string FileName, GeoKernelLayerStyle Style)
{
    public Uri SourceUrl { get; } = new($"https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/{ArchiveName}");
}
