using System.IO.Compression;

namespace GeoKernel.Project.Winforms;

internal sealed record SampleDataProgress(string Message, int? Percentage);

internal static class SampleData
{
    private static readonly Uri SourceUrl = new(
        "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/andalucia.zip");

    public static async Task<string> EnsureProjectAsync(
        IWin32Window owner,
        IProgress<SampleDataProgress>? progress = null)
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        var extractDirectory = Path.Combine(dataDirectory, "andalucia");
        var projectPath = Path.Combine(extractDirectory, "andalucia.geokernel");
        var archivePath = Path.Combine(dataDirectory, "andalucia.zip");

        if (File.Exists(projectPath))
            return projectPath;

        try
        {
            Directory.CreateDirectory(dataDirectory);
            using var httpClient = new HttpClient();
            using var response = await httpClient.GetAsync(
                SourceUrl,
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
                        percentage.HasValue
                            ? $"Downloading Andalucia project... {percentage}%"
                            : "Downloading Andalucia project...",
                        percentage));
                }
            }

            progress?.Report(new SampleDataProgress("Extracting Andalucia project...", null));
            Directory.CreateDirectory(extractDirectory);
            ZipFile.ExtractToDirectory(archivePath, extractDirectory, overwriteFiles: true);
            File.Delete(archivePath);

            if (!File.Exists(projectPath))
                throw new FileNotFoundException(
                    "The expected project file was not found after extraction.",
                    projectPath);

            return projectPath;
        }
        catch (Exception exception)
        {
            TryDelete(archivePath);
            MessageBox.Show(
                owner,
                $"Sample data could not be prepared.{Environment.NewLine}{Environment.NewLine}{exception.Message}",
                "Project",
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
