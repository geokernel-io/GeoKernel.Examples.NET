using System.Diagnostics;
using System.Text;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.DuckDbGeoParquetAnalytics.Wpf;

internal sealed record PathMetrics(long ElapsedMs, long SourceRows, long ResultRows, long PayloadBytes, int TransferredColumns);
internal sealed record ComparisonResult(PathMetrics FullTransfer, PathMetrics PushedDown, long DatasetRows, string ClassName, IReadOnlyList<byte[]> Geometries);

internal static class AnalyticsEngine
{
    private const double XMin = 18.04, YMin = 59.30, XMax = 18.10, YMax = 59.35;
    internal static ComparisonResult Run(string path, string className, long limit)
    {
        using var connection = new GeoKernelDuckDbConnection();
        var metadata = GeoKernelDuckGeoParquet.Inspect(connection, path);
        connection.Query("SELECT count(*) FROM read_parquet(?)", [path]);
        var watch = Stopwatch.StartNew();
        var all = connection.Query("SELECT id,class,geometry,bbox.xmin AS xmin,bbox.ymin AS ymin,bbox.xmax AS xmax,bbox.ymax AS ymax FROM read_parquet(?)", [path]);
        long matched = 0;
        for (var row = 0; row < all.RowCount && matched < limit; row++)
            if (Convert.ToString(all.Value(row, "class")) == className && Convert.ToDouble(all.Value(row, "xmax")) >= XMin &&
                Convert.ToDouble(all.Value(row, "xmin")) <= XMax && Convert.ToDouble(all.Value(row, "ymax")) >= YMin && Convert.ToDouble(all.Value(row, "ymin")) <= YMax) matched++;
        watch.Stop();
        var baseline = new PathMetrics(watch.ElapsedMilliseconds, all.RowCount, matched, Bytes(all), all.ColumnCount);
        watch.Restart();
        var filtered = GeoKernelDuckGeoParquet.Query(connection, path, new GeoKernelDuckGeoParquetQuery {
            Columns = ["id", "class", metadata.PrimaryGeometryColumn], Extent = new GeoKernelExtent(XMin,YMin,XMax,YMax),
            PredicateSql = "class = ?", PredicateParameters = [className], Limit = limit });
        var geometries = Enumerable.Range(0, filtered.RowCount).Select(row => filtered.Value(row, metadata.PrimaryGeometryColumn)).OfType<byte[]>().Where(value => value.Length > 0).ToArray();
        watch.Stop();
        return new ComparisonResult(baseline, new PathMetrics(watch.ElapsedMilliseconds,filtered.RowCount,geometries.Length,Bytes(filtered),filtered.ColumnCount),metadata.FeatureCount,className,geometries);
    }
    internal static string Report(ComparisonResult r,long materialize) {
        var optimized=r.PushedDown.ElapsedMs+materialize; var speed=optimized<=0?0:(double)r.FullTransfer.ElapsedMs/optimized;
        return string.Join(Environment.NewLine,"DUCKDB GEOPARQUET ANALYTICS","",$"Dataset: stockholm_buildings.parquet",$"Dataset rows: {r.DatasetRows}",$"Filter: class = '{r.ClassName}'",$"BBOX: {XMin}, {YMin}, {XMax}, {YMax}",$"Result rows: {r.PushedDown.ResultRows}","",
        "FULL TRANSFER + APPLICATION FILTER",$"Rows transferred: {r.FullTransfer.SourceRows}",$"Columns transferred: {r.FullTransfer.TransferredColumns}",$"Payload approximation: {Human(r.FullTransfer.PayloadBytes)}",$"Elapsed: {r.FullTransfer.ElapsedMs} ms","",
        "DUCKDB PUSHDOWN",$"Rows transferred: {r.PushedDown.SourceRows}",$"Columns transferred: {r.PushedDown.TransferredColumns}",$"Payload approximation: {Human(r.PushedDown.PayloadBytes)}",$"Elapsed + Viewer materialization: {optimized} ms","",
        "MEASURED GAIN",$"Speedup: {speed:F2}x",$"Row transfer reduction: {Reduction(r.FullTransfer.SourceRows,r.PushedDown.SourceRows):F2}%",$"Payload reduction: {Reduction(r.FullTransfer.PayloadBytes,r.PushedDown.PayloadBytes):F2}%","","The optimized path pushes class, BBOX, projection and limit into DuckDB before WKB crosses into the Viewer.");
    }
    private static long Bytes(GeoKernelDuckQueryResult r) { long total=0; for(var y=0;y<r.RowCount;y++) for(var x=0;x<r.ColumnCount;x++) total += r.Value(y,x) switch { byte[] b=>b.LongLength,null=>0,var v=>Encoding.UTF8.GetByteCount(Convert.ToString(v)??"")}; return total; }
    private static double Reduction(long a,long b)=>a<=0?0:100*(1-(double)b/a);
    private static string Human(long b)=>b>=1048576?$"{b/1048576d:F2} MiB":$"{b/1024d:F1} KiB";
}

