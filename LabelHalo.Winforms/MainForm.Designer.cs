namespace GeoKernel.LabelHalo.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private SplitContainer splitContainer;
    private TableLayoutPanel sidePanelLayout;
    private Label titleLabel;
    private CheckBox haloEnabledCheckBox;
    private Label haloColorLabel;
    private ComboBox haloColorComboBox;
    private Label haloWidthLabel;
    private NumericUpDown haloWidthNumeric;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        splitContainer = new SplitContainer();
        sidePanelLayout = new TableLayoutPanel();
        titleLabel = new Label();
        haloEnabledCheckBox = new CheckBox();
        haloColorLabel = new Label();
        haloColorComboBox = new ComboBox();
        haloWidthLabel = new Label();
        haloWidthNumeric = new NumericUpDown();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        sidePanelLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)haloWidthNumeric).BeginInit();
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
        sidePanelLayout.Controls.Add(haloEnabledCheckBox, 0, 1);
        sidePanelLayout.Controls.Add(haloColorLabel, 0, 2);
        sidePanelLayout.Controls.Add(haloColorComboBox, 0, 3);
        sidePanelLayout.Controls.Add(haloWidthLabel, 0, 4);
        sidePanelLayout.Controls.Add(haloWidthNumeric, 0, 5);
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
        titleLabel.Text = "Label halo";
        titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // haloEnabledCheckBox
        // 
        haloEnabledCheckBox.Checked = true;
        haloEnabledCheckBox.CheckState = CheckState.Checked;
        haloEnabledCheckBox.Dock = DockStyle.Fill;
        haloEnabledCheckBox.Location = new Point(8, 36);
        haloEnabledCheckBox.Margin = new Padding(0);
        haloEnabledCheckBox.Name = "haloEnabledCheckBox";
        haloEnabledCheckBox.Size = new Size(214, 34);
        haloEnabledCheckBox.TabIndex = 1;
        haloEnabledCheckBox.Text = "Halo enabled";
        haloEnabledCheckBox.UseVisualStyleBackColor = true;
        haloEnabledCheckBox.CheckedChanged += haloControl_Changed;
        // 
        // haloColorLabel
        // 
        haloColorLabel.Dock = DockStyle.Fill;
        haloColorLabel.Location = new Point(8, 70);
        haloColorLabel.Margin = new Padding(0);
        haloColorLabel.Name = "haloColorLabel";
        haloColorLabel.Size = new Size(214, 24);
        haloColorLabel.TabIndex = 2;
        haloColorLabel.Text = "Halo color";
        haloColorLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // haloColorComboBox
        // 
        haloColorComboBox.Dock = DockStyle.Fill;
        haloColorComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        haloColorComboBox.FormattingEnabled = true;
        haloColorComboBox.Location = new Point(8, 94);
        haloColorComboBox.Margin = new Padding(0, 0, 0, 10);
        haloColorComboBox.Name = "haloColorComboBox";
        haloColorComboBox.Size = new Size(214, 23);
        haloColorComboBox.TabIndex = 3;
        haloColorComboBox.SelectedIndexChanged += haloControl_Changed;
        // 
        // haloWidthLabel
        // 
        haloWidthLabel.Dock = DockStyle.Fill;
        haloWidthLabel.Location = new Point(8, 128);
        haloWidthLabel.Margin = new Padding(0);
        haloWidthLabel.Name = "haloWidthLabel";
        haloWidthLabel.Size = new Size(214, 24);
        haloWidthLabel.TabIndex = 4;
        haloWidthLabel.Text = "Halo width";
        haloWidthLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // haloWidthNumeric
        // 
        haloWidthNumeric.DecimalPlaces = 1;
        haloWidthNumeric.Dock = DockStyle.Fill;
        haloWidthNumeric.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
        haloWidthNumeric.Location = new Point(8, 152);
        haloWidthNumeric.Margin = new Padding(0, 0, 0, 10);
        haloWidthNumeric.Maximum = new decimal(new int[] { 8, 0, 0, 0 });
        haloWidthNumeric.Minimum = new decimal(new int[] { 5, 0, 0, 65536 });
        haloWidthNumeric.Name = "haloWidthNumeric";
        haloWidthNumeric.Size = new Size(214, 23);
        haloWidthNumeric.TabIndex = 5;
        haloWidthNumeric.Value = new decimal(new int[] { 25, 0, 0, 65536 });
        haloWidthNumeric.ValueChanged += haloControl_Changed;
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
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar });
        statusStrip.Location = new Point(0, 776);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1200, 24);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 1;
        downloadProgressBar.Size = new Size(180, 18);
        downloadProgressBar.Visible = false;
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
        Icon = (Icon)resources.GetObject("$this.Icon");
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "LabelHalo";
        Shown += MainForm_Shown;
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        sidePanelLayout.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)haloWidthNumeric).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
