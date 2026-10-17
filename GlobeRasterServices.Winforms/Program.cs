namespace GeoKernel.GlobeRasterServices.Winforms;

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
            MessageBox.Show(error.Message, "Globe Raster Services", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}



