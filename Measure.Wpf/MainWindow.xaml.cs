using System.IO;
using System.Windows;
using System.Windows.Controls.Primitives;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.Measure.Wpf;

public partial class MainWindow
{
    public MainWindow() => InitializeComponent();

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        SelectTool(panButton);
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.ScaleBarVisible = true;
        viewerControl.SetScaleBarAnchor(GeoKernelOverlayAnchor.BottomLeft);
        LoadSampleLayers();
    }

    private void LoadSampleLayers()
    {
        try
        {
            statusText.Text = "Preparing sample data...";
            var world = SampleData.EnsureKnownWpfSampleFile("world_4326.shp", this);
            var cities = SampleData.EnsureKnownWpfSampleFile("world_cities_4326.shp", this);
            if (!File.Exists(world) || !File.Exists(cities))
                return;
            statusText.Text = "Loading sample layers...";
            if (!viewerControl.AddLayerFile(world) || !viewerControl.AddLayerFile(cities))
                throw new InvalidOperationException("Sample layers could not be loaded.");
            viewerControl.FullExtent();
            statusText.Text = "Ready";
        }
        catch (Exception ex)
        {
            statusText.Text = "Sample layers could not be loaded.";
            MessageBox.Show(this, ex.Message, "Measure", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Pan_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.MeasureToolActive = false;
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        SelectTool(panButton);
        statusText.Text = "Pan tool active.";
    }

    private void Distance_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.StartMeasureDistance();
        SelectTool(distanceButton);
        statusText.Text = "Measure distance: click vertices, then double-click to finish.";
    }

    private void Area_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ActiveTool = GeoKernelViewerTool.Pan;
        viewerControl.StartMeasureArea();
        SelectTool(areaButton);
        statusText.Text = "Measure area: click vertices, then double-click to finish.";
    }

    private void Clear_Click(object sender, RoutedEventArgs e)
    {
        viewerControl.ClearMeasure();
        statusText.Text = "Measurements cleared.";
    }

    private void FullExtent_Click(object sender, RoutedEventArgs e) => viewerControl.FullExtent();

    private void SelectTool(ToggleButton selected)
    {
        panButton.IsChecked = ReferenceEquals(selected, panButton);
        distanceButton.IsChecked = ReferenceEquals(selected, distanceButton);
        areaButton.IsChecked = ReferenceEquals(selected, areaButton);
    }
}
