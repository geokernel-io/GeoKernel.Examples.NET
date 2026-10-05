using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace GeoKernel.ModelPlacement.Wpf;

public partial class MainWindow : Window
{
    private CancellationTokenSource? _load;
    private bool _closed;
    private bool _started;
    private bool _modelReady;

    public MainWindow() => InitializeComponent();

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_started) return;
        _started = true;
        await LoadSampleAsync();
    }

    private async void Apply_Click(object sender, RoutedEventArgs e) => await LoadSampleAsync();
    private void Cancel_Click(object sender, RoutedEventArgs e) => _load?.Cancel();
    private void Focus_Click(object sender, RoutedEventArgs e) => Viewer.FocusModel();
    private void Reset_Click(object sender, RoutedEventArgs e) => Viewer.ResetCamera();
    private void Model_Changed(object sender, RoutedEventArgs e)
    {
        if (_modelReady && !_closed) Viewer.SetModelVisible(ModelVisible.IsChecked == true);
    }
    private void Imagery_Changed(object sender, RoutedEventArgs e)
    {
        if (Viewer != null && !_closed) Viewer.ImageryVisible = Imagery.IsChecked == true;
    }
    private void Height_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Viewer != null && !_closed) Viewer.HeightScale = (float)e.NewValue;
    }
    private void Window_Closed(object? sender, EventArgs e)
    {
        _closed = true;
        _load?.Cancel();
        Viewer.Dispose();
    }

    private double[] ReadPlacement()
    {
        (TextBox Input, double Min, double Max)[] fields = [
            (Longitude, -180, 180), (Latitude, -90, 90), (Offset, -500, 1000),
            (Heading, -180, 180), (Pitch, -180, 180), (Roll, -180, 180), (Scale, .01, 100)
        ];
        return fields.Select(field =>
        {
            string text = field.Input.Text.Trim().Replace(',', '.');
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double value) ||
                !double.IsFinite(value) || value < field.Min || value > field.Max)
            {
                field.Input.Focus();
                field.Input.SelectAll();
                throw new ArgumentException($"{field.Input.Name}: enter a number between {field.Min} and {field.Max}.");
            }
            return value;
        }).ToArray();
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null || _closed) return;
        double[] placement;
        try { placement = ReadPlacement(); }
        catch (ArgumentException error)
        {
            MessageBox.Show(this, error.Message, "Placement", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        ApplyButton.IsEnabled = PlacementFields.IsEnabled = false;
        CancelButton.IsEnabled = true;
        Progress.Visibility = Visibility.Visible;
        try
        {
            var progress = new Progress<string>(message => { if (!_closed) StatusText.Text = message; });
            string dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            string photo = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            string model = await SampleData.EnsureArchiveAsync("sagrada_familia_3d_model",
                "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/sagrada_familia_3d_model.zip",
                "model.gltf", progress, cancellation.Token);
            string geoid = await SampleData.EnsureArchiveAsync("egm08d595",
                "https://icgc-web-pro.s3.eu-central-1.amazonaws.com/produccio/s3fs-public/EGM08D595_19839.zip",
                "cat80000.gr", progress, cancellation.Token);
            if (_closed) return;
            StatusText.Text = "Preparing terrain and model…";
            await Viewer.LoadModelOnTerrainAsync(dem, photo, model, geoid, placement, cancellationToken: cancellation.Token);
            if (_closed) return;
            _modelReady = true;
            Viewer.SetModelVisible(ModelVisible.IsChecked == true);
            Viewer.ImageryVisible = Imagery.IsChecked == true;
            Viewer.FocusModel();
            ModelVisible.IsEnabled = Imagery.IsEnabled = FocusButton.IsEnabled = true;
            StatusText.Text = $"Model loaded — {Viewer.GetModelVertexCount() / 3:N0} triangles; EGM08D595 corrected terrain";
        }
        catch (OperationCanceledException)
        {
            if (!_closed) StatusText.Text = "Loading cancelled; previous scene retained.";
        }
        catch (Exception error)
        {
            System.Diagnostics.Trace.WriteLine(error);
            if (!_closed)
            {
                StatusText.Text = "Unable to load model. Apply placement / Reload to retry.";
                MessageBox.Show(this, error.Message, "ModelPlacement", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _load = null;
            if (!_closed)
            {
                ApplyButton.IsEnabled = PlacementFields.IsEnabled = true;
                CancelButton.IsEnabled = false;
                Progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
