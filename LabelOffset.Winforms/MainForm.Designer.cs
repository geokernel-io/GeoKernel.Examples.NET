namespace GeoKernel.LabelOffset.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private SplitContainer splitContainer;
    private TableLayoutPanel controlsLayout;
    private Label titleLabel;
    private Label offsetXLabel;
    private NumericUpDown offsetXNumeric;
    private Label offsetYLabel;
    private NumericUpDown offsetYNumeric;
    private Button resetButton;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;

    protected override void Dispose(bool disposing) { if (disposing && components is not null) components.Dispose(); base.Dispose(disposing); }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        splitContainer = new SplitContainer(); controlsLayout = new TableLayoutPanel(); titleLabel = new Label(); offsetXLabel = new Label(); offsetXNumeric = new NumericUpDown(); offsetYLabel = new Label(); offsetYNumeric = new NumericUpDown(); resetButton = new Button(); geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl(); statusStrip = new StatusStrip(); statusLabel = new ToolStripStatusLabel(); downloadProgressBar = new ToolStripProgressBar();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit(); splitContainer.Panel1.SuspendLayout(); splitContainer.Panel2.SuspendLayout(); splitContainer.SuspendLayout(); controlsLayout.SuspendLayout(); ((System.ComponentModel.ISupportInitialize)offsetXNumeric).BeginInit(); ((System.ComponentModel.ISupportInitialize)offsetYNumeric).BeginInit(); statusStrip.SuspendLayout(); SuspendLayout();
        splitContainer.Dock = DockStyle.Fill; splitContainer.FixedPanel = FixedPanel.Panel1; splitContainer.Panel1.Controls.Add(controlsLayout); splitContainer.Panel2.Controls.Add(geoKernelViewerControl); splitContainer.SplitterDistance = 240; splitContainer.SplitterWidth = 1;
        controlsLayout.ColumnCount = 1; controlsLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F)); controlsLayout.Controls.Add(titleLabel, 0, 0); controlsLayout.Controls.Add(offsetXLabel, 0, 1); controlsLayout.Controls.Add(offsetXNumeric, 0, 2); controlsLayout.Controls.Add(offsetYLabel, 0, 3); controlsLayout.Controls.Add(offsetYNumeric, 0, 4); controlsLayout.Controls.Add(resetButton, 0, 5); controlsLayout.Dock = DockStyle.Fill; controlsLayout.Padding = new Padding(10); controlsLayout.RowCount = 7; controlsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F)); controlsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F)); controlsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F)); controlsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F)); controlsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 34F)); controlsLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F)); controlsLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        titleLabel.Dock = DockStyle.Fill; titleLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold); titleLabel.Text = "Label offset"; titleLabel.TextAlign = ContentAlignment.MiddleLeft;
        offsetXLabel.Dock = DockStyle.Fill; offsetXLabel.Text = "Offset X"; offsetXLabel.TextAlign = ContentAlignment.MiddleLeft; offsetXNumeric.DecimalPlaces = 1; offsetXNumeric.Minimum = -80; offsetXNumeric.Maximum = 80; offsetXNumeric.Increment = 2; offsetXNumeric.Value = 0; offsetXNumeric.Enabled = false; offsetXNumeric.Dock = DockStyle.Fill; offsetXNumeric.ValueChanged += offsetControl_ValueChanged;
        offsetYLabel.Dock = DockStyle.Fill; offsetYLabel.Text = "Offset Y"; offsetYLabel.TextAlign = ContentAlignment.MiddleLeft; offsetYNumeric.DecimalPlaces = 1; offsetYNumeric.Minimum = -80; offsetYNumeric.Maximum = 80; offsetYNumeric.Increment = 2; offsetYNumeric.Value = 0; offsetYNumeric.Enabled = false; offsetYNumeric.Dock = DockStyle.Fill; offsetYNumeric.ValueChanged += offsetControl_ValueChanged; resetButton.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right; resetButton.Height = 30; resetButton.Margin = new Padding(0, 5, 0, 5); resetButton.Text = "Reset Offset"; resetButton.Enabled = false; resetButton.UseVisualStyleBackColor = true; resetButton.Click += resetButton_Click;
        geoKernelViewerControl.Dock = DockStyle.Fill; geoKernelViewerControl.BackColor = Color.White;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar }); statusStrip.Dock = DockStyle.Bottom; statusLabel.Spring = true; statusLabel.Text = "Ready"; statusLabel.TextAlign = ContentAlignment.MiddleLeft; downloadProgressBar.Size = new Size(180, 18); downloadProgressBar.Visible = false;
        AutoScaleDimensions = new SizeF(7F, 15F); AutoScaleMode = AutoScaleMode.Font; ClientSize = new Size(1200, 800); Controls.Add(splitContainer); Controls.Add(statusStrip); Icon = (Icon)resources.GetObject("$this.Icon"); Name = "MainForm"; StartPosition = FormStartPosition.CenterScreen; Text = "LabelOffset"; Shown += MainForm_Shown;
        splitContainer.Panel1.ResumeLayout(false); splitContainer.Panel2.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit(); splitContainer.ResumeLayout(false); controlsLayout.ResumeLayout(false); ((System.ComponentModel.ISupportInitialize)offsetXNumeric).EndInit(); ((System.ComponentModel.ISupportInitialize)offsetYNumeric).EndInit(); statusStrip.ResumeLayout(false); statusStrip.PerformLayout(); ResumeLayout(false); PerformLayout();
    }
}
