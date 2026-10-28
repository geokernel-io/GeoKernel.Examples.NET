using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf.Controls;
using Microsoft.Win32;

namespace GeoKernel.Sam3SegmentationInference.Wpf;

public sealed class MainWindow : Window
{
    private readonly GeoKernelViewerControl viewer = new();
    private readonly ComboBox task = Combo("Text segmentation", "Point / box segmentation");
    private readonly ComboBox provider = Combo("CPU", "CUDA", "Auto");
    private readonly ComboBox tool = Combo("Pan map", "Positive point", "Negative point", "Box: click two corners");
    private readonly TextBox text = new() { Text = "building" };
    private readonly Slider score = new() { Minimum = 0.01, Maximum = 0.99, Value = 0.5, TickFrequency = 0.01, IsSnapToTickEnabled = true };
    private readonly TextBlock scoreText = Caption("Text score threshold: 0.50");
    private readonly Slider opacity = new() { Minimum = 0, Maximum = 100, Value = 60 };
    private readonly TextBlock opacityText = Caption("Output transparency: 40%");
    private readonly TextBlock selection = Caption("0 points · no box");
    private readonly Button clear = Button("Clear prompts");
    private readonly Button generate = Button("Generate draft");
    private readonly Button accept = Button("Accept draft");
    private readonly Button discard = Button("Discard result");
    private readonly Button saveShape = Button("Save accepted Shapefile");
    private readonly Button saveRaster = Button("Save accepted GeoTIFF");
    private readonly Button cancel = Button("Cancel");
    private readonly ProgressBar progress = new() { Maximum = 100, Height = 20 };
    private readonly TextBlock message = Caption("");
    private readonly TextBox details = new() { IsReadOnly = true, TextWrapping = TextWrapping.Wrap, AcceptsReturn = true, Height = 130, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
    private readonly Sam3Session session;
    private readonly Sam3MapView map;
    private Task operation = Task.CompletedTask;
    private bool closing;
    private bool closeAllowed;

    public MainWindow()
    {
        ExampleIcon.Apply(this);
        Title = "SAM3 Segmentation — WPF";
        Width = 1280;
        Height = 950;
        MinWidth = 900;
        MinHeight = 700;
        var root = new DockPanel();
        var side = new StackPanel { Margin = new Thickness(14) };
        var scroll = new ScrollViewer { Content = side, Width = 380, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        DockPanel.SetDock(scroll, Dock.Right);
        var tools = new StackPanel { Orientation = Orientation.Horizontal };
        DockPanel.SetDock(tools, Dock.Top);
        AddTool(tools, "Zoom In", () => viewer.ZoomIn());
        AddTool(tools, "Zoom Out", () => viewer.ZoomOut());
        AddTool(tools, "Full Extent", () => viewer.FullExtent());
        root.Children.Add(tools);
        root.Children.Add(scroll);
        root.Children.Add(viewer);
        Content = root;
        side.Children.Add(Caption("Meta SAM3 — text / point / box segmentation"));
        foreach (var pair in new (string, UIElement)[] { ("Task", task), ("Execution provider", provider), ("Text concept", text) })
        {
            side.Children.Add(Caption(pair.Item1));
            side.Children.Add(pair.Item2);
        }
        foreach (UIElement element in new UIElement[] { scoreText, score, Caption("Map tool"), tool, selection, clear, opacityText, opacity, generate, accept, discard, saveShape, saveRaster, cancel, progress, message, details })
            side.Children.Add(element);
        side.Children.Add(Caption("Point/box selects one object. A large box does not find every building inside.\nModel: Meta SAM3; retain SAM License and attribution.\nImagery source/license not verified."));
        map = new Sam3MapView(viewer);
        var renderFence = new GeoAiRenderFence(viewer);
        session = new Sam3Session
        {
            WaitForRenderer = renderFence.WaitAsync,
            ShowSource = map.ShowSource,
            ShowResult = path => { map.ShowResult(path); map.ApplyOpacity(opacity.Value); },
            ClearResult = map.ClearResult
        };
        map.CanSelect = () => !session.IsWorking && !session.HasResult && task.SelectedIndex == 1;
        map.SelectionChanged += () => selection.Text = map.SelectionText;
        session.Changed += UpdateState;
        task.SelectionChanged += (_, _) => { map.ClearPrompts(); tool.SelectedIndex = 0; UpdateState(); };
        tool.SelectionChanged += (_, _) => map.SetTool((Sam3MapTool)tool.SelectedIndex);
        clear.Click += (_, _) => map.ClearPrompts();
        score.ValueChanged += (_, _) => scoreText.Text = $"Text score threshold: {score.Value:F2}";
        opacity.ValueChanged += (_, _) => { opacityText.Text = $"Output transparency: {100 - opacity.Value:F0}%"; map.ApplyOpacity(opacity.Value); };
        generate.Click += async (_, _) =>
        {
            if (task.SelectedIndex == 1 && map.Prompts.FirstCorner.HasValue) { selection.Text = "Complete the box or clear prompts first."; return; }
            await (operation = session.GenerateAsync(new(task.SelectedIndex == 0, provider.SelectedItem.ToString()!, text.Text, score.Value,
                map.Prompts.Points.ToArray(), map.Prompts.Box)));
        };
        accept.Click += (_, _) => session.Accept();
        discard.Click += async (_, _) => await (operation = session.DiscardAsync());
        cancel.Click += (_, _) => session.Cancel();
        saveShape.Click += async (_, _) => await SaveAsync(false);
        saveRaster.Click += async (_, _) => await SaveAsync(true);
        Loaded += async (_, _) => { map.SetTool(Sam3MapTool.Pan); await (operation = session.InitializeAsync()); };
        Closing += CloseAsync;
        UpdateState();
    }

    private async Task SaveAsync(bool raster)
    {
        var dialog = new SaveFileDialog { Filter = raster ? "GeoTIFF (*.tif)|*.tif" : "Shapefile (*.shp)|*.shp", FileName = raster ? "sam3-instances.tif" : "sam3-instances.shp", AddExtension = true, OverwritePrompt = true };
        if (dialog.ShowDialog(this) == true)
            await (operation = session.ExportAsync(dialog.FileName, raster ? "geotiff" : "shapefile"));
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
        map.ClearResult();
        viewer.ClearLayers();
        await session.DisposeAsync();
        closeAllowed = true;
        // Cleanup can complete synchronously when no job exists. Leave Closing before closing again.
        _ = Dispatcher.BeginInvoke(new Action(Close));
    }

    private void UpdateState()
    {
        bool editable = !session.IsWorking && !session.HasResult;
        task.IsEnabled = provider.IsEnabled = generate.IsEnabled = editable;
        text.IsEnabled = score.IsEnabled = editable && task.SelectedIndex == 0;
        tool.IsEnabled = clear.IsEnabled = editable && task.SelectedIndex == 1;
        accept.IsEnabled = !session.IsWorking && session.HasResult && !session.Accepted;
        discard.IsEnabled = !session.IsWorking && session.HasResult;
        saveShape.IsEnabled = saveRaster.IsEnabled = !session.IsWorking && session.Accepted;
        cancel.IsEnabled = session.CanCancel;
        progress.Value = session.Percent;
        message.Text = session.Status;
        details.Text = session.Details;
    }

    private static void AddTool(StackPanel tools, string name, Action action)
    {
        var button = new Button { Content = name, Padding = new Thickness(10, 5, 10, 5) };
        button.Click += (_, _) => action();
        tools.Children.Add(button);
    }
    private static TextBlock Caption(string value) => new() { Text = value, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4) };
    private static Button Button(string value) => new() { Content = value, Height = 28, Margin = new Thickness(0, 2, 0, 2) };
    private static ComboBox Combo(params string[] values) => new() { ItemsSource = values, SelectedIndex = 0 };
}
