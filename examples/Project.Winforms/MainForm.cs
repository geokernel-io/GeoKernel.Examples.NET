using GeoKernel.NET.WinForms;

namespace GeoKernel.Project.Winforms;

public sealed partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);
        LoadProject();
    }

    private void LoadProject()
    {
        var projectPath = Path.Combine(FindRepositoryRoot(), "data", "andalucia.geokernel");

        progressBar.Style = ProgressBarStyle.Marquee;
        progressBar.MarqueeAnimationSpeed = 30;
        progressBar.Value = 0;
        progressLabel.Text = "Loading andalucia.geokernel...";
        Application.DoEvents();

        if (!File.Exists(projectPath))
        {
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 0;
            progressLabel.Text = "Project file could not be found.";
            MessageBox.Show(
                this,
                $"Project file could not be found:{Environment.NewLine}{projectPath}",
                "Project",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        if (!geoKernelViewerControl.OpenProject(projectPath))
        {
            progressBar.Style = ProgressBarStyle.Blocks;
            progressBar.Value = 0;
            progressLabel.Text = "Project could not be loaded.";
            MessageBox.Show(
                this,
                $"Project could not be loaded:{Environment.NewLine}{projectPath}",
                "Project",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        progressBar.MarqueeAnimationSpeed = 0;
        progressBar.Style = ProgressBarStyle.Blocks;
        progressBar.Value = 100;
        progressLabel.Text = "Project loaded.";
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        geoKernelViewerControl.ActiveTool = tool;
        zoomRectButton.BackColor = tool == GeoKernelViewerTool.ZoomBox
            ? Color.FromArgb(200, 230, 255)
            : SystemColors.Control;
        panButton.BackColor = tool == GeoKernelViewerTool.Pan
            ? Color.FromArgb(200, 230, 255)
            : SystemColors.Control;
    }

    private void zoomInButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ZoomIn();
    }

    private void zoomOutButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.ZoomOut();
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.FullExtent();
    }

    private void zoomRectButton_Click(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.ZoomBox);
    }

    private void panButton_Click(object sender, EventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "data")))
                return directory.FullName;

            directory = directory.Parent;
        }

        return AppContext.BaseDirectory;
    }
}
