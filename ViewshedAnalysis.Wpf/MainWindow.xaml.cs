using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ViewshedAnalysis.Wpf;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _poll = new() { Interval = TimeSpan.FromMilliseconds(150) };
    private CancellationTokenSource? _load;
    private bool _closed, _started, _ready, _updating;
    private string? _snapshot;

    public MainWindow()
    {
        InitializeComponent();
        SetBusy(false);
        _poll.Tick += (_, _) =>
        {
            try { RefreshResult(); }
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
    private void Calculate_Click(object sender, RoutedEventArgs e) => Safe(() => Viewer.RunViewshedAction(ViewshedAction.Calculate));
    private void Pause_Click(object sender, RoutedEventArgs e) => Safe(() => Viewer.RunViewshedAction(ViewshedAction.Pause));
    private void Clear_Click(object sender, RoutedEventArgs e) => Safe(() => Viewer.RunViewshedAction(ViewshedAction.Clear));
    private void Pick_Changed(object sender, RoutedEventArgs e) => Safe(() => Viewer.SetViewshedCapture(Pick.IsChecked == true));
    private void Overlay_Changed(object sender, RoutedEventArgs e) => Safe(() => Viewer.SetViewshedOverlay(Overlay.IsChecked == true));
    private void Imagery_Changed(object sender, RoutedEventArgs e) => Safe(() => Viewer.ImageryVisible = Imagery.IsChecked == true);
    private void Settings_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) => Safe(Configure);
    private void Grid_Changed(object sender, SelectionChangedEventArgs e) => Safe(Configure);

    private void Height_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Viewer != null && !_closed) Viewer.HeightScale = (float)e.NewValue;
    }

    private void Configure() => Viewer.ConfigureViewshed(Eye.Value, Target.Value,
        Radius.Value, Spacing.Value, new[] { 17, 33, 65 }[GridSize.SelectedIndex]);

    private void Safe(Action action)
    {
        if (!_ready || _load != null || _closed || _updating) return;
        try { action(); RefreshResult(); }
        catch (Exception ex) { StatusText.Text = ex.Message; }
    }

    private void RefreshResult()
    {
        if (!_ready || _closed) return;
        var result = Viewer.GetViewshed();
        string snapshot = JsonSerializer.Serialize(result);
        if (_snapshot == snapshot) return;
        _snapshot = snapshot;
        _updating = true;
        try { Pick.IsChecked = result.Capture; }
        finally { _updating = false; }
        LocationText.Text = result.HasObserver
            ? $"Observer: {result.Longitude:F6}° E, {result.Latitude:F6}° N" : "No observer selected.";
        Map.Result = result;
        AnalysisProgress.Value = result.Cells.Count == 0 ? 0 : Math.Clamp(100 * result.Processed / result.Cells.Count, 0, 100);
        int Count(ViewshedCell cell) => result.Cells.Count(value => value == cell);
        ResultText.Text = $"{result.Message}\nVisible: {Count(ViewshedCell.Clear)} | Blocked: {Count(ViewshedCell.Blocked)}\n" +
            $"Unknown: {Count(ViewshedCell.Unknown)} | Pending: {Count(ViewshedCell.Pending)}\n" +
            $"Grid spacing: {result.CellSpacing:F1} m\nMaximum ray spacing: {result.MaxRaySpacing:F1} m\n{result.OverlayMessage}";
        CalculateButton.Content = result.Complete ? "Recalculate" : result.Cells.Count > 0 ? "Resume calculation" : "Calculate";
        CalculateButton.IsEnabled = _load == null && result.HasObserver && !result.Running;
        PauseButton.IsEnabled = _load == null && result.Running;
        ClearButton.IsEnabled = _load == null && result.HasObserver;
    }

    private void SetBusy(bool busy)
    {
        ReloadButton.IsEnabled = !busy;
        CancelButton.IsEnabled = busy;
        LoadProgress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        foreach (Control control in new Control[] { Pick, Imagery, Overlay, Eye, Target, Radius, Spacing, GridSize })
            control.IsEnabled = _ready && !busy;
        CalculateButton.IsEnabled = PauseButton.IsEnabled = ClearButton.IsEnabled = false;
        _snapshot = null;
        if (_ready && !busy) { RefreshResult(); _poll.Start(); }
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
            if (_ready)
            {
                Viewer.SetViewshedCapture(false);
                if (Viewer.GetViewshed().Running) Viewer.RunViewshedAction(ViewshedAction.Pause);
            }
            var progress = new Progress<string>(message => { if (!_closed) StatusText.Text = message; });
            string dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            string imagery = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            if (_closed) return;
            StatusText.Text = "Loading terrain…";
            await Viewer.LoadTerrainAndImageryAsync(dem, imagery, 512, cancellation.Token);
            if (_closed) return;
            _ready = true;
            Configure();
            Viewer.ImageryVisible = Imagery.IsChecked == true;
            Viewer.SetViewshedOverlay(Overlay.IsChecked == true);
            Viewer.SetViewshedCapture(true);
            StatusText.Text = "Ready — select an observer on terrain.";
        }
        catch (OperationCanceledException) { if (!_closed) StatusText.Text = "Loading cancelled."; }
        catch (Exception ex)
        {
            if (!_closed)
            {
                StatusText.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "ViewshedAnalysis", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _load = null;
            if (!_closed)
            {
                try { SetBusy(false); }
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
