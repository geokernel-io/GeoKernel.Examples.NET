using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.Examples.Common;
using GeoKernel.NET.Wpf;
using GeoKernel.NET.Wpf.Controls;
using Microsoft.Win32;
using Button = System.Windows.Controls.Button;
using ComboBox = System.Windows.Controls.ComboBox;
using Panel = System.Windows.Controls.Panel;
using ProgressBar = System.Windows.Controls.ProgressBar;
using TextBox = System.Windows.Controls.TextBox;
using Orientation = System.Windows.Controls.Orientation;
using Brushes = System.Windows.Media.Brushes;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace GeoKernel.BuildingSegmentationInference.Wpf;

public sealed class MainWindow : Window
{
    static readonly Uri RasterUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/buildings.zip");
    static readonly Uri ModelUrl = new("https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/buildings-model.zip");
    readonly GeoKernelViewerControl viewer = new();
    readonly TextBox modelPath = new();
    readonly TextBox rasterPath = new();
    readonly TextBox labelPath = new();
    readonly ComboBox provider = new();
    readonly Button run = new() { Content = "Run building segmentation inference", Height = 30 };
    readonly ProgressBar progress = new() { Minimum = 0, Maximum = 100, Height = 20 };
    readonly TextBlock progressText = new();
    readonly TextBox details = new() { IsReadOnly = true, AcceptsReturn = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    readonly TextBlock status = new();
    readonly GeoKernelAnalysis analysis = new();
    AnalysisLayer? predictionLayer;

    public MainWindow()
    {
        Title = "BuildingSegmentationInference";
        Width = 1280;
        Height = 850;
        Icon = System.Windows.Media.Imaging.BitmapFrame.Create(new Uri("pack://application:,,,/Images/GeoKernelAppIcon.ico"));
        provider.ItemsSource = new[] { "Auto", "CPU", "CUDA", "DirectML" };
        provider.SelectedIndex = 0;

        var root = new DockPanel();
        var tools = new StackPanel { Orientation = Orientation.Horizontal, Height = 36, Background = Brushes.WhiteSmoke };
        foreach (var item in new (string Text, Action Action)[]
        {
            ("Zoom In", () => viewer.ZoomIn()), ("Zoom Out", () => viewer.ZoomOut()),
            ("Full Extent", viewer.FullExtent), ("Zoom Rect", () => viewer.ActiveTool = GeoKernelViewerTool.ZoomBox),
            ("Pan", () => viewer.ActiveTool = GeoKernelViewerTool.Pan)
        })
        {
            var button = new Button { Content = item.Text, Margin = new Thickness(2) };
            button.Click += (_, _) => item.Action();
            tools.Children.Add(button);
        }
        DockPanel.SetDock(tools, Dock.Top);
        root.Children.Add(tools);

        var footer = new Border { Child = status, Padding = new Thickness(4) };
        DockPanel.SetDock(footer, Dock.Bottom);
        root.Children.Add(footer);

        var right = new DockPanel { Width = 430, Margin = new Thickness(10) };
        DockPanel.SetDock(right, Dock.Right);
        var form = new StackPanel();
        form.Children.Add(new TextBlock { Text = "Building segmentation inference", FontWeight = FontWeights.Bold, FontSize = 14 });
        form.Children.Add(new TextBlock { Text = "Run a GeoKernel model package on a georeferenced RGB raster.", TextWrapping = TextWrapping.Wrap });
        AddPath(form, "Model package", modelPath, BrowseModel);
        AddPath(form, "Input raster", rasterPath, BrowseRaster);
        AddPath(form, "Instance labels", labelPath, BrowseLabel);
        form.Children.Add(new TextBlock { Text = "Execution provider" });
        form.Children.Add(provider);
        form.Children.Add(run);
        form.Children.Add(progress);
        form.Children.Add(progressText);
        form.Children.Add(new TextBlock { Text = "Inference diagnostics", Margin = new Thickness(0, 7, 0, 2), FontWeight = FontWeights.Bold });
        DockPanel.SetDock(form, Dock.Top);
        right.Children.Add(form);
        var legend = new TextBlock { Text = "Building segmentation classes\n  Background   🔴 Building", TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 8, 0, 0) };
        DockPanel.SetDock(legend, Dock.Bottom);
        right.Children.Add(legend);
        right.Children.Add(details);
        root.Children.Add(right);
        root.Children.Add(viewer);
        Content = root;

        viewer.ActiveTool = GeoKernelViewerTool.Pan;
        viewer.BusyChanged += (_, e) => Dispatcher.BeginInvoke(() => RenderProgress(e.Busy));
        run.Click += async (_, _) => await RunInference();
        Closed += (_, _) => predictionLayer?.Dispose();
        details.Text = "Preparing the buildings sample...";
        ContentRendered += async (_, _) => await PrepareSamples();
    }

    async Task PrepareSamples()
    {
        run.IsEnabled = false;
        try
        {
            await Task.Yield();
            rasterPath.Text = SampleData.EnsureWpfSampleFile(RasterUrl, "buildings.zip", "buildings", "buildings.tif", this);
            var manifest = SampleData.EnsureWpfSampleFile(ModelUrl, "buildings-model.zip", "buildings-model", "geokernel-model.json", this);
            modelPath.Text = Path.GetDirectoryName(manifest)!;
            UpdateLabelPath();
            OpenBaseRaster();
            details.Text = "The buildings raster and ONNX model package are ready. Run inference to add building polygons.";
        }
        catch (Exception ex) { details.Text = "Sample preparation failed:\n" + ex.Message; MessageBox.Show(this, ex.Message, Title); }
        finally { run.IsEnabled = true; }
    }

