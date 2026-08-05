namespace GeoKernel.AddPolylineInteractive.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button fullExtentButton;
    private Panel navigationSeparator;
    private Button addPolylineButton;
    private Button panButton;
    private Panel editSeparator;
    private Button clearLinesButton;
    private Label polylineCountLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;
    private ToolTip toolbarToolTip;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        toolbarToolTip = new ToolTip(components);
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        fullExtentButton = new Button();
        navigationSeparator = new Panel();
        addPolylineButton = new Button();
        panButton = new Button();
        editSeparator = new Panel();
        clearLinesButton = new Button();
        polylineCountLabel = new Label();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // layoutPanel
        // 
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(toolbarPanel, 0, 0);
        layoutPanel.Controls.Add(geoKernelViewerControl, 0, 1);
        layoutPanel.Controls.Add(statusStrip, 0, 2);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.Location = new Point(0, 0);
        layoutPanel.Margin = new Padding(0);
        layoutPanel.Name = "layoutPanel";
        layoutPanel.RowCount = 3;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 48F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = Color.FromArgb(248, 249, 250);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(navigationSeparator);
        toolbarPanel.Controls.Add(addPolylineButton);
        toolbarPanel.Controls.Add(panButton);
        toolbarPanel.Controls.Add(editSeparator);
        toolbarPanel.Controls.Add(clearLinesButton);
        toolbarPanel.Controls.Add(polylineCountLabel);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1200, 48);
        toolbarPanel.TabIndex = 0;
        // 
        // fullExtentButton
        // 
        fullExtentButton.BackgroundImage = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.BackgroundImageLayout = ImageLayout.Center;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.BackColor = Color.FromArgb(248, 249, 250);
        fullExtentButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
        fullExtentButton.Location = new Point(4, 3);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(42, 42);
        fullExtentButton.TabIndex = 0;
        fullExtentButton.Text = "";
        fullExtentButton.AccessibleName = "Full Extent";
        toolbarToolTip.SetToolTip(fullExtentButton, "Full Extent");
        fullExtentButton.UseVisualStyleBackColor = false;
        fullExtentButton.TabStop = false;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // navigationSeparator
        // 
        navigationSeparator.BackColor = Color.FromArgb(180, 180, 180);
        navigationSeparator.Location = new Point(45, 7);
        navigationSeparator.Name = "navigationSeparator";
        navigationSeparator.Size = new Size(1, 25);
        navigationSeparator.TabIndex = 1;
        navigationSeparator.Visible = false;
        // 
        // addPolylineButton
        // 
        addPolylineButton.BackgroundImage = (Image)resources.GetObject("addPolylineButton.Image");
        addPolylineButton.BackgroundImageLayout = ImageLayout.Center;
        addPolylineButton.FlatStyle = FlatStyle.Flat;
        addPolylineButton.BackColor = Color.FromArgb(219, 234, 254);
        addPolylineButton.FlatAppearance.BorderSize = 0;
        addPolylineButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(191, 219, 254);
        addPolylineButton.Location = new Point(52, 3);
        addPolylineButton.Name = "addPolylineButton";
        addPolylineButton.Size = new Size(42, 42);
        addPolylineButton.TabIndex = 2;
        addPolylineButton.Text = "";
        addPolylineButton.AccessibleName = "Add Polyline";
        toolbarToolTip.SetToolTip(addPolylineButton, "Add Polyline");
        addPolylineButton.UseVisualStyleBackColor = false;
        addPolylineButton.TabStop = false;
        addPolylineButton.Click += addPolylineButton_Click;
        // 
        // panButton
        // 
        panButton.BackgroundImage = (Image)resources.GetObject("panButton.Image");
        panButton.BackgroundImageLayout = ImageLayout.Center;
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.BackColor = Color.FromArgb(248, 249, 250);
        panButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(226, 232, 240);
        panButton.Location = new Point(100, 3);
        panButton.Name = "panButton";
        panButton.Size = new Size(42, 42);
        panButton.TabIndex = 3;
        panButton.Text = "";
        panButton.AccessibleName = "Pan";
        toolbarToolTip.SetToolTip(panButton, "Pan");
        panButton.UseVisualStyleBackColor = false;
        panButton.TabStop = false;
        panButton.Click += panButton_Click;
        // 
        // editSeparator
        // 
        editSeparator.BackColor = Color.FromArgb(180, 180, 180);
        editSeparator.Location = new Point(136, 7);
        editSeparator.Name = "editSeparator";
        editSeparator.Size = new Size(1, 25);
        editSeparator.TabIndex = 4;
        editSeparator.Visible = false;
        // 
        // clearLinesButton
        // 
        clearLinesButton.BackgroundImage = (Image)resources.GetObject("clearLinesButton.Image");
        clearLinesButton.BackgroundImageLayout = ImageLayout.Center;
        clearLinesButton.FlatStyle = FlatStyle.Flat;
        clearLinesButton.FlatAppearance.BorderSize = 0;
        clearLinesButton.BackColor = Color.FromArgb(248, 249, 250);
        clearLinesButton.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 226, 226);
        clearLinesButton.Location = new Point(148, 3);
        clearLinesButton.Name = "clearLinesButton";
        clearLinesButton.Size = new Size(42, 42);
        clearLinesButton.TabIndex = 5;
        clearLinesButton.Text = "";
        clearLinesButton.AccessibleName = "Clear Lines";
        toolbarToolTip.SetToolTip(clearLinesButton, "Clear Lines");
        clearLinesButton.UseVisualStyleBackColor = false;
        clearLinesButton.TabStop = false;
        clearLinesButton.Click += clearLinesButton_Click;
        // 
        // polylineCountLabel
        // 
        polylineCountLabel.AutoSize = false;
        polylineCountLabel.Location = new Point(202, 0);
        polylineCountLabel.Name = "polylineCountLabel";
        polylineCountLabel.Size = new Size(150, 48);
        polylineCountLabel.TabIndex = 6;
        polylineCountLabel.Text = "Polyline count: 0";
        polylineCountLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 48);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(1200, 728);
        geoKernelViewerControl.TabIndex = 1;
        // 
        // statusStrip
        // 
        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar });
        downloadProgressBar.Size = new Size(180, 18);
        downloadProgressBar.Visible = false;
        statusStrip.Location = new Point(0, 776);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1200, 24);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(39, 19);
        statusLabel.Text = "Ready";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 800);
        Controls.Add(layoutPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "AddPolylineInteractive";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
