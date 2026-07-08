namespace GeoKernel.ClassificationMethods.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private SplitContainer splitContainer;
    private Panel controlPanel;
    private Label methodLabel;
    private ComboBox methodComboBox;
    private ListView legendListView;
    private ColumnHeader classColumn;
    private ImageList legendImageList;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        splitContainer = new SplitContainer();
        legendListView = new ListView();
        classColumn = new ColumnHeader();
        legendImageList = new ImageList(components);
        controlPanel = new Panel();
        methodComboBox = new ComboBox();
        methodLabel = new Label();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        controlPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
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
        splitContainer.Panel1.Controls.Add(legendListView);
        splitContainer.Panel1.Controls.Add(controlPanel);
        splitContainer.Panel1MinSize = 210;
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(geoKernelViewerControl);
        splitContainer.Size = new Size(1200, 778);
        splitContainer.SplitterDistance = 245;
        splitContainer.SplitterWidth = 1;
        splitContainer.TabIndex = 0;
        // 
        // legendListView
        // 
        legendListView.Columns.AddRange(new ColumnHeader[] { classColumn });
        legendListView.Dock = DockStyle.Fill;
        legendListView.FullRowSelect = true;
        legendListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        legendListView.Location = new Point(0, 64);
        legendListView.MultiSelect = false;
        legendListView.Name = "legendListView";
        legendListView.Size = new Size(245, 714);
        legendListView.SmallImageList = legendImageList;
        legendListView.TabIndex = 1;
        legendListView.UseCompatibleStateImageBehavior = false;
        legendListView.View = View.Details;
        legendListView.Resize += legendListView_Resize;
        // 
        // classColumn
        // 
        classColumn.Text = "POPULATION - Equal Interval";
        classColumn.Width = 235;
        // 
        // legendImageList
        // 
        legendImageList.ColorDepth = ColorDepth.Depth32Bit;
        legendImageList.ImageSize = new Size(42, 24);
        legendImageList.TransparentColor = Color.Transparent;
        // 
        // controlPanel
        // 
        controlPanel.Controls.Add(methodComboBox);
        controlPanel.Controls.Add(methodLabel);
        controlPanel.Dock = DockStyle.Top;
        controlPanel.Location = new Point(0, 0);
        controlPanel.Name = "controlPanel";
        controlPanel.Padding = new Padding(8, 8, 8, 6);
        controlPanel.Size = new Size(245, 64);
        controlPanel.TabIndex = 0;
        // 
        // methodComboBox
        // 
        methodComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        methodComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        methodComboBox.FormattingEnabled = true;
        methodComboBox.Location = new Point(8, 28);
        methodComboBox.Name = "methodComboBox";
        methodComboBox.Size = new Size(229, 23);
        methodComboBox.TabIndex = 1;
        methodComboBox.SelectedIndexChanged += methodComboBox_SelectedIndexChanged;
        // 
        // methodLabel
        // 
        methodLabel.AutoSize = true;
        methodLabel.Location = new Point(8, 8);
        methodLabel.Name = "methodLabel";
        methodLabel.Size = new Size(49, 15);
        methodLabel.TabIndex = 0;
        methodLabel.Text = "Method";
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
        statusStrip.TabIndex = 1;
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
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ClassificationMethods";
        Shown += MainForm_Shown;
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        controlPanel.ResumeLayout(false);
        controlPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
