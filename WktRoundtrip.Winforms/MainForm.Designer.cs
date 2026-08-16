namespace GeoKernel.WktRoundtrip.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayout;
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
        System.ComponentModel.ComponentResourceManager resources = new(typeof(MainForm));
        components = new System.ComponentModel.Container();
        rootLayout = new TableLayoutPanel();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        rootLayout.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();

        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(geoKernelViewerControl, 0, 0);
        rootLayout.Controls.Add(detailsTextBox, 0, 1);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Margin = Padding.Empty;
        rootLayout.Name = "rootLayout";
        rootLayout.RowCount = 2;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 170F));

        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Margin = Padding.Empty;
        geoKernelViewerControl.Name = "geoKernelViewerControl";

        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Margin = Padding.Empty;
        detailsTextBox.Multiline = true;
        detailsTextBox.Name = "detailsTextBox";
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;

        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Name = "statusStrip";
        statusLabel.Name = "statusLabel";

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 720);
        Controls.Add(rootLayout);
        Controls.Add(statusStrip);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "WktRoundtrip";
        Shown += MainForm_Shown;
        rootLayout.ResumeLayout(false);
        rootLayout.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
