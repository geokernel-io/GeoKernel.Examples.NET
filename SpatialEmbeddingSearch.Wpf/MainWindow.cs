using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using GeoKernel.Examples.EmbeddingSearch;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.EmbeddingSearch.Wpf;

/// <summary>
/// Places that look alike, but only in one CRS, one area and one time span: a seed cell's AlphaEarth
/// embedding is searched in a SpatialEmbeddingIndex of the Netherlands (3 years × 2 UTM zones) with CRS,
/// extent and time filters, and the panel shows how many records each filter rejected.
/// </summary>
public sealed class MainWindow : Window
{
    const string Title_ = "Spatial embedding search";

    readonly bool smoke;  // --smoke: run three searches that each show one filter, print, close
    readonly GeoKernelViewerControl viewer = new();
    readonly EmbeddingSearchSession session = new();
    readonly CancellationTokenSource cancellation = new();
    readonly ComboBox place = new(), seedYear = new(), crs = new(), area = new(), fromYear = new(), toYear = new();
    readonly TextBox maximum = new() { Text = "20" };
    readonly Button search = new() { Content = "Search", Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(0, 6, 0, 0) };
    readonly TextBlock stats = new() { TextWrapping = TextWrapping.Wrap, Text = "Search to see how many records each filter rejected." };
    readonly ListBox matches = new();
    readonly ProgressBar progress = new() { Height = 14, Margin = new Thickness(0, 6, 0, 0) };
    readonly TextBlock status = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 0), Text = "Preparing the Netherlands AlphaEarth embeddings (14 MB on the first run)…" };

    public MainWindow(bool smoke = false)
    {
        this.smoke = smoke;
        Title = Title_ + " — WPF";
        ExampleIcon.Apply(this);
        Width = 1476;
        Height = 960;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var form = new Grid();
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        form.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        void Row(string label, FrameworkElement control)
        {
            var row = form.RowDefinitions.Count;
            form.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            var text = new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 2, 8, 2) };
            control.Margin = new Thickness(0, 2, 0, 2);
            Grid.SetRow(text, row);
            Grid.SetRow(control, row);
            Grid.SetColumn(control, 1);
            form.Children.Add(text);
            form.Children.Add(control);
        }
        Row("Look for places like", place);
        Row("Seed year", seedYear);
        Row("Search in CRS", crs);
        Row("Extent", area);
        Row("From year", fromYear);
        Row("To year", toYear);
        Row("Matches", maximum);
        var top = new StackPanel();
        top.Children.Add(new GroupBox { Header = "Query", Content = new StackPanel { Children = { form, search } }, Padding = new Thickness(6) });
        top.Children.Add(new GroupBox { Header = "Filters", Content = stats, Padding = new Thickness(6), Margin = new Thickness(0, 6, 0, 0) });
        top.Children.Add(new TextBlock { Text = "Matches (click to zoom):", Margin = new Thickness(0, 8, 0, 2) });
        var bottom = new StackPanel();
        bottom.Children.Add(progress);
        bottom.Children.Add(status);
        bottom.Children.Add(new TextBlock { Text = EmbeddingSearchSession.Attribution, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.DimGray, Margin = new Thickness(0, 4, 0, 0) });
        var side = new DockPanel { Margin = new Thickness(8) };
        DockPanel.SetDock(top, Dock.Top);
        DockPanel.SetDock(bottom, Dock.Bottom);
        side.Children.Add(top);
        side.Children.Add(bottom);
        side.Children.Add(matches);

        var content = new Grid();
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(430) });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        Grid.SetColumn(viewer, 1);
        content.Children.Add(side);
        content.Children.Add(viewer);
        Content = content;
        SetControlsEnabled(false);

        place.SelectionChanged += (_, _) => FillSeedYears();
        crs.SelectionChanged += (_, _) => FillExtents();
        search.Click += (_, _) => Search(Current());
        matches.SelectionChanged += (_, _) => { if (session.MatchView(matches.SelectedIndex) is { } view) viewer.ViewExtent = view; };
        ContentRendered += async (_, _) => await PrepareAsync();
        Closing += (_, _) => cancellation.Cancel();
        Closed += (_, _) => session.Dispose();
    }

    void SetControlsEnabled(bool enabled)
    {
        foreach (var control in new Control[] { place, seedYear, crs, area, fromYear, toYear, maximum, search })
            control.IsEnabled = enabled;
    }

    async Task PrepareAsync()
    {
        var report = new Progress<DownloadProgress>(p => { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; });
        try
        {
            await session.PrepareAsync(report, cancellation.Token);
            session.AttachBackgrounds(viewer);
        }
        catch (OperationCanceledException)
        {
            return;
        }
        catch (Exception e)
        {
            status.Text = e.Message;
            if (smoke)
            {
                Console.WriteLine("smoke | error: " + e.Message);
                Environment.ExitCode = 2;
                Close();
            }
            return;
        }
        foreach (var p in session.Places)
            place.Items.Add(new ComboBoxItem { Content = $"{p["name"]} — {p["note"]}", Tag = p });
        foreach (var (value, label) in EmbeddingSearchSession.CrsChoices)
            crs.Items.Add(new ComboBoxItem { Content = label, Tag = value });
        foreach (var year in EmbeddingSearchSession.Years)
        {
            fromYear.Items.Add(year);
            toYear.Items.Add(year);
        }
        fromYear.SelectedIndex = toYear.SelectedIndex = 2;
        place.SelectedIndex = 0;
        crs.SelectedIndex = 0;
        SetControlsEnabled(true);
        progress.Value = 100;
        status.Text = $"Ready: {session.Index!.Count:N0} records (3 years × 2 UTM zones). Pick a place, a CRS, an extent and years, then Search.";
        if (smoke)
            _ = Dispatcher.BeginInvoke(RunChecks);
    }

    void FillSeedYears()
    {
        seedYear.Items.Clear();
        if ((place.SelectedItem as ComboBoxItem)?.Tag is JsonObject p)
            foreach (var year in p["years"]!.AsArray())
                seedYear.Items.Add((int)year!);
        seedYear.SelectedIndex = seedYear.Items.Count - 1;
    }

    void FillExtents()
    {
        area.Items.Clear();
        if ((crs.SelectedItem as ComboBoxItem)?.Tag is string value)
            foreach (var e in session.Extents(value))
                area.Items.Add(new ComboBoxItem { Content = (string)e["name"]!, Tag = e });
        if (area.Items.Count > 0)
            area.SelectedIndex = 0;
    }

    SearchRequest Current()
    {
        int first = (int)fromYear.SelectedItem!, last = (int)toYear.SelectedItem!;
        var count = int.TryParse(maximum.Text, out var parsed) ? Math.Clamp(parsed, 1, 200) : 20;
        return new((JsonObject)((ComboBoxItem)place.SelectedItem!).Tag, (int)seedYear.SelectedItem!, (string)((ComboBoxItem)crs.SelectedItem!).Tag,
                   (JsonObject)((ComboBoxItem)area.SelectedItem!).Tag, Math.Min(first, last), Math.Max(first, last), count);
    }

    bool Search(SearchRequest request)
    {
        var ok = session.Run(viewer, request);
        stats.Text = session.Describe(request);
        matches.Items.Clear();
        foreach (var line in session.MatchLines())
            matches.Items.Add(line);
        status.Text = $"Seed: {request.Place["name"]}, {request.SeedYear} (cell {request.Place["column"]}, {request.Place["row"]} in {request.Place["crs"]}).";
        return ok;
    }

    void RunChecks()
    {
        var all = true;
        foreach (var (label, request) in session.Checks())
        {
            var ok = Search(request);
            all &= ok;
            Console.WriteLine(session.CheckLine(label, ok));
        }
        Environment.ExitCode = all ? 0 : 2;
        _ = Dispatcher.BeginInvoke(Close);
    }
}
