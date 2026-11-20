using System.Windows;
using GeoKernel.Examples.DsmToDtm;

namespace GeoKernel.DsmToDtmProfile.Wpf;

public sealed class App : Application
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            var options = DsmOptions.Parse(args);
            Gdal.ConfigureRuntime();
            new App().Run(new MainWindow(options));
        }
        catch (Exception e) { MessageBox.Show(e.Message, "DsmToDtmProfile", MessageBoxButton.OK, MessageBoxImage.Error); }
    }
}
