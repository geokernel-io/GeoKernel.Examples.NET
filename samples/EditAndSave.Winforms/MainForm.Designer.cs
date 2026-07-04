namespace GeoKernel.EditAndSave.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private FlowLayoutPanel toolbarPanel;
    private Button resetCopyButton;
    private CheckBox addPointButton;
    private CheckBox panButton;
    private Panel editSeparator;
    private Button commitButton;
    private Button reloadButton;
    private Button fullExtentButton;
    private Label stateLabel;
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
        resetCopyButton = new Button();
        addPointButton = new CheckBox();
        panButton = new CheckBox();
        editSeparator = new Panel();
        commitButton = new Button();
        reloadButton = new Button();
        fullExtentButton = new Button();
        stateLabel = new Label();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        infoTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
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
        toolbarPanel.Controls.AddRange(new Control[] { resetCopyButton, addPointButton, panButton, editSeparator, commitButton, reloadButton, fullExtentButton, stateLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // resetCopyButton
        // 
        resetCopyButton.Name = "resetCopyButton";
        resetCopyButton.AutoSize = true;
        resetCopyButton.Height = 28;
        resetCopyButton.Margin = new Padding(0, 3, 4, 3);
        resetCopyButton.Padding = new Padding(8, 0, 8, 0);
        resetCopyButton.UseVisualStyleBackColor = true;
        resetCopyButton.Size = new Size(119, 29);
        resetCopyButton.Text = "Reset Working Copy";
        resetCopyButton.Click += resetCopyButton_Click;
        // 
        // addPointButton
        // 
        addPointButton.Checked = true;
        addPointButton.CheckState = CheckState.Checked;
        addPointButton.Name = "addPointButton";
        addPointButton.Appearance = Appearance.Button;
        addPointButton.AutoSize = true;
        addPointButton.Height = 28;
        addPointButton.Margin = new Padding(0, 3, 4, 3);
        addPointButton.Padding = new Padding(8, 0, 8, 0);
        addPointButton.TextAlign = ContentAlignment.MiddleCenter;
        addPointButton.UseVisualStyleBackColor = true;
        addPointButton.Size = new Size(62, 29);
        addPointButton.Text = "Add Point";
        addPointButton.Click += addPointButton_Click;
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
        // editSeparator
        // 
        editSeparator.BackColor = SystemColors.ControlDark;
        editSeparator.Margin = new Padding(4, 5, 6, 5);
        editSeparator.Name = "editSeparator";
        editSeparator.Size = new Size(1, 26);
        // 
        // commitButton
        // 
        commitButton.Name = "commitButton";
        commitButton.AutoSize = true;
        commitButton.Height = 28;
        commitButton.Margin = new Padding(0, 3, 4, 3);
        commitButton.Padding = new Padding(8, 0, 8, 0);
        commitButton.UseVisualStyleBackColor = true;
        commitButton.Size = new Size(90, 29);
        commitButton.Text = "Commit To File";
        commitButton.Click += commitButton_Click;
        // 
        // reloadButton
        // 
        reloadButton.Name = "reloadButton";
        reloadButton.AutoSize = true;
        reloadButton.Height = 28;
        reloadButton.Margin = new Padding(0, 3, 4, 3);
        reloadButton.Padding = new Padding(8, 0, 8, 0);
        reloadButton.UseVisualStyleBackColor = true;
        reloadButton.Size = new Size(93, 29);
        reloadButton.Text = "Reload From File";
        reloadButton.Click += reloadButton_Click;
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
        // stateLabel
        // 
        stateLabel.Margin = new Padding(12, 1, 0, 2);
        stateLabel.Name = "stateLabel";
        stateLabel.AutoSize = true;
        stateLabel.Margin = new Padding(8, 9, 8, 0);
        stateLabel.TextAlign = ContentAlignment.MiddleLeft;
        stateLabel.Size = new Size(165, 29);
        stateLabel.Text = "Editing: OFF | Feature count: 0";
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
        splitContainer.SplitterDistance = 798;
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
        geoKernelViewerControl.Size = new Size(798, 744);
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
        infoTextBox.Size = new Size(401, 744);
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
        Text = "EditAndSave";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
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
