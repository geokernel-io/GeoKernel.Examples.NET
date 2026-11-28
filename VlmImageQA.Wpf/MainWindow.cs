using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;
using GeoKernel.Examples.GeoAi;
using GeoKernel.Examples.Vlm;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.ImageQA.Wpf;

/// <summary>
/// What is in this area? Pick an area of an aerial photo; Florence-2 describes it and marks objects named in plain
/// language on this computer, and a remote vision model (optional) answers questions about it.
/// </summary>
internal sealed class MainWindow : Window
{
    readonly VlmSession session = new();
    readonly GeoKernelViewerControl viewer = new();
    readonly ToggleButton pick = new() { Content = "Pick an area on the map (drag a box or click)", Margin = new Thickness(0, 6, 0, 6), Padding = new Thickness(6, 4, 6, 4) };
    readonly Button describe = new() { Content = "Describe", Margin = new Thickness(0, 2, 4, 2), Padding = new Thickness(6, 3, 6, 3) },
        detail = new() { Content = "In detail", Margin = new Thickness(0, 2, 0, 2), Padding = new Thickness(6, 3, 6, 3) },
        ask = new() { Content = "Ask", Width = 70, Margin = new Thickness(4, 2, 0, 2) },
        mark = new() { Content = "Mark", Width = 70, Margin = new Thickness(4, 2, 0, 2) };
    readonly TextBox question = new() { Text = "How many houses are in this area?", Width = 300 },
        prompt = new() { Text = "car", Width = 300 },
        answer = new() { IsReadOnly = true, TextWrapping = TextWrapping.Wrap, Height = 260, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Margin = new Thickness(0, 6, 0, 6) };
    readonly TextBlock status = new() { TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 4, 0, 4), Text = "Preparing the aerial photo (56 MB) and the Florence-2 model (630 MB) on the first run…" };
    readonly ProgressBar progress = new() { Height = 18, Maximum = 100 };
    readonly List<Button> areas = [];
    readonly DispatcherTimer poll = new() { Interval = TimeSpan.FromMilliseconds(200) };
    readonly bool smoke;  // --smoke: describe the houses, mark cars (and ask), print, close
    CancellationTokenSource? preparing;

    public MainWindow(bool smoke = false)
    {
        this.smoke = smoke;
        Title = "What is in this area? — WPF";
        ExampleIcon.Apply(this);
        Width = 1400;
        Height = 900;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var panel = new StackPanel { Margin = new Thickness(10) };
        panel.Children.Add(new TextBlock { Text = "Example areas", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 4) });
        var grid = new UniformGrid { Columns = 3 };
        for (int i = 0; i < VlmSession.Areas.Length; ++i)
        {
            int index = i;
            var button = new Button { Content = VlmSession.Areas[i].Name, ToolTip = VlmSession.Areas[i].Note, Margin = new Thickness(0, 2, 4, 2), Padding = new Thickness(6, 3, 6, 3) };
            button.Click += (_, _) => Run(() => session.Pick(index), false);
            areas.Add(button);
            grid.Children.Add(button);
        }
        panel.Children.Add(grid);
        panel.Children.Add(pick);
        var describeRow = new UniformGrid { Columns = 2 };
        describeRow.Children.Add(describe);
        describeRow.Children.Add(detail);
        panel.Children.Add(describeRow);
        StackPanel Line(Control field, Control button)
        {
            var row = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 2, 0, 2) };
            row.Children.Add(field);
            row.Children.Add(button);
            return row;
        }
        panel.Children.Add(Line(question, ask));
        panel.Children.Add(Line(prompt, mark));
        panel.Children.Add(answer);
        panel.Children.Add(progress);
        panel.Children.Add(status);
        panel.Children.Add(new TextBlock { Text = VlmSession.Help + "\n\n" + VlmSession.Credit, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 6, 0, 0) });
        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(420) });
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        var scroll = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        Grid.SetColumn(scroll, 0);
        Grid.SetColumn(viewer, 1);
        layout.Children.Add(scroll);
        layout.Children.Add(viewer);
        Content = layout;
        Enable(false);

        pick.Checked += (_, _) => session.Assistant?.SetInteractive(true);
        pick.Unchecked += (_, _) => session.Assistant?.SetInteractive(false);
        describe.Click += (_, _) => Run(() => session.Assistant!.Describe("caption"));
        detail.Click += (_, _) => Run(() => session.Assistant!.Describe("paragraph"));
        ask.Click += (_, _) => Run(() => session.Assistant!.Ask(question.Text));
        mark.Click += (_, _) => Run(() => session.Assistant!.Find(prompt.Text));
        poll.Tick += (_, _) => { if (session.NewResult() is { } result) Show(result); };
        Loaded += async (_, _) => await PrepareAsync();
        Closing += (_, _) => { preparing?.Cancel(); poll.Stop(); session.Dispose(); };
    }

    void Enable(bool enabled)
    {
        foreach (var control in new Control[] { pick, describe, detail, ask, mark, question, prompt }.Concat(areas))
            control.IsEnabled = enabled;
        if (enabled && !session.HasRemote)
        {
            ask.IsEnabled = question.IsEnabled = false;
            question.Text = "Set GEOKERNEL_LLM_PROVIDER to ask questions";
        }
    }

    async Task PrepareAsync()
    {
        preparing = new CancellationTokenSource();
        try
        {
            await session.PrepareAsync(new Progress<DownloadProgress>(p => { progress.Value = Math.Clamp(p.Percent, 0, 100); status.Text = p.Message; }), preparing.Token);
            status.Text = "Loading Florence-2…";
            await Dispatcher.InvokeAsync(() => { }, DispatcherPriority.Render);
            status.Text = session.Attach(viewer);
            progress.Value = 100;
            Enable(true);
            poll.Start();
            if (smoke)
                _ = Dispatcher.BeginInvoke(() => { session.Pick(0); Run(() => session.Assistant!.Describe("caption")); }, DispatcherPriority.Background);
        }
        catch (OperationCanceledException)
        {
            status.Text = "Cancelled.";
        }
        catch (Exception e)
        {
            status.Text = e.Message;
            if (smoke)
            {
                Console.WriteLine("error: " + e.Message);
                _ = Dispatcher.BeginInvoke(Close, DispatcherPriority.Background);
            }
        }
    }

    // Starts a run (or another action); the result arrives through the poll timer.
    void Run(Action action, bool starts = true)
    {
        try
        {
            action();
            if (!starts)
                return;
            Enable(false);
            status.Text = "Working… (Florence-2 on this computer, or the remote model for questions)";
        }
        catch (Exception e)
        {
            status.Text = e.Message;
        }
    }

    void Show(System.Text.Json.Nodes.JsonObject result)
    {
        Enable(true);
        status.Text = VlmSession.Status(result);
        answer.Text = VlmSession.Answer(result);
        if (smoke)
            Smoke(result);
    }

    void Smoke(System.Text.Json.Nodes.JsonObject result)
    {
        var kind = (string?)result["kind"];
        Console.WriteLine(kind == "find" ? $"smoke find car: {result["objects"]?.AsArray().Count} objects ({result["elapsedMs"]} ms)"
                                         : $"smoke {kind}: {result["text"] ?? result["error"]} ({result["elapsedMs"]} ms)");
        if (kind == "describe")
            Run(() => session.Assistant!.Find("car"));
        else if (kind == "find" && session.HasRemote)
            Run(() => session.Assistant!.Ask("How many houses are in this area?"));
        else
            _ = Dispatcher.BeginInvoke(Close, DispatcherPriority.Background);
    }
}
