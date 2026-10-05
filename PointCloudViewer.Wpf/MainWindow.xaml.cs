using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace GeoKernel.PointCloudViewer.Wpf;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _poll = new() { Interval = TimeSpan.FromSeconds(1) };
    private CancellationTokenSource? _load;
    private bool _started, _closed, _ready;

    public MainWindow()
    {
        InitializeComponent();
        _poll.Tick += (_, _) => Perform(RefreshState);
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        if (_started) return;
        _started = true;
        await LoadSampleAsync();
    }

    private async void Reload_Click(object sender, RoutedEventArgs e) => await LoadSampleAsync();

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        _load?.Cancel();
        CancelButton.IsEnabled = false;
    }

    private void Reset_Click(object sender, RoutedEventArgs e) => Perform(() => Viewer.ResetCamera());

    private void Colors_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_ready && _load == null)
            Perform(() => Viewer.SetPointCloudStyle(Colors.SelectedIndex == 1));
    }

    private void Perform(Action action)
    {
        if (_closed) return;
        try { action(); }
        catch (Exception ex) { Errors.Text = ex.Message; }
    }

    private void SetBusy(bool busy)
    {
        ReloadButton.IsEnabled = Samples.IsEnabled = !busy;
        CancelButton.IsEnabled = busy;
        Colors.IsEnabled = _ready && !busy;
        Progress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        if (_ready && !busy) _poll.Start();
        else _poll.Stop();
    }

    private void RefreshState()
    {
        var state = Viewer.GetPointCloudState();
        Details.Text = $"Source: {state.Total:N0} points\nDisplay sample: {state.Sample:N0} points\n" +
            $"Source Z: {state.MinimumZ:F2} – {state.MaximumZ:F2} m\nRGB available: {(state.HasRgb ? "Yes" : "No")}";
        Errors.Text = state.Error.Trim();
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null || _closed) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        SetBusy(true);
        try
        {
            var progress = new Progress<string>(text => { if (!_closed) StatusText.Text = text; });
            string path = await SampleData.EnsureArchiveAsync("eurosdr_p4_pointcloud",
                "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/eurosdr_p4_pointcloud.zip",
                "eurosdr_p4_pointcloud.laz", progress, cancellation.Token);
            if (_closed) return;
            StatusText.Text = "Reading and sampling LAZ…";
            int limit = Samples.SelectedIndex switch { 0 => 50000, 1 => 100000, _ => 250000 };
            await Viewer.LoadPointCloudAsync(path, limit, cancellation.Token);
            if (_closed) return;
            _ready = true;
            Viewer.SetPointCloudStyle(Colors.SelectedIndex == 1);
            RefreshState();
            StatusText.Text = "Point cloud loaded.";
        }
        catch (OperationCanceledException)
        {
            if (!_closed) StatusText.Text = _ready ? "Cancelled; previous cloud retained." : "Loading cancelled.";
        }
        catch (Exception ex)
        {
            if (!_closed)
            {
                StatusText.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "PointCloudViewer", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _load = null;
            if (!_closed) SetBusy(false);
        }
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _closed = true;
        _poll.Stop();
        _load?.Cancel();
        Viewer.Dispose();
    }
}
