using GeoKernel.Examples.GeoAi;
using GeoKernel.NET.WinForms;

namespace GeoKernel.Sen2srSuperResolution.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewerControl viewer = new() { Dock = DockStyle.Fill };
    private readonly ComboBox provider = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 340 };
    private readonly TrackBar opacity = new() { Minimum = 0, Maximum = 100, Value = 100, TickFrequency = 10, Width = 340 };
    private readonly Label opacityText = new() { AutoSize = true };
    private readonly Button run = new() { Text = "Run 4× super-resolution", Width = 340, Height = 32 };
    private readonly Button save = new() { Text = "Save GeoTIFF", Width = 340, Height = 32 };
    private readonly Button cancel = new() { Text = "Cancel", Width = 340, Height = 30 };
    private readonly ProgressBar progress = new() { Maximum = 100, Width = 340 };
    private readonly Label message = new() { AutoSize = true, MaximumSize = new Size(340, 0) };
    private readonly TextBox details = new() { Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, Width = 340, Height = 200 };
    private readonly GeoAiSession session;
    private Task operation = Task.CompletedTask;
    private bool overlay;
    private bool closing;
    private bool closeAllowed;

    public MainForm()
    {
        ExampleIcon.Apply(this);
        Text = "SEN2SR Super Resolution — WinForms";
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
        side.Controls.Add(new Label { Text = "ESAOpenSR / SEN2SRLite — 4× super-resolution", AutoSize = true, Font = new Font(Font, FontStyle.Bold) });
        side.Controls.Add(new Label { Text = "Execution provider", AutoSize = true });
        side.Controls.Add(provider);
        side.Controls.Add(opacityText);
        side.Controls.Add(opacity);
        side.Controls.Add(new Label { Text = "Zoom in to compare detail. Left: original; right: enhanced.", AutoSize = true, MaximumSize = new Size(340, 0) });
        side.Controls.Add(run);
        side.Controls.Add(save);
        side.Controls.Add(cancel);
        side.Controls.Add(progress);
        side.Controls.Add(message);
        side.Controls.Add(new Label { Text = "Inference diagnostics", AutoSize = true });
        side.Controls.Add(details);
        side.Controls.Add(new Label { Text = "Output\n" + GeoAiProfile.Sen2sr.Legend, AutoSize = true, MaximumSize = new Size(340, 0) });
        side.Controls.Add(new Label { Text = "Model: ESAOpenSR / SEN2SRLite.\nCC0-1.0 + Apache-2.0 notices.\nImagery: Sentinel-2 / FTW test sample.\nEstimated detail; not new measurements.", AutoSize = true, MaximumSize = new Size(340, 0), Margin = new Padding(3, 18, 3, 3) });
        var tools = new ToolStrip { GripStyle = ToolStripGripStyle.Hidden, Dock = DockStyle.Top };
        tools.Items.Add("Zoom In", null, (_, _) => viewer.ZoomIn());
        tools.Items.Add("Zoom Out", null, (_, _) => viewer.ZoomOut());
        tools.Items.Add("Full Extent", null, (_, _) => viewer.FullExtent());
        tools.Items.Add("Pan", null, (_, _) => viewer.ActiveTool = GeoKernelViewerTool.Pan);
        Controls.Add(viewer);
        Controls.Add(side);
        Controls.Add(tools);
        var renderFence = new GeoAiRenderFence(viewer);
        session = new GeoAiSession(GeoAiProfile.Sen2sr)
        {
            WaitForRenderer = renderFence.WaitAsync,
            ShowSource = path =>
            {
                var previousExtent = viewer.LayerCount > 0 ? viewer.ViewExtent : default;
                bool restoreExtent = viewer.LayerCount > 0;
                viewer.ClearLayers();
                if (!viewer.AddLayerFile(path)) throw new InvalidOperationException("Cannot open source RGB preview.");
                viewer.SetLayerName(0, "Original RGB source");
                if (restoreExtent) viewer.ViewExtent = previousExtent;
                else viewer.FullExtent();
            },
            ShowResult = path =>
            {
                if (!viewer.AddLayerFile(path)) throw new InvalidOperationException("Cannot display enhanced RGB preview.");
                overlay = true;
                viewer.SetLayerName(0, "SEN2SR enhanced RGB");
                ApplyOpacity();
            },
            ClearResult = () =>
            {
                if (overlay) { viewer.RemoveLayer(0); overlay = false; }
            }
        };
        session.Changed += UpdateState;
        opacity.ValueChanged += (_, _) => ApplyOpacity();
        run.Click += async (_, _) => await (operation = session.RunAsync(provider.Text));
        cancel.Click += (_, _) => session.Cancel();
        save.Click += async (_, _) =>
        {
            using var dialog = new SaveFileDialog { Filter = "GeoTIFF (*.tif)|*.tif", FileName = "enhanced-rgbn.tif", AddExtension = true, OverwritePrompt = true };
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
        opacityText.Text = !overlay ? "Original image - run 4× super-resolution"
            : opacity.Value == 0 ? "Original image"
            : opacity.Value == 100 ? "Enhanced image (100%)"
            : $"Original / enhanced blend: {opacity.Value}% enhanced";
        if (overlay)
        {
            viewer.SetLayerOpacity(0, opacity.Value / 100.0);
            viewer.RefreshLayers();
        }
    }

    private void UpdateState()
    {
        opacity.Enabled = overlay && !session.IsWorking;
        ApplyOpacity();
        run.Enabled = !session.IsWorking;
        provider.Enabled = !session.IsWorking;
        save.Enabled = !session.IsWorking && session.HasResult;
        cancel.Enabled = session.CanCancel;
        progress.Value = session.Percent;
        message.Text = session.Status;
        details.Text = session.Details.Replace("\n", Environment.NewLine);
    }
}
