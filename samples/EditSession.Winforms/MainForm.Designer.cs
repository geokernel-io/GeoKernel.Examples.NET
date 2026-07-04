namespace GeoKernel.EditSession.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private FlowLayoutPanel toolbarPanel;
    private Button beginEditButton;
    private Button addFeatureButton;
    private Panel editSeparator;
    private Button commitEditButton;
    private Button rollbackEditButton;
    private Panel navigationSeparator;
    private Button fullExtentButton;
    private Label editStateLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;

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
        toolbarPanel = new FlowLayoutPanel();
        beginEditButton = new Button();
        addFeatureButton = new Button();
        editSeparator = new Panel();
        commitEditButton = new Button();
        rollbackEditButton = new Button();
        navigationSeparator = new Panel();
        fullExtentButton = new Button();
        editStateLabel = new Label();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
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
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
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
        toolbarPanel.Controls.AddRange(new Control[] { beginEditButton, addFeatureButton, editSeparator, commitEditButton, rollbackEditButton, navigationSeparator, fullExtentButton, editStateLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // beginEditButton
        // 
        beginEditButton.Name = "beginEditButton";
        beginEditButton.AutoSize = true;
        beginEditButton.Height = 28;
        beginEditButton.Margin = new Padding(0, 3, 4, 3);
        beginEditButton.Padding = new Padding(8, 0, 8, 0);
        beginEditButton.UseVisualStyleBackColor = true;
        beginEditButton.Size = new Size(64, 29);
        beginEditButton.Text = "Begin Edit";
        beginEditButton.Click += beginEditButton_Click;
        // 
        // addFeatureButton
        // 
        addFeatureButton.Enabled = false;
        addFeatureButton.Name = "addFeatureButton";
        addFeatureButton.AutoSize = true;
        addFeatureButton.Height = 28;
        addFeatureButton.Margin = new Padding(0, 3, 4, 3);
        addFeatureButton.Padding = new Padding(8, 0, 8, 0);
        addFeatureButton.UseVisualStyleBackColor = true;
        addFeatureButton.Size = new Size(75, 29);
        addFeatureButton.Text = "Add Feature";
        addFeatureButton.Click += addFeatureButton_Click;
        // 
        // editSeparator
        // 
        editSeparator.BackColor = SystemColors.ControlDark;
        editSeparator.Margin = new Padding(4, 5, 6, 5);
        editSeparator.Name = "editSeparator";
        editSeparator.Size = new Size(1, 26);
        // 
        // commitEditButton
        // 
        commitEditButton.Enabled = false;
        commitEditButton.Name = "commitEditButton";
        commitEditButton.AutoSize = true;
        commitEditButton.Height = 28;
        commitEditButton.Margin = new Padding(0, 3, 4, 3);
        commitEditButton.Padding = new Padding(8, 0, 8, 0);
        commitEditButton.UseVisualStyleBackColor = true;
        commitEditButton.Size = new Size(78, 29);
        commitEditButton.Text = "Commit Edit";
        commitEditButton.Click += commitEditButton_Click;
        // 
        // rollbackEditButton
        // 
        rollbackEditButton.Enabled = false;
        rollbackEditButton.Name = "rollbackEditButton";
        rollbackEditButton.AutoSize = true;
        rollbackEditButton.Height = 28;
        rollbackEditButton.Margin = new Padding(0, 3, 4, 3);
        rollbackEditButton.Padding = new Padding(8, 0, 8, 0);
        rollbackEditButton.UseVisualStyleBackColor = true;
        rollbackEditButton.Size = new Size(79, 29);
        rollbackEditButton.Text = "Rollback Edit";
        rollbackEditButton.Click += rollbackEditButton_Click;
        // 
        // navigationSeparator
        // 
        navigationSeparator.BackColor = SystemColors.ControlDark;
        navigationSeparator.Margin = new Padding(4, 5, 6, 5);
        navigationSeparator.Name = "navigationSeparator";
        navigationSeparator.Size = new Size(1, 26);
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.AutoSize = true;
        fullExtentButton.Height = 28;
        fullExtentButton.Margin = new Padding(0, 3, 4, 3);
        fullExtentButton.Padding = new Padding(8, 0, 8, 0);
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Size = new Size(66, 29);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // editStateLabel
        // 
        editStateLabel.Margin = new Padding(12, 1, 0, 2);
        editStateLabel.Name = "editStateLabel";
        editStateLabel.AutoSize = true;
        editStateLabel.Margin = new Padding(8, 9, 8, 0);
        editStateLabel.TextAlign = ContentAlignment.MiddleLeft;
        editStateLabel.Size = new Size(165, 29);
        editStateLabel.Text = "Editing: OFF | Feature count: 0";
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 32);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(1200, 744);
        geoKernelViewerControl.TabIndex = 1;
        // 
        // statusStrip
        // 
        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
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
        Text = "EditSession";
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
