namespace GeoKernel.OnTheFlyReproject.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayoutPanel;
    private Panel toolbarPanel;
    private Button fullExtentButton;
    private Label spatialReferenceLabel;
    private ComboBox spatialReferenceComboBox;
    private Label hintLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
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
        rootLayoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        fullExtentButton = new Button();
        spatialReferenceLabel = new Label();
        spatialReferenceComboBox = new ComboBox();
        hintLabel = new Label();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        rootLayoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayoutPanel
        // 
        rootLayoutPanel.ColumnCount = 1;
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayoutPanel.Controls.Add(toolbarPanel, 0, 0);
        rootLayoutPanel.Controls.Add(geoKernelViewerControl, 0, 1);
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
        toolbarPanel.Controls.Add(spatialReferenceLabel);
        toolbarPanel.Controls.Add(spatialReferenceComboBox);
        toolbarPanel.Controls.Add(hintLabel);
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
        // spatialReferenceLabel
        // 
        spatialReferenceLabel.AutoSize = true;
        spatialReferenceLabel.Location = new Point(48, 12);
        spatialReferenceLabel.Name = "spatialReferenceLabel";
        spatialReferenceLabel.Size = new Size(96, 15);
        spatialReferenceLabel.TabIndex = 1;
        spatialReferenceLabel.Text = "Spatial reference:";
        // 
        // spatialReferenceComboBox
        // 
        spatialReferenceComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        spatialReferenceComboBox.FormattingEnabled = true;
        spatialReferenceComboBox.Location = new Point(150, 8);
        spatialReferenceComboBox.Name = "spatialReferenceComboBox";
        spatialReferenceComboBox.Size = new Size(360, 23);
        spatialReferenceComboBox.TabIndex = 2;
        spatialReferenceComboBox.SelectedIndexChanged += spatialReferenceComboBox_SelectedIndexChanged;
        // 
        // hintLabel
        // 
        hintLabel.AutoSize = true;
        hintLabel.ForeColor = Color.FromArgb(78, 95, 91);
        hintLabel.Location = new Point(522, 12);
        hintLabel.Name = "hintLabel";
        hintLabel.Size = new Size(402, 15);
        hintLabel.TabIndex = 3;
        hintLabel.Text = "world_4326.shp is reprojected on the fly into the selected viewer CRS.";
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 39);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(1200, 737);
        geoKernelViewerControl.TabIndex = 1;
        // 
        // statusStrip
        // 
        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 776);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1200, 24);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(147, 19);
        statusLabel.Text = "Spatial reference: -";
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
        Text = "OnTheFlyReproject";
        Shown += MainForm_Shown;
        rootLayoutPanel.ResumeLayout(false);
        rootLayoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
