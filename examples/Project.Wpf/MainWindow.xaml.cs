using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Threading;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.Project.Wpf;

public partial class MainWindow
{
    private int _maxProgressValue;
    private bool _projectLoaded;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_ContentRendered(object? sender, EventArgs e)
    {
        if (_projectLoaded)
            return;

        _projectLoaded = true;
        viewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        SetTool(GeoKernelViewerTool.Pan);
        LoadProject();
    }

    private void LoadProject()
    {
        var projectPath = Path.Combine(FindRepositoryRoot(), "data", "andalucia.geokernel");

        _maxProgressValue = 0;
        SetProgress(0, "Loading andalucia.geokernel...");

        if (!File.Exists(projectPath))
        {
            ShowProjectError("Project file could not be found.", projectPath);
            return;
        }

        if (!viewerControl.OpenProject(projectPath, UpdateProjectProgress))
        {
            ShowProjectError("Project could not be loaded.", projectPath);
            return;
        }

        SetProgress(100, "Project loaded.");
    }

    private void UpdateProjectProgress(GeoKernelLayerLoadProgress progress)
    {
        if (progress.Progress is int value)
        {
            value = Math.Clamp(value, 0, 100);
            _maxProgressValue = Math.Max(_maxProgressValue, value);
        }

        SetProgress(
            _maxProgressValue,
            string.IsNullOrWhiteSpace(progress.Status)
                ? progressLabel.Text
                : progress.Status);
    }

    private void ShowProjectError(string message, string path)
    {
        SetProgress(0, message);
        MessageBox.Show(
            this,
            $"{message}{Environment.NewLine}{path}",
            "Project",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void SetProgress(int value, string text)
    {
        progressBar.IsIndeterminate = false;
        progressBar.Value = Math.Clamp(value, 0, 100);
        progressLabel.Text = text;
        PumpUi();
    }

    private void PumpUi()
    {
        progressBar.UpdateLayout();
        progressLabel.UpdateLayout();
        Dispatcher.Invoke(static () => { }, DispatcherPriority.Render);
    }

    private void ZoomIn_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ZoomIn();
    }

    private void ZoomOut_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ZoomOut();
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.FullExtent();
    }

    private void ZoomRect_Click(object sender, RoutedEventArgs e)
    {
        SetTool(GeoKernelViewerTool.ZoomBox);
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        SetTool(GeoKernelViewerTool.Pan);
    }

    private void SetTool(GeoKernelViewerTool tool)
    {
        viewerControl.ActiveTool = tool;
        zoomRectButton.IsChecked = tool == GeoKernelViewerTool.ZoomBox;
        panButton.IsChecked = tool == GeoKernelViewerTool.Pan;
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
