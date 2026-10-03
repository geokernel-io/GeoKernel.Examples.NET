using System.IO;
using System.Net.Http;
using System.IO.Compression;

namespace GeoKernel.TerrainLoading.Wpf;

internal static class SampleData
{
    public static async Task<string> EnsureTerrainAsync(IProgress<string> progress, CancellationToken cancellation)
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "data", "sagrada_familia_terrain");
        string target = Path.Combine(folder, "sagrada_familia_terrain.tif");
        if (File.Exists(target)) return target;
        Directory.CreateDirectory(folder);
        string temporary = Path.Combine(folder, Guid.NewGuid() + ".zip");
        string partial = target + ".partial";
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
            using var response = await client.GetAsync(
                "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/sagrada_familia_terrain.zip",
                HttpCompletionOption.ResponseHeadersRead, cancellation);
            response.EnsureSuccessStatusCode();
            await using (var input = await response.Content.ReadAsStreamAsync(cancellation))
            await using (var output = File.Create(temporary))
            {
                var buffer = new byte[81920];
                long total = 0;
                int lastPercent = -1;
                progress.Report("Downloading terrain…");
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
                            progress.Report($"Downloading terrain… {percent}%");
                            lastPercent = percent;
                        }
                    }
                }
            }
            progress.Report("Extracting terrain…");
            using var zip = ZipFile.OpenRead(temporary);
            var entries = zip.Entries.Where(entry => entry.Name == "sagrada_familia_terrain.tif").ToArray();
            if (entries.Length != 1) throw new InvalidDataException("Archive must contain one sagrada_familia_terrain.tif.");
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
