using GeoKernel.NET.WinForms;

namespace GeoKernel.EditDirtyState.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private Button beginEditButton;
    private Button addFeatureButton;
    private Button commitEditButton;
    private Button rollbackEditButton;
    private Button fullExtentButton;
    private Label editStateLabel;
    private SplitContainer splitContainer;
    private GeoKernelViewerControl geoKernelViewerControl;
    private TextBox logTextBox;
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
        beginEditButton = new Button();
        addFeatureButton = new Button();
        commitEditButton = new Button();
        rollbackEditButton = new Button();
        fullExtentButton = new Button();
        editStateLabel = new Label();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernelViewerControl();
        logTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
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
        toolbarPanel.Controls.AddRange(new Control[] { beginEditButton, addFeatureButton, commitEditButton, rollbackEditButton, fullExtentButton, editStateLabel });
        toolbarPanel.Size = new Size(980, 32);
        toolbarPanel.TabIndex = 0;
        // 
        // beginEditButton
        // 
        beginEditButton.Name = "beginEditButton";
        beginEditButton.AutoSize = true;
        beginEditButton.Height = 28;
        beginEditButton.Margin = new Padding(0, 2, 4, 2);
        beginEditButton.Padding = new Padding(8, 0, 8, 0);
        beginEditButton.UseVisualStyleBackColor = true;
        beginEditButton.Size = new Size(64, 22);
        beginEditButton.Text = "Begin Edit";
        beginEditButton.Click += beginEditButton_Click;
        // 
        // addFeatureButton
        // 
        addFeatureButton.Enabled = false;
        addFeatureButton.Name = "addFeatureButton";
        addFeatureButton.AutoSize = true;
        addFeatureButton.Height = 28;
        addFeatureButton.Margin = new Padding(0, 2, 4, 2);
        addFeatureButton.Padding = new Padding(8, 0, 8, 0);
        addFeatureButton.UseVisualStyleBackColor = true;
        addFeatureButton.Size = new Size(75, 22);
        addFeatureButton.Text = "Add Feature";
        addFeatureButton.Click += addFeatureButton_Click;
        // 
        // commitEditButton
        // 
        commitEditButton.Enabled = false;
        commitEditButton.Name = "commitEditButton";
        commitEditButton.AutoSize = true;
        commitEditButton.Height = 28;
        commitEditButton.Margin = new Padding(0, 2, 4, 2);
        commitEditButton.Padding = new Padding(8, 0, 8, 0);
        commitEditButton.UseVisualStyleBackColor = true;
        commitEditButton.Size = new Size(78, 22);
        commitEditButton.Text = "Commit Edit";
        commitEditButton.Click += commitEditButton_Click;
        // 
        // rollbackEditButton
        // 
        rollbackEditButton.Enabled = false;
        rollbackEditButton.Name = "rollbackEditButton";
        rollbackEditButton.AutoSize = true;
        rollbackEditButton.Height = 28;
        rollbackEditButton.Margin = new Padding(0, 2, 4, 2);
        rollbackEditButton.Padding = new Padding(8, 0, 8, 0);
        rollbackEditButton.UseVisualStyleBackColor = true;
        rollbackEditButton.Size = new Size(82, 22);
        rollbackEditButton.Text = "Rollback Edit";
        rollbackEditButton.Click += rollbackEditButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.AutoSize = true;
        fullExtentButton.Height = 28;
        fullExtentButton.Margin = new Padding(0, 2, 4, 2);
        fullExtentButton.Padding = new Padding(8, 0, 8, 0);
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Size = new Size(66, 22);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // editStateLabel
        // 
        editStateLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        editStateLabel.Margin = new Padding(12, 1, 0, 2);
        editStateLabel.Name = "editStateLabel";
        editStateLabel.AutoSize = true;
        editStateLabel.Margin = new Padding(8, 8, 8, 0);
        editStateLabel.TextAlign = ContentAlignment.MiddleLeft;
        editStateLabel.Size = new Size(203, 22);
        editStateLabel.Text = "Editing: OFF | Dirty: NO | Signals: 0";
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.Location = new Point(0, 25);
        splitContainer.Name = "splitContainer";
        splitContainer.Orientation = Orientation.Horizontal;
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(logTextBox);
        splitContainer.Size = new Size(1184, 714);
        splitContainer.SplitterDistance = 555;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(1184, 555);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // logTextBox
        // 
        logTextBox.Dock = DockStyle.Fill;
        logTextBox.Location = new Point(0, 0);
        logTextBox.Multiline = true;
        logTextBox.Name = "logTextBox";
        logTextBox.ReadOnly = true;
        logTextBox.ScrollBars = ScrollBars.Vertical;
        logTextBox.Size = new Size(1184, 155);
        logTextBox.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar });
        downloadProgressBar.Size = new Size(180, 18);
        downloadProgressBar.Visible = false;
        statusStrip.Location = new Point(0, 739);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1184, 22);
        statusStrip.TabIndex = 2;
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
        MinimumSize = new Size(900, 600);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EditDirtyState";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        splitContainer.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
