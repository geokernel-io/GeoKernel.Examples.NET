using System.IO.Compression;

namespace GeoKernel.GlobeBuildings.Winforms;

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

}


