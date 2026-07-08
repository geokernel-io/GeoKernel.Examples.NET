using GeoKernel.NET.WinForms;

namespace GeoKernel.LabelOffset.Winforms;

public sealed class MainForm : Form
{
    private readonly GeoKernelViewerControl _viewer = new();
    private readonly NumericUpDown _offsetX = new();
    private readonly NumericUpDown _offsetY = new();
    private readonly ToolStripStatusLabel _status = new("Ready");
    private int _worldLayerIndex = -1;
    private bool _loading = true;

    public MainForm()
    {
        Text = "LabelOffset";
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
            RowCount = 7
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 28));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        panel.Controls.Add(new Label { Text = "Label offset", Dock = DockStyle.Fill, Font = new Font("Segoe UI", 9, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft }, 0, 0);
        panel.Controls.Add(new Label { Text = "Offset X", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 1);
        ConfigureOffsetInput(_offsetX);
        panel.Controls.Add(_offsetX, 0, 2);
        panel.Controls.Add(new Label { Text = "Offset Y", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft }, 0, 3);
        ConfigureOffsetInput(_offsetY);
        panel.Controls.Add(_offsetY, 0, 4);
        var resetButton = new Button { Text = "Reset Offset", Dock = DockStyle.Fill };
        resetButton.Click += (_, _) => ResetOffset();
        panel.Controls.Add(resetButton, 0, 5);

        _viewer.Dock = DockStyle.Fill;
        split.Panel1.Controls.Add(panel);
        split.Panel2.Controls.Add(_viewer);

        var statusStrip = new StatusStrip { SizingGrip = false };
        statusStrip.Items.Add(_status);

        Controls.Add(split);
        Controls.Add(statusStrip);
        Shown += MainForm_Shown;
    }

    private void ConfigureOffsetInput(NumericUpDown input)
    {
        input.DecimalPlaces = 1;
        input.Minimum = -80;
        input.Maximum = 80;
        input.Increment = 2;
        input.Dock = DockStyle.Fill;
        input.ValueChanged += (_, _) => OffsetControlChanged();
    }

    private void MainForm_Shown(object? sender, EventArgs e)
    {
        _viewer.MapBackgroundColor = System.Drawing.Color.FromArgb(247, 248, 250);
        _viewer.ActiveTool = GeoKernelViewerTool.Pan;

        if (!LoadLayer())
            return;

        ApplyOffsetStyle();
        _viewer.ViewExtent = new GeoKernelExtent(-180.0, -58.0, 180.0, 82.0);
        _loading = false;
        _status.Text = "Labels use labelOffsetX and labelOffsetY.";
    }

    private bool LoadLayer()
    {
        var path = Path.Combine(FindRepositoryRoot(), "assets", "data", "world_4326.shp");
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"Sample data was not found:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        if (!_viewer.AddLayerFile(path, new GeoKernelLayerLoadOptions { ApplyDefaultStyle = true, DefaultStyle = OffsetStyle() }))
        {
            MessageBox.Show(this, $"World layer could not be loaded:{Environment.NewLine}{path}", Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            return false;
        }

        _viewer.SetLayerName(0, "World - label offset");
        _worldLayerIndex = _viewer.GetLayerInfoByName("World - label offset")?.Index ?? 0;
        return true;
    }

    private void OffsetControlChanged()
    {
        if (_loading)
            return;

        ApplyOffsetStyle();
        _status.Text = $"Label offset X: {_offsetX.Value:0.0}, Y: {_offsetY.Value:0.0}";
    }

    private void ResetOffset()
    {
        _offsetX.Value = 0;
        _offsetY.Value = 0;
        OffsetControlChanged();
    }

    private void ApplyOffsetStyle()
    {
        if (_worldLayerIndex < 0)
            return;

        _viewer.SetLayerStyle(_worldLayerIndex, OffsetStyle());
        _viewer.InvalidateRenderCache(clearTileCache: true, clearLayerCache: true);
        _viewer.RefreshLayers();
    }

    private GeoKernelLayerStyle OffsetStyle()
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
            LabelOffsetX = (double)_offsetX.Value,
            LabelOffsetY = (double)_offsetY.Value
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
