namespace GeoKernel.EmbeddingSearch.Winforms;

internal static class Program
{
    // SpatialEmbeddingSearch.Winforms.exe [--smoke]
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
            MessageBox.Show(e.Message, "Spatial embedding search", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}
