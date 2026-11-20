using System.Windows;
using GeoKernel.Examples.DsmToDtm;

namespace GeoKernel.DsmOrthoToDtm.Wpf;

public sealed class App : Application
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            var options = OrthoOptions.Parse(args);
            Gdal.ConfigureRuntime();
            new App().Run(new MainWindow(options));
        }
        catch (Exception e) { MessageBox.Show(e.Message, "DsmOrthoToDtm", MessageBoxButton.OK, MessageBoxImage.Error); }
    }
}
