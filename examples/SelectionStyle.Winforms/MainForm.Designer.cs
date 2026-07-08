namespace GeoKernel.SelectionStyle.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private SplitContainer splitContainer;
    private TableLayoutPanel sidePanelLayout;
    private Label selectedLineColorLabel;
    private Button selectedLineColorButton;
    private Label selectedLineWidthLabel;
    private NumericUpDown selectedLineWidthNumeric;
    private Button clearSelectionButton;
    private Button resetStyleButton;
    private Label hintLabel;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private ColorDialog colorDialog;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        splitContainer = new SplitContainer();
        sidePanelLayout = new TableLayoutPanel();
        selectedLineColorLabel = new Label();
        selectedLineColorButton = new Button();
        selectedLineWidthLabel = new Label();
        selectedLineWidthNumeric = new NumericUpDown();
        clearSelectionButton = new Button();
        resetStyleButton = new Button();
        hintLabel = new Label();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        colorDialog = new ColorDialog();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        sidePanelLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)selectedLineWidthNumeric).BeginInit();
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
        splitContainer.Panel1MinSize = 210;
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(geoKernelViewerControl);
        splitContainer.Size = new Size(1100, 698);
        splitContainer.SplitterDistance = 230;
        splitContainer.SplitterWidth = 1;
        splitContainer.TabIndex = 0;
        // 
        // sidePanelLayout
        // 
        sidePanelLayout.ColumnCount = 1;
        sidePanelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sidePanelLayout.Controls.Add(selectedLineColorLabel, 0, 0);
        sidePanelLayout.Controls.Add(selectedLineColorButton, 0, 1);
        sidePanelLayout.Controls.Add(selectedLineWidthLabel, 0, 2);
        sidePanelLayout.Controls.Add(selectedLineWidthNumeric, 0, 3);
        sidePanelLayout.Controls.Add(clearSelectionButton, 0, 4);
        sidePanelLayout.Controls.Add(resetStyleButton, 0, 5);
        sidePanelLayout.Controls.Add(hintLabel, 0, 6);
        sidePanelLayout.Dock = DockStyle.Fill;
        sidePanelLayout.Location = new Point(0, 0);
        sidePanelLayout.Name = "sidePanelLayout";
        sidePanelLayout.Padding = new Padding(10);
        sidePanelLayout.RowCount = 8;
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        sidePanelLayout.Size = new Size(230, 698);
        sidePanelLayout.TabIndex = 0;
        // 
        // selectedLineColorLabel
        // 
        selectedLineColorLabel.Dock = DockStyle.Fill;
        selectedLineColorLabel.Location = new Point(10, 10);
        selectedLineColorLabel.Margin = new Padding(0);
        selectedLineColorLabel.Name = "selectedLineColorLabel";
        selectedLineColorLabel.Size = new Size(210, 24);
        selectedLineColorLabel.TabIndex = 0;
        selectedLineColorLabel.Text = "Selected Line Color";
        selectedLineColorLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // selectedLineColorButton
        // 
        selectedLineColorButton.Dock = DockStyle.Fill;
        selectedLineColorButton.Location = new Point(10, 34);
        selectedLineColorButton.Margin = new Padding(0, 0, 0, 10);
        selectedLineColorButton.Name = "selectedLineColorButton";
        selectedLineColorButton.Size = new Size(210, 26);
        selectedLineColorButton.TabIndex = 1;
        selectedLineColorButton.UseVisualStyleBackColor = false;
        selectedLineColorButton.Click += selectedLineColorButton_Click;
        // 
        // selectedLineWidthLabel
        // 
        selectedLineWidthLabel.Dock = DockStyle.Fill;
        selectedLineWidthLabel.Location = new Point(10, 70);
        selectedLineWidthLabel.Margin = new Padding(0);
        selectedLineWidthLabel.Name = "selectedLineWidthLabel";
        selectedLineWidthLabel.Size = new Size(210, 24);
        selectedLineWidthLabel.TabIndex = 2;
        selectedLineWidthLabel.Text = "Selected Line Width";
        selectedLineWidthLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // selectedLineWidthNumeric
        // 
        selectedLineWidthNumeric.DecimalPlaces = 1;
        selectedLineWidthNumeric.Dock = DockStyle.Fill;
        selectedLineWidthNumeric.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
        selectedLineWidthNumeric.Location = new Point(10, 94);
        selectedLineWidthNumeric.Margin = new Padding(0, 0, 0, 10);
        selectedLineWidthNumeric.Maximum = new decimal(new int[] { 16, 0, 0, 0 });
        selectedLineWidthNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        selectedLineWidthNumeric.Name = "selectedLineWidthNumeric";
        selectedLineWidthNumeric.Size = new Size(210, 23);
        selectedLineWidthNumeric.TabIndex = 3;
        selectedLineWidthNumeric.Value = new decimal(new int[] { 4, 0, 0, 0 });
        selectedLineWidthNumeric.ValueChanged += selectedLineWidthNumeric_ValueChanged;
        // 
        // clearSelectionButton
        // 
        clearSelectionButton.Dock = DockStyle.Fill;
        clearSelectionButton.Location = new Point(10, 130);
        clearSelectionButton.Margin = new Padding(0, 0, 0, 10);
        clearSelectionButton.Name = "clearSelectionButton";
        clearSelectionButton.Size = new Size(210, 26);
        clearSelectionButton.TabIndex = 4;
        clearSelectionButton.Text = "Clear Selection";
        clearSelectionButton.UseVisualStyleBackColor = true;
        clearSelectionButton.Click += clearSelectionButton_Click;
        // 
        // resetStyleButton
        // 
        resetStyleButton.Dock = DockStyle.Fill;
        resetStyleButton.Location = new Point(10, 166);
        resetStyleButton.Margin = new Padding(0, 0, 0, 10);
        resetStyleButton.Name = "resetStyleButton";
        resetStyleButton.Size = new Size(210, 26);
        resetStyleButton.TabIndex = 5;
        resetStyleButton.Text = "Reset Style";
        resetStyleButton.UseVisualStyleBackColor = true;
        resetStyleButton.Click += resetStyleButton_Click;
        // 
        // hintLabel
        // 
        hintLabel.Dock = DockStyle.Fill;
        hintLabel.Location = new Point(10, 202);
        hintLabel.Margin = new Padding(0);
        hintLabel.Name = "hintLabel";
        hintLabel.Size = new Size(210, 96);
        hintLabel.TabIndex = 6;
        hintLabel.Text = "Click any polygon, line, or point on the map. Selection rendering uses SelectedLineColor and SelectedLineWidth.";
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(869, 698);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 698);
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
        ClientSize = new Size(1100, 720);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SelectionStyle";
        Shown += MainForm_Shown;
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        sidePanelLayout.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)selectedLineWidthNumeric).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
