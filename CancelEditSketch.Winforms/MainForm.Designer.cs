namespace GeoKernel.CancelEditSketch.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button fullExtentButton;
    private Panel navigationSeparator;
    private Button addPolygonButton;
    private Button panButton;
    private Panel editSeparator;
    private Button clearPolygonsButton;
    private Label polygonCountLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;
    private ToolTip toolbarToolTip;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        toolbarToolTip = new ToolTip(components);
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        fullExtentButton = new Button();
        navigationSeparator = new Panel();
        addPolygonButton = new Button();
        panButton = new Button();
        editSeparator = new Panel();
        clearPolygonsButton = new Button();
        polygonCountLabel = new Label();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
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
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(polygonCountLabel);
        toolbarPanel.Controls.Add(clearPolygonsButton);
        toolbarPanel.Controls.Add(editSeparator);
        toolbarPanel.Controls.Add(panButton);
        toolbarPanel.Controls.Add(addPolygonButton);
        toolbarPanel.Controls.Add(navigationSeparator);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Size = new Size(1200, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // fullExtentButton
        // 
        fullExtentButton.BackColor = SystemColors.Control;
        fullExtentButton.BackgroundImage = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.BackgroundImageLayout = ImageLayout.Center;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.Location = new Point(0, 0);
        fullExtentButton.Margin = new Padding(0);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Padding = new Padding(3);
        fullExtentButton.Size = new Size(36, 36);
        fullExtentButton.TabIndex = 0;
        fullExtentButton.Text = "";
        fullExtentButton.AccessibleName = "Full Extent";
        toolbarToolTip.SetToolTip(fullExtentButton, "Full Extent");
        fullExtentButton.UseVisualStyleBackColor = false;
        fullExtentButton.TabStop = false;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // navigationSeparator
        // 
        navigationSeparator.BackColor = SystemColors.ControlDark;
        navigationSeparator.Location = new Point(40, 4);
        navigationSeparator.Margin = new Padding(0);
        navigationSeparator.Name = "navigationSeparator";
        navigationSeparator.Size = new Size(1, 28);
        navigationSeparator.TabIndex = 1;
        // 
        // addPolygonButton
        // 
        addPolygonButton.BackColor = Color.FromArgb(210, 232, 255);
        addPolygonButton.BackgroundImage = (Image)resources.GetObject("addPolygonButton.Image");
        addPolygonButton.BackgroundImageLayout = ImageLayout.Center;
        addPolygonButton.FlatAppearance.BorderColor = Color.FromArgb(0, 120, 215);
        addPolygonButton.FlatAppearance.BorderSize = 0;
        addPolygonButton.FlatStyle = FlatStyle.Flat;
        addPolygonButton.Location = new Point(44, 0);
        addPolygonButton.Margin = new Padding(0);
        addPolygonButton.Name = "addPolygonButton";
        addPolygonButton.Padding = new Padding(3);
        addPolygonButton.Size = new Size(36, 36);
        addPolygonButton.TabIndex = 2;
        addPolygonButton.Text = "";
        addPolygonButton.AccessibleName = "Add Polygon";
        toolbarToolTip.SetToolTip(addPolygonButton, "Add Polygon");
        addPolygonButton.UseVisualStyleBackColor = false;
        addPolygonButton.TabStop = false;
        addPolygonButton.Click += addPolygonButton_Click;
        // 
        // panButton
        // 
        panButton.BackColor = SystemColors.Control;
        panButton.BackgroundImage = (Image)resources.GetObject("panButton.Image");
        panButton.BackgroundImageLayout = ImageLayout.Center;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.Location = new Point(80, 0);
        panButton.Margin = new Padding(0);
        panButton.Name = "panButton";
        panButton.Padding = new Padding(3);
        panButton.Size = new Size(36, 36);
        panButton.TabIndex = 3;
        panButton.Text = "";
        panButton.AccessibleName = "Pan";
        toolbarToolTip.SetToolTip(panButton, "Pan");
        panButton.UseVisualStyleBackColor = false;
        panButton.TabStop = false;
        panButton.Click += panButton_Click;
        // 
        // editSeparator
        // 
        editSeparator.BackColor = SystemColors.ControlDark;
        editSeparator.Location = new Point(120, 4);
        editSeparator.Margin = new Padding(0);
        editSeparator.Name = "editSeparator";
        editSeparator.Size = new Size(1, 28);
        editSeparator.TabIndex = 4;
        // 
        // clearPolygonsButton
        // 
        clearPolygonsButton.BackColor = SystemColors.Control;
        clearPolygonsButton.BackgroundImage = (Image)resources.GetObject("clearPolygonsButton.Image");
        clearPolygonsButton.BackgroundImageLayout = ImageLayout.Center;
        clearPolygonsButton.FlatAppearance.BorderSize = 0;
        clearPolygonsButton.FlatStyle = FlatStyle.Flat;
        clearPolygonsButton.Location = new Point(124, 0);
        clearPolygonsButton.Margin = new Padding(0);
        clearPolygonsButton.Name = "clearPolygonsButton";
        clearPolygonsButton.Padding = new Padding(3);
        clearPolygonsButton.Size = new Size(36, 36);
        clearPolygonsButton.TabIndex = 5;
        clearPolygonsButton.Text = "";
        clearPolygonsButton.AccessibleName = "Cancel Sketch";
        toolbarToolTip.SetToolTip(clearPolygonsButton, "Cancel Sketch");
        clearPolygonsButton.UseVisualStyleBackColor = false;
        clearPolygonsButton.TabStop = false;
        clearPolygonsButton.Click += clearPolygonsButton_Click;
        // 
        // polygonCountLabel
        // 
        polygonCountLabel.AutoSize = false;
        polygonCountLabel.Location = new Point(172, 0);
        polygonCountLabel.Margin = new Padding(0);
        polygonCountLabel.Name = "polygonCountLabel";
        polygonCountLabel.Size = new Size(170, 36);
        polygonCountLabel.TabIndex = 6;
        polygonCountLabel.Text = "Polygon count: 0";
        polygonCountLabel.TextAlign = ContentAlignment.MiddleLeft;
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
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar });
        downloadProgressBar.Size = new Size(180, 18);
        downloadProgressBar.Visible = false;
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
        Text = "CancelEditSketch";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
