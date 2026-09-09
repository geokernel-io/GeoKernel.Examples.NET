using GeoKernel.NET.WinForms;

namespace GeoKernel.ZoomToSelection.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private ToolTip toolbarToolTip;
    private CheckBox addButton;
    private CheckBox toggleButton;
    private CheckBox panButton;
    private Button clearButton;
    private Button zoomToSelectionButton;
    private Button fullExtentButton;
    private Label toolStateLabel;
    private SplitContainer splitContainer;
    private GeoKernelViewerControl geoKernelViewerControl;
    private DataGridView detailsGrid;
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
        addButton = new CheckBox();
        toggleButton = new CheckBox();
        panButton = new CheckBox();
        clearButton = new Button();
        zoomToSelectionButton = new Button();
        fullExtentButton = new Button();
        toolStateLabel = new Label();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernelViewerControl();
        detailsGrid = new DataGridView();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)detailsGrid).BeginInit();
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
        toolbarPanel.Controls.AddRange(new Control[] { addButton, toggleButton, panButton, clearButton, zoomToSelectionButton, fullExtentButton, toolStateLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // addButton
        // 
        addButton.Checked = true;
        addButton.CheckState = CheckState.Checked;
        addButton.Name = "addButton";
        addButton.Image = (Image)resources.GetObject("addButton.Image");
        addButton.ImageAlign = ContentAlignment.MiddleCenter;
        addButton.BackColor = Color.Transparent;
        addButton.FlatStyle = FlatStyle.Flat;
        addButton.FlatAppearance.BorderSize = 0;
        addButton.TabStop = false;
        addButton.AccessibleName = "Add Select";
        toolbarToolTip.SetToolTip(addButton, "Add Select");
        addButton.Appearance = Appearance.Button;
        addButton.AutoSize = false;
        addButton.Height = 28;
        addButton.Margin = new Padding(0, 3, 4, 3);
        addButton.Padding = new Padding(0);
        addButton.TextAlign = ContentAlignment.MiddleCenter;
        addButton.UseVisualStyleBackColor = false;
        addButton.Size = new Size(36, 36);
        addButton.Text = "";
        addButton.Click += addButton_Click;
        // 
        // toggleButton
        // 
        toggleButton.Name = "toggleButton";
        toggleButton.Image = (Image)resources.GetObject("toggleButton.Image");
        toggleButton.ImageAlign = ContentAlignment.MiddleCenter;
        toggleButton.BackColor = Color.Transparent;
        toggleButton.FlatStyle = FlatStyle.Flat;
        toggleButton.FlatAppearance.BorderSize = 0;
        toggleButton.TabStop = false;
        toggleButton.AccessibleName = "Toggle Select";
        toolbarToolTip.SetToolTip(toggleButton, "Toggle Select");
        toggleButton.Appearance = Appearance.Button;
        toggleButton.AutoSize = false;
        toggleButton.Height = 28;
        toggleButton.Margin = new Padding(0, 3, 4, 3);
        toggleButton.Padding = new Padding(0);
        toggleButton.TextAlign = ContentAlignment.MiddleCenter;
        toggleButton.UseVisualStyleBackColor = false;
        toggleButton.Size = new Size(36, 36);
        toggleButton.Text = "";
        toggleButton.Click += toggleButton_Click;
        // 
        // panButton
        // 
        panButton.Name = "panButton";
        panButton.Image = (Image)resources.GetObject("panButton.Image");
        panButton.ImageAlign = ContentAlignment.MiddleCenter;
        panButton.BackColor = Color.Transparent;
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
        panButton.UseVisualStyleBackColor = false;
        panButton.Size = new Size(36, 36);
        panButton.Text = "";
        panButton.Click += panButton_Click;
        // 
        // clearButton
        // 
        clearButton.Name = "clearButton";
        clearButton.Image = (Image)resources.GetObject("clearButton.Image");
        clearButton.ImageAlign = ContentAlignment.MiddleCenter;
        clearButton.BackColor = Color.Transparent;
        clearButton.FlatStyle = FlatStyle.Flat;
        clearButton.FlatAppearance.BorderSize = 0;
        clearButton.TabStop = false;
        clearButton.AccessibleName = "Clear Selection";
        toolbarToolTip.SetToolTip(clearButton, "Clear Selection");
        clearButton.AutoSize = false;
        clearButton.Height = 28;
        clearButton.Margin = new Padding(0, 3, 4, 3);
        clearButton.Padding = new Padding(0);
        clearButton.UseVisualStyleBackColor = false;
        clearButton.Size = new Size(36, 36);
        clearButton.Text = "";
        clearButton.Click += clearButton_Click;
        // 
        // zoomToSelectionButton
        // 
        zoomToSelectionButton.Name = "zoomToSelectionButton";
        zoomToSelectionButton.Image = (Image)resources.GetObject("zoomToSelectionButton.Image");
        zoomToSelectionButton.ImageAlign = ContentAlignment.MiddleCenter;
        zoomToSelectionButton.BackColor = Color.Transparent;
        zoomToSelectionButton.FlatStyle = FlatStyle.Flat;
        zoomToSelectionButton.FlatAppearance.BorderSize = 0;
        zoomToSelectionButton.TabStop = false;
        zoomToSelectionButton.AccessibleName = "Zoom To Selection";
        toolbarToolTip.SetToolTip(zoomToSelectionButton, "Zoom To Selection");
        zoomToSelectionButton.AutoSize = false;
        zoomToSelectionButton.Height = 28;
        zoomToSelectionButton.Margin = new Padding(0, 3, 4, 3);
        zoomToSelectionButton.Padding = new Padding(0);
        zoomToSelectionButton.UseVisualStyleBackColor = false;
        zoomToSelectionButton.Size = new Size(36, 36);
        zoomToSelectionButton.Text = "";
        zoomToSelectionButton.Click += zoomToSelectionButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Image = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.ImageAlign = ContentAlignment.MiddleCenter;
        fullExtentButton.BackColor = Color.Transparent;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.TabStop = false;
        fullExtentButton.AccessibleName = "Full Extent";
        toolbarToolTip.SetToolTip(fullExtentButton, "Full Extent");
        fullExtentButton.AutoSize = false;
        fullExtentButton.Height = 28;
        fullExtentButton.Margin = new Padding(0, 3, 4, 3);
        fullExtentButton.Padding = new Padding(0);
        fullExtentButton.UseVisualStyleBackColor = false;
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
        toolStateLabel.Size = new Size(173, 22);
        toolStateLabel.Text = "API: zoomToSelectedFeatures";
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.Location = new Point(0, 25);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(detailsGrid);
        splitContainer.Size = new Size(1184, 714);
        splitContainer.SplitterDistance = 834;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(834, 714);
        geoKernelViewerControl.TabIndex = 0;
        geoKernelViewerControl.MapMouseUp += geoKernelViewerControl_MapMouseUp;
        // 
        // detailsGrid
        // 
        detailsGrid.AllowUserToAddRows = false;
        detailsGrid.AllowUserToDeleteRows = false;
        detailsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        detailsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        detailsGrid.Dock = DockStyle.Fill;
        detailsGrid.Location = new Point(0, 0);
        detailsGrid.MultiSelect = false;
        detailsGrid.Name = "detailsGrid";
        detailsGrid.ReadOnly = true;
        detailsGrid.RowHeadersVisible = false;
        detailsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        detailsGrid.Size = new Size(346, 714);
        detailsGrid.TabIndex = 0;
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
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(900, 600);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ZoomToSelection";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)detailsGrid).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}

