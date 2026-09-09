using GeoKernel.NET.WinForms;

namespace GeoKernel.ClearRenderer.Winforms;

public sealed partial class MainForm : Form
{
    private const string StateLayerName = "USA States";
    private int _statesLayerIndex = -1;

    public MainForm()
    {
        InitializeComponent();
    }

    private async void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.AddOpenStreetMapLayer();

        var path = await SampleData.EnsureFileAsync("usa_states_3857.zip", "usa_states_3857", "usa_states_3857.shp", "USA states", this, CreateSampleProgress());
        if (string.IsNullOrEmpty(path) || !LoadStatesLayer(path))
            return;

        ApplyCategorizedRenderer();
        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-16831516.0, 1856556.0, -4631023.0, 7472472.0);
        statusLabel.Text = "Categorized renderer applied. Use Clear Renderer to return to the default layer style.";
        downloadProgressBar.Visible = false;
    }

    private bool LoadStatesLayer(string path)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"States shapefile could not be found:{Environment.NewLine}{path}",
                "ClearRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(path, new GeoKernelLayerLoadOptions
            {
                ApplyDefaultStyle = true,
                DefaultStyle = DefaultStateStyle()
            }))
        {
            MessageBox.Show(
                this,
                $"States layer could not be loaded:{Environment.NewLine}{path}",
                "ClearRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is null)
        {
            MessageBox.Show(
                this,
                "Loaded states layer could not be inspected.",
                "ClearRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        _statesLayerIndex = layer.Index;
        geoKernelViewerControl.SetLayerName(_statesLayerIndex, StateLayerName);
        ApplyBaseStateStyle();
        return true;
    }

    private IProgress<SampleDataProgress> CreateSampleProgress() => new ControlProgress<SampleDataProgress>(this, p =>
    { statusLabel.Text = p.Message; downloadProgressBar.Visible = true; downloadProgressBar.Style = p.Percentage.HasValue ? ProgressBarStyle.Blocks : ProgressBarStyle.Marquee; if (p.Percentage.HasValue) downloadProgressBar.Value = Math.Clamp(p.Percentage.Value, 0, 100); });

    private void applyRendererButton_Click(object sender, EventArgs e)
    {
        ApplyCategorizedRenderer();
    }

    private void clearRendererButton_Click(object sender, EventArgs e)
    {
        if (_statesLayerIndex < 0)
            return;

        if (!geoKernelViewerControl.ClearLayerSymbolRenderer(_statesLayerIndex))
        {
            statusLabel.Text = "Renderer could not be cleared.";
            return;
        }

        ApplyBaseStateStyle();
        rendererStateLabel.Text = "Renderer: none, default layer style";
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
        statusLabel.Text = "Symbol renderer cleared. Layer is back to the default style.";
    }

    private void ApplyCategorizedRenderer()
    {
        if (_statesLayerIndex < 0)
            return;

        ApplyBaseStateStyle();

        if (!geoKernelViewerControl.ApplyLayerCategorizedRenderer(
                _statesLayerIndex,
                "STATE",
                GeoKernelColorRampNames.Unique,
                categoryLimit: 64))
        {
            MessageBox.Show(
                this,
                "Could not create categorized renderer from STATE field.",
                "ClearRenderer",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        rendererStateLabel.Text = "Renderer: categorized by STATE";
        geoKernelViewerControl.InvalidateRenderCache(clearTileCache: false, clearLayerCache: true);
        geoKernelViewerControl.RefreshLayers();
        statusLabel.Text = "Categorized renderer applied.";
    }

    private void ApplyBaseStateStyle()
    {
        if (_statesLayerIndex < 0)
            return;

        geoKernelViewerControl.SetLayerStyle(_statesLayerIndex, DefaultStateStyle());
    }

    private static GeoKernelLayerStyle DefaultStateStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 220,
            LineColor = "#536B68",
            LineWidth = 0.9
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
