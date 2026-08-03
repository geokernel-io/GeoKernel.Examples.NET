using System.IO.Compression;

namespace GeoKernel.HelloMap.Winforms;

internal sealed record SampleDataProgress(string Message, int? Percentage);

internal static class SampleData
{
    private static readonly Uri WorldLayerUrl = new(
        "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/world_4326.zip");

    public static async Task<string> EnsureWorldLayerAsync(
        IWin32Window owner,
        IProgress<SampleDataProgress>? progress = null)
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        var layerDirectory = Path.Combine(dataDirectory, "world_4326");
        var layerPath = Path.Combine(layerDirectory, "world_4326.shp");
        var archivePath = Path.Combine(dataDirectory, "world_4326.zip");

        if (File.Exists(layerPath))
            return layerPath;

        try
        {
            Directory.CreateDirectory(dataDirectory);
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(
                WorldLayerUrl,
                HttpCompletionOption.ResponseHeadersRead);
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
                    if (bytesRead == 0)
                        break;

                    await archive.WriteAsync(buffer.AsMemory(0, bytesRead));
                    totalRead += bytesRead;

                    int? percentage = totalLength.HasValue
                        ? (int)Math.Min(100, totalRead * 100 / totalLength.Value)
                        : null;
                    progress?.Report(new SampleDataProgress(
                        percentage.HasValue ? $"Downloading sample data... {percentage}%" : "Downloading sample data...",
                        percentage));
                }
            }

            progress?.Report(new SampleDataProgress("Extracting sample data...", null));
            Directory.CreateDirectory(layerDirectory);
            ZipFile.ExtractToDirectory(archivePath, layerDirectory, overwriteFiles: true);
            File.Delete(archivePath);

            if (!File.Exists(layerPath))
                throw new FileNotFoundException("The expected sample layer was not found after extraction.", layerPath);

            return layerPath;
        }
        catch (Exception exception)
        {
            TryDelete(archivePath);
            MessageBox.Show(
                owner,
                $"Sample data could not be prepared.{Environment.NewLine}{Environment.NewLine}{exception.Message}",
                "HelloMap",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return string.Empty;
        }
    }

    private static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {
            // Best-effort cleanup after a failed download.
        }
    }
}
