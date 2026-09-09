using GeoKernel.NET.WinForms;

namespace GeoKernel.UndoRedo.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private CheckBox addPointButton;
    private CheckBox panButton;
    private Button undoButton;
    private Button redoButton;
    private Button undoFiveButton;
    private Button redoFiveButton;
    private Button resetButton;
    private Button fullExtentButton;
    private Label stateLabel;
    private SplitContainer splitContainer;
    private TextBox infoTextBox;
    private GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        toolbarPanel = new FlowLayoutPanel();
        addPointButton = new CheckBox();
        panButton = new CheckBox();
        undoButton = new Button();
        redoButton = new Button();
        undoFiveButton = new Button();
        redoFiveButton = new Button();
        resetButton = new Button();
        fullExtentButton = new Button();
        stateLabel = new Label();
        splitContainer = new SplitContainer();
        infoTextBox = new TextBox();
        geoKernelViewerControl = new GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
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
        toolbarPanel.Controls.AddRange(new Control[] { addPointButton, panButton, undoButton, redoButton, undoFiveButton, redoFiveButton, resetButton, fullExtentButton, stateLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
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
        addPointButton.Size = new Size(63, 22);
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
        panButton.Size = new Size(31, 22);
        panButton.Text = "Pan";
        panButton.Click += panButton_Click;
        // 
        // undoButton
        // 
        undoButton.Name = "undoButton";
        undoButton.AutoSize = true;
        undoButton.Height = 28;
        undoButton.Margin = new Padding(0, 3, 4, 3);
        undoButton.Padding = new Padding(8, 0, 8, 0);
        undoButton.UseVisualStyleBackColor = true;
        undoButton.Size = new Size(40, 22);
        undoButton.Text = "Undo";
        undoButton.Click += undoButton_Click;
        // 
        // redoButton
        // 
        redoButton.Name = "redoButton";
        redoButton.AutoSize = true;
        redoButton.Height = 28;
        redoButton.Margin = new Padding(0, 3, 4, 3);
        redoButton.Padding = new Padding(8, 0, 8, 0);
        redoButton.UseVisualStyleBackColor = true;
        redoButton.Size = new Size(38, 22);
        redoButton.Text = "Redo";
        redoButton.Click += redoButton_Click;
        // 
        // undoFiveButton
        // 
        undoFiveButton.Name = "undoFiveButton";
        undoFiveButton.AutoSize = true;
        undoFiveButton.Height = 28;
        undoFiveButton.Margin = new Padding(0, 3, 4, 3);
        undoFiveButton.Padding = new Padding(8, 0, 8, 0);
        undoFiveButton.UseVisualStyleBackColor = true;
        undoFiveButton.Size = new Size(52, 22);
        undoFiveButton.Text = "Undo 5";
        undoFiveButton.Click += undoFiveButton_Click;
        // 
        // redoFiveButton
        // 
        redoFiveButton.Name = "redoFiveButton";
        redoFiveButton.AutoSize = true;
        redoFiveButton.Height = 28;
        redoFiveButton.Margin = new Padding(0, 3, 4, 3);
        redoFiveButton.Padding = new Padding(8, 0, 8, 0);
        redoFiveButton.UseVisualStyleBackColor = true;
        redoFiveButton.Size = new Size(50, 22);
        redoFiveButton.Text = "Redo 5";
        redoFiveButton.Click += redoFiveButton_Click;
        // 
        // resetButton
        // 
        resetButton.Name = "resetButton";
        resetButton.AutoSize = true;
        resetButton.Height = 28;
        resetButton.Margin = new Padding(0, 3, 4, 3);
        resetButton.Padding = new Padding(8, 0, 8, 0);
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Size = new Size(39, 22);
        resetButton.Text = "Reset";
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
        fullExtentButton.Size = new Size(65, 22);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // stateLabel
        // 
        stateLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        stateLabel.Name = "stateLabel";
        stateLabel.AutoSize = true;
        stateLabel.Margin = new Padding(8, 9, 8, 0);
        stateLabel.TextAlign = ContentAlignment.MiddleLeft;
        stateLabel.Size = new Size(161, 22);
        stateLabel.Text = "Points: 0 | Undo: no | Redo: no";
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel1;
        splitContainer.Location = new Point(0, 25);
        splitContainer.Name = "splitContainer";
        splitContainer.Panel1.Controls.Add(infoTextBox);
        splitContainer.Panel2.Controls.Add(geoKernelViewerControl);
        splitContainer.Size = new Size(1184, 714);
        splitContainer.SplitterDistance = 360;
        splitContainer.TabIndex = 1;
        // 
        // infoTextBox
        // 
        infoTextBox.Dock = DockStyle.Fill;
        infoTextBox.Location = new Point(0, 0);
        infoTextBox.Multiline = true;
        infoTextBox.Name = "infoTextBox";
        infoTextBox.ReadOnly = true;
        infoTextBox.ScrollBars = ScrollBars.Vertical;
        infoTextBox.Size = new Size(360, 714);
        infoTextBox.TabIndex = 0;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.AddPoint;
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(820, 714);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar });
        downloadProgressBar.Size = new Size(180, 18);
        downloadProgressBar.Visible = false;
        statusStrip.Location = new Point(0, 739);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1184, 22);
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(39, 17);
        statusLabel.Text = "Ready.";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 761);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        MinimumSize = new Size(900, 600);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "UndoRedo";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel1.PerformLayout();
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
