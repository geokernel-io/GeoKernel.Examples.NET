using System.Windows;
using System.Windows.Threading;

namespace GeoKernel.Tiles3DStreaming.Wpf;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _poll = new() { Interval = TimeSpan.FromMilliseconds(500) };
    private CancellationTokenSource? _load;
    private string? _source;
    private bool _closed, _started, _ready, _updating;

    public MainWindow()
    {
        InitializeComponent();
        _poll.Tick += (_, _) =>
        {
            try { RefreshState(); }
            catch (Exception ex) { _poll.Stop(); Errors.Text = ex.Message; }
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
    private void Start_Click(object sender, RoutedEventArgs e) => Perform(StartStreaming);
    private void Stop_Click(object sender, RoutedEventArgs e) => Perform(() => Viewer.StopTiles());
    private void Focus_Click(object sender, RoutedEventArgs e) => Perform(() => Viewer.FocusTiles());
    private void Reset_Click(object sender, RoutedEventArgs e) => Perform(() => Viewer.ResetCamera());

    private void Buildings_Changed(object sender, RoutedEventArgs e)
    {
        if (_updating || !_ready || _load != null || _closed) return;
        Perform(() => { if (Buildings.IsChecked == true) StartStreaming(); else Viewer.StopTiles(clear: true); });
    }

    private void Imagery_Changed(object sender, RoutedEventArgs e)
    {
        if (_ready && !_closed) Perform(() => Viewer.ImageryVisible = Imagery.IsChecked == true);
    }

    private void Perform(Action action)
    {
        if (_closed) return;
        try { action(); Errors.Text = ""; RefreshState(); }
        catch (Exception ex) { Errors.Text = ex.Message; }
    }

    private void StartStreaming()
    {
        if (!_ready || _source == null || _closed) return;
        Viewer.StartTiles(_source, 64 + 32 * Budget.SelectedIndex);
        _updating = true;
        try { Buildings.IsChecked = true; }
        finally { _updating = false; }
    }

    private void RefreshState()
    {
        if (!_ready || _load != null || _closed) return;
        var state = Viewer.GetTilesState();
        StopButton.IsEnabled = state.Active;
        FocusButton.IsEnabled = state.Attached && Buildings.IsChecked == true;
        Details.Text = $"State: {(state.Active ? (state.Loading ? "Updating (previous tiles visible)" : "Active") : "Stopped")}\n" +
            $"Loaded tiles: {state.Tiles}\nDeferred tiles: {state.DeferredTiles}\n" +
            $"Detail budget limited: {(state.SelectionLimited ? "Yes" : "No")}\n" +
            $"CPU cache: {state.CacheBytes / (1024.0 * 1024):F1} MiB\nCache hits: {state.CacheHits}\n" +
            $"Drawn vertices: {state.DrawnVertices}\nGPU upload pending: {(state.UploadPending ? "Yes" : "No")}";
        Errors.Text = state.Error.Trim();
    }

    private void SetBusy(bool busy)
    {
        ReloadButton.IsEnabled = !busy;
        CancelButton.IsEnabled = busy;
        Progress.Visibility = busy ? Visibility.Visible : Visibility.Collapsed;
        StartButton.IsEnabled = Budget.IsEnabled = Buildings.IsEnabled = Imagery.IsEnabled = _ready && !busy;
        StopButton.IsEnabled = FocusButton.IsEnabled = false;
        if (_ready && !busy) { _poll.Start(); RefreshState(); }
        else _poll.Stop();
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null || _closed) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        SetBusy(true);
        bool resume = false;
        try
        {
            if (_ready) { resume = Viewer.GetTilesState().Active; Viewer.StopTiles(); }
            var progress = new Progress<string>(text => { if (!_closed) StatusText.Text = text; });
            string dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            string image = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            string tiles = await SampleData.EnsureArchiveAsync("sagrada_familia_3d_tiles",
                "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/sagrada_familia_3d_tiles.zip",
                "tileset.json", progress, cancellation.Token);
            string geoid = await SampleData.EnsureArchiveAsync("egm08d595",
                "https://icgc-web-pro.s3.eu-central-1.amazonaws.com/produccio/s3fs-public/EGM08D595_19839.zip",
                "cat80000.gr", progress, cancellation.Token);
            if (_closed) return;
            StatusText.Text = "Loading corrected terrain…";
            await Viewer.LoadTilesTerrainAsync(dem, image, geoid, tiles, cancellation.Token);
            if (_closed) return;
            _source = tiles;
            _ready = true;
            Viewer.ImageryVisible = Imagery.IsChecked == true;
            resume = Buildings.IsChecked == true;
            StatusText.Text = "Terrain loaded — camera movement controls tile detail.";
        }
        catch (OperationCanceledException) { if (!_closed) StatusText.Text = "Loading cancelled."; }
        catch (Exception ex)
        {
            if (!_closed)
            {
                StatusText.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "Tiles3DStreaming", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _load = null;
            if (!_closed)
            {
                try { if (resume && _ready) StartStreaming(); SetBusy(false); }
                catch (Exception ex)
                {
                    _poll.Stop(); Errors.Text = ex.Message;
                    ReloadButton.IsEnabled = true; CancelButton.IsEnabled = false; Progress.Visibility = Visibility.Collapsed;
                }
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
