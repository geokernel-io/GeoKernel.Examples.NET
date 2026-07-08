using GeoKernel.NET.WinForms;

namespace GeoKernel.ScaleBasedLayerVisibility.Winforms;

public sealed partial class MainForm : Form
{
    private double _currentScale;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {
        geoKernelViewerControl.MapBackgroundColor = Color.FromArgb(244, 246, 245);
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        geoKernelViewerControl.ZoomChanged += (_, e) =>
        {
            _currentScale = e.ZoomScale;
            RefreshLayerList();
        };
        geoKernelViewerControl.LayersChanged += (_, _) => RefreshLayerList();

        if (!LoadLayers())
            return;

        geoKernelViewerControl.ViewExtent = new GeoKernelExtent(-151.2, 16.4, -41.6, 55.6);
        RefreshLayerList();
    }

    private bool LoadLayers()
    {
        var dataDirectory = Path.Combine(FindRepositoryRoot(), "assets", "data");

        return AddLayer(
                "World",
                Path.Combine(dataDirectory, "world_4326.shp"),
                WorldStyle(),
                minVisibleScale: 0.0,
                maxVisibleScale: 11.0)
            && AddLayer(
                "States",
                Path.Combine(dataDirectory, "usa_states_3857.shp"),
                StatesStyle(),
                minVisibleScale: 5.0,
                maxVisibleScale: 45.0)
            && AddLayer(
                "Cities",
                Path.Combine(dataDirectory, "usa_cities_4326.kml"),
                CitiesStyle(),
                minVisibleScale: 28.0,
                maxVisibleScale: 0.0);
    }

    private bool AddLayer(
        string name,
        string path,
        GeoKernelLayerStyle style,
        double minVisibleScale,
        double maxVisibleScale)
    {
        if (!File.Exists(path))
        {
            MessageBox.Show(
                this,
                $"Layer file could not be found:{Environment.NewLine}{path}",
                "ScaleBasedLayerVisibility",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        if (!geoKernelViewerControl.AddLayerFile(
                path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                }))
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{path}",
                "ScaleBasedLayerVisibility",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is null)
            return false;

        geoKernelViewerControl.SetLayerName(layer.Index, name);
        geoKernelViewerControl.SetLayerVisibleScaleRange(layer.Index, minVisibleScale, maxVisibleScale);
        geoKernelViewerControl.RefreshLayers();
        return true;
    }

    private void RefreshLayerList()
    {
        scaleLabel.Text = $"Current scale: {ScaleText(_currentScale)} px/map unit";

        layerListBox.BeginUpdate();
        try
        {
            layerListBox.Items.Clear();
            foreach (var layer in geoKernelViewerControl.GetLayersInfo())
                layerListBox.Items.Add(LayerListText(layer));
        }
        finally
        {
            layerListBox.EndUpdate();
        }
    }

    private string LayerListText(GeoKernelLayerInfo layer)
    {
        var visibleAtScale = IsVisibleAtScale(layer, _currentScale);
        return $"{(visibleAtScale ? "[x]" : "[ ]")} [{ScaleText(layer.MinVisibleScale),5} - {ScaleText(layer.MaxVisibleScale),5}] {layer.DisplayText}";
    }

    private static bool IsVisibleAtScale(GeoKernelLayerInfo layer, double scale)
    {
        if (!layer.Visible)
            return false;

        if (layer.MinVisibleScale > 0.0 && scale < layer.MinVisibleScale)
            return false;

        if (layer.MaxVisibleScale > 0.0 && scale > layer.MaxVisibleScale)
            return false;

        return true;
    }

    private static GeoKernelLayerStyle WorldStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 225,
            LineColor = "#7B918D",
            LineWidth = 0.8
        };
    }

    private static GeoKernelLayerStyle StatesStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#A9C8DB",
            FillOpacity = 135,
            LineColor = "#356780",
            LineWidth = 1.1
        };
    }

    private static GeoKernelLayerStyle CitiesStyle()
    {
        return new GeoKernelLayerStyle
        {
            PointColor = "#D95D39",
            PointSize = 7.0,
            LineColor = "#873A24",
            LineWidth = 1.0
        };
    }

    private static string ScaleText(double value)
    {
        if (value <= 0.0)
            return "-";

        return value < 10.0 ? value.ToString("0.00") : value.ToString("0");
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
