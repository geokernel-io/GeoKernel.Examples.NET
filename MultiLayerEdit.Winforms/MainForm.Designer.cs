namespace GeoKernel.MultiLayerEdit.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private FlowLayoutPanel toolbarPanel;
    private CheckBox redLayerButton;
    private CheckBox blueLayerButton;
    private Panel layerSeparator;
    private Button addButton;
    private Button commitButton;
    private Button rollbackButton;
    private Button resetButton;
    private Panel navigationSeparator;
    private Button fullExtentButton;
    private Label stateLabel;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private TextBox infoTextBox;
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new FlowLayoutPanel();
        redLayerButton = new CheckBox();
        blueLayerButton = new CheckBox();
        layerSeparator = new Panel();
        addButton = new Button();
        commitButton = new Button();
        rollbackButton = new Button();
        resetButton = new Button();
        navigationSeparator = new Panel();
        fullExtentButton = new Button();
        stateLabel = new Label();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        infoTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
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
        toolbarPanel.Controls.AddRange(new Control[] { redLayerButton, blueLayerButton, layerSeparator, addButton, commitButton, rollbackButton, resetButton, navigationSeparator, fullExtentButton, stateLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // redLayerButton
        // 
        redLayerButton.Checked = true;
        redLayerButton.CheckState = CheckState.Checked;
        redLayerButton.Name = "redLayerButton";
        redLayerButton.Appearance = Appearance.Button;
        redLayerButton.AutoSize = true;
        redLayerButton.Height = 28;
        redLayerButton.Margin = new Padding(0, 3, 4, 3);
        redLayerButton.Padding = new Padding(8, 0, 8, 0);
        redLayerButton.TextAlign = ContentAlignment.MiddleCenter;
        redLayerButton.UseVisualStyleBackColor = true;
        redLayerButton.Size = new Size(103, 29);
        redLayerButton.Text = "Active: Red Points";
        redLayerButton.Click += redLayerButton_Click;
        // 
        // blueLayerButton
        // 
        blueLayerButton.Name = "blueLayerButton";
        blueLayerButton.Appearance = Appearance.Button;
        blueLayerButton.AutoSize = true;
        blueLayerButton.Height = 28;
        blueLayerButton.Margin = new Padding(0, 3, 4, 3);
        blueLayerButton.Padding = new Padding(8, 0, 8, 0);
        blueLayerButton.TextAlign = ContentAlignment.MiddleCenter;
        blueLayerButton.UseVisualStyleBackColor = true;
        blueLayerButton.AutoSize = true;
        blueLayerButton.Height = 28;
        blueLayerButton.Margin = new Padding(0, 3, 4, 3);
        blueLayerButton.Padding = new Padding(8, 0, 8, 0);
        blueLayerButton.UseVisualStyleBackColor = true;
        blueLayerButton.Size = new Size(105, 29);
        blueLayerButton.Text = "Active: Blue Points";
        blueLayerButton.Click += blueLayerButton_Click;
        // 
        // layerSeparator
        // 
        layerSeparator.BackColor = SystemColors.ControlDark;
        layerSeparator.Margin = new Padding(4, 5, 6, 5);
        layerSeparator.Name = "layerSeparator";
        layerSeparator.Size = new Size(1, 26);
        // 
        // addButton
        // 
        addButton.Name = "addButton";
        addButton.AutoSize = true;
        addButton.Height = 28;
        addButton.Margin = new Padding(0, 3, 4, 3);
        addButton.Padding = new Padding(8, 0, 8, 0);
        addButton.UseVisualStyleBackColor = true;
        addButton.Size = new Size(117, 29);
        addButton.Text = "Add To Active Layer";
        addButton.Click += addButton_Click;
        // 
        // commitButton
        // 
        commitButton.Name = "commitButton";
        commitButton.AutoSize = true;
        commitButton.Height = 28;
        commitButton.Margin = new Padding(0, 3, 4, 3);
        commitButton.Padding = new Padding(8, 0, 8, 0);
        commitButton.UseVisualStyleBackColor = true;
        commitButton.Size = new Size(81, 29);
        commitButton.Text = "Commit Both";
        commitButton.Click += commitButton_Click;
        // 
        // rollbackButton
        // 
        rollbackButton.Name = "rollbackButton";
        rollbackButton.AutoSize = true;
        rollbackButton.Height = 28;
        rollbackButton.Margin = new Padding(0, 3, 4, 3);
        rollbackButton.Padding = new Padding(8, 0, 8, 0);
        rollbackButton.UseVisualStyleBackColor = true;
        rollbackButton.Size = new Size(83, 29);
        rollbackButton.Text = "Rollback Both";
        rollbackButton.Click += rollbackButton_Click;
        // 
        // resetButton
        // 
        resetButton.Name = "resetButton";
        resetButton.AutoSize = true;
        resetButton.Height = 28;
        resetButton.Margin = new Padding(0, 3, 4, 3);
        resetButton.Padding = new Padding(8, 0, 8, 0);
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Size = new Size(39, 29);
        resetButton.Text = "Reset";
        resetButton.Click += resetButton_Click;
        // 
        // navigationSeparator
        // 
        navigationSeparator.BackColor = SystemColors.ControlDark;
        navigationSeparator.Margin = new Padding(4, 5, 6, 5);
        navigationSeparator.Name = "navigationSeparator";
        navigationSeparator.Size = new Size(1, 26);
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
        stateLabel.Size = new Size(137, 29);
        stateLabel.Text = "Active edit layer: -";
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.Location = new Point(0, 32);
        splitContainer.Margin = new Padding(0);
        splitContainer.Name = "splitContainer";
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
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
        infoTextBox.Font = new Font("Consolas", 9F);
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
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar });
        downloadProgressBar.Size = new Size(180, 18);
        downloadProgressBar.Visible = false;
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
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MultiLayerEdit";
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
