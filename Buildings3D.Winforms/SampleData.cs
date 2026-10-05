using System.IO.Compression;

namespace GeoKernel.Buildings3D.Winforms;

internal static class SampleData
{
    public static async Task<string> EnsureBuildingsAsync(IProgress<string> progress, CancellationToken cancellation)
    {
        string folder = Path.Combine(AppContext.BaseDirectory, "data", "sagrada_familia_buildings");
        string[] required = [".shp", ".shx", ".dbf", ".prj"];
        if (required.All(ext => File.Exists(Path.Combine(folder, "buildings" + ext))))
            return Path.Combine(folder, "buildings.shp");
        Directory.CreateDirectory(folder);
        string temporary = Path.Combine(folder, Guid.NewGuid() + ".zip");
        var partials = new List<(string Partial, string Target)>();
        try
        {
            progress.Report("Downloading buildings...");
            using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(15) };
            using var response = await client.GetAsync(
                "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/sagrada_familia_buildings.zip",
                HttpCompletionOption.ResponseHeadersRead, cancellation);
            response.EnsureSuccessStatusCode();
            await using (var input = await response.Content.ReadAsStreamAsync(cancellation))
            await using (var output = File.Create(temporary))
                await input.CopyToAsync(output, cancellation);
            using var zip = ZipFile.OpenRead(temporary);
            progress.Report("Extracting buildings...");
            foreach (string ext in required.Concat([".cpg"]))
            {
                var entries = zip.Entries.Where(e => e.Name.Equals("buildings" + ext, StringComparison.OrdinalIgnoreCase)).ToArray();
                if (entries.Length == 0 && ext == ".cpg") continue;
                if (entries.Length != 1) throw new InvalidDataException($"Expected one buildings{ext} in the archive.");
                string target = Path.Combine(folder, "buildings" + ext);
                string partial = target + ".partial";
                partials.Add((partial, target));
                await using var input = entries[0].Open();
                await using var output = File.Create(partial);
                await input.CopyToAsync(output, cancellation);
            }
            cancellation.ThrowIfCancellationRequested();
            foreach (var item in partials) File.Move(item.Partial, item.Target, true);
            return Path.Combine(folder, "buildings.shp");
        }
        finally
        {
            if (File.Exists(temporary)) File.Delete(temporary);
            foreach (var item in partials) if (File.Exists(item.Partial)) File.Delete(item.Partial);
        }
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
