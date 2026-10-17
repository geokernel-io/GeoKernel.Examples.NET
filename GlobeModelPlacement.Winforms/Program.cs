namespace GeoKernel.GlobeModelPlacement.Winforms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            Application.Run(new MainForm());
        }
        catch (Exception error)
        {
            MessageBox.Show(error.Message, "Globe Model Placement", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}



