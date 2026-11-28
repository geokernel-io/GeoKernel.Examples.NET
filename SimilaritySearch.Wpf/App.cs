using System.Windows;

namespace GeoKernel.FindSimilar.Wpf;

internal sealed class App : Application
{
    // SimilaritySearch.Wpf.exe [--smoke]
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            new App().Run(new MainWindow(args.Contains("--smoke")));
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Find similar", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
