using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.TerrainAnalysis.Wpf;

public partial class MainWindow : Window
{
    private CancellationTokenSource? _load;
    private bool _initialized, _started, _closed, _ready;

    public MainWindow()
    {
        InitializeComponent();
        _initialized = true;
        ApplyMode();
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
    private void Mode_Changed(object sender, SelectionChangedEventArgs e)
    {
        if (_initialized) Perform(ApplyMode);
    }
    private void Height_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_initialized) Perform(() => Viewer.HeightScale = (float)Exaggeration.Value);
    }
    private void Perform(Action action)
    {
        if (_closed) return;
        try { action(); }
        catch (Exception ex) { StatusText.Text = ex.Message; }
    }

    private void ApplyMode()
    {
        if (_ready)
        {
            Viewer.ColorRamp = TerrainColorRamp.Terrain;
            Viewer.SetTerrainDisplayMode((TerrainDisplayMode)DisplayMode.SelectedIndex);
        }
        (string Color, string Text)[] rows = DisplayMode.SelectedIndex switch
        {
            1 => [("#2c7bb6", "0 to <5°"), ("#abd9e9", "5 to <15°"),
                  ("#ffffbf", "15 to <30°"), ("#fdae61", "30 to <45°"), ("#d7191c", "45 to 90°")],
            2 => [("#e6194b", "N — 0°"), ("#f58230", "NE — 45°"), ("#ffe119", "E — 90°"),
                  ("#3cb44b", "SE — 135°"), ("#46f0f0", "S — 180°"), ("#0082c8", "SW — 225°"),
                  ("#911eb4", "W — 270°"), ("#f032e6", "NW — 315°"), ("#808080", "Flat (slope <0.01°)")],
            _ => [("#185b3f", "Low"), ("#68984c", "↓"), ("#cfbe7e", "↓"),
                  ("#926b4c", "↓"), ("#fafafa", "High")]
        };
        Legend.Children.Clear();
        foreach (var row in rows)
        {
            var line = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            line.Children.Add(new Border
            {
                Width = 24, Height = 18, Margin = new Thickness(0, 0, 8, 0),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(row.Color))
            });
            line.Children.Add(new TextBlock { Text = row.Text, Margin = new Thickness(0), VerticalAlignment = VerticalAlignment.Center });
            Legend.Children.Add(line);
        }
        if (DisplayMode.SelectedIndex != 1)
            Legend.Children.Add(new TextBlock
            {
                Text = DisplayMode.SelectedIndex == 2 ? "Each direction spans ±22.5°." : "Relative ENU relief, with lighting."
            });
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null || _closed) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        ReloadButton.IsEnabled = Quality.IsEnabled = false;
        CancelButton.IsEnabled = true;
        Progress.Visibility = Visibility.Visible;
        int resolution = 256 << Quality.SelectedIndex;
        try
        {
            var progress = new Progress<string>(text => { if (!_closed) StatusText.Text = text; });
            string path = await SampleData.EnsureTerrainAsync(progress, cancellation.Token);
            if (_closed) return;
            StatusText.Text = "Loading terrain…";
            await Viewer.LoadTerrainAsync(path, resolution, cancellation.Token);
            if (_closed) return;
            _ready = true;
            ApplyMode();
            Viewer.HeightScale = (float)Exaggeration.Value;
            Info.Text = $"DEM: {Path.GetFileName(path)}\nHeights: Unknown — unverified preview\nMesh: up to {resolution} samples per side";
            StatusText.Text = "Terrain loaded.";
        }
        catch (OperationCanceledException)
        {
            if (!_closed) StatusText.Text = "Loading cancelled. Previous scene retained.";
        }
        catch (Exception ex)
        {
            if (!_closed)
            {
                StatusText.Text = ex.Message;
                MessageBox.Show(this, ex.Message, "TerrainAnalysis", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _load = null;
            if (!_closed)
            {
                ReloadButton.IsEnabled = Quality.IsEnabled = true;
                CancelButton.IsEnabled = false;
                Progress.Visibility = Visibility.Collapsed;
            }
        }
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _closed = true;
        _load?.Cancel();
        Viewer.Dispose();
    }
}
