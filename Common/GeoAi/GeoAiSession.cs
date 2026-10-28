using System.IO;
using System.Text.Json;
#if GEOAI_WPF
using NativeTask = GeoKernel.NET.Wpf.AITask;
#else
using NativeTask = GeoKernel.NET.WinForms.AITask;
#endif

namespace GeoKernel.Examples.GeoAi;

internal sealed class GeoAiSession(GeoAiProfile profile) : IAsyncDisposable
{
    private NativeTask? job;
    private string? raster;
    private string? sourceDisplay;
    private CancellationTokenSource? cancellation;
    private bool working;
    private bool cancellable;

    public event Action? Changed;
    public required Action<string> ShowSource { get; init; }
    public required Action<string> ShowResult { get; init; }
    public required Action ClearResult { get; init; }
    public Func<Task> WaitForRenderer { get; init; } = () => Task.CompletedTask;
    public bool IsWorking => working;
    public bool CanCancel => working && cancellable;
    public bool HasResult { get; private set; }
    public int Percent { get; private set; }
    public string Status { get; private set; } = "Preparing sample…";
    public string Details { get; private set; } = profile.Description;

    private IProgress<DownloadProgress> DownloadStatus => new Progress<DownloadProgress>(p => Update(p.Percent, p.Message));

    public async Task InitializeAsync()
    {
        await ExecuteAsync(async token =>
        {
            await PrepareSourceAsync(token);
            Update(0, "Ready. Click Run to process the image.");
        });
    }

    public async Task RunAsync(string provider, double threshold = 0.5)
    {
        await ExecuteAsync(async token =>
        {
            ClearResult();
            HasResult = false;
            if (job != null)
            {
                // A completed SR job owns both comparison previews. Detach its source before releasing it.
                if (profile.SuperResolution && sourceDisplay != null) ShowSource(sourceDisplay);
                await WaitForRenderer();
                await job.DisposeAsync();
                job = null;
            }
            if (raster == null)
            {
                await PrepareSourceAsync(token);
            }
            string manifest = await ModelAssets.PrepareAsync(profile.ModelUrl, profile.ModelHash, "geokernel-model.json", DownloadStatus, token);
            token.ThrowIfCancellationRequested();
            var request = profile.CreateRequest(manifest, raster!, provider, threshold);
            Update(0, "Starting inference…");
            job = await Task.Run(() => NativeTask.Start(request));
            bool cancelRequested = false;
            while (true)
            {
                if (token.IsCancellationRequested && !cancelRequested) { job.Cancel(); cancelRequested = true; }
                JsonElement status = job.Status();
                string? state = status.GetProperty("state").GetString();
                Update(status.GetProperty("percent").GetInt32(), status.GetProperty("message").GetString() ?? state!);
                if (state == "failed") throw new InvalidOperationException(status.GetProperty("error").GetString());
                if (state == "cancelled") throw new OperationCanceledException();
                if (state == "completed") break;
                await Task.Delay(100);
            }
            token.ThrowIfCancellationRequested();
            JsonElement result = job.Result();
            if (profile.SuperResolution) ShowSource(result.GetProperty("sourcePreviewPath").GetString()!);
            string resultDisplay = result.GetProperty(profile.BuildingFootprints ? "vectorPath" : "previewPath").GetString()!;
            if (profile.BuildingFootprints) resultDisplay = await VectorPreview.PrepareAsync(resultDisplay, token);
            token.ThrowIfCancellationRequested();
            ShowResult(resultDisplay);
            HasResult = true;
            string instances = result.TryGetProperty("instances", out JsonElement count) ? $"Instances: {count}\n" : "";
            Details = profile.SuperResolution
                ? $"{profile.Title}\nProvider: {result.GetProperty("provider")}\n" +
                    $"Source: {result.GetProperty("sourceWidth")} × {result.GetProperty("sourceHeight")}\n" +
                    $"Output: {result.GetProperty("width")} × {result.GetProperty("height")} · 4 bands · Float32\n" +
                    $"Elapsed: {result.GetProperty("elapsedMilliseconds")} ms\n\n" +
                    "Save GeoTIFF retains RGB + NIR, source footprint and AI provenance.\nEstimated detail on a finer grid, not new sensor measurements."
                : $"{profile.Title}\nProvider: {result.GetProperty("provider").GetString()}\n" +
                instances + $"Polygons: {result.GetProperty("polygons")}\nElapsed: {result.GetProperty("elapsedMilliseconds")} ms\n\n" +
                "Results are temporary for this session. Use Save Shapefile to keep the polygons.\n\n" +
                (profile.BuildingFootprints ? $"Threshold: {threshold:F2}\nPredictions are not ground truth. Model errors can remain."
                    : profile.TaskKind == "prithvi-flood"
                        ? "Predicted water / flood is class 1; non-water is class 0. NoData is excluded.\nA single image does not distinguish permanent water from newly flooded land."
                    : profile.TaskKind == "prue-fields"
                        ? "Class 1 field interiors are exported; background and boundary pixels are not polygons.\nImage-derived field divisions are not legal/cadastral parcels."
                        : "Predictions are not ground truth. Water (class 0) is valid; NoData is excluded.");
            Update(100, "Inference complete.");
        });
    }

