namespace GeoKernel.ScaleBasedLayerVisibility.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private SplitContainer splitContainer;
    private TableLayoutPanel sidePanelLayout;
    private Label scaleLabel;
    private Label rangeHeaderLabel;
    private ListBox layerListBox;
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
        splitContainer = new SplitContainer();
        sidePanelLayout = new TableLayoutPanel();
        scaleLabel = new Label();
        rangeHeaderLabel = new Label();
        layerListBox = new ListBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        layoutPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        sidePanelLayout.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // layoutPanel
        // 
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(splitContainer, 0, 0);
        layoutPanel.Controls.Add(statusStrip, 0, 1);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.Location = new Point(0, 0);
        layoutPanel.Margin = new Padding(0);
        layoutPanel.Name = "layoutPanel";
        layoutPanel.RowCount = 2;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel1;
        splitContainer.Location = new Point(0, 0);
        splitContainer.Margin = new Padding(0);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(sidePanelLayout);
        splitContainer.Panel1MinSize = 240;
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(geoKernelViewerControl);
        splitContainer.Size = new Size(1200, 776);
        splitContainer.SplitterDistance = 280;
        splitContainer.TabIndex = 0;
        // 
        // sidePanelLayout
        // 
        sidePanelLayout.ColumnCount = 1;
        sidePanelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sidePanelLayout.Controls.Add(scaleLabel, 0, 0);
        sidePanelLayout.Controls.Add(rangeHeaderLabel, 0, 1);
        sidePanelLayout.Controls.Add(layerListBox, 0, 2);
        sidePanelLayout.Dock = DockStyle.Fill;
        sidePanelLayout.Location = new Point(0, 0);
        sidePanelLayout.Name = "sidePanelLayout";
        sidePanelLayout.Padding = new Padding(8);
        sidePanelLayout.RowCount = 3;
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        sidePanelLayout.Size = new Size(280, 776);
        sidePanelLayout.TabIndex = 0;
        // 
        // scaleLabel
        // 
        scaleLabel.AutoSize = true;
        scaleLabel.Dock = DockStyle.Fill;
        scaleLabel.Location = new Point(8, 8);
        scaleLabel.Margin = new Padding(0);
        scaleLabel.Name = "scaleLabel";
        scaleLabel.Size = new Size(264, 28);
        scaleLabel.TabIndex = 0;
        scaleLabel.Text = "Current scale: -";
        scaleLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // rangeHeaderLabel
        // 
        rangeHeaderLabel.AutoSize = true;
        rangeHeaderLabel.Dock = DockStyle.Fill;
        rangeHeaderLabel.Location = new Point(8, 36);
        rangeHeaderLabel.Margin = new Padding(0);
        rangeHeaderLabel.Name = "rangeHeaderLabel";
        rangeHeaderLabel.Size = new Size(264, 28);
        rangeHeaderLabel.TabIndex = 1;
        rangeHeaderLabel.Text = "Visible scale ranges: [min - max]";
        rangeHeaderLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // layerListBox
        // 
        layerListBox.Dock = DockStyle.Fill;
        layerListBox.Font = new Font("Consolas", 9F);
        layerListBox.FormattingEnabled = true;
        layerListBox.Location = new Point(8, 72);
        layerListBox.Margin = new Padding(0, 8, 0, 0);
        layerListBox.Name = "layerListBox";
        layerListBox.Size = new Size(264, 696);
        layerListBox.TabIndex = 2;
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
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 776);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1200, 24);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 1;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(375, 19);
        statusLabel.Text = "Zoom in/out: World, States and Cities appear at different scale ranges.";
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
        Text = "ScaleBasedLayerVisibility";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        sidePanelLayout.ResumeLayout(false);
        sidePanelLayout.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
