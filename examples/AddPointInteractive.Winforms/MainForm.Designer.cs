namespace GeoKernel.AddPointInteractive.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button fullExtentButton;
    private Panel navigationSeparator;
    private Button addPointButton;
    private Button panButton;
    private Panel editSeparator;
    private Button clearPointsButton;
    private Label pointCountLabel;
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
        toolbarPanel = new Panel();
        fullExtentButton = new Button();
        navigationSeparator = new Panel();
        addPointButton = new Button();
        panButton = new Button();
        editSeparator = new Panel();
        clearPointsButton = new Button();
        pointCountLabel = new Label();
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
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(pointCountLabel);
        toolbarPanel.Controls.Add(clearPointsButton);
        toolbarPanel.Controls.Add(editSeparator);
        toolbarPanel.Controls.Add(panButton);
        toolbarPanel.Controls.Add(addPointButton);
        toolbarPanel.Controls.Add(navigationSeparator);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Size = new Size(1200, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // fullExtentButton
        // 
        fullExtentButton.BackColor = SystemColors.Control;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.Location = new Point(0, 0);
        fullExtentButton.Margin = new Padding(0);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(82, 36);
        fullExtentButton.TabIndex = 0;
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.UseVisualStyleBackColor = false;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // navigationSeparator
        // 
        navigationSeparator.BackColor = SystemColors.ControlDark;
        navigationSeparator.Location = new Point(86, 4);
        navigationSeparator.Margin = new Padding(0);
        navigationSeparator.Name = "navigationSeparator";
        navigationSeparator.Size = new Size(1, 28);
        navigationSeparator.TabIndex = 1;
        // 
        // addPointButton
        // 
        addPointButton.BackColor = Color.FromArgb(210, 232, 255);
        addPointButton.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
        addPointButton.FlatAppearance.BorderSize = 1;
        addPointButton.FlatStyle = FlatStyle.Flat;
        addPointButton.Location = new Point(91, 0);
        addPointButton.Margin = new Padding(0);
        addPointButton.Name = "addPointButton";
        addPointButton.Size = new Size(82, 36);
        addPointButton.TabIndex = 2;
        addPointButton.Text = "Add Point";
        addPointButton.UseVisualStyleBackColor = false;
        addPointButton.Click += addPointButton_Click;
        // 
        // panButton
        // 
        panButton.BackColor = SystemColors.Control;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.Location = new Point(173, 0);
        panButton.Margin = new Padding(0);
        panButton.Name = "panButton";
        panButton.Size = new Size(48, 36);
        panButton.TabIndex = 3;
        panButton.Text = "Pan";
        panButton.UseVisualStyleBackColor = false;
        panButton.Click += panButton_Click;
        // 
        // editSeparator
        // 
        editSeparator.BackColor = SystemColors.ControlDark;
        editSeparator.Location = new Point(225, 4);
        editSeparator.Margin = new Padding(0);
        editSeparator.Name = "editSeparator";
        editSeparator.Size = new Size(1, 28);
        editSeparator.TabIndex = 4;
        // 
        // clearPointsButton
        // 
        clearPointsButton.BackColor = SystemColors.Control;
        clearPointsButton.FlatAppearance.BorderSize = 0;
        clearPointsButton.FlatStyle = FlatStyle.Flat;
        clearPointsButton.Location = new Point(230, 0);
        clearPointsButton.Margin = new Padding(0);
        clearPointsButton.Name = "clearPointsButton";
        clearPointsButton.Size = new Size(90, 36);
        clearPointsButton.TabIndex = 5;
        clearPointsButton.Text = "Clear Points";
        clearPointsButton.UseVisualStyleBackColor = false;
        clearPointsButton.Click += clearPointsButton_Click;
        // 
        // pointCountLabel
        // 
        pointCountLabel.AutoSize = false;
        pointCountLabel.Location = new Point(332, 0);
        pointCountLabel.Margin = new Padding(0);
        pointCountLabel.Name = "pointCountLabel";
        pointCountLabel.Size = new Size(150, 36);
        pointCountLabel.TabIndex = 6;
        pointCountLabel.Text = "Point count: 0";
        pointCountLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 39);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(1200, 737);
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
        Text = "AddPointInteractive";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        toolbarPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
