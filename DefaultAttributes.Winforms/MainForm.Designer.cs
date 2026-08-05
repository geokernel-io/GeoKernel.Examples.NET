using GeoKernel.NET.WinForms;

namespace GeoKernel.DefaultAttributes.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel toolbarPanel;
    private Button addPointButton;
    private Button infoButton;
    private Button clearPointsButton;
    private Button fullExtentButton;
    private Label pointCountLabel;
    private SplitContainer mainSplitContainer;
    private SplitContainer mapSplitContainer;
    private GeoKernelViewerControl geoKernelViewerControl;
    private TextBox infoTextBox;
    private DataGridView attributesGrid;
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
        toolbarPanel = new Panel();
        addPointButton = new Button();
        infoButton = new Button();
        clearPointsButton = new Button();
        fullExtentButton = new Button();
        pointCountLabel = new Label();
        mainSplitContainer = new SplitContainer();
        mapSplitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernelViewerControl();
        infoTextBox = new TextBox();
        attributesGrid = new DataGridView();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).BeginInit();
        mainSplitContainer.Panel1.SuspendLayout();
        mainSplitContainer.Panel2.SuspendLayout();
        mainSplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)mapSplitContainer).BeginInit();
        mapSplitContainer.Panel1.SuspendLayout();
        mapSplitContainer.Panel2.SuspendLayout();
        mapSplitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)attributesGrid).BeginInit();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = Color.FromArgb(242, 242, 242);
        toolbarPanel.Controls.Add(addPointButton);
        toolbarPanel.Controls.Add(infoButton);
        toolbarPanel.Controls.Add(clearPointsButton);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(pointCountLabel);
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1184, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // addPointButton
        // 
        addPointButton.FlatStyle = FlatStyle.Flat;
        addPointButton.FlatAppearance.BorderSize = 0;
        addPointButton.Location = new Point(4, 6);
        addPointButton.Name = "addPointButton";
        addPointButton.Size = new Size(157, 27);
        addPointButton.TabIndex = 0;
        addPointButton.Text = "Add Point With Attributes";
        addPointButton.UseVisualStyleBackColor = true;
        addPointButton.Click += addPointButton_Click;
        // 
        // infoButton
        // 
        infoButton.FlatStyle = FlatStyle.Flat;
        infoButton.FlatAppearance.BorderSize = 0;
        infoButton.Location = new Point(167, 6);
        infoButton.Name = "infoButton";
        infoButton.Size = new Size(49, 27);
        infoButton.TabIndex = 1;
        infoButton.Text = "Info";
        infoButton.UseVisualStyleBackColor = true;
        infoButton.Click += infoButton_Click;
        // 
        // clearPointsButton
        // 
        clearPointsButton.FlatStyle = FlatStyle.Flat;
        clearPointsButton.FlatAppearance.BorderSize = 0;
        clearPointsButton.Location = new Point(222, 6);
        clearPointsButton.Name = "clearPointsButton";
        clearPointsButton.Size = new Size(91, 27);
        clearPointsButton.TabIndex = 2;
        clearPointsButton.Text = "Clear Points";
        clearPointsButton.UseVisualStyleBackColor = true;
        clearPointsButton.Click += clearPointsButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.Location = new Point(319, 6);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(82, 27);
        fullExtentButton.TabIndex = 3;
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // pointCountLabel
        // 
        pointCountLabel.AutoSize = true;
        pointCountLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        pointCountLabel.Location = new Point(414, 12);
        pointCountLabel.Name = "pointCountLabel";
        pointCountLabel.Size = new Size(96, 15);
        pointCountLabel.TabIndex = 4;
        pointCountLabel.Text = "Feature count: 0";
        // 
        // mainSplitContainer
        // 
        mainSplitContainer.Dock = DockStyle.Fill;
        mainSplitContainer.FixedPanel = FixedPanel.Panel2;
        mainSplitContainer.Location = new Point(0, 39);
        mainSplitContainer.Name = "mainSplitContainer";
        mainSplitContainer.Orientation = Orientation.Horizontal;
        // 
        // mainSplitContainer.Panel1
        // 
        mainSplitContainer.Panel1.Controls.Add(mapSplitContainer);
        // 
        // mainSplitContainer.Panel2
        // 
        mainSplitContainer.Panel2.Controls.Add(attributesGrid);
        mainSplitContainer.Size = new Size(1184, 700);
        mainSplitContainer.SplitterDistance = 532;
        mainSplitContainer.TabIndex = 1;
        // 
        // mapSplitContainer
        // 
        mapSplitContainer.Dock = DockStyle.Fill;
        mapSplitContainer.FixedPanel = FixedPanel.Panel2;
        mapSplitContainer.Location = new Point(0, 0);
        mapSplitContainer.Name = "mapSplitContainer";
        // 
        // mapSplitContainer.Panel1
        // 
        mapSplitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // mapSplitContainer.Panel2
        // 
        mapSplitContainer.Panel2.Controls.Add(infoTextBox);
        mapSplitContainer.Size = new Size(1184, 532);
        mapSplitContainer.SplitterDistance = 836;
        mapSplitContainer.TabIndex = 0;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(836, 532);
        geoKernelViewerControl.TabIndex = 0;
        geoKernelViewerControl.MapMouseUp += geoKernelViewerControl_MapMouseUp;
        // 
        // infoTextBox
        // 
        infoTextBox.Dock = DockStyle.Fill;
        infoTextBox.Location = new Point(0, 0);
        infoTextBox.Multiline = true;
        infoTextBox.Name = "infoTextBox";
        infoTextBox.ReadOnly = true;
        infoTextBox.ScrollBars = ScrollBars.Vertical;
        infoTextBox.Size = new Size(344, 532);
        infoTextBox.TabIndex = 0;
        // 
        // attributesGrid
        // 
        attributesGrid.AllowUserToAddRows = false;
        attributesGrid.AllowUserToDeleteRows = false;
        attributesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        attributesGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        attributesGrid.Dock = DockStyle.Fill;
        attributesGrid.Location = new Point(0, 0);
        attributesGrid.MultiSelect = false;
        attributesGrid.Name = "attributesGrid";
        attributesGrid.ReadOnly = true;
        attributesGrid.RowHeadersVisible = false;
        attributesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        attributesGrid.Size = new Size(1184, 164);
        attributesGrid.TabIndex = 0;
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
        statusLabel.Size = new Size(42, 17);
        statusLabel.Text = "Ready.";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1184, 761);
        Controls.Add(mainSplitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(900, 600);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "DefaultAttributes";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        mainSplitContainer.Panel1.ResumeLayout(false);
        mainSplitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)mainSplitContainer).EndInit();
        mainSplitContainer.ResumeLayout(false);
        mapSplitContainer.Panel1.ResumeLayout(false);
        mapSplitContainer.Panel2.ResumeLayout(false);
        mapSplitContainer.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)mapSplitContainer).EndInit();
        mapSplitContainer.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)attributesGrid).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