    public async Task ExportAsync(string path)
    {
        if (!HasResult || job == null) return;
        await ExecuteAsync(async _ =>
        {
            Update(Percent, profile.SuperResolution ? "Saving GeoTIFF…" : "Saving Shapefile…");
            await job.ExportAsync(path, profile.SuperResolution ? "geotiff" : "shapefile");
            Update(100, profile.SuperResolution ? "GeoTIFF saved." : "Shapefile saved.");
        }, canCancel: false);
    }

    public void Cancel()
    {
        if (cancellable) cancellation?.Cancel();
    }

    private async Task PrepareSourceAsync(CancellationToken token)
    {
        string prepared = await ModelAssets.PrepareAsync(profile.DataUrl, profile.DataHash, profile.RasterName, DownloadStatus, token, profile.ArchivedData);
        string display = profile.TaskKind == "prithvi-flood"
            ? await RasterPreview.PrepareFloodAsync(prepared, ModelAssets.CacheDirectory(profile.DataHash), token)
            : profile.TaskKind == "prue-fields"
                ? await RasterPreview.PrepareFieldsAsync(prepared, ModelAssets.CacheDirectory(profile.DataHash), token)
            : profile.SuperResolution
                ? await RasterPreview.PrepareSuperResolutionAsync(prepared, ModelAssets.CacheDirectory(profile.DataHash), token)
            : Path.Combine(Path.GetDirectoryName(prepared)!, profile.DisplayName);
        token.ThrowIfCancellationRequested();
        ShowSource(display);
        sourceDisplay = display;
        raster = prepared;
    }

    private async Task ExecuteAsync(Func<CancellationToken, Task> action, bool canCancel = true)
    {
        if (working) return;
        working = true;
        cancellable = canCancel;
        if (canCancel) Percent = 0;
        cancellation = new CancellationTokenSource();
        Changed?.Invoke();
        try { await action(cancellation.Token); }
        catch (OperationCanceledException) { Update(0, "Cancelled."); }
        catch (Exception error) { Details = error.Message; Update(0, "Operation failed. See diagnostics."); }
        finally
        {
            if (!HasResult && job != null)
            {
                ClearResult();
                if (profile.SuperResolution && sourceDisplay != null) ShowSource(sourceDisplay);
                await WaitForRenderer();
                await job.DisposeAsync();
                job = null;
            }
            cancellation.Dispose();
            cancellation = null;
            working = false;
            Changed?.Invoke();
        }
    }

    private void Update(int percent, string message)
    {
        Percent = Math.Clamp(percent, 0, 100);
        Status = message.Replace("%p%", $"{Percent}%");
        Changed?.Invoke();
    }

    public async ValueTask DisposeAsync()
    {
        ClearResult();
        HasResult = false;
        if (job != null) { await WaitForRenderer(); await job.DisposeAsync(); job = null; }
    }
}
