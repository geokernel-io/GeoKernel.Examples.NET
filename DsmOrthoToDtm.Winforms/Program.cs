using GeoKernel.Examples.DsmToDtm;

namespace GeoKernel.DsmOrthoToDtm.Winforms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            var options = OrthoOptions.Parse(args);
            Gdal.ConfigureRuntime();
            Application.Run(new MainForm(options));
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "DsmOrthoToDtm", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
