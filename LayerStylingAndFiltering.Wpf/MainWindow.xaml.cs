using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.LayerStylingAndFiltering.Wpf;

public partial class MainWindow : Window
{
    private readonly DispatcherTimer _debounce = new() { Interval = TimeSpan.FromMilliseconds(250) };
    private List<LayerStyling3D> _layers = [];
    private string _activeId = "";
    private bool _syncing;
    private bool _ready;
    private bool _closed;
    private CancellationTokenSource? _load;

    public MainWindow()
    {
        InitializeComponent();
        _debounce.Tick += Debounce_Tick;
    }

    private static string SelectedField(ComboBox combo) =>
        combo.SelectedIndex > 0 ? combo.SelectedItem?.ToString() ?? "" : "";
    private LayerStyling3D? Current() => _layers.FirstOrDefault(layer => layer.Id == _activeId);

    private void Safe(Action action)
    {
        if (!_ready || _syncing || _closed) return;
        try { action(); }
        catch (Exception ex) { _debounce.Stop(); StatusText.Text = ex.Message; }
    }

    private void Layer_Changed(object sender, SelectionChangedEventArgs e) => Safe(() =>
    {
        if (_debounce.IsEnabled) ApplyFilter();
        _activeId = (Layer.SelectedItem as LayerStyling3D)?.Id ?? "";
        RefreshLayers();
    });

    private void Appearance_Changed(object sender, RoutedEventArgs e) => Safe(() =>
    {
        Viewer.SetLayerAppearance(_activeId, LayerVisible.IsChecked == true, (float)LayerOpacity.Value / 100);
        UpdateCounts();
    });

    private void Color_Click(object sender, RoutedEventArgs e) => Safe(() =>
    {
        ApplyFilter();
        var text = Current()?.Color;
        var dialog = new LayerColorDialog(string.IsNullOrEmpty(text) ? Colors.White : (Color)ColorConverter.ConvertFromString(text))
        {
            Owner = this
        };
        if (dialog.ShowDialog() == true)
        {
            Viewer.SetLayerColor(_activeId, dialog.SelectedColor);
            RefreshLayers();
        }
    });

    private void Source_Click(object sender, RoutedEventArgs e) => Safe(() =>
    {
        ApplyFilter();
        Viewer.SetLayerColor(_activeId, null);
        RefreshLayers();
    });

    private void Theme_Changed(object sender, SelectionChangedEventArgs e) => Safe(() =>
    {
        ApplyFilter();
        Viewer.SetLayerTheme(_activeId, SelectedField(Theme));
        RefreshLayers();
    });

    private void Field_Changed(object sender, SelectionChangedEventArgs e) => Safe(() =>
    {
        PopulateValues();
        ApplyFilter();
    });

    private void FilterText_Changed(object sender, TextChangedEventArgs e)
    {
        if (!_syncing && _ready && !_closed) { _debounce.Stop(); _debounce.Start(); }
    }

    private void Values_Changed(object sender, SelectionChangedEventArgs e) => Safe(() =>
    {
        if (Values.SelectedIndex > 0) FilterText.Text = Values.SelectedItem?.ToString() ?? "";
    });

