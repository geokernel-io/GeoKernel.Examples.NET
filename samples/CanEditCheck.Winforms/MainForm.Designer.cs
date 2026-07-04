namespace GeoKernel.CanEditCheck.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private FlowLayoutPanel toolbarPanel;
    private Button beginEditButton;
    private Button commitEditButton;
    private Button rollbackEditButton;
    private Panel editSeparator;
    private CheckBox selectButton;
    private Button clearSelectionButton;
    private Panel resetSeparator;
    private Button resetButton;
    private Button fullExtentButton;
    private Label stateLabel;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private TableLayoutPanel sidePanel;
    private ListView checkListView;
    private TextBox selectionTextBox;
    private ListView featureListView;
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new FlowLayoutPanel();
        beginEditButton = new Button();
        commitEditButton = new Button();
        rollbackEditButton = new Button();
        editSeparator = new Panel();
        selectButton = new CheckBox();
        clearSelectionButton = new Button();
        resetSeparator = new Panel();
        resetButton = new Button();
        fullExtentButton = new Button();
        stateLabel = new Label();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        sidePanel = new TableLayoutPanel();
        checkListView = new ListView();
        selectionTextBox = new TextBox();
        featureListView = new ListView();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        sidePanel.SuspendLayout();
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
        toolbarPanel.Controls.AddRange(new Control[] { beginEditButton, commitEditButton, rollbackEditButton, editSeparator, selectButton, clearSelectionButton, resetSeparator, resetButton, fullExtentButton, stateLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // beginEditButton
        // 
        beginEditButton.Name = "beginEditButton";
        beginEditButton.AutoSize = true;
        beginEditButton.Height = 28;
        beginEditButton.Margin = new Padding(0, 3, 4, 3);
        beginEditButton.Padding = new Padding(8, 0, 8, 0);
        beginEditButton.UseVisualStyleBackColor = true;
        beginEditButton.Size = new Size(64, 29);
        beginEditButton.Text = "Begin Edit";
        beginEditButton.Click += beginEditButton_Click;
        // 
        // commitEditButton
        // 
        commitEditButton.Enabled = false;
        commitEditButton.Name = "commitEditButton";
        commitEditButton.AutoSize = true;
        commitEditButton.Height = 28;
        commitEditButton.Margin = new Padding(0, 3, 4, 3);
        commitEditButton.Padding = new Padding(8, 0, 8, 0);
        commitEditButton.UseVisualStyleBackColor = true;
        commitEditButton.Size = new Size(78, 29);
        commitEditButton.Text = "Commit Edit";
        commitEditButton.Click += commitEditButton_Click;
        // 
        // rollbackEditButton
        // 
        rollbackEditButton.Enabled = false;
        rollbackEditButton.Name = "rollbackEditButton";
        rollbackEditButton.AutoSize = true;
        rollbackEditButton.Height = 28;
        rollbackEditButton.Margin = new Padding(0, 3, 4, 3);
        rollbackEditButton.Padding = new Padding(8, 0, 8, 0);
        rollbackEditButton.UseVisualStyleBackColor = true;
        rollbackEditButton.Size = new Size(79, 29);
        rollbackEditButton.Text = "Rollback Edit";
        rollbackEditButton.Click += rollbackEditButton_Click;
        // 
        // editSeparator
        // 
        editSeparator.BackColor = SystemColors.ControlDark;
        editSeparator.Margin = new Padding(4, 5, 6, 5);
        editSeparator.Name = "editSeparator";
        editSeparator.Size = new Size(1, 26);
        // 
        // selectButton
        // 
        selectButton.Checked = true;
        selectButton.CheckState = CheckState.Checked;
        selectButton.Name = "selectButton";
        selectButton.Appearance = Appearance.Button;
        selectButton.AutoSize = true;
        selectButton.Height = 28;
        selectButton.Margin = new Padding(0, 3, 4, 3);
        selectButton.Padding = new Padding(8, 0, 8, 0);
        selectButton.TextAlign = ContentAlignment.MiddleCenter;
        selectButton.UseVisualStyleBackColor = true;
        selectButton.Size = new Size(42, 29);
        selectButton.Text = "Select";
        selectButton.Click += selectButton_Click;
        // 
        // clearSelectionButton
        // 
        clearSelectionButton.Enabled = false;
        clearSelectionButton.Name = "clearSelectionButton";
        clearSelectionButton.AutoSize = true;
        clearSelectionButton.Height = 28;
        clearSelectionButton.Margin = new Padding(0, 3, 4, 3);
        clearSelectionButton.Padding = new Padding(8, 0, 8, 0);
        clearSelectionButton.UseVisualStyleBackColor = true;
        clearSelectionButton.Size = new Size(89, 29);
        clearSelectionButton.Text = "Clear Selection";
        clearSelectionButton.Click += clearSelectionButton_Click;
        // 
        // resetSeparator
        // 
        resetSeparator.BackColor = SystemColors.ControlDark;
        resetSeparator.Margin = new Padding(4, 5, 6, 5);
        resetSeparator.Name = "resetSeparator";
        resetSeparator.Size = new Size(1, 26);
        // 
        // resetButton
        // 
        resetButton.Name = "resetButton";
        resetButton.AutoSize = true;
        resetButton.Height = 28;
        resetButton.Margin = new Padding(0, 3, 4, 3);
        resetButton.Padding = new Padding(8, 0, 8, 0);
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Size = new Size(75, 29);
        resetButton.Text = "Reset Points";
        resetButton.Click += resetButton_Click;
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
        stateLabel.Size = new Size(136, 29);
        stateLabel.Text = "Editing: OFF | Selected: 0";
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
        splitContainer.Panel2.Controls.Add(sidePanel);
        splitContainer.Size = new Size(1200, 744);
        splitContainer.SplitterDistance = 801;
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
        geoKernelViewerControl.Size = new Size(801, 744);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // sidePanel
        // 
        sidePanel.ColumnCount = 1;
        sidePanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        sidePanel.Controls.Add(checkListView, 0, 0);
        sidePanel.Controls.Add(selectionTextBox, 0, 1);
        sidePanel.Controls.Add(featureListView, 0, 2);
        sidePanel.Dock = DockStyle.Fill;
        sidePanel.Location = new Point(0, 0);
        sidePanel.Margin = new Padding(0);
        sidePanel.Name = "sidePanel";
        sidePanel.RowCount = 3;
        sidePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 140F));
        sidePanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 150F));
        sidePanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        sidePanel.Size = new Size(398, 744);
        sidePanel.TabIndex = 0;
        // 
        // checkListView
        // 
        checkListView.Dock = DockStyle.Fill;
        checkListView.FullRowSelect = true;
        checkListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        checkListView.Location = new Point(0, 0);
        checkListView.Margin = new Padding(0);
        checkListView.Name = "checkListView";
        checkListView.Size = new Size(398, 140);
        checkListView.TabIndex = 0;
        checkListView.UseCompatibleStateImageBehavior = false;
        checkListView.View = View.Details;
        // 
        // selectionTextBox
        // 
        selectionTextBox.BackColor = Color.White;
        selectionTextBox.Dock = DockStyle.Fill;
        selectionTextBox.Location = new Point(0, 140);
        selectionTextBox.Margin = new Padding(0);
        selectionTextBox.Multiline = true;
        selectionTextBox.Name = "selectionTextBox";
        selectionTextBox.ReadOnly = true;
        selectionTextBox.ScrollBars = ScrollBars.Vertical;
        selectionTextBox.Size = new Size(398, 150);
        selectionTextBox.TabIndex = 1;
        // 
        // featureListView
        // 
        featureListView.Dock = DockStyle.Fill;
        featureListView.FullRowSelect = true;
        featureListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        featureListView.Location = new Point(0, 290);
        featureListView.Margin = new Padding(0);
        featureListView.Name = "featureListView";
        featureListView.Size = new Size(398, 454);
        featureListView.TabIndex = 2;
        featureListView.UseCompatibleStateImageBehavior = false;
        featureListView.View = View.Details;
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
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CanEditCheck";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        sidePanel.ResumeLayout(false);
        sidePanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
