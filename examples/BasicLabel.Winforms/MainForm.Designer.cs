namespace GeoKernel.BasicLabel.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private SplitContainer splitContainer;
    private TableLayoutPanel sidePanelLayout;
    private Label titleLabel;
    private CheckBox showLabelsCheckBox;
    private Label fieldLabel;
    private ComboBox fieldComboBox;
    private Label fontSizeLabel;
    private NumericUpDown fontSizeNumeric;
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
        splitContainer = new SplitContainer();
        sidePanelLayout = new TableLayoutPanel();
        titleLabel = new Label();
        showLabelsCheckBox = new CheckBox();
        fieldLabel = new Label();
        fieldComboBox = new ComboBox();
        fontSizeLabel = new Label();
        fontSizeNumeric = new NumericUpDown();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        sidePanelLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)fontSizeNumeric).BeginInit();
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
        splitContainer.Panel1.Controls.Add(sidePanelLayout);
        splitContainer.Panel1MinSize = 210;
        splitContainer.Panel2.Controls.Add(geoKernelViewerControl);
        splitContainer.Size = new Size(1200, 776);
        splitContainer.SplitterDistance = 230;
        splitContainer.SplitterWidth = 1;
        splitContainer.TabIndex = 0;
        // 
        // sidePanelLayout
        // 
        sidePanelLayout.BackColor = Color.FromArgb(239, 239, 239);
        sidePanelLayout.ColumnCount = 1;
        sidePanelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sidePanelLayout.Controls.Add(titleLabel, 0, 0);
        sidePanelLayout.Controls.Add(showLabelsCheckBox, 0, 1);
        sidePanelLayout.Controls.Add(fieldLabel, 0, 2);
        sidePanelLayout.Controls.Add(fieldComboBox, 0, 3);
        sidePanelLayout.Controls.Add(fontSizeLabel, 0, 4);
        sidePanelLayout.Controls.Add(fontSizeNumeric, 0, 5);
        sidePanelLayout.Dock = DockStyle.Fill;
        sidePanelLayout.Location = new Point(0, 0);
        sidePanelLayout.Name = "sidePanelLayout";
        sidePanelLayout.Padding = new Padding(8);
        sidePanelLayout.RowCount = 7;
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        sidePanelLayout.Size = new Size(230, 776);
        sidePanelLayout.TabIndex = 0;
        // 
        // titleLabel
        // 
        titleLabel.Dock = DockStyle.Fill;
        titleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        titleLabel.Location = new Point(8, 8);
        titleLabel.Margin = new Padding(0);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(214, 28);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "Label style";
        titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // showLabelsCheckBox
        // 
        showLabelsCheckBox.Checked = true;
        showLabelsCheckBox.CheckState = CheckState.Checked;
        showLabelsCheckBox.Dock = DockStyle.Fill;
        showLabelsCheckBox.Location = new Point(8, 36);
        showLabelsCheckBox.Margin = new Padding(0);
        showLabelsCheckBox.Name = "showLabelsCheckBox";
        showLabelsCheckBox.Size = new Size(214, 34);
        showLabelsCheckBox.TabIndex = 1;
        showLabelsCheckBox.Text = "Show labels";
        showLabelsCheckBox.UseVisualStyleBackColor = true;
        showLabelsCheckBox.CheckedChanged += labelControl_Changed;
        // 
        // fieldLabel
        // 
        fieldLabel.Dock = DockStyle.Fill;
        fieldLabel.Location = new Point(8, 70);
        fieldLabel.Margin = new Padding(0);
        fieldLabel.Name = "fieldLabel";
        fieldLabel.Size = new Size(214, 24);
        fieldLabel.TabIndex = 2;
        fieldLabel.Text = "Label field";
        fieldLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // fieldComboBox
        // 
        fieldComboBox.Dock = DockStyle.Fill;
        fieldComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        fieldComboBox.FormattingEnabled = true;
        fieldComboBox.Location = new Point(8, 94);
        fieldComboBox.Margin = new Padding(0, 0, 0, 10);
        fieldComboBox.Name = "fieldComboBox";
        fieldComboBox.Size = new Size(214, 23);
        fieldComboBox.TabIndex = 3;
        fieldComboBox.SelectedIndexChanged += labelControl_Changed;
        // 
        // fontSizeLabel
        // 
        fontSizeLabel.Dock = DockStyle.Fill;
        fontSizeLabel.Location = new Point(8, 128);
        fontSizeLabel.Margin = new Padding(0);
        fontSizeLabel.Name = "fontSizeLabel";
        fontSizeLabel.Size = new Size(214, 24);
        fontSizeLabel.TabIndex = 4;
        fontSizeLabel.Text = "Font size";
        fontSizeLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // fontSizeNumeric
        // 
        fontSizeNumeric.DecimalPlaces = 1;
        fontSizeNumeric.Dock = DockStyle.Fill;
        fontSizeNumeric.Location = new Point(8, 152);
        fontSizeNumeric.Margin = new Padding(0, 0, 0, 10);
        fontSizeNumeric.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
        fontSizeNumeric.Minimum = new decimal(new int[] { 5, 0, 0, 0 });
        fontSizeNumeric.Name = "fontSizeNumeric";
        fontSizeNumeric.Size = new Size(214, 23);
        fontSizeNumeric.TabIndex = 5;
        fontSizeNumeric.Value = new decimal(new int[] { 9, 0, 0, 0 });
        fontSizeNumeric.ValueChanged += labelControl_Changed;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(969, 776);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // statusStrip
        // 
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
        statusLabel.Size = new Size(39, 19);
        statusLabel.Text = "Ready";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 800);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "BasicLabel";
        Shown += MainForm_Shown;
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        sidePanelLayout.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)fontSizeNumeric).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
