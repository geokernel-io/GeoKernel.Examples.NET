using System.IO;
using System.Net.Http;
using System.IO.Compression;
using System.Security.Cryptography;

namespace GeoKernel.GlobeRasterServices.Wpf;

internal static class SampleData
{
    public static async Task<string> EnsureRasterAsync(string name, string checksum,
        IProgress<string> progress, CancellationToken cancellation)
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "data");
        string target = Path.Combine(folder, name + ".tif");
        if (File.Exists(target)) return target;
        Directory.CreateDirectory(folder);
        string archive = Path.Combine(folder, Guid.NewGuid() + ".zip");
        string partial = target + ".partial";
        try
        {
            progress.Report($"Downloading {name}…");
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
            using var response = await client.GetAsync(
                $"https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/{name}.zip",
                HttpCompletionOption.ResponseHeadersRead, cancellation);
            response.EnsureSuccessStatusCode();
            await using (var input = await response.Content.ReadAsStreamAsync(cancellation))
            await using (var output = File.Create(archive))
                await input.CopyToAsync(output, cancellation);
            await using (var input = File.OpenRead(archive))
            {
                string hash = Convert.ToHexString(await SHA256.HashDataAsync(input, cancellation));
                if (!hash.Equals(checksum, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException($"Checksum mismatch: {name}");
            }
            progress.Report($"Extracting {name}…");
            using var zip = ZipFile.OpenRead(archive);
            var entries = zip.Entries.Where(e => e.Name.Equals(name + ".tif", StringComparison.OrdinalIgnoreCase)).ToArray();
            if (entries.Length != 1) throw new InvalidDataException($"Expected one {name}.tif in the archive.");
            await using (var input = entries[0].Open())
            await using (var output = File.Create(partial))
                await input.CopyToAsync(output, cancellation);
            cancellation.ThrowIfCancellationRequested();
            File.Move(partial, target, true);
            return target;
        }
        finally
        {
            if (File.Exists(archive)) File.Delete(archive);
            if (File.Exists(partial)) File.Delete(partial);
        }
    }
}

