namespace GeoKernel.CrsDatabase.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayoutPanel;
    private Panel toolbarPanel;
    private Label sridLabel;
    private NumericUpDown sridNumericUpDown;
    private Button findButton;
    private TextBox summaryTextBox;
    private TextBox detailsTextBox;
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
        rootLayoutPanel = new TableLayoutPanel();
        toolbarPanel = new Panel();
        sridLabel = new Label();
        sridNumericUpDown = new NumericUpDown();
        findButton = new Button();
        summaryTextBox = new TextBox();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        rootLayoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)sridNumericUpDown).BeginInit();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // rootLayoutPanel
        // 
        rootLayoutPanel.ColumnCount = 1;
        rootLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayoutPanel.Controls.Add(toolbarPanel, 0, 0);
        rootLayoutPanel.Controls.Add(detailsTextBox, 0, 1);
        rootLayoutPanel.Controls.Add(statusStrip, 0, 2);
        rootLayoutPanel.Dock = DockStyle.Fill;
        rootLayoutPanel.Location = new Point(0, 0);
        rootLayoutPanel.Name = "rootLayoutPanel";
        rootLayoutPanel.RowCount = 3;
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 42F));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayoutPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        rootLayoutPanel.Size = new Size(1000, 720);
        rootLayoutPanel.TabIndex = 0;
        // 
        // toolbarPanel
        // 
        toolbarPanel.Controls.Add(sridLabel);
        toolbarPanel.Controls.Add(sridNumericUpDown);
        toolbarPanel.Controls.Add(findButton);
        toolbarPanel.Controls.Add(summaryTextBox);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(3, 3);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(994, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // sridLabel
        // 
        sridLabel.AutoSize = true;
        sridLabel.Location = new Point(8, 13);
        sridLabel.Name = "sridLabel";
        sridLabel.Size = new Size(34, 15);
        sridLabel.TabIndex = 0;
        sridLabel.Text = "SRID:";
        // 
        // sridNumericUpDown
        // 
        sridNumericUpDown.Location = new Point(48, 10);
        sridNumericUpDown.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        sridNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        sridNumericUpDown.Name = "sridNumericUpDown";
        sridNumericUpDown.Size = new Size(110, 23);
        sridNumericUpDown.TabIndex = 1;
        sridNumericUpDown.Value = new decimal(new int[] { 4326, 0, 0, 0 });
        sridNumericUpDown.KeyDown += sridNumericUpDown_KeyDown;
        // 
        // findButton
        // 
        findButton.Location = new Point(166, 9);
        findButton.Name = "findButton";
        findButton.Size = new Size(100, 25);
        findButton.TabIndex = 2;
        findButton.Text = "Find by SRID";
        findButton.Click += findButton_Click;
        // 
        // summaryTextBox
        // 
        summaryTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        summaryTextBox.Location = new Point(274, 10);
        summaryTextBox.Name = "summaryTextBox";
        summaryTextBox.ReadOnly = true;
        summaryTextBox.Size = new Size(1508, 23);
        summaryTextBox.TabIndex = 3;
        // 
        // detailsTextBox
        // 
        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Font = new Font("Consolas", 9F);
        detailsTextBox.Location = new Point(3, 45);
        detailsTextBox.Multiline = true;
        detailsTextBox.Name = "detailsTextBox";
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Both;
        detailsTextBox.Size = new Size(994, 648);
        detailsTextBox.TabIndex = 1;
        detailsTextBox.WordWrap = false;
        // 
        // statusStrip
        // 
        statusStrip.Dock = DockStyle.Fill;
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 696);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1000, 24);
        statusStrip.SizingGrip = false;
        statusStrip.TabIndex = 2;
        // 
        // statusLabel
        // 
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(132, 19);
        statusLabel.Text = "CrsDatabase::findBySrid";
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1000, 720);
        Controls.Add(rootLayoutPanel);
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "CrsDatabase";
        Shown += MainForm_Shown;
        rootLayoutPanel.ResumeLayout(false);
        rootLayoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)sridNumericUpDown).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
