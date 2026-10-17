using System.Windows;
using System.Windows.Threading;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GlobeHello.Wpf;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _statusTimer = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private int _esriId = -1;

    public MainWindow()
    {
        InitializeComponent();
        // Match Qt and WinForms: start blank and enable features independently.
        GlobeView.StartupOptions = new GlobeOptions { NetworkEnabled = true };
        _statusTimer.Tick += (_, _) => UpdateStatus();
    }

    private void GlobeReady(object? sender, EventArgs e)
    {
        _esriId = GlobeView.Globe.GetBasemaps().FirstOrDefault(map => map.Name == "Esri Satellite")?.Id ?? -1;
        Toolbar.IsEnabled = true;
        Esri.IsEnabled = _esriId >= 0;
        UpdateStatus();
        _statusTimer.Start();
    }

    private void ApplyOptions(object sender, RoutedEventArgs e)
    {
        GlobeView.Globe.Options = GlobeView.Globe.Options with
        {
            AtmosphereEnabled = Atmosphere.IsChecked == true,
            Basemap = Esri.IsChecked == true ? _esriId : -1,
            TerrainEnabled = Dem.IsChecked == true,
            SunlightEnabled = Sunlight.IsChecked == true
        };
        UpdateStatus();
    }

    private void UpdateStatus()
    {
        var options = GlobeView.Globe.Options;
        Atmosphere.IsChecked = options.AtmosphereEnabled;
        Esri.IsChecked = _esriId >= 0 && options.Basemap == _esriId;
        Dem.IsChecked = options.TerrainEnabled;
        Sunlight.IsChecked = options.SunlightEnabled;

        var messages = new List<string>();
        if (options.Basemap >= 0) messages.Add(GlobeView.Globe.GetAttribution());
        if (options.TerrainEnabled) messages.Add("Terrain: Esri / source contributors");
        messages.AddRange(GlobeView.Globe.GetState().Errors.Values.Where(error => !string.IsNullOrWhiteSpace(error)));
        Status.Text = messages.Count > 0
            ? string.Join(" | ", messages)
            : "Left: pan | Right / wheel: zoom | Middle / Ctrl: orbit";
    }

    protected override void OnClosed(EventArgs e)
    {
        _statusTimer.Stop();
        GlobeView.Dispose();
        base.OnClosed(e);
    }
}
