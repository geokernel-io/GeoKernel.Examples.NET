using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.Sam3SegmentationInference.Winforms;

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
            MessageBox.Show(error.Message, "SAM3 Segmentation", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
