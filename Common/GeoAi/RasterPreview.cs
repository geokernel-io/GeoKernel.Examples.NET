using System.IO;
using System.Runtime.InteropServices;

namespace GeoKernel.Examples.GeoAi;

internal static class RasterPreview
{
    public static Task<string> PrepareFloodAsync(string input, string cache, CancellationToken cancellation) =>
        PrepareAsync(input, cache, cancellation, false);

    public static Task<string> PrepareFieldsAsync(string input, string cache, CancellationToken cancellation) =>
        PrepareAsync(input, cache, cancellation, true);

    public static Task<string> PrepareSuperResolutionAsync(string input, string cache, CancellationToken cancellation) =>
        PrepareAsync(input, cache, cancellation, true, true);

    private static Task<string> PrepareAsync(string input, string cache, CancellationToken cancellation, bool fields, bool superResolution = false) => Task.Run(() =>
    {
        // This is an input display image, never an inference output. Keep the original spectral raster intact.
        string output = Path.Combine(cache, superResolution ? "sen2sr-source-rgb-v1.tif" : fields ? "prue-source-rgb-v1.tif" : "prithvi-source-rgb-v1.tif");
        if (File.Exists(output)) return output;
        string temporary = Path.Combine(cache, Guid.NewGuid() + ".tif");
        try
        {
            Create(input, temporary, cancellation, fields, superResolution);
            cancellation.ThrowIfCancellationRequested();
            try { File.Move(temporary, output); }
            catch (IOException) when (File.Exists(output)) { }
            return output;
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }, cancellation);

    private static void Create(string input, string output, CancellationToken cancellation, bool fields, bool superResolution)
    {
        Gdal.GDALAllRegister();
        using var source = new Dataset(Gdal.GDALOpen(input, 0));
        int width = Gdal.GDALGetRasterXSize(source.Handle), height = Gdal.GDALGetRasterYSize(source.Handle);
        if (width < 1 || height < 1 || (long)width * height > 16_000_000)
            throw new InvalidDataException("Unsupported source display dimensions.");
        int count = Gdal.GDALGetRasterCount(source.Handle);
        if (superResolution && count is not (4 or 8)) throw new InvalidDataException("SEN2SR needs RGBN input.");
        if (fields && !superResolution && count != 8) throw new InvalidDataException("PRUE needs the aligned two-period eight-band stack.");
        int[] bands = superResolution ? [1, 2, 3, 4] : fields ? [1, 2, 3, 4, 5, 6, 7, 8] : count switch
        {
            13 => [2, 3, 4, 9, 12, 13],
            6 => [1, 2, 3, 4, 5, 6],
            _ => throw new InvalidDataException("Prithvi needs the original 13-band chip or a six-band spectral stack.")
        };
        int pixels = width * height;
        bool[] valid = Enumerable.Repeat(true, pixels).ToArray();
        var channels = new float[3][];
        for (int b = 0; b < bands.Length; b++)
        {
            cancellation.ThrowIfCancellationRequested();
            IntPtr band = Gdal.GDALGetRasterBand(source.Handle, bands[b]);
            float[] values = new float[pixels];
            Check(Gdal.Read(band, 0, 0, 0, width, height, values, width, height, 6, 0, 0));
            double nodata = Gdal.GDALGetRasterNoDataValue(band, out int hasNodata);
            for (int i = 0; i < pixels; i++)
                valid[i] &= float.IsFinite(values[i]) && values[i] >= 0 && (hasNodata == 0 || values[i] != nodata);
            // PRUE starts with RGB; Prithvi starts with B2/B3/B4, so reverse those three display bands.
            if (b < 3) channels[fields ? b : 2 - b] = values;
        }
        double[] transform = new double[6];
        Check(Gdal.GDALGetGeoTransform(source.Handle, transform));
        string projection = Marshal.PtrToStringUTF8(Gdal.GDALGetProjectionRef(source.Handle)) ?? "";
        using var destination = new Dataset(Gdal.GDALCreate(Gdal.GDALGetDriverByName("GTiff"), output, width, height, 4, 1, IntPtr.Zero));
        Check(Gdal.GDALSetGeoTransform(destination.Handle, transform));
        Check(Gdal.GDALSetProjection(destination.Handle, projection));
        for (int channel = 0; channel < 4; channel++)
        {
            cancellation.ThrowIfCancellationRequested();
            byte[] bytes = new byte[pixels];
            if (channel == 3)
            {
                for (int i = 0; i < pixels; i++) bytes[i] = valid[i] ? (byte)255 : (byte)0;
            }
            else
            {
                float[] values = channels[channel];
                float[] sorted = values.Where((_, i) => valid[i]).ToArray();
                if (sorted.Length == 0) throw new InvalidDataException("The source has no valid RGB display pixels.");
                Array.Sort(sorted);
                double minimum = sorted[(int)((sorted.Length - 1) * 0.02)];
                double maximum = sorted[(int)((sorted.Length - 1) * 0.98)];
                double range = Math.Max(maximum - minimum, 1);
                for (int i = 0; i < pixels; i++)
                    if (valid[i]) bytes[i] = (byte)Math.Clamp((values[i] - minimum) * 255 / range, 0, 255);
            }
            IntPtr band = Gdal.GDALGetRasterBand(destination.Handle, channel + 1);
            Check(Gdal.Write(band, 1, 0, 0, width, height, bytes, width, height, 1, 0, 0));
            Check(Gdal.GDALSetRasterColorInterpretation(band, channel + 3)); // Red, green, blue, alpha.
        }
    }

    private static void Check(int error)
    {
        if (error != 0) throw new IOException("GDAL could not prepare the source RGB preview.");
    }

    private sealed class Dataset(IntPtr handle) : IDisposable
    {
        public IntPtr Handle { get; } = handle != IntPtr.Zero ? handle : throw new IOException("Cannot open/create RGB display raster.");
        public void Dispose() => Gdal.GDALClose(Handle);
    }

    private static class Gdal
    {
        private const string Dll = "gdal.dll";
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void GDALAllRegister();
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALOpen([MarshalAs(UnmanagedType.LPUTF8Str)] string path, int access);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern void GDALClose(IntPtr dataset);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALGetRasterXSize(IntPtr dataset);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALGetRasterYSize(IntPtr dataset);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALGetRasterCount(IntPtr dataset);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALGetRasterBand(IntPtr dataset, int band);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern double GDALGetRasterNoDataValue(IntPtr band, out int success);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALGetProjectionRef(IntPtr dataset);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALGetGeoTransform(IntPtr dataset, [Out] double[] transform);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALSetGeoTransform(IntPtr dataset, double[] transform);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALSetProjection(IntPtr dataset, [MarshalAs(UnmanagedType.LPUTF8Str)] string projection);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALGetDriverByName([MarshalAs(UnmanagedType.LPUTF8Str)] string name);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern IntPtr GDALCreate(IntPtr driver, [MarshalAs(UnmanagedType.LPUTF8Str)] string path, int width, int height, int bands, int type, IntPtr options);
        [DllImport(Dll, EntryPoint = "GDALRasterIO", CallingConvention = CallingConvention.Cdecl)] internal static extern int Read(IntPtr band, int mode, int x, int y, int width, int height, [Out] float[] data, int bufferWidth, int bufferHeight, int type, int pixelSpace, int lineSpace);
        [DllImport(Dll, EntryPoint = "GDALRasterIO", CallingConvention = CallingConvention.Cdecl)] internal static extern int Write(IntPtr band, int mode, int x, int y, int width, int height, byte[] data, int bufferWidth, int bufferHeight, int type, int pixelSpace, int lineSpace);
        [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)] internal static extern int GDALSetRasterColorInterpretation(IntPtr band, int interpretation);
    }
}
