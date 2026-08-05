namespace GeoKernel.Measure.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button panButton;
    private Button distanceButton;
    private Button areaButton;
    private Panel separator;
    private Button clearButton;
    private Button fullExtentButton;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;
    private ToolTip toolTip;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new(typeof(MainForm));
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        panButton = new Button();
        distanceButton = new Button();
        areaButton = new Button();
        separator = new Panel();
        clearButton = new Button();
        fullExtentButton = new Button();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
        toolTip = new ToolTip(components);
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // layoutPanel
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(toolbarPanel, 0, 0);
        layoutPanel.Controls.Add(geoKernelViewerControl, 0, 1);
        layoutPanel.Controls.Add(statusStrip, 0, 2);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.RowCount = 3;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        // toolbarPanel
        toolbarPanel.Controls.Add(panButton);
        toolbarPanel.Controls.Add(distanceButton);
        toolbarPanel.Controls.Add(areaButton);
        toolbarPanel.Controls.Add(separator);
        toolbarPanel.Controls.Add(clearButton);
        toolbarPanel.Controls.Add(fullExtentButton);
        toolbarPanel.Dock = DockStyle.Fill;
        // buttons
        ConfigureButton(panButton, "pan.png", 0, "Pan");
        ConfigureButton(distanceButton, "measure-distance.png", 36, "Measure distance");
        ConfigureButton(areaButton, "measure-area.png", 72, "Measure area");
        separator.BackColor = SystemColors.ControlDark;
        separator.Location = new Point(112, 4);
        separator.Size = new Size(1, 28);
        ConfigureButton(clearButton, "delete.png", 116, "Clear measurements");
        ConfigureButton(fullExtentButton, "full-extent.png", 152, "Full extent");
        panButton.Click += panButton_Click;
        distanceButton.Click += distanceButton_Click;
        areaButton.Click += areaButton_Click;
        clearButton.Click += clearButton_Click;
        fullExtentButton.Click += fullExtentButton_Click;
        // viewer
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Margin = new Padding(0);
        // statusStrip
        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
        statusStrip.SizingGrip = false;
        statusLabel.Spring = true;
        statusLabel.Text = "Ready";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        progressBar.Size = new Size(180, 18);
        progressBar.Visible = false;
        // form
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 800);
        Controls.Add(layoutPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Measure";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }

    private void ConfigureButton(Button button, string imageName, int left, string tip)
    {
        button.BackColor = SystemColors.Control;
        button.BackgroundImage = new Bitmap(Path.Combine(AppContext.BaseDirectory, "resources", imageName));
        button.BackgroundImageLayout = ImageLayout.Center;
        button.FlatAppearance.BorderSize = 0;
        button.TabStop = false;
        button.FlatStyle = FlatStyle.Flat;
        button.Location = new Point(left, 0);
        button.Margin = new Padding(0);
        button.Size = new Size(36, 36);
        button.UseVisualStyleBackColor = false;
        toolTip.SetToolTip(button, tip);
    }
}
