using System.Text.Json;
using System.IO;
using System.Text.Json.Nodes;
using GeoKernel.Examples.GeoAi;
#if GEOAI_WPF
using GeoKernel.NET.Wpf;
using GeoKernel.NET.Wpf.Controls;
#else
using GeoKernel.NET.WinForms;
#endif
namespace GeoKernel.Examples.BuildingChange;

internal sealed class ChangeSession
{
    internal readonly GeoKernelViewerControl[] Views = [new(), new()];
    internal event Action? Changed;
    internal JsonObject? Result
    {
        get; private set;
    }
    internal bool Busy
    {
        get; private set;
    }
    internal bool Overlays = true;
    internal bool SourcesReady
    {
        get; private set;
    }
    internal int Selected { get; private set; } = -1;
    internal int Percent
    {
        get; private set;
    }
    internal string Status { get; private set; } = "Preparing imagery…";
    internal Task Operation = Task.CompletedTask;
    Inputs? inputs; CancellationTokenSource? cancellation; bool syncing, closing; int revision;
    readonly double[]?[] applied = new double[]?[2];
    readonly string workspace = Path.Combine(Path.GetTempPath(), "geokernel-change-view-" + Guid.NewGuid());
    readonly bool[] renderBusy = [false, false];
    JsonObject? publishedResult;
    bool candidatesDirty;
    internal ChangeSession()
    {
        Directory.CreateDirectory(workspace);
        for (int i = 0; i < 2; i++)
        {
            int side = i;
            Views[i].VisibleExtentChanged += (_, _) => QueueSync(side);
            Views[i].BusyChanged += (_, e) => renderBusy[side] = e.Busy;
        }
    }
    void QueueSync(int side)
    {
        if (closing)
            return;
        // Leave the native callback before reading or changing either viewer.
#if GEOAI_WPF
        Views[side].Dispatcher.BeginInvoke(new Action(() => Sync(side)));
#else
        if (Views[side].IsHandleCreated && !Views[side].IsDisposed)
            Views[side].BeginInvoke(new Action(() => Sync(side)));
#endif
    }
    void Sync(int side)
    {
        if (syncing || closing)
            return;
        var extent = Views[side].ViewExtent;
        double[] current = [extent.XMin, extent.YMin, extent.XMax, extent.YMax];
        if (applied[side] is { } previous && current.Zip(previous).All(p => Math.Abs(p.First - p.Second) < 1e-10))
            return;
        applied[side] = current;
        syncing = true;
        try
        {
            Views[1 - side].ViewExtent = extent;
            var other = Views[1 - side].ViewExtent;
            applied[1 - side] = [other.XMin, other.YMin, other.XMax, other.YMax];
        }
        finally { syncing = false; }
    }
    IProgress<DownloadProgress> Progress => new Progress<DownloadProgress>(p => { Percent = p.Percent; Status = p.Message; Changed?.Invoke(); });
    async Task Execute(Func<CancellationToken, Task> action)
    {
        if (Busy)
            return;
        Busy = true;
        cancellation = new();
        Changed?.Invoke();
        try
        {
            await action(cancellation.Token);
        }
        catch (OperationCanceledException) { Status = "Cancelled. Previous results retained."; }
        catch (Exception e) { Status = e.Message; }
        finally { Busy = false; cancellation.Dispose(); cancellation = null; Changed?.Invoke(); }
    }
    internal void Cancel() => cancellation?.Cancel();
    internal async Task Initialize()
    {
        await Execute(async token => { await LoadSources(token); Status = "Ready"; });
    }
    async Task LoadSources(CancellationToken token)
    {
        inputs ??= await ChangeCore.Prepare(Progress, token);
        Status = "Preparing map previews…";
        Changed?.Invoke();
        var previews = await Task.Run(() =>
        {
            string[] paths = [inputs.Before, inputs.After];
            for (int side = 0; side < 2; side++)
            {
                token.ThrowIfCancellationRequested();
                paths[side] = D.DisplayRaster(paths[side], Path.Combine(workspace, $"display-{side}.tif"));
            }
            return paths;
        }, token);
        token.ThrowIfCancellationRequested();
        for (int side = 0; side < 2; side++)
        {
            Views[side].ClearLayers();
            if (!Views[side].SetCoordinateSystemPreset(GeoKernelCoordinateSystemPreset.Wgs84) || !Views[side].AddLayerFile(previews[side]))
                throw new IOException("Cannot display imagery.");
            var extent = Views[side].GetLayerProjectedExtent(0);
            if (extent.XMin is < -180 or > 180 || extent.YMin is < -90 or > 90)
                throw new InvalidDataException("Imagery and map coordinate systems differ.");
            Views[side].SetLayerName(0, "Imagery");
            Views[side].ActiveTool = GeoKernelViewerTool.Pan;
        }
        SourcesReady = true;
        Focus(ChangeCore.Demo);
    }
    internal async Task Run(string area, Policy policy, double threshold, string provider)
    {
        await Execute(async token =>
        {
            Capture();
            if (!SourcesReady)
                await LoadSources(token);
            double[] bounds = area == "Full dataset" ? ChangeCore.Bounds : ChangeCore.Demo;
            if (area == "Current view")
            {
                var e = Views[0].ViewExtent;
                bounds = ChangeCore.Envelope(D.Geometry(ChangeCore.Rectangle([e.XMin, e.YMin, e.XMax, e.YMax])).Geometry);
            }
            var progress = Progress;
            var result = await Task.Run(() => ChangeCore.Analyze(inputs!, bounds, policy, threshold, provider, progress, token));
            token.ThrowIfCancellationRequested();
            Result = result;
            Selected = result["features"]!.AsArray().Count > 0 ? 0 : -1;
            Redraw();
            Status = "Complete";
            Percent = 100;
        });
    }
    internal void Focus(double[] bounds)
    {
        var geometry = D.Geometry(ChangeCore.Rectangle(bounds), 25831, 4326).Geometry;
        var b = ChangeCore.Envelope(geometry);
        Views[0].ViewExtent = new(b[0], b[1], b[2], b[3]);
        Sync(0);
    }
    internal void Select(int index)
    {
        if (Busy || Result is null || Result["features"]!.AsArray().Count == 0)
            return;
        index = Math.Clamp(index, 0, Result["features"]!.AsArray().Count - 1);
        if (index == Selected && ReferenceEquals(publishedResult, Result))
            return;
        Capture(notify: false);
        EndSelectionEdit();
        Selected = index;
        if (Overlays)
        {
            EnsureCandidates();
            UpdateSelection();
        }
        Changed?.Invoke();
    }
    internal bool Capture(bool notify = true)
    {
        int layer = SelectionIndex(Views[1]);
        if (Result is null || Selected < 0 || !Overlays || layer < 0 || !Views[1].IsLayerEditing(layer))
            return false;
        string wkt = Views[1].WriteLayerLastShapeWkt(layer);
        if (string.IsNullOrEmpty(wkt))
            throw new InvalidOperationException("Selected editable polygon is unavailable.");
        var feature = Result["features"]![Selected]!.AsObject();
        string previousStatus = feature["properties"]!["review_status"]!.GetValue<string>();
        bool changed = ChangeCore.Update(feature, D.FromWkt(wkt));
        if (changed)
        {
            // Keep the active edit layer/undo stack while dragging. Publish the
            // changed candidate geometry when leaving the edit or reviewing it.
            candidatesDirty = true;
            if (previousStatus != "pending")
                ApplyCandidateStyle();
            if (notify)
                Changed?.Invoke();
        }
        return changed;
    }
    internal void Review(string status, string actor, string note)
    {
        Capture();
        if (Result is null || Selected < 0)
            return;
        string previousStatus = Result["features"]![Selected]!["properties"]!["review_status"]!.GetValue<string>();
        ChangeCore.Review(Result["features"]![Selected]!.AsObject(), status, actor, note);
        if (candidatesDirty)
            Redraw();
        else if (previousStatus != status)
            ApplyCandidateStyle();
        Changed?.Invoke();
    }
    internal void Export(string path)
    {
        Capture();
        if (Result is null)
            throw new InvalidOperationException("Run analysis first.");
        ChangeCore.Export(Result, path);
    }
    internal void Locate()
    {
        if (Result is null || Selected < 0)
            return;
        var b = ChangeCore.Envelope(Result["features"]![Selected]!["geometry"]!);
        double dx = Math.Max(.00015, (b[2] - b[0]) * .5), dy = Math.Max(.00015, (b[3] - b[1]) * .5);
        Views[0].ViewExtent = new(b[0] - dx, b[1] - dy, b[2] + dx, b[3] + dy);
        Sync(0);
    }
    internal void Undo(bool redo)
    {
        int layer = SelectionIndex(Views[1]);
        if (Result is null || Selected < 0 || !Overlays || layer < 0)
            return;
        if (redo)
            Views[1].RedoEditLayer(layer);
        else
            Views[1].UndoEditLayer(layer);
        Capture();
    }
    internal void SetOverlays(bool value)
    {
        if (Overlays == value)
            return;
        Capture();
        Overlays = value;
        if (value)
            Redraw();
        else
        {
            foreach (var view in Views)
                view.ClearSelectedFeatures();
            foreach (var view in Views)
                foreach (string name in new[] { "Selection", "Selection targets", "Candidates" })
                    if (view.GetLayerInfoByName(name) is { } layer)
                        view.SetLayerVisible(layer.Index, false);
        }
    }
    static int SelectionIndex(GeoKernelViewerControl view) => view.GetLayerInfoByName("Selection")?.Index ?? -1;
    string Write(JsonArray features)
    {
        string path = Path.Combine(workspace, $"overlay-{++revision}.geojson");
        File.WriteAllText(path, new JsonObject { { "type", "FeatureCollection" }, { "features", features } }.ToJsonString());
        return path;
    }
    internal void Redraw()
    {
        if (!Overlays)
            return;
        if (!ReferenceEquals(publishedResult, Result) || candidatesDirty)
            EndSelectionEdit();
        EnsureCandidates();
        UpdateSelection();
    }
    void EnsureCandidates()
    {
        if (ReferenceEquals(publishedResult, Result) && !candidatesDirty)
            return;
        for (int side = 0; side < 2; side++)
        {
            var view = Views[side];
            while (view.RemoveLayerByName("Candidates"))
            {
            }
            while (view.RemoveLayerByName("Selection targets"))
            {
            }
            if (Result is null)
                continue;
            var targets = new JsonArray(Result["features"]!.AsArray().Select(feature =>
            {
                var copy = feature!.DeepClone().AsObject();
                if (side == 0)
                    copy["geometry"] = ChangeCore.Rectangle(ChangeCore.Envelope(copy["geometry"]!));
                return (JsonNode)copy;
            }).ToArray());
            if (targets.Count > 0)
            {
                if (!view.AddLayerFile(Write(targets)))
                    throw new IOException("Cannot display selection references.");
                view.SetLayerName(0, "Selection targets");
                view.SetLayerStyleJson(0, "{\"fillOpacity\":0,\"lineColor\":\"#00000000\",\"lineWidth\":0,\"selectedLineColor\":\"#ef9b00\",\"selectedLineWidth\":2}");
            }
            string kind = side == 0 ? "removed" : "new";
            var features = new JsonArray(Result["features"]!.AsArray()
                .Where(f => f!["properties"]!["change_type"]!.GetValue<string>() == kind)
                .Select(f => f!.DeepClone()).ToArray());
            if (features.Count == 0)
                continue;
            if (!view.AddLayerFile(Write(features)))
                throw new IOException("Cannot display candidates.");
            view.SetLayerName(0, "Candidates");
        }
        publishedResult = Result;
        candidatesDirty = false;
        ApplyCandidateStyle();
    }
    void ApplyCandidateStyle()
    {
        if (Result is null)
            return;
        for (int side = 0; side < 2; side++)
        {
            var view = Views[side];
            int index = view.GetLayerInfoByName("Candidates")?.Index ?? -1;
            if (index < 0)
                continue;
            string kind = side == 0 ? "removed" : "new", color = side == 0 ? "#ee554f" : "#20bf73";
            var rules = Result["features"]!.AsArray()
                .Where(f => f!["properties"]!["change_type"]!.GetValue<string>() == kind && f["properties"]!["review_status"]!.GetValue<string>() != "pending")
                .Select(f => new
                {
                    field = "candidate_id",
                    @operator = "equals",
                    value = f!["id"]!.GetValue<string>(),
                    style = f["properties"]!["review_status"]!.GetValue<string>() == "approved"
                        ? new
                        {
                            fillColor = color,
                            lineColor = color,
                            fillOpacity = 90,
                            lineWidth = 3
                        }
                        : new
                        {
                            fillColor = "#82909f",
                            lineColor = "#82909f",
                            fillOpacity = 10,
                            lineWidth = 1
                        }
                }).ToArray();
            // Style changes update the existing layer; its data source stays open.
            if (!view.SetLayerSymbolRendererJson(index, JsonSerializer.Serialize(new
            {
                type = "ruleBased",
                defaultStyle = new
                {
                    fillColor = color,
                    lineColor = color,
                    fillOpacity = 35,
                    lineWidth = 2
                },
                rules
            })))
                throw new InvalidOperationException("Cannot style candidates.");
        }
    }
    void UpdateSelection()
    {
        for (int side = 0; side < 2; side++)
        {
            var view = Views[side];
            if (!Overlays || Result is null || Selected < 0)
            {
                view.ClearSelectedFeatures();
                continue;
            }
            foreach (string name in new[] { "Candidates", "Selection targets", "Selection" })
                if (view.GetLayerInfoByName(name) is { Visible: false } layer)
                    view.SetLayerVisible(layer.Index, true);
            var feature = Result["features"]![Selected]!;
            var geometry = feature["geometry"]!;
            if (side == 0)
                geometry = ChangeCore.Rectangle(ChangeCore.Envelope(geometry));
            int editLayer = side == 1 ? SelectionIndex(view) : -1;
            int target = editLayer >= 0 ? editLayer : view.GetLayerInfoByName("Selection targets")?.Index ?? -1;
            SelectGeometry(view, target, geometry, editLayer >= 0 ? null : feature["id"]!.GetValue<string>());
        }
    }
    static void SelectGeometry(GeoKernelViewerControl view, int layer, JsonNode geometry, string? candidateId)
    {
        if (layer < 0)
            throw new InvalidOperationException("Selection references are unavailable.");
        var b = ChangeCore.Envelope(geometry);
        double tolerance = Math.Max(Math.Max(b[2] - b[0], b[3] - b[1]), 1e-9) * 2;
        var hit = view.HitTestFeaturesInExtent(new GeoKernelExtent(b[0], b[1], b[2], b[3]))
            .FirstOrDefault(hit => hit.LayerIndex == layer && (candidateId is null ||
                hit.Attributes.TryGetValue("candidate_id", out var id) && id?.ToString() == candidateId));
        if (hit is null)
            throw new InvalidOperationException("Cannot find selected candidate.");
        // Published 1.5.39 hit JSON parsing leaves the readonly point at (0,0).
        // Supply the known extent centre rather than passing that default point.
        var selection = new GeoKernelFeatureHitTestResult
        {
            IsValid = hit.IsValid,
            LayerIndex = hit.LayerIndex,
            LayerName = hit.LayerName,
            ShapeId = hit.ShapeId,
            ShapeType = hit.ShapeType,
            FeatureId = hit.FeatureId,
            IsProviderFeature = hit.IsProviderFeature,
            Attributes = hit.Attributes,
            WorldPoint = new GeoKernelPoint((b[0] + b[2]) / 2, (b[1] + b[3]) / 2),
            Extent = new GeoKernelExtent(b[0], b[1], b[2], b[3])
        };
        // The centre can be outside a concave roof; cover it with the tolerance.
        if (!view.SelectFeatureHit(selection, tolerance))
            throw new InvalidOperationException("Cannot highlight selected candidate.");
    }
    void EndSelectionEdit()
    {
        if (SelectionIndex(Views[1]) < 0)
            return;
        Views[1].ClearSelectedFeatures();
        Views[1].RemoveLayerByName("Selection");
        Views[1].ActiveTool = GeoKernelViewerTool.Pan;
    }
    internal void UseTool(GeoKernelViewerTool tool)
    {
        if (tool == GeoKernelViewerTool.EditVertices)
        {
            if (!Overlays || Result is null || Selected < 0)
                throw new InvalidOperationException("Select a visible candidate before editing.");
            if (SelectionIndex(Views[1]) < 0)
            {
                var view = Views[1];
                var geometry = Result["features"]![Selected]!["geometry"]!;
                if (geometry["type"]!.GetValue<string>() != "Polygon")
                    throw new InvalidOperationException("Multipart geometry is preserved but cannot be edited in this example.");
                IReadOnlyList<IReadOnlyList<GeoKernelPoint>> rings = geometry["coordinates"]!.AsArray()
                    .Select(r => (IReadOnlyList<GeoKernelPoint>)r!.AsArray()
                        .Select(p => new GeoKernelPoint(p![0]!.GetValue<double>(), p[1]!.GetValue<double>())).ToArray()).ToArray();
                view.ClearSelectedFeatures();
                int index = view.AddPolygonLayer("Selection", rings, new GeoKernelLayerStyle
                {
                    FillOpacity = 0,
                    LineColor = "#ef9b00",
                    LineWidth = 2,
                    SelectedLineColor = "#ef9b00",
                    SelectedLineWidth = 2
                });
                if (index < 0 || !view.BeginEditLayer(index) || !view.SetActiveEditLayerIndex(index))
                    throw new InvalidOperationException("Cannot edit selected candidate.");
                SelectGeometry(view, index, geometry, null);
            }
        }
        Views[1].ActiveTool = tool;
    }
    internal async Task Close()
    {
        closing = true;
        Cancel();
        await Operation;
        foreach (var view in Views)
            view.ClearLayers();
        await Task.Delay(200);
        while (renderBusy.Any(x => x))
            await Task.Delay(50);
        Directory.Delete(workspace, true);
    }
}
