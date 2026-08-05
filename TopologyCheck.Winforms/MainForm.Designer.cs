using GeoKernel.NET.WinForms;

namespace GeoKernel.TopologyCheck.Winforms;

public sealed partial class MainForm
{
    private GeoKernelViewerControl geoKernelViewerControl = null!;
    private FlowLayoutPanel toolbarPanel = null!;
    private Button fullExtentButton = null!;
    private Button runCheckButton = null!;
    private Label operationLabel = null!;
    private SplitContainer splitContainer = null!;
    private TextBox detailsTextBox = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel statusLabel = null!;

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        geoKernelViewerControl = new GeoKernelViewerControl();
        toolbarPanel = new FlowLayoutPanel();
        fullExtentButton = new Button();
        operationLabel = new Label();
        runCheckButton = new Button();
        splitContainer = new SplitContainer();
        detailsTextBox = new TextBox();
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
        toolbarPanel.Controls.AddRange(new Control[] { fullExtentButton, operationLabel, runCheckButton });
        toolbarPanel.Size = new Size(980, 32);
        toolbarPanel.TabIndex = 0;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(65, 22);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // operationLabel
        // 
        operationLabel.Name = "operationLabel";
        operationLabel.Size = new Size(149, 22);
        operationLabel.Text = "Operation: CheckShape";
        // 
        // runCheckButton
        // 
        runCheckButton.Name = "runCheckButton";
        runCheckButton.Size = new Size(103, 22);
        runCheckButton.Text = "Run CheckShape";
        runCheckButton.Click += runCheckButton_Click;
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
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.Size = new Size(980, 633);
        splitContainer.SplitterDistance = 690;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(690, 633);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // detailsTextBox
        // 
        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Location = new Point(0, 0);
        detailsTextBox.Multiline = true;
        detailsTextBox.Name = "detailsTextBox";
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Size = new Size(286, 633);
        detailsTextBox.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 658);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(980, 22);
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(211, 17);
        statusLabel.Text = "Two polygons are ready. Click Run CheckShape.";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(980, 680);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "TopologyCheck";
        Shown += MainForm_Shown;
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
        PerformLayout();
    }
}
