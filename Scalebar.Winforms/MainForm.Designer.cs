namespace GeoKernel.Scalebar.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private Panel progressPanel;
    private Label progressLabel;
    private ProgressBar progressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        progressPanel = new Panel();
        progressLabel = new Label();
        progressBar = new ProgressBar();
        progressPanel.SuspendLayout();
        SuspendLayout();
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.TabIndex = 0;
        // 
        // progressPanel
        // 
        progressPanel.Controls.Add(progressLabel);
        progressPanel.Controls.Add(progressBar);
        progressPanel.Dock = DockStyle.Bottom;
        progressPanel.Location = new Point(0, 770);
        progressPanel.Name = "progressPanel";
        progressPanel.Padding = new Padding(8, 4, 8, 4);
        progressPanel.Size = new Size(1200, 30);
        progressPanel.TabIndex = 1;
        // 
        // progressLabel
        // 
        progressLabel.Dock = DockStyle.Fill;
        progressLabel.Location = new Point(8, 4);
        progressLabel.Name = "progressLabel";
        progressLabel.Size = new Size(924, 22);
        progressLabel.TabIndex = 0;
        progressLabel.Text = "Ready";
        progressLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // progressBar
        // 
        progressBar.Dock = DockStyle.Right;
        progressBar.Location = new Point(932, 4);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(260, 22);
        progressBar.TabIndex = 1;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(geoKernelViewerControl);
        Controls.Add(progressPanel);
        ClientSize = new Size(1200, 800);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Scalebar";
        Shown += MainForm_Shown;
        progressPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
