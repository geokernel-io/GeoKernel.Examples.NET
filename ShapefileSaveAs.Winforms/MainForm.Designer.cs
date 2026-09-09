namespace GeoKernel.ShapefileSaveAs.Winforms;

partial class MainForm
{
    private System.ComponentModel.IContainer components = null;
    private TableLayoutPanel rootLayout;
    private TableLayoutPanel topBar;
    private Button saveButton;
    private ProgressBar saveProgressBar;
    private TableLayoutPanel mainContent;
    private global::GeoKernel.NET.WinForms.GeoKernelViewerControl viewerControl;
    private TableLayoutPanel rightLayout;
    private Label stateLabel;
    private TextBox detailsTextBox;
    private Label attributesLabel;
    private DataGridView attributesGrid;
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
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new(typeof(MainForm));
        rootLayout = new TableLayoutPanel();
        topBar = new TableLayoutPanel();
        saveButton = new Button();
        saveProgressBar = new ProgressBar();
        mainContent = new TableLayoutPanel();
        viewerControl = new GeoKernel.NET.WinForms.GeoKernelViewerControl();
        rightLayout = new TableLayoutPanel();
        stateLabel = new Label();
        detailsTextBox = new TextBox();
        attributesLabel = new Label();
        attributesGrid = new DataGridView();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        rootLayout.SuspendLayout();
        topBar.SuspendLayout();
        mainContent.SuspendLayout();
        rightLayout.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)attributesGrid).BeginInit();
        statusStrip.SuspendLayout();
        SuspendLayout();

        rootLayout.ColumnCount = 1;
        rootLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rootLayout.Controls.Add(topBar, 0, 0);
        rootLayout.Controls.Add(mainContent, 0, 1);
        rootLayout.Controls.Add(statusStrip, 0, 2);
        rootLayout.Dock = DockStyle.Fill;
        rootLayout.Margin = new Padding(0);
        rootLayout.RowCount = 3;
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 39F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        rootLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 22F));

        topBar.ColumnCount = 2;
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        topBar.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        topBar.Controls.Add(saveButton, 0, 0);
        topBar.Controls.Add(saveProgressBar, 1, 0);
        topBar.Dock = DockStyle.Fill;
        topBar.Padding = new Padding(6, 4, 6, 4);
        topBar.RowCount = 1;
        topBar.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        saveButton.AutoSize = true;
        saveButton.Dock = DockStyle.Fill;
        saveButton.Margin = new Padding(0, 0, 8, 0);
        saveButton.MinimumSize = new Size(140, 27);
        saveButton.Text = "Save As Shapefile";
        saveButton.Click += saveButton_Click;

        saveProgressBar.Dock = DockStyle.Fill;
        saveProgressBar.Margin = new Padding(0);
        saveProgressBar.Maximum = 100;

        mainContent.ColumnCount = 2;
        mainContent.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        mainContent.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 320F));
        mainContent.Controls.Add(viewerControl, 0, 0);
        mainContent.Controls.Add(rightLayout, 1, 0);
        mainContent.Dock = DockStyle.Fill;
        mainContent.Margin = new Padding(0);
        mainContent.RowCount = 1;
        mainContent.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        viewerControl.BackColor = Color.White;
        viewerControl.Dock = DockStyle.Fill;
        viewerControl.Margin = new Padding(0);

        rightLayout.ColumnCount = 1;
        rightLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        rightLayout.Controls.Add(stateLabel, 0, 0);
        rightLayout.Controls.Add(detailsTextBox, 0, 1);
        rightLayout.Controls.Add(attributesLabel, 0, 2);
        rightLayout.Controls.Add(attributesGrid, 0, 3);
        rightLayout.Dock = DockStyle.Fill;
        rightLayout.Padding = new Padding(6);
        rightLayout.RowCount = 4;
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 28F));
        rightLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        stateLabel.Dock = DockStyle.Fill;
        stateLabel.Text = "SaveAs state";
        stateLabel.TextAlign = ContentAlignment.MiddleLeft;

        detailsTextBox.Dock = DockStyle.Fill;
        detailsTextBox.Font = new Font("Consolas", 9F);
        detailsTextBox.Multiline = true;
        detailsTextBox.ReadOnly = true;
        detailsTextBox.ScrollBars = ScrollBars.Both;
        detailsTextBox.WordWrap = false;

        attributesLabel.Dock = DockStyle.Fill;
        attributesLabel.Text = "Reloaded output attributes";
        attributesLabel.TextAlign = ContentAlignment.BottomLeft;

        attributesGrid.AllowUserToAddRows = false;
        attributesGrid.AllowUserToDeleteRows = false;
        attributesGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.DisplayedCells;
        attributesGrid.Dock = DockStyle.Fill;
        attributesGrid.ReadOnly = true;
        attributesGrid.RowHeadersVisible = false;
        attributesGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusLabel.Text = "Preparing shapefile sample...";

        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1200, 760);
        Controls.Add(rootLayout);
        Icon = (Icon)resources.GetObject("$this.Icon");
        MinimumSize = new Size(980, 640);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "ShapefileSaveAs";
        Shown += MainForm_Shown;
        rootLayout.ResumeLayout(false);
        rootLayout.PerformLayout();
        topBar.ResumeLayout(false);
        topBar.PerformLayout();
        mainContent.ResumeLayout(false);
        rightLayout.ResumeLayout(false);
        rightLayout.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)attributesGrid).EndInit();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
    }
}
