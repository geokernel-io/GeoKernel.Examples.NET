namespace GeoKernel.WktReadPolyline.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel inputPanel;
    private Label wktLabel;
    private TextBox wktTextBox;
    private Button readButton;
    private Button resetButton;
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
        inputPanel = new Panel();
        wktLabel = new Label();
        wktTextBox = new TextBox();
        readButton = new Button();
        resetButton = new Button();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        inputPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        inputPanel.Controls.Add(wktLabel);
        inputPanel.Controls.Add(wktTextBox);
        inputPanel.Controls.Add(readButton);
        inputPanel.Controls.Add(resetButton);
        inputPanel.Dock = DockStyle.Top;
        inputPanel.Location = new Point(0, 0);
        inputPanel.Name = "inputPanel";
        inputPanel.Padding = new Padding(6, 4, 6, 4);
        inputPanel.Size = new Size(1100, 34);
        inputPanel.TabIndex = 0;
        wktLabel.AutoSize = true;
        wktLabel.Location = new Point(8, 9);
        wktLabel.Name = "wktLabel";
        wktLabel.Size = new Size(35, 15);
        wktLabel.TabIndex = 0;
        wktLabel.Text = "WKT:";
        wktTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        wktTextBox.Location = new Point(49, 5);
        wktTextBox.Name = "wktTextBox";
        wktTextBox.Size = new Size(832, 23);
        wktTextBox.TabIndex = 1;
        wktTextBox.Text = "LINESTRING(-122.4194 37.7749, -121.8863 37.3382, -121.4944 38.5816, -120.7401 37.6391)";
        wktTextBox.KeyDown += wktTextBox_KeyDown;
        readButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        readButton.Location = new Point(887, 4);
        readButton.Name = "readButton";
        readButton.Size = new Size(122, 25);
        readButton.TabIndex = 2;
        readButton.Text = "Read LineString";
        readButton.UseVisualStyleBackColor = true;
        readButton.Click += readButton_Click;
        resetButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        resetButton.Location = new Point(1015, 4);
        resetButton.Name = "resetButton";
        resetButton.Size = new Size(80, 25);
        resetButton.TabIndex = 3;
        resetButton.Text = "Reset";
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Click += resetButton_Click;
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 34);
        splitContainer.Name = "splitContainer";
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.Size = new Size(1100, 664);
        splitContainer.SplitterDistance = 715;
        splitContainer.TabIndex = 1;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(715, 664);
        geoKernelViewerControl.TabIndex = 0;
        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Location = new Point(0, 0);
        detailsTextBox.Multiline = true;
        detailsTextBox.Name = "detailsTextBox";
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Size = new Size(381, 664);
        detailsTextBox.TabIndex = 0;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 698);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1100, 22);
        statusStrip.TabIndex = 2;
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(0, 17);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 720);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(inputPanel);
        MinimumSize = new Size(800, 520);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "WktReadPolyline";
        Shown += MainForm_Shown;
        inputPanel.ResumeLayout(false);
        inputPanel.PerformLayout();
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
