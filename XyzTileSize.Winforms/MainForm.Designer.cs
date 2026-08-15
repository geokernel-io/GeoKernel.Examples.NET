namespace GeoKernel.XyzTileSize.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayout;
    private FlowLayoutPanel toolbarPanel;
    private Button zoomInButton, zoomOutButton, fullExtentButton, zoomRectButton, panButton;
    private SplitContainer outerSplit, viewerSplit;
    private TableLayoutPanel leftPanel, rightPanel;
    private Label leftLabel, rightLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl leftViewerControl, rightViewerControl;
    private TextBox detailsTextBox;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolTip toolbarToolTip;

    protected override void Dispose(bool disposing) { if (disposing && components is not null) components.Dispose(); base.Dispose(disposing); }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        rootLayout = new TableLayoutPanel(); toolbarPanel = new FlowLayoutPanel();
        zoomInButton = new Button(); zoomOutButton = new Button(); fullExtentButton = new Button(); zoomRectButton = new Button(); panButton = new Button();
        toolbarToolTip = new ToolTip(components); outerSplit = new SplitContainer(); viewerSplit = new SplitContainer();
        leftPanel = new TableLayoutPanel(); rightPanel = new TableLayoutPanel(); leftLabel = new Label(); rightLabel = new Label();
        leftViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl(); rightViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        detailsTextBox = new TextBox(); statusStrip = new StatusStrip(); statusLabel = new ToolStripStatusLabel();

        rootLayout.Dock = DockStyle.Fill; rootLayout.Margin = Padding.Empty; rootLayout.RowCount = 3; rootLayout.ColumnCount = 1;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40)); rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100)); rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22));
        rootLayout.Controls.Add(toolbarPanel, 0, 0); rootLayout.Controls.Add(outerSplit, 0, 1); rootLayout.Controls.Add(statusStrip, 0, 2);
        toolbarPanel.Dock = DockStyle.Fill; toolbarPanel.Margin = Padding.Empty; toolbarPanel.WrapContents = false;
        ConfigureButton(zoomInButton, resources, "zoomInButton.Image", "Zoom In", zoomInButton_Click);
        ConfigureButton(zoomOutButton, resources, "zoomOutButton.Image", "Zoom Out", zoomOutButton_Click);
        ConfigureButton(fullExtentButton, resources, "fullExtentButton.Image", "Full Extent", fullExtentButton_Click);
        ConfigureButton(zoomRectButton, resources, "zoomRectButton.Image", "Zoom Rectangle", zoomRectButton_Click);
        ConfigureButton(panButton, resources, "panButton.Image", "Pan", panButton_Click);
        toolbarPanel.Controls.AddRange(new Control[] { zoomInButton, zoomOutButton, fullExtentButton, zoomRectButton, panButton });

        outerSplit.Dock = DockStyle.Fill; outerSplit.Panel2Collapsed = true;
        outerSplit.Panel1.Controls.Add(viewerSplit); outerSplit.Panel2.Controls.Add(detailsTextBox);
        viewerSplit.Dock = DockStyle.Fill;
        viewerSplit.Panel1.Controls.Add(leftPanel); viewerSplit.Panel2.Controls.Add(rightPanel);
        ConfigureViewerPanel(leftPanel, leftLabel, leftViewerControl, "256 px tiles | tileSize: 256");
        ConfigureViewerPanel(rightPanel, rightLabel, rightViewerControl, "512 px tiles | tileSize: 512");
        detailsTextBox.Dock = DockStyle.Fill; detailsTextBox.Multiline = true; detailsTextBox.ReadOnly = true; detailsTextBox.ScrollBars = ScrollBars.Vertical; detailsTextBox.Font = new Font("Consolas", 9F);
        statusStrip.Items.Add(statusLabel); statusLabel.Text = "XyzTileSize ready.";
        ClientSize = new Size(1280, 800); Controls.Add(rootLayout); Icon = (Icon)resources.GetObject("$this.Icon");
        StartPosition = FormStartPosition.CenterScreen; Text = "XyzTileSize"; Shown += MainForm_Shown; Resize += MainForm_Resize;
    }

    private void ConfigureButton(Button button, System.ComponentModel.ComponentResourceManager resources, string key, string tooltip, EventHandler handler)
    {
        button.BackgroundImage = (Image)resources.GetObject(key); button.BackgroundImageLayout = ImageLayout.Center;
        button.FlatStyle = FlatStyle.Flat; button.FlatAppearance.BorderSize = 0; button.Size = new Size(38, 38); button.Margin = Padding.Empty; button.TabStop = false;
        toolbarToolTip.SetToolTip(button, tooltip); button.Click += handler;
    }

    private static void ConfigureViewerPanel(TableLayoutPanel panel, Label label, Control viewer, string text)
    {
        panel.Dock = DockStyle.Fill; panel.Margin = Padding.Empty; panel.RowCount = 2; panel.ColumnCount = 1;
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 27)); panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        label.Dock = DockStyle.Fill; label.Text = text; label.BackColor = Color.FromArgb(238, 238, 238); label.Font = new Font(SystemFonts.DefaultFont, FontStyle.Bold); label.Padding = new Padding(6, 6, 0, 0);
        viewer.Dock = DockStyle.Fill; panel.Controls.Add(label, 0, 0); panel.Controls.Add(viewer, 0, 1);
    }
}
