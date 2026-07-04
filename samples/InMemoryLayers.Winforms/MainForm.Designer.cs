namespace GeoKernel.InMemoryLayers.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button addPointButton;
    private Button addLineButton;
    private Button addPolygonButton;
    private Panel clearSeparator;
    private Button clearMemoryButton;
    private Button fullExtentButton;
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
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        addPointButton = new Button();
        addLineButton = new Button();
        addPolygonButton = new Button();
        clearSeparator = new Panel();
        clearMemoryButton = new Button();
        fullExtentButton = new Button();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
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
        layoutPanel.Controls.Add(statusStrip, 0, 2);
        layoutPanel.Controls.Add(geoKernelViewerControl, 0, 1);
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
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(clearMemoryButton);
        toolbarPanel.Controls.Add(clearSeparator);
        toolbarPanel.Controls.Add(addPolygonButton);
        toolbarPanel.Controls.Add(addLineButton);
        toolbarPanel.Controls.Add(addPointButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Size = new Size(1200, 32);
        toolbarPanel.TabIndex = 0;
        // 
        // addPointButton
        // 
        addPointButton.BackColor = SystemColors.Control;
        addPointButton.FlatAppearance.BorderSize = 0;
        addPointButton.FlatStyle = FlatStyle.Flat;
        addPointButton.Location = new Point(0, 0);
        addPointButton.Margin = new Padding(0);
        addPointButton.Name = "addPointButton";
        addPointButton.Size = new Size(86, 32);
        addPointButton.TabIndex = 0;
        addPointButton.Text = "Add Point";
        addPointButton.UseVisualStyleBackColor = false;
        addPointButton.Click += addPointButton_Click;
        // 
        // addLineButton
        // 
        addLineButton.BackColor = SystemColors.Control;
        addLineButton.FlatAppearance.BorderSize = 0;
        addLineButton.FlatStyle = FlatStyle.Flat;
        addLineButton.Location = new Point(86, 0);
        addLineButton.Margin = new Padding(0);
        addLineButton.Name = "addLineButton";
        addLineButton.Size = new Size(78, 32);
        addLineButton.TabIndex = 1;
        addLineButton.Text = "Add Line";
        addLineButton.UseVisualStyleBackColor = false;
        addLineButton.Click += addLineButton_Click;
        // 
        // addPolygonButton
        // 
        addPolygonButton.BackColor = SystemColors.Control;
        addPolygonButton.FlatAppearance.BorderSize = 0;
        addPolygonButton.FlatStyle = FlatStyle.Flat;
        addPolygonButton.Location = new Point(164, 0);
        addPolygonButton.Margin = new Padding(0);
        addPolygonButton.Name = "addPolygonButton";
        addPolygonButton.Size = new Size(104, 32);
        addPolygonButton.TabIndex = 2;
        addPolygonButton.Text = "Add Polygon";
        addPolygonButton.UseVisualStyleBackColor = false;
        addPolygonButton.Click += addPolygonButton_Click;
        // 
        // clearSeparator
        // 
        clearSeparator.BackColor = SystemColors.ControlDark;
        clearSeparator.Location = new Point(274, 4);
        clearSeparator.Margin = new Padding(0);
        clearSeparator.Name = "clearSeparator";
        clearSeparator.Size = new Size(1, 24);
        clearSeparator.TabIndex = 3;
        // 
        // clearMemoryButton
        // 
        clearMemoryButton.BackColor = SystemColors.Control;
        clearMemoryButton.FlatAppearance.BorderSize = 0;
        clearMemoryButton.FlatStyle = FlatStyle.Flat;
        clearMemoryButton.Location = new Point(280, 0);
        clearMemoryButton.Margin = new Padding(0);
        clearMemoryButton.Name = "clearMemoryButton";
        clearMemoryButton.Size = new Size(146, 32);
        clearMemoryButton.TabIndex = 4;
        clearMemoryButton.Text = "Clear Memory Layers";
        clearMemoryButton.UseVisualStyleBackColor = false;
        clearMemoryButton.Click += clearMemoryButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.BackColor = SystemColors.Control;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.Location = new Point(426, 0);
        fullExtentButton.Margin = new Padding(0);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Size = new Size(86, 32);
        fullExtentButton.TabIndex = 5;
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.UseVisualStyleBackColor = false;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.TabIndex = 0;
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
        Text = "InMemoryLayers";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
