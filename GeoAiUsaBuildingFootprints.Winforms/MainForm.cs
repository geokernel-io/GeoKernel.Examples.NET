using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.GeoAiUsaBuildingFootprints.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    private readonly ComboBox provider = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 340 };
    private readonly TrackBar opacity = new() { Minimum = 0, Maximum = 100, Value = 60, TickFrequency = 10, Width = 340 };
    private readonly Label opacityText = new() { AutoSize = true };
    private readonly Button run = new() { Text = "Run building segmentation inference", Width = 340, Height = 32 };
    private readonly Button save = new() { Text = "Save Shapefile", Width = 340, Height = 32 };
    private readonly Button cancel = new() { Text = "Cancel", Width = 340, Height = 30 };
    private readonly ProgressBar progress = new() { Maximum = 100, Width = 340 };
    private readonly Label message = new() { AutoSize = true, MaximumSize = new Size(340, 0) };
    private readonly TextBox details = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Width = 340, Height = 200 };
    private readonly NumericUpDown threshold = new() { Minimum = 0.01m, Maximum = 0.99m, DecimalPlaces = 2, Increment = 0.05m, Value = 0.5m, Width = 340 };
    private readonly GeoAiSession session;
    private Task operation = Task.CompletedTask;
    private bool overlay;
    private bool closing;
    private bool closeAllowed;

    public MainForm()
    {
        ExampleIcon.Apply(this);
        Text = "GeoAI USA Building Footprints — WinForms";
        Width = 1280;
        Height = 850;
        MinimumSize = new Size(900, 650);
        provider.Items.AddRange(["Auto", "CPU", "CUDA"]);
        provider.SelectedIndex = 0;
        var side = new FlowLayoutPanel
        {
            Dock = DockStyle.Right, Width = 380, Padding = new Padding(14),
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
        };
        side.Controls.Add(new Label { Text = "GeoAI USA — Mask R-CNN building footprints", AutoSize = true, Font = new Font(Font, FontStyle.Bold) });
        side.Controls.Add(new Label { Text = "Execution provider", AutoSize = true });
        side.Controls.Add(provider);
        side.Controls.Add(new Label { Text = "Confidence threshold", AutoSize = true });
        side.Controls.Add(threshold);
        side.Controls.Add(opacityText);
        side.Controls.Add(opacity);
        side.Controls.Add(run);
        side.Controls.Add(save);
        side.Controls.Add(cancel);
        side.Controls.Add(progress);
        side.Controls.Add(message);
        side.Controls.Add(new Label { Text = "Inference diagnostics", AutoSize = true });
        side.Controls.Add(details);
        side.Controls.Add(new Label { Text = "Output\n" + GeoAiProfile.GeoAiUsa.Legend, AutoSize = true, MaximumSize = new Size(340, 0) });
        side.Controls.Add(new Label { Text = "Model: giswqs/geoai — Mask R-CNN.\nMIT model declaration; retain package notices.\nImagery source/license not verified.", AutoSize = true, MaximumSize = new Size(340, 0), Margin = new Padding(3, 18, 3, 3) });
        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        tools.Items.Add("Zoom In", null, (_, _) => viewer.ZoomIn());
        tools.Items.Add("Zoom Out", null, (_, _) => viewer.ZoomOut());
        tools.Items.Add("Full Extent", null, (_, _) => viewer.FullExtent());
        tools.Items.Add("Pan", null, (_, _) => viewer.ActiveTool = GeoKernelViewerTool.Pan);
        Controls.Add(viewer);
        Controls.Add(side);
        Controls.Add(tools);
        var renderFence = new GeoAiRenderFence(viewer);
        session = new GeoAiSession(GeoAiProfile.GeoAiUsa)
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
        run.Click += async (_, _) => await (operation = session.RunAsync(provider.Text, (double)threshold.Value));
        cancel.Click += (_, _) => session.Cancel();
        save.Click += async (_, _) =>
        {
            using var dialog = new SaveFileDialog { Filter = "Shapefile (*.shp)|*.shp", FileName = "building-footprints.shp", AddExtension = true, OverwritePrompt = true };
            if (dialog.ShowDialog(this) == DialogResult.OK) await (operation = session.ExportAsync(dialog.FileName));
        };
        Shown += async (_, _) =>
        {
            viewer.ActiveTool = GeoKernelViewerTool.Pan;
            await (operation = session.InitializeAsync());
        };
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
            viewer.ClearLayers();
            overlay = false;
            await session.DisposeAsync();
            closeAllowed = true;
            Close();
        };
        ApplyOpacity();
        UpdateState();
    }

    private void ApplyOpacity()
    {
        opacityText.Text = $"Output transparency: {100 - opacity.Value}%";
        if (overlay)
        {
            VectorOverlay.ApplyOpacity(viewer, 0, opacity.Value);
        }
    }

    private void UpdateState()
    {
        run.Enabled = !session.IsWorking;
        provider.Enabled = !session.IsWorking;
        threshold.Enabled = !session.IsWorking;
        save.Enabled = !session.IsWorking && session.HasResult;
        cancel.Enabled = session.CanCancel;
        progress.Value = session.Percent;
        message.Text = session.Status;
        details.Text = session.Details.Replace("\n", Environment.NewLine);
    }
}
