# SimilaritySearch — WPF

Pick a solar park, a lake, a town centre or a field (or click a place, or drag a box) and every
80 m cell of Flevoland is ranked by how much it looks like it, using AlphaEarth satellite
embeddings (64 values per cell). WPF port of the Qt `SimilaritySearch` example; the
workflow (sample data, native search, presets, framing the result) is shared with the
[Windows Forms version](../SimilaritySearch.Winforms/README.md) in
[`SimilaritySession.cs`](../Common/Similarity/SimilaritySession.cs), on top of GeoKernel.NET's
`SimilaritySearch` (C ABI `GeoKernelSimilarity_*`).

## Requirements and build

Requires Windows x64, .NET SDK 10 and a valid GeoKernel license. The find-similar API ships after
GeoKernel.NET **1.5.40**; until the next release, build against the GeoKernel.NET sources and put
a native GeoKernel runtime with the API on `PATH`:

```powershell
$env:Path = "D:\projects\GeoKernel\outputs\build\Release;" + $env:Path
dotnet run -c Release --project SimilaritySearch.Wpf -p:GeoKernelNetSource=D:/projects/GeoKernel.NET
```

After the release, use the NuGet package instead: `-p:GeoKernelSdkVersion=<version>`. Without
either property the build stops with an explanation.

At startup the example downloads `flevoland_alphaearth_2024_80m_v1.zip` from
GeoKernel.SampleData v1 (31 MB, SHA-256 checked, cached and shared with the other GeoAI
examples). `--smoke` searches the solar park, prints the result and closes.

## Workflow

1. The map shows the embedding grid as muted false colour (its first three dimensions).
2. **Look for places like…**: **Solar park** searches the mean embedding of Zonnepark Zuyderzon
   (Almere); **Open water**, **Town centre** and **Farmland** search one cell. The view frames
   the query and its ten best matches.
3. **Find similar on the map**: click a place (its cell) or drag a box (the mean of its cells);
   the list follows these interactive searches (`SimilaritySearch.Revision`).
4. The slider sets the heat-map threshold; select a match to zoom to it.

The AlphaEarth Foundations Satellite Embedding dataset is produced by Google and Google DeepMind
(CC BY 4.0).
