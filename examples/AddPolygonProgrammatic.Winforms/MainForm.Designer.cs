using GeoKernel.NET.WinForms;

namespace GeoKernel.AddPolygonProgrammatic.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel toolbarPanel;
    private Button addPolygonButton;
    private Button clearPolygonsButton;
    private Button fullExtentButton;
    private Label polygonCountLabel;
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
        addPolygonButton = new Button();
        clearPolygonsButton = new Button();
        fullExtentButton = new Button();
        polygonCountLabel = new Label();
        geoKernelViewerControl = new GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        toolbarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(polygonCountLabel);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(clearPolygonsButton);
        toolbarPanel.Controls.Add(addPolygonButton);
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1184, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // addPolygonButton
        // 
        addPolygonButton.BackColor = SystemColors.Control;
        addPolygonButton.FlatAppearance.BorderSize = 0;
        addPolygonButton.FlatStyle = FlatStyle.Flat;
        addPolygonButton.Location = new Point(0, 0);
        addPolygonButton.Margin = new Padding(0);
        addPolygonButton.Name = "addPolygonButton";
        addPolygonButton.Size = new Size(96, 36);
        addPolygonButton.TabIndex = 0;
        addPolygonButton.Text = "Add Polygon";
        addPolygonButton.UseVisualStyleBackColor = false;
        addPolygonButton.Click += addPolygonButton_Click;
        // 
        // clearPolygonsButton
        // 
        clearPolygonsButton.BackColor = SystemColors.Control;
        clearPolygonsButton.FlatAppearance.BorderSize = 0;
        clearPolygonsButton.FlatStyle = FlatStyle.Flat;
        clearPolygonsButton.Location = new Point(96, 0);
        clearPolygonsButton.Margin = new Padding(0);
        clearPolygonsButton.Name = "clearPolygonsButton";
        clearPolygonsButton.Size = new Size(108, 36);
        clearPolygonsButton.TabIndex = 1;
        clearPolygonsButton.Text = "Clear Polygons";
        clearPolygonsButton.UseVisualStyleBackColor = false;
        clearPolygonsButton.Click += clearPolygonsButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.BackColor = SystemColors.Control;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.Location = new Point(204, 0);
        fullExtentButton.Margin = new Padding(0);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(82, 36);
        fullExtentButton.TabIndex = 2;
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.UseVisualStyleBackColor = false;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // polygonCountLabel
        // 
        polygonCountLabel.AutoSize = false;
        polygonCountLabel.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        polygonCountLabel.Location = new Point(298, 0);
        polygonCountLabel.Margin = new Padding(0);
        polygonCountLabel.Name = "polygonCountLabel";
        polygonCountLabel.Size = new Size(170, 36);
        polygonCountLabel.TabIndex = 3;
        polygonCountLabel.Text = "Polygon count: 0";
        polygonCountLabel.TextAlign = ContentAlignment.MiddleLeft;
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
        Text = "AddPolygonProgrammatic";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
