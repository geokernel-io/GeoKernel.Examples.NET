using GeoKernel.NET.WinForms;

namespace GeoKernel.InfoTool.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private ToolTip toolbarToolTip;
    private CheckBox hitTestButton;
    private CheckBox panButton;
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
        hitTestButton = new CheckBox();
        panButton = new CheckBox();
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
        toolbarPanel.Controls.AddRange(new Control[] { hitTestButton, panButton, fullExtentButton, toolStateLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // hitTestButton
        // 
        hitTestButton.Checked = true;
        hitTestButton.CheckState = CheckState.Checked;
        hitTestButton.Name = "hitTestButton";
        hitTestButton.BackgroundImage = (Image)resources.GetObject("hitTestButton.Image");
        hitTestButton.BackgroundImageLayout = ImageLayout.Center;
        hitTestButton.BackColor = Color.Transparent;
        hitTestButton.FlatStyle = FlatStyle.Flat;
        hitTestButton.FlatAppearance.BorderSize = 0;
        hitTestButton.FlatAppearance.CheckedBackColor = Color.Transparent;
        hitTestButton.FlatAppearance.MouseDownBackColor = Color.Transparent;
        hitTestButton.FlatAppearance.MouseOverBackColor = Color.Transparent;
        hitTestButton.TabStop = false;
        hitTestButton.AccessibleName = "Info Tool";
        toolbarToolTip.SetToolTip(hitTestButton, "Info Tool");
        hitTestButton.Appearance = Appearance.Button;
        hitTestButton.AutoSize = false;
        hitTestButton.Height = 28;
        hitTestButton.Margin = new Padding(0, 3, 4, 3);
        hitTestButton.Padding = new Padding(0);
        hitTestButton.TextAlign = ContentAlignment.MiddleCenter;
        hitTestButton.UseVisualStyleBackColor = false;
        hitTestButton.Size = new Size(36, 36);
        hitTestButton.Text = "";
        hitTestButton.Click += hitTestButton_Click;
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
        toolStateLabel.Size = new Size(210, 22);
        toolStateLabel.Text = "Tool: Info | API: HitTestTopFeatureAt";
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
        Text = "InfoTool";
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
