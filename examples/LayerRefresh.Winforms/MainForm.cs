using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerRefresh.Winforms;

public sealed partial class MainForm : Form
{
    private readonly string[] _fillColors = ["#D8E5E1", "#D9C7A5", "#C7D7EA", "#D7C5DE"];
    private readonly string[] _outlineColors = ["#6F8883", "#A24A3D", "#356780", "#6F4D8C"];
    private readonly int[] _opacities = [210, 160, 110, 235];
    private int _fillIndex;
    private int _outlineIndex;
    private int _opacityIndex;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadLayer())
            return;

        geoKernelViewerControl.RefreshLayers();
        geoKernelViewerControl.FullExtent();
        UpdateStatus("Layer loaded. Change style, then press Refresh Layer.");
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "california", "california.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "LayerRefresh",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = CurrentStyle()
                }))
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{path}",
                "LayerRefresh",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, "California");

        return true;
    }

    private void changeFillButton_Click(object sender, EventArgs e)
    {
        _fillIndex = (_fillIndex + 1) % _fillColors.Length;
        ApplyPendingStyle("Fill changed. Press Refresh Layer to redraw.");
    }

    private void changeOutlineButton_Click(object sender, EventArgs e)
    {
        _outlineIndex = (_outlineIndex + 1) % _outlineColors.Length;
        ApplyPendingStyle("Outline changed. Press Refresh Layer to redraw.");
    }

    private void changeOpacityButton_Click(object sender, EventArgs e)
    {
        _opacityIndex = (_opacityIndex + 1) % _opacities.Length;
        ApplyPendingStyle("Opacity changed. Press Refresh Layer to redraw.");
    }

    private void refreshLayerButton_Click(object sender, EventArgs e)
    {
        if (!geoKernelViewerControl.SetLayerStyle(0, CurrentStyle()))
        {
            UpdateStatus("Style could not be applied.");
            return;
        }

        geoKernelViewerControl.RefreshLayers();
        UpdateStatus("Layer refreshed.");
    }

    private void fullExtentButton_Click(object sender, EventArgs e)
    {
        geoKernelViewerControl.FullExtent();
    }

    private void ApplyPendingStyle(string message)
    {
        UpdateStatus(message);
    }

    private GeoKernelLayerStyle CurrentStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = _fillColors[_fillIndex],
            FillOpacity = _opacities[_opacityIndex],
            LineColor = _outlineColors[_outlineIndex],
            LineWidth = _outlineIndex == 0 ? 0.9 : 1.6
        };
    }

    private void UpdateStatus(string message)
    {
        statusLabel.Text =
            $"{message} Fill: {_fillColors[_fillIndex]} | Outline: {_outlineColors[_outlineIndex]} | Opacity: {_opacities[_opacityIndex]}";
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
