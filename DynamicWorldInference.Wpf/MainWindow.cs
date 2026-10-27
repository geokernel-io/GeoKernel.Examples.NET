using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf.Controls;
using Microsoft.Win32;

namespace GeoKernel.DynamicWorldInference.Wpf;

public sealed class MainWindow : Window
{
    private readonly GeoKernelViewerControl viewer = new();
    private readonly ComboBox provider = new() { ItemsSource = new[] { "Auto", "CPU", "CUDA" }, SelectedIndex = 0 };
    private readonly Slider opacity = new() { Minimum = 0, Maximum = 100, Value = 60, Margin = new Thickness(0, 8, 0, 12) };
    private readonly TextBlock opacityText = new();
    private readonly Button run = new() { Content = "Run land-cover classification", Height = 32 };
    private readonly Button save = new() { Content = "Save Shapefile", Height = 32 };
    private readonly Button cancel = new() { Content = "Cancel", Height = 30 };
    private readonly ProgressBar progress = new() { Maximum = 100, Height = 20 };
    private readonly TextBlock message = new() { TextWrapping = TextWrapping.Wrap };
    private readonly TextBox details = new() { IsReadOnly = true, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, Height = 200, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    private readonly GeoAiSession session;
    private Task operation = Task.CompletedTask;
    private bool overlay;
    private bool closing;
    private bool closeAllowed;

    public MainWindow()
    {
        ExampleIcon.Apply(this);
        Title = "Dynamic World — WPF";
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
        side.Children.Add(new TextBlock { Text = "Google Dynamic World — land cover", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 14) });
        side.Children.Add(new TextBlock { Text = "Execution provider" });
        side.Children.Add(provider);
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
        side.Children.Add(new TextBlock { Text = "Land-cover classes\n" + GeoAiProfile.DynamicWorld.Legend, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 12, 0, 12) });
        side.Children.Add(new TextBlock { Text = "Contains modified Copernicus Sentinel data 2019.\nModel: Google / National Geographic / WRI.\nApache-2.0; see downloaded notices.", TextWrapping = TextWrapping.Wrap });
        var renderFence = new GeoAiRenderFence(viewer);
        session = new GeoAiSession(GeoAiProfile.DynamicWorld)
        {
            WaitForRenderer = renderFence.WaitAsync,
            ShowSource = path =>
            {
                viewer.ClearLayers();
                if (!viewer.AddLayerFile(path)) throw new InvalidOperationException("Cannot open source RGB preview.");
                viewer.SetLayerName(0, "Sentinel-2 RGB source");
                viewer.FullExtent();
            },
            ShowResult = path =>
            {
                if (!viewer.AddLayerFile(path)) throw new InvalidOperationException("Cannot display classification preview.");
                overlay = true;
                viewer.SetLayerName(0, "Dynamic World classification");
                ApplyOpacity();
            },
            ClearResult = () =>
            {
                if (overlay) { viewer.RemoveLayer(0); overlay = false; }
            }
        };
        session.Changed += UpdateState;
        opacity.ValueChanged += (_, _) => ApplyOpacity();
        run.Click += async (_, _) => await (operation = session.RunAsync(provider.SelectedItem.ToString()!));
        cancel.Click += (_, _) => session.Cancel();
        save.Click += async (_, _) =>
        {
            var dialog = new SaveFileDialog { Filter = "Shapefile (*.shp)|*.shp", FileName = "dynamic-world.shp", AddExtension = true, OverwritePrompt = true };
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
            // Newly loaded results are layer 0; the source remains underneath.
            viewer.SetLayerOpacity(0, opacity.Value / 100.0);
            viewer.RefreshLayers();
        }
    }

    private void UpdateState()
    {
        run.IsEnabled = !session.IsWorking;
        opacity.IsEnabled = overlay && !session.IsWorking && !closing;
        provider.IsEnabled = !session.IsWorking;
        save.IsEnabled = !session.IsWorking && session.HasResult;
        cancel.IsEnabled = session.CanCancel;
        progress.Value = session.Percent;
        message.Text = session.Status;
        details.Text = session.Details;
    }
}
