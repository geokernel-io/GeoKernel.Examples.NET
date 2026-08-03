using System.IO.Compression;

namespace GeoKernel.LayerZoomTo.Winforms;

internal sealed record SampleDataProgress(string Message, int? Percentage);
internal sealed record CityLayer(string Name, string Path, string FillColor);

internal static class SampleData
{
    private static readonly Uri SourceUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/california_cities.zip");
    private static readonly string[] Palette = ["#BFD6E5", "#C9D5C9", "#D8CDA7", "#D7B79B", "#D6C6E3", "#B9D8C5"];

    public static async Task<IReadOnlyList<CityLayer>> EnsureCityLayersAsync(IWin32Window owner, IProgress<SampleDataProgress>? progress = null)
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        var extractDirectory = Path.Combine(dataDirectory, "california_cities");
        var requiredPath = Path.Combine(extractDirectory, "alameda.shp");
        var archivePath = Path.Combine(dataDirectory, "california_cities.zip");
        try
        {
            if (!File.Exists(requiredPath))
            {
                Directory.CreateDirectory(dataDirectory);
                using var httpClient = new HttpClient();
                using var response = await httpClient.GetAsync(SourceUrl, HttpCompletionOption.ResponseHeadersRead);
                response.EnsureSuccessStatusCode();
                await using (var download = await response.Content.ReadAsStreamAsync())
                await using (var archive = File.Create(archivePath))
                {
                    var length = response.Content.Headers.ContentLength;
                    var buffer = new byte[81920];
                    long total = 0;
                    while (true)
                    {
                        var count = await download.ReadAsync(buffer);
                        if (count == 0) break;
                        await archive.WriteAsync(buffer.AsMemory(0, count));
                        total += count;
                        int? percentage = length.HasValue ? (int)Math.Min(100, total * 100 / length.Value) : null;
                        progress?.Report(new SampleDataProgress(percentage.HasValue ? $"Downloading California cities... {percentage}%" : "Downloading California cities...", percentage));
                    }
                }
                progress?.Report(new SampleDataProgress("Extracting California cities...", null));
                Directory.CreateDirectory(extractDirectory);
                ZipFile.ExtractToDirectory(archivePath, extractDirectory, true);
                File.Delete(archivePath);
            }

            if (!File.Exists(requiredPath)) throw new FileNotFoundException("The expected California city layer was not found after extraction.", requiredPath);
            return Directory.EnumerateFiles(extractDirectory, "*.shp")
                .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
                .Select((path, index) => new CityLayer(DisplayName(path), path, Palette[index % Palette.Length]))
                .ToArray();
        }
        catch (Exception exception)
        {
            TryDelete(archivePath);
            MessageBox.Show(owner, $"Sample data could not be prepared.{Environment.NewLine}{Environment.NewLine}{exception.Message}", "LayerZoomTo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return [];
        }
    }

    private static string DisplayName(string path)
    {
        var words = Path.GetFileNameWithoutExtension(path).Replace('_', ' ').Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return string.Join(" ", words.Select(word => char.ToUpperInvariant(word[0]) + word[1..]));
    }

    private static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch { } }
}
