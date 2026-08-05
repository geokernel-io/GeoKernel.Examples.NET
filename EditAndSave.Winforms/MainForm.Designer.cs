namespace GeoKernel.EditAndSave.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private ToolStrip toolStrip;
    private ToolStripButton fullExtentButton;
    private ToolStripButton addPointButton;
    private ToolStripButton panButton;
    private ToolStripButton saveButton;
    private ToolStripButton clearButton;
    private ToolStripLabel countLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        var resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        toolStrip = new ToolStrip();
        fullExtentButton = new ToolStripButton();
        addPointButton = new ToolStripButton();
        panButton = new ToolStripButton();
        saveButton = new ToolStripButton();
        clearButton = new ToolStripButton();
        countLabel = new ToolStripLabel();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        toolStrip.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();

        toolStrip.AutoSize = false;
        toolStrip.Dock = DockStyle.Top;
        toolStrip.GripStyle = ToolStripGripStyle.Hidden;
        toolStrip.ImageScalingSize = new Size(32, 32);
        toolStrip.Items.AddRange([fullExtentButton, new ToolStripSeparator(), addPointButton, panButton,
            new ToolStripSeparator(), saveButton, clearButton, countLabel]);
        toolStrip.Name = "toolStrip";
        toolStrip.Size = new Size(1200, 40);

        ConfigureButton(fullExtentButton, "Full Extent", "FullExtent.png");
        fullExtentButton.Click += fullExtentButton_Click;
        ConfigureButton(addPointButton, "Add Point", "Point.png");
        addPointButton.CheckOnClick = true;
        addPointButton.Click += addPointButton_Click;
        ConfigureButton(panButton, "Pan", "Pan.png");
        panButton.CheckOnClick = true;
        panButton.Click += panButton_Click;
        ConfigureButton(saveButton, "Save Shapefile", "SaveProject.png");
        saveButton.Enabled = false;
        saveButton.Click += saveButton_Click;
        ConfigureButton(clearButton, "Clear Points", "Delete.png");
        clearButton.Click += clearButton_Click;
        countLabel.Margin = new Padding(12, 0, 12, 0);
        countLabel.Text = "Point count: 0";

        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 40);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(1200, 736);

        statusStrip.Items.AddRange([statusLabel, downloadProgressBar]);
        statusStrip.Location = new Point(0, 776);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1200, 24);
        statusLabel.Spring = true;
        statusLabel.Text = "Choose Add Point, click the map, then save the points as a shapefile.";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        downloadProgressBar.Size = new Size(180, 18);
        downloadProgressBar.Visible = false;

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 800);
        Controls.Add(geoKernelViewerControl);
        Controls.Add(toolStrip);
        Controls.Add(statusStrip);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "EditAndSave";
        Shown += MainForm_Shown;
        toolStrip.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        ResumeLayout(false);
        PerformLayout();
    }

    private static void ConfigureButton(ToolStripButton button, string text, string imageName)
    {
        button.DisplayStyle = ToolStripItemDisplayStyle.Image;
        button.Image = Image.FromFile(Path.Combine(AppContext.BaseDirectory, "resources", imageName));
        button.ImageTransparentColor = Color.Magenta;
        button.Text = text;
        button.ToolTipText = text;
    }
}
