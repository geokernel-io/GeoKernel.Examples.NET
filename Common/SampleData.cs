using System.IO;
using System.IO.Compression;
using System.Net.Http;
using WinForms = System.Windows.Forms;
using Wpf = System.Windows;
using WpfControls = System.Windows.Controls;

namespace GeoKernel.Examples.Common;

public static class SampleData
{
    private const string DataDirectoryEnvironmentVariable = "GEOKERNEL_EXAMPLES_DATA_DIR";
    private const string SampleDataReleaseUrl =
        "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/";

    public static string EnsureKnownWpfSampleFile(string relativePath, Wpf.Window? owner)
    {
        var normalizedPath = relativePath.Replace('\\', '/').TrimStart('/');
        var fileName = Path.GetFileName(normalizedPath);
        var (archiveName, extractFolderName, requiredFileName) = fileName.ToLowerInvariant() switch
        {
            "world_4326.shp" => ("world_4326.zip", "world_4326", "world_4326.shp"),
            "world_3857.shp" => ("world_3857.zip", "world_3857", "world_3857.shp"),
            "cities_4326.shp" => ("cities_4326.zip", "cities_4326", "cities_4326.shp"),
            "world_cities_4326.shp" => ("world_cities_4326.zip", "world_cities_4326", "world_cities_4326.shp"),
            "usa_states.shp" => ("usa_states.zip", "usa_states", "usa_states.shp"),
            "usa_states_3857.shp" => ("usa_states_3857.zip", "usa_states_3857", "usa_states_3857.shp"),
            "usa_cities.shp" => ("usa_cities.zip", "usa_cities", "usa_cities.shp"),
            "california.shp" => ("california.zip", "california", "california.shp"),
            "output_1m_points.shp" => ("output_1m_points.zip", "output_1m_points", "output_1m_points.shp"),
            "geog_25000.dxf" => ("geog_25000_dxf.zip", "geog_25000_dxf", "geog_25000.dxf"),
            "europe_detailed.gpkg" => ("europe_detailed.zip", "europe_detailed_gpkg", "europe_detailed.gpkg"),
            "travel.kmz" => ("travel.zip", "travel", "travel.kmz"),
            "albania.mif" => ("albania.zip", "albania_mif", "albania.mif"),
            "paris.tab" => ("paris_tab.zip", "paris_tab", "paris.tab"),
            "world_8km.tif" => ("world_8km_tif.zip", "world_8km_tif", "world_8km.tif"),
            "world_8km.ecw" => ("world_8km_ecw.zip", "world_8km_ecw", "world_8km.ecw"),
            "andalucia.geokernel" => ("andalucia.zip", "andalucia", "andalucia.geokernel"),
            _ => throw new ArgumentException($"Unknown GeoKernel sample data file: {relativePath}", nameof(relativePath))
        };

        return EnsureWpfSampleFile(
            new Uri(SampleDataReleaseUrl + archiveName),
            archiveName,
            extractFolderName,
            requiredFileName,
            owner);
    }

