using GeoKernel.NET.WinForms;

namespace GeoKernel.ModelPlacement.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewer3DControl _viewer = new() { Dock = DockStyle.Fill };
    private readonly NumericUpDown[] _placement;
    private readonly Button _apply = new() { Text = "Apply placement / Reload", Width = 240 };
    private readonly Button _cancel = new() { Text = "Cancel loading", Width = 240, Enabled = false };
    private readonly Button _focus = new() { Text = "Focus model", Width = 240, Enabled = false };
    private readonly CheckBox _visible = new() { Text = "Show model", Checked = true, Enabled = false, Width = 240 };
    private readonly CheckBox _imagery = new() { Text = "Show orthophoto", Checked = true, Enabled = false, Width = 240 };
    private readonly NumericUpDown _height = new() { Minimum = .25m, Maximum = 10, Increment = .25m, DecimalPlaces = 2, Value = 1, Width = 240 };
    private readonly ToolStripStatusLabel _status = new() { Text = "Preparing sample…" };
    private readonly ToolStripProgressBar _progress = new() { Style = ProgressBarStyle.Marquee };
    private CancellationTokenSource? _load;

    public MainForm()
    {
        Text = "ModelPlacement — GeoKernel";
        ClientSize = new Size(1280, 850);
        StartPosition = FormStartPosition.CenterScreen;
        Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Left, Width = 275, Padding = new Padding(12),
            FlowDirection = FlowDirection.TopDown, WrapContents = false, AutoScroll = true
        };
        panel.Controls.Add(Caption("Sagrada Família — textured model"));
        NumericUpDown Field(string label, decimal minimum, decimal maximum, decimal value, int decimals)
        {
            var input = new NumericUpDown { Minimum = minimum, Maximum = maximum, DecimalPlaces = decimals, Value = value, Width = 240 };
            panel.Controls.Add(Caption(label));
            panel.Controls.Add(input);
            return input;
        }
        _placement = [
            Field("Longitude", -180, 180, 2.174401283m, 9),
            Field("Latitude", -90, 90, 41.403605046m, 9),
            Field("Above terrain (m)", -500, 1000, 2.938m, 3),
            Field("Heading (degrees)", -180, 180, .2373346m, 7),
            Field("Pitch (degrees)", -180, 180, 0, 2),
            Field("Roll (degrees)", -180, 180, 0, 2),
            Field("Scale", .01m, 100, .958139m, 6)
        ];
        var reset = new Button { Text = "Reset scene view", Width = 240 };
        panel.Controls.AddRange([_apply, _cancel, _focus, reset, _visible, _imagery,
            Caption("Vertical exaggeration"), _height,
            Caption("Approximate demo placement; not a surveyed fit. Height is an offset above terrain at the model anchor. DEM uses EGM08D595 geoid correction. Textures are retained."),
            Caption("Model: PeeJaa — La Sagrada Familia. Source and license: downloaded model/license.txt.")]);
        var statusBar = new StatusStrip();
        statusBar.Items.AddRange([_status, _progress]);
        Controls.Add(_viewer); Controls.Add(panel); Controls.Add(statusBar);
        _apply.Click += async (_, _) => await LoadSampleAsync();
        _cancel.Click += (_, _) => _load?.Cancel();
        _focus.Click += (_, _) => _viewer.FocusModel();
        reset.Click += (_, _) => _viewer.ResetCamera();
        _visible.CheckedChanged += (_, _) => _viewer.SetModelVisible(_visible.Checked);
        _imagery.CheckedChanged += (_, _) => _viewer.ImageryVisible = _imagery.Checked;
        _height.ValueChanged += (_, _) => _viewer.HeightScale = (float)_height.Value;
        Shown += async (_, _) => await LoadSampleAsync();
        FormClosing += (_, _) => _load?.Cancel();
    }

    private static Label Caption(string text) => new()
    {
        Text = text, AutoSize = true, MaximumSize = new Size(240, 0), Margin = new Padding(0, 8, 0, 6)
    };

    private async Task LoadSampleAsync()
    {
        if (_load != null || IsDisposed) return;
        using var cancellation = new CancellationTokenSource();
        _load = cancellation;
        _apply.Enabled = false;
        foreach (var input in _placement) input.Enabled = false;
        _cancel.Enabled = _progress.Visible = true;
        try
        {
            var progress = new Progress<string>(message => { if (!IsDisposed) _status.Text = message; });
            string dem = await SampleData.EnsureAsync("terrain", progress, cancellation.Token);
            string photo = await SampleData.EnsureAsync("ortophoto", progress, cancellation.Token);
            string model = await SampleData.EnsureArchiveAsync("sagrada_familia_3d_model",
                "https://github.com/geokernel-io/GeoKernel.SampleData/releases/download/v1/sagrada_familia_3d_model.zip",
                "model.gltf", progress, cancellation.Token);
            string geoid = await SampleData.EnsureArchiveAsync("egm08d595",
                "https://icgc-web-pro.s3.eu-central-1.amazonaws.com/produccio/s3fs-public/EGM08D595_19839.zip",
                "cat80000.gr", progress, cancellation.Token);
            if (IsDisposed) return;
            _status.Text = "Preparing terrain and model…";
            await _viewer.LoadModelOnTerrainAsync(dem, photo, model, geoid,
                _placement.Select(input => (double)input.Value).ToArray(), cancellationToken: cancellation.Token);
            if (IsDisposed) return;
            _viewer.SetModelVisible(_visible.Checked);
            _viewer.ImageryVisible = _imagery.Checked;
            _viewer.FocusModel();
            _visible.Enabled = _imagery.Enabled = _focus.Enabled = true;
            _status.Text = $"Model loaded — {_viewer.GetModelVertexCount() / 3:N0} triangles; EGM08D595 corrected terrain";
        }
        catch (OperationCanceledException)
        {
            if (!IsDisposed) _status.Text = "Loading cancelled; previous scene retained.";
        }
        catch (Exception error)
        {
            System.Diagnostics.Trace.WriteLine(error);
            if (!IsDisposed)
            {
                _status.Text = "Unable to load model. Apply placement / Reload to retry.";
                MessageBox.Show(this, error.Message, "ModelPlacement", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        finally
        {
            _load = null;
            if (!IsDisposed)
            {
                _apply.Enabled = true;
                foreach (var input in _placement) input.Enabled = true;
                _cancel.Enabled = _progress.Visible = false;
            }
        }
    }
}
