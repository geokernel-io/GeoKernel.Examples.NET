using GeoKernel.NET.WinForms;

namespace GeoKernel.SelectionBoxSignal.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private ToolTip toolbarToolTip;
    private CheckBox selectButton;
    private CheckBox panButton;
    private Button clearSelectionButton;
    private Button fullExtentButton;
    private Label toolStateLabel;
    private SplitContainer mainSplitContainer;
    private SplitContainer rightSplitContainer;
    private GeoKernelViewerControl geoKernelViewerControl;
    private DataGridView signalGrid;
    private DataGridView hitsGrid;
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
        selectButton = new CheckBox();
        panButton = new CheckBox();
        clearSelectionButton = new Button();
        fullExtentButton = new Button();
        toolStateLabel = new Label();
        mainSplitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernelViewerControl();
        rightSplitContainer = new SplitContainer();
        signalGrid = new DataGridView();
        hitsGrid = new DataGridView();
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
        ((System.ComponentModel.ISupportInitialize)signalGrid).BeginInit();
        ((System.ComponentModel.ISupportInitialize)hitsGrid).BeginInit();
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
        toolbarPanel.Controls.AddRange(new Control[] { selectButton, panButton, clearSelectionButton, fullExtentButton, toolStateLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // selectButton
        // 
        selectButton.Checked = true;
        selectButton.CheckState = CheckState.Checked;
        selectButton.Name = "selectButton";
        selectButton.BackgroundImage = (Image)resources.GetObject("selectButton.Image");
        selectButton.BackgroundImageLayout = ImageLayout.Center;
        selectButton.FlatStyle = FlatStyle.Flat;
        selectButton.FlatAppearance.BorderSize = 0;
        selectButton.TabStop = false;
        selectButton.AccessibleName = "Box Select";
        toolbarToolTip.SetToolTip(selectButton, "Box Select");
        selectButton.Appearance = Appearance.Button;
        selectButton.AutoSize = false;
        selectButton.Height = 28;
        selectButton.Margin = new Padding(0, 3, 4, 3);
        selectButton.Padding = new Padding(0);
        selectButton.TextAlign = ContentAlignment.MiddleCenter;
        selectButton.UseVisualStyleBackColor = true;
        selectButton.Size = new Size(36, 36);
        selectButton.Text = "";
        selectButton.Click += selectButton_Click;
        // 
        // panButton
        // 
        panButton.Name = "panButton";
        panButton.BackgroundImage = (Image)resources.GetObject("panButton.Image");
        panButton.BackgroundImageLayout = ImageLayout.Center;
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.TabStop = false;
        panButton.AccessibleName = "Pan";
        toolbarToolTip.SetToolTip(panButton, "Pan");
        panButton.Appearance = Appearance.Button;
        panButton.AutoSize = false;
        panButton.Height = 28;
        panButton.Margin = new Padding(0, 3, 4, 3);
        panButton.Padding = new Padding(0);
        panButton.TextAlign = ContentAlignment.MiddleCenter;
        panButton.UseVisualStyleBackColor = true;
        panButton.Size = new Size(36, 36);
        panButton.Text = "";
        panButton.Click += panButton_Click;
        // 
        // clearSelectionButton
        // 
        clearSelectionButton.Name = "clearSelectionButton";
        clearSelectionButton.BackgroundImage = (Image)resources.GetObject("clearSelectionButton.Image");
        clearSelectionButton.BackgroundImageLayout = ImageLayout.Center;
        clearSelectionButton.FlatStyle = FlatStyle.Flat;
        clearSelectionButton.FlatAppearance.BorderSize = 0;
        clearSelectionButton.TabStop = false;
        clearSelectionButton.AccessibleName = "Clear Selection";
        toolbarToolTip.SetToolTip(clearSelectionButton, "Clear Selection");
        clearSelectionButton.AutoSize = false;
        clearSelectionButton.Height = 28;
        clearSelectionButton.Margin = new Padding(0, 3, 4, 3);
        clearSelectionButton.Padding = new Padding(0);
        clearSelectionButton.UseVisualStyleBackColor = true;
        clearSelectionButton.Size = new Size(36, 36);
        clearSelectionButton.Text = "";
        clearSelectionButton.Click += clearSelectionButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.BackgroundImage = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.BackgroundImageLayout = ImageLayout.Center;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.TabStop = false;
        fullExtentButton.AccessibleName = "Full Extent";
        toolbarToolTip.SetToolTip(fullExtentButton, "Full Extent");
        fullExtentButton.AutoSize = false;
        fullExtentButton.Height = 28;
        fullExtentButton.Margin = new Padding(0, 3, 4, 3);
        fullExtentButton.Padding = new Padding(0);
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Size = new Size(36, 36);
        fullExtentButton.Text = "";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // toolStateLabel
        // 
        toolStateLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toolStateLabel.Margin = new Padding(12, 1, 0, 2);
        toolStateLabel.Name = "toolStateLabel";
        toolStateLabel.AutoSize = true;
        toolStateLabel.Margin = new Padding(8, 9, 8, 0);
        toolStateLabel.TextAlign = ContentAlignment.MiddleLeft;
        toolStateLabel.Size = new Size(323, 22);
        toolStateLabel.Text = "Signal: mapSelectionBoxFinished(rect, extent, modifiers)";
        // 
        // mainSplitContainer
        // 
        mainSplitContainer.Dock = DockStyle.Fill;
        mainSplitContainer.FixedPanel = FixedPanel.Panel2;
        mainSplitContainer.Location = new Point(0, 25);
        mainSplitContainer.Name = "mainSplitContainer";
        // 
        // mainSplitContainer.Panel1
        // 
        mainSplitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // mainSplitContainer.Panel2
        // 
        mainSplitContainer.Panel2.Controls.Add(rightSplitContainer);
        mainSplitContainer.Size = new Size(1184, 714);
        mainSplitContainer.SplitterDistance = 744;
        mainSplitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(744, 714);
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
        rightSplitContainer.Panel1.Controls.Add(signalGrid);
        // 
        // rightSplitContainer.Panel2
        // 
        rightSplitContainer.Panel2.Controls.Add(hitsGrid);
        rightSplitContainer.Size = new Size(436, 714);
        rightSplitContainer.SplitterDistance = 260;
        rightSplitContainer.TabIndex = 0;
        // 
        // signalGrid
        // 
        signalGrid.AllowUserToAddRows = false;
        signalGrid.AllowUserToDeleteRows = false;
        signalGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        signalGrid.Dock = DockStyle.Fill;
        signalGrid.Location = new Point(0, 0);
        signalGrid.MultiSelect = false;
        signalGrid.Name = "signalGrid";
        signalGrid.ReadOnly = true;
        signalGrid.RowHeadersVisible = false;
        signalGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        signalGrid.Size = new Size(436, 260);
        signalGrid.TabIndex = 0;
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
        hitsGrid.Size = new Size(436, 450);
        hitsGrid.TabIndex = 0;
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
        Text = "SelectionBoxSignal";
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
        ((System.ComponentModel.ISupportInitialize)signalGrid).EndInit();
        ((System.ComponentModel.ISupportInitialize)hitsGrid).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
