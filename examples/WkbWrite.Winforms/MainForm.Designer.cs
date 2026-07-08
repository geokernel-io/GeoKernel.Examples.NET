namespace GeoKernel.WkbWrite.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel inputPanel;
    private Label geometryLabel;
    private ComboBox geometryComboBox;
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
        geometryLabel = new Label();
        geometryComboBox = new ComboBox();
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
        inputPanel.Controls.Add(geometryLabel);
        inputPanel.Controls.Add(geometryComboBox);
        inputPanel.Controls.Add(resetButton);
        inputPanel.Dock = DockStyle.Top;
        inputPanel.Location = new Point(0, 0);
        inputPanel.Name = "inputPanel";
        inputPanel.Padding = new Padding(6, 4, 6, 4);
        inputPanel.Size = new Size(1120, 34);
        inputPanel.TabIndex = 0;
        geometryLabel.AutoSize = true;
        geometryLabel.Location = new Point(8, 9);
        geometryLabel.Name = "geometryLabel";
        geometryLabel.Size = new Size(62, 15);
        geometryLabel.TabIndex = 0;
        geometryLabel.Text = "Geometry:";
        geometryComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        geometryComboBox.Items.AddRange(new object[] { "Point", "Polyline", "Polygon" });
        geometryComboBox.Location = new Point(76, 5);
        geometryComboBox.Name = "geometryComboBox";
        geometryComboBox.Size = new Size(130, 23);
        geometryComboBox.TabIndex = 1;
        geometryComboBox.SelectedIndexChanged += geometryComboBox_SelectedIndexChanged;
        resetButton.Location = new Point(216, 4);
        resetButton.Name = "resetButton";
        resetButton.Size = new Size(90, 25);
        resetButton.TabIndex = 2;
        resetButton.Text = "Reset";
        resetButton.UseVisualStyleBackColor = true;
        resetButton.Click += resetButton_Click;
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 34);
        splitContainer.Name = "splitContainer";
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.Size = new Size(1120, 664);
        splitContainer.SplitterDistance = 700;
        splitContainer.TabIndex = 1;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(700, 664);
        geoKernelViewerControl.TabIndex = 0;
        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Location = new Point(0, 0);
        detailsTextBox.Multiline = true;
        detailsTextBox.Name = "detailsTextBox";
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Size = new Size(416, 664);
        detailsTextBox.TabIndex = 0;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 698);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1120, 22);
        statusStrip.TabIndex = 2;
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(0, 17);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1120, 720);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(inputPanel);
        MinimumSize = new Size(840, 520);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "WkbWrite";
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
