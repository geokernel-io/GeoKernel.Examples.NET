using System.Windows;
using System.Windows.Controls;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.Buildings3D.Wpf;

public partial class MainWindow : Window
{
    private System.Windows.Media.Color _buildingColor = System.Windows.Media.Color.FromRgb(217, 196, 165);
    private bool _buildingsReady;
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

    private void ApplyBuildingStyle()
    {
        if (!_buildingsReady || _closed) return;
        Viewer.SetBuildingStyle(Buildings.IsChecked == true, (float)BuildingOpacity.Value / 100, _buildingColor);
        BuildingColorButton.Content = $"Building color: #{_buildingColor.R:X2}{_buildingColor.G:X2}{_buildingColor.B:X2}";
    }
    private void BuildingStyle_Changed(object sender, RoutedEventArgs e) => ApplyBuildingStyle();
    private void BuildingOpacity_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) => ApplyBuildingStyle();
    private void BuildingColor_Click(object sender, RoutedEventArgs e)
    {
        using var dialog = new System.Windows.Forms.ColorDialog
        {
            Color = System.Drawing.Color.FromArgb(_buildingColor.R, _buildingColor.G, _buildingColor.B),
            FullOpen = true
        };
        var owner = new DialogOwner(new System.Windows.Interop.WindowInteropHelper(this).Handle);
        if (dialog.ShowDialog(owner) == System.Windows.Forms.DialogResult.OK)
        {
            _buildingColor = System.Windows.Media.Color.FromRgb(dialog.Color.R, dialog.Color.G, dialog.Color.B);
            ApplyBuildingStyle();
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
        ReloadButton.IsEnabled = Quality.IsEnabled = BuildingHeight.IsEnabled = false;
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
            string buildings = await SampleData.EnsureBuildingsAsync(progress, cancellation.Token);
            if (_closed) return;
            StatusText.Text = "Loading terrain…";
            await Viewer.LoadBuildingsOnTerrainAsync(path, imagery, buildings, (float)BuildingHeight.Value, 256 << Quality.SelectedIndex, cancellation.Token);
            if (_closed) return;
            _buildingsReady = true;
            Buildings.IsEnabled = BuildingColorButton.IsEnabled = BuildingOpacity.IsEnabled = true;
            ApplyBuildingStyle();
            Imagery.IsEnabled = true;
            Viewer.ImageryVisible = Imagery.IsChecked == true;
            StatusText.Text = $"Terrain, orthophoto and buildings loaded - {Viewer.GetBuildingVertexCount():N0} building vertices";
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
                MessageBox.Show(this, ex.Message, "Buildings3D", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        finally
        {
            _load = null;
            if (!_closed)
            {
                ReloadButton.IsEnabled = Quality.IsEnabled = BuildingHeight.IsEnabled = true;
                CancelButton.IsEnabled = false;
                Progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
