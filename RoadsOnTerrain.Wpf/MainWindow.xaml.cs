using System.Windows;
using System.Windows.Controls;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.RoadsOnTerrain.Wpf;

public partial class MainWindow : Window
{
    private System.Windows.Media.Color _roadColor = System.Windows.Media.Color.FromRgb(255, 207, 51);
    private bool _roadsReady;
    private CancellationTokenSource? _load;
    private bool _closed;
    private bool _started;

    public MainWindow() => InitializeComponent();

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

    private void ApplyRoadStyle()
    {
        if (!_roadsReady || _closed) return;
        Viewer.SetRoadStyle(Roads.IsChecked == true, (float)RoadOpacity.Value / 100, _roadColor);
        RoadColorButton.Content = $"Road color: #{_roadColor.R:X2}{_roadColor.G:X2}{_roadColor.B:X2}";
    }
    private void RoadStyle_Changed(object sender, RoutedEventArgs e) => ApplyRoadStyle();
    private void RoadOpacity_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) => ApplyRoadStyle();
    private void RoadColor_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.ColorDialog
        {
            Color = System.Drawing.Color.FromArgb(_roadColor.R, _roadColor.G, _roadColor.B),
            FullOpen = true
        };
        var owner = new DialogOwner(new System.Windows.Interop.WindowInteropHelper(this).Handle);
        if (dialog.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK)
        {
            _roadColor = System.Windows.Media.Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);
            ApplyRoadStyle();
        }
    }
    private sealed record DialogOwner(IntPtr Handle) : System.Windows.Forms.IWin32Window;

    private void Height_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (Viewer != null) Viewer.HeightScale = (float)e.NewValue;
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        _closed = true;
        _load?.Cancel();
        Viewer.Dispose();
    }

    private async Task LoadSampleAsync()
    {
        if (_load != null || _closed) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
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
            string roads = await SampleData.EnsureRoadsAsync(progress, cancellation.Token);
            if (_closed) return;
            StatusText.Text = "Loading terrain…";
            await Viewer.LoadRoadsOnTerrainAsync(path, imagery, roads, 256 << Quality.SelectedIndex, cancellation.Token);
            if (_closed) return;
            _roadsReady = true;
            Roads.IsEnabled = RoadColorButton.IsEnabled = RoadOpacity.IsEnabled = true;
            ApplyRoadStyle();
            Imagery.IsEnabled = true;
            Viewer.ImageryVisible = Imagery.IsChecked == true;
            StatusText.Text = $"Terrain, orthophoto and roads loaded - {Viewer.GetRoadVertexCount():N0} road vertices";
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
                MessageBox.Show(this, ex.Message, "RoadsOnTerrain", MessageBoxButton.OK, MessageBoxImage.Error);
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
}
