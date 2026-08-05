namespace GeoKernel.WebMercator.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayoutPanel;
    private Panel toolbarPanel;
    private Button fullExtentButton;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private TextBox detailsTextBox;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        rootLayoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        fullExtentButton = new Button();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        rootLayoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayoutPanel
        // 
        rootLayoutPanel.ColumnCount = 1;
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayoutPanel.Controls.Add(toolbarPanel, 0, 0);
        rootLayoutPanel.Controls.Add(splitContainer, 0, 1);
        rootLayoutPanel.Controls.Add(statusStrip, 0, 2);
        rootLayoutPanel.Dock = DockStyle.Fill;
        rootLayoutPanel.Location = new Point(0, 0);
        rootLayoutPanel.Margin = new Padding(0);
        rootLayoutPanel.Name = "rootLayoutPanel";
        rootLayoutPanel.RowCount = 3;
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        rootLayoutPanel.Size = new Size(1200, 800);
        rootLayoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1200, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // fullExtentButton
        // 
        fullExtentButton.BackColor = SystemColors.Control;
        fullExtentButton.BackgroundImage = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.BackgroundImageLayout = ImageLayout.Zoom;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.Location = new Point(0, 0);
        fullExtentButton.Margin = new Padding(0);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Padding = new Padding(3);
        fullExtentButton.Size = new Size(36, 36);
        fullExtentButton.TabIndex = 0;
        fullExtentButton.UseVisualStyleBackColor = false;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.Location = new Point(0, 39);
        splitContainer.Margin = new Padding(0);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.Size = new Size(1200, 737);
        splitContainer.SplitterDistance = 840;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(840, 737);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // detailsTextBox
        // 
        detailsTextBox.BackColor = Color.White;
        detailsTextBox.BorderStyle = BorderStyle.None;
        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Font = new Font("Consolas", 9F);
        detailsTextBox.Location = new Point(0, 0);
        detailsTextBox.Multiline = true;
        detailsTextBox.Name = "detailsTextBox";
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Size = new Size(356, 737);
        detailsTextBox.TabIndex = 0;
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
        statusLabel.Size = new Size(169, 19);
        statusLabel.Text = "Screen: - | WebMercator: -";
        downloadProgressBar.Alignment = ToolStripItemAlignment.Right;
        downloadProgressBar.Name = "downloadProgressBar";
        downloadProgressBar.Size = new Size(180, 18);
        downloadProgressBar.Visible = false;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 800);
        Controls.Add(rootLayoutPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "WebMercator";
        Shown += MainForm_Shown;
        rootLayoutPanel.ResumeLayout(false);
        rootLayoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        splitContainer.Panel2.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
