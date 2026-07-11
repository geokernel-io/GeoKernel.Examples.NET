using System.IO;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LayerZoomTo.Wpf;

public partial class MainWindow
{
    private readonly List<CityLayer> _cities = [];
    private bool _loading;

    public MainWindow()
    {
        InitializeComponent();
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {        
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadCityLayers())
            return;

        PopulateCityComboBox();
        viewerControl.FullExtent();
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
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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

        if (!viewerControl.AddLayerFile(
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
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return false;
        }

        var layer = viewerControl.GetLayerInfo(0);
        if (layer is not null)
            viewerControl.SetLayerName(layer.Index, city.Name);

        return true;
    }

    private void PopulateCityComboBox()
    {
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
    }

    private int LayerIndexByName(string name)
    {
        foreach (var layer in viewerControl.GetLayersInfo())
        {
            if (string.Equals(layer.Name, name, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(layer.DisplayText, name, StringComparison.OrdinalIgnoreCase))
            {
                return layer.Index;
            }
        }

        return -1;
    }

    private void CityComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_loading)
            return;

        var selected = cityComboBox.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(selected) || selected == "-")
        {
            viewerControl.FullExtent();
            statusText.Text = "Full extent";
            return;
        }

        var index = LayerIndexByName(selected);
        statusText.Text = index >= 0 && viewerControl.ZoomToLayer(index)
            ? $"Zoomed to {selected}"
            : $"Layer not found: {selected}";
    }

    private void UpdateStatus()
    {
        statusText.Text = $"Layers: {viewerControl.LayerCount} | Labels: NAME";
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
