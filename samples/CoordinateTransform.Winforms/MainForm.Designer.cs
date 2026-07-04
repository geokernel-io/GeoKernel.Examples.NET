namespace GeoKernel.CoordinateTransform.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayoutPanel;
    private Panel toolbarPanel;
    private Button fullExtentButton;
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
        rootLayoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        fullExtentButton = new Button();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        rootLayoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayoutPanel
        // 
        rootLayoutPanel.ColumnCount = 1;
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayoutPanel.Controls.Add(toolbarPanel, 0, 0);
        rootLayoutPanel.Controls.Add(geoKernelViewerControl, 0, 1);
        rootLayoutPanel.Controls.Add(statusStrip, 0, 2);
        rootLayoutPanel.Dock = DockStyle.Fill;
        rootLayoutPanel.Location = new Point(0, 0);
        rootLayoutPanel.Margin = new Padding(0);
        rootLayoutPanel.Name = "rootLayoutPanel";
        rootLayoutPanel.RowCount = 3;
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        rootLayoutPanel.Size = new Size(1200, 800);
        rootLayoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1200, 35);
        toolbarPanel.TabIndex = 0;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Location = new Point(4, 4);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(88, 27);
        fullExtentButton.TabIndex = 0;
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 35);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(1200, 741);
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
        statusLabel.Size = new Size(134, 19);
        statusLabel.Text = "EPSG:4326 -> EPSG:3857";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 800);
        Controls.Add(rootLayoutPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CoordinateTransform";
        Shown += MainForm_Shown;
        rootLayoutPanel.ResumeLayout(false);
        rootLayoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
