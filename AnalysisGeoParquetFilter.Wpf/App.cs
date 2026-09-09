using System.Windows;

namespace GeoKernel.AnalysisGeoParquetFilter.Wpf;

public sealed class App : Application
{
    [STAThread]
    public static void Main()
    {
        var app = new App();
        app.Run(new MainWindow());
    }
}
