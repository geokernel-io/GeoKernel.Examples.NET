namespace GeoKernel.FindSimilar.Winforms;

internal static class Program
{
    // SimilaritySearch.Winforms.exe [--smoke]
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
            MessageBox.Show(e.Message, "Find similar", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
