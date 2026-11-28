namespace GeoKernel.AgentPlannerBasics.Winforms;

internal static class Program
{
    // AgentPlannerBasics.Winforms.exe [--smoke]
    [STAThread]
    private static void Main(string[] args)
    {
        ApplicationConfiguration.Initialize();
        try
        {
            Application.Run(new MainForm(args.Contains("--smoke")));
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message, "Agent Planner Basics", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
