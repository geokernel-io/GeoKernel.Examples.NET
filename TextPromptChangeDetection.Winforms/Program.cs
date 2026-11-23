namespace GeoKernel.TextPromptChangeDetection.Winforms;

internal static class Program
{
    // TextPromptChangeDetection.Winforms.exe [--smoke]
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
            MessageBox.Show(e.Message, "Text-prompt change detection", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
