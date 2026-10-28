using System.Windows;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf;

namespace GeoKernel.Sen2srSuperResolution.Wpf;

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
        catch (Exception error) { MessageBox.Show(error.Message, "SEN2SR Super Resolution"); }
    }
}
