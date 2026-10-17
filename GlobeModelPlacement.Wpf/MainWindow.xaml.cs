using System.Globalization;
using System.Windows;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GlobeModelPlacement.Wpf;

public partial class MainWindow : Window
{
    private const string LayerId = "sagrada-familia";
    private readonly CancellationTokenSource _closing = new();
    private NET.Wpf.Controls.GlobeModelPlacement _placement = new(LayerId, 2.1744, 41.4036) { Name = "Sagrada Familia", Height = 14.089 };
    private string? _modelPath;
    private int _esriId = -1;
    private bool _present;

    public MainWindow()
    {
        InitializeComponent();
        GlobeView.StartupCamera = ModelCamera();
        GlobeView.StartupOptions = new GlobeOptions { NetworkEnabled = true, AtmosphereEnabled = true };
        ResetInputs();
    }

    private async void GlobeReady(object? sender, EventArgs e) => await LoadModelAsync();
    private void ZoomToModel(object sender, RoutedEventArgs e) => FocusModel();
    private async void RotateModel(object sender, RoutedEventArgs e) =>
        await PlaceModelAsync(_placement with { Heading = (_placement.Heading + 30) % 360 });
    private async void RestoreModel(object sender, RoutedEventArgs e) => await LoadModelAsync();

    private async void ApplyPlacement(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(ModelScale.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out double scale) ||
            !double.IsFinite(scale) || scale < .01 || scale > 10 ||
            !double.TryParse(ModelHeight.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out double height) ||
            !double.IsFinite(height) || height < -100 || height > 10000)
        {
            Report("Enter a scale from 0.01 to 10 and an anchor height from -100 to 10000 m using your regional decimal separator.");
            return;
        }
        await PlaceModelAsync(_placement with { Scale = scale, Height = height });
    }

    private void RemoveModel(object sender, RoutedEventArgs e)
    {
        GlobeView.Globe.RemoveLayer(LayerId);
        _present = false;
        UpdateControls();
        ShowSummary();
    }

    private void ToggleAtmosphere(object sender, RoutedEventArgs e) =>
        GlobeView.Globe.Options = GlobeView.Globe.Options with { AtmosphereEnabled = Atmosphere.IsChecked == true };

    private void ToggleEsri(object sender, RoutedEventArgs e)
    {
        GlobeView.Globe.SetBasemap(Esri.IsChecked == true ? _esriId : -1);
        ShowSummary();
    }

    private void ResetInputs()
    {
        ModelScale.Text = _placement.Scale.ToString("0.##", CultureInfo.CurrentCulture);
        ModelHeight.Text = _placement.Height.ToString("0.###", CultureInfo.CurrentCulture);
    }

    private GlobeCamera ModelCamera() => new(_placement.Longitude, _placement.Latitude,
        _placement.Height + 65 * _placement.Scale, 550 * _placement.Scale, 1.15 * 180 / Math.PI, .6 * 180 / Math.PI);

    private void FocusModel()
    {
        GlobeView.Globe.Camera = ModelCamera();
        GlobeView.Globe.SetHomeCamera(ModelCamera());
    }

    private async Task LoadModelAsync()
    {
        Toolbar.IsEnabled = false;
        try
        {
            var globe = GlobeView.Globe;
            if (_modelPath is null)
            {
                _esriId = globe.GetBasemaps().FirstOrDefault(map => map.Name == "Esri Satellite")?.Id ?? -1;
                globe.SetBasemap(_esriId);
                _modelPath = await SampleData.EnsureModelAsync(new Progress<string>(Report), _closing.Token);
            }
            _closing.Token.ThrowIfCancellationRequested();
            Report("Loading Sagrada Familia model…");
            await WaitAsync(globe.LoadModel(new GlobeModelRequest(LayerId, _modelPath, _placement.Longitude, _placement.Latitude)
            {
                Name = _placement.Name, Height = _placement.Height, Scale = _placement.Scale, Heading = _placement.Heading
            }));
            _present = true;
            FocusModel();
            await WaitForUploadAsync();
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
        finally { if (!_closing.IsCancellationRequested) UpdateControls(); }
    }

    private async Task PlaceModelAsync(NET.Wpf.Controls.GlobeModelPlacement next)
    {
        if (!_present || !Toolbar.IsEnabled) return;
        Toolbar.IsEnabled = false;
        try
        {
            Report("Updating model placement…");
            await WaitAsync(GlobeView.Globe.SetModelPlacement(next));
            _placement = next;
            FocusModel();
            await WaitForUploadAsync();
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
        finally
        {
            if (!_closing.IsCancellationRequested)
            {
                ResetInputs();
                UpdateControls();
            }
        }
    }

    private async Task WaitAsync(ulong job)
    {
        var globe = GlobeView.Globe;
        try { await globe.WaitForJobAsync(job, _closing.Token); }
        finally { if (!globe.IsDisposed) globe.ForgetJob(job); }
    }

    private async Task WaitForUploadAsync()
    {
        Report("Uploading model to the GPU…");
        while (true)
        {
            _closing.Token.ThrowIfCancellationRequested();
            var state = GlobeView.Globe.GetState();
            if (state.Errors.TryGetValue("scene", out string? error) && !string.IsNullOrWhiteSpace(error))
                throw new InvalidOperationException(error);
            if (!state.SceneUploadPending && state.SceneVerticesDrawn > 0) return;
            await Task.Delay(50, _closing.Token);
        }
    }

    private void UpdateControls()
    {
        Toolbar.IsEnabled = true;
        Rotate.IsEnabled = ModelScale.IsEnabled = ModelHeight.IsEnabled = Apply.IsEnabled = Remove.IsEnabled = _present;
        Restore.IsEnabled = !_present;
        Esri.IsEnabled = _esriId >= 0;
    }

    private void ShowSummary() => Report($"Sagrada Familia | {(_present ? "Approximate placement" : "Model removed — click Restore")} | Model: PeeJaa | {GlobeView.Globe.GetAttribution()}");

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