    private void Debounce_Tick(object? sender, EventArgs e) => Safe(ApplyFilter);
    private void Clear_Click(object sender, RoutedEventArgs e) => Safe(() => { FilterText.Clear(); ApplyFilter(); });
    private void Focus_Click(object sender, RoutedEventArgs e) => Safe(() => { ApplyFilter(); Viewer.FocusLayer(_activeId); });
    private void ResetStyles_Click(object sender, RoutedEventArgs e) => Safe(() =>
    {
        _debounce.Stop();
        Viewer.ResetLayerStyling();
        RefreshLayers();
    });
    private void Imagery_Changed(object sender, RoutedEventArgs e) => Safe(() => Viewer.ImageryVisible = Imagery.IsChecked == true);
    private void Height_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) => Safe(() => Viewer.HeightScale = (float)HeightScale.Value);
    private void Reset_Click(object sender, RoutedEventArgs e) => Safe(Viewer.ResetCamera);
    private async void Reload_Click(object sender, RoutedEventArgs e) => await LoadSampleAsync();
    private void Cancel_Click(object sender, RoutedEventArgs e) => _load?.Cancel();
    private async void Window_Loaded(object sender, RoutedEventArgs e) => await LoadSampleAsync();

    private void ApplyFilter()
    {
        _debounce.Stop();
        if (string.IsNullOrEmpty(_activeId)) return;
        Viewer.SetLayerFilter(_activeId, SelectedField(Field), FilterText.Text);
        UpdateCounts();
    }

    private void UpdateCounts()
    {
        _layers = Viewer.GetLayerStyling();
        var current = Current();
        if (current == null) return;
        Count.Text = $"Matching records: {current.MatchingRecords:N0} / {current.Records:N0}\nDrawable matching ranges: {current.DrawableRanges:N0}";
        FocusButton.IsEnabled = current.DrawableRanges > 0 && current.Visible && current.Opacity > 0;
    }

    private void RefreshLayers()
    {
        _layers = Viewer.GetLayerStyling();
        _syncing = true;
        try
        {
            Layer.Items.Clear();
            foreach (var item in _layers) Layer.Items.Add(item);
            var current = Current() ?? _layers.FirstOrDefault();
            _activeId = current?.Id ?? "";
            Styles.IsEnabled = current != null;
            if (current == null) return;
            Layer.SelectedItem = current;
            LayerVisible.IsChecked = current.Visible;
            LayerOpacity.Value = Math.Clamp(current.Opacity * 100, 0, 100);
            ColorButton.Content = current.Color.Length > 0 ? $"Uniform color: {current.Color}" : "Choose uniform color";
            Field.Items.Clear();
            Theme.Items.Clear();
            Field.Items.Add("All attributes / source IDs");
            Theme.Items.Add("Uniform / source color");
            foreach (var key in current.Attributes.Keys.OrderBy(key => key, StringComparer.OrdinalIgnoreCase))
            {
                Field.Items.Add(key);
                Theme.Items.Add(key);
            }
            Field.SelectedIndex = Math.Max(0, Field.Items.IndexOf(current.Field));
            Theme.SelectedIndex = Math.Max(0, Theme.Items.IndexOf(current.Theme));
            FilterText.Text = current.Text;
            PopulateValues();
        }
        finally { _syncing = false; }
        UpdateCounts();
    }

    private void PopulateValues()
    {
        bool wasSyncing = _syncing;
        _syncing = true;
        try
        {
            Values.Items.Clear();
            Values.Items.Add("Choose a value or type text");
            if (Current()?.Attributes.TryGetValue(SelectedField(Field), out var values) == true)
                foreach (var value in values) Values.Items.Add(value);
            Values.SelectedIndex = 0;
            Values.IsEnabled = Values.Items.Count > 1;
        }
        finally { _syncing = wasSyncing; }
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null || _closed) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        if (_ready) Safe(ApplyFilter);
        _debounce.Stop();
        Styles.IsEnabled = Imagery.IsEnabled = ReloadButton.IsEnabled = false;
        CancelButton.IsEnabled = true;
        Progress.Visibility = Visibility.Visible;
        try
        {
            var progress = new Progress<string>(message => { if (!_closed) StatusText.Text = message; });
            var dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            var imagery = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            var roads = await SampleData.EnsureRoadsAsync(progress, cancellation.Token);
            var buildings = await SampleData.EnsureBuildingsAsync(progress, cancellation.Token);
            cancellation.Token.ThrowIfCancellationRequested();
            if (_closed) return;
            StatusText.Text = "Loading terrain, roads and buildings…";
            await Viewer.LoadFeaturesOnTerrainAsync(dem, imagery, roads, buildings, cancellationToken: cancellation.Token);
            if (_closed) return;
            _ready = true;
            Viewer.ImageryVisible = Imagery.IsChecked == true;
            Viewer.HeightScale = (float)HeightScale.Value;
            RefreshLayers();
            StatusText.Text = "Ready — choose a layer to style or filter.";
        }
        catch (OperationCanceledException) { if (!_closed) StatusText.Text = "Loading cancelled."; }
        catch (Exception ex)
        {
            if (!_closed)
            {
                StatusText.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "Unable to load sample", MessageBoxButton.OK, MessageBoxImage.Error);
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
                Styles.IsEnabled = Imagery.IsEnabled = _ready;
            }
        }
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _closed = true;
        _debounce.Stop();
        _debounce.Tick -= Debounce_Tick;
        _load?.Cancel();
        Viewer.Dispose();
    }
}
