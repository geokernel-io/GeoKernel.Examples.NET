using System.Windows;

namespace GeoKernel.AgentMapAssistant.Wpf;

public sealed class App : Application
{
    [STAThread]
    public static void Main()
    {
        try
        {
            new App().Run(new MainWindow());
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Agent Map Assistant", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
