using System.IO.Compression;

namespace GeoKernel.UndoRedo.Winforms;

internal sealed record SampleDataProgress(string Message, int? Percentage);

internal sealed class ControlProgress<T>(Control control, Action<T> callback) : IProgress<T>
{
    public void Report(T value)
    {
        if (control.IsDisposed) return;
        if (control.InvokeRequired) control.Invoke(() => callback(value)); else callback(value);
    }
}

internal static class SampleData
{
    public static async Task<string> EnsureFileAsync(string archiveName, string folderName, string fileName, string displayName, IWin32Window owner, IProgress<SampleDataProgress>? progress = null)
    {
        var dataDirectory = Path.Combine(AppContext.BaseDirectory, "data");
        var extractDirectory = Path.Combine(dataDirectory, folderName);
        var requiredPath = Path.Combine(extractDirectory, fileName);
        var archivePath = Path.Combine(dataDirectory, archiveName);
        if (File.Exists(requiredPath)) return requiredPath;
        try
        {
            Directory.CreateDirectory(dataDirectory);
            using var client = new HttpClient();
            var url = new Uri($"https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/{archiveName}");
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            await using (var input = await response.Content.ReadAsStreamAsync())
            await using (var output = File.Create(archivePath))
            {
                var length = response.Content.Headers.ContentLength;
                var buffer = new byte[81920]; long total = 0;
                while (true)
                {
                    var count = await input.ReadAsync(buffer); if (count == 0) break;
                    await output.WriteAsync(buffer.AsMemory(0, count)); total += count;
                    int? percent = length.HasValue ? (int)Math.Min(100, total * 100 / length.Value) : null;
                    progress?.Report(new SampleDataProgress(percent.HasValue ? $"Downloading {displayName}... {percent}%" : $"Downloading {displayName}...", percent));
                }
            }
            progress?.Report(new SampleDataProgress($"Extracting {displayName}...", null));
            Directory.CreateDirectory(extractDirectory);
            ZipFile.ExtractToDirectory(archivePath, extractDirectory, true);
            File.Delete(archivePath);
            if (!File.Exists(requiredPath)) throw new FileNotFoundException("Expected sample file was not found after extraction.", requiredPath);
            return requiredPath;
        }
        catch (Exception ex)
        {
            try { if (File.Exists(archivePath)) File.Delete(archivePath); } catch { }
            MessageBox.Show(owner, $"Sample data could not be prepared.{Environment.NewLine}{Environment.NewLine}{ex.Message}", "UndoRedo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return string.Empty;
        }
    }
}
