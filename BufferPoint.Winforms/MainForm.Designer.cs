namespace GeoKernel.BufferPoint.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private Button fullExtentButton;
    private Label distanceLabel;
    private NumericUpDown distanceNumeric;
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
        toolbarPanel = new FlowLayoutPanel();
        fullExtentButton = new Button();
        distanceLabel = new Label();
        distanceNumeric = new NumericUpDown();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)distanceNumeric).BeginInit();
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
        toolbarPanel.Controls.AddRange(new Control[] { fullExtentButton, distanceLabel, distanceNumeric });
        toolbarPanel.Size = new Size(980, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.AutoSize = true;
        fullExtentButton.Height = 28;
        fullExtentButton.Margin = new Padding(0, 3, 4, 3);
        fullExtentButton.Padding = new Padding(8, 0, 8, 0);
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Size = new Size(63, 22);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // distanceLabel
        // 
        distanceLabel.Name = "distanceLabel";
        distanceLabel.AutoSize = true;
        distanceLabel.Margin = new Padding(8, 9, 8, 0);
        distanceLabel.TextAlign = ContentAlignment.MiddleLeft;
        distanceLabel.Size = new Size(55, 22);
        distanceLabel.Text = "Distance:";
        // 
        // distanceNumeric
        // 
        distanceNumeric.DecimalPlaces = 2;
        distanceNumeric.Increment = 0.25M;
        distanceNumeric.Maximum = 5M;
        distanceNumeric.Minimum = 0.25M;
        distanceNumeric.Name = "distanceNumeric";
        distanceNumeric.Margin = new Padding(0, 4, 8, 0);
        distanceNumeric.Size = new Size(90, 23);
        distanceNumeric.Value = 2M;
        distanceNumeric.ValueChanged += distanceNumeric_ValueChanged;
        // 
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
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
        splitContainer.SplitterDistance = 700;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(700, 633);
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
        detailsTextBox.Size = new Size(276, 633);
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
        Text = "BufferPoint";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)distanceNumeric).EndInit();
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
