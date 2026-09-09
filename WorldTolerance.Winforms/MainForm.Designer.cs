using GeoKernel.NET.WinForms;

namespace GeoKernel.WorldTolerance.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private ToolTip toolbarToolTip;
    private CheckBox identifyButton;
    private CheckBox panButton;
    private Button fullExtentButton;
    private Label toleranceLabel;
    private NumericUpDown toleranceUpDown;
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
        toolbarPanel = new FlowLayoutPanel();
        toolbarToolTip = new ToolTip();
        identifyButton = new CheckBox();
        panButton = new CheckBox();
        fullExtentButton = new Button();
        toleranceLabel = new Label();
        toleranceUpDown = new NumericUpDown();
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
        ((System.ComponentModel.ISupportInitialize)toleranceUpDown).BeginInit();
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
        toolbarPanel.AutoSize = false;
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.FlowDirection = FlowDirection.LeftToRight;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Controls.AddRange(new Control[] { identifyButton, panButton, fullExtentButton, toleranceLabel, toleranceUpDown, toolStateLabel });
        toolbarPanel.Size = new Size(1200, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // identifyButton
        // 
        identifyButton.Checked = true;
        identifyButton.CheckState = CheckState.Checked;
        identifyButton.Name = "identifyButton";
        identifyButton.BackgroundImage = (Image)resources.GetObject("identifyButton.Image");
        identifyButton.BackgroundImageLayout = ImageLayout.Zoom;
        identifyButton.FlatStyle = FlatStyle.Flat;
        identifyButton.FlatAppearance.BorderSize = 0;
        identifyButton.TabStop = false;
        identifyButton.AccessibleName = "World Tolerance";
        toolbarToolTip.SetToolTip(identifyButton, "World Tolerance");
        identifyButton.Appearance = Appearance.Button;
        identifyButton.AutoSize = false;
        identifyButton.Height = 34;
        identifyButton.Margin = new Padding(0, 2, 2, 2);
        identifyButton.Padding = new Padding(0);
        identifyButton.TextAlign = ContentAlignment.MiddleCenter;
        identifyButton.UseVisualStyleBackColor = true;
        identifyButton.Size = new Size(36, 34);
        identifyButton.Text = "";
        identifyButton.Click += identifyButton_Click;
        // 
        // panButton
        // 
        panButton.Name = "panButton";
        panButton.BackgroundImage = (Image)resources.GetObject("panButton.Image");
        panButton.BackgroundImageLayout = ImageLayout.Zoom;
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.TabStop = false;
        panButton.AccessibleName = "Pan";
        toolbarToolTip.SetToolTip(panButton, "Pan");
        panButton.Appearance = Appearance.Button;
        panButton.AutoSize = false;
        panButton.Height = 34;
        panButton.Margin = new Padding(0, 2, 2, 2);
        panButton.Padding = new Padding(0);
        panButton.TextAlign = ContentAlignment.MiddleCenter;
        panButton.UseVisualStyleBackColor = true;
        panButton.Size = new Size(36, 34);
        panButton.Text = "";
        panButton.Click += panButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.BackgroundImage = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.BackgroundImageLayout = ImageLayout.Zoom;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.TabStop = false;
        fullExtentButton.AccessibleName = "Full Extent";
        toolbarToolTip.SetToolTip(fullExtentButton, "Full Extent");
        fullExtentButton.AutoSize = false;
        fullExtentButton.Height = 34;
        fullExtentButton.Margin = new Padding(0, 2, 4, 2);
        fullExtentButton.Padding = new Padding(0);
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Size = new Size(36, 34);
        fullExtentButton.Text = "";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // toleranceLabel
        // 
        toleranceLabel.Margin = new Padding(12, 1, 2, 2);
        toleranceLabel.Name = "toleranceLabel";
        toleranceLabel.AutoSize = true;
        toleranceLabel.Margin = new Padding(8, 9, 8, 0);
        toleranceLabel.TextAlign = ContentAlignment.MiddleLeft;
        toleranceLabel.Size = new Size(94, 23);
        toleranceLabel.Text = "World tolerance:";
        // 
        // toleranceUpDown
        // 
        toleranceUpDown.AccessibleName = "toleranceUpDown";
        toleranceUpDown.DecimalPlaces = 2;
        toleranceUpDown.Increment = new decimal(new int[] { 25, 0, 0, 131072 });
        toleranceUpDown.Location = new Point(301, 1);
        toleranceUpDown.Maximum = new decimal(new int[] { 10, 0, 0, 0 });
        toleranceUpDown.Name = "toleranceUpDown";
        toleranceUpDown.Margin = new Padding(0, 4, 8, 0);
        toleranceUpDown.Size = new Size(50, 23);
        toleranceUpDown.TabIndex = 0;
        toleranceUpDown.Value = new decimal(new int[] { 1, 0, 0, 0 });
        // 
        // 
        // toolStateLabel
        // 
        toolStateLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toolStateLabel.Margin = new Padding(12, 1, 0, 2);
        toolStateLabel.Name = "toolStateLabel";
        toolStateLabel.AutoSize = true;
        toolStateLabel.Margin = new Padding(8, 9, 8, 0);
        toolStateLabel.TextAlign = ContentAlignment.MiddleLeft;
        toolStateLabel.Size = new Size(246, 23);
        toolStateLabel.Text = "API: hitTestFeatures(worldPoint, tolerance)";
        // 
        // mainSplitContainer
        // 
        mainSplitContainer.Dock = DockStyle.Fill;
        mainSplitContainer.FixedPanel = FixedPanel.Panel2;
        mainSplitContainer.Location = new Point(0, 26);
        mainSplitContainer.Name = "mainSplitContainer";
        // 
        // mainSplitContainer.Panel1
        // 
        mainSplitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // mainSplitContainer.Panel2
        // 
        mainSplitContainer.Panel2.Controls.Add(rightSplitContainer);
        mainSplitContainer.Size = new Size(1184, 713);
        mainSplitContainer.SplitterDistance = 824;
        mainSplitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(824, 713);
        geoKernelViewerControl.TabIndex = 0;
        geoKernelViewerControl.MapMouseUp += geoKernelViewerControl_MapMouseUp;
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
        rightSplitContainer.Size = new Size(356, 713);
        rightSplitContainer.SplitterDistance = 267;
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
        hitsGrid.Size = new Size(356, 267);
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
        attributesGrid.Size = new Size(356, 442);
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
        Text = "WorldTolerance";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)toleranceUpDown).EndInit();
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
