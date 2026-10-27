using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf.Controls;
using Microsoft.Win32;

namespace GeoKernel.HotosmBuildingFootprints.Wpf;

public sealed class MainWindow : Window
{
    private readonly GeoKernelViewerControl viewer = new();
    private readonly ComboBox provider = new() { ItemsSource = new[] { "Auto", "CPU", "CUDA" }, SelectedIndex = 0 };
    private readonly Slider opacity = new() { Minimum = 0, Maximum = 100, Value = 60, Margin = new Thickness(0, 8, 0, 12) };
    private readonly TextBlock opacityText = new();
    private readonly Button run = new() { Content = "Run building segmentation inference", Height = 32 };
    private readonly Button save = new() { Content = "Save Shapefile", Height = 32 };
    private readonly Button cancel = new() { Content = "Cancel", Height = 30 };
    private readonly ProgressBar progress = new() { Maximum = 100, Height = 20 };
    private readonly TextBlock message = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBox details = new() { IsReadOnly = true, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, Height = 200, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    private readonly Slider threshold = new() { Minimum = 0.01, Maximum = 0.99, Value = 0.5, TickFrequency = 0.01, IsSnapToTickEnabled = true };
    private readonly TextBlock thresholdText = new() { Text = "Building threshold: 0.50" };
    private readonly GeoAiSession session;
    private Task operation = Task.CompletedTask;
    private bool overlay;
    private bool closing;
    private bool closeAllowed;

    public MainWindow()
    {
        ExampleIcon.Apply(this);
        Title = "HOTOSM Building Footprints — WPF";
        Width = 1280;
        Height = 850;
        MinWidth = 900;
        MinHeight = 650;
        var root = new DockPanel();
        var side = new StackPanel { Margin = new Thickness(14) };
        var scroll = new ScrollViewer { Content = side, Width = 380, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        DockPanel.SetDock(scroll, Dock.Right);
        var tools = new StackPanel { Orientation = Orientation.Horizontal };
        DockPanel.SetDock(tools, Dock.Top);
        AddTool(tools, "Zoom In", () => viewer.ZoomIn());
        AddTool(tools, "Zoom Out", () => viewer.ZoomOut());
        AddTool(tools, "Full Extent", () => viewer.FullExtent());
        AddTool(tools, "Pan", () => viewer.ActiveTool = GeoKernelViewerTool.Pan);
        root.Children.Add(tools);
        root.Children.Add(scroll);
        root.Children.Add(viewer);
        Content = root;
        side.Children.Add(new TextBlock { Text = "HOTOSM DINOv3-S — building footprints", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 14) });
        side.Children.Add(new TextBlock { Text = "Execution provider" });
        side.Children.Add(provider);
        side.Children.Add(thresholdText);
        side.Children.Add(threshold);
        threshold.ValueChanged += (_, _) => thresholdText.Text = $"Building threshold: {threshold.Value:F2}";
        side.Children.Add(opacityText);
        side.Children.Add(opacity);
        foreach (var control in new Control[] { run, save, cancel, progress })
        {
            control.Margin = new Thickness(0, 3, 0, 3);
            side.Children.Add(control);
        }
        side.Children.Add(message);
        side.Children.Add(new TextBlock { Text = "Inference diagnostics", Margin = new Thickness(0, 12, 0, 4) });
        side.Children.Add(details);
        side.Children.Add(new TextBlock { Text = "Output\n" + GeoAiProfile.Hotosm.Legend, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 12) });
        side.Children.Add(new TextBlock { Text = "Model: HOTOSM / Meta DINOv3.\nCC-BY-4.0 + DINOv3 License; see notices.\nImagery source/license not verified.", TextWrapping = TextWrapping.Wrap });
        var renderFence = new GeoAiRenderFence(viewer);
        session = new GeoAiSession(GeoAiProfile.Hotosm)
        {
            WaitForRenderer = renderFence.WaitAsync,
            ShowSource = path =>
            {
                viewer.ClearLayers();
                if (!viewer.AddLayerFile(path)) throw new InvalidOperationException("Cannot open source RGB preview.");
                viewer.SetLayerName(0, "Buildings RGB source");
                viewer.FullExtent();
            },
            ShowResult = path =>
            {
                if (!viewer.AddLayerFile(path)) throw new InvalidOperationException("Cannot display building footprints.");
                overlay = true;
                viewer.SetLayerStyleJson(0, "{\"fillColor\":\"#ef5350\",\"fillOpacity\":110,\"lineColor\":\"#ffdd55\",\"lineWidth\":1.0}");
                viewer.SetLayerName(0, "Building footprints");
                ApplyOpacity();
            },
            ClearResult = () =>
            {
                if (overlay) { viewer.RemoveLayer(0); overlay = false; }
            }
        };
        session.Changed += UpdateState;
        opacity.ValueChanged += (_, _) => ApplyOpacity();
        run.Click += async (_, _) => await (operation = session.RunAsync(provider.SelectedItem.ToString()!, threshold.Value));
        cancel.Click += (_, _) => session.Cancel();
        save.Click += async (_, _) =>
        {
            var dialog = new SaveFileDialog { Filter = "Shapefile (*.shp)|*.shp", FileName = "building-footprints.shp", AddExtension = true, OverwritePrompt = true };
            if (dialog.ShowDialog(this) == true) await (operation = session.ExportAsync(dialog.FileName));
        };
        Loaded += async (_, _) =>
        {
            viewer.ActiveTool = GeoKernelViewerTool.Pan;
            await (operation = session.InitializeAsync());
        };
        Closing += CloseAsync;
        ApplyOpacity();
        UpdateState();
    }

    private async void CloseAsync(object? sender, CancelEventArgs e)
    {
        if (closeAllowed) return;
        e.Cancel = true;
        if (closing) return;
        closing = true;
        IsEnabled = false;
        session.Cancel();
        await operation;
        session.Changed -= UpdateState;
        viewer.ClearLayers();
        overlay = false;
        await session.DisposeAsync();
        closeAllowed = true;
        // Cleanup can complete synchronously when no job exists. Leave Closing before closing again.
        _ = Dispatcher.BeginInvoke(new Action(Close));
    }

    private static void AddTool(StackPanel tools, string name, Action action)
    {
        var button = new Button { Content = name, Padding = new Thickness(10, 5, 10, 5) };
        button.Click += (_, _) => action();
        tools.Children.Add(button);
    }

    private void ApplyOpacity()
    {
        opacityText.Text = $"Output transparency: {100 - opacity.Value:F0}%";
        if (overlay)
        {
            VectorOverlay.ApplyOpacity(viewer, 0, opacity.Value);
        }
    }

    private void UpdateState()
    {
        run.IsEnabled = !session.IsWorking;
        provider.IsEnabled = !session.IsWorking;
        threshold.IsEnabled = !session.IsWorking;
        save.IsEnabled = !session.IsWorking && session.HasResult;
        cancel.IsEnabled = session.CanCancel;
        progress.Value = session.Percent;
        message.Text = session.Status;
        details.Text = session.Details;
    }
}
