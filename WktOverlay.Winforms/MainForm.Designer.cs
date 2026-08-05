namespace GeoKernel.WktOverlay.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private Panel sidePanel;
    private Label inputLabel;
    private TextBox wktTextBox;
    private Button renderButton;
    private Button resetButton;
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        components = new System.ComponentModel.Container();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        sidePanel = new Panel();
        inputLabel = new Label();
        wktTextBox = new TextBox();
        renderButton = new Button();
        resetButton = new Button();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        sidePanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 0);
        splitContainer.Name = "splitContainer";
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        splitContainer.Panel2.Controls.Add(sidePanel);
        splitContainer.Size = new Size(1160, 718);
        splitContainer.SplitterDistance = 760;
        splitContainer.TabIndex = 0;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(760, 718);
        geoKernelViewerControl.TabIndex = 0;
        sidePanel.Controls.Add(detailsTextBox);
        sidePanel.Controls.Add(resetButton);
        sidePanel.Controls.Add(renderButton);
        sidePanel.Controls.Add(wktTextBox);
        sidePanel.Controls.Add(inputLabel);
        sidePanel.Dock = DockStyle.Fill;
        sidePanel.Location = new Point(0, 0);
        sidePanel.Name = "sidePanel";
        sidePanel.Padding = new Padding(6);
        sidePanel.Size = new Size(396, 718);
        sidePanel.TabIndex = 0;
        inputLabel.AutoSize = true;
        inputLabel.Location = new Point(6, 8);
        inputLabel.Name = "inputLabel";
        inputLabel.Size = new Size(202, 15);
        inputLabel.TabIndex = 0;
        inputLabel.Text = "WKT overlay input (one geometry per line)";
        wktTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        wktTextBox.Location = new Point(6, 28);
        wktTextBox.Multiline = true;
        wktTextBox.Name = "wktTextBox";
        wktTextBox.ScrollBars = ScrollBars.Vertical;
        wktTextBox.Size = new Size(384, 152);
        wktTextBox.TabIndex = 1;
        renderButton.Location = new Point(6, 186);
        renderButton.Name = "renderButton";
        renderButton.Size = new Size(122, 26);
        renderButton.TabIndex = 2;
        renderButton.Text = "Render Overlay";
        renderButton.UseVisualStyleBackColor = true;
        renderButton.Click += renderButton_Click;
        resetButton.Location = new Point(134, 186);
        resetButton.Name = "resetButton";
        resetButton.Size = new Size(86, 26);
        resetButton.TabIndex = 3;
        resetButton.Text = "Reset";
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Click += resetButton_Click;
        detailsTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        detailsTextBox.Location = new Point(6, 220);
        detailsTextBox.Multiline = true;
        detailsTextBox.Name = "detailsTextBox";
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Size = new Size(384, 492);
        detailsTextBox.TabIndex = 4;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 718);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1160, 22);
        statusStrip.TabIndex = 1;
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(0, 17);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1160, 740);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        MinimumSize = new Size(900, 540);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "WktOverlay";
        Shown += MainForm_Shown;
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        sidePanel.ResumeLayout(false);
        sidePanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }
}
