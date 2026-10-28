using System.Reflection;
#if GEOAI_WPF
using System.Windows;
using System.Windows.Media.Imaging;
#else
using System.Drawing;
using System.Windows.Forms;
#endif

namespace GeoKernel.Examples.GeoAi;

internal static class ExampleIcon
{
#if GEOAI_WPF
    public static void Apply(Window window)
    {
        using var stream = Open();
        window.Icon = BitmapFrame.Create(stream, BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
    }
#else
    public static void Apply(Form window)
    {
        using var stream = Open();
        using var icon = new Icon(stream);
        window.Icon = (Icon)icon.Clone();
    }
#endif

    private static System.IO.Stream Open() =>
        typeof(ExampleIcon).Assembly.GetManifestResourceStream("GeoKernel.Examples.AppIcon.ico")
        ?? throw new InvalidOperationException("Embedded GeoKernel application icon is missing.");
}
