using System.IO.Compression;

namespace GeoKernel.LayerLoadCancel.Winforms;

internal sealed record SampleDataProgress(string Message, int? Percentage);

internal static class SampleData
{
    private static readonly Uri SourceUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/output_1m_points.zip");

    public static async Task<string> EnsureLargeLayerAsync(IWin32Window owner, IProgress<SampleDataProgress>? progress = null)
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        var extractDirectory = Path.Combine(dataDirectory, "output_1m_points");
        var requiredPath = Path.Combine(extractDirectory, "output_1m_points.shp");
        var archivePath = Path.Combine(dataDirectory, "output_1m_points.zip");
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
                    progress?.Report(new SampleDataProgress(percentage.HasValue ? $"Downloading one-million-point data... {percentage}%" : "Downloading one-million-point data...", percentage));
                }
            }
            progress?.Report(new SampleDataProgress("Extracting one-million-point data...", null));
            Directory.CreateDirectory(extractDirectory);
            ZipFile.ExtractToDirectory(archivePath, extractDirectory, true);
            File.Delete(archivePath);
            if (!File.Exists(requiredPath)) throw new FileNotFoundException("The expected point shapefile was not found after extraction.", requiredPath);
            return requiredPath;
        }
        catch (Exception exception)
        {
            TryDelete(archivePath);
            MessageBox.Show(owner, $"Sample data could not be prepared.{Environment.NewLine}{Environment.NewLine}{exception.Message}", "LayerLoadCancel", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.Empty;
        }
    }

    private static void TryDelete(string path) { try { if (File.Exists(path)) File.Delete(path); } catch { } }
}
