using GeoKernel.NET.WinForms;

namespace GeoKernel.DeleteVertex.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private CheckBox panButton;
    private CheckBox selectButton;
    private CheckBox editVerticesButton;
    private Button deleteSelectedVertexButton;
    private Label partLabel;
    private NumericUpDown partNumeric;
    private Label vertexIndexLabel;
    private NumericUpDown vertexIndexNumeric;
    private Button deleteByIndexButton;
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
        selectButton = new CheckBox();
        editVerticesButton = new CheckBox();
        deleteSelectedVertexButton = new Button();
        partLabel = new Label();
        partNumeric = new NumericUpDown();
        vertexIndexLabel = new Label();
        vertexIndexNumeric = new NumericUpDown();
        deleteByIndexButton = new Button();
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
        ((System.ComponentModel.ISupportInitialize)partNumeric).BeginInit();
        ((System.ComponentModel.ISupportInitialize)vertexIndexNumeric).BeginInit();
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
        toolbarPanel.Controls.AddRange(new Control[] { panButton, selectButton, editVerticesButton, deleteSelectedVertexButton, partLabel, partNumeric, vertexIndexLabel, vertexIndexNumeric, deleteByIndexButton, resetButton, fullExtentButton, countLabel });
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
        panButton.Size = new Size(31, 23);
        panButton.Text = "Pan";
        panButton.Click += panButton_Click;
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
        selectButton.Size = new Size(42, 23);
        selectButton.Text = "Select";
        selectButton.Click += selectButton_Click;
        // 
        // editVerticesButton
        // 
        editVerticesButton.Name = "editVerticesButton";
        editVerticesButton.Appearance = Appearance.Button;
        editVerticesButton.AutoSize = true;
        editVerticesButton.Height = 28;
        editVerticesButton.Margin = new Padding(0, 3, 4, 3);
        editVerticesButton.Padding = new Padding(8, 0, 8, 0);
        editVerticesButton.TextAlign = ContentAlignment.MiddleCenter;
        editVerticesButton.UseVisualStyleBackColor = true;
        editVerticesButton.Size = new Size(74, 23);
        editVerticesButton.Text = "Edit Vertices";
        editVerticesButton.Click += editVerticesButton_Click;
        // 
        // deleteSelectedVertexButton
        // 
        deleteSelectedVertexButton.Name = "deleteSelectedVertexButton";
        deleteSelectedVertexButton.AutoSize = true;
        deleteSelectedVertexButton.Height = 28;
        deleteSelectedVertexButton.Margin = new Padding(0, 3, 4, 3);
        deleteSelectedVertexButton.Padding = new Padding(8, 0, 8, 0);
        deleteSelectedVertexButton.UseVisualStyleBackColor = true;
        deleteSelectedVertexButton.Size = new Size(126, 23);
        deleteSelectedVertexButton.Text = "Delete Selected Vertex";
        deleteSelectedVertexButton.Click += deleteSelectedVertexButton_Click;
        // 
        // partLabel
        // 
        partLabel.Name = "partLabel";
        partLabel.AutoSize = true;
        partLabel.Margin = new Padding(8, 9, 8, 0);
        partLabel.TextAlign = ContentAlignment.MiddleLeft;
        partLabel.Size = new Size(28, 23);
        partLabel.Text = "Part";
        // 
        // partNumeric
        // 
        partNumeric.AccessibleName = "partNumeric";
        partNumeric.Location = new Point(301, 1);
        partNumeric.Maximum = new decimal(new int[] { 0, 0, 0, 0 });
        partNumeric.Name = "partNumeric";
        partNumeric.Margin = new Padding(0, 4, 8, 0);
        partNumeric.Size = new Size(48, 23);
        partNumeric.TabIndex = 0;
        partNumeric.ValueChanged += partNumeric_ValueChanged;
        // 
        // 
        // vertexIndexLabel
        // 
        vertexIndexLabel.Name = "vertexIndexLabel";
        vertexIndexLabel.AutoSize = true;
        vertexIndexLabel.Margin = new Padding(8, 9, 8, 0);
        vertexIndexLabel.TextAlign = ContentAlignment.MiddleLeft;
        vertexIndexLabel.Size = new Size(71, 23);
        vertexIndexLabel.Text = "Vertex index";
        // 
        // vertexIndexNumeric
        // 
        vertexIndexNumeric.AccessibleName = "vertexIndexNumeric";
        vertexIndexNumeric.Location = new Point(401, 1);
        vertexIndexNumeric.Maximum = new decimal(new int[] { 5, 0, 0, 0 });
        vertexIndexNumeric.Name = "vertexIndexNumeric";
        vertexIndexNumeric.Margin = new Padding(0, 4, 8, 0);
        vertexIndexNumeric.Size = new Size(48, 23);
        vertexIndexNumeric.TabIndex = 1;
        vertexIndexNumeric.Value = new decimal(new int[] { 2, 0, 0, 0 });
        vertexIndexNumeric.ValueChanged += vertexIndexNumeric_ValueChanged;
        // 
        // 
        // deleteByIndexButton
        // 
        deleteByIndexButton.Name = "deleteByIndexButton";
        deleteByIndexButton.AutoSize = true;
        deleteByIndexButton.Height = 28;
        deleteByIndexButton.Margin = new Padding(0, 3, 4, 3);
        deleteByIndexButton.Padding = new Padding(8, 0, 8, 0);
        deleteByIndexButton.UseVisualStyleBackColor = true;
        deleteByIndexButton.Size = new Size(92, 23);
        deleteByIndexButton.Text = "Delete By Index";
        deleteByIndexButton.Click += deleteByIndexButton_Click;
        // 
        // resetButton
        // 
        resetButton.Name = "resetButton";
        resetButton.AutoSize = true;
        resetButton.Height = 28;
        resetButton.Margin = new Padding(0, 3, 4, 3);
        resetButton.Padding = new Padding(8, 0, 8, 0);
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Size = new Size(74, 23);
        resetButton.Text = "Reset Shape";
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
        fullExtentButton.Size = new Size(66, 23);
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
        countLabel.Size = new Size(93, 23);
        countLabel.Text = "Vertex count: 0";
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.Location = new Point(0, 26);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(infoTextBox);
        splitContainer.Size = new Size(1184, 713);
        splitContainer.SplitterDistance = 820;
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
        infoTextBox.Size = new Size(360, 713);
        infoTextBox.TabIndex = 0;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(820, 713);
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
        statusLabel.Size = new Size(42, 17);
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
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(900, 600);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "DeleteVertex";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)partNumeric).EndInit();
        ((System.ComponentModel.ISupportInitialize)vertexIndexNumeric).EndInit();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        splitContainer.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
