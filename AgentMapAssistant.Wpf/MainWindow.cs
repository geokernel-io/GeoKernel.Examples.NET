using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using GeoKernel.Examples.AgentMap;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf.Controls;

namespace GeoKernel.AgentMapAssistant.Wpf;

/// <summary>
/// Natural-language map requests as validated, previewed and recorded workflows. A planner (an LLM or
/// the offline demo) proposes a workflow DSL v2 plan; GeoKernel validates it, the plan is shown before
/// anything runs, steps that change data need the user's confirmation of that exact plan, and every
/// executed workflow is undoable and recorded with hash-chained provenance.
/// </summary>
public sealed class MainWindow : Window
{
    const string Title_ = "Agent Map Assistant";

    readonly AgentDemo demo = AgentDemo.Load();
    readonly GeoKernelViewerControl viewer = new();
    readonly ComboBox requests = new() { Width = 520, Margin = new Thickness(4, 0, 4, 0) };
    readonly Button ask = Make("Ask"), fullExtent = Make("Full Extent"), provenance = Make("Provenance..."),
        discard = Make("Discard"), run = Make("Run"), send = Make("Send"), undo = Make("Undo last");
    readonly RichTextBox log = new() { IsReadOnly = true, VerticalScrollBarVisibility = ScrollBarVisibility.Auto, Document = new FlowDocument { PagePadding = new Thickness(4) } };
    readonly TextBox plan = new()
    {
        IsReadOnly = true, TextWrapping = TextWrapping.Wrap, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        FontFamily = new FontFamily("Consolas"), FontSize = 12,
    };
    readonly TextBox input = new() { VerticalContentAlignment = VerticalAlignment.Center };
    readonly TextBlock status = new(), planner = new();
    AgentConversation? conversation;

    static Button Make(string text) => new() { Content = new TextBlock { Text = text }, Padding = new Thickness(10, 3, 10, 3), Margin = new Thickness(4, 0, 0, 0), MinWidth = 70 };

    public MainWindow()
    {
        Title = Title_;
        ExampleIcon.Apply(this);
        Width = 1376;
        Height = 860;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        foreach (var item in demo.Requests)
            requests.Items.Add(item.Request);
        requests.SelectedIndex = 0;
        var toolbar = new ToolBar();
        ToolBarTray.SetIsLocked(toolbar, true);
        toolbar.Items.Add(new TextBlock { Text = "Try:", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(4, 0, 0, 0) });
        toolbar.Items.Add(requests);
        toolbar.Items.Add(ask);
        toolbar.Items.Add(new Separator());
        toolbar.Items.Add(fullExtent);
        toolbar.Items.Add(provenance);

        var planButtons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 6, 0, 0) };
        planButtons.Children.Add(discard);
        planButtons.Children.Add(run);
        var planPanel = new DockPanel { Margin = new Thickness(4) };
        DockPanel.SetDock(planButtons, Dock.Bottom);
        planPanel.Children.Add(planButtons);
        planPanel.Children.Add(plan);
        var planBox = new GroupBox { Header = "Plan", Content = planPanel, Margin = new Thickness(0, 8, 0, 0) };

        var entry = new DockPanel { Margin = new Thickness(0, 8, 0, 0) };
        DockPanel.SetDock(send, Dock.Right);
        entry.Children.Add(send);
        entry.Children.Add(input);

        var side = new Grid { Margin = new Thickness(8) };
        side.RowDefinitions.Add(new RowDefinition { Height = new GridLength(58, GridUnitType.Star) });
        side.RowDefinitions.Add(new RowDefinition { Height = new GridLength(42, GridUnitType.Star) });
        side.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        side.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        undo.HorizontalAlignment = HorizontalAlignment.Left;
        undo.Margin = new Thickness(0, 8, 0, 0);
        Grid.SetRow(planBox, 1);
        Grid.SetRow(entry, 2);
        Grid.SetRow(undo, 3);
        side.Children.Add(log);
        side.Children.Add(planBox);
        side.Children.Add(entry);
        side.Children.Add(undo);

        var content = new Grid();
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        content.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(430) });
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
        DockPanel.SetDock(toolbar, Dock.Top);
        DockPanel.SetDock(statusBar, Dock.Bottom);
        root.Children.Add(toolbar);
        root.Children.Add(statusBar);
        root.Children.Add(content);
        Content = root;

        ask.Click += async (_, _) => { if (conversation is not null) await conversation.SubmitAsync(requests.SelectedItem as string ?? ""); };
        send.Click += async (_, _) => await SendTyped();
        input.KeyDown += async (_, e) => { if (e.Key == Key.Enter) { e.Handled = true; await SendTyped(); } };
        run.Click += (_, _) => conversation?.RunPending();
        discard.Click += (_, _) => conversation?.Discard();
        undo.Click += (_, _) => conversation?.UndoLast();
        fullExtent.Click += (_, _) => viewer.FullExtent();
        provenance.Click += (_, _) => ShowProvenance();
        ContentRendered += (_, _) => Initialize();
        Closed += (_, _) => conversation?.Dispose();
        Refresh(null);
    }

    void Initialize()
    {
        try
        {
            demo.LoadNeighbourhood(viewer);
            conversation = new AgentConversation(viewer, demo);
        }
        catch (Exception e)
        {
            MessageBox.Show(this, "The map could not be prepared:\n" + e.Message, Title_, MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        conversation.Logged += Append;
        conversation.Changed += () => Refresh(conversation);
        conversation.Status += text => status.Text = text;
        planner.Text = conversation.PlannerDescription;
        Refresh(conversation);
    }

    async Task SendTyped()
    {
        if (conversation is not null && await conversation.SubmitAsync(input.Text))
            input.Clear();
    }

    void Refresh(AgentConversation? c)
    {
        var ready = c is not null && !c.Busy;
        ask.IsEnabled = send.IsEnabled = ready;
        run.IsEnabled = discard.IsEnabled = ready && c!.HasPlan;
        undo.IsEnabled = ready && c!.CanUndo;
        ((TextBlock)run.Content).Text = c is not null && c.NeedsConfirmation ? "Confirm and run" : "Run";
        plan.Text = c?.PlanText ?? "";
    }

    void Append(LogEntry entry)
    {
        var paragraph = new Paragraph { Margin = new Thickness(0) };
        paragraph.Inlines.Add(new Bold(new Run(entry.Who + ": ")));
        var text = new Run(entry.Text);
        if (entry.Color is not null)
            text.Foreground = (Brush)new BrushConverter().ConvertFromString(entry.Color)!;
        paragraph.Inlines.Add(text);
        log.Document.Blocks.Add(paragraph);
        log.ScrollToEnd();
    }

    void ShowProvenance()
    {
        if (conversation is null)
            return;
        var (summary, json) = conversation.Provenance();
        var panel = new DockPanel { Margin = new Thickness(8) };
        var header = new TextBlock { Text = summary, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(0, 0, 0, 8) };
        DockPanel.SetDock(header, Dock.Top);
        panel.Children.Add(header);
        panel.Children.Add(new TextBox
        {
            Text = json, IsReadOnly = true, FontFamily = new FontFamily("Consolas"),
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto, VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
        });
        new Window { Title = "Provenance", Owner = this, Width = 760, Height = 560, WindowStartupLocation = WindowStartupLocation.CenterOwner, Content = panel, ShowInTaskbar = false }.ShowDialog();
    }
}
