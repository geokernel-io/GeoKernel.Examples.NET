namespace GeoKernel.LayerZoomTo.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = new System.ComponentModel.Container();
    private TableLayoutPanel layoutPanel;
    private Panel topPanel;
    private ComboBox cityComboBox;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        layoutPanel = new TableLayoutPanel();
        topPanel = new Panel();
        cityComboBox = new ComboBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        layoutPanel.SuspendLayout();
        topPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // layoutPanel
        // 
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(topPanel, 0, 0);
        layoutPanel.Controls.Add(statusStrip, 0, 2);
        layoutPanel.Controls.Add(geoKernelViewerControl, 0, 1);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.Location = new Point(0, 0);
        layoutPanel.Margin = new Padding(0);
        layoutPanel.Name = "layoutPanel";
        layoutPanel.RowCount = 3;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
        // 
        // topPanel
        // 
        topPanel.Controls.Add(cityComboBox);
        topPanel.Dock = DockStyle.Fill;
        topPanel.Location = new Point(0, 0);
        topPanel.Margin = new Padding(0);
        topPanel.Name = "topPanel";
        topPanel.Padding = new Padding(6, 4, 6, 4);
        topPanel.Size = new Size(1200, 34);
        topPanel.TabIndex = 0;
        // 
        // cityComboBox
        // 
        cityComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        cityComboBox.FormattingEnabled = true;
        cityComboBox.Location = new Point(6, 5);
        cityComboBox.Name = "cityComboBox";
        cityComboBox.Size = new Size(240, 23);
        cityComboBox.TabIndex = 0;
        cityComboBox.SelectedIndexChanged += cityComboBox_SelectedIndexChanged;
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
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 778);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1200, 22);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(39, 17);
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
        Text = "Example: Layer ZoomTo";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        topPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
