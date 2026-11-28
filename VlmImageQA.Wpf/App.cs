using System.Windows;

namespace GeoKernel.ImageQA.Wpf;

internal sealed class App : Application
{
    // VlmImageQA.Wpf.exe [--smoke]
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            new App().Run(new MainWindow(args.Contains("--smoke")));
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "What is in this area?", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
