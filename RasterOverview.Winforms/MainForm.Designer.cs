namespace GeoKernel.RasterOverview.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayout;
    private FlowLayoutPanel toolbarPanel;
    private Button resetButton;
    private Button loadWithoutButton;
    private Button loadWithButton;
    private Button benchmarkButton;
    private Button fullExtentButton;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl viewerControl;
    private TextBox detailsTextBox;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new(typeof(MainForm));
        rootLayout = new TableLayoutPanel();
        toolbarPanel = new FlowLayoutPanel();
        resetButton = new Button();
        loadWithoutButton = new Button();
        loadWithButton = new Button();
        benchmarkButton = new Button();
        fullExtentButton = new Button();
        splitContainer = new SplitContainer();
        viewerControl = new global::GeoKernel.NET.WinForms.GeoKernelViewerControl();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
        rootLayout.SuspendLayout();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(toolbarPanel, 0, 0);
        rootLayout.Controls.Add(splitContainer, 0, 1);
        rootLayout.Controls.Add(statusStrip, 0, 2);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Margin = Padding.Empty;
        rootLayout.RowCount = 3;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 38F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.FlowDirection = FlowDirection.LeftToRight;
        toolbarPanel.Padding = new Padding(6, 4, 0, 3);
        toolbarPanel.WrapContents = false;
        ConfigureButton(resetButton, "Reset Working Copy", resetButton_Click);
        ConfigureButton(loadWithoutButton, "Load Without Overview", loadWithoutButton_Click);
        ConfigureButton(loadWithButton, "Load With Overview", loadWithButton_Click);
        ConfigureButton(benchmarkButton, "Run Downsample Benchmark", benchmarkButton_Click);
        ConfigureButton(fullExtentButton, "Full Extent", fullExtentButton_Click);
        toolbarPanel.Controls.AddRange(new Control[] { resetButton, loadWithoutButton, loadWithButton, benchmarkButton, fullExtentButton });
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Margin = Padding.Empty;
        splitContainer.Panel1.Controls.Add(viewerControl);
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.SplitterDistance = 750;
        splitContainer.FixedPanel = FixedPanel.Panel2;
        splitContainer.IsSplitterFixed = false;
        splitContainer.SplitterWidth = 4;
        viewerControl.Dock = DockStyle.Fill;
        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Multiline = true;
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Font = new Font("Consolas", 9F);
        detailsTextBox.BackColor = SystemColors.Window;
        detailsTextBox.BorderStyle = BorderStyle.FixedSingle;
        detailsTextBox.Text = "RasterOverview sample\r\n\r\nPreparing sample data...";
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
        statusLabel.Spring = true;
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        statusLabel.Text = "Ready.";
        progressBar.Minimum = 0;
        progressBar.Maximum = 100;
        progressBar.Size = new Size(240, 16);
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 800);
        Controls.Add(rootLayout);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "RasterOverview";
        Shown += MainForm_Shown;
        rootLayout.ResumeLayout(false);
        rootLayout.PerformLayout();
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
    }

    private static void ConfigureButton(Button button, string text, EventHandler handler)
    {
        button.AutoSize = true;
        button.Height = 28;
        button.Margin = new Padding(0, 0, 8, 0);
        button.Text = text;
        button.UseVisualStyleBackColor = true;
        button.Click += handler;
    }
}
