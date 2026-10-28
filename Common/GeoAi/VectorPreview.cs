using System.IO;
using System.Runtime.InteropServices;

namespace GeoKernel.Examples.GeoAi;

internal static class VectorPreview
{
    // The GeoPackage remains the authoritative result for export. Use a small,
    // job-owned WGS84 GeoJSON for display so provider-only GPKG metadata cannot
    // leave the viewer with a default CRS and an empty drawable extent.
    public static Task<string> PrepareAsync(string vectorPath, CancellationToken cancellation) => Task.Run(() =>
    {
        cancellation.ThrowIfCancellationRequested();
        string output = Path.Combine(Path.GetDirectoryName(vectorPath)!, "viewer-preview.geojson");
        Gdal.GDALAllRegister();
        IntPtr input = Gdal.GDALOpenEx(vectorPath, 4, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero);
        if (input == IntPtr.Zero) throw new InvalidDataException("Cannot open result polygons for display.");
        IntPtr options = IntPtr.Zero;
        string[] arguments = ["-f", "GeoJSON", "-t_srs", "EPSG:4326", "-dim", "XY"];
        IntPtr[] nativeArguments = new IntPtr[arguments.Length + 1];
        Gdal.Progress progress = (_, _, _) => cancellation.IsCancellationRequested ? 0 : 1;
        try
        {
            for (int i = 0; i < arguments.Length; i++) nativeArguments[i] = Marshal.StringToCoTaskMemUTF8(arguments[i]);
            options = Gdal.GDALVectorTranslateOptionsNew(nativeArguments, IntPtr.Zero);
            if (options == IntPtr.Zero) throw new InvalidOperationException("Cannot initialize polygon preview conversion.");
            Gdal.GDALVectorTranslateOptionsSetProgress(options, progress, IntPtr.Zero);
            IntPtr translated = Gdal.GDALVectorTranslate(output, IntPtr.Zero, 1, [input], options, out int usageError);
            if (translated == IntPtr.Zero)
            {
                cancellation.ThrowIfCancellationRequested();
                throw new InvalidDataException("Cannot prepare result polygon preview.");
            }
            Gdal.GDALClose(translated);
            cancellation.ThrowIfCancellationRequested();
            if (usageError != 0) throw new InvalidOperationException("Invalid polygon preview conversion options.");
            return output;
        }
        finally
        {
            if (options != IntPtr.Zero) Gdal.GDALVectorTranslateOptionsFree(options);
            foreach (IntPtr argument in nativeArguments) if (argument != IntPtr.Zero) Marshal.FreeCoTaskMem(argument);
            Gdal.GDALClose(input);
            GC.KeepAlive(progress);
        }
    }, cancellation);

    private static class Gdal
    {
        private const string Library = "gdal.dll";
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        internal delegate int Progress(double fraction, IntPtr message, IntPtr context);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GDALAllRegister();
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr GDALOpenEx([MarshalAs(UnmanagedType.LPUTF8Str)] string path, uint flags, IntPtr drivers, IntPtr options, IntPtr siblings);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr GDALVectorTranslateOptionsNew([In] IntPtr[] arguments, IntPtr binaryOptions);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GDALVectorTranslateOptionsSetProgress(IntPtr options, Progress progress, IntPtr context);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern IntPtr GDALVectorTranslate([MarshalAs(UnmanagedType.LPUTF8Str)] string destination, IntPtr output, int count, [In] IntPtr[] inputs, IntPtr options, out int usageError);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GDALVectorTranslateOptionsFree(IntPtr options);
        [DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
        internal static extern void GDALClose(IntPtr dataset);
    }
}
