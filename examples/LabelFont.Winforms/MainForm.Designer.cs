namespace GeoKernel.LabelFont.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private SplitContainer splitContainer;
    private TableLayoutPanel sidePanelLayout;
    private Label titleLabel;
    private Label fontFamilyLabel;
    private ComboBox fontFamilyComboBox;
    private CheckBox boldCheckBox;
    private CheckBox italicCheckBox;
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
        fontFamilyLabel = new Label();
        fontFamilyComboBox = new ComboBox();
        boldCheckBox = new CheckBox();
        italicCheckBox = new CheckBox();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        sidePanelLayout.SuspendLayout();
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
        splitContainer.Panel1MinSize = 230;
        splitContainer.Panel2.Controls.Add(geoKernelViewerControl);
        splitContainer.Size = new Size(1200, 776);
        splitContainer.SplitterDistance = 245;
        splitContainer.SplitterWidth = 1;
        splitContainer.TabIndex = 0;
        // 
        // sidePanelLayout
        // 
        sidePanelLayout.BackColor = Color.FromArgb(239, 239, 239);
        sidePanelLayout.ColumnCount = 1;
        sidePanelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sidePanelLayout.Controls.Add(titleLabel, 0, 0);
        sidePanelLayout.Controls.Add(fontFamilyLabel, 0, 1);
        sidePanelLayout.Controls.Add(fontFamilyComboBox, 0, 2);
        sidePanelLayout.Controls.Add(boldCheckBox, 0, 3);
        sidePanelLayout.Controls.Add(italicCheckBox, 0, 4);
        sidePanelLayout.Dock = DockStyle.Fill;
        sidePanelLayout.Location = new Point(0, 0);
        sidePanelLayout.Name = "sidePanelLayout";
        sidePanelLayout.Padding = new Padding(8);
        sidePanelLayout.RowCount = 6;
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        sidePanelLayout.Size = new Size(245, 776);
        sidePanelLayout.TabIndex = 0;
        // 
        // titleLabel
        // 
        titleLabel.Dock = DockStyle.Fill;
        titleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        titleLabel.Location = new Point(8, 8);
        titleLabel.Margin = new Padding(0);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(229, 28);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "Label font";
        titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // fontFamilyLabel
        // 
        fontFamilyLabel.Dock = DockStyle.Fill;
        fontFamilyLabel.Location = new Point(8, 36);
        fontFamilyLabel.Margin = new Padding(0);
        fontFamilyLabel.Name = "fontFamilyLabel";
        fontFamilyLabel.Size = new Size(229, 24);
        fontFamilyLabel.TabIndex = 1;
        fontFamilyLabel.Text = "Font family";
        fontFamilyLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // fontFamilyComboBox
        // 
        fontFamilyComboBox.Dock = DockStyle.Fill;
        fontFamilyComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        fontFamilyComboBox.FormattingEnabled = true;
        fontFamilyComboBox.Location = new Point(8, 60);
        fontFamilyComboBox.Margin = new Padding(0, 0, 0, 10);
        fontFamilyComboBox.Name = "fontFamilyComboBox";
        fontFamilyComboBox.Size = new Size(229, 23);
        fontFamilyComboBox.TabIndex = 2;
        fontFamilyComboBox.SelectedIndexChanged += labelFontControl_Changed;
        // 
        // boldCheckBox
        // 
        boldCheckBox.Dock = DockStyle.Fill;
        boldCheckBox.Location = new Point(8, 94);
        boldCheckBox.Margin = new Padding(0);
        boldCheckBox.Name = "boldCheckBox";
        boldCheckBox.Size = new Size(229, 34);
        boldCheckBox.TabIndex = 3;
        boldCheckBox.Text = "Bold";
        boldCheckBox.UseVisualStyleBackColor = true;
        boldCheckBox.CheckedChanged += labelFontControl_Changed;
        // 
        // italicCheckBox
        // 
        italicCheckBox.Dock = DockStyle.Fill;
        italicCheckBox.Location = new Point(8, 128);
        italicCheckBox.Margin = new Padding(0);
        italicCheckBox.Name = "italicCheckBox";
        italicCheckBox.Size = new Size(229, 34);
        italicCheckBox.TabIndex = 4;
        italicCheckBox.Text = "Italic";
        italicCheckBox.UseVisualStyleBackColor = true;
        italicCheckBox.CheckedChanged += labelFontControl_Changed;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(954, 776);
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
        Text = "LabelFont";
        Shown += MainForm_Shown;
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        sidePanelLayout.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
