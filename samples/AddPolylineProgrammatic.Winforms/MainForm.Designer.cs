using GeoKernel.NET.WinForms;

namespace GeoKernel.AddPolylineProgrammatic.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel toolbarPanel;
    private Button addPolylineButton;
    private Button clearLinesButton;
    private Button fullExtentButton;
    private Label polylineCountLabel;
    private GeoKernelViewerControl geoKernelViewerControl;
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
        toolbarPanel = new Panel();
        addPolylineButton = new Button();
        clearLinesButton = new Button();
        fullExtentButton = new Button();
        polylineCountLabel = new Label();
        geoKernelViewerControl = new GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        toolbarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = Color.FromArgb(242, 242, 242);
        toolbarPanel.Controls.Add(addPolylineButton);
        toolbarPanel.Controls.Add(clearLinesButton);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(polylineCountLabel);
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1184, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // addPolylineButton
        // 
        addPolylineButton.FlatStyle = FlatStyle.Flat;
        addPolylineButton.FlatAppearance.BorderSize = 0;
        addPolylineButton.Location = new Point(4, 6);
        addPolylineButton.Name = "addPolylineButton";
        addPolylineButton.Size = new Size(91, 27);
        addPolylineButton.TabIndex = 0;
        addPolylineButton.Text = "Add Polyline";
        addPolylineButton.UseVisualStyleBackColor = true;
        addPolylineButton.Click += addPolylineButton_Click;
        // 
        // clearLinesButton
        // 
        clearLinesButton.FlatStyle = FlatStyle.Flat;
        clearLinesButton.FlatAppearance.BorderSize = 0;
        clearLinesButton.Location = new Point(101, 6);
        clearLinesButton.Name = "clearLinesButton";
        clearLinesButton.Size = new Size(86, 27);
        clearLinesButton.TabIndex = 1;
        clearLinesButton.Text = "Clear Lines";
        clearLinesButton.UseVisualStyleBackColor = true;
        clearLinesButton.Click += clearLinesButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.Location = new Point(193, 6);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(82, 27);
        fullExtentButton.TabIndex = 2;
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // polylineCountLabel
        // 
        polylineCountLabel.AutoSize = true;
        polylineCountLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        polylineCountLabel.Location = new Point(288, 12);
        polylineCountLabel.Name = "polylineCountLabel";
        polylineCountLabel.Size = new Size(97, 15);
        polylineCountLabel.TabIndex = 3;
        polylineCountLabel.Text = "Polyline count: 0";
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
        Controls.Add(geoKernelViewerControl);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(900, 600);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "AddPolylineProgrammatic";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