    public static string EnsureKnownWpfSampleDirectory(string sampleName, Wpf.Window? owner)
    {
        if (!string.Equals(sampleName, "california_cities", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Unknown GeoKernel sample data directory: {sampleName}", nameof(sampleName));

        var requiredPath = EnsureWpfSampleFile(
            new Uri(SampleDataReleaseUrl + "california_cities.zip"),
            "california_cities.zip",
            "california_cities",
            "alameda.shp",
            owner);
        return string.IsNullOrWhiteSpace(requiredPath) ? string.Empty : Path.GetDirectoryName(requiredPath)!;
    }

    public static string EnsureSampleFile(
        Uri sourceUrl,
        string archiveName,
        string extractFolderName,
        string requiredFileName,
        WinForms.IWin32Window? owner = null)
    {
        var request = SampleDataRequest.Create(sourceUrl, archiveName, extractFolderName, requiredFileName);
        request.SeedFromLocalOutputData();

        if (File.Exists(request.RequiredPath))
            return request.RequiredPath;

        Directory.CreateDirectory(request.DataDirectory);

        using var dialog = new WinFormsSampleDataDownloadDialog(request);
        return dialog.ShowDialog(owner) == WinForms.DialogResult.OK && File.Exists(request.RequiredPath)
            ? request.RequiredPath
            : string.Empty;
    }

    public static string EnsureWpfSampleFile(
        Uri sourceUrl,
        string archiveName,
        string extractFolderName,
        string requiredFileName,
        Wpf.Window? owner)
    {
        var request = SampleDataRequest.Create(sourceUrl, archiveName, extractFolderName, requiredFileName);
        request.SeedFromLocalOutputData();

        if (File.Exists(request.RequiredPath))
            return request.RequiredPath;

        Directory.CreateDirectory(request.DataDirectory);

        var dialog = new WpfSampleDataDownloadWindow(request)
        {
            Owner = owner,
            WindowStartupLocation = owner is null
                ? Wpf.WindowStartupLocation.CenterScreen
                : Wpf.WindowStartupLocation.CenterOwner,
            ShowInTaskbar = owner is null
        };

        return dialog.ShowDialog() == true && File.Exists(request.RequiredPath)
            ? request.RequiredPath
            : string.Empty;
    }

    internal static string ResolveDataDirectory()
    {
        var configuredDirectory = Environment.GetEnvironmentVariable(DataDirectoryEnvironmentVariable);
        if (!string.IsNullOrWhiteSpace(configuredDirectory))
            return Path.GetFullPath(configuredDirectory);

        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (string.Equals(directory.Name, "outputs", StringComparison.OrdinalIgnoreCase))
                return Path.Combine(directory.FullName, "data");

            directory = directory.Parent;
        }

        return Path.Combine(AppContext.BaseDirectory, "data");
    }
}

internal sealed record SampleDataRequest(
    Uri SourceUrl,
    string ArchivePath,
    string ExtractDirectory,
    string RequiredPath,
    string DataDirectory,
    string LocalExtractDirectory,
    string LocalRequiredPath)
{
    public static SampleDataRequest Create(
        Uri sourceUrl,
        string archiveName,
        string extractFolderName,
        string requiredFileName)
    {
        var dataDirectory = SampleData.ResolveDataDirectory();
        var extractDirectory = Path.Combine(dataDirectory, extractFolderName);
        var localExtractDirectory = Path.Combine(AppContext.BaseDirectory, "data", extractFolderName);

        return new SampleDataRequest(
            sourceUrl,
            Path.Combine(dataDirectory, archiveName),
            extractDirectory,
            Path.Combine(extractDirectory, requiredFileName),
            dataDirectory,
            localExtractDirectory,
            Path.Combine(localExtractDirectory, requiredFileName));
    }

    public void SeedFromLocalOutputData()
    {
        if (File.Exists(RequiredPath) || !File.Exists(LocalRequiredPath))
            return;

        if (SamePath(ExtractDirectory, LocalExtractDirectory))
            return;

        Directory.CreateDirectory(ExtractDirectory);
        CopyDirectory(LocalExtractDirectory, ExtractDirectory);
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        foreach (var directory in Directory.EnumerateDirectories(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, directory);
            Directory.CreateDirectory(Path.Combine(destinationDirectory, relativePath));
        }

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relativePath = Path.GetRelativePath(sourceDirectory, file);
            var destinationPath = Path.Combine(destinationDirectory, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
            File.Copy(file, destinationPath, overwrite: true);
        }
    }

    private static bool SamePath(string left, string right)
    {
        static string Normalize(string path) =>
            Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

        return string.Equals(Normalize(left), Normalize(right), StringComparison.OrdinalIgnoreCase);
    }
}

internal sealed class WinFormsSampleDataDownloadDialog : WinForms.Form
{
    private readonly SampleDataRequest request;
    private readonly WinForms.Label statusLabel;
    private readonly WinForms.ProgressBar progressBar;

    public WinFormsSampleDataDownloadDialog(SampleDataRequest request)
    {
        this.request = request;

        Text = "GeoKernel Sample Data";
        Width = 560;
        Height = 170;
        FormBorderStyle = WinForms.FormBorderStyle.FixedDialog;
        StartPosition = WinForms.FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;
        ShowInTaskbar = false;

        statusLabel = new WinForms.Label
        {
            AutoSize = false,
            Left = 24,
            Top = 24,
            Width = 500,
            Height = 36,
            Text = "Preparing sample data..."
        };

        progressBar = new WinForms.ProgressBar
        {
            Left = 24,
            Top = 72,
            Width = 500,
            Height = 22
        };

        Controls.Add(statusLabel);
        Controls.Add(progressBar);
    }

    protected override async void OnShown(EventArgs e)
    {
        base.OnShown(e);
        await PrepareAsync();
    }

    private async Task PrepareAsync()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(request.ArchivePath)!);
            Directory.CreateDirectory(request.ExtractDirectory);

            statusLabel.Text = $"Downloading {Path.GetFileName(request.ArchivePath)}...";
            await DownloadAsync();

            statusLabel.Text = "Extracting sample data...";
            progressBar.Style = WinForms.ProgressBarStyle.Marquee;
            await Task.Run(() => ZipFile.ExtractToDirectory(request.ArchivePath, request.ExtractDirectory, overwriteFiles: true));

            if (File.Exists(request.ArchivePath))
                File.Delete(request.ArchivePath);

            if (!File.Exists(request.RequiredPath))
                throw new FileNotFoundException("The expected sample file was not found after extraction.", request.RequiredPath);

            DialogResult = WinForms.DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            TryDeleteArchive();
            WinForms.MessageBox.Show(
                this,
                $"Sample data could not be prepared.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "GeoKernel Sample Data",
                WinForms.MessageBoxButtons.OK,
                WinForms.MessageBoxIcon.Error);

