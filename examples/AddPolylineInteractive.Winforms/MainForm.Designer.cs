namespace GeoKernel.AddPolylineInteractive.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button fullExtentButton;
    private Panel navigationSeparator;
    private Button addPolylineButton;
    private Button panButton;
    private Panel editSeparator;
    private Button clearLinesButton;
    private Label polylineCountLabel;
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
        addPolylineButton = new Button();
        panButton = new Button();
        editSeparator = new Panel();
        clearLinesButton = new Button();
        polylineCountLabel = new Label();
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
        toolbarPanel.BackColor = Color.FromArgb(242, 242, 242);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(navigationSeparator);
        toolbarPanel.Controls.Add(addPolylineButton);
        toolbarPanel.Controls.Add(panButton);
        toolbarPanel.Controls.Add(editSeparator);
        toolbarPanel.Controls.Add(clearLinesButton);
        toolbarPanel.Controls.Add(polylineCountLabel);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1200, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // fullExtentButton
        // 
        fullExtentButton.BackgroundImage = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.BackgroundImageLayout = ImageLayout.Center;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.Location = new Point(3, 2);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(36, 36);
        fullExtentButton.TabIndex = 0;
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // navigationSeparator
        // 
        navigationSeparator.BackColor = Color.FromArgb(180, 180, 180);
        navigationSeparator.Location = new Point(45, 7);
        navigationSeparator.Name = "navigationSeparator";
        navigationSeparator.Size = new Size(1, 25);
        navigationSeparator.TabIndex = 1;
        // 
        // addPolylineButton
        // 
        addPolylineButton.BackgroundImage = (Image)resources.GetObject("addPolylineButton.Image");
        addPolylineButton.BackgroundImageLayout = ImageLayout.Center;
        addPolylineButton.FlatStyle = FlatStyle.Flat;
        addPolylineButton.Location = new Point(52, 2);
        addPolylineButton.Name = "addPolylineButton";
        addPolylineButton.Size = new Size(36, 36);
        addPolylineButton.TabIndex = 2;
        addPolylineButton.Text = "Add Polyline";
        addPolylineButton.UseVisualStyleBackColor = true;
        addPolylineButton.Click += addPolylineButton_Click;
        // 
        // panButton
        // 
        panButton.BackgroundImage = (Image)resources.GetObject("panButton.Image");
        panButton.BackgroundImageLayout = ImageLayout.Center;
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.Location = new Point(94, 2);
        panButton.Name = "panButton";
        panButton.Size = new Size(36, 36);
        panButton.TabIndex = 3;
        panButton.Text = "Pan";
        panButton.UseVisualStyleBackColor = true;
        panButton.Click += panButton_Click;
        // 
        // editSeparator
        // 
        editSeparator.BackColor = Color.FromArgb(180, 180, 180);
        editSeparator.Location = new Point(136, 7);
        editSeparator.Name = "editSeparator";
        editSeparator.Size = new Size(1, 25);
        editSeparator.TabIndex = 4;
        // 
        // clearLinesButton
        // 
        clearLinesButton.FlatStyle = FlatStyle.Flat;
        clearLinesButton.FlatAppearance.BorderSize = 0;
        clearLinesButton.Location = new Point(144, 5);
        clearLinesButton.Name = "clearLinesButton";
        clearLinesButton.Size = new Size(88, 28);
        clearLinesButton.TabIndex = 5;
        clearLinesButton.Text = "Clear Lines";
        clearLinesButton.UseVisualStyleBackColor = true;
        clearLinesButton.Click += clearLinesButton_Click;
        // 
        // polylineCountLabel
        // 
        polylineCountLabel.AutoSize = true;
        polylineCountLabel.Location = new Point(244, 12);
        polylineCountLabel.Name = "polylineCountLabel";
        polylineCountLabel.Size = new Size(94, 15);
        polylineCountLabel.TabIndex = 6;
        polylineCountLabel.Text = "Polyline count: 0";
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
        Text = "AddPolylineInteractive";
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
