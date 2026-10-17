namespace GeoKernel.GlobeBuildings.Winforms;

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
            MessageBox.Show(error.Message, "Globe Buildings", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}


