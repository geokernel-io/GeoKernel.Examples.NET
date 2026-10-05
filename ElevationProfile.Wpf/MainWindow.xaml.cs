using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GeoKernel.ElevationProfile.Wpf;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _poll = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private CancellationTokenSource? _load;
    private bool _closed, _started, _ready;
    private string? _lastSnapshot;

    public MainWindow()
    {
        InitializeComponent();
        _poll.Tick += (_, _) =>
        {
            try { RefreshProfile(); }
            catch (Exception ex) { _poll.Stop(); StatusText.Text = ex.Message; }
        };
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
    private void Finish_Click(object sender, RoutedEventArgs e) => Capture.IsChecked = false;
    private void Undo_Click(object sender, RoutedEventArgs e) => EditPoints(true);
    private void Clear_Click(object sender, RoutedEventArgs e) => EditPoints(false);
    private void Spacing_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) => ApplyMode();
    private void Capture_Changed(object sender, RoutedEventArgs e) => ApplyMode();

    private void Imagery_Changed(object sender, RoutedEventArgs e)
    {
        if (_ready && !_closed) Viewer.ImageryVisible = Imagery.IsChecked == true;
    }

    private void Height_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Viewer != null && !_closed) Viewer.HeightScale = (float)e.NewValue;
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (!_ready || _load != null || _closed) return;
        if (e.Key is Key.Escape or Key.Back)
        {
            EditPoints(e.Key == Key.Back);
            e.Handled = true;
        }
    }

    private void ApplyMode()
    {
        if (!_ready || _load != null || _closed) return;
        Viewer.SetProfileMode(Capture.IsChecked == true, Spacing.Value);
        RefreshProfile();
    }

    private void EditPoints(bool undo)
    {
        if (!_ready || _load != null || _closed) return;
        Viewer.EditProfile(undo);
        RefreshProfile();
    }

    private void RefreshProfile()
    {
        if (!_ready || _closed) return;
        var measurement = Viewer.GetProfile();
        string snapshot = JsonSerializer.Serialize(measurement);
        if (snapshot == _lastSnapshot) return;
        _lastSnapshot = snapshot;
        Chart.Profile = measurement;
        int count = measurement.PointCount;
        ResultText.Text = $"{count} route points\nSamples: {measurement.Processed}/{measurement.Samples.Count}\n" +
            $"Horizontal: {measurement.Horizontal:F1} m\nAscent: {measurement.Ascent:F1} m\n" +
            $"Descent: {measurement.Descent:F1} m\nMaximum interval: {measurement.MaxInterval:F1} m\n" +
            $"NoData: {measurement.Missing}\n{measurement.Message}";
        UndoButton.IsEnabled = ClearButton.IsEnabled = _load == null && count > 0;
        FinishButton.IsEnabled = _load == null && measurement.Capture && count >= 2;
    }
    private void SetBusy(bool busy)
    {
        ReloadButton.IsEnabled = !busy;
        CancelButton.IsEnabled = busy;
        Progress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        Spacing.IsEnabled = Capture.IsEnabled = Imagery.IsEnabled = _ready && !busy;
        FinishButton.IsEnabled = UndoButton.IsEnabled = ClearButton.IsEnabled = false;
        _lastSnapshot = null;
        if (_ready && !busy) { RefreshProfile(); _poll.Start(); }
        else _poll.Stop();
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null || _closed) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        SetBusy(true);
        try
        {
            if (_ready) Viewer.SetProfileMode(false, Spacing.Value);
            var progress = new Progress<string>(message => { if (!_closed) StatusText.Text = message; });
            string dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            string imagery = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            if (_closed) return;
            StatusText.Text = "Loading terrain…";
            await Viewer.LoadTerrainAndImageryAsync(dem, imagery, 512, cancellation.Token);
            if (_closed) return;
            _ready = true;
            Viewer.ImageryVisible = Imagery.IsChecked == true;
            StatusText.Text = "Ready — click terrain to draw a profile.";
        }
        catch (OperationCanceledException) { if (!_closed) StatusText.Text = "Loading cancelled."; }
        catch (Exception ex)
        {
            if (!_closed)
            {
                StatusText.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "ElevationProfile", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _load = null;
            if (!_closed)
            {
                // Failed or cancelled replacement loads retain the current profile.
                try { if (_ready) ApplyMode(); SetBusy(false); }
                catch (Exception ex) { _poll.Stop(); StatusText.Text = ex.Message; }
            }
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
