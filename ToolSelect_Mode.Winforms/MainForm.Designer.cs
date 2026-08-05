using GeoKernel.NET.WinForms;

namespace GeoKernel.ToolSelect_Mode.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel toolbarPanel;
    private ToolTip toolbarToolTip;
    private Button selectButton;
    private Button panButton;
    private Button clearSelectionButton;
    private Button fullExtentButton;
    private Label toolStateLabel;
    private SplitContainer mainSplitContainer;
    private SplitContainer rightSplitContainer;
    private GeoKernelViewerControl geoKernelViewerControl;
    private DataGridView hitsGrid;
    private DataGridView attributesGrid;
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
        selectButton = new Button();
        panButton = new Button();
        clearSelectionButton = new Button();
        fullExtentButton = new Button();
        toolStateLabel = new Label();
        mainSplitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernelViewerControl();
        rightSplitContainer = new SplitContainer();
        hitsGrid = new DataGridView();
        attributesGrid = new DataGridView();
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
        toolbarPanel.Controls.Add(selectButton);
        toolbarPanel.Controls.Add(panButton);
        toolbarPanel.Controls.Add(clearSelectionButton);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(toolStateLabel);
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1184, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // selectButton
        // 
        selectButton.FlatStyle = FlatStyle.Flat;
        selectButton.Location = new Point(4, 2);
        selectButton.Name = "selectButton";
        selectButton.BackgroundImage = (Image)resources.GetObject("selectButton.Image");
        selectButton.BackgroundImageLayout = ImageLayout.Zoom;
        selectButton.FlatStyle = FlatStyle.Flat;
        selectButton.FlatAppearance.BorderSize = 0;
        selectButton.TabStop = false;
        selectButton.AccessibleName = "Box Select";
        toolbarToolTip.SetToolTip(selectButton, "Box Select");
        selectButton.Size = new Size(36, 34);
        selectButton.TabIndex = 0;
        selectButton.Text = "";
        selectButton.UseVisualStyleBackColor = true;
        selectButton.Click += selectButton_Click;
        // 
        // panButton
        // 
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.Location = new Point(40, 2);
        panButton.Name = "panButton";
        panButton.BackgroundImage = (Image)resources.GetObject("panButton.Image");
        panButton.BackgroundImageLayout = ImageLayout.Zoom;
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
        // clearSelectionButton
        // 
        clearSelectionButton.FlatStyle = FlatStyle.Flat;
        clearSelectionButton.FlatAppearance.BorderSize = 0;
        clearSelectionButton.Location = new Point(76, 2);
        clearSelectionButton.Name = "clearSelectionButton";
        clearSelectionButton.BackgroundImage = (Image)resources.GetObject("clearSelectionButton.Image");
        clearSelectionButton.BackgroundImageLayout = ImageLayout.Zoom;
        clearSelectionButton.FlatStyle = FlatStyle.Flat;
        clearSelectionButton.FlatAppearance.BorderSize = 0;
        clearSelectionButton.TabStop = false;
        clearSelectionButton.AccessibleName = "Clear Selection";
        toolbarToolTip.SetToolTip(clearSelectionButton, "Clear Selection");
        clearSelectionButton.Size = new Size(36, 34);
        clearSelectionButton.TabIndex = 2;
        clearSelectionButton.Text = "";
        clearSelectionButton.UseVisualStyleBackColor = true;
        clearSelectionButton.Click += clearSelectionButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.Location = new Point(112, 2);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.BackgroundImage = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.BackgroundImageLayout = ImageLayout.Zoom;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.TabStop = false;
        fullExtentButton.AccessibleName = "Full Extent";
        toolbarToolTip.SetToolTip(fullExtentButton, "Full Extent");
        fullExtentButton.Size = new Size(36, 34);
        fullExtentButton.TabIndex = 3;
        fullExtentButton.Text = "";
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // toolStateLabel
        // 
        toolStateLabel.AutoSize = true;
        toolStateLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toolStateLabel.Location = new Point(160, 12);
        toolStateLabel.Name = "toolStateLabel";
        toolStateLabel.Size = new Size(266, 15);
        toolStateLabel.TabIndex = 4;
        toolStateLabel.Text = "API: hitTestFeaturesInScreenRect(screenRect)";
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
        mainSplitContainer.Panel2.Controls.Add(rightSplitContainer);
        mainSplitContainer.Size = new Size(1184, 700);
        mainSplitContainer.SplitterDistance = 824;
        mainSplitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(824, 700);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // rightSplitContainer
        // 
        rightSplitContainer.Dock = DockStyle.Fill;
        rightSplitContainer.Location = new Point(0, 0);
        rightSplitContainer.Name = "rightSplitContainer";
        rightSplitContainer.Orientation = Orientation.Horizontal;
        // 
        // rightSplitContainer.Panel1
        // 
        rightSplitContainer.Panel1.Controls.Add(hitsGrid);
        // 
        // rightSplitContainer.Panel2
        // 
        rightSplitContainer.Panel2.Controls.Add(attributesGrid);
        rightSplitContainer.Size = new Size(356, 700);
        rightSplitContainer.SplitterDistance = 268;
        rightSplitContainer.TabIndex = 0;
        // 
        // hitsGrid
        // 
        hitsGrid.AllowUserToAddRows = false;
        hitsGrid.AllowUserToDeleteRows = false;
        hitsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        hitsGrid.Dock = DockStyle.Fill;
        hitsGrid.Location = new Point(0, 0);
        hitsGrid.MultiSelect = false;
        hitsGrid.Name = "hitsGrid";
        hitsGrid.ReadOnly = true;
        hitsGrid.RowHeadersVisible = false;
        hitsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        hitsGrid.Size = new Size(356, 268);
        hitsGrid.TabIndex = 0;
        hitsGrid.SelectionChanged += hitsGrid_SelectionChanged;
        // 
        // attributesGrid
        // 
        attributesGrid.AllowUserToAddRows = false;
        attributesGrid.AllowUserToDeleteRows = false;
        attributesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        attributesGrid.Dock = DockStyle.Fill;
        attributesGrid.Location = new Point(0, 0);
        attributesGrid.MultiSelect = false;
        attributesGrid.Name = "attributesGrid";
        attributesGrid.ReadOnly = true;
        attributesGrid.RowHeadersVisible = false;
        attributesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        attributesGrid.Size = new Size(356, 428);
        attributesGrid.TabIndex = 0;
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
        Controls.Add(mainSplitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(950, 620);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ToolSelect_Mode";
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
