using GeoKernel.Examples.DsmToDtm;

namespace GeoKernel.DsmToDtm.Winforms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            var options = DsmOptions.Parse(args);
            Gdal.ConfigureRuntime();
            Application.Run(new MainForm(options));
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "DsmToDtm", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