            DialogResult = WinForms.DialogResult.Cancel;
            Close();
        }
    }

    private async Task DownloadAsync()
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(request.SourceUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalLength = response.Content.Headers.ContentLength;
        progressBar.Value = 0;
        progressBar.Style = totalLength.HasValue
            ? WinForms.ProgressBarStyle.Continuous
            : WinForms.ProgressBarStyle.Marquee;

        await using var source = await response.Content.ReadAsStreamAsync();
        await using var destination = File.Create(request.ArchivePath);

        var buffer = new byte[81920];
        long totalRead = 0;

        while (true)
        {
            var bytesRead = await source.ReadAsync(buffer);
            if (bytesRead == 0)
                break;

            await destination.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalRead += bytesRead;

            if (!totalLength.HasValue)
                continue;

            var percent = (int)Math.Min(100, totalRead * 100 / totalLength.Value);
            progressBar.Value = percent;
            statusLabel.Text = $"Downloading {Path.GetFileName(request.ArchivePath)}... {percent}%";
        }
    }

    private void TryDeleteArchive()
    {
        try
        {
            if (File.Exists(request.ArchivePath))
                File.Delete(request.ArchivePath);
        }
        catch
        {
            // Best-effort cleanup after a failed download or extraction.
        }
    }
}

internal sealed class WpfSampleDataDownloadWindow : Wpf.Window
{
    private readonly SampleDataRequest request;
    private readonly WpfControls.TextBlock statusText;
    private readonly WpfControls.ProgressBar progressBar;

    public WpfSampleDataDownloadWindow(SampleDataRequest request)
    {
        this.request = request;

        Title = "GeoKernel Sample Data";
        Width = 560;
        Height = 170;
        ResizeMode = Wpf.ResizeMode.NoResize;

        var panel = new WpfControls.StackPanel
        {
            Margin = new Wpf.Thickness(24)
        };

        statusText = new WpfControls.TextBlock
        {
            Height = 36,
            Text = "Preparing sample data...",
            TextWrapping = Wpf.TextWrapping.Wrap
        };

        progressBar = new WpfControls.ProgressBar
        {
            Height = 22,
            Minimum = 0,
            Maximum = 100
        };

        panel.Children.Add(statusText);
        panel.Children.Add(progressBar);
        Content = panel;
    }

    protected override async void OnContentRendered(EventArgs e)
    {
        base.OnContentRendered(e);
        await PrepareAsync();
    }

    private async Task PrepareAsync()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(request.ArchivePath)!);
            Directory.CreateDirectory(request.ExtractDirectory);

            statusText.Text = $"Downloading {Path.GetFileName(request.ArchivePath)}...";
            await DownloadAsync();

            statusText.Text = "Extracting sample data...";
            progressBar.IsIndeterminate = true;
            await Task.Run(() => ZipFile.ExtractToDirectory(request.ArchivePath, request.ExtractDirectory, overwriteFiles: true));

            if (File.Exists(request.ArchivePath))
                File.Delete(request.ArchivePath);

            if (!File.Exists(request.RequiredPath))
                throw new FileNotFoundException("The expected sample file was not found after extraction.", request.RequiredPath);

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            TryDeleteArchive();
            Wpf.MessageBox.Show(
                this,
                $"Sample data could not be prepared.{Environment.NewLine}{Environment.NewLine}{ex.Message}",
                "GeoKernel Sample Data",
                Wpf.MessageBoxButton.OK,
                Wpf.MessageBoxImage.Error);

            DialogResult = false;
            Close();
        }
    }

    private async Task DownloadAsync()
    {
        using var httpClient = new HttpClient();
        using var response = await httpClient.GetAsync(request.SourceUrl, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        var totalLength = response.Content.Headers.ContentLength;
        progressBar.Value = 0;
        progressBar.IsIndeterminate = !totalLength.HasValue;

        await using var source = await response.Content.ReadAsStreamAsync();
        await using var destination = File.Create(request.ArchivePath);

        var buffer = new byte[81920];
        long totalRead = 0;

        while (true)
        {
            var bytesRead = await source.ReadAsync(buffer);
            if (bytesRead == 0)
                break;

            await destination.WriteAsync(buffer.AsMemory(0, bytesRead));
            totalRead += bytesRead;

            if (!totalLength.HasValue)
                continue;

            var percent = (int)Math.Min(100, totalRead * 100 / totalLength.Value);
            progressBar.Value = percent;
            statusText.Text = $"Downloading {Path.GetFileName(request.ArchivePath)}... {percent}%";
        }
    }

    private void TryDeleteArchive()
    {
        try
        {
            if (File.Exists(request.ArchivePath))
                File.Delete(request.ArchivePath);
        }
        catch
        {
            // Best-effort cleanup after a failed download or extraction.
        }
    }
}
