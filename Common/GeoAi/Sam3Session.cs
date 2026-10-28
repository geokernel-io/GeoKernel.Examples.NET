using System.IO;
using System.Text.Json;
#if GEOAI_WPF
using NativeTask = GeoKernel.NET.Wpf.AITask;
#else
using NativeTask = GeoKernel.NET.WinForms.AITask;
#endif

namespace GeoKernel.Examples.GeoAi;

internal sealed class Sam3Session : IAsyncDisposable
{
    internal const string TextHash = "95d9bc3ebdec6a6147a9bfa97325712bfbb77fccbd6bb3f20be453c381820931";
    internal const string TrackerHash = "c4309ed67b2292f64641d740f2558c2d4253476ca1ec40c17d49c4800c050896";
    private const string ModelRoot = "https://pub-00d214f25f754884b22349c19ac37bbf.r2.dev/geokernel-models/";
    private NativeTask? job;
    private string? raster;
    private CancellationTokenSource? cancellation;
    private bool cancellable;
    public required Action<string> ShowSource { get; init; }
    public required Action<string> ShowResult { get; init; }
    public required Action ClearResult { get; init; }
    public Func<Task> WaitForRenderer { get; init; } = () => Task.CompletedTask;
    public event Action? Changed;
    public bool IsWorking { get; private set; }
    public bool CanCancel => IsWorking && cancellable;
    public bool HasResult { get; private set; }
    public bool Accepted { get; private set; }
    public int Percent { get; private set; }
    public string Status { get; private set; } = "Preparing source…";
    public string Details { get; private set; } = "SAM3 text / point / box segmentation.\nNo predictions are loaded at startup.";
    private IProgress<DownloadProgress> DownloadStatus => new Progress<DownloadProgress>(p => Update(p.Percent, p.Message));

    public Task InitializeAsync() => ExecuteAsync(async token =>
    {
        await PrepareSourceAsync(token);
        Update(0, "Ready. Select a task, then Generate draft.");
    });

    public Task GenerateAsync(Sam3Generation options) => ExecuteAsync(async token =>
    {
        if (HasResult) throw new InvalidOperationException("Discard the current result before generating another draft.");
        if (options.TextMode && string.IsNullOrWhiteSpace(options.Text)) throw new ArgumentException("Enter a text concept, for example building.");
        if (!options.TextMode && !options.Box.HasValue && !options.Points.Any(p => p.Positive))
            throw new ArgumentException("Add a positive point or a box around one object.");
        if (raster == null) await PrepareSourceAsync(token);
        string name = options.TextMode ? "text" : "tracker";
        string manifest = await ModelAssets.PrepareAsync(ModelRoot + $"meta-sam3-{name}-fp32-geokernel-onnx-v1.zip",
            options.TextMode ? TextHash : TrackerHash, "sam3-package.json", DownloadStatus, token);
        var request = new Dictionary<string, object?>
        {
            ["task"] = "sam3-" + name, ["modelPackagePath"] = Path.GetDirectoryName(manifest),
            ["rasterPath"] = raster, ["provider"] = options.Provider.ToLowerInvariant(),
            ["intraOpThreads"] = 4, ["maximumMilliseconds"] = 1800000
        };
        if (options.TextMode)
        {
            request["text"] = options.Text.Trim();
            request["scoreThreshold"] = options.ScoreThreshold;
        }
        else
        {
            var prompts = await Task.Run(() => Sam3Coordinates.ToSource(raster!, options), token);
            request["coordinateSpace"] = "source-crs";
            request["points"] = prompts.Points.Select(p => new { x = p.X, y = p.Y, positive = p.Positive }).ToArray();
            if (prompts.Box != null) request["box"] = prompts.Box;
        }
        token.ThrowIfCancellationRequested();
        Update(0, "Loading SAM3 model…");
        job = await Task.Run(() => NativeTask.Start(JsonSerializer.SerializeToElement(request)));
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
        string preview = await VectorPreview.PrepareAsync(result.GetProperty("vectorPath").GetString()!, token);
        token.ThrowIfCancellationRequested();
        ShowResult(preview);
        HasResult = true;
        Accepted = false;
        Details = $"SAM3 {name}\nProvider: {result.GetProperty("provider")}\nInstances: {result.GetProperty("instances")}\n" +
            $"Polygons: {result.GetProperty("polygons")}\nElapsed: {result.GetProperty("elapsedMilliseconds")} ms\n\n" +
            "Review the draft on the map. Accept enables saving; Discard removes the result.\nA point/box selects one object; a box is not an area scan for all buildings.";
        Update(100, "Draft ready. Review, then Accept or Discard.");
    });

    public void Accept()
    {
        if (IsWorking || !HasResult) return;
        Accepted = true;
        Update(100, "Draft accepted. Save explicitly to retain the result.");
    }

    public Task DiscardAsync() => ExecuteAsync(async _ =>
    {
        ClearResult();
        HasResult = false;
        Accepted = false;
        if (job != null) { await WaitForRenderer(); await job.DisposeAsync(); job = null; }
        Update(0, "Result discarded. Select prompts or generate a new draft.");
    }, false);

    public Task ExportAsync(string path, string format) => ExecuteAsync(async _ =>
    {
        if (!Accepted || job == null) throw new InvalidOperationException("Accept the draft before saving.");
        await job.ExportAsync(path, format);
        Update(100, "Accepted result saved.");
    }, false);

    public void Cancel() { if (cancellable) cancellation?.Cancel(); }

    private async Task PrepareSourceAsync(CancellationToken token)
    {
        string prepared = await ModelAssets.PrepareAsync(GeoAiProfile.Hotosm.DataUrl, GeoAiProfile.Hotosm.DataHash,
            "buildings.tif", DownloadStatus, token);
        token.ThrowIfCancellationRequested();
        ShowSource(prepared);
        raster = prepared;
    }

    private async Task ExecuteAsync(Func<CancellationToken, Task> action, bool canCancel = true)
    {
        if (IsWorking) return;
        IsWorking = true;
        cancellable = canCancel;
        cancellation = new CancellationTokenSource();
        if (canCancel) Percent = 0;
        Changed?.Invoke();
        try { await action(cancellation.Token); }
        catch (OperationCanceledException) { Update(0, "Cancelled."); }
        catch (Exception error) { Details = error.Message; Update(0, "Operation failed. See diagnostics."); }
        finally
        {
            if (!HasResult && job != null) { ClearResult(); await WaitForRenderer(); await job.DisposeAsync(); job = null; }
            cancellation.Dispose();
            cancellation = null;
            IsWorking = false;
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
        Accepted = false;
        if (job != null) { await WaitForRenderer(); await job.DisposeAsync(); job = null; }
    }
}
