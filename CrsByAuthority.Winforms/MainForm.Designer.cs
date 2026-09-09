namespace GeoKernel.CrsByAuthority.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayoutPanel;
    private Panel toolbarPanel;
    private Label authorityLabel;
    private ComboBox authorityComboBox;
    private Label authoritySridLabel;
    private NumericUpDown authoritySridNumericUpDown;
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
        authorityLabel = new Label();
        authorityComboBox = new ComboBox();
        authoritySridLabel = new Label();
        authoritySridNumericUpDown = new NumericUpDown();
        findButton = new Button();
        summaryTextBox = new TextBox();
        detailsTextBox = new TextBox();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        rootLayoutPanel.SuspendLayout();
        toolbarPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)authoritySridNumericUpDown).BeginInit();
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
        toolbarPanel.Controls.Add(authorityLabel);
        toolbarPanel.Controls.Add(authorityComboBox);
        toolbarPanel.Controls.Add(authoritySridLabel);
        toolbarPanel.Controls.Add(authoritySridNumericUpDown);
        toolbarPanel.Controls.Add(findButton);
        toolbarPanel.Controls.Add(summaryTextBox);
        toolbarPanel.Dock = DockStyle.Fill;
        toolbarPanel.Location = new Point(3, 3);
        toolbarPanel.Name = "toolbarPanel";
        toolbarPanel.Size = new Size(994, 36);
        toolbarPanel.TabIndex = 0;
        // 
        // authorityLabel
        // 
        authorityLabel.AutoSize = true;
        authorityLabel.Location = new Point(8, 13);
        authorityLabel.Name = "authorityLabel";
        authorityLabel.Size = new Size(59, 15);
        authorityLabel.TabIndex = 0;
        authorityLabel.Text = "Authority:";
        // 
        // authorityComboBox
        // 
        authorityComboBox.Items.AddRange(new object[] { "EPSG", "ESRI", "IGNF" });
        authorityComboBox.Location = new Point(74, 10);
        authorityComboBox.Name = "authorityComboBox";
        authorityComboBox.Size = new Size(100, 23);
        authorityComboBox.TabIndex = 1;
        authorityComboBox.Text = "EPSG";
        authorityComboBox.KeyDown += authorityComboBox_KeyDown;
        // 
        // authoritySridLabel
        // 
        authoritySridLabel.AutoSize = true;
        authoritySridLabel.Location = new Point(184, 13);
        authoritySridLabel.Name = "authoritySridLabel";
        authoritySridLabel.Size = new Size(38, 15);
        authoritySridLabel.TabIndex = 2;
        authoritySridLabel.Text = "Code:";
        // 
        // authoritySridNumericUpDown
        // 
        authoritySridNumericUpDown.Location = new Point(228, 10);
        authoritySridNumericUpDown.Maximum = new decimal(new int[] { 999999, 0, 0, 0 });
        authoritySridNumericUpDown.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
        authoritySridNumericUpDown.Name = "authoritySridNumericUpDown";
        authoritySridNumericUpDown.Size = new Size(110, 23);
        authoritySridNumericUpDown.TabIndex = 3;
        authoritySridNumericUpDown.Value = new decimal(new int[] { 32635, 0, 0, 0 });
        authoritySridNumericUpDown.KeyDown += authoritySridNumericUpDown_KeyDown;
        // 
        // findButton
        // 
        findButton.Location = new Point(346, 9);
        findButton.Name = "findButton";
        findButton.Size = new Size(120, 25);
        findButton.TabIndex = 4;
        findButton.Text = "Find by Authority";
        findButton.Click += findButton_Click;
        // 
        // summaryTextBox
        // 
        summaryTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        summaryTextBox.Location = new Point(474, 10);
        summaryTextBox.Name = "summaryTextBox";
        summaryTextBox.ReadOnly = true;
        summaryTextBox.Size = new Size(516, 23);
        summaryTextBox.TabIndex = 5;
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
        statusLabel.Text = "CrsDatabase::findByAuthority";
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
        Text = "CrsByAuthority";
        Shown += MainForm_Shown;
        rootLayoutPanel.ResumeLayout(false);
        rootLayoutPanel.PerformLayout();
        toolbarPanel.ResumeLayout(false);
        toolbarPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)authoritySridNumericUpDown).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
