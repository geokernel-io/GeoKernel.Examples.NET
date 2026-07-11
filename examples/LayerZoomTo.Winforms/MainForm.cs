using GeoKernel.NET.WinForms;

namespace GeoKernel.LayerZoomTo.Winforms;

public sealed partial class MainForm : Form
{
    private readonly List<CityLayer> _cities = [];
    private bool _loading;

    public MainForm()
    {
        InitializeComponent();
    }

    private void MainForm_Shown(object sender, EventArgs e)
    {        
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadCityLayers())
            return;

        _loading = true;
        try
        {
            cityComboBox.Items.Clear();
            cityComboBox.Items.Add("-");
            foreach (var city in _cities)
                cityComboBox.Items.Add(city.Name);

            cityComboBox.SelectedIndex = 0;
        }
        finally
        {
            _loading = false;
        }

        geoKernelViewerControl.FullExtent();
        UpdateStatus();
    }

    private bool LoadCityLayers()
    {
        var cityDirectory = Path.Combine(FindRepositoryRoot(), "assets", "data", "california", "cities");
        if (!Directory.Exists(cityDirectory))
        {
            MessageBox.Show(
                this,
                $"City data directory could not be found:{Environment.NewLine}{cityDirectory}",
                "LayerZoomTo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var palette = new[]
        {
            "#BFD6E5",
            "#C9D5C9",
            "#D8CDA7",
            "#D7B79B",
            "#D6C6E3",
            "#B9D8C5"
        };

        var files = Directory
            .EnumerateFiles(cityDirectory, "*.shp")
            .OrderBy(Path.GetFileName, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        for (var i = 0; i < files.Length; ++i)
        {
            var city = new CityLayer(DisplayNameFromFileName(files[i]), files[i], palette[i % palette.Length]);
            if (!AddLayer(city))
                return false;

            _cities.Add(city);
        }

        return _cities.Count > 0;
    }

    private bool AddLayer(CityLayer city)
    {
        var style = new GeoKernelLayerStyle
        {
            FillColor = city.FillColor,
            FillOpacity = 150,
            LineColor = "#5F7772",
            LineWidth = 0.8,
            ShowLabels = true,
            LabelFontSize = 12,
            LabelAllowOverlap = true,
            LabelAvoidObstacles = false,
            LabelField = "NAME",
            LabelColor = "#000000",
            LabelHaloEnabled = true,
            LabelHaloColor = "#FFFF00",
            LabelHaloWidth = 2.0
        };

        if (!geoKernelViewerControl.AddLayerFile(
                city.Path,
                new GeoKernelLayerLoadOptions
                {
                    ApplyDefaultStyle = true,
                    DefaultStyle = style
                }))
        {
            MessageBox.Show(
                this,
                $"Layer could not be loaded:{Environment.NewLine}{city.Path}",
                "LayerZoomTo",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return false;
        }

        var layer = geoKernelViewerControl.GetLayerInfo(0);
        if (layer is not null)
            geoKernelViewerControl.SetLayerName(layer.Index, city.Name);

        return true;
    }

    private int LayerIndexByName(string name)
    {
        foreach (var layer in geoKernelViewerControl.GetLayersInfo())
        {
            if (string.Equals(layer.Name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(layer.DisplayText, name, StringComparison.OrdinalIgnoreCase))
            {
                return layer.Index;
            }
        }

        return -1;
    }

    private void cityComboBox_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_loading)
            return;

        var selected = cityComboBox.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(selected) || selected == "-")
        {
            geoKernelViewerControl.FullExtent();
            statusLabel.Text = "Full extent";
            return;
        }

        var index = LayerIndexByName(selected);
        if (index >= 0 && geoKernelViewerControl.ZoomToLayer(index))
            statusLabel.Text = $"Zoomed to {selected}";
        else
            statusLabel.Text = $"Layer not found: {selected}";
    }

    private void UpdateStatus()
    {
        statusLabel.Text = $"Layers: {geoKernelViewerControl.LayerCount} | Labels: NAME";
    }

    private static string DisplayNameFromFileName(string path)
    {
        var words = Path.GetFileNameWithoutExtension(path)
            .Replace('_', ' ')
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        return string.Join(" ", words.Select(word =>
            word.Length == 0 ? word : char.ToUpperInvariant(word[0]) + word[1..]));
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

    private sealed record CityLayer(string Name, string Path, string FillColor);
}
