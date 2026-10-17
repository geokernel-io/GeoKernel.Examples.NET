using System.IO;
using System.Net.Http;
using System.IO.Compression;
using System.Security.Cryptography;

namespace GeoKernel.GlobeModelPlacement.Wpf;

internal static class SampleData
{
    public static async Task<string> EnsureModelAsync(IProgress<string> progress, CancellationToken cancellation)
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "data", "sagrada_familia_3d_model");
        string marker = Path.Combine(folder, ".complete");
        string model = Path.Combine(folder, "model.gltf");
        if (File.Exists(marker) && File.Exists(model)) return model;
        Directory.CreateDirectory(folder);
        string temporary = Path.Combine(folder, Guid.NewGuid() + ".zip");
        try
        {
            progress.Report("Downloading Sagrada Familia model…");
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
            using var response = await client.GetAsync(
                "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/sagrada_familia_3d_model.zip",
                HttpCompletionOption.ResponseHeadersRead, cancellation);
            response.EnsureSuccessStatusCode();
            await using (var input = await response.Content.ReadAsStreamAsync(cancellation))
            await using (var output = File.Create(temporary))
                await input.CopyToAsync(output, cancellation);
            await using (var input = File.OpenRead(temporary))
            {
                string hash = Convert.ToHexString(await SHA256.HashDataAsync(input, cancellation));
                if (!hash.Equals("d7f031059b2dc3fb22ce8d25dd3d7bd3d71b5f7675489c818309a78a068dd956", StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("The model archive checksum does not match.");
            }
            progress.Report("Extracting Sagrada Familia model…");
            using var zip = ZipFile.OpenRead(temporary);
            string root = Path.GetFullPath(folder) + Path.DirectorySeparatorChar;
            foreach (var entry in zip.Entries)
            {
                cancellation.ThrowIfCancellationRequested();
                string target = Path.GetFullPath(Path.Combine(folder, entry.FullName));
                if (!target.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                    throw new InvalidDataException("Invalid path in the model archive.");
                if (string.IsNullOrEmpty(entry.Name)) { Directory.CreateDirectory(target); continue; }
                Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                await using var input = entry.Open();
                await using var output = File.Create(target);
                await input.CopyToAsync(output, cancellation);
            }
            if (!File.Exists(model)) throw new InvalidDataException("model.gltf is missing from the archive.");
            await File.WriteAllTextAsync(marker, "1", cancellation);
            return model;
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}

