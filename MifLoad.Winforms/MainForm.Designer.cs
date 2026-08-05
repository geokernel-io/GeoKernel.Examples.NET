namespace GeoKernel.MifLoad.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel layoutPanel;
    private Panel toolbarPanel;
    private Button primaryButton;
    private Button secondaryButton;
    private SplitContainer splitContainer;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl viewerControl;
    private TextBox detailsTextBox;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel statusLabel;
    private ToolStripProgressBar downloadProgressBar;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components is not null)
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        components = new System.ComponentModel.Container();
        layoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        primaryButton = new Button();
        secondaryButton = new Button();
        splitContainer = new SplitContainer();
        viewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        downloadProgressBar = new ToolStripProgressBar();
        layoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)splitContainer).BeginInit();
        splitContainer.Panel1.SuspendLayout();
        splitContainer.Panel2.SuspendLayout();
        splitContainer.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        layoutPanel.ColumnCount = 1;
        layoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        layoutPanel.Controls.Add(toolbarPanel, 0, 0);
        layoutPanel.Controls.Add(splitContainer, 0, 1);
        layoutPanel.Controls.Add(statusStrip, 0, 2);
        layoutPanel.Dock = DockStyle.Fill;
        layoutPanel.Location = new Point(0, 0);
        layoutPanel.Margin = new Padding(0);
        layoutPanel.RowCount = 3;
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        layoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));
        layoutPanel.Size = new Size(1180, 760);
        toolbarPanel.BackColor = SystemColors.Control;
        toolbarPanel.Controls.Add(secondaryButton);
        toolbarPanel.Controls.Add(primaryButton);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Margin = new Padding(0);
        primaryButton.Location = new Point(4, 5);
        primaryButton.Size = new Size(126, 25);
        primaryButton.Text = "Load Sample";
        primaryButton.Click += primaryButton_Click;
        secondaryButton.Location = new Point(136, 5);
        secondaryButton.Size = new Size(100, 25);
        secondaryButton.Text = "Full Extent";
        secondaryButton.Click += secondaryButton_Click;
        splitContainer.Dock = DockStyle.Fill;
        splitContainer.Margin = new Padding(0);
        splitContainer.Panel1.Controls.Add(viewerControl);
        splitContainer.Panel2.Controls.Add(detailsTextBox);
        splitContainer.SplitterDistance = 760;
        viewerControl.Dock = DockStyle.Fill;
        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Multiline = true;
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Vertical;
        detailsTextBox.Font = new Font("Consolas", 9F);
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel, downloadProgressBar });
        downloadProgressBar.Size = new Size(180, 16);
        downloadProgressBar.Visible = false;
        statusLabel.Text = "MifLoad ready.";
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1180, 760);
        Controls.Add(layoutPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "MifLoad";
        Shown += MainForm_Shown;
        layoutPanel.ResumeLayout(false);
        layoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        splitContainer.Panel1.ResumeLayout(false);
        splitContainer.Panel2.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)splitContainer).EndInit();
        splitContainer.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
