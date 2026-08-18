using System.Diagnostics;
using System.Text;
using GeoKernel.NET.WinForms;

namespace GeoKernel.DuckDbGeoParquetAnalytics.Winforms;

internal sealed record PathMetrics(long ElapsedMs, long SourceRows, long ResultRows, long PayloadBytes, int TransferredColumns);
internal sealed record ComparisonResult(PathMetrics FullTransfer, PathMetrics PushedDown, long DatasetRows, string ClassName, string Path, IReadOnlyList<byte[]> Geometries);

internal static class AnalyticsEngine
{
    internal const double XMin = 18.04, YMin = 59.30, XMax = 18.10, YMax = 59.35;

    internal static ComparisonResult Run(string path, string className, long limit)
    {
        using var connection = new GeoKernelDuckDbConnection();
        var metadata = GeoKernelDuckGeoParquet.Inspect(connection, path);
        connection.Query("SELECT count(*) FROM read_parquet(?)", [path]);

        var timer = Stopwatch.StartNew();
        var allRows = connection.Query("SELECT id,class,geometry,bbox.xmin AS xmin,bbox.ymin AS ymin,bbox.xmax AS xmax,bbox.ymax AS ymax FROM read_parquet(?)", [path]);
        long matched = 0;
        for (var row = 0; row < allRows.RowCount && matched < limit; row++)
            if (string.Equals(Convert.ToString(allRows.Value(row, "class")), className, StringComparison.Ordinal) &&
                Convert.ToDouble(allRows.Value(row, "xmax")) >= XMin && Convert.ToDouble(allRows.Value(row, "xmin")) <= XMax &&
                Convert.ToDouble(allRows.Value(row, "ymax")) >= YMin && Convert.ToDouble(allRows.Value(row, "ymin")) <= YMax)
                matched++;
        timer.Stop();
        var baseline = new PathMetrics(timer.ElapsedMilliseconds, allRows.RowCount, matched, TransferredBytes(allRows), allRows.ColumnCount);

        timer.Restart();
        var filtered = GeoKernelDuckGeoParquet.Query(connection, path, new GeoKernelDuckGeoParquetQuery
        {
            Columns = ["id", "class", metadata.PrimaryGeometryColumn],
            Extent = new GeoKernelExtent(XMin, YMin, XMax, YMax),
            PredicateSql = "class = ?",
            PredicateParameters = [className],
            Limit = limit
        });
        var geometries = new List<byte[]>(filtered.RowCount);
        for (var row = 0; row < filtered.RowCount; row++)
            if (filtered.Value(row, metadata.PrimaryGeometryColumn) is byte[] wkb && wkb.Length > 0)
                geometries.Add(wkb);
        timer.Stop();
        var optimized = new PathMetrics(timer.ElapsedMilliseconds, filtered.RowCount, geometries.Count, TransferredBytes(filtered), filtered.ColumnCount);
        return new ComparisonResult(baseline, optimized, metadata.FeatureCount, className, path, geometries);
    }

    internal static string Report(ComparisonResult result, long materializationMs)
    {
        var optimizedMs = result.PushedDown.ElapsedMs + materializationMs;
        var speedup = optimizedMs <= 0 ? 0 : (double)result.FullTransfer.ElapsedMs / optimizedMs;
        return string.Join(Environment.NewLine,
            "DUCKDB GEOPARQUET ANALYTICS", "",
            "Dataset: stockholm_buildings.parquet",
            $"Dataset rows: {result.DatasetRows}",
            $"Filter: class = '{result.ClassName}'",
            $"BBOX: {XMin}, {YMin}, {XMax}, {YMax}",
            $"Result rows: {result.PushedDown.ResultRows}", "",
            "FULL TRANSFER + APPLICATION FILTER",
            $"Rows transferred: {result.FullTransfer.SourceRows}",
            $"Columns transferred: {result.FullTransfer.TransferredColumns}",
            $"Payload approximation: {HumanBytes(result.FullTransfer.PayloadBytes)}",
            $"Elapsed: {result.FullTransfer.ElapsedMs} ms", "",
            "DUCKDB PUSHDOWN",
            $"Rows transferred: {result.PushedDown.SourceRows}",
            $"Columns transferred: {result.PushedDown.TransferredColumns}",
            $"Payload approximation: {HumanBytes(result.PushedDown.PayloadBytes)}",
            $"Elapsed + Viewer materialization: {optimizedMs} ms", "",
            "MEASURED GAIN",
            $"Speedup: {speedup:F2}x",
            $"Row transfer reduction: {Reduction(result.FullTransfer.SourceRows, result.PushedDown.SourceRows):F2}%",
            $"Payload reduction: {Reduction(result.FullTransfer.PayloadBytes, result.PushedDown.PayloadBytes):F2}%", "",
            "The optimized path pushes class, BBOX, projection and limit into DuckDB before WKB crosses into the Viewer.");
    }

    private static long TransferredBytes(GeoKernelDuckQueryResult result)
    {
        long bytes = 0;
        for (var row = 0; row < result.RowCount; row++)
            for (var column = 0; column < result.ColumnCount; column++)
                bytes += result.Value(row, column) switch
                {
                    byte[] binary => binary.LongLength,
                    null => 0,
                    var value => Encoding.UTF8.GetByteCount(Convert.ToString(value) ?? "")
                };
        return bytes;
    }

    private static double Reduction(long before, long after) => before <= 0 ? 0 : 100.0 * (1.0 - (double)after / before);
    private static string HumanBytes(long bytes) => bytes >= 1024 * 1024 ? $"{bytes / 1024d / 1024d:F2} MiB" : $"{bytes / 1024d:F1} KiB";
}
