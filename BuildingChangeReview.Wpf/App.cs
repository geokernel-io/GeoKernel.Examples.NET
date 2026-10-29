using System.Windows;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf;
namespace GeoKernel.BuildingChangeReview.Wpf;

public sealed class App : Application
{
    [STAThread]
    public static void Main()
    {
        try
        {
            RuntimeSetup.Initialize(typeof(AITask).Assembly);
            new App().Run(new MainWindow());
        }
        catch (Exception e) { MessageBox.Show(e.Message, "Building change review"); }
    }
}
