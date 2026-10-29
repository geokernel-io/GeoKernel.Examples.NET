using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;
namespace GeoKernel.BuildingChangeReview.Winforms;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        try
        {
            RuntimeSetup.Initialize(typeof(AITask).Assembly);
            Application.Run(new MainForm());
        }
        catch (Exception e) { MessageBox.Show(e.Message, "Building change review"); }
    }
}
