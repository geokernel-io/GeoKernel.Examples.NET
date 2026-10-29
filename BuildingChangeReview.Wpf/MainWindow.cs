using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Threading;
using GeoKernel.Examples.BuildingChange;
using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.Wpf;
using GeoKernel.NET.Wpf.Controls;
using Grid = System.Windows.Controls.Grid;
using Microsoft.Win32;
namespace GeoKernel.BuildingChangeReview.Wpf;

public sealed class MainWindow : Window
{
    readonly ChangeSession session = new();
    readonly ComboBox area = new() { ItemsSource = new[] { "Demo area (fixed)", "Current view", "Full dataset" }, SelectedIndex = 0 }, filter = new() { ItemsSource = new[] { "Conservative", "Balanced" }, SelectedIndex = 0 }, provider = new() { ItemsSource = new[] { "CPU", "CUDA" }, SelectedIndex = 0 };
    readonly TextBox threshold = new() { Text = "0.4371" }, actor = new() { Text = "demo-reviewer", MaxLength = 100 }, note = new() { MaxLength = 512 };
    readonly Button run = new() { Content = "Run analysis" }, cancel = new() { Content = "Cancel" }, save = new() { Content = "Export GeoJSON" };
    readonly ProgressBar progress = new() { Maximum = 100, Height = 18 }; readonly TextBlock status = new() { TextWrapping = TextWrapping.Wrap }, summary = new() { TextWrapping = TextWrapping.Wrap };
    readonly DataGrid table = new() { AutoGenerateColumns = false, IsReadOnly = true, CanUserAddRows = false, CanUserDeleteRows = false, SelectionMode = DataGridSelectionMode.Single, MinHeight = 120, Height = 330 };
    readonly ObservableCollection<Row> rows = [];
    JsonObject? rowsResult;
    readonly List<Button> decisions = []; readonly DispatcherTimer timer = new() { Interval = TimeSpan.FromMilliseconds(100) };
    bool changing, closing, closeAllowed;
    public MainWindow()
    {
        Title = "Building change review — WPF";
        Width = 1460;
        Height = 860;
        MinWidth = 1000;
        MinHeight = 650;
        ExampleIcon.Apply(this);
        table.ItemsSource = rows;
        var root = new DockPanel();
        Content = root;
        var side = new Grid { Width = 370, Margin = new Thickness(10) };
        var scroll = new ScrollViewer { Content = side, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        DockPanel.SetDock(scroll, Dock.Right);
        root.Children.Add(scroll);
        void Add(UIElement control, double height = 0)
        {
            int row = side.RowDefinitions.Count;
            side.RowDefinitions.Add(new()
            {
                Height = height < 0 ? new GridLength(1, GridUnitType.Star) : height == 0 ? GridLength.Auto : new GridLength(height)
            });
            Grid.SetRow(control, row);
            if (control is FrameworkElement f)
                f.Margin = new Thickness(0, 3, 0, 3);
            side.Children.Add(control);
        }
        UIElement Field(string label, Control control)
        {
            var row = new Grid();
            row.ColumnDefinitions.Add(new()
            {
                Width = new GridLength(90)
            });
            row.ColumnDefinitions.Add(new());
            row.Children.Add(new TextBlock { Text = label, VerticalAlignment = VerticalAlignment.Center });
            Grid.SetColumn(control, 1);
            row.Children.Add(control);
            return row;
        }
        Add(new TextBlock { Text = "Oosterwold · 2016 → 2025", FontWeight = FontWeights.Bold }, 28);
        Add(Field("Area", area), 30);
        Add(Field("Filter", filter), 30);
        var advanced = new StackPanel();
        advanced.Children.Add(Field("Provider", provider));
        advanced.Children.Add(Field("Threshold", threshold));
        var settings = new Expander { Header = "Settings", Content = advanced };
        Add(settings);
        // A Grid inside a ScrollViewer is measured with infinite height; bound
        // the table so its own scrollbar keeps the review actions in view.
        void ResizeTable() => table.Height = Math.Max(120, ActualHeight - 530 - (settings.IsExpanded ? 50 : 0));
        SizeChanged += (_, _) => ResizeTable();
        settings.Expanded += (_, _) => ResizeTable();
        settings.Collapsed += (_, _) => ResizeTable();
        var actions = new UniformGrid { Columns = 2 };
        actions.Children.Add(run);
        actions.Children.Add(cancel);
        Add(actions, 36);
        Add(progress, 24);
        Add(summary, 44);
        foreach (var column in new[] { ("Candidate", "Candidate"), ("Type", "Type"), ("m²", "Area"), ("Review", "Review") })
            table.Columns.Add(new DataGridTextColumn { Header = column.Item1, Binding = new Binding(column.Item2), Width = column.Item1 == "Candidate" ? new DataGridLength(1, DataGridLengthUnitType.Star) : DataGridLength.Auto });
        Add(table, -1);
        Add(Field("Reviewer", actor), 30);
        Add(Field("Note", note), 30);
        var review = new UniformGrid { Columns = 3 };
        foreach (var pair in new[] { ("Approve", "approved"), ("Reject", "rejected"), ("Pending", "pending") })
        {
            var b = new Button { Content = pair.Item1, Margin = new Thickness(2) };
            b.Click += (_, _) => Guard(() => session.Review(pair.Item2, actor.Text, note.Text));
            review.Children.Add(b);
            decisions.Add(b);
        }
        Add(review, 36);
        var exports = new UniformGrid { Columns = 2 };
        var locate = new Button { Content = "Locate" };
        locate.Click += (_, _) => session.Locate();
        exports.Children.Add(locate);
        exports.Children.Add(save);
        Add(exports, 36);
        var overlays = new CheckBox { Content = "Overlays · green: new · orange: selected", IsChecked = true };
        overlays.Click += (_, _) => Guard(() =>
        {
            try
            {
                session.SetOverlays(overlays.IsChecked == true);
            }
            catch { overlays.IsChecked = session.Overlays; throw; }
        });
        Add(overlays, 26);
        var credit = new Button { Content = "Beeldmateriaal / PDOK · CC BY 4.0", ToolTip = ChangeCore.Attribution };
        credit.Click += (_, _) => MessageBox.Show(ChangeCore.Attribution + "\nhttps://www.beeldmateriaal.nl/dataroom\nhttps://creativecommons.org/licenses/by/4.0/", "Attribution");
        Add(credit, 26);
        var toolbar = new StackPanel { Orientation = Orientation.Horizontal };
        DockPanel.SetDock(toolbar, Dock.Top);
        root.Children.Add(toolbar);
        foreach (var tool in new (string, Action)[] { ("Zoom in", () => session.Views[0].ZoomIn()), ("Zoom out", () => session.Views[0].ZoomOut()), ("Demo area", () => session.Focus(ChangeCore.Demo)), ("Full extent", () => session.Focus(ChangeCore.Bounds)), ("Pan", () => session.UseTool(GeoKernelViewerTool.Pan)), ("Edit vertices", () => session.UseTool(GeoKernelViewerTool.EditVertices)), ("Undo", () => session.Undo(false)), ("Redo", () => session.Undo(true)) })
        {
            var b = new Button { Content = tool.Item1, Padding = new Thickness(8, 4, 8, 4) };
            b.Click += (_, _) => Guard(tool.Item2);
            toolbar.Children.Add(b);
        }
        DockPanel.SetDock(status, Dock.Bottom);
        root.Children.Add(status);
        var maps = new Grid();
        maps.ColumnDefinitions.Add(new());
        maps.ColumnDefinitions.Add(new());
        for (int i = 0; i < 2; i++)
        {
            var pane = new DockPanel();
            Grid.SetColumn(pane, i);
            var label = new TextBlock { Text = i == 0 ? "2016 — BEFORE" : "2025 — AFTER", TextAlignment = TextAlignment.Center, Height = 28 };
            DockPanel.SetDock(label, Dock.Top);
            pane.Children.Add(label);
            pane.Children.Add(session.Views[i]);
            maps.Children.Add(pane);
        }
        root.Children.Add(maps);
        session.Changed += UpdateState;
        run.Click += async (_, _) => { if (session.Result is not null && MessageBox.Show("Replace this session’s candidates and review decisions?", "Run analysis", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return; if (!double.TryParse(threshold.Text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double value) || value < .05 || value > .95) { MessageBox.Show("Enter a threshold from 0.05 to 0.95."); return; } await (session.Operation = session.Run(area.Text, filter.SelectedIndex == 0 ? Policy.Conservative : Policy.Balanced, value, provider.Text.ToLowerInvariant())); };
        cancel.Click += (_, _) => session.Cancel();
        save.Click += (_, _) => Guard(() => { var dialog = new SaveFileDialog { Filter = "GeoJSON (*.geojson)|*.geojson", FileName = "building_changes.geojson", OverwritePrompt = true }; if (dialog.ShowDialog(this) == true) session.Export(dialog.FileName); });
        table.SelectionChanged += (_, _) =>
        {
            if (changing || table.SelectedIndex < 0)
                return;
            Guard(() =>
            {
                try
                {
                    session.Select(table.SelectedIndex);
                }
                catch
                {
                    changing = true;
                    try
                    {
                        table.SelectedIndex = session.Selected;
                    }
                    finally { changing = false; }
                    throw;
                }
                if (session.Result is not null && session.Selected >= 0)
                    note.Text = session.Result["features"]![session.Selected]!["properties"]!["review_note"]!.GetValue<string>();
            });
        };
        table.MouseDoubleClick += (_, _) => session.Locate();
        timer.Tick += (_, _) => { if (!session.Busy && !closing) Guard(() => session.Capture(), false); };
        timer.Start();
        Loaded += async (_, _) => await (session.Operation = session.Initialize());
        Closing += Close;
        UpdateState();
    }
    void Guard(Action work, bool dialog = true)
    {
        try
        {
            work();
        }
        catch (Exception e) { status.Text = e.Message; if (dialog) MessageBox.Show(e.Message, "Building change review"); }
    }
    async void Close(object? sender, CancelEventArgs e)
    {
        if (closeAllowed)
            return;
        e.Cancel = true;
        if (closing)
            return;
        closing = true;
        IsEnabled = false;
        timer.Stop();
        await session.Close();
        closeAllowed = true;
        Close();
    }
    void UpdateState()
    {
        status.Text = session.Status;
        progress.Value = session.Percent;
        run.IsEnabled = area.IsEnabled = filter.IsEnabled = threshold.IsEnabled = provider.IsEnabled = !session.Busy;
        table.IsEnabled = !session.Busy;
        cancel.Visibility = session.Busy ? Visibility.Visible : Visibility.Collapsed;
        foreach (var button in decisions)
            button.IsEnabled = !session.Busy && session.Selected >= 0;
        save.IsEnabled = !session.Busy && session.Result is not null && session.Result["features"]!.AsArray().Any(f => f!["properties"]!["review_status"]!.GetValue<string>() == "approved");
        if (session.Result is null || session.Busy)
            return;
        changing = true;
        try
        {
            var features = session.Result["features"]!.AsArray();
            if (!ReferenceEquals(rowsResult, session.Result))
            {
                rows.Clear();
                foreach (var f in features)
                    rows.Add(new Row(f!["id"]!.GetValue<string>(), f["properties"]!["change_type"]!.GetValue<string>(), f["properties"]!["area_m2"]!.GetValue<double>().ToString("F1"), f["properties"]!["review_status"]!.GetValue<string>()));
                rowsResult = session.Result;
            }
            else
            {
                for (int i = 0; i < features.Count; i++)
                    rows[i].Update(features[i]!["properties"]!["area_m2"]!.GetValue<double>().ToString("F1"), features[i]!["properties"]!["review_status"]!.GetValue<string>());
            }
            if (table.SelectedIndex != session.Selected)
                table.SelectedIndex = session.Selected;
            summary.Text = $"{features.Count(f => f!["properties"]!["change_type"]!.GetValue<string>() == "new")} new · {features.Count(f => f!["properties"]!["change_type"]!.GetValue<string>() == "removed")} removed\n{features.Count(f => f!["properties"]!["review_status"]!.GetValue<string>() == "approved")} approved · {features.Count(f => f!["properties"]!["review_status"]!.GetValue<string>() == "pending")} pending";
        }
        finally { changing = false; }
    }
    public sealed class Row(string candidate, string type, string area, string review) : INotifyPropertyChanged
    {
        public string Candidate { get; } = candidate;
        public string Type { get; } = type;
        public string Area => area;
        public string Review => review;
        public event PropertyChangedEventHandler? PropertyChanged;
        public void Update(string nextArea, string nextReview)
        {
            if (area != nextArea)
            {
                area = nextArea;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Area)));
            }
            if (review != nextReview)
            {
                review = nextReview;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Review)));
            }
        }
    }
}
