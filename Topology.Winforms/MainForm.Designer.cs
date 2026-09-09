namespace GeoKernel.Topology.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private Button panButton;
    private Button zoomBoxButton;
    private Button fullExtentButton;
    private Label operationLabel;
    private ComboBox operationComboBox;
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
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        components = new System.ComponentModel.Container();
        toolbarPanel = new FlowLayoutPanel();
        panButton = new Button();
        zoomBoxButton = new Button();
        fullExtentButton = new Button();
        operationLabel = new Label();
        operationComboBox = new ComboBox();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        toolbarPanel.AutoSize = false;
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.FlowDirection = FlowDirection.LeftToRight;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Controls.AddRange(new Control[] { panButton, zoomBoxButton, fullExtentButton, operationLabel, operationComboBox });
        toolbarPanel.Size = new Size(1040, 32);
        toolbarPanel.TabIndex = 0;
        panButton.Name = "panButton";
        panButton.AutoSize = true;
        panButton.Padding = new Padding(8, 0, 8, 0);
        panButton.Size = new Size(55, 28);
        panButton.Margin = new Padding(0, 2, 2, 2);
        panButton.Text = "Pan";
        panButton.UseVisualStyleBackColor = true;
        panButton.Click += panButton_Click;
        zoomBoxButton.Name = "zoomBoxButton";
        zoomBoxButton.AutoSize = true;
        zoomBoxButton.Padding = new Padding(8, 0, 8, 0);
        zoomBoxButton.Size = new Size(83, 28);
        zoomBoxButton.Margin = new Padding(0, 2, 2, 2);
        zoomBoxButton.Text = "Zoom Rect";
        zoomBoxButton.UseVisualStyleBackColor = true;
        zoomBoxButton.Click += zoomBoxButton_Click;
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.AutoSize = true;
        fullExtentButton.Padding = new Padding(8, 0, 8, 0);
        fullExtentButton.Size = new Size(86, 28);
        fullExtentButton.Margin = new Padding(0, 2, 4, 2);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Click += fullExtentButton_Click;
        operationLabel.Name = "operationLabel";
        operationLabel.AutoSize = true;
        operationLabel.Margin = new Padding(8, 8, 8, 0);
        operationLabel.TextAlign = ContentAlignment.MiddleLeft;
        operationLabel.Size = new Size(64, 22);
        operationLabel.Text = "Operation:";
        operationComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        operationComboBox.Margin = new Padding(0, 4, 4, 2);
        operationComboBox.Name = "operationComboBox";
        operationComboBox.Size = new Size(250, 23);
        operationComboBox.SelectedIndexChanged += operationComboBox_SelectedIndexChanged;
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 25);
        splitContainer.Name = "splitContainer";
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.Size = new Size(1040, 633);
        splitContainer.SplitterDistance = 680;
        splitContainer.TabIndex = 1;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(680, 633);
        geoKernelViewerControl.TabIndex = 0;
        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Location = new Point(0, 0);
        detailsTextBox.Multiline = true;
        detailsTextBox.Name = "detailsTextBox";
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Size = new Size(356, 633);
        detailsTextBox.TabIndex = 0;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 658);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1040, 22);
        statusStrip.TabIndex = 2;
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(0, 17);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1040, 680);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        MinimumSize = new Size(760, 500);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Topology";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
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
