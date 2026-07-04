using System.IO;
using System.Windows;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LabelRotation.Wpf;

public partial class MainWindow
{
    private int _worldLayerIndex = -1;
    private bool _loading = true;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        viewerControl.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadLayer())
            return;

        ApplyRotationStyle();
        viewerControl.ViewExtent = new GeoKernelExtent(-180.0, -58.0, 180.0, 82.0);
        _loading = false;
        statusText.Text = "Labels use labelRotationDegrees.";
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", Title);
            return false;
        }

        if (!viewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = RotationStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Title);
            return false;
        }

        viewerControl.SetLayerName(0, "World - label rotation");
        _worldLayerIndex = viewerControl.GetLayerInfoByName("World - label rotation")?.Index ?? 0;
        return true;
    }

    private void RotationControl_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        rotationText.Text = $"{rotationSlider.Value:0.0} deg";
        if (_loading)
            return;

        ApplyRotationStyle();
        statusText.Text = $"Label rotation: {rotationSlider.Value:0.0} degrees";
    }

    private void ResetRotation_Click(object sender, RoutedEventArgs e)
    {
        rotationSlider.Value = 0;
        RotationControl_Changed(sender, new RoutedPropertyChangedEventArgs<double>(0, 0));
    }

    private void ApplyRotationStyle()
    {
        if (_worldLayerIndex < 0)
            return;

        viewerControl.SetLayerStyle(_worldLayerIndex, RotationStyle());
        viewerControl.InvalidateRenderCache(clearTileCache: true, clearLayerCache: true);
        viewerControl.RefreshLayers();
    }

    private GeoKernelLayerStyle RotationStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 215,
            LineColor = "#6F8380",
            LineWidth = 0.8,
            ShowLabels = true,
            LabelField = "COUNTRY",
            LabelFontSize = 12.0,
            LabelColor = "#253238",
            LabelHaloEnabled = true,
            LabelHaloColor = "#FFFFFF",
            LabelHaloWidth = 2.0,
            LabelRotationDegrees = rotationSlider.Value
        };
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "assets", "data")))
                return directory.FullName;
            directory = directory.Parent;
        }
        return AppContext.BaseDirectory;
    }
}
