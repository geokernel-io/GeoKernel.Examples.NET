namespace GeoKernel.RouteAnimation.Wpf;

public sealed class App : System.Windows.Application
{
    [STAThread]
    public static void Main()
    {
        var app = new App();
        app.Run(new MainWindow());
    }
}
