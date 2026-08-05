using GeoKernel.NET.WinForms;

namespace GeoKernel.MoveFeatureProgrammatic.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private CheckBox selectButton;
    private Label deltaCaptionLabel;
    private NumericUpDown deltaNumeric;
    private Button moveWestButton;
    private Button moveEastButton;
    private Button moveNorthButton;
    private Button moveSouthButton;
    private Button resetButton;
    private Button fullExtentButton;
    private Label countLabel;
    private SplitContainer splitContainer;
    private ListView featureListView;
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
        selectButton = new CheckBox();
        deltaCaptionLabel = new Label();
        deltaNumeric = new NumericUpDown();
        moveWestButton = new Button();
        moveEastButton = new Button();
        moveNorthButton = new Button();
        moveSouthButton = new Button();
        resetButton = new Button();
        fullExtentButton = new Button();
        countLabel = new Label();
        splitContainer = new SplitContainer();
        featureListView = new ListView();
        geoKernelViewerControl = new GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)deltaNumeric).BeginInit();
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
        toolbarPanel.Controls.AddRange(new Control[] { selectButton, deltaCaptionLabel, deltaNumeric, moveWestButton, moveEastButton, moveNorthButton, moveSouthButton, resetButton, fullExtentButton, countLabel });
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
        // deltaCaptionLabel
        // 
        deltaCaptionLabel.Name = "deltaCaptionLabel";
        deltaCaptionLabel.AutoSize = true;
        deltaCaptionLabel.Margin = new Padding(8, 9, 8, 0);
        deltaCaptionLabel.TextAlign = ContentAlignment.MiddleLeft;
        deltaCaptionLabel.Size = new Size(37, 22);
        deltaCaptionLabel.Text = "Delta:";
        // 
        // deltaNumeric
        // 
        deltaNumeric.DecimalPlaces = 2;
        deltaNumeric.Increment = new decimal(new int[] { 50, 0, 0, 131072 });
        deltaNumeric.Maximum = new decimal(new int[] { 30, 0, 0, 0 });
        deltaNumeric.Minimum = new decimal(new int[] { 10, 0, 0, 131072 });
        deltaNumeric.Name = "deltaNumeric";
        deltaNumeric.Margin = new Padding(0, 4, 8, 0);
        deltaNumeric.Size = new Size(70, 23);
        deltaNumeric.Value = new decimal(new int[] { 3, 0, 0, 0 });
        // 
        // 
        // moveWestButton
        // 
        moveWestButton.Name = "moveWestButton";
        moveWestButton.AutoSize = true;
        moveWestButton.Height = 28;
        moveWestButton.Margin = new Padding(0, 3, 4, 3);
        moveWestButton.Padding = new Padding(8, 0, 8, 0);
        moveWestButton.UseVisualStyleBackColor = true;
        moveWestButton.Size = new Size(38, 22);
        moveWestButton.Text = "West";
        moveWestButton.Click += moveWestButton_Click;
        // 
        // moveEastButton
        // 
        moveEastButton.Name = "moveEastButton";
        moveEastButton.AutoSize = true;
        moveEastButton.Height = 28;
        moveEastButton.Margin = new Padding(0, 3, 4, 3);
        moveEastButton.Padding = new Padding(8, 0, 8, 0);
        moveEastButton.UseVisualStyleBackColor = true;
        moveEastButton.Size = new Size(34, 22);
        moveEastButton.Text = "East";
        moveEastButton.Click += moveEastButton_Click;
        // 
        // moveNorthButton
        // 
        moveNorthButton.Name = "moveNorthButton";
        moveNorthButton.AutoSize = true;
        moveNorthButton.Height = 28;
        moveNorthButton.Margin = new Padding(0, 3, 4, 3);
        moveNorthButton.Padding = new Padding(8, 0, 8, 0);
        moveNorthButton.UseVisualStyleBackColor = true;
        moveNorthButton.Size = new Size(42, 22);
        moveNorthButton.Text = "North";
        moveNorthButton.Click += moveNorthButton_Click;
        // 
        // moveSouthButton
        // 
        moveSouthButton.Name = "moveSouthButton";
        moveSouthButton.AutoSize = true;
        moveSouthButton.Height = 28;
        moveSouthButton.Margin = new Padding(0, 3, 4, 3);
        moveSouthButton.Padding = new Padding(8, 0, 8, 0);
        moveSouthButton.UseVisualStyleBackColor = true;
        moveSouthButton.Size = new Size(42, 22);
        moveSouthButton.Text = "South";
        moveSouthButton.Click += moveSouthButton_Click;
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
        splitContainer.Panel1.Controls.Add(featureListView);
        splitContainer.Panel2.Controls.Add(geoKernelViewerControl);
        splitContainer.Size = new Size(1184, 714);
        splitContainer.SplitterDistance = 330;
        splitContainer.TabIndex = 1;
        // 
        // featureListView
        // 
        featureListView.Columns.Add("ID", 48);
        featureListView.Columns.Add("Name", 90);
        featureListView.Columns.Add("Group", 70);
        featureListView.Columns.Add("Current X, Y", 130);
        featureListView.Dock = DockStyle.Fill;
        featureListView.FullRowSelect = true;
        featureListView.GridLines = true;
        featureListView.HeaderStyle = ColumnHeaderStyle.Nonclickable;
        featureListView.HideSelection = false;
        featureListView.MultiSelect = false;
        featureListView.Name = "featureListView";
        featureListView.Size = new Size(330, 714);
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
        geoKernelViewerControl.Size = new Size(850, 714);
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
        Text = "MoveFeatureProgrammatic";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)deltaNumeric).EndInit();
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
