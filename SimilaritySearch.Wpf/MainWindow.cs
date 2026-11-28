using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using GeoKernel.Examples.GeoAi;
using GeoKernel.Examples.Similarity;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.FindSimilar.Wpf;

/// <summary>
/// Find similar: pick a solar park, a lake, a town or a field (or click / drag on the map) and every
/// 80 m cell of Flevoland is ranked by AlphaEarth embedding similarity, with a heat map and ranked matches.
/// </summary>
internal sealed class MainWindow : Window
{
    readonly SimilaritySession session = new();
    readonly GeoKernelViewerControl viewer = new();
    readonly ToggleButton interactive = new() { Content = "Find similar on the map (click or drag a box)", IsEnabled = false, Margin = new Thickness(0, 6, 0, 6), Padding = new Thickness(6, 4, 6, 4) };
    readonly Slider threshold = new() { Minimum = 0.5, Maximum = 0.99, Value = 0.9, SmallChange = 0.01, LargeChange = 0.05, Width = 220 };
    readonly TextBlock thresholdLabel = new() { Text = "Heat map ≥ 0.90", Width = 120, VerticalAlignment = VerticalAlignment.Center },
        status = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4), Text = "Preparing the Flevoland AlphaEarth embeddings (31 MB on the first run)…" };
    readonly ListBox matches = new() { Height = 330 };
    readonly ProgressBar progress = new() { Height = 18, Maximum = 100 };
    readonly List<Button> presets = [];
    readonly DispatcherTimer poll = new() { Interval = TimeSpan.FromMilliseconds(200) };
    readonly bool smoke;  // --smoke: search the solar park, print the result, close
    CancellationTokenSource? preparing;

    public MainWindow(bool smoke = false)
    {
        this.smoke = smoke;
        Title = "Find similar — WPF";
        ExampleIcon.Apply(this);
        Width = 1400;
        Height = 900;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var panel = new StackPanel { Margin = new Thickness(10) };
        panel.Children.Add(new TextBlock { Text = "Look for places like…", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) });
        var grid = new UniformGrid { Columns = 2 };
        for (int i = 0; i < SimilaritySession.Presets.Length; ++i)
        {
            int index = i;
            var button = new Button { Content = SimilaritySession.Presets[i].Name, ToolTip = SimilaritySession.Presets[i].Note, IsEnabled = false, Margin = new Thickness(0, 2, 4, 2), Padding = new Thickness(6, 3, 6, 3) };
            button.Click += (_, _) => SearchPreset(index);
            presets.Add(button);
            grid.Children.Add(button);
        }
        panel.Children.Add(grid);
        panel.Children.Add(interactive);
        var row = new StackPanel { Orientation = Orientation.Horizontal };
        row.Children.Add(thresholdLabel);
        row.Children.Add(threshold);
        panel.Children.Add(row);
        panel.Children.Add(matches);
        panel.Children.Add(progress);
        panel.Children.Add(status);
        panel.Children.Add(new TextBlock { Text = SimilaritySession.Help + "\n\n" + SimilaritySession.Attribution, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) });
        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(400) });
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var scroll = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Grid.SetColumn(scroll, 0);
        Grid.SetColumn(viewer, 1);
        layout.Children.Add(scroll);
        layout.Children.Add(viewer);
        Content = layout;

        interactive.Checked += (_, _) => session.Search?.SetInteractive(true);
        interactive.Unchecked += (_, _) => session.Search?.SetInteractive(false);
        threshold.ValueChanged += (_, _) =>
        {
            thresholdLabel.Text = $"Heat map ≥ {threshold.Value:0.00}";
            session.Search?.SetHeatmapThreshold(threshold.Value);
        };
        matches.SelectionChanged += (_, _) => { if (matches.SelectedIndex >= 0) session.Search?.ZoomToMatch(matches.SelectedIndex); };
        poll.Tick += (_, _) => { if (session.NewInteractiveResult() is { } result) Show(result); };
        Loaded += async (_, _) => await PrepareAsync();
        Closing += (_, _) => { preparing?.Cancel(); poll.Stop(); session.Dispose(); };
    }

    async Task PrepareAsync()
    {
        preparing = new CancellationTokenSource();
        try
        {
            await session.PrepareAsync(new Progress<DownloadProgress>(p => { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; }), preparing.Token);
            status.Text = session.Attach(viewer);
            progress.Value = 100;
            foreach (var button in presets)
                button.IsEnabled = true;
            interactive.IsEnabled = true;
            poll.Start();
            if (smoke)
                _ = Dispatcher.BeginInvoke(() => SearchPreset(0), DispatcherPriority.Background);
        }
        catch (OperationCanceledException)
        {
            status.Text = "Cancelled.";
        }
        catch (Exception e)
        {
            status.Text = e.Message;
        }
    }

    void SearchPreset(int index)
    {
        try
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var (result, view) = session.SearchPreset(index);
            Show(result);
            viewer.ViewExtent = view;
            if (smoke)
            {
                Console.WriteLine($"smoke: {SimilaritySession.Presets[index].Name}: {SimilaritySession.Summary(result)} ({watch.ElapsedMilliseconds} ms with the view)");
                _ = Dispatcher.BeginInvoke(Close, DispatcherPriority.Background);
            }
        }
        catch (Exception e)
        {
            status.Text = e.Message;
        }
    }

    void Show(System.Text.Json.Nodes.JsonObject result)
    {
        matches.ItemsSource = SimilaritySession.MatchLines(result).ToList();
        status.Text = SimilaritySession.Summary(result);
    }
}
