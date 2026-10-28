using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.Sam3SegmentationInference.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    private readonly ComboBox task = Combo("Text segmentation", "Point / box segmentation");
    private readonly ComboBox provider = Combo("CPU", "CUDA", "Auto");
    private readonly ComboBox tool = Combo("Pan map", "Positive point", "Negative point", "Box: click two corners");
    private readonly TextBox text = new() { Text = "building", Width = 340 };
    private readonly NumericUpDown score = new() { Minimum = 0.01m, Maximum = 0.99m, DecimalPlaces = 2, Increment = 0.05m, Value = 0.50m, Width = 340 };
    private readonly TrackBar opacity = new() { Minimum = 0, Maximum = 100, Value = 60, Width = 340 };
    private readonly Label opacityText = Caption("");
    private readonly Label selection = Caption("0 points · no box");
    private readonly Button clear = Button("Clear prompts");
    private readonly Button generate = Button("Generate draft");
    private readonly Button accept = Button("Accept draft");
    private readonly Button discard = Button("Discard result");
    private readonly Button saveShape = Button("Save accepted Shapefile");
    private readonly Button saveRaster = Button("Save accepted GeoTIFF");
    private readonly Button cancel = Button("Cancel");
    private readonly ProgressBar progress = new() { Maximum = 100, Width = 340 };
    private readonly Label message = Caption("");
    private readonly TextBox details = new() { Multiline = true, ReadOnly = true, Width = 340, Height = 130, ScrollBars = ScrollBars.Vertical };
    private readonly Sam3Session session;
    private readonly Sam3MapView map;
    private Task operation = Task.CompletedTask;
    private bool closing;
    private bool closeAllowed;

    public MainForm()
    {
        ExampleIcon.Apply(this);
        Text = "SAM3 Segmentation — WinForms";
        Width = 1280;
        Height = 950;
        MinimumSize = new Size(900, 700);
        var side = new FlowLayoutPanel { Dock = DockStyle.Right, Width = 380, Padding = new Padding(14), FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true };
        side.Controls.Add(Caption("Meta SAM3 — text / point / box segmentation"));
        foreach (var pair in new (string, Control)[] { ("Task", task), ("Execution provider", provider), ("Text concept", text), ("Text score threshold", score), ("Map tool", tool) })
        {
            side.Controls.Add(Caption(pair.Item1));
            side.Controls.Add(pair.Item2);
        }
        foreach (Control control in new Control[] { selection, clear, opacityText, opacity, generate, accept, discard, saveShape, saveRaster, cancel, progress, message, details }) side.Controls.Add(control);
        side.Controls.Add(Caption("Point/box selects one object. A large box does not find every building inside.\nModel: Meta SAM3; retain SAM License and attribution.\nImagery source/license not verified."));
        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden };
        tools.Items.Add("Zoom In", null, (_, _) => viewer.ZoomIn());
        tools.Items.Add("Zoom Out", null, (_, _) => viewer.ZoomOut());
        tools.Items.Add("Full Extent", null, (_, _) => viewer.FullExtent());
        Controls.Add(viewer);
        Controls.Add(side);
        Controls.Add(tools);
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
        task.SelectedIndexChanged += (_, _) => { map.ClearPrompts(); tool.SelectedIndex = 0; UpdateState(); };
        tool.SelectedIndexChanged += (_, _) => map.SetTool((Sam3MapTool)tool.SelectedIndex);
        clear.Click += (_, _) => map.ClearPrompts();
        opacity.ValueChanged += (_, _) => { opacityText.Text = $"Output transparency: {100 - opacity.Value}%"; map.ApplyOpacity(opacity.Value); };
        generate.Click += async (_, _) =>
        {
            if (task.SelectedIndex == 1 && map.Prompts.FirstCorner.HasValue) { selection.Text = "Complete the box or clear prompts first."; return; }
            await (operation = session.GenerateAsync(new(task.SelectedIndex == 0, provider.Text, text.Text, (double)score.Value,
                map.Prompts.Points.ToArray(), map.Prompts.Box)));
        };
        accept.Click += (_, _) => session.Accept();
        discard.Click += async (_, _) => await (operation = session.DiscardAsync());
        cancel.Click += (_, _) => session.Cancel();
        saveShape.Click += async (_, _) => await SaveAsync(false);
        saveRaster.Click += async (_, _) => await SaveAsync(true);
        Shown += async (_, _) => { map.SetTool(Sam3MapTool.Pan); await (operation = session.InitializeAsync()); };
        FormClosing += async (_, e) =>
        {
            if (closeAllowed) return;
            e.Cancel = true;
            if (closing) return;
            closing = true;
            Enabled = false;
            session.Cancel();
            await operation;
            session.Changed -= UpdateState;
            map.ClearResult();
            viewer.ClearLayers();
            await session.DisposeAsync();
            closeAllowed = true;
            Close();
        };
        opacityText.Text = "Output transparency: 40%";
        UpdateState();
    }

    private async Task SaveAsync(bool raster)
    {
        using var dialog = new SaveFileDialog { Filter = raster ? "GeoTIFF (*.tif)|*.tif" : "Shapefile (*.shp)|*.shp", FileName = raster ? "sam3-instances.tif" : "sam3-instances.shp", AddExtension = true, OverwritePrompt = true };
        if (dialog.ShowDialog(this) == DialogResult.OK)
            await (operation = session.ExportAsync(dialog.FileName, raster ? "geotiff" : "shapefile"));
    }

    private void UpdateState()
    {
        bool editable = !session.IsWorking && !session.HasResult;
        task.Enabled = provider.Enabled = generate.Enabled = editable;
        text.Enabled = score.Enabled = editable && task.SelectedIndex == 0;
        tool.Enabled = clear.Enabled = editable && task.SelectedIndex == 1;
        accept.Enabled = !session.IsWorking && session.HasResult && !session.Accepted;
        discard.Enabled = !session.IsWorking && session.HasResult;
        saveShape.Enabled = saveRaster.Enabled = !session.IsWorking && session.Accepted;
        cancel.Enabled = session.CanCancel;
        progress.Value = session.Percent;
        message.Text = session.Status;
        details.Text = session.Details.Replace("\n", Environment.NewLine);
    }

    private static Label Caption(string value) => new() { Text = value, AutoSize = true, MaximumSize = new Size(340, 0) };
    private static Button Button(string value) => new() { Text = value, Width = 340, Height = 28 };
    private static ComboBox Combo(params string[] values)
    {
        var combo = new ComboBox { Width = 340, DropDownStyle = ComboBoxStyle.DropDownList };
        combo.Items.AddRange(values);
        combo.SelectedIndex = 0;
        return combo;
    }
}
