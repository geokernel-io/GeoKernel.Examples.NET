using System.Windows;
using System.Windows.Controls;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.CameraNavigation.Wpf;

public partial class MainWindow : Window
{
    private CancellationTokenSource? _load;
    private bool _closed;
    private bool _started;

    private readonly CameraPanel _navigation;
    public MainWindow()
    {
        InitializeComponent();
        _navigation = new CameraPanel(Viewer);
        NavigationHost.Content = _navigation;
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_started) return;
        _started = true;
        await LoadSampleAsync();
    }

    private async void Reload_Click(object sender, RoutedEventArgs e) => await LoadSampleAsync();
    private void Cancel_Click(object sender, RoutedEventArgs e) => _load?.Cancel();
    private void Reset_Click(object sender, RoutedEventArgs e) => Viewer.ResetCamera();

    private void Imagery_Changed(object sender, RoutedEventArgs e)
    {
        if (Viewer != null) Viewer.ImageryVisible = Imagery.IsChecked == true;
    }

    private void Height_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Viewer != null) Viewer.HeightScale = (float)e.NewValue;
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _closed = true;
        _load?.Cancel();
        _navigation.Dispose();
        Viewer.Dispose();
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null || _closed) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        Viewer.StopCamera();
        NavigationHost.IsEnabled = false;
        ReloadButton.IsEnabled = Quality.IsEnabled = false;
        CancelButton.IsEnabled = true;
        Progress.Visibility = Visibility.Visible;
        try
        {
            var progress = new Progress<string>(message =>
            {
                if (!_closed) StatusText.Text = message;
            });
            string path = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            string imagery = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            if (_closed) return;
            StatusText.Text = "Loading terrain…";
            await Viewer.LoadTerrainAndImageryAsync(path, imagery, 256 << Quality.SelectedIndex, cancellation.Token);
            if (_closed) return;
            _navigation.SetHome();
            NavigationHost.IsEnabled = true;
            Imagery.IsEnabled = true;
            Viewer.ImageryVisible = Imagery.IsChecked == true;
            StatusText.Text = "Terrain and orthophoto loaded";
        }
        catch (OperationCanceledException)
        {
            if (!_closed) StatusText.Text = "Loading cancelled.";
        }
        catch (Exception ex)
        {
            System.Diagnostics.Trace.WriteLine(ex);
            if (!_closed)
            {
                StatusText.Text = "Terrain could not be loaded.";
                MessageBox.Show(this, ex.Message, "CameraNavigation", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _load = null;
            if (!_closed)
            {
                NavigationHost.IsEnabled = _navigation.Ready;
                ReloadButton.IsEnabled = Quality.IsEnabled = true;
                CancelButton.IsEnabled = false;
                Progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
