namespace GeoKernel.ArcOperations.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel toolbarPanel;
    private Button fullExtentButton;
    private Label operationLabel;
    private Button runOperationsButton;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private TextBox detailsTextBox;
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
        components = new System.ComponentModel.Container();
        toolbarPanel = new Panel();
        fullExtentButton = new Button();
        operationLabel = new Label();
        runOperationsButton = new Button();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
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
        toolbarPanel.BackColor = Color.FromArgb(242, 242, 242);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(operationLabel);
        toolbarPanel.Controls.Add(runOperationsButton);
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(980, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // fullExtentButton
        // 
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.Location = new Point(4, 6);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(82, 27);
        fullExtentButton.TabIndex = 0;
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // operationLabel
        // 
        operationLabel.AutoSize = true;
        operationLabel.Location = new Point(100, 12);
        operationLabel.Name = "operationLabel";
        operationLabel.Size = new Size(321, 15);
        operationLabel.TabIndex = 1;
        operationLabel.Text = "Operations: ArcFind / ArcMakeConnected / ArcSplitOnCross";
        // 
        // runOperationsButton
        // 
        runOperationsButton.FlatStyle = FlatStyle.Flat;
        runOperationsButton.FlatAppearance.BorderSize = 0;
        runOperationsButton.Location = new Point(435, 6);
        runOperationsButton.Name = "runOperationsButton";
        runOperationsButton.Size = new Size(133, 27);
        runOperationsButton.TabIndex = 2;
        runOperationsButton.Text = "Run Arc Operations";
        runOperationsButton.UseVisualStyleBackColor = true;
        runOperationsButton.Click += runOperationsButton_Click;
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 39);
        splitContainer.Name = "splitContainer";
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.Size = new Size(980, 619);
        splitContainer.SplitterDistance = 690;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(690, 619);
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
        detailsTextBox.Size = new Size(286, 619);
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
        statusLabel.Size = new Size(0, 17);
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(980, 680);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "resources", "geokernel.ico"));
        MinimumSize = new Size(720, 480);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ArcOperations";
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
