using System.IO;
using System.Net.Http;
using System.IO.Compression;

namespace GeoKernel.ModelPlacement.Wpf;

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

    public static async Task<string> EnsureAsync(string dataset, IProgress<string> progress, CancellationToken cancellation)
    {
        string sample = "sagrada_familia_" + dataset;
        string filename = sample + ".tif";
        string folder = Path.Combine(AppContext.BaseDirectory, "data", sample);
        string target = Path.Combine(folder, filename);
        if (File.Exists(target)) return target;
        Directory.CreateDirectory(folder);
        string temporary = Path.Combine(folder, Guid.NewGuid() + ".zip");
        string partial = target + ".partial";
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
            using var response = await client.GetAsync(
                $"https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/{sample}.zip",
                HttpCompletionOption.ResponseHeadersRead, cancellation);
            response.EnsureSuccessStatusCode();
            await using (var input = await response.Content.ReadAsStreamAsync(cancellation))
            await using (var output = File.Create(temporary))
            {
                var buffer = new byte[81920];
                long total = 0;
                int lastPercent = -1;
                progress.Report($"Downloading {dataset}...");
                int count;
                while ((count = await input.ReadAsync(buffer, cancellation)) != 0)
                {
                    await output.WriteAsync(buffer.AsMemory(0, count), cancellation);
                    total += count;
                    if (response.Content.Headers.ContentLength is > 0 and var length)
                    {
                        int percent = (int)(100 * total / length);
                        if (percent != lastPercent)
                        {
                            progress.Report($"Downloading {dataset}... {percent}%");
                            lastPercent = percent;
                        }
                    }
                }
            }
            progress.Report($"Extracting {dataset}...");
            using var zip = ZipFile.OpenRead(temporary);
            var entries = zip.Entries.Where(entry => entry.Name == filename).ToArray();
            if (entries.Length != 1) throw new InvalidDataException($"Archive must contain one {filename}.");
            await using (var source = entries[0].Open())
            await using (var output = File.Create(partial))
                await source.CopyToAsync(output, cancellation);
            cancellation.ThrowIfCancellationRequested();
            File.Move(partial, target, true);
            return target;
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            if (File.Exists(partial)) File.Delete(partial);
        }
    }
}
