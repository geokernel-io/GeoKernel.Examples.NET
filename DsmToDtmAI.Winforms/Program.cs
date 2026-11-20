using GeoKernel.Examples.DsmToDtm;

namespace GeoKernel.DsmToDtmAI.Winforms;

internal static class Program
{
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            var options = AiOptions.Parse(args);
            Gdal.ConfigureRuntime();
            Application.Run(new MainForm(options));
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "DsmToDtmAI", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
