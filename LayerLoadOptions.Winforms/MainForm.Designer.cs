namespace GeoKernel.LayerLoadOptions.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button loadNoIndexButton;
    private Button loadRTreeButton;
    private Button runQueryTestButton;
    private Button clearButton;
    private Panel resultPanel;
    private Label helpLabel;
    private Label noIndexResultLabel;
    private Label rtreeResultLabel;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar progressBar;

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
        loadNoIndexButton = new Button();
        loadRTreeButton = new Button();
        runQueryTestButton = new Button();
        clearButton = new Button();
        resultPanel = new Panel();
        helpLabel = new Label();
        noIndexResultLabel = new Label();
        rtreeResultLabel = new Label();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        resultPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // layoutPanel
        // 
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(toolbarPanel, 0, 0);
        layoutPanel.Controls.Add(resultPanel, 0, 1);
        layoutPanel.Controls.Add(statusStrip, 0, 3);
        layoutPanel.Controls.Add(geoKernelViewerControl, 0, 2);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.Location = new Point(0, 0);
        layoutPanel.Margin = new Padding(0);
        layoutPanel.Name = "layoutPanel";
        layoutPanel.RowCount = 4;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 32F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        layoutPanel.Size = new Size(1200, 800);
        layoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(clearButton);
        toolbarPanel.Controls.Add(runQueryTestButton);
        toolbarPanel.Controls.Add(loadRTreeButton);
        toolbarPanel.Controls.Add(loadNoIndexButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1200, 32);
        toolbarPanel.TabIndex = 0;
        // 
        // loadNoIndexButton
        // 
        loadNoIndexButton.BackColor = SystemColors.Control;
        loadNoIndexButton.FlatAppearance.BorderSize = 0;
        loadNoIndexButton.FlatStyle = FlatStyle.Flat;
        loadNoIndexButton.Location = new Point(0, 0);
        loadNoIndexButton.Margin = new Padding(0);
        loadNoIndexButton.Name = "loadNoIndexButton";
        loadNoIndexButton.Size = new Size(112, 32);
        loadNoIndexButton.TabIndex = 0;
        loadNoIndexButton.Text = "Load No Index";
        loadNoIndexButton.UseVisualStyleBackColor = false;
        loadNoIndexButton.Click += loadNoIndexButton_Click;
        // 
        // loadRTreeButton
        // 
        loadRTreeButton.BackColor = SystemColors.Control;
        loadRTreeButton.FlatAppearance.BorderSize = 0;
        loadRTreeButton.FlatStyle = FlatStyle.Flat;
        loadRTreeButton.Location = new Point(112, 0);
        loadRTreeButton.Margin = new Padding(0);
        loadRTreeButton.Name = "loadRTreeButton";
        loadRTreeButton.Size = new Size(96, 32);
        loadRTreeButton.TabIndex = 1;
        loadRTreeButton.Text = "Load RTree";
        loadRTreeButton.UseVisualStyleBackColor = false;
        loadRTreeButton.Click += loadRTreeButton_Click;
        // 
        // runQueryTestButton
        // 
        runQueryTestButton.BackColor = SystemColors.Control;
        runQueryTestButton.FlatAppearance.BorderSize = 0;
        runQueryTestButton.FlatStyle = FlatStyle.Flat;
        runQueryTestButton.Location = new Point(208, 0);
        runQueryTestButton.Margin = new Padding(0);
        runQueryTestButton.Name = "runQueryTestButton";
        runQueryTestButton.Size = new Size(116, 32);
        runQueryTestButton.TabIndex = 2;
        runQueryTestButton.Text = "Run Query Test";
        runQueryTestButton.UseVisualStyleBackColor = false;
        runQueryTestButton.Click += runQueryTestButton_Click;
        // 
        // clearButton
        // 
        clearButton.BackColor = SystemColors.Control;
        clearButton.FlatAppearance.BorderSize = 0;
        clearButton.FlatStyle = FlatStyle.Flat;
        clearButton.Location = new Point(324, 0);
        clearButton.Margin = new Padding(0);
        clearButton.Name = "clearButton";
        clearButton.Size = new Size(64, 32);
        clearButton.TabIndex = 3;
        clearButton.Text = "Clear";
        clearButton.UseVisualStyleBackColor = false;
        clearButton.Click += clearButton_Click;
        // 
        // resultPanel
        // 
        resultPanel.Controls.Add(helpLabel);
        resultPanel.Controls.Add(noIndexResultLabel);
        resultPanel.Controls.Add(rtreeResultLabel);
        resultPanel.Dock = DockStyle.Fill;
        resultPanel.Location = new Point(0, 32);
        resultPanel.Margin = new Padding(0);
        resultPanel.Name = "resultPanel";
        resultPanel.Padding = new Padding(6, 4, 6, 4);
        resultPanel.Size = new Size(1200, 72);
        resultPanel.TabIndex = 1;
        // 
        // helpLabel
        // 
        helpLabel.AutoSize = true;
        helpLabel.Location = new Point(6, 6);
        helpLabel.Name = "helpLabel";
        helpLabel.Size = new Size(582, 15);
        helpLabel.TabIndex = 0;
        helpLabel.Text = "Load one index mode, then run the query test. Load time is shown separately and is not part of the test result.";
        // 
        // noIndexResultLabel
        // 
        noIndexResultLabel.AutoSize = true;
        noIndexResultLabel.Location = new Point(6, 28);
        noIndexResultLabel.Name = "noIndexResultLabel";
        noIndexResultLabel.Size = new Size(66, 15);
        noIndexResultLabel.TabIndex = 1;
        noIndexResultLabel.Text = "No Index: -";
        // 
        // rtreeResultLabel
        // 
        rtreeResultLabel.AutoSize = true;
        rtreeResultLabel.Location = new Point(6, 49);
        rtreeResultLabel.Name = "rtreeResultLabel";
        rtreeResultLabel.Size = new Size(45, 15);
        rtreeResultLabel.TabIndex = 2;
        rtreeResultLabel.Text = "RTree: -";
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
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
        statusStrip.Location = new Point(0, 776);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1200, 24);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 3;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(1023, 19);
        statusLabel.Spring = true;
        statusLabel.Text = "Ready.";
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // progressBar
        // 
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(160, 18);
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
        Text = "LayerLoadOptions";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        resultPanel.ResumeLayout(false);
        resultPanel.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
