using GeoKernel.NET.WinForms;

namespace GeoKernel.MoveFeatureTool.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private CheckBox selectButton;
    private CheckBox moveButton;
    private Button resetButton;
    private Button fullExtentButton;
    private Label countLabel;
    private SplitContainer splitContainer;
    private ListView featureListView;
    private GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            components?.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        toolbarPanel = new FlowLayoutPanel();
        selectButton = new CheckBox();
        moveButton = new CheckBox();
        resetButton = new Button();
        fullExtentButton = new Button();
        countLabel = new Label();
        splitContainer = new SplitContainer();
        featureListView = new ListView();
        geoKernelViewerControl = new GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
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
        toolbarPanel.Controls.AddRange(new Control[] { selectButton, moveButton, resetButton, fullExtentButton, countLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
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
        selectButton.Size = new Size(42, 22);
        selectButton.Text = "Select";
        selectButton.Click += selectButton_Click;
        // 
        // moveButton
        // 
        moveButton.Name = "moveButton";
        moveButton.Appearance = Appearance.Button;
        moveButton.AutoSize = true;
        moveButton.Height = 28;
        moveButton.Margin = new Padding(0, 3, 4, 3);
        moveButton.Padding = new Padding(8, 0, 8, 0);
        moveButton.TextAlign = ContentAlignment.MiddleCenter;
        moveButton.UseVisualStyleBackColor = true;
        moveButton.Size = new Size(84, 22);
        moveButton.Text = "Move Feature";
        moveButton.Click += moveButton_Click;
        // 
        // resetButton
        // 
        resetButton.Name = "resetButton";
        resetButton.AutoSize = true;
        resetButton.Height = 28;
        resetButton.Margin = new Padding(0, 3, 4, 3);
        resetButton.Padding = new Padding(8, 0, 8, 0);
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Size = new Size(73, 22);
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
        fullExtentButton.Size = new Size(65, 22);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // countLabel
        // 
        countLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        countLabel.Name = "countLabel";
        countLabel.AutoSize = true;
        countLabel.Margin = new Padding(8, 9, 8, 0);
        countLabel.TextAlign = ContentAlignment.MiddleLeft;
        countLabel.Size = new Size(161, 22);
        countLabel.Text = "Feature count: 0 | Selected: 0";
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel1;
        splitContainer.Location = new Point(0, 25);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(featureListView);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(geoKernelViewerControl);
        splitContainer.Size = new Size(1184, 714);
        splitContainer.SplitterDistance = 300;
        splitContainer.TabIndex = 1;
        // 
        // featureListView
        // 
        featureListView.Columns.Add("ID", 48);
        featureListView.Columns.Add("Name", 90);
        featureListView.Columns.Add("Group", 70);
        featureListView.Columns.Add("X, Y", 110);
        featureListView.Dock = DockStyle.Fill;
        featureListView.FullRowSelect = true;
        featureListView.GridLines = true;
        featureListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        featureListView.HideSelection = false;
        featureListView.MultiSelect = false;
        featureListView.Name = "featureListView";
        featureListView.Size = new Size(300, 714);
        featureListView.TabIndex = 0;
        featureListView.UseCompatibleStateImageBehavior = false;
        featureListView.View = View.Details;
        featureListView.SelectedIndexChanged += featureListView_SelectedIndexChanged;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Select;
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(880, 714);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
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
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MoveFeatureTool";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
