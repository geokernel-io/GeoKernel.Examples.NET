using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using GeoKernel.Examples.AgentPlanner;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AgentPlannerBasics.Wpf;

/// <summary>
/// How a natural-language request becomes a safe, recorded map operation: five lessons send requests
/// through the planner's stages (request → proposal → validation → execution → provenance) and show
/// what each stage guarantees. The lessons run offline; your own requests need GEOKERNEL_LLM_PROVIDER.
/// </summary>
public sealed class MainWindow : Window
{
    const string Title_ = "Agent Planner Basics";

    readonly bool smoke;  // --smoke: run the five lessons, print them, close
    readonly GeoKernelViewerControl viewer = new();
    readonly TextBox log = new()
    {
        IsReadOnly = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        FontFamily = new FontFamily("Consolas"), FontSize = 12,
    };
    readonly List<Button> lessonButtons = [];
    readonly TextBox request = new() { VerticalContentAlignment = VerticalAlignment.Center };
    readonly Button plan = Make("Plan"), run = Make("Run"), undo = Make("Undo all"), clear = Make("Clear log");
    readonly TextBlock status = new(), planner = new();
    PlannerLessons? lessons;
    bool busy;

    static Button Make(string text) => new() { Content = new TextBlock { Text = text }, Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(4, 0, 0, 0), MinWidth = 70 };

    public MainWindow(bool smoke = false)
    {
        this.smoke = smoke;
        Title = Title_;
        ExampleIcon.Apply(this);
        Width = 1456;
        Height = 940;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        var side = new DockPanel { Margin = new Thickness(8) };
        var top = new StackPanel();
        top.Children.Add(new TextBlock
        {
            Text = "Each lesson sends a request through the planner's five stages: ① request → ② proposal → ③ validation → "
                   + "④ execution → ⑤ provenance. A lesson starts from the original map; its result stays on the map.",
            TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 6),
        });
        for (var i = 0; i < PlannerLessons.Titles.Length; i++)
        {
            var index = i;
            var button = new Button
            {
                Content = new TextBlock { Text = PlannerLessons.Titles[i] }, HorizontalContentAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(8, 4, 8, 4), Margin = new Thickness(0, 0, 0, 4),
            };
            button.Click += (_, _) => RunLesson(index);
            lessonButtons.Add(button);
            top.Children.Add(button);
        }
        top.Children.Add(new TextBlock { Text = "Your own request:", Margin = new Thickness(0, 8, 0, 2) });
        var entry = new DockPanel { Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(run, Dock.Right);
        DockPanel.SetDock(plan, Dock.Right);
        entry.Children.Add(run);
        entry.Children.Add(plan);
        entry.Children.Add(request);
        top.Children.Add(entry);
        var bottom = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(0, 8, 0, 0) };
        undo.Margin = new Thickness(0);
        bottom.Children.Add(undo);
        bottom.Children.Add(clear);
        DockPanel.SetDock(top, Dock.Top);
        DockPanel.SetDock(bottom, Dock.Bottom);
        side.Children.Add(top);
        side.Children.Add(bottom);
        side.Children.Add(log);

        var content = new Grid();
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(600) });
        var splitter = new GridSplitter { Width = 4, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Gainsboro };
        Grid.SetColumn(splitter, 1);
        Grid.SetColumn(side, 2);
        content.Children.Add(viewer);
        content.Children.Add(splitter);
        content.Children.Add(side);

        var statusBar = new StatusBar();
        statusBar.Items.Add(new StatusBarItem { Content = status });
        statusBar.Items.Add(new StatusBarItem { Content = planner, HorizontalAlignment = HorizontalAlignment.Right });
        var root = new DockPanel();
        DockPanel.SetDock(statusBar, Dock.Bottom);
        root.Children.Add(statusBar);
        root.Children.Add(content);
        Content = root;

        plan.Click += async (_, _) => await PlanAsync();
        request.KeyDown += async (_, e) => { if (e.Key == Key.Enter) { e.Handled = true; await PlanAsync(); } };
        run.Click += (_, _) => { lessons?.RunPending(); viewer.RefreshLayers(); RefreshButtons(); };
        undo.Click += (_, _) => { lessons?.Reset(); viewer.RefreshLayers(); status.Text = "Every lesson result was undone"; };
        clear.Click += (_, _) => log.Clear();
        ContentRendered += (_, _) => Initialize();
        Closed += (_, _) => lessons?.Dispose();
        RefreshButtons();
    }

    void Initialize()
    {
        try
        {
            lessons = new PlannerLessons(viewer);
        }
        catch (Exception e)
        {
            if (smoke)
                Console.WriteLine("error: " + e.Message);
            MessageBox.Show(this, "The map could not be prepared:\n" + e.Message, Title_, MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        lessons.Logged += Append;
        planner.Text = lessons.PlannerDescription;
        if (lessons.UsesLlm)
            request.Text = "Show the parks with more than 50 trees in dark green";
        else
            request.ToolTip = "Set GEOKERNEL_LLM_PROVIDER (anthropic, openai or ollama) to plan your own requests";
        RefreshButtons();
        if (smoke)
            Dispatcher.BeginInvoke(RunSmoke);
    }

    bool RunLesson(int index)
    {
        if (lessons is null || busy)
            return false;
        var ok = lessons.Run(index);
        viewer.RefreshLayers();
        status.Text = $"Lesson {index + 1} " + (ok ? "done" : "did not behave as described");
        RefreshButtons();
        return ok;
    }

    void RunSmoke()
    {
        var results = Enumerable.Range(0, PlannerLessons.Titles.Length).Select(RunLesson).ToList();
        for (var i = 0; i < results.Count; i++)
            Console.WriteLine($"smoke | lesson {i + 1}: {(results[i] ? "ok" : "FAILED")}");
        Environment.ExitCode = results.All(ok => ok) ? 0 : 2;
        Dispatcher.BeginInvoke(Close);
    }

    async Task PlanAsync()
    {
        if (lessons is null || busy || !lessons.UsesLlm || request.Text.Trim().Length == 0)
            return;
        busy = true;
        status.Text = "Asking the model for a plan…";
        RefreshButtons();
        await lessons.ProposeAsync(request.Text.Trim());
        busy = false;
        status.Text = "";
        RefreshButtons();
    }

    void RefreshButtons()
    {
        var ready = lessons is not null && !busy;
        foreach (var button in lessonButtons)
            button.IsEnabled = ready;
        request.IsEnabled = plan.IsEnabled = ready && lessons!.UsesLlm;
        run.IsEnabled = ready && lessons!.HasPending;
        ((TextBlock)run.Content).Text = lessons is not null && lessons.PendingNeedsConfirmation ? "Confirm and run" : "Run";
        undo.IsEnabled = ready;
    }

    void Append(string line)
    {
        log.AppendText((log.Text.Length > 0 ? Environment.NewLine : "") + line);
        log.ScrollToEnd();
        if (smoke)
            Console.WriteLine(line);
    }
}
