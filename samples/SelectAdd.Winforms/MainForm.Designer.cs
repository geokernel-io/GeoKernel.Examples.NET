using GeoKernel.NET.WinForms;

namespace GeoKernel.SelectAdd.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private CheckBox addButton;
    private CheckBox toggleButton;
    private CheckBox panButton;
    private Button clearButton;
    private Button fullExtentButton;
    private Label toolStateLabel;
    private SplitContainer splitContainer;
    private GeoKernelViewerControl geoKernelViewerControl;
    private DataGridView detailsGrid;
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        toolbarPanel = new FlowLayoutPanel();
        addButton = new CheckBox();
        toggleButton = new CheckBox();
        panButton = new CheckBox();
        clearButton = new Button();
        fullExtentButton = new Button();
        toolStateLabel = new Label();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernelViewerControl();
        detailsGrid = new DataGridView();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)detailsGrid).BeginInit();
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
        toolbarPanel.Controls.AddRange(new Control[] { addButton, toggleButton, panButton, clearButton, fullExtentButton, toolStateLabel });
        toolbarPanel.Size = new Size(1200, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // addButton
        // 
        addButton.Checked = true;
        addButton.CheckState = CheckState.Checked;
        addButton.Name = "addButton";
        addButton.Appearance = Appearance.Button;
        addButton.AutoSize = true;
        addButton.Height = 28;
        addButton.Margin = new Padding(0, 3, 4, 3);
        addButton.Padding = new Padding(8, 0, 8, 0);
        addButton.TextAlign = ContentAlignment.MiddleCenter;
        addButton.UseVisualStyleBackColor = true;
        addButton.Size = new Size(67, 22);
        addButton.Text = "Add Select";
        addButton.Click += addButton_Click;
        // 
        // toggleButton
        // 
        toggleButton.Name = "toggleButton";
        toggleButton.Appearance = Appearance.Button;
        toggleButton.AutoSize = true;
        toggleButton.Height = 28;
        toggleButton.Margin = new Padding(0, 3, 4, 3);
        toggleButton.Padding = new Padding(8, 0, 8, 0);
        toggleButton.TextAlign = ContentAlignment.MiddleCenter;
        toggleButton.UseVisualStyleBackColor = true;
        toggleButton.Size = new Size(80, 22);
        toggleButton.Text = "Toggle Select";
        toggleButton.Click += toggleButton_Click;
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
        // clearButton
        // 
        clearButton.Name = "clearButton";
        clearButton.AutoSize = true;
        clearButton.Height = 28;
        clearButton.Margin = new Padding(0, 3, 4, 3);
        clearButton.Padding = new Padding(8, 0, 8, 0);
        clearButton.UseVisualStyleBackColor = true;
        clearButton.Size = new Size(89, 22);
        clearButton.Text = "Clear Selection";
        clearButton.Click += clearButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.AutoSize = true;
        fullExtentButton.Height = 28;
        fullExtentButton.Margin = new Padding(0, 3, 4, 3);
        fullExtentButton.Padding = new Padding(8, 0, 8, 0);
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Size = new Size(66, 22);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // toolStateLabel
        // 
        toolStateLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        toolStateLabel.Margin = new Padding(12, 1, 0, 2);
        toolStateLabel.Name = "toolStateLabel";
        toolStateLabel.AutoSize = true;
        toolStateLabel.Margin = new Padding(8, 9, 8, 0);
        toolStateLabel.TextAlign = ContentAlignment.MiddleLeft;
        toolStateLabel.Size = new Size(159, 22);
        toolStateLabel.Text = "Click = addSelectedFeature";
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.Location = new Point(0, 25);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(detailsGrid);
        splitContainer.Size = new Size(1184, 714);
        splitContainer.SplitterDistance = 834;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(834, 714);
        geoKernelViewerControl.TabIndex = 0;
        geoKernelViewerControl.MouseClick += geoKernelViewerControl_MouseClick;
        // 
        // detailsGrid
        // 
        detailsGrid.AllowUserToAddRows = false;
        detailsGrid.AllowUserToDeleteRows = false;
        detailsGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        detailsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        detailsGrid.Dock = DockStyle.Fill;
        detailsGrid.Location = new Point(0, 0);
        detailsGrid.MultiSelect = false;
        detailsGrid.Name = "detailsGrid";
        detailsGrid.ReadOnly = true;
        detailsGrid.RowHeadersVisible = false;
        detailsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        detailsGrid.Size = new Size(346, 714);
        detailsGrid.TabIndex = 0;
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
        Text = "SelectAdd";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)detailsGrid).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}

