using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Security.Cryptography;

namespace GeoKernel.Examples.GeoAi;

internal sealed record DownloadProgress(int Percent, string Message);

internal static class ModelAssets
{
    private static readonly HttpClient Client = new() { Timeout = Timeout.InfiniteTimeSpan };
    private static readonly SemaphoreSlim Gate = new(1, 1);

    internal static string CacheDirectory(string sha256)
    {
        string cache = Environment.GetEnvironmentVariable("GEOKERNEL_EXAMPLES_DATA_DIR") ??
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "GeoKernel", "Examples", "GeoAI");
        return Path.Combine(Path.GetFullPath(cache), sha256);
    }

    // Cache only immutable inputs. Predictions belong to the native job's temporary workspace.
    public static async Task<string> PrepareAsync(string url, string sha256, string requiredFile,
        IProgress<DownloadProgress> progress, CancellationToken cancellation, bool extractArchive = true)
    {
        await Gate.WaitAsync(cancellation);
        try
        {
            string root = CacheDirectory(sha256);
            Directory.CreateDirectory(root);
            string archive = Path.Combine(root, extractArchive ? "download.zip" : requiredFile);
            string directory = Path.Combine(root, "content");
            if (!File.Exists(archive) || !await HasHashAsync(archive, sha256, cancellation))
            {
                string temporary = Path.Combine(root, Guid.NewGuid() + ".part");
                try
                {
                    using var response = await Client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellation);
                    response.EnsureSuccessStatusCode();
                    await using (var source = await response.Content.ReadAsStreamAsync(cancellation))
                    await using (var destination = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write,
                        FileShare.None, 131072, FileOptions.Asynchronous))
                    {
                        byte[] buffer = new byte[131072];
                        long received = 0;
                        long? length = response.Content.Headers.ContentLength;
                        int count;
                        while ((count = await source.ReadAsync(buffer, cancellation)) != 0)
                        {
                            await destination.WriteAsync(buffer.AsMemory(0, count), cancellation);
                            received += count;
                            progress.Report(new(length is > 0 ? (int)(received * 100 / length.Value) : 0,
                                $"Downloading {Path.GetFileName(new Uri(url).LocalPath)} — {received / 1048576.0:F1} MB"));
                        }
                    }
                    if (!await HasHashAsync(temporary, sha256, cancellation))
                        throw new InvalidDataException("Download SHA-256 mismatch. Please retry.");
                    File.Move(temporary, archive, true);
                }
                finally
                {
                    if (File.Exists(temporary)) File.Delete(temporary);
                }
            }

            if (!extractArchive) return archive;

            if (!Directory.Exists(directory))
            {
                string staging = Path.Combine(root, "extract-" + Guid.NewGuid());
                try
                {
                    progress.Report(new(0, "Extracting verified sample package…"));
                    await Task.Run(() => ZipFile.ExtractToDirectory(archive, staging), cancellation);
                    cancellation.ThrowIfCancellationRequested();
                    try { Directory.Move(staging, directory); }
                    catch (IOException) when (Directory.Exists(directory)) { }
                }
                finally
                {
                    if (Directory.Exists(staging)) Directory.Delete(staging, true);
                }
            }
            string[] files = Directory.GetFiles(directory, requiredFile, SearchOption.AllDirectories);
            if (files.Length != 1)
                throw new InvalidDataException($"Expected one {requiredFile} in the sample package; found {files.Length}.");
            return files[0];
        }
        finally { Gate.Release(); }
    }

    private static async Task<bool> HasHashAsync(string file, string expected, CancellationToken cancellation)
    {
        await using var stream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read,
            131072, FileOptions.Asynchronous | FileOptions.SequentialScan);
        byte[] hash = await SHA256.HashDataAsync(stream, cancellation);
        return Convert.ToHexString(hash).Equals(expected, StringComparison.OrdinalIgnoreCase);
    }
}
