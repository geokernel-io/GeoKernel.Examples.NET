using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.Sen2srSuperResolution.Winforms;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            RuntimeSetup.Initialize(typeof(AITask).Assembly);
            Application.Run(new MainForm());
        }
        catch (Exception error)
        {
            MessageBox.Show(error.Message, "SEN2SR Super Resolution", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
