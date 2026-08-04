namespace GeoKernel.StylePerFeature.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private SplitContainer splitContainer;
    private TableLayoutPanel sidePanelLayout;
    private Label featureListLabel;
    private ListView featureListView;
    private ColumnHeader featureColumn;
    private ImageList featureImageList;
    private GroupBox selectedFeatureGroupBox;
    private TableLayoutPanel selectedFeatureLayout;
    private Label zoneLabel;
    private ComboBox zoneComboBox;
    private Button applyButton;
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
        sidePanelLayout = new TableLayoutPanel();
        featureListLabel = new Label();
        featureListView = new ListView();
        featureColumn = new ColumnHeader();
        featureImageList = new ImageList(components);
        selectedFeatureGroupBox = new GroupBox();
        selectedFeatureLayout = new TableLayoutPanel();
        zoneLabel = new Label();
        zoneComboBox = new ComboBox();
        applyButton = new Button();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        sidePanelLayout.SuspendLayout();
        selectedFeatureGroupBox.SuspendLayout();
        selectedFeatureLayout.SuspendLayout();
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
        splitContainer.Panel1.Controls.Add(sidePanelLayout);
        splitContainer.Panel1MinSize = 230;
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(geoKernelViewerControl);
        splitContainer.Size = new Size(1100, 738);
        splitContainer.SplitterDistance = 255;
        splitContainer.SplitterWidth = 1;
        splitContainer.TabIndex = 0;
        // 
        // sidePanelLayout
        // 
        sidePanelLayout.ColumnCount = 1;
        sidePanelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sidePanelLayout.Controls.Add(featureListLabel, 0, 0);
        sidePanelLayout.Controls.Add(featureListView, 0, 1);
        sidePanelLayout.Controls.Add(selectedFeatureGroupBox, 0, 2);
        sidePanelLayout.Dock = DockStyle.Fill;
        sidePanelLayout.Location = new Point(0, 0);
        sidePanelLayout.Margin = new Padding(0);
        sidePanelLayout.Name = "sidePanelLayout";
        sidePanelLayout.Padding = new Padding(8);
        sidePanelLayout.RowCount = 4;
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 132F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 4F));
        sidePanelLayout.Size = new Size(255, 738);
        sidePanelLayout.TabIndex = 0;
        // 
        // featureListLabel
        // 
        featureListLabel.Dock = DockStyle.Fill;
        featureListLabel.Location = new Point(8, 8);
        featureListLabel.Margin = new Padding(0);
        featureListLabel.Name = "featureListLabel";
        featureListLabel.Size = new Size(239, 24);
        featureListLabel.TabIndex = 0;
        featureListLabel.Text = "Feature attributes";
        featureListLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // featureListView
        // 
        featureListView.Columns.AddRange(new ColumnHeader[] { featureColumn });
        featureListView.Dock = DockStyle.Fill;
        featureListView.FullRowSelect = true;
        featureListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        featureListView.Location = new Point(8, 32);
        featureListView.Margin = new Padding(0, 0, 0, 8);
        featureListView.MultiSelect = false;
        featureListView.Name = "featureListView";
        featureListView.Size = new Size(239, 554);
        featureListView.SmallImageList = featureImageList;
        featureListView.TabIndex = 1;
        featureListView.UseCompatibleStateImageBehavior = false;
        featureListView.View = View.Details;
        featureListView.SelectedIndexChanged += featureListView_SelectedIndexChanged;
        featureListView.Resize += featureListView_Resize;
        // 
        // featureColumn
        // 
        featureColumn.Text = "Parcel - zone";
        featureColumn.Width = 230;
        // 
        // featureImageList
        // 
        featureImageList.ColorDepth = ColorDepth.Depth32Bit;
        featureImageList.ImageSize = new Size(46, 22);
        featureImageList.TransparentColor = Color.Transparent;
        // 
        // selectedFeatureGroupBox
        // 
        selectedFeatureGroupBox.Controls.Add(selectedFeatureLayout);
        selectedFeatureGroupBox.Dock = DockStyle.Fill;
        selectedFeatureGroupBox.Location = new Point(8, 594);
        selectedFeatureGroupBox.Margin = new Padding(0);
        selectedFeatureGroupBox.Name = "selectedFeatureGroupBox";
        selectedFeatureGroupBox.Padding = new Padding(8);
        selectedFeatureGroupBox.Size = new Size(239, 132);
        selectedFeatureGroupBox.TabIndex = 2;
        selectedFeatureGroupBox.TabStop = false;
        selectedFeatureGroupBox.Text = "Selected Feature";
        // 
        // selectedFeatureLayout
        // 
        selectedFeatureLayout.ColumnCount = 1;
        selectedFeatureLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        selectedFeatureLayout.Controls.Add(zoneLabel, 0, 0);
        selectedFeatureLayout.Controls.Add(zoneComboBox, 0, 1);
        selectedFeatureLayout.Controls.Add(applyButton, 0, 2);
        selectedFeatureLayout.Dock = DockStyle.Fill;
        selectedFeatureLayout.Location = new Point(8, 24);
        selectedFeatureLayout.Margin = new Padding(0);
        selectedFeatureLayout.Name = "selectedFeatureLayout";
        selectedFeatureLayout.RowCount = 3;
        selectedFeatureLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        selectedFeatureLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        selectedFeatureLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        selectedFeatureLayout.Size = new Size(223, 100);
        selectedFeatureLayout.TabIndex = 0;
        // 
        // zoneLabel
        // 
        zoneLabel.Dock = DockStyle.Fill;
        zoneLabel.Location = new Point(0, 0);
        zoneLabel.Margin = new Padding(0);
        zoneLabel.Name = "zoneLabel";
        zoneLabel.Size = new Size(223, 24);
        zoneLabel.TabIndex = 0;
        zoneLabel.Text = "Zone attribute";
        zoneLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // zoneComboBox
        // 
        zoneComboBox.Dock = DockStyle.Fill;
        zoneComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        zoneComboBox.FormattingEnabled = true;
        zoneComboBox.Location = new Point(0, 24);
        zoneComboBox.Margin = new Padding(0, 0, 0, 8);
        zoneComboBox.Name = "zoneComboBox";
        zoneComboBox.Size = new Size(223, 23);
        zoneComboBox.TabIndex = 1;
        // 
        // applyButton
        // 
        applyButton.Dock = DockStyle.Fill;
        applyButton.Location = new Point(0, 56);
        applyButton.Margin = new Padding(0);
        applyButton.Name = "applyButton";
        applyButton.Size = new Size(223, 44);
        applyButton.TabIndex = 2;
        applyButton.Text = "Apply Feature Style";
        applyButton.UseVisualStyleBackColor = true;
        applyButton.Click += applyButton_Click;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(844, 738);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 738);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1100, 22);
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
        ClientSize = new Size(1100, 760);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "StylePerFeature";
        Shown += MainForm_Shown;
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        sidePanelLayout.ResumeLayout(false);
        selectedFeatureGroupBox.ResumeLayout(false);
        selectedFeatureLayout.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
