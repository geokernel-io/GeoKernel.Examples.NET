namespace GeoKernel.ImageQA.Winforms;

internal static class Program
{
    // VlmImageQA.Winforms.exe [--smoke]
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
            MessageBox.Show(e.Message, "What is in this area?", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
