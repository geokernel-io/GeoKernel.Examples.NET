using GeoKernel.NET.WinForms;

namespace GeoKernel.EditVerticesTool.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private CheckBox panButton;
    private CheckBox editVerticesButton;
    private Button deleteVertexButton;
    private Button resetButton;
    private Button fullExtentButton;
    private Label countLabel;
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
        panButton = new CheckBox();
        editVerticesButton = new CheckBox();
        deleteVertexButton = new Button();
        resetButton = new Button();
        fullExtentButton = new Button();
        countLabel = new Label();
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
        toolbarPanel.Controls.AddRange(new Control[] { panButton, editVerticesButton, deleteVertexButton, resetButton, fullExtentButton, countLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
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
        // editVerticesButton
        // 
        editVerticesButton.Checked = true;
        editVerticesButton.CheckState = CheckState.Checked;
        editVerticesButton.Name = "editVerticesButton";
        editVerticesButton.Appearance = Appearance.Button;
        editVerticesButton.AutoSize = true;
        editVerticesButton.Height = 28;
        editVerticesButton.Margin = new Padding(0, 3, 4, 3);
        editVerticesButton.Padding = new Padding(8, 0, 8, 0);
        editVerticesButton.TextAlign = ContentAlignment.MiddleCenter;
        editVerticesButton.UseVisualStyleBackColor = true;
        editVerticesButton.Size = new Size(76, 22);
        editVerticesButton.Text = "Edit Vertices";
        editVerticesButton.Click += editVerticesButton_Click;
        // 
        // deleteVertexButton
        // 
        deleteVertexButton.Name = "deleteVertexButton";
        deleteVertexButton.AutoSize = true;
        deleteVertexButton.Height = 28;
        deleteVertexButton.Margin = new Padding(0, 3, 4, 3);
        deleteVertexButton.Padding = new Padding(8, 0, 8, 0);
        deleteVertexButton.UseVisualStyleBackColor = true;
        deleteVertexButton.Size = new Size(79, 22);
        deleteVertexButton.Text = "Delete Vertex";
        deleteVertexButton.Click += deleteVertexButton_Click;
        // 
        // resetButton
        // 
        resetButton.Name = "resetButton";
        resetButton.AutoSize = true;
        resetButton.Height = 28;
        resetButton.Margin = new Padding(0, 3, 4, 3);
        resetButton.Padding = new Padding(8, 0, 8, 0);
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Size = new Size(79, 22);
        resetButton.Text = "Reset Shapes";
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
        countLabel.Size = new Size(196, 22);
        countLabel.Text = "Lines: 0 | Polygons: 0 | Selected: 0";
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
        splitContainer.SplitterDistance = 330;
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
        infoTextBox.Size = new Size(330, 714);
        infoTextBox.TabIndex = 0;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.EditVertices;
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
        KeyPreview = true;
        MinimumSize = new Size(900, 600);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EditVerticesTool";
        Shown += MainForm_Shown;
        KeyDown += MainForm_KeyDown;
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
