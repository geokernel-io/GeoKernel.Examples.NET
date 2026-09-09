namespace GeoKernel.BufferAnimated.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private FlowLayoutPanel toolbarPanel;
    private Button playPauseButton;
    private Button fullExtentButton;
    private Label intervalLabel;
    private TrackBar intervalTrackBar;
    private Label distanceTextLabel;
    private Label distanceValueLabel;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl geoKernelViewerControl;
    private TextBox detailsTextBox;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private System.Windows.Forms.Timer animationTimer;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();

        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        toolbarPanel = new FlowLayoutPanel();
        playPauseButton = new Button();
        fullExtentButton = new Button();
        intervalLabel = new Label();
        intervalTrackBar = new TrackBar();
        distanceTextLabel = new Label();
        distanceValueLabel = new Label();
        splitContainer = new SplitContainer();
        geoKernelViewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        animationTimer = new System.Windows.Forms.Timer(components);
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)intervalTrackBar).BeginInit();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // toolbarPanel
        // 
        toolbarPanel.AutoSize = false;
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Dock = DockStyle.Top;
        toolbarPanel.FlowDirection = FlowDirection.LeftToRight;
        toolbarPanel.Location = new Point(0, 0);
        toolbarPanel.Margin = new Padding(0);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Padding = new Padding(0);
        toolbarPanel.Controls.AddRange(new Control[] { playPauseButton, fullExtentButton, intervalLabel, intervalTrackBar, distanceTextLabel, distanceValueLabel });
        toolbarPanel.Size = new Size(980, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // playPauseButton
        // 
        playPauseButton.Name = "playPauseButton";
        playPauseButton.AutoSize = true;
        playPauseButton.Height = 28;
        playPauseButton.Margin = new Padding(0, 3, 4, 3);
        playPauseButton.Padding = new Padding(8, 0, 8, 0);
        playPauseButton.UseVisualStyleBackColor = true;
        playPauseButton.Size = new Size(42, 42);
        playPauseButton.Text = "Pause";
        playPauseButton.Click += playPauseButton_Click;
        // 
        // fullExtentButton
        // 
        fullExtentButton.Name = "fullExtentButton";
        fullExtentButton.AutoSize = true;
        fullExtentButton.Height = 28;
        fullExtentButton.Margin = new Padding(0, 3, 4, 3);
        fullExtentButton.Padding = new Padding(8, 0, 8, 0);
        fullExtentButton.UseVisualStyleBackColor = true;
        fullExtentButton.Size = new Size(63, 42);
        fullExtentButton.Text = "Full Extent";
        fullExtentButton.Click += fullExtentButton_Click;
        // 
        // intervalLabel
        // 
        intervalLabel.Name = "intervalLabel";
        intervalLabel.AutoSize = true;
        intervalLabel.Margin = new Padding(8, 9, 8, 0);
        intervalLabel.TextAlign = ContentAlignment.MiddleLeft;
        intervalLabel.Size = new Size(49, 42);
        intervalLabel.Text = "Interval:";
        // 
        // intervalTrackBar
        // 
        intervalTrackBar.AutoSize = false;
        intervalTrackBar.Maximum = 200;
        intervalTrackBar.Minimum = 20;
        intervalTrackBar.Name = "intervalTrackBar";
        intervalTrackBar.Margin = new Padding(0, 4, 8, 0);
        intervalTrackBar.Size = new Size(160, 32);
        intervalTrackBar.TickFrequency = 30;
        intervalTrackBar.Value = 60;
        intervalTrackBar.ValueChanged += intervalTrackBar_ValueChanged;
        // 
        // 
        // distanceTextLabel
        // 
        distanceTextLabel.Name = "distanceTextLabel";
        distanceTextLabel.AutoSize = true;
        distanceTextLabel.Margin = new Padding(8, 9, 8, 0);
        distanceTextLabel.TextAlign = ContentAlignment.MiddleLeft;
        distanceTextLabel.Size = new Size(55, 42);
        distanceTextLabel.Text = "Distance:";
        // 
        // distanceValueLabel
        // 
        distanceValueLabel.Name = "distanceValueLabel";
        distanceValueLabel.AutoSize = true;
        distanceValueLabel.Margin = new Padding(8, 9, 8, 0);
        distanceValueLabel.TextAlign = ContentAlignment.MiddleLeft;
        distanceValueLabel.Size = new Size(0, 42);
        // 
        // splitContainer
        // 
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Location = new Point(0, 45);
        splitContainer.Name = "splitContainer";
        // 
        // splitContainer.Panel1
        // 
        splitContainer.Panel1.Controls.Add(geoKernelViewerControl);
        // 
        // splitContainer.Panel2
        // 
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.Size = new Size(980, 613);
        splitContainer.SplitterDistance = 700;
        splitContainer.TabIndex = 1;
        // 
        // geoKernelViewerControl
        // 
        geoKernelViewerControl.Dock = DockStyle.Fill;
        geoKernelViewerControl.Location = new Point(0, 0);
        geoKernelViewerControl.Name = "geoKernelViewerControl";
        geoKernelViewerControl.Size = new Size(700, 613);
        geoKernelViewerControl.TabIndex = 0;
        // 
        // detailsTextBox
        // 
        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Location = new Point(0, 0);
        detailsTextBox.Multiline = true;
        detailsTextBox.Name = "detailsTextBox";
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Size = new Size(276, 613);
        detailsTextBox.TabIndex = 0;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 658);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(980, 22);
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(0, 17);
        // 
        // animationTimer
        // 
        animationTimer.Interval = 60;
        animationTimer.Tick += animationTimer_Tick;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(980, 680);
        Controls.Add(splitContainer);
        Controls.Add(statusStrip);
        Controls.Add(toolbarPanel);
        Icon = new Icon(Path.Combine(AppContext.BaseDirectory, "resources", "GeoKernelAppIcon.ico"));
        MinimumSize = new Size(720, 480);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "BufferAnimated";
        Shown += MainForm_Shown;
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)intervalTrackBar).EndInit();
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
