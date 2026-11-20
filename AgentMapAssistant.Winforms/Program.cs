namespace GeoKernel.AgentMapAssistant.Winforms;

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
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Agent Map Assistant", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
