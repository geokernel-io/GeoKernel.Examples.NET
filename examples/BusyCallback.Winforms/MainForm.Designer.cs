namespace GeoKernel.BusyCallback.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private Panel toolbarPanel;
    private Button loadButton;
    private Button clearButton;
    private Label busyStateLabel;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private TextBox eventLogTextBox;
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
        toolbarPanel = new Panel();
        busyStateLabel = new Label();
        clearButton = new Button();
        loadButton = new Button();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        eventLogTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        progressBar = new ToolStripProgressBar();
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
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(busyStateLabel);
        toolbarPanel.Controls.Add(clearButton);
        toolbarPanel.Controls.Add(loadButton);
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(1100, 32);
        toolbarPanel.TabIndex = 0;
        // 
        // busyStateLabel
        // 
        busyStateLabel.AutoSize = true;
        busyStateLabel.Location = new Point(204, 9);
        busyStateLabel.Name = "busyStateLabel";
        busyStateLabel.Size = new Size(59, 15);
        busyStateLabel.TabIndex = 2;
        busyStateLabel.Text = "Busy: OFF";
        // 
        // clearButton
        // 
        clearButton.BackColor = SystemColors.Control;
        clearButton.FlatAppearance.BorderSize = 0;
        clearButton.FlatStyle = FlatStyle.Flat;
        clearButton.Location = new Point(126, 0);
        clearButton.Margin = new Padding(0);
        clearButton.Name = "clearButton";
        clearButton.Size = new Size(64, 32);
        clearButton.TabIndex = 1;
        clearButton.Text = "Clear";
        clearButton.UseVisualStyleBackColor = false;
        clearButton.Click += clearButton_Click;
        // 
        // loadButton
        // 
        loadButton.BackColor = SystemColors.Control;
        loadButton.FlatAppearance.BorderSize = 0;
        loadButton.FlatStyle = FlatStyle.Flat;
        loadButton.Location = new Point(0, 0);
        loadButton.Margin = new Padding(0);
        loadButton.Name = "loadButton";
        loadButton.Size = new Size(126, 32);
        loadButton.TabIndex = 0;
        loadButton.Text = "Load Large Layer";
        loadButton.UseVisualStyleBackColor = false;
        loadButton.Click += loadButton_Click;
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 32);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(eventLogTextBox);
        splitContainer.Size = new Size(1100, 626);
        splitContainer.SplitterDistance = 720;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.BackColor = Color.White;
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(720, 626);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // eventLogTextBox
        // 
        eventLogTextBox.Dock = DockStyle.Fill;
        eventLogTextBox.Font = new Font("Consolas", 9F);
        eventLogTextBox.Location = new Point(0, 0);
        eventLogTextBox.Multiline = true;
        eventLogTextBox.Name = "eventLogTextBox";
        eventLogTextBox.ReadOnly = true;
        eventLogTextBox.ScrollBars = ScrollBars.Vertical;
        eventLogTextBox.Size = new Size(376, 626);
        eventLogTextBox.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, progressBar });
        statusStrip.Location = new Point(0, 658);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1100, 22);
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(983, 17);
        statusLabel.Spring = true;
        statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // progressBar
        // 
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(100, 16);
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1100, 680);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(820, 520);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "BusyCallback";
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
