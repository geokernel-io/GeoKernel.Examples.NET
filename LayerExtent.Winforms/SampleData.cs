using System.IO.Compression;

namespace GeoKernel.LayerExtent.Winforms;

internal sealed record SampleDataProgress(string Message, int? Percentage);

internal static class SampleData
{
    private static readonly Uri SourceUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/california.zip");

    public static async Task<string> EnsureCaliforniaAsync(IWin32Window owner, IProgress<SampleDataProgress>? progress = null)
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        var extractDirectory = Path.Combine(dataDirectory, "california");
        var requiredPath = Path.Combine(extractDirectory, "california.shp");
        var archivePath = Path.Combine(dataDirectory, "california.zip");
        if (File.Exists(requiredPath)) return requiredPath;
        try
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
                    progress?.Report(new SampleDataProgress(percentage.HasValue ? $"Downloading California... {percentage}%" : "Downloading California...", percentage));
                }
            }
            progress?.Report(new SampleDataProgress("Extracting California...", null));
            Directory.CreateDirectory(extractDirectory);
            ZipFile.ExtractToDirectory(archivePath, extractDirectory, true);
            File.Delete(archivePath);
            if (!File.Exists(requiredPath)) throw new FileNotFoundException("The expected California shapefile was not found after extraction.", requiredPath);
            return requiredPath;
        }
        catch (Exception exception)
        {
            TryDelete(archivePath);
            MessageBox.Show(owner, $"Sample data could not be prepared.{Environment.NewLine}{Environment.NewLine}{exception.Message}", "LayerExtent", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.Empty;
        }
    }

    private static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch { } }
}
