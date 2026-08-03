namespace GeoKernel.MultiWindowSync.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button zoomInButton;
    private Button zoomOutButton;
    private Button fullExtentButton;
    private CheckBox syncButton;
    private Panel toolbarSeparator;
    private CheckBox zoomRectButton;
    private CheckBox panButton;
    private SplitContainer splitContainer;
    private TableLayoutPanel leftPaneLayout;
    private Label leftTitleLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl leftViewerControl;
    private TableLayoutPanel rightPaneLayout;
    private Label rightTitleLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl rightViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        zoomInButton = new Button();
        zoomOutButton = new Button();
        fullExtentButton = new Button();
        syncButton = new CheckBox();
        toolbarSeparator = new Panel();
        zoomRectButton = new CheckBox();
        panButton = new CheckBox();
        splitContainer = new SplitContainer();
        leftPaneLayout = new TableLayoutPanel();
        leftTitleLabel = new Label();
        leftViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        rightPaneLayout = new TableLayoutPanel();
        rightTitleLabel = new Label();
        rightViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        leftPaneLayout.SuspendLayout();
        rightPaneLayout.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // layoutPanel
        // 
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(toolbarPanel, 0, 0);
        layoutPanel.Controls.Add(splitContainer, 0, 1);
        layoutPanel.Controls.Add(statusStrip, 0, 2);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.Location = new Point(0, 0);
        layoutPanel.Margin = new Padding(0);
        layoutPanel.Name = "layoutPanel";
        layoutPanel.RowCount = 3;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        layoutPanel.Size = new Size(1280, 760);
        layoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(panButton);
        toolbarPanel.Controls.Add(zoomRectButton);
        toolbarPanel.Controls.Add(toolbarSeparator);
        toolbarPanel.Controls.Add(syncButton);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(zoomOutButton);
        toolbarPanel.Controls.Add(zoomInButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Size = new Size(1280, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // zoomInButton
        // 
        zoomInButton.AccessibleName = "Zoom In";
        zoomInButton.BackColor = SystemColors.Control;
        zoomInButton.BackgroundImage = (Image)resources.GetObject("zoomInButton.Image");
        zoomInButton.BackgroundImageLayout = ImageLayout.Zoom;
        zoomInButton.FlatAppearance.BorderSize = 0;
        zoomInButton.FlatStyle = FlatStyle.Flat;
        zoomInButton.Location = new Point(0, 0);
        zoomInButton.Margin = new Padding(0);
        zoomInButton.Name = "zoomInButton";
        zoomInButton.Size = new Size(36, 36);
        zoomInButton.TabIndex = 0;
        zoomInButton.Text = "";
        zoomInButton.UseVisualStyleBackColor = false;
        zoomInButton.Click += zoomInButton_Click;
        // 
        // zoomOutButton
        // 
        zoomOutButton.AccessibleName = "Zoom Out";
        zoomOutButton.BackColor = SystemColors.Control;
        zoomOutButton.BackgroundImage = (Image)resources.GetObject("zoomOutButton.Image");
        zoomOutButton.BackgroundImageLayout = ImageLayout.Zoom;
        zoomOutButton.FlatAppearance.BorderSize = 0;
        zoomOutButton.FlatStyle = FlatStyle.Flat;
        zoomOutButton.Location = new Point(36, 0);
        zoomOutButton.Margin = new Padding(0);
        zoomOutButton.Name = "zoomOutButton";
        zoomOutButton.Size = new Size(36, 36);
        zoomOutButton.TabIndex = 1;
        zoomOutButton.Text = "";
        zoomOutButton.UseVisualStyleBackColor = false;
        zoomOutButton.Click += zoomOutButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.AccessibleName = "Full Extent";
        fullExtentButton.BackColor = SystemColors.Control;
        fullExtentButton.BackgroundImage = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.BackgroundImageLayout = ImageLayout.Zoom;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.Location = new Point(72, 0);
        fullExtentButton.Margin = new Padding(0);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(36, 36);
        fullExtentButton.TabIndex = 2;
        fullExtentButton.Text = "";
        fullExtentButton.UseVisualStyleBackColor = false;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // syncButton
        // 
        syncButton.Appearance = Appearance.Button;
        syncButton.BackColor = SystemColors.Control;
        syncButton.Checked = true;
        syncButton.CheckState = CheckState.Checked;
        syncButton.FlatAppearance.BorderSize = 0;
        syncButton.FlatStyle = FlatStyle.Flat;
        syncButton.Location = new Point(108, 0);
        syncButton.Margin = new Padding(0);
        syncButton.Name = "syncButton";
        syncButton.Size = new Size(76, 36);
        syncButton.TabIndex = 3;
        syncButton.Text = "Sync On";
        syncButton.TextAlign = ContentAlignment.MiddleCenter;
        syncButton.UseVisualStyleBackColor = false;
        syncButton.Click += syncButton_Click;
        // 
        // toolbarSeparator
        // 
        toolbarSeparator.BackColor = SystemColors.ControlDark;
        toolbarSeparator.Location = new Point(184, 4);
        toolbarSeparator.Margin = new Padding(0);
        toolbarSeparator.Name = "toolbarSeparator";
        toolbarSeparator.Size = new Size(1, 28);
        toolbarSeparator.TabIndex = 4;
        // 
        // zoomRectButton
        // 
        zoomRectButton.AccessibleName = "Zoom Rect";
        zoomRectButton.Appearance = Appearance.Button;
        zoomRectButton.BackColor = SystemColors.Control;
        zoomRectButton.BackgroundImage = (Image)resources.GetObject("zoomRectButton.Image");
        zoomRectButton.BackgroundImageLayout = ImageLayout.Zoom;
        zoomRectButton.FlatAppearance.BorderSize = 0;
        zoomRectButton.FlatStyle = FlatStyle.Flat;
        zoomRectButton.Location = new Point(188, 0);
        zoomRectButton.Margin = new Padding(0);
        zoomRectButton.Name = "zoomRectButton";
        zoomRectButton.Size = new Size(36, 36);
        zoomRectButton.TabIndex = 5;
        zoomRectButton.Text = "";
        zoomRectButton.UseVisualStyleBackColor = false;
        zoomRectButton.Click += zoomRectButton_Click;
        // 
        // panButton
        // 
        panButton.AccessibleName = "Pan";
        panButton.Appearance = Appearance.Button;
        panButton.BackColor = SystemColors.Control;
        panButton.BackgroundImage = (Image)resources.GetObject("panButton.Image");
        panButton.BackgroundImageLayout = ImageLayout.Zoom;
        panButton.Checked = true;
        panButton.CheckState = CheckState.Checked;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.Location = new Point(224, 0);
        panButton.Margin = new Padding(0);
        panButton.Name = "panButton";
        panButton.Size = new Size(36, 36);
        panButton.TabIndex = 6;
        panButton.Text = "";
        panButton.UseVisualStyleBackColor = false;
        panButton.Click += panButton_Click;
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 39);
        splitContainer.Margin = new Padding(0);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(leftPaneLayout);
        splitContainer.Panel1MinSize = 320;
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(rightPaneLayout);
        splitContainer.Panel2MinSize = 320;
        splitContainer.Size = new Size(1280, 697);
        splitContainer.SplitterDistance = 640;
        splitContainer.SplitterWidth = 2;
        splitContainer.TabIndex = 1;
        // 
        // leftPaneLayout
        // 
        leftPaneLayout.ColumnCount = 1;
        leftPaneLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        leftPaneLayout.Controls.Add(leftTitleLabel, 0, 0);
        leftPaneLayout.Controls.Add(leftViewerControl, 0, 1);
        leftPaneLayout.Dock = DockStyle.Fill;
        leftPaneLayout.Location = new Point(0, 0);
        leftPaneLayout.Margin = new Padding(0);
        leftPaneLayout.Name = "leftPaneLayout";
        leftPaneLayout.RowCount = 2;
        leftPaneLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        leftPaneLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        leftPaneLayout.Size = new Size(640, 697);
        leftPaneLayout.TabIndex = 0;
        // 
        // leftTitleLabel
        // 
        leftTitleLabel.BackColor = Color.FromArgb(238, 242, 241);
        leftTitleLabel.BorderStyle = BorderStyle.FixedSingle;
        leftTitleLabel.Dock = DockStyle.Fill;
        leftTitleLabel.Location = new Point(0, 0);
        leftTitleLabel.Margin = new Padding(0);
        leftTitleLabel.Name = "leftTitleLabel";
        leftTitleLabel.Size = new Size(640, 28);
        leftTitleLabel.TabIndex = 0;
        leftTitleLabel.Text = "Viewer A";
        leftTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // leftViewerControl
        // 
        leftViewerControl.BackColor = Color.White;
        leftViewerControl.Dock = DockStyle.Fill;
        leftViewerControl.Location = new Point(0, 0);
        leftViewerControl.Margin = new Padding(0);
        leftViewerControl.Name = "leftViewerControl";
        leftViewerControl.TabIndex = 1;
        // 
        // rightPaneLayout
        // 
        rightPaneLayout.ColumnCount = 1;
        rightPaneLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rightPaneLayout.Controls.Add(rightTitleLabel, 0, 0);
        rightPaneLayout.Controls.Add(rightViewerControl, 0, 1);
        rightPaneLayout.Dock = DockStyle.Fill;
        rightPaneLayout.Location = new Point(0, 0);
        rightPaneLayout.Margin = new Padding(0);
        rightPaneLayout.Name = "rightPaneLayout";
        rightPaneLayout.RowCount = 2;
        rightPaneLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        rightPaneLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rightPaneLayout.Size = new Size(638, 697);
        rightPaneLayout.TabIndex = 0;
        // 
        // rightTitleLabel
        // 
        rightTitleLabel.BackColor = Color.FromArgb(238, 242, 241);
        rightTitleLabel.BorderStyle = BorderStyle.FixedSingle;
        rightTitleLabel.Dock = DockStyle.Fill;
        rightTitleLabel.Location = new Point(0, 0);
        rightTitleLabel.Margin = new Padding(0);
        rightTitleLabel.Name = "rightTitleLabel";
        rightTitleLabel.Size = new Size(638, 28);
        rightTitleLabel.TabIndex = 0;
        rightTitleLabel.Text = "Viewer B";
        rightTitleLabel.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // rightViewerControl
        // 
        rightViewerControl.BackColor = Color.White;
        rightViewerControl.Dock = DockStyle.Fill;
        rightViewerControl.Location = new Point(0, 0);
        rightViewerControl.Margin = new Padding(0);
        rightViewerControl.Name = "rightViewerControl";
        rightViewerControl.TabIndex = 1;
        // 
        // statusStrip
        // 
        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar });
        statusStrip.Location = new Point(0, 736);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1280, 24);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(1083, 19);
        statusLabel.Spring = true;
        statusLabel.Text = "Sync enabled. Drive Viewer A.";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // downloadProgressBar
        // 
        downloadProgressBar.Name = "downloadProgressBar";
        downloadProgressBar.Size = new Size(180, 18);
        downloadProgressBar.Visible = false;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 760);
        Controls.Add(layoutPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MultiWindowSync";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        leftPaneLayout.ResumeLayout(false);
        rightPaneLayout.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
