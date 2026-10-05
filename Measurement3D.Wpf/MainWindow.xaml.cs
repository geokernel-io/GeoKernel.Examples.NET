using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace GeoKernel.Measurement3D.Wpf;

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
            try { RefreshMeasurement(); }
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
    private void Mode_Changed(object sender, SelectionChangedEventArgs e) => ApplyMode();
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
        Viewer.SetMeasurementMode(Mode.SelectedIndex == 1, Capture.IsChecked == true);
        RefreshMeasurement();
    }

    private void EditPoints(bool undo)
    {
        if (!_ready || _load != null || _closed) return;
        Viewer.EditMeasurement(undo);
        RefreshMeasurement();
    }

    private void RefreshMeasurement()
    {
        if (!_ready || _closed) return;
        var measurement = Viewer.GetMeasurement();
        string snapshot = JsonSerializer.Serialize(measurement);
        if (snapshot == _lastSnapshot) return;
        _lastSnapshot = snapshot;
        Points.ItemsSource = measurement.Points;
        int count = measurement.Points.Count;
        int minimum = measurement.AreaMode ? 3 : 2;
        string area = measurement.ValidArea ? $"Plan area: {measurement.PlanArea:F2} m²\n" : "";
        string delta = !measurement.AreaMode && count >= 2 ? $"\nEnd-to-start ΔUp: {measurement.DeltaUp:F2} m" : "";
        ResultText.Text = $"{count} points\n" + (count < minimum ? $"Add at least {minimum} points.\n" : "") + area +
            $"Horizontal {(measurement.AreaMode ? "perimeter" : "length")}: {measurement.Horizontal:F2} m\n" +
            $"3D segment total: {measurement.Length3D:F2} m" + delta +
            (string.IsNullOrEmpty(measurement.Message) ? "" : "\n" + measurement.Message);
        UndoButton.IsEnabled = ClearButton.IsEnabled = _load == null && count > 0;
        FinishButton.IsEnabled = _load == null && measurement.Capture && count >= minimum &&
            (!measurement.AreaMode || measurement.ValidArea);
    }

    private void SetBusy(bool busy)
    {
        ReloadButton.IsEnabled = !busy;
        CancelButton.IsEnabled = busy;
        Progress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        Mode.IsEnabled = Capture.IsEnabled = Imagery.IsEnabled = _ready && !busy;
        FinishButton.IsEnabled = UndoButton.IsEnabled = ClearButton.IsEnabled = false;
        _lastSnapshot = null;
        if (_ready && !busy) { RefreshMeasurement(); _poll.Start(); }
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
            if (_ready) Viewer.SetMeasurementMode(Mode.SelectedIndex == 1, false);
            var progress = new Progress<string>(message => { if (!_closed) StatusText.Text = message; });
            string dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            string imagery = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            if (_closed) return;
            StatusText.Text = "Loading terrain…";
            await Viewer.LoadTerrainAndImageryAsync(dem, imagery, 512, cancellation.Token);
            if (_closed) return;
            _ready = true;
            Viewer.ImageryVisible = Imagery.IsChecked == true;
            StatusText.Text = "Ready — click terrain to measure.";
        }
        catch (OperationCanceledException) { if (!_closed) StatusText.Text = "Loading cancelled."; }
        catch (Exception ex)
        {
            if (!_closed)
            {
                StatusText.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "Measurement3D", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _load = null;
            if (!_closed)
            {
                // Failed or cancelled replacement loads retain the current measurement.
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
