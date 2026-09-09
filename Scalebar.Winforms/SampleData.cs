using System.IO.Compression;

namespace GeoKernel.Scalebar.Winforms;

internal sealed record SampleDataProgress(string Message, int? Percentage);

internal static class SampleData
{
    private static readonly Uri SourceUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/world_4326.zip");

    public static async Task<string> EnsureWorldLayerAsync(IWin32Window owner, IProgress<SampleDataProgress>? progress = null)
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        var extractDirectory = Path.Combine(dataDirectory, "world_4326");
        var requiredPath = Path.Combine(extractDirectory, "world_4326.shp");
        var archivePath = Path.Combine(dataDirectory, "world_4326.zip");
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
                var totalLength = response.Content.Headers.ContentLength;
                var buffer = new byte[81920];
                long totalRead = 0;
                while (true)
                {
                    var bytesRead = await download.ReadAsync(buffer);
                    if (bytesRead == 0) break;
                    await archive.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalRead += bytesRead;
                    int? percentage = totalLength.HasValue ? (int)Math.Min(100, totalRead * 100 / totalLength.Value) : null;
                    progress?.Report(new SampleDataProgress(percentage.HasValue ? $"Downloading world data... {percentage}%" : "Downloading world data...", percentage));
                }
            }
            progress?.Report(new SampleDataProgress("Extracting world data...", null));
            Directory.CreateDirectory(extractDirectory);
            ZipFile.ExtractToDirectory(archivePath, extractDirectory, overwriteFiles: true);
            File.Delete(archivePath);
            if (!File.Exists(requiredPath)) throw new FileNotFoundException("The expected shapefile was not found after extraction.", requiredPath);
            return requiredPath;
        }
        catch (Exception exception)
        {
            TryDelete(archivePath);
            MessageBox.Show(owner, $"Sample data could not be prepared.{Environment.NewLine}{Environment.NewLine}{exception.Message}", "Scalebar", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.Empty;
        }
    }

    private static void TryDelete(string path)
    {
        try { if (File.Exists(path)) File.Delete(path); } catch { }
    }
}
