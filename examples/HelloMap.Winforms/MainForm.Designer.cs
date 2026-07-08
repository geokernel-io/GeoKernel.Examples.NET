namespace GeoKernel.HelloMap.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button zoomInButton;
    private Button zoomOutButton;
    private Button fullExtentButton;
    private Panel toolbarSeparator;
    private Button zoomRectButton;
    private Button panButton;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;

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
        zoomInButton = new Button();
        zoomOutButton = new Button();
        fullExtentButton = new Button();
        toolbarSeparator = new Panel();
        zoomRectButton = new Button();
        panButton = new Button();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        SuspendLayout();
        // 
        // layoutPanel
        // 
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(toolbarPanel, 0, 0);
        layoutPanel.Controls.Add(geoKernelViewerControl, 0, 1);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.Location = new Point(0, 0);
        layoutPanel.Margin = new Padding(0);
        layoutPanel.Name = "layoutPanel";
        layoutPanel.RowCount = 2;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(panButton);
        toolbarPanel.Controls.Add(zoomRectButton);
        toolbarPanel.Controls.Add(toolbarSeparator);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Controls.Add(zoomOutButton);
        toolbarPanel.Controls.Add(zoomInButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Size = new Size(1200, 39);
        toolbarPanel.TabIndex = 0;
        // 
        // zoomInButton
        // 
        zoomInButton.BackColor = SystemColors.Control;
        zoomInButton.BackgroundImage = (Image)resources.GetObject("zoomInButton.Image");
        zoomInButton.BackgroundImageLayout = ImageLayout.Zoom;
        zoomInButton.FlatAppearance.BorderSize = 0;
        zoomInButton.FlatStyle = FlatStyle.Flat;
        zoomInButton.Location = new Point(0, 0);
        zoomInButton.Margin = new Padding(0);
        zoomInButton.Name = "zoomInButton";
        zoomInButton.Padding = new Padding(3);
        zoomInButton.Size = new Size(36, 36);
        zoomInButton.TabIndex = 0;
        zoomInButton.Text = "";
        zoomInButton.UseVisualStyleBackColor = false;
        zoomInButton.Click += zoomInButton_Click;
        // 
        // zoomOutButton
        // 
        zoomOutButton.BackColor = SystemColors.Control;
        zoomOutButton.BackgroundImage = (Image)resources.GetObject("zoomOutButton.Image");
        zoomOutButton.BackgroundImageLayout = ImageLayout.Zoom;
        zoomOutButton.FlatAppearance.BorderSize = 0;
        zoomOutButton.FlatStyle = FlatStyle.Flat;
        zoomOutButton.Location = new Point(36, 0);
        zoomOutButton.Margin = new Padding(0);
        zoomOutButton.Name = "zoomOutButton";
        zoomOutButton.Padding = new Padding(3);
        zoomOutButton.Size = new Size(36, 36);
        zoomOutButton.TabIndex = 1;
        zoomOutButton.Text = "";
        zoomOutButton.UseVisualStyleBackColor = false;
        zoomOutButton.Click += zoomOutButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.BackColor = SystemColors.Control;
        fullExtentButton.BackgroundImage = (Image)resources.GetObject("fullExtentButton.Image");
        fullExtentButton.BackgroundImageLayout = ImageLayout.Zoom;
        fullExtentButton.FlatAppearance.BorderSize = 0;
        fullExtentButton.FlatStyle = FlatStyle.Flat;
        fullExtentButton.Location = new Point(72, 0);
        fullExtentButton.Margin = new Padding(0);
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.Padding = new Padding(3);
        fullExtentButton.Size = new Size(36, 36);
        fullExtentButton.TabIndex = 2;
        fullExtentButton.Text = "";
        fullExtentButton.UseVisualStyleBackColor = false;
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // toolbarSeparator
        // 
        toolbarSeparator.BackColor = SystemColors.ControlDark;
        toolbarSeparator.Location = new Point(112, 4);
        toolbarSeparator.Margin = new Padding(0);
        toolbarSeparator.Name = "toolbarSeparator";
        toolbarSeparator.Size = new Size(1, 28);
        toolbarSeparator.TabIndex = 3;
        // 
        // zoomRectButton
        // 
        zoomRectButton.BackColor = SystemColors.Control;
        zoomRectButton.BackgroundImage = (Image)resources.GetObject("zoomRectButton.Image");
        zoomRectButton.BackgroundImageLayout = ImageLayout.Zoom;
        zoomRectButton.FlatAppearance.BorderSize = 0;
        zoomRectButton.FlatStyle = FlatStyle.Flat;
        zoomRectButton.Location = new Point(116, 0);
        zoomRectButton.Margin = new Padding(0);
        zoomRectButton.Name = "zoomRectButton";
        zoomRectButton.Padding = new Padding(3);
        zoomRectButton.Size = new Size(36, 36);
        zoomRectButton.TabIndex = 4;
        zoomRectButton.Text = "";
        zoomRectButton.UseVisualStyleBackColor = false;
        zoomRectButton.Click += zoomRectButton_Click;
        // 
        // panButton
        // 
        panButton.BackColor = SystemColors.Control;
        panButton.BackgroundImage = (Image)resources.GetObject("panButton.Image");
        panButton.BackgroundImageLayout = ImageLayout.Zoom;
        panButton.FlatAppearance.BorderSize = 0;
        panButton.FlatStyle = FlatStyle.Flat;
        panButton.Location = new Point(152, 0);
        panButton.Margin = new Padding(0);
        panButton.Name = "panButton";
        panButton.Padding = new Padding(3);
        panButton.Size = new Size(36, 36);
        panButton.TabIndex = 5;
        panButton.Text = "";
        panButton.UseVisualStyleBackColor = false;
        panButton.Click += panButton_Click;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 39);
        geoKernelViewerControl.Margin = new Padding(0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(1200, 761);
        geoKernelViewerControl.TabIndex = 0;
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
        Text = "HelloMap";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        toolbarPanel.ResumeLayout(false);
        ResumeLayout(false);
    }
}