    void OpenBaseRaster()
    {
        predictionLayer?.Dispose(); predictionLayer = null; viewer.ClearLayers();
        if (!viewer.AddLayerFile(rasterPath.Text)) throw new InvalidOperationException("The buildings input raster could not be opened.");
        viewer.SetLayerName(0, "Buildings RGB source"); viewer.RefreshLayers(); viewer.FullExtent(); status.Text = "Map ready.";
    }

    void UpdateLabelPath()
    {
        if (File.Exists(rasterPath.Text))
            labelPath.Text = Path.Combine(Path.GetDirectoryName(rasterPath.Text)!, Path.GetFileNameWithoutExtension(rasterPath.Text) + "_building_instances.tif");
    }

    static void AddPath(Panel panel, string label, TextBox box, RoutedEventHandler browse)
    {
        panel.Children.Add(new TextBlock { Text = label });
        var grid = new Grid();
        grid.ColumnDefinitions.Add(new ColumnDefinition());
        grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(82) });
        box.Margin = new Thickness(0, 0, 4, 2);
        Grid.SetColumn(box, 0);
        var button = new Button { Content = "Browse...", Margin = new Thickness(0, 0, 0, 2) };
        button.Click += browse;
        Grid.SetColumn(button, 1);
        grid.Children.Add(box);
        grid.Children.Add(button);
        panel.Children.Add(grid);
    }

    void BrowseModel(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { InitialDirectory = modelPath.Text };
        if (dialog.ShowDialog(this) == true) modelPath.Text = dialog.FolderName;
    }

    void BrowseRaster(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "GeoTIFF (*.tif;*.tiff)|*.tif;*.tiff", FileName = rasterPath.Text };
        if (dialog.ShowDialog(this) == true) { rasterPath.Text = dialog.FileName; UpdateLabelPath(); OpenBaseRaster(); }
    }

    void BrowseLabel(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog { Filter = "GeoTIFF (*.tif)|*.tif", FileName = labelPath.Text };
        if (dialog.ShowDialog(this) == true) labelPath.Text = dialog.FileName;
    }

    async Task RunInference()
    {
        if (!Directory.Exists(modelPath.Text) || !File.Exists(rasterPath.Text))
        {
            MessageBox.Show(this, "Select an existing model package and input raster.", Title);
            return;
        }

        run.IsEnabled = false;
        SetProgress(0, "Opening model package...");
        details.Text = "Validating the model package and preparing instance inference...";
        try
        {
            var reporter = new Progress<AiInstanceVectorizationProgress>(x => SetProgress(x.Percent, x.Message));
            var layer = await analysis.RunAiInstanceVectorizationToMemoryAsync(new AiInstanceVectorizationRequest
            {
                ModelPackagePath = modelPath.Text,
                RasterPath = rasterPath.Text,
                LabelRasterPath = labelPath.Text,
                Provider = Enum.Parse<AIExecutionProvider>(provider.SelectedItem!.ToString()!),
                LayerName = "building_predictions",
                InstanceIdField = "instance_id",
                Connectivity = 4
            }, reporter);
            var diagnostics = layer.Diagnostics;
            SetProgress(95, "Opening source and prediction overlay...");
            predictionLayer?.Dispose();
            predictionLayer = layer;
            viewer.ClearLayers();
            if (!viewer.AddLayerFile(rasterPath.Text)) throw new InvalidOperationException("The source GeoTIFF could not be opened.");
            layer.AddTo(viewer);
            if (!viewer.SetLayerStyle(0, new GeoKernelLayerStyle { FillColor = "#FF3B30", FillOpacity = 125, LineColor = "#C5160A", LineWidth = 1.8 }))
                throw new InvalidOperationException("The prediction layer style could not be applied.");
            viewer.RefreshLayers();
            viewer.FullExtent();
            var count = diagnostics.GetProperty("materializedCount").GetInt64();
            details.Text = $"GeoKernel AI building segmentation inference\n\nProvider: {provider.SelectedItem}\nBuilding polygons: {count}\n\nInstance labels:\n{labelPath.Text}\n\nVector output: in-memory layer";
            SetProgress(100, "Inference complete");
            status.Text = $"Building mask and {count} vector polygons created.";
        }
        catch (Exception ex)
        {
            progress.Value = 0;
            progressText.Text = "Inference failed";
            details.Text = "Inference failed:\n" + ex.Message;
            MessageBox.Show(this, ex.Message, Title);
        }
        finally { run.IsEnabled = true; }
    }

    void SetProgress(int value, string text)
    {
        progress.IsIndeterminate = false;
        progress.Value = Math.Clamp(value, 0, 100);
        progressText.Text = $"{progress.Value}% — {text}";
        status.Text = text;
    }

    void RenderProgress(bool busy)
    {
        if (!run.IsEnabled) return;
        progress.IsIndeterminate = busy;
        if (!busy) { progress.Value = 100; status.Text = "Map ready."; }
    }
}
