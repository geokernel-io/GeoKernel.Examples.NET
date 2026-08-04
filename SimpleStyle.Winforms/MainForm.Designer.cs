namespace GeoKernel.SimpleStyle.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private SplitContainer splitContainer;
    private TableLayoutPanel sidePanelLayout;
    private Label fillColorLabel;
    private Button fillColorButton;
    private Label lineColorLabel;
    private Button lineColorButton;
    private Label lineWidthLabel;
    private NumericUpDown lineWidthNumeric;
    private Label pointSizeLabel;
    private NumericUpDown pointSizeNumeric;
    private Button resetStyleButton;
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
        fillColorLabel = new Label();
        fillColorButton = new Button();
        lineColorLabel = new Label();
        lineColorButton = new Button();
        lineWidthLabel = new Label();
        lineWidthNumeric = new NumericUpDown();
        pointSizeLabel = new Label();
        pointSizeNumeric = new NumericUpDown();
        resetStyleButton = new Button();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        colorDialog = new ColorDialog();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        sidePanelLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)lineWidthNumeric).BeginInit();
        ((System.ComponentModel.ISupportInitialize)pointSizeNumeric).BeginInit();
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
        splitContainer.Size = new Size(1100, 720);
        splitContainer.SplitterDistance = 230;
        splitContainer.SplitterWidth = 1;
        splitContainer.TabIndex = 0;
        // 
        // sidePanelLayout
        // 
        sidePanelLayout.ColumnCount = 1;
        sidePanelLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sidePanelLayout.Controls.Add(fillColorLabel, 0, 0);
        sidePanelLayout.Controls.Add(fillColorButton, 0, 1);
        sidePanelLayout.Controls.Add(lineColorLabel, 0, 2);
        sidePanelLayout.Controls.Add(lineColorButton, 0, 3);
        sidePanelLayout.Controls.Add(lineWidthLabel, 0, 4);
        sidePanelLayout.Controls.Add(lineWidthNumeric, 0, 5);
        sidePanelLayout.Controls.Add(pointSizeLabel, 0, 6);
        sidePanelLayout.Controls.Add(pointSizeNumeric, 0, 7);
        sidePanelLayout.Controls.Add(resetStyleButton, 0, 8);
        sidePanelLayout.Dock = DockStyle.Fill;
        sidePanelLayout.Location = new Point(0, 0);
        sidePanelLayout.Name = "sidePanelLayout";
        sidePanelLayout.Padding = new Padding(10);
        sidePanelLayout.RowCount = 10;
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        sidePanelLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        sidePanelLayout.Size = new Size(230, 720);
        sidePanelLayout.TabIndex = 0;
        // 
        // fillColorLabel
        // 
        fillColorLabel.Dock = DockStyle.Fill;
        fillColorLabel.Location = new Point(10, 10);
        fillColorLabel.Margin = new Padding(0);
        fillColorLabel.Name = "fillColorLabel";
        fillColorLabel.Size = new Size(210, 24);
        fillColorLabel.TabIndex = 0;
        fillColorLabel.Text = "Fill Color";
        fillColorLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // fillColorButton
        // 
        fillColorButton.Dock = DockStyle.Fill;
        fillColorButton.Location = new Point(10, 34);
        fillColorButton.Margin = new Padding(0, 0, 0, 10);
        fillColorButton.Name = "fillColorButton";
        fillColorButton.Size = new Size(210, 26);
        fillColorButton.TabIndex = 1;
        fillColorButton.UseVisualStyleBackColor = false;
        fillColorButton.Click += fillColorButton_Click;
        // 
        // lineColorLabel
        // 
        lineColorLabel.Dock = DockStyle.Fill;
        lineColorLabel.Location = new Point(10, 70);
        lineColorLabel.Margin = new Padding(0);
        lineColorLabel.Name = "lineColorLabel";
        lineColorLabel.Size = new Size(210, 24);
        lineColorLabel.TabIndex = 2;
        lineColorLabel.Text = "Line Color";
        lineColorLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lineColorButton
        // 
        lineColorButton.Dock = DockStyle.Fill;
        lineColorButton.Location = new Point(10, 94);
        lineColorButton.Margin = new Padding(0, 0, 0, 10);
        lineColorButton.Name = "lineColorButton";
        lineColorButton.Size = new Size(210, 26);
        lineColorButton.TabIndex = 3;
        lineColorButton.UseVisualStyleBackColor = false;
        lineColorButton.Click += lineColorButton_Click;
        // 
        // lineWidthLabel
        // 
        lineWidthLabel.Dock = DockStyle.Fill;
        lineWidthLabel.Location = new Point(10, 130);
        lineWidthLabel.Margin = new Padding(0);
        lineWidthLabel.Name = "lineWidthLabel";
        lineWidthLabel.Size = new Size(210, 24);
        lineWidthLabel.TabIndex = 4;
        lineWidthLabel.Text = "Line Width";
        lineWidthLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // lineWidthNumeric
        // 
        lineWidthNumeric.DecimalPlaces = 1;
        lineWidthNumeric.Dock = DockStyle.Fill;
        lineWidthNumeric.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
        lineWidthNumeric.Location = new Point(10, 154);
        lineWidthNumeric.Margin = new Padding(0, 0, 0, 10);
        lineWidthNumeric.Maximum = new decimal(new int[] { 12, 0, 0, 0 });
        lineWidthNumeric.Minimum = new decimal(new int[] { 5, 0, 0, 65536 });
        lineWidthNumeric.Name = "lineWidthNumeric";
        lineWidthNumeric.Size = new Size(210, 23);
        lineWidthNumeric.TabIndex = 5;
        lineWidthNumeric.Value = new decimal(new int[] { 2, 0, 0, 0 });
        lineWidthNumeric.ValueChanged += styleNumeric_ValueChanged;
        // 
        // pointSizeLabel
        // 
        pointSizeLabel.Dock = DockStyle.Fill;
        pointSizeLabel.Location = new Point(10, 190);
        pointSizeLabel.Margin = new Padding(0);
        pointSizeLabel.Name = "pointSizeLabel";
        pointSizeLabel.Size = new Size(210, 24);
        pointSizeLabel.TabIndex = 6;
        pointSizeLabel.Text = "Point Size";
        pointSizeLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // pointSizeNumeric
        // 
        pointSizeNumeric.DecimalPlaces = 1;
        pointSizeNumeric.Dock = DockStyle.Fill;
        pointSizeNumeric.Increment = new decimal(new int[] { 5, 0, 0, 65536 });
        pointSizeNumeric.Location = new Point(10, 214);
        pointSizeNumeric.Margin = new Padding(0, 0, 0, 10);
        pointSizeNumeric.Maximum = new decimal(new int[] { 32, 0, 0, 0 });
        pointSizeNumeric.Minimum = new decimal(new int[] { 2, 0, 0, 0 });
        pointSizeNumeric.Name = "pointSizeNumeric";
        pointSizeNumeric.Size = new Size(210, 23);
        pointSizeNumeric.TabIndex = 7;
        pointSizeNumeric.Value = new decimal(new int[] { 10, 0, 0, 0 });
        pointSizeNumeric.ValueChanged += styleNumeric_ValueChanged;
        // 
        // resetStyleButton
        // 
        resetStyleButton.Dock = DockStyle.Fill;
        resetStyleButton.Location = new Point(10, 250);
        resetStyleButton.Margin = new Padding(0, 0, 0, 10);
        resetStyleButton.Name = "resetStyleButton";
        resetStyleButton.Size = new Size(210, 26);
        resetStyleButton.TabIndex = 8;
        resetStyleButton.Text = "Reset Style";
        resetStyleButton.UseVisualStyleBackColor = true;
        resetStyleButton.Click += resetStyleButton_Click;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(869, 720);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 720);
        Controls.Add(splitContainer);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SimpleStyle";
        Shown += MainForm_Shown;
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        sidePanelLayout.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)lineWidthNumeric).EndInit();
        ((System.ComponentModel.ISupportInitialize)pointSizeNumeric).EndInit();
        ResumeLayout(false);
    }
}
