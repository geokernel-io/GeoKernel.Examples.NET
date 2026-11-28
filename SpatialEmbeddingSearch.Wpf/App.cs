using System.Windows;

namespace GeoKernel.EmbeddingSearch.Wpf;

public sealed class App : Application
{
    // SpatialEmbeddingSearch.Wpf.exe [--smoke]
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            new App().Run(new MainWindow(args.Contains("--smoke")));
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Spatial embedding search", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
