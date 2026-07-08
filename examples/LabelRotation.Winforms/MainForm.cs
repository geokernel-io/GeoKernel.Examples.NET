using GeoKernel.NET.WinForms;

namespace GeoKernel.LabelRotation.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewerControl _viewer = new();
    private readonly NumericUpDown _rotation = new();
    private readonly ToolStripStatusLabel _status = new("Ready");
    private int _worldLayerIndex = -1;
    private bool _loading = true;

    public MainForm()
    {
        Text = "LabelRotation";
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1200, 800);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            FixedPanel = FixedPanel.Panel1,
            SplitterDistance = 230,
            SplitterWidth = 1
        };

        var panel = new TableLayoutPanel
        {
            BackColor = Color.FromArgb(239, 239, 239),
            Dock = DockStyle.Fill,
            Padding = new Padding(8),
            ColumnCount = 1,
            RowCount = 5
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        panel.Controls.Add(new Label { Text = "Label rotation", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        panel.Controls.Add(new Label { Text = "Rotation degrees", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        ConfigureRotationInput();
        panel.Controls.Add(_rotation, 0, 2);

        var resetButton = new Button { Text = "Reset Rotation", Dock = DockStyle.Fill };
        resetButton.Click += (_, _) => ResetRotation();
        panel.Controls.Add(resetButton, 0, 3);

        _viewer.Dock = DockStyle.Fill;
        split.Panel1.Controls.Add(panel);
        split.Panel2.Controls.Add(_viewer);

        var statusStrip = new StatusStrip { SizingGrip = false };
        statusStrip.Items.Add(_status);

        Controls.Add(split);
        Controls.Add(statusStrip);
        Shown += MainForm_Shown;
    }

    private void ConfigureRotationInput()
    {
        _rotation.DecimalPlaces = 1;
        _rotation.Minimum = -180;
        _rotation.Maximum = 180;
        _rotation.Increment = 5;
        _rotation.Dock = DockStyle.Fill;
        _rotation.ValueChanged += (_, _) => RotationControlChanged();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        _viewer.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        _viewer.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadLayer())
            return;

        ApplyRotationStyle();
        _viewer.ViewExtent = new GeoKernelExtent(-180.0, -58.0, 180.0, 82.0);
        _loading = false;
        _status.Text = "Labels use labelRotationDegrees.";
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!_viewer.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = RotationStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        _viewer.SetLayerName(0, "World - label rotation");
        _worldLayerIndex = _viewer.GetLayerInfoByName("World - label rotation")?.Index ?? 0;
        return true;
    }

    private void RotationControlChanged()
    {
        if (_loading)
            return;

        ApplyRotationStyle();
        _status.Text = $"Label rotation: {_rotation.Value:0.0} degrees";
    }

    private void ResetRotation()
    {
        _rotation.Value = 0;
        RotationControlChanged();
    }

    private void ApplyRotationStyle()
    {
        if (_worldLayerIndex < 0)
            return;

        _viewer.SetLayerStyle(_worldLayerIndex, RotationStyle());
        _viewer.InvalidateRenderCache(clearTileCache: true, clearLayerCache: true);
        _viewer.RefreshLayers();
    }

    private GeoKernelLayerStyle RotationStyle()
    {
        return new GeoKernelLayerStyle
        {
            FillColor = "#D8E5E1",
            FillOpacity = 215,
            LineColor = "#6F8380",
            LineWidth = 0.8,
            ShowLabels = true,
            LabelField = "COUNTRY",
            LabelFontSize = 12.0,
            LabelColor = "#253238",
            LabelHaloEnabled = true,
            LabelHaloColor = "#FFFFFF",
            LabelHaloWidth = 2.0,
            LabelRotationDegrees = (double)_rotation.Value
        };
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (Directory.Exists(Path.Combine(directory.FullName, "assets", "data")))
                return directory.FullName;
            directory = directory.Parent;
        }
        return AppContext.BaseDirectory;
    }
}
