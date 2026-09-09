namespace GeoKernel.GeoJsonWrite.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel inputPanel;
    private Button clearButton;
    private Label hintLabel;
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
        System.ComponentModel.ComponentResourceManager resources = new(typeof(MainForm));
        components = new System.ComponentModel.Container();
        inputPanel = new Panel();
        clearButton = new Button();
        hintLabel = new Label();
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

        inputPanel.Controls.Add(clearButton);
        inputPanel.Controls.Add(hintLabel);
        inputPanel.Dock = DockStyle.Top;
        inputPanel.Height = 34;
        inputPanel.Padding = new Padding(6, 4, 6, 4);
        inputPanel.Name = "inputPanel";

        clearButton.Location = new Point(6, 4);
        clearButton.Name = "clearButton";
        clearButton.Size = new Size(75, 25);
        clearButton.Text = "Clear";
        clearButton.UseVisualStyleBackColor = true;
        clearButton.Click += clearButton_Click;

        hintLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        hintLabel.Location = new Point(89, 8);
        hintLabel.Name = "hintLabel";
        hintLabel.Size = new Size(1005, 18);
        hintLabel.Text = "Click polygon vertices, then press Enter or double-click to finish. GeoJSON is written automatically.";

        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.Location = new Point(0, 34);
        splitContainer.Name = "splitContainer";
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.Size = new Size(1100, 664);
        splitContainer.SplitterDistance = 666;

        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Name = "geoKernelViewerControl";

        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Multiline = true;
        detailsTextBox.Name = "detailsTextBox";
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Text = "GisGeoJsonWriter::writePolygonString(shape)\r\n\r\nDraw a polygon on the map. The GeoJSON string will appear here.";

        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Name = "statusStrip";
        statusLabel.Name = "statusLabel";

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 720);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(inputPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "GeoJsonWrite";
        Shown += MainForm_Shown;
        inputPanel.ResumeLayout(false);
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
