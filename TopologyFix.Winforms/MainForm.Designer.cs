using GeoKernel.NET.WinForms;

namespace GeoKernel.TopologyFix.Winforms;

public sealed partial class MainForm
{
    private GeoKernelViewerControl geoKernelViewerControl = null!;
    private FlowLayoutPanel toolbarPanel = null!;
    private Button fullExtentButton = null!;
    private Label operationLabel = null!;
    private ComboBox operationComboBox = null!;
    private SplitContainer splitContainer = null!;
    private TextBox detailsTextBox = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel statusLabel = null!;

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        geoKernelViewerControl = new GeoKernelViewerControl();
        toolbarPanel = new FlowLayoutPanel();
        fullExtentButton = new Button();
        operationLabel = new Label();
        operationComboBox = new ComboBox();
        splitContainer = new SplitContainer();
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
        toolbarPanel.Controls.AddRange(new Control[] { fullExtentButton, operationLabel, operationComboBox });
        toolbarPanel.Size = new Size(980, 32);
        toolbarPanel.TabIndex = 0;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(65, 22);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // operationLabel
        // 
        operationLabel.Name = "operationLabel";
        operationLabel.Size = new Size(64, 22);
        operationLabel.Text = "Operation:";
        // 
        // operationComboBox
        // 
        operationComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        operationComboBox.Margin = new Padding(0, 3, 4, 0);
        operationComboBox.Name = "operationComboBox";
        operationComboBox.Size = new Size(220, 23);
        operationComboBox.SelectedIndexChanged += operationComboBox_SelectedIndexChanged;
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.Location = new Point(0, 25);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.Size = new Size(1040, 633);
        splitContainer.SplitterDistance = 700;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(700, 633);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // detailsTextBox
        // 
        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Location = new Point(0, 0);
        detailsTextBox.Multiline = true;
        detailsTextBox.Name = "detailsTextBox";
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Size = new Size(336, 633);
        detailsTextBox.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 658);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1040, 22);
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(263, 17);
        statusLabel.Text = "Source geometry is shown. Choose a fix operation.";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(1040, 680);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "TopologyFix";
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
