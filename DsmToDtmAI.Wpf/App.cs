using System.Windows;
using GeoKernel.Examples.DsmToDtm;

namespace GeoKernel.DsmToDtmAI.Wpf;

public sealed class App : Application
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            var options = AiOptions.Parse(args);
            Gdal.ConfigureRuntime();
            new App().Run(new MainWindow(options));
        }
        catch (Exception e) { MessageBox.Show(e.Message, "DsmToDtmAI", MessageBoxButton.OK, MessageBoxImage.Error); }
    }
}
