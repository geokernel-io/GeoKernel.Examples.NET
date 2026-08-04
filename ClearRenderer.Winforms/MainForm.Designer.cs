namespace GeoKernel.ClearRenderer.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private FlowLayoutPanel toolbarPanel;
    private Button applyRendererButton;
    private Button clearRendererButton;
    private Label rendererStateLabel;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;
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
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new FlowLayoutPanel();
        applyRendererButton = new Button();
        clearRendererButton = new Button();
        rendererStateLabel = new Label();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // layoutPanel
        // 
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(toolbarPanel, 0, 0);
        layoutPanel.Controls.Add(geoKernelViewerControl, 0, 1);
        layoutPanel.Controls.Add(statusStrip, 0, 2);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.Location = new Point(0, 0);
        layoutPanel.Margin = new Padding(0);
        layoutPanel.Name = "layoutPanel";
        layoutPanel.RowCount = 3;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
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
        toolbarPanel.Controls.AddRange(new Control[] { applyRendererButton, clearRendererButton, rendererStateLabel });
        toolbarPanel.Size = new Size(980, 32);
        toolbarPanel.TabIndex = 0;
        // 
        // applyRendererButton
        // 
        applyRendererButton.Name = "applyRendererButton";
        applyRendererButton.AutoSize = true;
        applyRendererButton.Height = 28;
        applyRendererButton.Margin = new Padding(0, 2, 4, 2);
        applyRendererButton.Padding = new Padding(8, 0, 8, 0);
        applyRendererButton.UseVisualStyleBackColor = true;
        applyRendererButton.Size = new Size(156, 29);
        applyRendererButton.Text = "Apply Categorized Renderer";
        applyRendererButton.Click += applyRendererButton_Click;
        // 
        // clearRendererButton
        // 
        clearRendererButton.Name = "clearRendererButton";
        clearRendererButton.AutoSize = true;
        clearRendererButton.Height = 28;
        clearRendererButton.Margin = new Padding(0, 2, 4, 2);
        clearRendererButton.Padding = new Padding(8, 0, 8, 0);
        clearRendererButton.UseVisualStyleBackColor = true;
        clearRendererButton.Size = new Size(88, 29);
        clearRendererButton.Text = "Clear Renderer";
        clearRendererButton.Click += clearRendererButton_Click;
        // 
        // rendererStateLabel
        // 
        rendererStateLabel.Margin = new Padding(8, 1, 0, 2);
        rendererStateLabel.Name = "rendererStateLabel";
        rendererStateLabel.AutoSize = true;
        rendererStateLabel.Margin = new Padding(8, 8, 8, 0);
        rendererStateLabel.TextAlign = ContentAlignment.MiddleLeft;
        rendererStateLabel.Size = new Size(184, 29);
        rendererStateLabel.Text = "Renderer: categorized by STATE";
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 32);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.TabIndex = 1;
        // 
        // statusStrip
        // 
        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar });
        statusStrip.Location = new Point(0, 776);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1200, 24);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(39, 19);
        statusLabel.Text = "Ready";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 800);
        Controls.Add(layoutPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ClearRenderer";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
