using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.FeaturePicking.Wpf;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _selectionTimer = new(DispatcherPriority.Background)
    {
        Interval = TimeSpan.FromMilliseconds(150)
    };
    private FeatureSelection3D? _snapshot;
    private CancellationTokenSource? _load;
    private bool _ready;
    private bool _refreshing;
    private bool _started;
    private bool _closed;

    public MainWindow()
    {
        InitializeComponent();
        _selectionTimer.Tick += SelectionTimer_Tick;
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
    private void Focus_Click(object sender, RoutedEventArgs e) => Viewer.FocusSelection();
    private void Clear_Click(object sender, RoutedEventArgs e) => ClearSelection();

    private void ClearSelection()
    {
        if (!_ready || _closed) return;
        Viewer.ClearSelection();
        RefreshSelection();
    }

    private void Window_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape && _ready)
        {
            ClearSelection();
            e.Handled = true;
        }
    }

    private void Imagery_Changed(object sender, RoutedEventArgs e)
    {
        if (_ready && !_closed) Viewer.ImageryVisible = Imagery.IsChecked == true;
    }

    private void Height_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Viewer != null && !_closed) Viewer.HeightScale = (float)e.NewValue;
    }

    private void Multiple_Changed(object sender, RoutedEventArgs e)
    {
        if (_ready && !_closed) Viewer.SetMultipleSelectionEnabled(Multiple.IsChecked == true);
    }

    private void Layers_Changed(object sender, RoutedEventArgs e) => ApplyVisibility();

    private void ApplyVisibility()
    {
        if (!_ready || _closed) return;
        Viewer.SetBuildingStyle(Buildings.IsChecked == true, 1, Color.FromRgb(217, 196, 165));
        Viewer.SetRoadStyle(Roads.IsChecked == true, 1, Color.FromRgb(240, 224, 160));
        RefreshSelection();
    }

    private void SelectedFeature_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_refreshing || !_ready || _closed || _snapshot == null) return;
        if (SelectedFeature.SelectedItem is SelectedFeature3D feature)
        {
            Viewer.ActivateSelection(_snapshot.Revision, feature.Index);
            RefreshSelection();
        }
    }

    private void SelectionTimer_Tick(object? sender, EventArgs e)
    {
        try { RefreshSelection(); }
        catch (Exception ex)
        {
            _selectionTimer.Stop();
            if (!_closed) StatusText.Text = ex.Message;
        }
    }

    private void RefreshSelection()
    {
        if (!_ready || _closed) return;
        var snapshot = Viewer.GetFeatureSelection();
        if (_snapshot?.Revision == snapshot.Revision) return;
        _snapshot = snapshot;
        _refreshing = true;
        try
        {
            SelectedFeature.ItemsSource = snapshot.Entries;
            var active = snapshot.Entries.FirstOrDefault(f => f.Index == snapshot.ActiveIndex)
                ?? snapshot.Entries.FirstOrDefault();
            SelectedFeature.SelectedItem = active;
            var rows = new List<AttributeRow>();
            if (active != null)
            {
                rows.Add(new("Layer", active.Layer));
                rows.Add(new("Type", active.Type));
                rows.Add(new("Scene feature ID", active.FeatureId));
                if (active.SourceFid != null) rows.Add(new("Source FID", active.SourceFid));
                if (active.DisplayHeight.HasValue)
                    rows.Add(new("Display height (estimated)", $"{active.DisplayHeight:0.##} m"));
                foreach (var pair in active.Attributes.OrderBy(p => p.Key, StringComparer.OrdinalIgnoreCase))
                    rows.Add(new(pair.Key, pair.Value ?? "(null)"));
            }
            Attributes.ItemsSource = rows;
            SelectionCount.Text = active == null ? "No selection" : $"{snapshot.Entries.Count} selected";
            SelectedFeature.IsEnabled = FocusButton.IsEnabled = ClearButton.IsEnabled = active != null;
        }
        finally { _refreshing = false; }
    }

    private void SetSceneControls(bool enabled)
    {
        Imagery.IsEnabled = Buildings.IsEnabled = Roads.IsEnabled = Multiple.IsEnabled = enabled;
        SelectedFeature.IsEnabled = FocusButton.IsEnabled = ClearButton.IsEnabled =
            enabled && (_snapshot?.Entries.Count ?? 0) > 0;
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null || _closed) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        _selectionTimer.Stop();
        ReloadButton.IsEnabled = false;
        CancelButton.IsEnabled = true;
        Progress.Visibility = Visibility.Visible;
        SetSceneControls(false);
        try
        {
            var progress = new Progress<string>(message => { if (!_closed) StatusText.Text = message; });
            var dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            var imagery = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            var roads = await SampleData.EnsureRoadsAsync(progress, cancellation.Token);
            var buildings = await SampleData.EnsureBuildingsAsync(progress, cancellation.Token);
            if (_closed) return;
            StatusText.Text = "Loading buildings and roads…";
            await Viewer.LoadFeaturesOnTerrainAsync(dem, imagery, roads, buildings, cancellationToken: cancellation.Token);
            if (_closed) return;
            _ready = true;
            _snapshot = null;
            Viewer.ImageryVisible = Imagery.IsChecked == true;
            Viewer.SetMultipleSelectionEnabled(Multiple.IsChecked == true);
            ApplyVisibility();
            StatusText.Text = "Ready — click a building or road.";
        }
        catch (OperationCanceledException) { if (!_closed) StatusText.Text = "Loading cancelled."; }
        catch (Exception ex)
        {
            if (!_closed)
            {
                StatusText.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "FeaturePicking", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _load = null;
            if (!_closed)
            {
                ReloadButton.IsEnabled = true;
                CancelButton.IsEnabled = false;
                Progress.Visibility = Visibility.Collapsed;
                SetSceneControls(_ready);
                if (_ready) _selectionTimer.Start();
            }
        }
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _closed = true;
        _selectionTimer.Stop();
        _selectionTimer.Tick -= SelectionTimer_Tick;
        _load?.Cancel();
        Viewer.Dispose();
    }

    public sealed record AttributeRow(string Property, string Value);
}
