using GeoKernel.NET.WinForms;

namespace GeoKernel.AllFeaturesAtPoint.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel toolbarPanel;
    private ToolTip toolbarToolTip;
    private Button identifyButton;
    private Button panButton;
    private Button fullExtentButton;
    private Label toolStateLabel;
    private SplitContainer mainSplitContainer;
    private SplitContainer rightSplitContainer;
    private GeoKernelViewerControl geoKernelViewerControl;
    private DataGridView hitsGrid;
    private DataGridView attributesGrid;
    private GroupBox hitsGroupBox;
    private GroupBox attributesGroupBox;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        toolbarPanel = new Panel();
        toolbarToolTip = new ToolTip();
        identifyButton = new Button();
        panButton = new Button();
        fullExtentButton = new Button();
        toolStateLabel = new Label();
        mainSplitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernelViewerControl();
        rightSplitContainer = new SplitContainer();
        hitsGrid = new DataGridView();
        attributesGrid = new DataGridView();
        hitsGroupBox = new GroupBox();
        attributesGroupBox = new GroupBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
        mainSplitContainer.Panel1.SuspendLayout();
        mainSplitContainer.Panel2.SuspendLayout();
        mainSplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)rightSplitContainer).BeginInit();
        rightSplitContainer.Panel1.SuspendLayout();
        rightSplitContainer.Panel2.SuspendLayout();
        rightSplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)hitsGrid).BeginInit();
        ((System.ComponentModel.ISupportInitialize)attributesGrid).BeginInit();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = Color.FromArgb(242, 242, 242);
        toolbarPanel.Controls.Add(identifyButton);
        toolbarPanel.Controls.Add(panButton);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(toolStateLabel);
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1184, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // identifyButton
        // 
        identifyButton.FlatStyle = FlatStyle.Flat;
        identifyButton.Location = new Point(4, 2);
        identifyButton.Name = "identifyButton";
        identifyButton.BackgroundImage = (Image)resources.GetObject("identifyButton.Image");
        identifyButton.BackgroundImageLayout = ImageLayout.Center;
        identifyButton.FlatStyle = FlatStyle.Flat;
        identifyButton.FlatAppearance.BorderSize = 0;
        identifyButton.TabStop = false;
        identifyButton.AccessibleName = "All Features Here";
        toolbarToolTip.SetToolTip(identifyButton, "All Features Here");
        identifyButton.Size = new Size(36, 34);
        identifyButton.TabIndex = 0;
        identifyButton.Text = "";
        identifyButton.UseVisualStyleBackColor = true;
        identifyButton.Click += identifyButton_Click;
        // 
        // panButton
        // 
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.Location = new Point(40, 2);
        panButton.Name = "panButton";
        panButton.BackgroundImage = (Image)resources.GetObject("panButton.Image");
        panButton.BackgroundImageLayout = ImageLayout.Center;
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.TabStop = false;
        panButton.AccessibleName = "Pan";
        toolbarToolTip.SetToolTip(panButton, "Pan");
        panButton.Size = new Size(36, 34);
        panButton.TabIndex = 1;
        panButton.Text = "";
        panButton.UseVisualStyleBackColor = true;
        panButton.Click += panButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.Location = new Point(76, 2);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.BackgroundImage = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.BackgroundImageLayout = ImageLayout.Center;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.TabStop = false;
        fullExtentButton.AccessibleName = "Full Extent";
        toolbarToolTip.SetToolTip(fullExtentButton, "Full Extent");
        fullExtentButton.Size = new Size(36, 34);
        fullExtentButton.TabIndex = 2;
        fullExtentButton.Text = "";
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // toolStateLabel
        // 
        toolStateLabel.AutoSize = true;
        toolStateLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toolStateLabel.Location = new Point(124, 12);
        toolStateLabel.Name = "toolStateLabel";
        toolStateLabel.Size = new Size(135, 15);
        toolStateLabel.TabIndex = 3;
        toolStateLabel.Text = "Tool: hitTestFeaturesAt";
        // 
        // mainSplitContainer
        // 
        mainSplitContainer.Dock = DockStyle.Fill;
        mainSplitContainer.FixedPanel = FixedPanel.Panel2;
        mainSplitContainer.Location = new Point(0, 39);
        mainSplitContainer.Name = "mainSplitContainer";
        // 
        // mainSplitContainer.Panel1
        // 
        mainSplitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // mainSplitContainer.Panel2
        // 
        mainSplitContainer.Panel2.Controls.Add(hitsGroupBox);
        mainSplitContainer.Size = new Size(1184, 500);
        mainSplitContainer.SplitterDistance = 920;
        mainSplitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(920, 500);
        geoKernelViewerControl.TabIndex = 0;
        geoKernelViewerControl.MapMouseUp += geoKernelViewerControl_MapMouseUp;
        // 
        // rightSplitContainer
        // 
        rightSplitContainer.Dock = DockStyle.Fill;
        rightSplitContainer.Location = new Point(0, 39);
        rightSplitContainer.Name = "rightSplitContainer";
        rightSplitContainer.Orientation = Orientation.Horizontal;
        // 
        // rightSplitContainer.Panel1
        // 
        rightSplitContainer.Panel1.Controls.Add(mainSplitContainer);
        // 
        // rightSplitContainer.Panel2
        // 
        rightSplitContainer.Panel2.Controls.Add(attributesGroupBox);
        rightSplitContainer.Size = new Size(1184, 700);
        rightSplitContainer.SplitterDistance = 500;
        rightSplitContainer.TabIndex = 0;
        // 
        // hitsGrid
        // 
        hitsGrid.AllowUserToAddRows = false;
        hitsGrid.AllowUserToDeleteRows = false;
        hitsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        hitsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        hitsGrid.Dock = DockStyle.Fill;
        hitsGrid.Location = new Point(0, 0);
        hitsGrid.MultiSelect = false;
        hitsGrid.Name = "hitsGrid";
        hitsGrid.ReadOnly = true;
        hitsGrid.RowHeadersVisible = false;
        hitsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        hitsGrid.Size = new Size(254, 477);
        hitsGrid.TabIndex = 0;
        hitsGrid.SelectionChanged += hitsGrid_SelectionChanged;
        // 
        // attributesGrid
        // 
        attributesGrid.AllowUserToAddRows = false;
        attributesGrid.AllowUserToDeleteRows = false;
        attributesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        attributesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        attributesGrid.Dock = DockStyle.Fill;
        attributesGrid.Location = new Point(0, 0);
        attributesGrid.MultiSelect = false;
        attributesGrid.Name = "attributesGrid";
        attributesGrid.ReadOnly = true;
        attributesGrid.RowHeadersVisible = false;
        attributesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        attributesGrid.Size = new Size(1172, 177);
        attributesGrid.TabIndex = 0;
        // 
        // hitsGroupBox
        // 
        hitsGroupBox.Controls.Add(hitsGrid);
        hitsGroupBox.Dock = DockStyle.Fill;
        hitsGroupBox.Name = "hitsGroupBox";
        hitsGroupBox.Padding = new Padding(3, 18, 3, 3);
        hitsGroupBox.Size = new Size(260, 500);
        hitsGroupBox.TabIndex = 0;
        hitsGroupBox.TabStop = false;
        hitsGroupBox.Text = "Features at clicked point";
        // 
        // attributesGroupBox
        // 
        attributesGroupBox.Controls.Add(attributesGrid);
        attributesGroupBox.Dock = DockStyle.Fill;
        attributesGroupBox.Name = "attributesGroupBox";
        attributesGroupBox.Padding = new Padding(3, 18, 3, 3);
        attributesGroupBox.Size = new Size(1184, 196);
        attributesGroupBox.TabIndex = 0;
        attributesGroupBox.TabStop = false;
        attributesGroupBox.Text = "Selected hit details";
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar });
        statusStrip.Location = new Point(0, 739);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1184, 22);
        statusStrip.TabIndex = 2;
        // 
        // downloadProgressBar
        // 
        downloadProgressBar.Name = "downloadProgressBar";
        downloadProgressBar.Size = new Size(180, 16);
        downloadProgressBar.Visible = false;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(42, 17);
        statusLabel.Text = "Ready.";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 761);
        Controls.Add(rightSplitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(950, 620);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "AllFeaturesAtPoint";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        mainSplitContainer.Panel1.ResumeLayout(false);
        mainSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
        mainSplitContainer.ResumeLayout(false);
        rightSplitContainer.Panel1.ResumeLayout(false);
        rightSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)rightSplitContainer).EndInit();
        rightSplitContainer.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)hitsGrid).EndInit();
        ((System.ComponentModel.ISupportInitialize)attributesGrid).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
