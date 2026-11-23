using System.Windows;

namespace GeoKernel.TextPromptChangeDetection.Wpf;

internal sealed class App : Application
{
    // TextPromptChangeDetection.Wpf.exe [--smoke]
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            new App().Run(new MainWindow(args.Contains("--smoke")));
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Text-prompt change detection", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
