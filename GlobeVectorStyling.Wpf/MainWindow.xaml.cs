using System.IO;
using System.Windows;
using System.Windows.Interop;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.GlobeVectorStyling.Wpf;

public partial class MainWindow : Window
{
    private const string LayerId = "buildings";
    private readonly CancellationTokenSource _closing = new();
    private GlobeCamera? _home;
    private string _baseColor = "#efb84c";
    private int _esriId = -1;
    private bool _loaded;

    public MainWindow()
    {
        InitializeComponent();
        GlobeView.StartupCamera = new GlobeCamera(2.1744, 41.4036, 0, 2500, 75.63, 34.38);
        GlobeView.StartupOptions = new GlobeOptions { NetworkEnabled = true, AtmosphereEnabled = true };
    }

    private async void GlobeReady(object? sender, EventArgs e)
    {
        try
        {
            var globe = GlobeView.Globe;
            _esriId = globe.GetBasemaps().FirstOrDefault(map => map.Name == "Esri Satellite")?.Id ?? -1;
            globe.SetBasemap(_esriId);
            globe.EventReceived += (_, item) =>
            {
                if (item.Type != "featurePicked" || !item.Data.GetProperty("hit").GetBoolean()) return;
                string id = item.Data.GetProperty("featureId").ToString();
                var info = globe.GetFeatureInfo(LayerId, id);
                Report($"Building {id}: " + string.Join(" | ", info.Attributes.Select(pair => $"{pair.Key}: {pair.Value}")));
            };
            string path = await SampleData.EnsureBuildingsAsync(new Progress<string>(Report), _closing.Token);
            _closing.Token.ThrowIfCancellationRequested();
            Report("Loading Sagrada Familia buildings…");
            await WaitAsync(globe.LoadVector(new GlobeVectorRequest(LayerId, path)
            {
                SourceCrs = "EPSG:25831",
                Style = new GlobeVectorStyle { CategoryField = "type", LabelField = "name", BaseColor = _baseColor, LabelSize = 12 }
            }));
            var stats = globe.GetLayerState(LayerId).Statistics;
            var fields = stats.GetProperty("fields").EnumerateArray().Select(field => field.GetString()!).ToArray();
            if (!fields.Contains("type") || !fields.Contains("name"))
                throw new InvalidDataException("Expected building fields 'type' and 'name' are missing.");
            Category.ItemsSource = new[] { "Uniform" }.Concat(fields).ToArray();
            LabelField.ItemsSource = fields;
            Category.SelectedItem = "type";
            LabelField.SelectedItem = "name";
            globe.SetLayerOpacity(LayerId, LayerOpacity.Value / 100.0);
            globe.FocusLayer(LayerId);
            _home = globe.Camera with
            {
                DistanceMetres = globe.Camera.DistanceMetres * 0.28,
                PitchDegrees = 1.32 * 180 / Math.PI,
                HeadingDegrees = 0.60 * 180 / Math.PI
            };
            globe.Camera = _home;
            globe.SetHomeCamera(_home);
            globe.SetInteractionMode(GlobeInteraction.Identify);
            _loaded = true;
            Toolbar.IsEnabled = true;
            Esri.IsEnabled = _esriId >= 0;
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
    }

    private async void ApplyStyle(object sender, RoutedEventArgs e) => await ApplyStyleAsync();

    private async Task ApplyStyleAsync()
    {
        if (!_loaded || !Toolbar.IsEnabled) return;
        Toolbar.IsEnabled = false;
        try
        {
            Report("Applying building style…");
            await WaitAsync(GlobeView.Globe.SetVectorStyle(LayerId, new GlobeVectorStyle
            {
                Fill = Fill.IsChecked == true,
                Labels = Labels.IsChecked == true,
                CategoryField = Category.SelectedIndex > 0 ? (string)Category.SelectedItem : "",
                LabelField = (string)LabelField.SelectedItem,
                BaseColor = _baseColor,
                LabelSize = 12
            }));
            ShowSummary();
        }
        catch (OperationCanceledException) { }
        catch (Exception error) { Report(error.Message); }
        finally { if (!_closing.IsCancellationRequested) Toolbar.IsEnabled = true; }
    }

    private async void ChooseColor(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.ColorDialog
        {
            Color = System.Drawing.ColorTranslator.FromHtml(_baseColor),
            FullOpen = true
        };
        var owner = new System.Windows.Forms.NativeWindow();
        owner.AssignHandle(new WindowInteropHelper(this).Handle);
        try
        {
            if (dialog.ShowDialog(owner) != System.Windows.Forms.DialogResult.OK) return;
            _baseColor = System.Drawing.ColorTranslator.ToHtml(dialog.Color);
        }
        finally { owner.ReleaseHandle(); }
        await ApplyStyleAsync();
    }

    private void OpacityChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_loaded) GlobeView.Globe.SetLayerOpacity(LayerId, e.NewValue / 100.0);
    }

    private void ZoomToBuildings(object sender, RoutedEventArgs e)
    {
        if (_home is not null) GlobeView.Globe.Camera = _home;
    }

    private void ToggleEsri(object sender, RoutedEventArgs e)
    {
        GlobeView.Globe.SetBasemap(Esri.IsChecked == true ? _esriId : -1);
        ShowSummary();
    }

    private async Task WaitAsync(ulong job)
    {
        var globe = GlobeView.Globe;
        try { await globe.WaitForJobAsync(job, _closing.Token); }
        finally { if (!globe.IsDisposed) globe.ForgetJob(job); }
    }

    private void ShowSummary()
    {
        var stats = GlobeView.Globe.GetLayerState(LayerId).Statistics;
        Report($"{stats.GetProperty("features")} buildings | {stats.GetProperty("outlineOnly")} outline-only | Click to identify | {GlobeView.Globe.GetAttribution()}");
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
