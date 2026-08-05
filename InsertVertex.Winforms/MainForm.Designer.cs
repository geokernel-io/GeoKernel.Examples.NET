using GeoKernel.NET.WinForms;

namespace GeoKernel.InsertVertex.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private CheckBox panButton;
    private CheckBox selectButton;
    private Label partLabel;
    private NumericUpDown partNumeric;
    private Label insertIndexLabel;
    private NumericUpDown insertIndexNumeric;
    private Button insertVertexButton;
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
        partLabel = new Label();
        partNumeric = new NumericUpDown();
        insertIndexLabel = new Label();
        insertIndexNumeric = new NumericUpDown();
        insertVertexButton = new Button();
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
        ((System.ComponentModel.ISupportInitialize)insertIndexNumeric).BeginInit();
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
        toolbarPanel.Controls.AddRange(new Control[] { panButton, selectButton, partLabel, partNumeric, insertIndexLabel, insertIndexNumeric, insertVertexButton, resetButton, fullExtentButton, countLabel });
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
        // partLabel
        // 
        partLabel.Name = "partLabel";
        partLabel.AutoSize = true;
        partLabel.Margin = new Padding(8, 9, 8, 0);
        partLabel.TextAlign = ContentAlignment.MiddleLeft;
        partLabel.Size = new Size(30, 22);
        partLabel.Text = "Part";
        // 
        // partNumeric
        // 
        partNumeric.Maximum = new decimal(new int[] { 0, 0, 0, 0 });
        partNumeric.Name = "partNumeric";
        partNumeric.Margin = new Padding(0, 4, 8, 0);
        partNumeric.Size = new Size(46, 23);
        partNumeric.ValueChanged += partNumeric_ValueChanged;
        // 
        // 
        // insertIndexLabel
        // 
        insertIndexLabel.Name = "insertIndexLabel";
        insertIndexLabel.AutoSize = true;
        insertIndexLabel.Margin = new Padding(8, 9, 8, 0);
        insertIndexLabel.TextAlign = ContentAlignment.MiddleLeft;
        insertIndexLabel.Size = new Size(68, 22);
        insertIndexLabel.Text = "Insert index";
        // 
        // insertIndexNumeric
        // 
        insertIndexNumeric.Maximum = new decimal(new int[] { 6, 0, 0, 0 });
        insertIndexNumeric.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        insertIndexNumeric.Name = "insertIndexNumeric";
        insertIndexNumeric.Margin = new Padding(0, 4, 8, 0);
        insertIndexNumeric.Size = new Size(54, 23);
        insertIndexNumeric.Value = new decimal(new int[] { 2, 0, 0, 0 });
        insertIndexNumeric.ValueChanged += insertIndexNumeric_ValueChanged;
        // 
        // 
        // insertVertexButton
        // 
        insertVertexButton.Name = "insertVertexButton";
        insertVertexButton.AutoSize = true;
        insertVertexButton.Height = 28;
        insertVertexButton.Margin = new Padding(0, 3, 4, 3);
        insertVertexButton.Padding = new Padding(8, 0, 8, 0);
        insertVertexButton.UseVisualStyleBackColor = true;
        insertVertexButton.Size = new Size(75, 22);
        insertVertexButton.Text = "Insert Vertex";
        insertVertexButton.Click += insertVertexButton_Click;
        // 
        // resetButton
        // 
        resetButton.Name = "resetButton";
        resetButton.AutoSize = true;
        resetButton.Height = 28;
        resetButton.Margin = new Padding(0, 3, 4, 3);
        resetButton.Padding = new Padding(8, 0, 8, 0);
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Size = new Size(76, 22);
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
        countLabel.Size = new Size(114, 22);
        countLabel.Text = "Vertex count: 0";
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
        geoKernelViewerControl.ActiveTool = GeoKernelViewerTool.Select;
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
        Text = "InsertVertex";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)partNumeric).EndInit();
        ((System.ComponentModel.ISupportInitialize)insertIndexNumeric).EndInit();
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
