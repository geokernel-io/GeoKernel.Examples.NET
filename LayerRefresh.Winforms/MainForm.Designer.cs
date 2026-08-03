namespace GeoKernel.LayerRefresh.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button changeFillButton;
    private Button changeOutlineButton;
    private Button changeOpacityButton;
    private Panel refreshSeparator;
    private Button refreshLayerButton;
    private Button fullExtentButton;
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
        changeFillButton = new Button();
        changeOutlineButton = new Button();
        changeOpacityButton = new Button();
        refreshSeparator = new Panel();
        refreshLayerButton = new Button();
        fullExtentButton = new Button();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
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
        layoutPanel.Controls.Add(statusStrip, 0, 2);
        layoutPanel.Controls.Add(geoKernelViewerControl, 0, 1);
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
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(refreshLayerButton);
        toolbarPanel.Controls.Add(refreshSeparator);
        toolbarPanel.Controls.Add(changeOpacityButton);
        toolbarPanel.Controls.Add(changeOutlineButton);
        toolbarPanel.Controls.Add(changeFillButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Size = new Size(1200, 32);
        toolbarPanel.TabIndex = 0;
        // 
        // changeFillButton
        // 
        changeFillButton.BackColor = SystemColors.Control;
        changeFillButton.FlatAppearance.BorderSize = 0;
        changeFillButton.FlatStyle = FlatStyle.Flat;
        changeFillButton.Location = new Point(0, 0);
        changeFillButton.Margin = new Padding(0);
        changeFillButton.Name = "changeFillButton";
        changeFillButton.Size = new Size(92, 32);
        changeFillButton.TabIndex = 0;
        changeFillButton.Text = "Change Fill";
        changeFillButton.UseVisualStyleBackColor = false;
        changeFillButton.Click += changeFillButton_Click;
        // 
        // changeOutlineButton
        // 
        changeOutlineButton.BackColor = SystemColors.Control;
        changeOutlineButton.FlatAppearance.BorderSize = 0;
        changeOutlineButton.FlatStyle = FlatStyle.Flat;
        changeOutlineButton.Location = new Point(92, 0);
        changeOutlineButton.Margin = new Padding(0);
        changeOutlineButton.Name = "changeOutlineButton";
        changeOutlineButton.Size = new Size(116, 32);
        changeOutlineButton.TabIndex = 1;
        changeOutlineButton.Text = "Change Outline";
        changeOutlineButton.UseVisualStyleBackColor = false;
        changeOutlineButton.Click += changeOutlineButton_Click;
        // 
        // changeOpacityButton
        // 
        changeOpacityButton.BackColor = SystemColors.Control;
        changeOpacityButton.FlatAppearance.BorderSize = 0;
        changeOpacityButton.FlatStyle = FlatStyle.Flat;
        changeOpacityButton.Location = new Point(208, 0);
        changeOpacityButton.Margin = new Padding(0);
        changeOpacityButton.Name = "changeOpacityButton";
        changeOpacityButton.Size = new Size(120, 32);
        changeOpacityButton.TabIndex = 2;
        changeOpacityButton.Text = "Change Opacity";
        changeOpacityButton.UseVisualStyleBackColor = false;
        changeOpacityButton.Click += changeOpacityButton_Click;
        // 
        // refreshSeparator
        // 
        refreshSeparator.BackColor = SystemColors.ControlDark;
        refreshSeparator.Location = new Point(334, 4);
        refreshSeparator.Margin = new Padding(0);
        refreshSeparator.Name = "refreshSeparator";
        refreshSeparator.Size = new Size(1, 24);
        refreshSeparator.TabIndex = 3;
        // 
        // refreshLayerButton
        // 
        refreshLayerButton.BackColor = SystemColors.Control;
        refreshLayerButton.FlatAppearance.BorderSize = 0;
        refreshLayerButton.FlatStyle = FlatStyle.Flat;
        refreshLayerButton.Location = new Point(340, 0);
        refreshLayerButton.Margin = new Padding(0);
        refreshLayerButton.Name = "refreshLayerButton";
        refreshLayerButton.Size = new Size(106, 32);
        refreshLayerButton.TabIndex = 4;
        refreshLayerButton.Text = "Refresh Layer";
        refreshLayerButton.UseVisualStyleBackColor = false;
        refreshLayerButton.Click += refreshLayerButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.BackColor = SystemColors.Control;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.Location = new Point(446, 0);
        fullExtentButton.Margin = new Padding(0);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(86, 32);
        fullExtentButton.TabIndex = 5;
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.UseVisualStyleBackColor = false;
        fullExtentButton.Visible = false;
        fullExtentButton.Click += fullExtentButton_Click;
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
        statusLabel.Text = "Ready";
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
        Text = "LayerRefresh";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
