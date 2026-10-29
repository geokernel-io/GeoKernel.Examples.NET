using System.Runtime.InteropServices;
using System.IO;
using System.Text.Json.Nodes;

namespace GeoKernel.Examples.BuildingChange;

internal static class D
{
    const string Lib = "gdal.dll";
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void GDALAllRegister();
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int access);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALOpenEx([MarshalAs(UnmanagedType.LPUTF8Str)] string path, uint flags, IntPtr a, IntPtr b, IntPtr c);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void GDALClose(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALGetDriverByName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALCreate(IntPtr driver, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, int w, int h, int bands, int type, IntPtr opts);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALAutoCreateWarpedVRT(IntPtr source, IntPtr sourceWkt, [MarshalAs(UnmanagedType.LPUTF8Str)] string targetWkt, int resampling, double maximumError, IntPtr options);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] static extern IntPtr GDALCreateCopy(IntPtr driver, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, IntPtr source, int strict, IntPtr options, IntPtr callback, IntPtr context);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALGetRasterXSize(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALGetRasterYSize(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALGetRasterBand(IntPtr ds, int band);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALGetGeoTransform(IntPtr ds, [Out] double[] gt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALSetGeoTransform(IntPtr ds, double[] gt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALGetProjectionRef(IntPtr ds);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALSetProjection(IntPtr ds, [MarshalAs(UnmanagedType.LPUTF8Str)] string wkt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALSetRasterColorInterpretation(IntPtr band, int value);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALRasterIO(IntPtr band, int rw, int x, int y, int w, int h, IntPtr data, int bw, int bh, int type, int ps, int ls);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALDatasetGetLayer(IntPtr ds, int index);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void OGR_L_ResetReading(IntPtr layer);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr OGR_L_GetNextFeature(IntPtr layer);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void OGR_F_Destroy(IntPtr f);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr OGR_F_GetGeometryRef(IntPtr f);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int OGR_F_GetFieldIndex(IntPtr f, [MarshalAs(UnmanagedType.LPUTF8Str)] string name);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int OGR_F_GetFieldAsInteger(IntPtr f, int field);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr OGR_G_ExportToJson(IntPtr g);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr OGR_G_CreateGeometryFromJson([MarshalAs(UnmanagedType.LPUTF8Str)] string json);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int OGR_G_CreateFromWkt(ref IntPtr text, IntPtr srs, out IntPtr geom);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void OGR_G_DestroyGeometry(IntPtr g);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern double OGR_G_Area(IntPtr g);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int OGR_G_IsValid(IntPtr g);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int OGR_G_Transform(IntPtr g, IntPtr transform);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void VSIFree(IntPtr p);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr OSRNewSpatialReference(IntPtr wkt);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int OSRImportFromEPSG(IntPtr srs, int epsg);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void OSRSetAxisMappingStrategy(IntPtr srs, int strategy);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void OSRDestroySpatialReference(IntPtr srs);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr OCTNewCoordinateTransformation(IntPtr a, IntPtr b);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern void OCTDestroyCoordinateTransformation(IntPtr transform);
    [DllImport(Lib, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALRasterizeLayers(IntPtr ds, int n, int[] bands, int count, IntPtr[] layers, IntPtr trans, IntPtr arg, IntPtr burn, IntPtr[] opts, IntPtr callback, IntPtr context);
    internal static void Check(int code)
    {
        if (code != 0)
            throw new InvalidOperationException("GDAL operation failed.");
    }
    internal static string Text(IntPtr p) => Marshal.PtrToStringUTF8(p) ?? "";
    internal static string DisplayRaster(string source, string destination)
    {
        // The published viewer cannot reproject this ETRS89 raster while drawing.
        // Reproject a display copy; inference always reads the original metre grid.
        GDALAllRegister();
        using var input = new Ds(Open(source));
        using var warped = new Ds(GDALAutoCreateWarpedVRT(input.Value, IntPtr.Zero, "EPSG:4326", 1, 0, IntPtr.Zero));
        using var output = new Ds(GDALCreateCopy(GDALGetDriverByName("GTiff"), destination, warped.Value, 0, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));
        var grid = Metadata(output.Value);
        if (grid.Transform[0] is < -180 or > 180 || grid.Transform[3] is < -90 or > 90)
            throw new InvalidDataException("Map preview was not reprojected to WGS84.");
        return destination;
    }
    internal static JsonObject Json(IntPtr geom)
    {
        var p = OGR_G_ExportToJson(geom);
        if (p == IntPtr.Zero)
            throw new InvalidOperationException("Cannot serialize geometry.");
        try
        {
            return JsonNode.Parse(Text(p))!.AsObject();
        }
        finally { VSIFree(p); }
    }
    internal static IntPtr Open(string path)
    {
        var ds = GDALOpen(path, 0);
        return ds != IntPtr.Zero ? ds : throw new IOException($"Cannot open {path}");
    }
    internal static Grid Metadata(IntPtr ds)
    {
        double[] gt = new double[6];
        Check(GDALGetGeoTransform(ds, gt));
        return new(GDALGetRasterXSize(ds), GDALGetRasterYSize(ds), gt, Text(GDALGetProjectionRef(ds)));
    }
    internal static T[] Read<T>(IntPtr ds, int band, int type) where T : unmanaged
    {
        var grid = Metadata(ds);
        var values = new T[checked(grid.Width * grid.Height)];
        IO(GDALGetRasterBand(ds, band), 0, 0, 0, grid.Width, grid.Height, values, type);
        return values;
    }
    internal static void IO<T>(IntPtr band, int rw, int x, int y, int w, int h, T[] data, int type) where T : unmanaged
    {
        var pin = GCHandle.Alloc(data, GCHandleType.Pinned);
        try
        {
            Check(GDALRasterIO(band, rw, x, y, w, h, pin.AddrOfPinnedObject(), w, h, type, 0, 0));
        }
        finally { pin.Free(); }
    }
    internal static void Crop(string source, double[] bounds, string target)
    {
        using var ds = new Ds(Open(source));
        var g = Metadata(ds.Value);
        if (g.Transform[1] != .5 || g.Transform[5] != -.5 || g.Transform[2] != 0 || g.Transform[4] != 0 || !g.Crs.Contains("25831"))
            throw new InvalidDataException("Expected the pinned EPSG:25831 0.5m grid.");
        int x = Math.Max(0, (int)Math.Floor((bounds[0] - g.Transform[0]) / .5)), y = Math.Max(0, (int)Math.Floor((g.Transform[3] - bounds[3]) / .5));
        int w = Math.Min(g.Width, (int)Math.Ceiling((bounds[2] - g.Transform[0]) / .5)) - x, h = Math.Min(g.Height, (int)Math.Ceiling((g.Transform[3] - bounds[1]) / .5)) - y;
        if (w <= 0 || h <= 0)
            throw new ArgumentException("Analysis area does not intersect the imagery.");
        using var output = new Ds(GDALCreate(GDALGetDriverByName("GTiff"), target, w, h, 3, 6, IntPtr.Zero));
        Check(GDALSetProjection(output.Value, g.Crs));
        Check(GDALSetGeoTransform(output.Value, [g.Transform[0] + x * .5, .5, 0, g.Transform[3] - y * .5, 0, -.5]));
        for (int band = 1; band <= 3; band++)
        {
            var a = new float[w * h];
            IO(GDALGetRasterBand(ds.Value, band), 0, x, y, w, h, a, 6);
            IO(GDALGetRasterBand(output.Value, band), 1, 0, 0, w, h, a, 6);
            Check(GDALSetRasterColorInterpretation(GDALGetRasterBand(output.Value, band), band + 2));
        }
    }
    internal static (JsonObject Geometry, double Area) Geometry(JsonNode value, int source = 4326, int target = 25831)
    {
        var geom = OGR_G_CreateGeometryFromJson(value.ToJsonString());
        if (geom == IntPtr.Zero)
            throw new InvalidDataException("Invalid geometry JSON.");
        IntPtr a = IntPtr.Zero, b = IntPtr.Zero, operation = IntPtr.Zero;
        try
        {
            if (OGR_G_IsValid(geom) == 0)
                throw new InvalidDataException("Invalid polygon; repair before approval.");
            if (source != target)
            {
                a = OSRNewSpatialReference(IntPtr.Zero);
                b = OSRNewSpatialReference(IntPtr.Zero);
                Check(OSRImportFromEPSG(a, source));
                Check(OSRImportFromEPSG(b, target));
                OSRSetAxisMappingStrategy(a, 0);
                OSRSetAxisMappingStrategy(b, 0);
                operation = OCTNewCoordinateTransformation(a, b);
                if (operation == IntPtr.Zero)
                    throw new InvalidOperationException("Cannot transform geometry.");
                Check(OGR_G_Transform(geom, operation));
            }
            return (Json(geom), OGR_G_Area(geom));
        }
        finally { OGR_G_DestroyGeometry(geom); if (operation != IntPtr.Zero) OCTDestroyCoordinateTransformation(operation); if (a != IntPtr.Zero) OSRDestroySpatialReference(a); if (b != IntPtr.Zero) OSRDestroySpatialReference(b); }
    }
    internal static JsonObject FromWkt(string text)
    {
        IntPtr memory = Marshal.StringToCoTaskMemUTF8(text), cursor = memory, geom = IntPtr.Zero;
        try
        {
            Check(OGR_G_CreateFromWkt(ref cursor, IntPtr.Zero, out geom));
            return Json(geom);
        }
        finally { if (geom != IntPtr.Zero) OGR_G_DestroyGeometry(geom); Marshal.FreeCoTaskMem(memory); }
    }
    internal static (List<Polygon> Polygons, int[] Labels) Polygons(string path, Grid grid)
    {
        using var ds = new Ds(GDALOpenEx(path, 4, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero));
        var layer = GDALDatasetGetLayer(ds.Value, 0);
        if (layer == IntPtr.Zero)
            throw new InvalidDataException("Prediction layer missing.");
        var polygons = new List<Polygon>();
        IntPtr f;
        while ((f = OGR_L_GetNextFeature(layer)) != IntPtr.Zero)
        {
            try
            {
                var geom = OGR_F_GetGeometryRef(f);
                if (geom == IntPtr.Zero || OGR_G_IsValid(geom) == 0)
                    continue;
                int field = OGR_F_GetFieldIndex(f, "instance_id");
                if (field < 0)
                    throw new InvalidDataException("Missing instance_id.");
                polygons.Add(new(OGR_F_GetFieldAsInteger(f, field), Json(geom), OGR_G_Area(geom)));
            }
            finally { OGR_F_Destroy(f); }
        }
        using var labels = new Ds(GDALCreate(GDALGetDriverByName("MEM"), "", grid.Width, grid.Height, 1, 5, IntPtr.Zero));
        Check(GDALSetProjection(labels.Value, grid.Crs));
        Check(GDALSetGeoTransform(labels.Value, grid.Transform));
        if (polygons.Count > 0)
        {
            OGR_L_ResetReading(layer);
            var attr = Marshal.StringToCoTaskMemUTF8("ATTRIBUTE=instance_id");
            try
            {
                Check(GDALRasterizeLayers(labels.Value, 1, [1], 1, [layer], IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, [attr, IntPtr.Zero], IntPtr.Zero, IntPtr.Zero));
            }
            finally { Marshal.FreeCoTaskMem(attr); }
        }
        return (polygons, Read<int>(labels.Value, 1, 5));
    }
    internal sealed class Ds(IntPtr value) : IDisposable
    {
        internal IntPtr Value { get; } = value != IntPtr.Zero ? value : throw new InvalidOperationException("GDAL dataset unavailable."); public void Dispose() => GDALClose(Value);
    }
}
internal sealed record Grid(int Width, int Height, double[] Transform, string Crs)
{
    internal bool Same(Grid g) => Width == g.Width && Height == g.Height && Transform.SequenceEqual(g.Transform) && Crs == g.Crs;
}
internal sealed record Polygon(int Id, JsonObject Geometry, double Area);
