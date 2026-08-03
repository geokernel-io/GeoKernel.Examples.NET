namespace GeoKernel.LayerReorder.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button zoomInButton;
    private Button zoomOutButton;
    private Button fullExtentButton;
    private Panel toolbarSeparator;
    private Button zoomRectButton;
    private Button panButton;
    private SplitContainer splitContainer;
    private TableLayoutPanel sidePanelLayout;
    private ListBox layerListBox;
    private Button moveUpButton;
    private Button moveDownButton;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;

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
        toolbarSeparator = new Panel();
        zoomRectButton = new Button();
        panButton = new Button();
        splitContainer = new SplitContainer();
        sidePanelLayout = new TableLayoutPanel();
        layerListBox = new ListBox();
        moveUpButton = new Button();
        moveDownButton = new Button();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        sidePanelLayout.SuspendLayout();
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
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(panButton);
        toolbarPanel.Controls.Add(zoomRectButton);
        toolbarPanel.Controls.Add(toolbarSeparator);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(zoomOutButton);
        toolbarPanel.Controls.Add(zoomInButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Size = new Size(1200, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // zoomInButton
        // 
        zoomInButton.BackColor = SystemColors.Control;
        zoomInButton.BackgroundImage = (Image)resources.GetObject("zoomInButton.Image");
        zoomInButton.BackgroundImageLayout = ImageLayout.Zoom;
        zoomInButton.FlatAppearance.BorderSize = 0;
        zoomInButton.FlatStyle = FlatStyle.Flat;
        zoomInButton.Location = new Point(0, 0);
        zoomInButton.Margin = new Padding(0);
        zoomInButton.Name = "zoomInButton";
        zoomInButton.Padding = new Padding(3);
        zoomInButton.Size = new Size(36, 36);
        zoomInButton.TabIndex = 0;
        zoomInButton.Text = "";
        zoomInButton.UseVisualStyleBackColor = false;
        zoomInButton.Click += zoomInButton_Click;
        // 
        // zoomOutButton
        // 
        zoomOutButton.BackColor = SystemColors.Control;
        zoomOutButton.BackgroundImage = (Image)resources.GetObject("zoomOutButton.Image");
        zoomOutButton.BackgroundImageLayout = ImageLayout.Zoom;
        zoomOutButton.FlatAppearance.BorderSize = 0;
        zoomOutButton.FlatStyle = FlatStyle.Flat;
        zoomOutButton.Location = new Point(36, 0);
        zoomOutButton.Margin = new Padding(0);
        zoomOutButton.Name = "zoomOutButton";
        zoomOutButton.Padding = new Padding(3);
        zoomOutButton.Size = new Size(36, 36);
        zoomOutButton.TabIndex = 1;
        zoomOutButton.Text = "";
        zoomOutButton.UseVisualStyleBackColor = false;
        zoomOutButton.Click += zoomOutButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.BackColor = SystemColors.Control;
        fullExtentButton.BackgroundImage = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.BackgroundImageLayout = ImageLayout.Zoom;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.Location = new Point(72, 0);
        fullExtentButton.Margin = new Padding(0);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Padding = new Padding(3);
        fullExtentButton.Size = new Size(36, 36);
        fullExtentButton.TabIndex = 2;
        fullExtentButton.Text = "";
        fullExtentButton.UseVisualStyleBackColor = false;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // toolbarSeparator
        // 
        toolbarSeparator.BackColor = SystemColors.ControlDark;
        toolbarSeparator.Location = new Point(112, 4);
        toolbarSeparator.Margin = new Padding(0);
        toolbarSeparator.Name = "toolbarSeparator";
        toolbarSeparator.Size = new Size(1, 28);
        toolbarSeparator.TabIndex = 3;
        // 
        // zoomRectButton
        // 
        zoomRectButton.BackColor = SystemColors.Control;
        zoomRectButton.BackgroundImage = (Image)resources.GetObject("zoomRectButton.Image");
        zoomRectButton.BackgroundImageLayout = ImageLayout.Zoom;
        zoomRectButton.FlatAppearance.BorderSize = 0;
        zoomRectButton.FlatStyle = FlatStyle.Flat;
        zoomRectButton.Location = new Point(116, 0);
        zoomRectButton.Margin = new Padding(0);
        zoomRectButton.Name = "zoomRectButton";
        zoomRectButton.Padding = new Padding(3);
        zoomRectButton.Size = new Size(36, 36);
        zoomRectButton.TabIndex = 4;
        zoomRectButton.Text = "";
        zoomRectButton.UseVisualStyleBackColor = false;
        zoomRectButton.Click += zoomRectButton_Click;
        // 
        // panButton
        // 
        panButton.BackColor = SystemColors.Control;
        panButton.BackgroundImage = (Image)resources.GetObject("panButton.Image");
        panButton.BackgroundImageLayout = ImageLayout.Zoom;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.Location = new Point(152, 0);
        panButton.Margin = new Padding(0);
        panButton.Name = "panButton";
        panButton.Padding = new Padding(3);
        panButton.Size = new Size(36, 36);
        panButton.TabIndex = 5;
        panButton.Text = "";
        panButton.UseVisualStyleBackColor = false;
        panButton.Click += panButton_Click;
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel1;
        splitContainer.Location = new Point(0, 39);
        splitContainer.Margin = new Padding(0);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(sidePanelLayout);
        splitContainer.Panel1MinSize = 180;
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(geoKernelViewerControl);
        splitContainer.Size = new Size(1200, 737);
        splitContainer.SplitterDistance = 220;
        splitContainer.TabIndex = 1;
        // 
        // sidePanelLayout
        // 
        sidePanelLayout.ColumnCount = 1;
        sidePanelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sidePanelLayout.Controls.Add(layerListBox, 0, 0);
        sidePanelLayout.Controls.Add(moveUpButton, 0, 1);
        sidePanelLayout.Controls.Add(moveDownButton, 0, 2);
        sidePanelLayout.Dock = DockStyle.Fill;
        sidePanelLayout.Location = new Point(0, 0);
        sidePanelLayout.Name = "sidePanelLayout";
        sidePanelLayout.Padding = new Padding(8);
        sidePanelLayout.RowCount = 3;
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        sidePanelLayout.Size = new Size(220, 737);
        sidePanelLayout.TabIndex = 0;
        // 
        // layerListBox
        // 
        layerListBox.Dock = DockStyle.Fill;
        layerListBox.FormattingEnabled = true;
        layerListBox.Location = new Point(8, 8);
        layerListBox.Margin = new Padding(0, 0, 0, 8);
        layerListBox.Name = "layerListBox";
        layerListBox.Size = new Size(204, 649);
        layerListBox.TabIndex = 0;
        layerListBox.SelectedIndexChanged += layerListBox_SelectedIndexChanged;
        // 
        // moveUpButton
        // 
        moveUpButton.Dock = DockStyle.Fill;
        moveUpButton.Location = new Point(8, 665);
        moveUpButton.Margin = new Padding(0, 0, 0, 4);
        moveUpButton.Name = "moveUpButton";
        moveUpButton.Size = new Size(204, 32);
        moveUpButton.TabIndex = 1;
        moveUpButton.Text = "Move Up";
        moveUpButton.UseVisualStyleBackColor = true;
        moveUpButton.Click += moveUpButton_Click;
        // 
        // moveDownButton
        // 
        moveDownButton.Dock = DockStyle.Fill;
        moveDownButton.Location = new Point(8, 697);
        moveDownButton.Margin = new Padding(0);
        moveDownButton.Name = "moveDownButton";
        moveDownButton.Size = new Size(204, 32);
        moveDownButton.TabIndex = 2;
        moveDownButton.Text = "Move Down";
        moveDownButton.UseVisualStyleBackColor = true;
        moveDownButton.Click += moveDownButton_Click;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
        statusStrip.Location = new Point(0, 776);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1200, 24);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(1003, 19);
        statusLabel.Spring = true;
        statusLabel.Text = "Layers: 0";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // progressBar
        // 
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(180, 18);
        progressBar.Visible = false;
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
        Text = "LayerReorder";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        sidePanelLayout.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
