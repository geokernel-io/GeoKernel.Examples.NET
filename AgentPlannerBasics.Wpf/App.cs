using System.Windows;

namespace GeoKernel.AgentPlannerBasics.Wpf;

public sealed class App : Application
{
    // AgentPlannerBasics.Wpf.exe [--smoke]
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            new App().Run(new MainWindow(args.Contains("--smoke")));
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Agent Planner Basics", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
