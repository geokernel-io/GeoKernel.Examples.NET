using GeoKernel.NET.WinForms;

namespace GeoKernel.AddPointProgrammatic.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel toolbarPanel;
    private Button addPointButton;
    private Button clearPointsButton;
    private Button fullExtentButton;
    private Label pointCountLabel;
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
        toolbarPanel = new Panel();
        addPointButton = new Button();
        clearPointsButton = new Button();
        fullExtentButton = new Button();
        pointCountLabel = new Label();
        geoKernelViewerControl = new GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        toolbarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(pointCountLabel);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(clearPointsButton);
        toolbarPanel.Controls.Add(addPointButton);
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1184, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // addPointButton
        // 
        addPointButton.BackColor = SystemColors.Control;
        addPointButton.FlatAppearance.BorderSize = 0;
        addPointButton.FlatStyle = FlatStyle.Flat;
        addPointButton.Location = new Point(0, 0);
        addPointButton.Margin = new Padding(0);
        addPointButton.Name = "addPointButton";
        addPointButton.Size = new Size(82, 36);
        addPointButton.TabIndex = 0;
        addPointButton.Text = "Add Point";
        addPointButton.UseVisualStyleBackColor = false;
        addPointButton.Click += addPointButton_Click;
        // 
        // clearPointsButton
        // 
        clearPointsButton.BackColor = SystemColors.Control;
        clearPointsButton.FlatAppearance.BorderSize = 0;
        clearPointsButton.FlatStyle = FlatStyle.Flat;
        clearPointsButton.Location = new Point(82, 0);
        clearPointsButton.Margin = new Padding(0);
        clearPointsButton.Name = "clearPointsButton";
        clearPointsButton.Size = new Size(90, 36);
        clearPointsButton.TabIndex = 1;
        clearPointsButton.Text = "Clear Points";
        clearPointsButton.UseVisualStyleBackColor = false;
        clearPointsButton.Click += clearPointsButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.BackColor = SystemColors.Control;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.Location = new Point(172, 0);
        fullExtentButton.Margin = new Padding(0);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(82, 36);
        fullExtentButton.TabIndex = 2;
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.UseVisualStyleBackColor = false;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // pointCountLabel
        // 
        pointCountLabel.AutoSize = false;
        pointCountLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        pointCountLabel.Location = new Point(266, 0);
        pointCountLabel.Margin = new Padding(0);
        pointCountLabel.Name = "pointCountLabel";
        pointCountLabel.Size = new Size(150, 36);
        pointCountLabel.TabIndex = 3;
        pointCountLabel.Text = "Point count: 0";
        pointCountLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 39);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(1184, 700);
        geoKernelViewerControl.TabIndex = 1;
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
        Controls.Add(geoKernelViewerControl);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(900, 600);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "AddPointProgrammatic";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
