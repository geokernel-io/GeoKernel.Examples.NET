namespace GeoKernel.XyzLocalCache.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button zoomInButton;
    private Button zoomOutButton;
    private Button zoomRectButton;
    private Button panButton;
    private ToolTip toolbarToolTip;
    private Button secondaryButton;
    private CheckBox cacheCheckBox;
    private Label cacheLabel;
    private TextBox cachePathTextBox;
    private Button browseButton;
    private Button applyButton;
    private Button refreshButton;
    private Button clearButton;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl viewerControl;
    private TextBox detailsTextBox;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new(typeof(MainForm));
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        zoomInButton = new Button();
        zoomOutButton = new Button();
        zoomRectButton = new Button();
        panButton = new Button();
        toolbarToolTip = new ToolTip(components);
        secondaryButton = new Button();
        cacheCheckBox = new CheckBox(); cacheLabel = new Label(); cachePathTextBox = new TextBox();
        browseButton = new Button(); applyButton = new Button(); refreshButton = new Button(); clearButton = new Button();
        splitContainer = new SplitContainer();
        viewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(toolbarPanel, 0, 0);
        layoutPanel.Controls.Add(splitContainer, 0, 1);
        layoutPanel.Controls.Add(statusStrip, 0, 2);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.Location = new Point(0, 0);
        layoutPanel.Margin = new Padding(0);
        layoutPanel.RowCount = 3;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        layoutPanel.Size = new Size(1180, 760);
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(zoomInButton);
        toolbarPanel.Controls.Add(zoomOutButton);
        toolbarPanel.Controls.Add(secondaryButton);
        toolbarPanel.Controls.Add(zoomRectButton);
        toolbarPanel.Controls.Add(panButton);
        toolbarPanel.Controls.Add(cacheCheckBox); toolbarPanel.Controls.Add(cacheLabel); toolbarPanel.Controls.Add(cachePathTextBox);
        toolbarPanel.Controls.Add(browseButton); toolbarPanel.Controls.Add(applyButton); toolbarPanel.Controls.Add(refreshButton); toolbarPanel.Controls.Add(clearButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Margin = new Padding(0);
                ConfigureToolbarButton(zoomInButton, resources, "zoomInButton.Image", new Point(4, 0), "Zoom In", zoomInButton_Click);
        ConfigureToolbarButton(zoomOutButton, resources, "zoomOutButton.Image", new Point(40, 0), "Zoom Out", zoomOutButton_Click);
        ConfigureToolbarButton(secondaryButton, resources, "fullExtentButton.Image", new Point(76, 0), "Full Extent", secondaryButton_Click);
        ConfigureToolbarButton(zoomRectButton, resources, "zoomRectButton.Image", new Point(112, 0), "Zoom Rectangle", zoomRectButton_Click);
        ConfigureToolbarButton(panButton, resources, "panButton.Image", new Point(148, 0), "Pan", panButton_Click);
        cacheCheckBox.AutoSize = true; cacheCheckBox.Checked = true; cacheCheckBox.Location = new Point(194, 8); cacheCheckBox.Text = "Local cache";
        cacheLabel.AutoSize = true; cacheLabel.Location = new Point(286, 10); cacheLabel.Text = "Cache:";
        cachePathTextBox.Location = new Point(334, 6); cachePathTextBox.Size = new Size(330, 23);
        browseButton.Location = new Point(670, 5); browseButton.Size = new Size(65, 25); browseButton.Text = "Browse"; browseButton.Click += browseButton_Click;
        applyButton.Location = new Point(741, 5); applyButton.Size = new Size(82, 25); applyButton.Text = "Apply Cache"; applyButton.Click += applyButton_Click;
        refreshButton.Location = new Point(829, 5); refreshButton.Size = new Size(88, 25); refreshButton.Text = "Refresh Stats"; refreshButton.Click += refreshButton_Click;
        clearButton.Location = new Point(923, 5); clearButton.Size = new Size(78, 25); clearButton.Text = "Clear Cache"; clearButton.Click += clearButton_Click;
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Margin = new Padding(0);
        splitContainer.Panel1.Controls.Add(viewerControl);
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.SplitterDistance = 760;
        viewerControl.Dock = DockStyle.Fill;
        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Multiline = true;
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Font = new Font("Consolas", 9F);
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusLabel.Text = "XyzLocalCache ready.";
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1180, 760);
        Controls.Add(layoutPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "XyzLocalCache";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }

    private void ConfigureToolbarButton(Button button, System.ComponentModel.ComponentResourceManager resources, string imageKey, Point location, string tooltip, EventHandler handler)
    {
        button.BackgroundImage = (Image)resources.GetObject(imageKey);
        button.BackgroundImageLayout = ImageLayout.Center;
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.Location = location;
        button.Size = new Size(36, 35);
        button.TabStop = false;
        button.Text = "";
        button.AccessibleName = tooltip;
        toolbarToolTip.SetToolTip(button, tooltip);
        button.Click += handler;
    }
}
