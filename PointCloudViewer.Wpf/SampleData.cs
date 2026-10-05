using System.IO;
using System.Net.Http;
using System.IO.Compression;

namespace GeoKernel.PointCloudViewer.Wpf;

internal static class SampleData
{
    public static async Task<string> EnsureArchiveAsync(string name, string url, string required, IProgress<string> progress, CancellationToken cancellation)
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "data", name);
        string marker = Path.Combine(folder, ".complete");
        if (File.Exists(marker))
        {
            string? cached = Directory.EnumerateFiles(folder, required, SearchOption.AllDirectories).SingleOrDefault();
            if (cached != null) return cached;
        }
        Directory.CreateDirectory(folder);
        string temporary = Path.Combine(folder, Guid.NewGuid() + ".zip");
        try
        {
            progress.Report($"Downloading {name}...");
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(30) };
            using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellation);
            response.EnsureSuccessStatusCode();
            await using (var input = await response.Content.ReadAsStreamAsync(cancellation))
            await using (var output = File.Create(temporary))
                await input.CopyToAsync(output, cancellation);
            using var zip = ZipFile.OpenRead(temporary);
            string root = Path.GetFullPath(folder) + Path.DirectorySeparatorChar;
            foreach (var entry in zip.Entries)
            {
                cancellation.ThrowIfCancellationRequested();
                string target = Path.GetFullPath(Path.Combine(folder, entry.FullName));
                if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Unsafe archive entry.");
                if (entry.Name.Length == 0) { Directory.CreateDirectory(target); continue; }
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                progress.Report($"Extracting {name}: {entry.Name}");
                await using var input = entry.Open();
                await using var output = File.Create(target);
                await input.CopyToAsync(output, cancellation);
            }
            var matches = Directory.GetFiles(folder, required, SearchOption.AllDirectories);
            if (matches.Length != 1) throw new InvalidDataException($"Expected one {required}.");
            await File.WriteAllTextAsync(marker, "complete", cancellation);
            return matches[0];
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

}
