namespace GeoKernel.Scalebar.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;

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
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.Add(geoKernelViewerControl);
        ClientSize = new Size(1200, 800);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Scalebar";
        Shown += MainForm_Shown;
        ResumeLayout(false);
    }
}
