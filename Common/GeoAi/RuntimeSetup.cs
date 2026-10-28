using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;

namespace GeoKernel.Examples.GeoAi;

internal static class RuntimeSetup
{
    // NuGet deploys the native runtime beside the application. An explicit override
    // remains available for a separately deployed compatible GPU runtime.
    public static void Initialize(Assembly controlsAssembly)
    {
        string directory = Environment.GetEnvironmentVariable("GEOKERNEL_NATIVE_DIR") ?? AppContext.BaseDirectory;
        directory = Path.GetFullPath(directory);
        if (!File.Exists(Path.Combine(directory, "GeoKernel.AI.dll")))
            throw new DirectoryNotFoundException("Restore/build GeoKernel.NET 1.5.39; the packaged GeoKernel.AI.dll is missing.");
        Environment.SetEnvironmentVariable("PATH", directory + Path.PathSeparator + Environment.GetEnvironmentVariable("PATH"));
        foreach (var (variable, folder) in new[] { ("GDAL_DATA", "gdal-data"), ("PROJ_DATA", "proj-data"), ("GDAL_DRIVER_PATH", "gdalplugins") })
        {
            string path = Path.Combine(directory, folder);
            if (Directory.Exists(path)) Environment.SetEnvironmentVariable(variable, path);
        }
        NativeLibrary.SetDllImportResolver(controlsAssembly, (name, assembly, _) =>
            name.StartsWith("GeoKernel.", StringComparison.OrdinalIgnoreCase) && File.Exists(Path.Combine(directory, name))
                ? NativeLibrary.Load(Path.Combine(directory, name), assembly, DllImportSearchPath.UseDllDirectoryForDependencies)
                : IntPtr.Zero);
        NativeLibrary.SetDllImportResolver(typeof(RuntimeSetup).Assembly, (name, assembly, _) =>
            name == "gdal.dll"
                ? NativeLibrary.Load(Path.Combine(directory, name), assembly, DllImportSearchPath.UseDllDirectoryForDependencies)
                : IntPtr.Zero);
    }
}
