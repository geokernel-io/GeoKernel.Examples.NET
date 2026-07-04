namespace GeoKernel.SnappingEnabled.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private FlowLayoutPanel toolbarPanel;
    private Button fullExtentButton;
    private Panel navigationSeparator;
    private CheckBox addLineButton;
    private CheckBox panButton;
    private Panel snappingSeparator;
    private CheckBox snappingButton;
    private Label toleranceLabel;
    private NumericUpDown toleranceNumeric;
    private Button resetGuideButton;
    private Label lineCountLabel;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private TextBox infoTextBox;
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
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new FlowLayoutPanel();
        fullExtentButton = new Button();
        navigationSeparator = new Panel();
        addLineButton = new CheckBox();
        panButton = new CheckBox();
        snappingSeparator = new Panel();
        snappingButton = new CheckBox();
        toleranceLabel = new Label();
        toleranceNumeric = new NumericUpDown();
        resetGuideButton = new Button();
        lineCountLabel = new Label();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        infoTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)toleranceNumeric).BeginInit();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // layoutPanel
        // 
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(toolbarPanel, 0, 0);
        layoutPanel.Controls.Add(splitContainer, 0, 1);
        layoutPanel.Controls.Add(statusStrip, 0, 2);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.Location = new Point(0, 0);
        layoutPanel.Margin = new Padding(0);
        layoutPanel.Name = "layoutPanel";
        layoutPanel.RowCount = 3;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.AutoSize = false;
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.FlowDirection = FlowDirection.LeftToRight;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Controls.AddRange(new Control[] { fullExtentButton, navigationSeparator, addLineButton, panButton, snappingSeparator, snappingButton, toleranceLabel, toleranceNumeric, resetGuideButton, lineCountLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.AutoSize = true;
        fullExtentButton.Height = 28;
        fullExtentButton.Margin = new Padding(0, 3, 4, 3);
        fullExtentButton.Padding = new Padding(8, 0, 8, 0);
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Size = new Size(66, 29);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // navigationSeparator
        // 
        navigationSeparator.BackColor = SystemColors.ControlDark;
        navigationSeparator.Margin = new Padding(4, 5, 6, 5);
        navigationSeparator.Name = "navigationSeparator";
        navigationSeparator.Size = new Size(1, 26);
        // 
        // addLineButton
        // 
        addLineButton.Checked = true;
        addLineButton.CheckState = CheckState.Checked;
        addLineButton.Name = "addLineButton";
        addLineButton.Appearance = Appearance.Button;
        addLineButton.AutoSize = true;
        addLineButton.Height = 28;
        addLineButton.Margin = new Padding(0, 3, 4, 3);
        addLineButton.Padding = new Padding(8, 0, 8, 0);
        addLineButton.TextAlign = ContentAlignment.MiddleCenter;
        addLineButton.UseVisualStyleBackColor = true;
        addLineButton.Size = new Size(81, 29);
        addLineButton.Text = "Add Polyline";
        addLineButton.Click += addLineButton_Click;
        // 
        // panButton
        // 
        panButton.Name = "panButton";
        panButton.Appearance = Appearance.Button;
        panButton.AutoSize = true;
        panButton.Height = 28;
        panButton.Margin = new Padding(0, 3, 4, 3);
        panButton.Padding = new Padding(8, 0, 8, 0);
        panButton.TextAlign = ContentAlignment.MiddleCenter;
        panButton.UseVisualStyleBackColor = true;
        panButton.AutoSize = true;
        panButton.Height = 28;
        panButton.Margin = new Padding(0, 3, 4, 3);
        panButton.Padding = new Padding(8, 0, 8, 0);
        panButton.UseVisualStyleBackColor = true;
        panButton.Size = new Size(31, 29);
        panButton.Text = "Pan";
        panButton.Click += panButton_Click;
        // 
        // snappingSeparator
        // 
        snappingSeparator.BackColor = SystemColors.ControlDark;
        snappingSeparator.Margin = new Padding(4, 5, 6, 5);
        snappingSeparator.Name = "snappingSeparator";
        snappingSeparator.Size = new Size(1, 26);
        // 
        // snappingButton
        // 
        snappingButton.Checked = true;
        snappingButton.CheckState = CheckState.Checked;
        snappingButton.Name = "snappingButton";
        snappingButton.Appearance = Appearance.Button;
        snappingButton.AutoSize = true;
        snappingButton.Height = 28;
        snappingButton.Margin = new Padding(0, 3, 4, 3);
        snappingButton.Padding = new Padding(8, 0, 8, 0);
        snappingButton.TextAlign = ContentAlignment.MiddleCenter;
        snappingButton.UseVisualStyleBackColor = true;
        snappingButton.Size = new Size(84, 29);
        snappingButton.Text = "Snapping ON";
        snappingButton.CheckedChanged += snappingButton_CheckedChanged;
        // 
        // toleranceLabel
        // 
        toleranceLabel.Name = "toleranceLabel";
        toleranceLabel.AutoSize = true;
        toleranceLabel.Margin = new Padding(8, 9, 8, 0);
        toleranceLabel.TextAlign = ContentAlignment.MiddleLeft;
        toleranceLabel.Size = new Size(76, 29);
        toleranceLabel.Text = "Tolerance px";
        // 
        // toleranceNumeric
        // 
        toleranceNumeric.Maximum = new decimal(new int[] { 64, 0, 0, 0 });
        toleranceNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        toleranceNumeric.Name = "toleranceNumeric";
        toleranceNumeric.Margin = new Padding(0, 4, 8, 0);
        toleranceNumeric.Size = new Size(58, 23);
        toleranceNumeric.Value = new decimal(new int[] { 14, 0, 0, 0 });
        toleranceNumeric.ValueChanged += toleranceNumeric_ValueChanged;
        // 
        // 
        // resetGuideButton
        // 
        resetGuideButton.Name = "resetGuideButton";
        resetGuideButton.AutoSize = true;
        resetGuideButton.Height = 28;
        resetGuideButton.Margin = new Padding(0, 3, 4, 3);
        resetGuideButton.Padding = new Padding(8, 0, 8, 0);
        resetGuideButton.UseVisualStyleBackColor = true;
        resetGuideButton.Size = new Size(72, 29);
        resetGuideButton.Text = "Reset Guide";
        resetGuideButton.Click += resetGuideButton_Click;
        // 
        // lineCountLabel
        // 
        lineCountLabel.Margin = new Padding(12, 1, 0, 2);
        lineCountLabel.Name = "lineCountLabel";
        lineCountLabel.AutoSize = true;
        lineCountLabel.Margin = new Padding(8, 9, 8, 0);
        lineCountLabel.TextAlign = ContentAlignment.MiddleLeft;
        lineCountLabel.Size = new Size(82, 29);
        lineCountLabel.Text = "Drawn lines: 0";
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.Location = new Point(0, 32);
        splitContainer.Margin = new Padding(0);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(infoTextBox);
        splitContainer.Size = new Size(1200, 744);
        splitContainer.SplitterDistance = 838;
        splitContainer.SplitterWidth = 1;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(838, 744);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // infoTextBox
        // 
        infoTextBox.BackColor = Color.White;
        infoTextBox.Dock = DockStyle.Fill;
        infoTextBox.Location = new Point(0, 0);
        infoTextBox.Multiline = true;
        infoTextBox.Name = "infoTextBox";
        infoTextBox.ReadOnly = true;
        infoTextBox.ScrollBars = ScrollBars.Vertical;
        infoTextBox.Size = new Size(361, 744);
        infoTextBox.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 776);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1200, 24);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 2;
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
        Controls.Add(layoutPanel);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "SnappingEnabled";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)toleranceNumeric).EndInit();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        splitContainer.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
