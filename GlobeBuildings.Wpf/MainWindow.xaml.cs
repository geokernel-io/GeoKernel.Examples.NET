using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GlobeBuildings.Wpf;

public partial class MainWindow : Window
{
    private const string LayerId = "buildings";
    private const string BuildingColor = "#e8d6b4";
    private readonly CancellationTokenSource _closing = new();
    private GlobeCamera? _home;
    private int _esriId = -1;
    private bool _loaded;

    public MainWindow()
    {
        InitializeComponent();
        GlobeView.StartupCamera = new GlobeCamera(2.1744, 41.4036, 0, 2500, 75.63, 34.38);
        GlobeView.StartupOptions = new GlobeOptions { NetworkEnabled = true, AtmosphereEnabled = true };
    }

    private async void GlobeReady(object? sender, EventArgs e) => await LoadBuildingsAsync();

    private async void ToggleExtrusion(object sender, RoutedEventArgs e) => await ToggleExtrusionAsync();

    private void ZoomToBuildings(object sender, RoutedEventArgs e)
    {
        if (_home is not null) GlobeView.Globe.Camera = _home;
    }

    private void ToggleAtmosphere(object sender, RoutedEventArgs e) =>
        GlobeView.Globe.Options = GlobeView.Globe.Options with { AtmosphereEnabled = Atmosphere.IsChecked == true };

    private void ToggleEsri(object sender, RoutedEventArgs e)
    {
        GlobeView.Globe.SetBasemap(Esri.IsChecked == true ? _esriId : -1);
        ShowSummary();
    }
    private async Task LoadBuildingsAsync()
    {
        try
        {
            var globe = GlobeView.Globe;
            _esriId = globe.GetBasemaps().FirstOrDefault(map => map.Name == "Esri Satellite")?.Id ?? -1;
            globe.SetBasemap(_esriId);
            string path = await SampleData.EnsureBuildingsAsync(new Progress<string>(Report), _closing.Token);
            _closing.Token.ThrowIfCancellationRequested();
            Report("Loading Sagrada Familia buildings…");
            await WaitAsync(globe.LoadVector(new GlobeVectorRequest(LayerId, path)
            {
                SourceCrs = "EPSG:25831",
                Style = new GlobeVectorStyle { Fill = true, Labels = false, BaseColor = BuildingColor }
            }));
            globe.SetLayerOpacity(LayerId, 0.65);
            globe.FocusLayer(LayerId);
            _home = globe.Camera with
            {
                DistanceMetres = globe.Camera.DistanceMetres * 0.28,
                PitchDegrees = 1.32 * 180 / Math.PI,
                HeadingDegrees = 0.60 * 180 / Math.PI
            };
            globe.Camera = _home;
            globe.SetHomeCamera(_home);
            _loaded = true;
            Toolbar.IsEnabled = true;
            Esri.IsEnabled = _esriId >= 0;
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
    }

    private async Task ToggleExtrusionAsync()
    {
        if (!_loaded || !Toolbar.IsEnabled) return;
        Toolbar.IsEnabled = false;
        try
        {
            Report(Extrude.IsChecked == true ? "Extruding buildings…" : "Restoring footprints…");
            // Match Qt: no DEM in this example, so bases lie on the reference sphere.
            await WaitAsync(GlobeView.Globe.ConfigureBuildings(new GlobeBuildingsRequest(LayerId, GlobeBuildingBase.ReferenceSphere)
            {
                Enabled = Extrude.IsChecked == true,
                DefaultHeight = (double)DisplayHeight.Value,
                BuildingColors = new GlobeBuildingColors { Mode = "uniform", Fallback = BuildingColor }
            }));
            if (Extrude.IsChecked == true)
            {
                Report("Uploading 3D buildings to the GPU…");
                // A completed geometry job does not mean the incremental GPU upload is finished.
                while (true)
                {
                    _closing.Token.ThrowIfCancellationRequested();
                    var render = GlobeView.Globe.GetState();
                    if (render.Errors.TryGetValue("scene", out string? error) && !string.IsNullOrWhiteSpace(error))
                        throw new InvalidOperationException(error);
                    if (!render.SceneUploadPending && render.SceneVerticesDrawn > 0) break;
                    await Task.Delay(50, _closing.Token);
                }
            }
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
        finally
        {
            if (!_closing.IsCancellationRequested)
            {
                Extrude.IsChecked = GlobeView.Globe.GetLayerState(LayerId).Extruded;
                DisplayHeight.IsEnabled = Extrude.IsChecked != true;
                Toolbar.IsEnabled = true;
            }
        }
    }

    private async Task WaitAsync(ulong job)
    {
        var globe = GlobeView.Globe;
        try { await globe.WaitForJobAsync(job, _closing.Token); }
        finally { if (!globe.IsDisposed) globe.ForgetJob(job); }
    }

    private void ShowSummary()
    {
        var layer = GlobeView.Globe.GetLayerState(LayerId);
        var stats = layer.Statistics;
        string message = layer.Extruded
            ? $"{stats.GetProperty("extrudedPolygons")} extruded polygons | Display height {DisplayHeight.Value} m (not measured) | Click Extrude for footprints"
            : $"{stats.GetProperty("features")} buildings | {stats.GetProperty("outlineOnly")} outline-only | Click Extrude for 3D";
        Report($"{message} | {GlobeView.Globe.GetAttribution()}");
    }

    private void Report(string message)
    {
        if (!_closing.IsCancellationRequested) Status.Text = message;
    }

    protected override void OnClosed(EventArgs e)
    {
        _closing.Cancel();
        GlobeView.Dispose();
        base.OnClosed(e);
    }
}
