using System.IO;
using System.Runtime.InteropServices;

namespace GeoKernel.Examples.GeoAi;

internal static class Sam3Coordinates
{
    // The example map is explicitly EPSG:4326. Native SAM prompts must use the original raster CRS.
    public static (Sam3MapPoint[] Points, double[]? Box) ToSource(string raster, Sam3Generation options)
    {
        Gdal.GDALAllRegister();
        IntPtr dataset = Gdal.GDALOpen(raster, 0);
        if (dataset == IntPtr.Zero) throw new IOException("Cannot read the SAM3 source CRS.");
        string wkt;
        try { wkt = Marshal.PtrToStringUTF8(Gdal.GDALGetProjectionRef(dataset)) ?? ""; }
        finally { Gdal.GDALClose(dataset); }
        IntPtr source = Gdal.OSRNewSpatialReference(null), destination = Gdal.OSRNewSpatialReference(wkt);
        IntPtr transform = IntPtr.Zero;
        try
        {
            if (source == IntPtr.Zero || destination == IntPtr.Zero || Gdal.OSRImportFromEPSG(source, 4326) != 0)
                throw new InvalidDataException("Cannot initialize SAM3 prompt coordinate systems.");
            Gdal.OSRSetAxisMappingStrategy(source, 0);
            Gdal.OSRSetAxisMappingStrategy(destination, 0);
            transform = Gdal.OCTNewCoordinateTransformation(source, destination);
            if (transform == IntPtr.Zero) throw new InvalidDataException("Cannot transform map prompts to the raster CRS.");
            Sam3MapPoint Convert(Sam3MapPoint point)
            {
                double[] x = [point.X], y = [point.Y], z = [0];
                if (Gdal.OCTTransform(transform, 1, x, y, z) == 0 || !double.IsFinite(x[0]) || !double.IsFinite(y[0]))
                    throw new InvalidDataException("A SAM3 prompt cannot be transformed to the raster CRS.");
                return new(x[0], y[0], point.Positive);
            }
            Sam3MapPoint[] points = options.Points.Select(Convert).ToArray();
            double[]? box = null;
            if (options.Box is Sam3MapBox b)
            {
                Sam3MapPoint[] corners = [Convert(new(b.MinimumX, b.MinimumY)), Convert(new(b.MinimumX, b.MaximumY)),
                    Convert(new(b.MaximumX, b.MinimumY)), Convert(new(b.MaximumX, b.MaximumY))];
                box = [corners.Min(p => p.X), corners.Min(p => p.Y), corners.Max(p => p.X), corners.Max(p => p.Y)];
            }
            return (points, box);
        }
        finally
        {
            if (transform != IntPtr.Zero) Gdal.OCTDestroyCoordinateTransformation(transform);
            if (source != IntPtr.Zero) Gdal.OSRDestroySpatialReference(source);
            if (destination != IntPtr.Zero) Gdal.OSRDestroySpatialReference(destination);
        }
    }

    private static class Gdal
    {
        private const string Dll = "gdal.dll";
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void GDALAllRegister();
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int access);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void GDALClose(IntPtr dataset);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALGetProjectionRef(IntPtr dataset);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr OSRNewSpatialReference([MarshalAs(UnmanagedType.LPUTF8Str)] string? wkt);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int OSRImportFromEPSG(IntPtr srs, int epsg);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void OSRSetAxisMappingStrategy(IntPtr srs, int strategy);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void OSRDestroySpatialReference(IntPtr srs);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr OCTNewCoordinateTransformation(IntPtr source, IntPtr destination);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int OCTTransform(IntPtr transform, int count, [In, Out] double[] x, [In, Out] double[] y, [In, Out] double[] z);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void OCTDestroyCoordinateTransformation(IntPtr transform);
    }
}
